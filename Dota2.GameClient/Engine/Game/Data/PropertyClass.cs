using System;
using System.Collections.Generic;

namespace Dota2.Engine.Game.Data
{
    /// <summary>
    /// A handler class for an entity.
    /// </summary>
    public class MappedEntityClass
    {
        public readonly uint Id;
        private readonly DotaGameState state;

        protected MappedEntityClass(uint id, DotaGameState state)
        {
            this.Id = id;
            this.state = state;
        }

        /// <summary>
        /// Bind an enum value.
        /// </summary>
        /// <typeparam name="T">Enum type</typeparam>
        /// <param name="table">Table name</param>
        /// <param name="name">Property name</param>
        /// <returns></returns>
        protected EnumValue<T> bindE<T>(string table, string name) where T : IConvertible
        {
            return new EnumValue<T>(state.Properties[new DotaGameState.PropertyHandle()
            {
                Entity = Id,
                Table = table,
                Name = name,
            }]);
        }

        /// <summary>
        /// Bind a type value.
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="table">Table name</param>
        /// <param name="name">Property name</param>
        /// <returns></returns>
        protected TypedValue<T> bind<T>(string table, string name)
        {
            return new TypedValue<T>(state.Properties[new DotaGameState.PropertyHandle()
            {
                Entity = Id,
                Table = table,
                Name = name,
            }]);
        }

        /// <summary>
        /// Bind a handle to a property.
        /// </summary>
        /// <param name="table">Table name</param>
        /// <param name="name">Property name</param>
        /// <returns></returns>
        protected Handle handle(string table, string name)
        {
            return new Handle(
                state.Properties[new DotaGameState.PropertyHandle()
                {
                    Entity = Id,
                    Table = table,
                    Name = name,
                }],
                state);
        }

        /// <summary>
        /// Bind a range to a property.
        /// </summary>
        /// <typeparam name="T">Range type</typeparam>
        /// <param name="name">Table name</param>
        /// <param name="count">Range index</param>
        /// <returns></returns>
        protected RangeValue<T> range<T>(string name, int count)
        {
            List<Property> properties = new List<Property>();
            for (int i = 0; i < count; ++i)
            {
                properties.Add(state.Properties[new DotaGameState.PropertyHandle()
                {
                    Entity = Id,
                    Table = name,
                    Name = i.ToString("D4"),
                }]);
            }
            return new RangeValue<T>(properties);
        }
    }
}
