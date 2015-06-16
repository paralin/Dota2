using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Dota2.Engine.Game.Data;

/*
    This code from the Nora project.
    See https://github.com/dschleck/nora/blob/master/lara/Bitstream.cs
 */

namespace Dota2.Utils
{
    public class Bitstream : Stream
    {

        public static Bitstream Create()
        {
            return new Bitstream(new uint[32], 0);
        }

        public static Bitstream CreateWith(byte[] bytes)
        {
            return CreateWith(bytes, bytes.Length);
        }

        public static Bitstream CreateWith(byte[] bytes, int length)
        {
            // Pad the back with some zeroes
            byte[] copied = new byte[length + 3];
            Array.Copy(bytes, copied, length);

            uint[] asInts = new uint[copied.Length / 4];

            for (int i = 0; i < asInts.Length; ++i)
            {
                uint l = BitConverter.ToUInt32(copied, i * 4);
                asInts[i] = l;
            }

            return new Bitstream(asInts, (uint)length);
        }

        private const int COORD_INTEGER_BITS = 14;
        private const int COORD_FRACTIONAL_BITS = 5;
        private const int COORD_DENOMINATOR = (1 << (COORD_FRACTIONAL_BITS));
        private const double COORD_RESOLUTION = (1.0 / (COORD_DENOMINATOR));

        private uint[] data;

        public override long Position { get; set; }
        public long BitLength { get; private set; }
        public override long Length { get { return (BitLength + 7) / 8; } }

        public bool Eof { get { return BitLength == Position; } }
        public long Remain { get { return BitLength - Position; } }

        private Bitstream(uint[] data, uint byteLength)
        {
            this.data = data;

            this.Position = 0;
            this.BitLength = 8 * byteLength;
        }

        public void Erase()
        {
            Position = 0;
            BitLength = 0;
        }

        public uint ReadBits(byte n)
        {
            if (n > 32)
            {
                throw new ArgumentException("Can't grab more than 32 bits");
            }

            if (!HasBits(n))
            {
                throw new ArgumentException("Not enough bits remain in the buffer");
            }

            uint a = data[Position / 32];
            uint b = data[(Position + n - 1) / 32];

            uint read = (uint)Position & 31;

            a >>= (byte)read;
            b <<= (byte)(32 - read);

            uint mask = (uint)(((ulong)1 << n) - 1);
            uint ret = (a | b) & mask;

            Position += n;
            return ret;
        }

        public bool HasBits(long n)
        {
            return n <= Remain;
        }

        public bool HasByte()
        {
            return HasBits(8);
        }

        public bool ReadBool()
        {
            return ReadBits(1) == 1;
        }

        public byte[] ReadManyBits(uint bits)
        {
            byte[] buffer = new byte[(bits + 7) / 8];

            for (uint i = 0; i < bits / 8; ++i)
            {
                buffer[i] = ReadByte();
            }

            if (bits % 8 > 0)
            {
                byte remain = (byte)(bits % 8);
                buffer[buffer.Length - 1] = (byte)ReadBits(remain);
            }

            return buffer;
        }

        public byte ReadByte(bool pad = false)
        {
            if (!pad)
            {
                return (byte)ReadBits(8);
            }
            else
            {
                byte remain = (byte)Math.Min(8, Remain);

                return (byte)((0xFFFFFFFF << remain) | ReadBits(remain));
            }
        }

        public char ReadChar()
        {
            return (char)ReadBits(8);
        }

        public string ReadString()
        {
            StringBuilder builder = new StringBuilder();

            char c = ReadChar();

            while (c != '\0')
            {
                builder.Append(c);

                c = ReadChar();
            }

            return builder.ToString();
        }

        public ushort ReadUInt16()
        {
            return (ushort)ReadBits(16);
        }

        public uint ReadUInt32()
        {
            return ReadBits(32);
        }

        public ulong ReadUInt64()
        {
            ulong low = ReadBits(32);
            ulong high = ReadBits(32);

            return (high << 32) | low;
        }

        public uint ReadVarUInt()
        {
            int read = 0;

            uint value = 0;
            uint got;
            do
            {
                got = ReadBits(8);

                uint lower = got & 0x7F;
                uint upper = got >> 7;

                value |= lower << read;
                read += 7;
            } while ((got >> 7) != 0 && read < 35);

            return value;
        }

        public byte[] ToBytes()
        {
            byte[] copied = new byte[Length];

            var old = Position;
            Position = 0;
            Read(copied, 0, copied.Length);
            Position = old;

            return copied;
        }

        public void WriteBitCoord(float f)
        {
            bool sign = f <= -COORD_RESOLUTION;
            uint integer = (uint)Math.Abs(f);
            uint fraction = (uint)(Math.Abs(((int)(f * COORD_DENOMINATOR))) & (COORD_DENOMINATOR - 1));

            WriteBool(integer != 0);
            WriteBool(fraction != 0);

            if (integer != 0 || fraction != 0)
            {
                WriteBool(sign);

                if (integer != 0)
                {
                    WriteBits(integer - 1, COORD_INTEGER_BITS);
                }

                if (fraction != 0)
                {
                    WriteBits(fraction, COORD_FRACTIONAL_BITS);
                }
            }
        }

