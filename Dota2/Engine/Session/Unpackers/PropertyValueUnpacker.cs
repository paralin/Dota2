/*
    This file heavily based off of the nora project.
    See https://github.com/dschleck/nora/blob/master/lara/unpackers/PropertyValueUnpacker.cs
*/

using System;
using System.Collections.Generic;
using System.Linq;
using Dota2.Engine.Game.Data;
using Dota2.Utils;

namespace Dota2.Engine.Session.Unpackers
{
    /// <summary>
    ///     Unpacks incoming properties.
    /// </summary>
    public class PropertyValueUnpacker
    {
        public const int COORD_INTEGER_BITS = 14;
        public const int COORD_FRACTIONAL_BITS = 5;
        public const int COORD_DENOMINATOR = (1 << (COORD_FRACTIONAL_BITS));
        public const double COORD_RESOLUTION = (1.0/(COORD_DENOMINATOR));
        public const int NORMAL_FRACTIONAL_BITS = 11;
        public const int NORMAL_DENOMINATOR = ((1 << (NORMAL_FRACTIONAL_BITS)) - 1);
        public const double NORMAL_RESOLUTION = (1.0/(NORMAL_DENOMINATOR));
        private const uint MAX_STRING_LENGTH = 0x200;

        public uint UnpackInt(PropertyInfo info, Bitstream stream)
        {
            var flags = info.Flags;

            if (flags.HasFlag(PropertyInfo.MultiFlag.EncodedAgainstTickcount))
            {
                if (flags.HasFlag(PropertyInfo.MultiFlag.Unsigned))
                {
                    return stream.ReadVarUInt();
                }
                var value = stream.ReadVarUInt();
                return unchecked((uint) ((-(value & 1)) ^ (value >> 1)));
            }

            var numBits = info.NumBits;

            var isUnsigned = Convert.ToUInt32(flags.HasFlag(PropertyInfo.MultiFlag.Unsigned));
            var signer = (0x80000000 >> (32 - numBits)) & unchecked((isUnsigned - 1));

            {
                var value = stream.ReadBits(numBits) ^ signer;
                return value - signer;
            }
        }

        public float UnpackFloat(PropertyInfo info, Bitstream stream)
        {
            var flags = info.Flags;

            if (flags.HasFlag(PropertyInfo.MultiFlag.Coord))
            {
                return UnpackFloatCoord(stream);
            }
            if (flags.HasFlag(PropertyInfo.MultiFlag.CoordMp))
            {
                return UnpackFloatCoordMp(stream, FloatType.None);
            }
            if (flags.HasFlag(PropertyInfo.MultiFlag.CoordMpLowPrecision))
            {
                return UnpackFloatCoordMp(stream, FloatType.LowPrecision);
            }
            if (flags.HasFlag(PropertyInfo.MultiFlag.CoordMpIntegral))
            {
                return UnpackFloatCoordMp(stream, FloatType.Integral);
            }
            if (flags.HasFlag(PropertyInfo.MultiFlag.NoScale))
            {
                return UnpackFloatNoScale(stream);
            }
            if (flags.HasFlag(PropertyInfo.MultiFlag.Normal))
            {
                return UnpackFloatNormal(stream);
            }
            if (flags.HasFlag(PropertyInfo.MultiFlag.CellCoord))
            {
                return UnpackFloatCellCoord(info, stream, FloatType.None);
            }
            if (flags.HasFlag(PropertyInfo.MultiFlag.CellCoordLowPrecision))
            {
                return UnpackFloatCellCoord(info, stream, FloatType.LowPrecision);
            }
            if (flags.HasFlag(PropertyInfo.MultiFlag.CellCoordIntegral))
            {
                return UnpackFloatCellCoord(info, stream, FloatType.Integral);
            }
            var dividend = stream.ReadBits(info.NumBits);
            var divisor = (uint) (1 << info.NumBits) - 1;

            var f = ((float) dividend)/divisor;
            var range = info.HighValue - info.LowValue;

            return f*range + info.LowValue;
        }

