using System;
using System.Collections.Generic;
using System.Linq;
using Dota2.Engine.Game.Data;
using SteamKit2.Internal;

namespace Dota2.Engine.Game.Entities
{
    /// <summary>
    /// Pool of entities currently in the game session.
    /// </summary>
    public class DotaEntityPool
    {
        private DotaGameState _state;
        private Dictionary<string, Constructor> _pendingMappings;
        private Dictionary<EntityClass, Constructor> _constructors = null;
        private HashSet<uint> MissedCreated = new HashSet<uint>(); 

        public readonly Dictionary<uint, MappedEntityClass> Entities;
        public readonly Dictionary<Type, HashSet<uint>> EntitiesByType; 

        private DotaEntityPool(DotaGameState state, Dictionary<string, Constructor> constructors)
        {
            _state = state;
            _pendingMappings = constructors;
            Entities = new Dictionary<uint, MappedEntityClass>();
            EntitiesByType = new Dictionary<Type, HashSet<uint>>();

            foreach (var con in constructors.Values)
                EntitiesByType[con.type] = new HashSet<uint>();
        }

        /// <summary>
        /// Check if the entity pool has a type.
        /// </summary>
        /// <typeparam name="T">Type to check</typeparam>
        /// <returns></returns>
        public bool Has<T>()
        {
            return EntitiesByType[typeof (T)].Count > 0;
        }

        /// <summary>
        /// Get an entity by ID.
        /// </summary>
        /// <typeparam name="T">Entity mapped type</typeparam>
        /// <param name="id">ID</param>
        /// <returns></returns>
        public T Get<T>(uint id) where T : MappedEntityClass
        {
            return (T) Entities[id];
        }

        /// <summary>
        /// Get a single entity by type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T GetSingle<T>() where T : MappedEntityClass
        {
            return (T) Entities[EntitiesByType[typeof (T)].Single()];
        }

        /// <summary>
        /// Called every tick.
        /// </summary>
        internal void Update()
        {
            if (_constructors == null)
            {
                foreach (var c in _state.Created) MissedCreated.Add(c);
                foreach (var c in _state.Deleted) MissedCreated.Remove(c);

                if (_state.ClassesByName.Keys.Count <= 10) return;
                _constructors = new Dictionary<EntityClass, Constructor>();
                foreach (var c in _pendingMappings)
                {
                    EntityClass cl;
                    if (_state.ClassesByName.TryGetValue(c.Key, out cl))
                        _constructors[cl] = c.Value;
                }
            }
            else
            {
                foreach (var del in _state.Deleted.Where(del => Entities.ContainsKey(del)))
                {
                    EntitiesByType[Entities[del].GetType()].Remove(del);
                    Entities.Remove(del);
                }
                foreach (var cre in _state.Created.Union(MissedCreated))
                {
                    var ent = _state.Slots[cre].Entity;
                    Constructor con;
                    if (!_constructors.TryGetValue(ent.Class, out con)) continue;
                    Entities[cre] = con.factory(cre, _state);
                    EntitiesByType[con.type].Add(cre);
                }
                MissedCreated.Clear();
            }
        }

        private struct Constructor
        {
            public Type type;
            public Func<uint, DotaGameState, MappedEntityClass> factory;
        }

        /// <summary>
        /// Builder to prepare entity mappings before usage.
        /// </summary>
        internal class Builder
        {
            private readonly Dictionary<string, Constructor> _constructors;

            internal Builder()
            {
                _constructors = new Dictionary<string, Constructor>();
            }

            /// <summary>
            /// Finalize the entity mappings.
            /// </summary>
            /// <returns></returns>
            public DotaEntityPool Build(DotaGameState state)
            {
                return new DotaEntityPool(state, _constructors);
            }

            /// <summary>
            /// Associate a mapped entity class with a classname
            /// </summary>
            /// <typeparam name="T">Mapped class</typeparam>
            /// <param name="cname">Class name, e.g. CDOTAPlayer</param>
            /// <param name="factory">Factory to build the class given an entity and an id</param>
            /// <returns></returns>
            public Builder Associate<T>(string cname, Func<uint, DotaGameState, MappedEntityClass> factory)
            {
                _constructors[cname] = new Constructor() {factory = factory, type = typeof(T)};
                return this;
            }
        }

    }
}