        public void WriteBitVec3Coord(Vector v)
        {
            bool x = (v.X <= -COORD_RESOLUTION) || (COORD_RESOLUTION <= v.X);
            bool y = (v.Y <= -COORD_RESOLUTION) || (COORD_RESOLUTION <= v.Y);
            bool z = (v.Z <= -COORD_RESOLUTION) || (COORD_RESOLUTION <= v.Z);

            WriteBool(x);
            WriteBool(y);
            WriteBool(z);

            if (x)
            {
                WriteBitCoord(v.X);
            }

            if (y)
            {
                WriteBitCoord(v.Y);
            }

            if (z)
            {
                WriteBitCoord(v.Z);
            }
        }

        public void WriteBits(uint value, byte n)
        {
            if (n > 32)
            {
                throw new ArgumentException("Can't write more than 32 bits");
            }

            if (value != (value & (((long)1 << n) - 1)))
            {
                throw new ArgumentException("Value cannot fit in requested bits");
            }

            Allocate(n);

            byte read = (byte)(Position & 31);
            if (read + n <= 32)
            {
                long at = Position / 32;

                uint a = data[at];
                uint a_mask_l = (uint)((long)0xFFFFFFFF << (read + n));
                uint a_mask_r = (uint)(((long)1 << read) - 1);

                data[at] = (a & a_mask_l) | (value << read) | (a & a_mask_r);
            }
            else
            {
                long at = Position / 32;

                uint a = data[at];

                uint a_mask_r = (uint)(((long)1 << read) - 1);
                data[at] = (value << read) | (a & a_mask_r);

                byte remaining = (byte)((read + n) % 32);

                uint b = data[at + 1];
                uint b_mask_l = (uint)((long)0xFFFFFFFF << remaining);

                data[at + 1] = (b_mask_l & b) | (value >> (n - remaining));
            }

            if (Position == BitLength)
            {
                BitLength += n;
            }

            Position += n;
        }

        public void WriteBool(bool b)
        {
            WriteBits((uint)(b ? 1 : 0), 1);
        }

        public override void WriteByte(byte b)
        {
            WriteBits(b, 8);
        }

        public void WriteChar(char c)
        {
            WriteBits(c, 8);
        }

        public void WriteFloat(float f)
        {
            byte[] bytes = BitConverter.GetBytes(f);
            Write(bytes);
        }

        public void WriteInt16(short value)
        {
            if (value < 0)
            {
                WriteBits((ushort)-value, 15);
                WriteBool(true);
            }
            else
            {
                WriteBits((ushort)value, 15);
                WriteBool(false);
            }
        }

        public void WriteUInt16(ushort value)
        {
            WriteBits(value, 16);
        }

        public void WriteUInt32(uint value)
        {
            WriteBits(value, 32);
        }

        public void WriteVarUInt(uint value)
        {
            do
            {
                uint lower = value & 0x7F;
                value >>= 7;

                uint high = (value > 0) ? (uint)0x80 : 0;

                WriteBits(high | lower, 8);
            } while (value > 0);
        }

        private void Allocate(long bits)
        {
            long dwords = (Position + bits + 31) / 32;
            BitLength = Math.Max(Position + bits, BitLength);

            if (dwords <= data.Length)
            {
                return;
            }

            long newLength = data.Length;
            while (newLength < dwords)
            {
                newLength *= 2;
            }

            uint[] newData = new uint[newLength];
            Array.Copy(data, newData, data.Length);

            data = newData;
        }

        public override bool CanRead { get { return true; } }

        public override bool CanSeek { get { return true; } }

        public override bool CanWrite { get { return true; } }

        public override void Flush() { }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (offset + count > buffer.Length)
            {
                throw new ArgumentException("offset + count > buffer.length");
            }

            if (offset < 0)
            {
                throw new ArgumentOutOfRangeException("offset is negative");
            }
            else if (count < 0)
            {
                throw new ArgumentOutOfRangeException("count is negative");
            }

            int got;
            for (got = 0; HasBits(8) && got < count; ++got)
            {
                buffer[got + offset] = ReadByte();
            }

            if (HasBits(1) && got < count)
            {
                byte remain = (byte)Remain;

                // really only need 0xFF
                buffer[got] = (byte)((0xFFFFFFFF << remain) | ReadBits(remain));
            }

            return got;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            if (origin == SeekOrigin.Begin)
            {
                Position = offset * 8;
            }
            else if (origin == SeekOrigin.Current)
            {
                Position += offset * 8;
            }
            else if (origin == SeekOrigin.End)
            {
                Position = BitLength - offset * 8;
            }
            else
            {
                throw new ArgumentException("Unsupported SeekOrigin " + origin);
            }

            return Position;
        }

        public override void SetLength(long newLength)
        {
            if (newLength < Length)
            {
                throw new InvalidOperationException("Can't shrink a bitstream");
            }

            long pos = Position;
            Position = 0;
            Allocate(newLength * 8);
            Position = pos;
        }

        public void Write(byte[] buffer)
        {
            Write(buffer, 0, buffer.Length);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            Allocate(count * 8);

            for (int i = offset; i < offset + count; ++i)
            {
                WriteBits(buffer[i], 8);
            }
        }
    }
}
