using Dota2.GameClient.Engine.Game.Data;

namespace Dota2.GameClient.Engine.Game.Entities.Dota
{
    /// <summary>
    /// 
    /// </summary>
    class Ability : MappedEntityClass
    {
        /// <summary>
        /// Table name for this mapping.
        /// </summary>
        public const string TableName = "CDOTABaseAbility";

        /// <summary>
        /// Ability level
        /// </summary>
        public TypedValue<uint> Level;

        /// <summary>
        /// The game time the ability comes off cooldown
        /// </summary>
        public TypedValue<float> Cooldown;

        /// <summary>
        /// Time of the cooldown if it were cast in this tick.
        /// </summary>
        public TypedValue<float> CooldownLength;

        /// <summary>
        /// Mana cost, 0 if not learned.
        /// </summary>
        public TypedValue<uint> ManaCost;

        /// <summary>
        /// Cast range.
        /// </summary>
        public TypedValue<uint> CastRange;

        public Ability(uint id, DotaGameState state) : base(id, state)
        {
            const string t = "DT_DOTABaseAbility";

            Level = bind<uint>(t, "m_iLevel");
            Cooldown = bind<float>(t, "m_fCooldown");
            CooldownLength = bind<float>(t, "m_flCooldownLength");
            ManaCost = bind<uint>(t, "m_iManaCost");
            CastRange = bind<uint>(t, "m_iCastRange");
        }
    }
}
