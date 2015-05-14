/*
    This file heavily based off of the nora project.
    See https://github.com/dschleck/nora/blob/master/lara/GameProcessor.cs
*/

using System;
using System.Collections.Generic;
using Dota2.Engine.Game;
using Dota2.Engine.Session.State.Enums;
using Dota2.Engine.Session.Unpackers;
using Dota2.GC.Dota.Internal;
using Dota2.Utils;
using ProtoBuf;

namespace Dota2.Engine.Session.Handlers.Game
{
    internal class DotaGame
    {
        private readonly DotaGameConnection connection;
        private readonly EntityUpdater entityUpdater;
        private readonly DotaGameState state;
        private readonly StringTableUpdater stringTableUpdater;

        public DotaGame(DotaGameState state, DotaGameConnection connection)
        {
            this.state = state;
            this.connection = connection;
            entityUpdater = new EntityUpdater(state);
            stringTableUpdater = new StringTableUpdater();
        }

        public void EnterGame()
        {
            var ack = new CCLCMsg_BaselineAck();
            ack.baseline_nr = state.Baseline;
            ack.baseline_tick = (int) state.ServerTick;
            var ackMsg = DotaGameConnection.ConvertProtoToMessage(
                (uint) CLC_Messages.clc_BaselineAck,
                ack);

            var ss = new CNETMsg_SignonState();
            ss.num_server_players = 0;
            ss.signon_state = (uint) SIGNONSTATE.SIGNONSTATE_FULL;
            ss.spawn_count = state.ServerCount;

            var ssMessage = DotaGameConnection.ConvertProtoToMessage(
                (uint) NET_Messages.net_SignonState,
                ss);

            var le = new CCLCMsg_ListenEvents();
            for (uint i = 0; i < 267; i += 32)
            {
                le.event_mask.Add(0xffffffff);
            }
            le.event_mask.Add(0x000003ff);
            var leMessage = DotaGameConnection.ConvertProtoToMessage(
                (uint) CLC_Messages.clc_ListenEvents,
                le);

            connection.SendReliably(ackMsg, ssMessage, leMessage);

            var clientMsgs = new List<DotaGameConnection.Message>();
            for (var i = 0; i < 10; ++i)
            {
                var msg = new CCLCMsg_ClientMessage();
                msg.data = new byte[] {0x0D, 0xCD, 0xCC, 0xCC, 0x3F};
                msg.msg_type = 2;

                var msgMessage = DotaGameConnection.ConvertProtoToMessage(
                    (uint) CLC_Messages.clc_ClientMessage,
                    msg);
                clientMsgs.Add(msgMessage);
            }
            connection.SendUnreliably(clientMsgs.ToArray());
        }

        public Events? Process(DotaGameConnection.Message message)
        {
            using (var stream = Bitstream.CreateWith(message.Data))
            {
                if (message.Type == (uint) NET_Messages.net_NOP)
                {
                    return null;
                }
                if (message.Type == (uint) NET_Messages.net_Disconnect)
                {
                    return Process(Serializer.Deserialize<CNETMsg_Disconnect>(stream));
                }
                if (message.Type == (uint) NET_Messages.net_StringCmd)
                {
                    return Process(Serializer.Deserialize<CNETMsg_StringCmd>(stream));
                }
                if (message.Type == (uint) NET_Messages.net_Tick)
                {
                    return Process(Serializer.Deserialize<CNETMsg_Tick>(stream));
                }
                if (message.Type == (uint) SVC_Messages.svc_PacketEntities)
                {
                    return Process(Serializer.Deserialize<CSVCMsg_PacketEntities>(stream));
                }
                if (message.Type == (uint) SVC_Messages.svc_UpdateStringTable)
                {
                    return Process(Serializer.Deserialize<CSVCMsg_UpdateStringTable>(stream));
                }
                if (message.Type == (uint) SVC_Messages.svc_UserMessage)
                {
                    return Process(Serializer.Deserialize<CSVCMsg_UserMessage>(stream));
                }
                if (message.Type == (uint) SVC_Messages.svc_GameEvent)
                {
                    return Process(Serializer.Deserialize<CSVCMsg_GameEvent>(stream));
                }
                return null;
            }
        }

        private Events? Process(CNETMsg_Disconnect message)
        {
            return Events.DISCONNECTED;
        }

        private Events? Process(CNETMsg_StringCmd message)
        {
            return null;
        }

        private Events? Process(CNETMsg_Tick message)
        {
            if (message.tick > state.ClientTick)
            {
                state.ClientTick = message.tick;
            }

            state.ServerTick = message.tick;

            return null;
        }

        private Events? Process(CSVCMsg_PacketEntities message)
        {
            using (var stream = Bitstream.CreateWith(message.entity_data))
            {
                entityUpdater.Update(
                    stream,
                    (uint) message.baseline,
                    message.update_baseline,
                    (uint) message.updated_entries,
                    message.is_delta);
            }

            if (message.update_baseline)
            {
                var ack = new CCLCMsg_BaselineAck
                {
                    baseline_nr = state.Baseline,
                    baseline_tick = (int) state.ServerTick
                };
                var ackMsg = DotaGameConnection.ConvertProtoToMessage(
                    (uint) CLC_Messages.clc_BaselineAck,
                    ack);
                connection.SendReliably(ackMsg);
                return null;
            }
            return null;
        }

        private Events? Process(CSVCMsg_UpdateStringTable message)
        {
            var table = state.Strings[message.table_id];

            stringTableUpdater.Update(table, message.num_changed_entries, message.string_data);
            return null;
        }

        private Events? Process(CSVCMsg_UserMessage message)
        {
            if (message.msg_type == (int) EBaseUserMessages.UM_SayText2)
            {
                using (var stream = Bitstream.CreateWith(message.msg_data))
                {
                    return Process(Serializer.Deserialize<CUserMsg_SayText2>(stream));
                }
            }
            if (message.msg_type == (int) EDotaUserMessages.DOTA_UM_ChatEvent)
            {
                using (var stream = Bitstream.CreateWith(message.msg_data))
                {
                    return Process(Serializer.Deserialize<CDOTAUserMsg_ChatEvent>(stream));
                }
            }
            return null;
        }

        private Events? Process(CUserMsg_SayText2 message)
        {
            return null;
        }

        private Events? Process(CDOTAUserMsg_ChatEvent message)
        {
            return null;
        }

        private Events? Process(CSVCMsg_GameEvent message)
        {
            return null;
        }

        public Events? Process(byte[] message)
        {
            throw new NotImplementedException();
        }
    }
}