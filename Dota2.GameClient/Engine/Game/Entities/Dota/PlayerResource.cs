using Dota2.Engine.Game.Data;

namespace Dota2.Engine.Game.Entities.Dota
{
    /// <summary>
    /// Player names and IDs
    /// </summary>
    public class PlayerResource : MappedEntityClass
    {
        public const string TableName = "CDOTA_PlayerResource";

        public RangeValue<string> Names { get; private set; }
        public RangeValue<ulong> SteamIds { get; private set; }

        internal PlayerResource(uint id, DotaGameState state) : base(id, state)
        {
            Names = range<string>("m_iszPlayerNames", 32);
            SteamIds = range<ulong>("m_iPlayerSteamIDs", 32);
        }
    }
}
