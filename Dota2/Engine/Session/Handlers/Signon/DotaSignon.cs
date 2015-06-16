/*
    This file heavily based off of the nora project.
    See https://github.com/dschleck/nora/blob/master/lara/SignonHandleor.cs
*/

using System;
using Dota2.Engine.Data;
using Dota2.Engine.Game;
using Dota2.Engine.Game.Data;
using Dota2.Engine.Session.State.Enums;
using Dota2.Engine.Session.Unpackers;
using Dota2.GC.Dota.Internal;
using Dota2.Utils;
using ProtoBuf;

namespace Dota2.Engine.Session.Handlers.Signon
{
    /// <summary>
    ///     Handles the Signon messages from the server.
    /// </summary>
    internal class DotaSignon : IHandler
    {
        private readonly DotaGameConnection connection;
        private readonly DOTAConnectDetails details;
        private readonly EntityUpdater entityUpdater;
        private readonly SendTableFlattener sendTableFlattener;
        private readonly DotaGameState state;
        private readonly StringTableUpdater stringTableUpdater;

        /// <summary>
        ///     Initialize the signon handler.
        /// </summary>
        /// <param name="state"></param>
        /// <param name="connection"></param>
        public DotaSignon(DotaGameState state, DotaGameConnection connection, DOTAConnectDetails details)
        {
            this.state = state;
            this.details = details;
            this.connection = connection;
            entityUpdater = new EntityUpdater(state);
            sendTableFlattener = new SendTableFlattener();
            stringTableUpdater = new StringTableUpdater();
        }

        public Events? Handle(byte[] message)
        {
            // todo: Handle this unhandled message?
            return null;
        }

        public Events? Handle(DotaGameConnection.Message message)
        {
            using (var stream = Bitstream.CreateWith(message.Data))
            {
                if (message.Type == (uint) NET_Messages.net_NOP)
                {
                    return null;
                }
                if (message.Type == (uint) NET_Messages.net_Disconnect)
                {
                    return Handle(Serializer.Deserialize<CNETMsg_Disconnect>(stream));
                }
                if (message.Type == (uint) NET_Messages.net_Tick)
                {
                    return Handle(Serializer.Deserialize<CNETMsg_Tick>(stream));
                }
                if (message.Type == (uint) NET_Messages.net_SetConVar)
                {
                    return Handle(Serializer.Deserialize<CNETMsg_SetConVar>(stream));
                }
                if (message.Type == (uint) NET_Messages.net_SignonState)
                {
                    return Handle(Serializer.Deserialize<CNETMsg_SignonState>(stream));
                }
                if (message.Type == (uint) SVC_Messages.svc_ServerInfo)
                {
                    return Handle(Serializer.Deserialize<CSVCMsg_ServerInfo>(stream));
                }
                if (message.Type == (uint) SVC_Messages.svc_SendTable)
                {
                    return Handle(Serializer.Deserialize<CSVCMsg_SendTable>(stream));
                }
                if (message.Type == (uint) SVC_Messages.svc_ClassInfo)
                {
                    return Handle(Serializer.Deserialize<CSVCMsg_ClassInfo>(stream));
                }
                if (message.Type == (uint) SVC_Messages.svc_PacketEntities)
                {
                    return Handle(Serializer.Deserialize<CSVCMsg_PacketEntities>(stream));
                }
                if (message.Type == (uint) SVC_Messages.svc_CreateStringTable)
                {
                    return Handle(Serializer.Deserialize<CSVCMsg_CreateStringTable>(stream));
                }
                if (message.Type == (uint) SVC_Messages.svc_UpdateStringTable)
                {
                    return Handle(Serializer.Deserialize<CSVCMsg_UpdateStringTable>(stream));
                }
                if (message.Type == (uint) SVC_Messages.svc_Print)
                {
                    return Handle(Serializer.Deserialize<CSVCMsg_Print>(stream));
                }
                if (message.Type == (uint) SVC_Messages.svc_GameEventList)
                {
                    return Handle(Serializer.Deserialize<CSVCMsg_GameEventList>(stream));
                }
                return null;
            }
        }

        public void EnterConnected()
        {
            connection.OpenChannel();
            state.Reset();

            var scv = new CNETMsg_SetConVar();
            scv.convars = state.ExposeCVars();

            var scvMessage = DotaGameConnection.ConvertProtoToMessage(
                (uint) NET_Messages.net_SetConVar,
                scv);

            var ss = new CNETMsg_SignonState();
            ss.num_server_players = 0;
            ss.spawn_count = 0xFFFFFFFF;
            ss.signon_state = (uint) SIGNONSTATE.SIGNONSTATE_CONNECTED;

            var ssMessage = DotaGameConnection.ConvertProtoToMessage(
                (uint) NET_Messages.net_SignonState,
                ss);

            connection.SendReliably(scvMessage, ssMessage);
        }