        private float UnpackFloatCoord(Bitstream stream)
        {
            var hasInteger = stream.ReadBool();
            var hasFraction = stream.ReadBool();

            if (hasInteger || hasFraction)
            {
                var sign = stream.ReadBool();

                uint integer = 0;
                if (hasInteger)
                {
                    integer = stream.ReadBits(COORD_INTEGER_BITS) + 1;
                }

                uint fraction = 0;
                if (hasFraction)
                {
                    fraction = stream.ReadBits(COORD_FRACTIONAL_BITS);
                }

                var f = (float) (integer + fraction*COORD_RESOLUTION);

                if (sign)
                {
                    f *= -1;
                }

                return f;
            }
            return 0;
        }

        private float UnpackFloatCoordMp(Bitstream stream, FloatType type)
        {
            throw new NotImplementedException();
        }

        private float UnpackFloatNoScale(Bitstream stream)
        {
            var data = stream.ReadManyBits(32);
            return BitConverter.ToSingle(data, 0);
        }

        private float UnpackFloatNormal(Bitstream stream)
        {
            var sign = stream.ReadBool();
            var value = stream.ReadBits(NORMAL_FRACTIONAL_BITS);

            var f = (float) (value*NORMAL_RESOLUTION);

            if (sign)
            {
                f *= -1;
            }

            return f;
        }

        private float UnpackFloatCellCoord(PropertyInfo info, Bitstream stream, FloatType type)
        {
            var value = stream.ReadBits(info.NumBits);
            float f = value;

            if ((value >> 31) > 0)
            {
                f *= -1;
            }

            if (type == FloatType.None)
            {
                var fraction = stream.ReadBits(5);

                return f + 0.03125f*fraction;
            }
            if (type == FloatType.LowPrecision)
            {
                var fraction = stream.ReadBits(3);

                return f + 0.125f*fraction;
            }
            if (type == FloatType.Integral)
            {
                return f;
            }
            throw new InvalidOperationException("Unknown float type");
        }

        public Vector UnpackVector(PropertyInfo info, Bitstream stream)
        {
            var x = UnpackFloat(info, stream);
            var y = UnpackFloat(info, stream);
            float z;

            if (info.Flags.HasFlag(PropertyInfo.MultiFlag.Normal))
            {
                var sign = stream.ReadBool();

                var f = x*x + y*y;

                if (1 >= f)
                {
                    z = 0;
                }
                else
                {
                    z = (float) Math.Sqrt(1 - f);
                }

                if (sign)
                {
                    z *= -1;
                }
            }
            else
            {
                z = UnpackFloat(info, stream);
            }

            return new Vector(x, y, z);
        }

        public VectorXy UnpackVectorXy(PropertyInfo info, Bitstream stream)
        {
            var x = UnpackFloat(info, stream);
            var y = UnpackFloat(info, stream);
            return new VectorXy(x, y);
        }

        public string UnpackString(PropertyInfo info, Bitstream stream)
        {
            var length = stream.ReadBits(9);

            var buffer = new byte[length];
            stream.Read(buffer, 0, (int) length);

            return new string((from byte b in buffer select (char) b).ToArray<char>());
        }

        public void UnpackArray(uint tick, List<Property> elements, PropertyInfo info, Bitstream stream)
        {
            var countBits = MiscMath.Log2(info.NumElements + 1);
            var count = stream.ReadBits(countBits);

            if (elements.Count > count)
            {
                elements.RemoveRange(0, elements.Count - (int) count);
            }
            else
            {
                while (elements.Count < count)
                {
                    elements.Add(Property.For(info.ArrayProp));
                }
            }

            foreach (var element in elements)
            {
                element.Update(tick, this, stream);
            }
        }

        public ulong UnpackInt64(PropertyInfo info, Bitstream stream)
        {
            if (info.Flags.HasFlag(PropertyInfo.MultiFlag.EncodedAgainstTickcount))
            {
                return stream.ReadVarUInt();
            }
            var negate = false;
            var secondBits = (byte) (info.NumBits - 32);

            if (!info.Flags.HasFlag(PropertyInfo.MultiFlag.Unsigned))
            {
                --secondBits;

                if (stream.ReadBool())
                {
                    negate = true;
                }
            }

            ulong a = stream.ReadBits(32);
            ulong b = stream.ReadBits(secondBits);
            var value = (b << 32) | a;

            if (negate)
            {
                value = unchecked((ulong) ((long) value*-1));
            }

            return value;
        }

        private enum FloatType
        {
            None,
            LowPrecision,
            Integral
        }
    }
}