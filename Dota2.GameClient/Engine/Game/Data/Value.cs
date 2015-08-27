using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dota2.Engine.Game.Data
{
    /// <summary>
    /// An entity property value
    /// </summary>
    public interface IValue
    {
        uint? ReadAt { get; }
    }

    /// <summary>
    /// Generic entity handle property
    /// </summary>
    public class Handle : IValue
    {
        private readonly Property property;
        private readonly DotaGameState state;
        private ushort value;

        public uint? ReadAt { get; private set; }
        public ushort? Value
        {
            get
            {
                if (!ReadAt.HasValue || ReadAt.Value < property.UpdatedAt)
                {
                    ReadAt = property.UpdatedAt;
                    value = (ushort)(property.ValueAs<uint>() & 0x7FF);
                }
                if (state.Slots.ContainsKey(value))
                {
                    var slot = state.Slots[value];
                    if (slot.Live)
                    {
                        return value;
                    }
                }

                return null;
            }
        }

        public Handle(Property property, DotaGameState state)
        {
            this.property = property;
            this.state = state;
            this.value = 0;
            this.ReadAt = null;
        }
    }

    /// <summary>
    /// An array type entity property
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class RangeValue<T> : IValue
    {
        private readonly List<Property> properties;
        private uint?[] readAt;
        private T[] values;

        public uint? ReadAt { get; private set; }
        public T[] Value
        {
            get
            {
                uint latest = 0;
                for (int i = 0; i < properties.Count; ++i)
                {
                    var property = properties[i];
                    latest = Math.Max(latest, property.UpdatedAt);
                    if (!readAt[i].HasValue || readAt[i].Value < property.UpdatedAt)
                    {
                        readAt[i] = property.UpdatedAt;
                        values[i] = property.ValueAs<T>();
                    }
                }
                ReadAt = latest;
                return values;
            }
        }

        public RangeValue(List<Property> properties)
        {
            this.properties = properties;
            this.readAt = new uint?[properties.Count];
            this.values = new T[properties.Count];
        }
    }

    /// <summary>
    /// An enumeration property value
    /// </summary>
    /// <typeparam name="T">Enumeration</typeparam>
    public class EnumValue<T> : TypedValue<uint> where T : IConvertible
    {
        public new T Value
        {
            get
            {
                return (T)Enum.ToObject(typeof(T), base.Value);
            }
        }

        public EnumValue(Property property) : base(property)
        {
        }
    }

    /// <summary>
    /// A typed entity property value
    /// </summary>
    /// <typeparam name="T">Property type</typeparam>
    public class TypedValue<T> : IValue
    {
        private readonly Property property;
        private T value;

        public uint? ReadAt { get; private set; }
        public T Value
        {
            get
            {
                if (!ReadAt.HasValue || ReadAt.Value < property.UpdatedAt)
                {
                    ReadAt = property.UpdatedAt;
                    value = property.ValueAs<T>();
                }
                return value;
            }
        }

        public TypedValue(Property property)
        {
            this.property = property;
            this.value = default(T);
            this.ReadAt = null;
        }
    }
}
