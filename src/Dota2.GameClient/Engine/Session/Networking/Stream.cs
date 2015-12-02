using System;
using System.Text;
using Dota2.GameClient.Utils;
using Snappy.Sharp;

/*
    This file heavily based off of the nora project.
    See https://github.com/dschleck/nora/blob/master/lara/net/Stream.cs
*/

namespace Dota2.GameClient.Engine.Session.Networking
{
    /// <summary>
    ///     A network stream over a connection.
    /// </summary>
    internal class Stream
    {
        private int countReceived;
        private byte[] dataIn;
        private bool[] dataReceived;
        private ChunkedHeader header;

        private Stream()
        {
        }

        public bool Receiving { get; private set; }

        public static Stream Create()
        {
            return new Stream();
        }

        public Message? Receive(Bitstream stream)
        {
            var hasData = stream.ReadBool();

            if (!hasData)
            {
                return null;
            }

            if (stream.ReadBool())
            {
                return ReadChunk(stream);
            }
            return ReadSingle(stream);
        }

        private void ReadChunkHeader(Bitstream stream)
        {
            header = new ChunkedHeader();

            header.IsFile = stream.ReadBool();
            if (header.IsFile)
            {
                var filenameLength = stream.ReadUInt32();
                var filename = new byte[filenameLength + 1]; // semantically wrong. should be
                // 0x104
                stream.Read(filename, 0, (int) filenameLength); // and then read to end of string
                filename[filenameLength] = 0; // whatever 
                header.Filename = Encoding.UTF8.GetString(filename);
                throw new NotImplementedException();
            }

            header.IsCompressed = stream.ReadBool();
            if (header.IsCompressed)
            {
                header.DecompressedLength = stream.ReadBits(26);
            }

            header.ByteLength = stream.ReadBits(26);
            header.ChunkCount =
                (header.ByteLength + DotaGameConnection.BYTES_PER_CHUNK - 1)/
                DotaGameConnection.BYTES_PER_CHUNK;

            Receiving = true;
            dataIn = new byte[header.ByteLength];
            dataReceived = new bool[header.ChunkCount];
            countReceived = 0;
        }

        private Message? ReadChunk(Bitstream stream)
        {
            var offset = stream.ReadBits(18);
            var count = stream.ReadBits(3);

            if (offset == 0)
            {
                ReadChunkHeader(stream);
            }

            var byteOffset = offset*DotaGameConnection.BYTES_PER_CHUNK;

            uint byteCount;
            if (offset + count < header.ChunkCount)
            {
                byteCount = count*DotaGameConnection.BYTES_PER_CHUNK;
            }
            else
            {
                byteCount = header.ByteLength - byteOffset;
            }

            stream.Read(dataIn, (int) byteOffset, (int) byteCount);

            for (var i = offset;
                i < offset + count;
                ++i)
            {
                if (!dataReceived[i])
                {
                    dataReceived[i] = true;
                    ++countReceived;
                }
            }

            if (countReceived == header.ChunkCount)
            {
                Receiving = false;
                return new Message
                {
                    IsCompressed = header.IsCompressed,
                    DecompressedLength = header.DecompressedLength,
                    Data = dataIn
                };
            }
            return null;
        }

        private Message ReadSingle(Bitstream stream)
        {
            var isCompressed = stream.ReadBool();

            if (isCompressed)
            {
                var uncompressed_length = stream.ReadBits(26);
                var length = stream.ReadBits(18);

                var data = new byte[length];
                stream.Read(data, 0, (int) length);

                return new Message
                {
                    IsCompressed = false,
                    Data = new SnappyDecompressor().Decompress(data, 0, data.Length)
                };
            }
            else
            {
                var length = stream.ReadBits(18);

                var data = new byte[length];
                stream.Read(data, 0, (int) length);

                return new Message
                {
                    IsCompressed = false,
                    Data = data
                };
            }
        }

        private struct ChunkedHeader
        {
            public uint ChunkCount { get; set; }
            public uint ByteLength { get; set; }
            public bool IsCompressed { get; set; }
            public uint DecompressedLength { get; set; }
            public bool IsFile { get; set; }
            public string Filename { get; set; }
        }

        public struct Message
        {
            public bool IsCompressed { get; set; }
            public uint DecompressedLength { get; set; }
            public byte[] Data { get; set; }
        }
    }
}