        public void EnterNew()
        {
            var ci = new CCLCMsg_ClientInfo();
            ci.server_count = state.ServerCount;
            var ciMessage = DotaGameConnection.ConvertProtoToMessage(
                (uint) CLC_Messages.clc_ClientInfo,
                ci);

            var ss = new CNETMsg_SignonState();
            ss.signon_state = (uint) SIGNONSTATE.SIGNONSTATE_NEW;
            ss.spawn_count = state.ServerCount;
            ss.num_server_players = 0;

            var ssMessage = DotaGameConnection.ConvertProtoToMessage(
                (uint) NET_Messages.net_SignonState,
                ss);

            var scv = new CNETMsg_SetConVar();
            scv.convars = new CMsg_CVars();
            var cvar = new CMsg_CVars.CVar();
            cvar.name = "steamworks_sessionid_client";
            cvar.value = details.SteamworksSessionId.ToString();
            state.CVars["steamworks_sessionid_client"] = details.SteamworksSessionId.ToString();
            scv.convars.cvars.Add(cvar);

            var scvMessage = DotaGameConnection.ConvertProtoToMessage(
                (uint) NET_Messages.net_SetConVar,
                scv);

            connection.SendReliably(ciMessage, ssMessage, scvMessage);
        }

        public void EnterPrespawn()
        {
            var ss = new CNETMsg_SignonState();
            ss.signon_state = (uint) SIGNONSTATE.SIGNONSTATE_PRESPAWN;
            ss.spawn_count = state.ServerCount;
            ss.num_server_players = 0;

            var ssMessage = DotaGameConnection.ConvertProtoToMessage(
                (uint) NET_Messages.net_SignonState,
                ss);

            // Send mask for game events? 
            // 0c 23
            //   0d 8b820592
            //   0d 0140e890
            //   0d f6ffff7f
            //   0d ff9bfc6e
            //   0d 0310e87c
            //   0d cbfffff8
            //   0d effc0700
            var le = new CCLCMsg_ListenEvents();
            for (uint i = 0; i < 267; i += 32)
            {
                le.event_mask.Add(0xffffffff);
            }
            le.event_mask.Add(0x0000000a);
            var leMessage = DotaGameConnection.ConvertProtoToMessage(
                (uint) CLC_Messages.clc_ListenEvents,
                le);

            connection.SendReliably(ssMessage, leMessage);
        }

        public void EnterSpawn()
        {
            var ss = new CNETMsg_SignonState();
            ss.signon_state = (uint) SIGNONSTATE.SIGNONSTATE_SPAWN;
            ss.spawn_count = state.ServerCount;
            ss.num_server_players = 0;

            var ssMessage = DotaGameConnection.ConvertProtoToMessage(
                (uint) NET_Messages.net_SignonState,
                ss);
            connection.SendReliably(ssMessage);
        }

        private Events? Handle(CNETMsg_Disconnect message)
        {
            return Events.DISCONNECTED;
        }

        private Events? Handle(CNETMsg_Tick message)
        {
            if (message.tick > state.ClientTick)
            {
                state.ClientTick = message.tick;
            }

            state.ServerTick = message.tick;

            return null;
        }

        private Events? Handle(CNETMsg_SetConVar message)
        {
            foreach (var cvar in message.convars.cvars)
            {
                state.CVars[cvar.name] = cvar.value;
            }

            return null;
        }

        private Events? Handle(CNETMsg_SignonState message)
        {
            switch (message.signon_state)
            {
                case (uint) SIGNONSTATE.SIGNONSTATE_CONNECTED:
                    return Events.CONNECTED;
                case (uint) SIGNONSTATE.SIGNONSTATE_NEW:
                    return Events.LOADING_START;
                case (uint) SIGNONSTATE.SIGNONSTATE_PRESPAWN:
                    return Events.PRESPAWN_START;
                case (uint) SIGNONSTATE.SIGNONSTATE_SPAWN:
                    return Events.SPAWNED;
                default:
                    throw new NotImplementedException("Unknown signon state " + message.signon_state);
            }
        }

        private Events? Handle(CSVCMsg_ServerInfo message)
        {
            state.ServerCount = (uint) message.server_count;
            state.TickInterval = message.tick_interval;

            return null;
        }

        private Events? Handle(CSVCMsg_SendTable message)
        {
            state.SendTables.Add(SendTable.CreateWith(message));
            return null;
        }

        private Events? Handle(CSVCMsg_ClassInfo message)
        {
            foreach (var clazz in message.classes)
            {
                var created = EntityClass.CreateWith(clazz);
                state.Classes.Add(created);
                state.ClassesByName.Add(created.ClassName, created);
            }

            foreach (var table in state.SendTables)
            {
                for (var i = 0; i < table.Properties.Count; ++i)
                {
                    var prop = table.Properties[i];

                    if (prop.Type == PropertyInfo.PropertyType.Array)
                    {
                        prop.ArrayProp = table.Properties[i - 1];
                    }
                }
            }

            state.FlatTables.AddRange(sendTableFlattener.Flatten(state.SendTables));
            return null;
        }

        private Events? Handle(CSVCMsg_PacketEntities message)
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
                state.Baseline = message.baseline;
                return Events.BASELINE;
            }
            return null;
        }

        private Events? Handle(CSVCMsg_CreateStringTable message)
        {
            var table = StringTable.Create(message);
            state.StringsIndex[message.name] = state.Strings.Count;
            state.Strings.Add(table);

            stringTableUpdater.Update(table, message.num_entries, message.string_data);

            return null;
        }

        private Events? Handle(CSVCMsg_UpdateStringTable message)
        {
            var table = state.Strings[message.table_id];

            stringTableUpdater.Update(table, message.num_changed_entries, message.string_data);

            return null;
        }

        private Events? Handle(CSVCMsg_Print message)
        {
            return null;
        }

        private Events? Handle(CSVCMsg_GameEventList message)
        {
            /*
            foreach (var descriptor in message.descriptors)
            {
                foreach (var key in descriptor.keys)
                {
                }
            }*/
            return null;
        }
    }
}