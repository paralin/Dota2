using System.Collections.Generic;
using System.Linq;
using Dota2.GameClient.Engine.Data;
using Dota2.GameClient.Engine.Game.Data;
using Dota2.GameClient.Engine.Game.Entities;
using Dota2.GC.Dota.Internal;

/*
    This file heavily based off of the nora project.
    See https://github.com/dschleck/nora/blob/master/lara/state/Client.cs
*/

namespace Dota2.GameClient.Engine.Game
{
    /// <summary>
    ///     Simulates the data stored in a DOTA 2 client.
    /// </summary>
    public class DotaGameState
    {
        /// <summary>
        ///     Connect details
        /// </summary>
        private readonly DOTAConnectDetails _details;

        /// <summary>
        ///     Instantiates a new game state.
        /// </summary>
        /// <param name="details">Details</param>
        /// <param name="pool">Entity pool</param>
        internal DotaGameState(DOTAConnectDetails details)
        {
            _details = details;

            CVars = new Dictionary<string, string>();
            Strings = new List<StringTable>();
            StringsIndex = new Dictionary<string, int>();
            Classes = new List<EntityClass>();
            ClassesByName = new Dictionary<string, EntityClass>();
            SendTables = new List<SendTable>();
            FlatTables = new List<FlatTable>();
            Properties = new Dictionary<PropertyHandle, Property>();
            Slots = new Dictionary<uint, Slot>();

            Created = new List<uint>();
            Deleted = new List<uint>();

            ChatMessages = new List<CUserMsg_SayText2>();
            GameEvents = new List<CSVCMsg_GameEvent>();
            ChatEvents = new List<CDOTAUserMsg_ChatEvent>();

            Reset();
        }

        public int Baseline { get; set; }
        public uint ServerCount { get; set; }
        public float TickInterval { get; set; }
        public uint ClientTick { get; set; }
        public uint ServerTick { get; set; }
        public Dictionary<string, string> CVars { get; }
        public List<StringTable> Strings { get; }
        public Dictionary<string, int> StringsIndex { get; }
        public List<EntityClass> Classes { get; }
        public Dictionary<string, EntityClass> ClassesByName { get; }
        public List<SendTable> SendTables { get; }
        public List<FlatTable> FlatTables { get; }
        public Dictionary<PropertyHandle, Property> Properties { get; }
        public Dictionary<uint, Slot> Slots { get; }

        public List<CUserMsg_SayText2> ChatMessages { get; } 
        public List<CDOTAUserMsg_ChatEvent> ChatEvents { get; } 
        public List<CSVCMsg_GameEvent> GameEvents { get; } 

        /// <summary>
        /// Parsed and updated entities. Will be null until connected.
        /// </summary>
        public DotaEntityPool EntityPool { get; internal set; }

        public List<uint> Created { get; }
        public List<uint> Deleted { get; }

        /// <summary>
        ///     Reset the local data.
        /// </summary>
        public void Reset()
        {
            CVars.Clear();
            CVars.Add("tv_nochat", "0");
            CVars.Add("joy_autoaimdampen", "0");
            CVars.Add("name", _details.Name);
            CVars.Add("cl_interp_ratio", "2");
            CVars.Add("tv_listen_voice_indices", "0");
            CVars.Add("cl_predict", "0");
            CVars.Add("cl_updaterate", "30");
            CVars.Add("cl_showhelp", "1");
            CVars.Add("steamworks_sessionid_lifetime_client", "0");
            CVars.Add("cl_mouselook", "1");
            CVars.Add("steamworks_sessionid_client", "0");
            CVars.Add("dota_mute_cobroadcasters", "0");
            CVars.Add("voice_loopback", "0");
            CVars.Add("dota_player_initial_skill", "0");
            CVars.Add("cl_lagcompensation", "1");
            CVars.Add("closecaption", "0");
            CVars.Add("cl_language", "english");
            CVars.Add("english", "1");
            CVars.Add("cl_class", "default");
            CVars.Add("snd_voipvolume", "1");
            CVars.Add("snd_musicvolume", "1");
            CVars.Add("cl_cmdrate", "30");
            CVars.Add("net_maxroutable", "1200");
            CVars.Add("cl_team", "default");
            CVars.Add("rate", "80000");
            CVars.Add("cl_predictweapons", "1");
            CVars.Add("cl_interpolate", "1");
            CVars.Add("cl_interp", "0.05");
            CVars.Add("dota_camera_edgemove", "1");
            CVars.Add("snd_gamevolume", "1");
            CVars.Add("cl_spec_mode", "1");

            Classes.Clear();
            ClassesByName.Clear();
            SendTables.Clear();
            FlatTables.Clear();
            Properties.Clear();
            Slots.Clear();
            Strings.Clear();
            StringsIndex.Clear();

            Created.Clear();
            Deleted.Clear();
        }

        /// <summary>
        /// Called every tick.
        /// </summary>
        internal void Update()
        {
            EntityPool.Update();
        }

        internal CMsg_CVars ExposeCVars()
        {
            var exposed = new CMsg_CVars();

            exposed.cvars.AddRange(CVars.Select(kv =>
            {
                var var = new CMsg_CVars.CVar();
                var.name = kv.Key;
                var.value = kv.Value;
                return var;
            }));

            return exposed;
        }

        public struct PropertyHandle
        {
            public uint Entity { get; set; }
            public string Table { get; set; }
            public string Name { get; set; }

            public override bool Equals(object o)
            {
                if (!(o is PropertyHandle))
                {
                    return false;
                }

                var handle = (PropertyHandle) o;
                return Entity == handle.Entity &&
                       Table.Equals(handle.Table) &&
                       Name.Equals(handle.Name);
            }

            public override int GetHashCode()
            {
                var result = (int) Entity;
                result = 31*result + Table.GetHashCode();
                result = 31*result + Name.GetHashCode();
                return result;
            }
        }

        public class Slot
        {
            public Slot(Entity entity)
            {
                Entity = entity;
                Live = true;
                Baselines = new Entity[2];
            }

            public Entity Entity { get; set; }
            public bool Live { get; set; }
            public Entity[] Baselines { get; set; }
        }
    }
}