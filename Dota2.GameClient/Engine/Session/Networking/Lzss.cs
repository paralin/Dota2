using System;

/*
    This file heavily based off of the nora project.
    See https://github.com/dschleck/nora/blob/master/lara/net/Lzss.cs
*/

namespace Dota2.GameClient.Engine.Session.Networking
{
    /// <summary>
    ///     LZss compression.
    /// </summary>
    internal class Lzss
    {
        public static byte[] Decompress(byte[] buffer)
        {
            var decompressed_length = BitConverter.ToUInt32(buffer, 0);
            var decompressed = new byte[decompressed_length];
            uint decompressing_at = 0;

            uint buffer_at = 4;
            uint since_block = 0;
            var block = 0;

            while (true)
            {
                if (since_block == 0)
                {
                    block = buffer[buffer_at];
                    ++buffer_at;
                }

                since_block = (since_block + 1) & 7;

                if ((block & 1) == 0)
                {
                    var b = buffer[buffer_at];
                    decompressed[decompressing_at] = b;
                    ++decompressing_at;
                }
                else
                {
                    uint b = buffer[buffer_at + 1];
                    var to_read = (byte) (b & 0x0F);

                    if (to_read == 0)
                    {
                        break;
                    }

                    ++to_read;

                    var offset = (((uint) (buffer[buffer_at] << 4)) | (b >> 4)) + 1;
                    ++buffer_at;

                    for (var i = 0; i < to_read; ++i)
                    {
                        decompressed[decompressing_at] = decompressed[decompressing_at - offset];
                        ++decompressing_at;
                    }
                }

                block >>= 1;
                ++buffer_at;
            }

            return decompressed;
        }
    }
}