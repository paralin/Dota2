using System;
using System.IO;
using System.Linq;
using System.Net;
using SteamKit2;

/*
    This code from the Nora project.
    See https://github.com/dschleck/nora/blob/master/clara/AuthTicket.cs
 */

namespace Dota2.GameClient.Engine.Data
{
    /// <summary>
    ///     Generates auth tickets.
    /// </summary>
    internal class AuthTicket
    {
        private static int connectionCount;

        public static byte[] CreateAuthTicket(byte[] token, IPAddress ip)
        {
            uint sessionSize = 4 + // unknown 1
                               4 + // unknown 2
                               4 + // external IP
                               4 + // filler
                               4 + // timestamp
                               4; // connection count

            var stream = new MemoryStream();
            using (var writer = new BinaryWriter(stream))
            {
                writer.Write(token.Length);
                writer.Write(token);

                writer.Write(sessionSize);
                writer.Write(1);
                writer.Write(2);

                var externalBytes = ip.GetAddressBytes();
                writer.Write(externalBytes.Reverse().ToArray());

                writer.Write(0);
                writer.Write(Environment.TickCount);
                writer.Write(connectionCount++);
            }

            return stream.ToArray();
        }

        public static byte[] CreateServerTicket(
            SteamID id, byte[] auth, byte[] ownershipTicket)
        {
            long size = 8 + // steam ID
                        auth.Length +
                        4 + // length of ticket
                        ownershipTicket.Length;

            var stream = new MemoryStream();
            using (var writer = new BinaryWriter(stream))
            {
                writer.Write((ushort) size);
                writer.Write(id.ConvertToUInt64());

                writer.Write(auth.ToArray());

                writer.Write(ownershipTicket.Length);
                writer.Write(ownershipTicket);

                writer.Write(0);
            }

            return stream.ToArray();
        }
    }
}