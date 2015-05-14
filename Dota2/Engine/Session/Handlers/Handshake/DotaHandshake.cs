/*
    This file heavily based off of the nora project.
    See https://github.com/dschleck/nora/blob/master/lara/Handshaker.cs
*/

using System;
using System.Text;
using Dota2.Engine.Data;
using Dota2.Engine.Game;
using Dota2.Engine.Session.State.Enums;
using Dota2.Engine.Session.State.Interfaces;
using Dota2.GC.Dota.Internal;
using Dota2.Utils;
using ProtoBuf;

namespace Dota2.Engine.Session.Handlers.Handshake
{
    /// <summary>
    ///     A handler that completes the handshake with the game server.
    /// </summary>
    internal class DotaHandshake : MessageHandler
    {
        private const char C2S_REQUEST = 'q';
        private const char S2C_CHALLENGE = 'A';
        private const char C2S_CONNECT = 'k';
        private const char S2C_ACCEPT = 'B';
        private const char S2C_REJECT = '9';
        // These constants may change and fail the handshake.
        private const uint SOURCE_PROTOCOL = 0x5A4F4933;
        private const uint SOURCE_VERSION = 0x00000029;
        private const uint STEAM_VERSION = 0x00000003;
        private readonly uint client_challenge;
        private readonly DotaGameConnection connection;
        private readonly DOTAConnectDetails details;
        private readonly DotaGameState state;
        private uint server_challenge;
        private ulong server_id;

        public DotaHandshake(
            DOTAConnectDetails details,
            DotaGameState state,
            DotaGameConnection connection)
        {
            this.details = details;
            this.connection = connection;
            this.state = state;
            client_challenge = (uint) new Random().Next();
        }

        public Events? Handle(byte[] response)
        {
            using (var stream = Bitstream.CreateWith(response))
            {
                var type = stream.ReadByte();

                if (type == S2C_CHALLENGE)
                {
                    server_challenge = stream.ReadUInt32();
                    server_id = stream.ReadUInt64();
                    return Events.HANDSHAKE_CHALLENGE;
                }
                if (type == S2C_ACCEPT)
                {
                    return Events.HANDSHAKE_COMPLETE;
                }
                if (type == S2C_REJECT)
                {
                    return Events.REJECTED;
                }
                throw new ArgumentException("Unknown response type " + type);
            }
        }

        public Events? Handle(DotaGameConnection.Message message)
        {
            throw new NotImplementedException();
        }

        public void RequestHandshake()
        {
            connection.Open();

            using (var stream = Bitstream.Create())
            {
                stream.WriteChar(C2S_REQUEST);

                stream.WriteUInt32(client_challenge);

                foreach (var c in "0000000000")
                {
                    stream.WriteChar(c);
                }
                stream.WriteByte(0);

                connection.EnqueueOutOfBand(stream.ToBytes());
            }
        }

        public void RespondHandshake()
        {
            using (var stream = Bitstream.Create())
            {
                stream.WriteChar(C2S_CONNECT);
                stream.WriteUInt32(SOURCE_VERSION);
                stream.WriteUInt32(STEAM_VERSION);
                stream.WriteUInt32(server_challenge);
                stream.WriteUInt32(client_challenge);

                stream.Write(Encoding.UTF8.GetBytes(state.CVars["name"]));
                stream.WriteByte(0);

                stream.Write(Encoding.UTF8.GetBytes(details.PassKey));
                stream.WriteByte(0);

                // num players to join, obviously one in this case
                stream.WriteByte(1);

                // cvars
                stream.WriteByte((byte) CLC_Messages.clc_SplitPlayerConnect);

                var split = new CCLCMsg_SplitPlayerConnect();
                split.convars = state.ExposeCVars();
                Serializer.SerializeWithLengthPrefix(stream, split, PrefixStyle.Base128);

                // auth ticket
                var bytes = new byte[details.AuthTicket.Length];
                details.AuthTicket.CopyTo(bytes, 0);

                var carry = false;
                for (var i = 0; i < bytes.Length; ++i)
                {
                    var d = (byte) (((bytes[i]*2)%256) + (carry ? 1 : 0));
                    carry = bytes[i] > 127;
                    bytes[i] = d;
                }
                stream.Write(bytes);

                connection.EnqueueOutOfBand(stream.ToBytes());
            }
        }
    }
}