using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dota2.Engine.Data
{
    /// <summary>
    /// Details used to connect to the game server.
    /// </summary>
    internal class DOTAConnectDetails
    {
        /// <summary>
        /// IP Address or Steam3 ID.
        /// </summary>
        public string ConnectInfo { get; set; }
        
        /// <summary>
        /// Auth ticket used to connect.
        /// </summary>
        public byte[] AuthTicket { get; set; }

        /// <summary>
        /// Auth ticket hash.
        /// </summary>
        public uint AuthTicketCRC { get; set; }

        /// <summary>
        /// Current connect attempt number.
        /// </summary>
        public uint ConnectID { get; set; }

        /// <summary>
        /// Steam ID converted to UInt64
        /// </summary>
        public ulong SteamId { get; set; }

        /// <summary>
        /// Profile name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Steamworks session ID
        /// </summary>
        public ulong SteamworksSessionId { get; set; }

        /// <summary>
        /// Pass key
        /// </summary>
        public string PassKey { get; set; }
    }
}
