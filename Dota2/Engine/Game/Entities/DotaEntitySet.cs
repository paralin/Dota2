using System;
using Dota2.Engine.Game.Data;
using Dota2.Engine.Game.Entities.Dota;

namespace Dota2.Engine.Game.Entities
{
    //todo: implement the rest of the known entities: https://github.com/skadistats/skadi/wiki/DT_DOTA_BaseNPC

    /// <summary>
    /// Built in entity set
    /// </summary>
    internal static class DotaEntitySet
    {
        /// <summary>
        /// Register all default/supported handlers.
        /// </summary>
        /// <param name="b">The builder.</param>
        internal static DotaEntityPool.Builder Associate(DotaEntityPool.Builder b)
        {
            b.Associate<Ability>(Ability.TableName, (i, c) => new Ability(i, c));
            b.Associate<GameRules>(GameRules.TableName, (i, c) => new GameRules(i, c));
            b.Associate<Player>(Player.TableName, (i, c) => new Player(i, c));
            b.Associate<PlayerResource>(PlayerResource.TableName, (i, c) => new PlayerResource(i, c));
            return b;
        }
    }
}
