using Dota2.GC.Dota.Internal;

namespace Dota2.Base.Data
{
    /// <summary>
    /// Parsed extra data about a lobby.
    /// </summary>
    public class LobbyExtraData
    {
        /// <summary>
        /// List of league admins in this lobby.
        /// </summary>
        public CMsgLeagueAdminList LeagueAdminList { get; protected internal set; }
    }
}
