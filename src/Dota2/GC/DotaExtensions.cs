using System.IO;
using Dota2.Base.Data;
using Dota2.GC.Dota.Internal;
using ProtoBuf;

namespace Dota2.GC
{
    /// <summary>
    /// General Linq extensions for Dota2.
    /// </summary>
    public static class DotaExtensions
    {
        /// <summary>
        /// Parse the extra data on a lobby.
        /// </summary>
        /// <param name="lobby">Lobby containing extra messages.</param>
        /// <returns></returns>
        public static LobbyExtraData ParseExtraMessages(this CSODOTALobby lobby)
        {
            var extra = new LobbyExtraData();
            foreach (var msg in lobby.extra_messages)
            {
                switch (msg.id)
                {
                    case (uint) EDOTAGCMsg.k_EMsgGCLeagueAdminList:
                        extra.LeagueAdminList = msg.contents.DeserializeProtobuf<CMsgLeagueAdminList>();
                        break;
                }
            }
            return extra;
        }

        /// <summary>
        /// Deserializes a serialized protobuf, convenience function.
        /// </summary>
        /// <param name="data">Binary data array.</param>
        /// <typeparam name="T">The protobuf type to deserialize.</typeparam>
        /// <returns></returns>
        public static T DeserializeProtobuf<T>(this byte[] data)
        {
            using (var ms = new MemoryStream(data))
            {
                return Serializer.Deserialize<T>(ms);
            }
        }
    }
}