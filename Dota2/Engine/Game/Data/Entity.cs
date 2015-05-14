using System.Collections.Generic;

/*
    This file heavily based off of the nora project.
    See https://github.com/dschleck/nora/blob/master/lara/state/Entity.cs
*/

namespace Dota2.Engine.Game.Data
{
    /// <summary>
    ///     A DOTA 2 game entity.
    /// </summary>
    public class Entity
    {
        private Entity(uint id, EntityClass clazz)
        {
            Id = id;
            Class = clazz;
            Properties = new List<Property>();
        }

        public uint Id { get; }
        public EntityClass Class { get; }
        public List<Property> Properties { get; }

        public static Entity CreateWith(uint id, EntityClass clazz, FlatTable table)
        {
            var entity = new Entity(id, clazz);

            foreach (var info in table.Properties)
            {
                entity.Properties.Add(Property.For(info));
            }

            return entity;
        }

        public Entity Copy()
        {
            var copy = new Entity(Id, Class);

            for (var i = 0; i < Properties.Count; ++i)
            {
                if (Properties[i] != null)
                {
                    copy.Properties.Add(Properties[i].Copy());
                }
                else
                {
                    copy.Properties.Add(null);
                }
            }

            return copy;
        }
    }
}