using System.Collections.Generic;

/*
    This file heavily based off of the nora project.
    See https://github.com/dschleck/nora/blob/master/lara/state/Entity.cs
*/

namespace Dota2.Engine.Game.Data
{
    /// <summary>
    /// A DOTA 2 game entity.
    /// </summary>
    public class Entity
    {
        public static Entity CreateWith(uint id, EntityClass clazz, FlatTable table)
        {
            var entity = new Entity(id, clazz);

            foreach (var info in table.Properties)
            {
                entity.Properties.Add(Property.For(info));
            }

            return entity;
        }

        public uint Id { get; private set; }
        public EntityClass Class { get; private set; }
        public List<Property> Properties { get; private set; }

        private Entity(uint id, EntityClass clazz)
        {
            this.Id = id;
            this.Class = clazz;
            this.Properties = new List<Property>();
        }

        public Entity Copy()
        {
            Entity copy = new Entity(Id, Class);

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