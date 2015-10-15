using Dota2.GameClient.Engine.Game.Data;

namespace Dota2.GameClient.Engine.Game.Entities.Dota
{
    /// <summary>
    /// A DOTA 2 player.
    /// </summary>
    public class Player : MappedEntityClass
    {
        public const string TableName = "CDOTAPlayer";

        /// <summary>
        /// The DOTA 2 player ID.
        /// </summary>
        public TypedValue<uint> PlayerId;

        /// <summary>
        /// The hero.
        /// </summary>
        public Handle Hero;
        
        public Player(uint id, DotaGameState state) : base(id, state)
        {
            const string t = "DT_DOTAPlayer";

            PlayerId = bind<uint>(t, "m_iPlayerID");
            Hero = handle(t, "m_hAssignedHero");
        }
    }
}
