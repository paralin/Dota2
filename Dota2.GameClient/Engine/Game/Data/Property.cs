/*
    This file heavily based off of the nora project.
    See https://github.com/dschleck/nora/blob/master/lara/state/Property.cs
*/

using System;
using System.Collections.Generic;
using Dota2.GameClient.Engine.Session.Unpackers;
using Dota2.GameClient.Utils;

namespace Dota2.GameClient.Engine.Game.Data
{
    /// <summary>
    ///     A networked entity property.
    /// </summary>
    public abstract class Property
    {
        public readonly PropertyInfo Info;

        protected Property(PropertyInfo info)
        {
            Info = info;
        }

        public uint UpdatedAt { get; private set; }

        public static Property For(PropertyInfo info)
        {
            switch (info.Type)
            {
                case PropertyInfo.PropertyType.Int:
                    return new IntProperty(info);
                case PropertyInfo.PropertyType.Float:
                    return new FloatProperty(info);
                case PropertyInfo.PropertyType.Vector:
                    return new VectorProperty(info);
                case PropertyInfo.PropertyType.VectorXy:
                    return new VectorXyProperty(info);
                case PropertyInfo.PropertyType.String:
                    return new StringProperty(info);
                case PropertyInfo.PropertyType.Array:
                    return new ArrayProperty(info);
                case PropertyInfo.PropertyType.Int64:
                    return new Int64Property(info);
                default:
                    throw new InvalidOperationException();
            }
        }

        public abstract Property Copy();
        protected abstract void Unpack(uint tick, PropertyValueUnpacker unpacker, Bitstream stream);

        public void Update(uint tick, PropertyValueUnpacker unpacker, Bitstream stream)
        {
            UpdatedAt = tick;
            Unpack(tick, unpacker, stream);
        }

        public T ValueAs<T>()
        {
            return ((TypedProperty<T>) this).Value;
        }

        public abstract class TypedProperty<T> : Property
        {
            protected TypedProperty(PropertyInfo info) : base(info)
            {
            }

            public T Value { get; protected set; }

            public override string ToString()
            {
                return Value.ToString();
            }
        }

        public class IntProperty : TypedProperty<uint>
        {
            public IntProperty(PropertyInfo info) : base(info)
            {
            }

            public override Property Copy()
            {
                var copy = new IntProperty(Info);
                copy.Value = Value;
                return copy;
            }

            protected override void Unpack(uint tick, PropertyValueUnpacker unpacker, Bitstream stream)
            {
                Value = unpacker.UnpackInt(Info, stream);
            }
        }

        public class FloatProperty : TypedProperty<float>
        {
            public FloatProperty(PropertyInfo info) : base(info)
            {
            }

            public override Property Copy()
            {
                var copy = new FloatProperty(Info);
                copy.Value = Value;
                return copy;
            }

            protected override void Unpack(uint tick, PropertyValueUnpacker unpacker, Bitstream stream)
            {
                Value = unpacker.UnpackFloat(Info, stream);
            }
        }

        public class VectorProperty : TypedProperty<Vector>
        {
            public VectorProperty(PropertyInfo info) : base(info)
            {
            }

            public override Property Copy()
            {
                var copy = new VectorProperty(Info);
                copy.Value = Value;
                return copy;
            }

            protected override void Unpack(uint tick, PropertyValueUnpacker unpacker, Bitstream stream)
            {
                Value = unpacker.UnpackVector(Info, stream);
            }
        }

        public class VectorXyProperty : TypedProperty<VectorXy>
        {
            public VectorXyProperty(PropertyInfo info) : base(info)
            {
            }

            public override Property Copy()
            {
                var copy = new VectorXyProperty(Info);
                copy.Value = Value;
                return copy;
            }

            protected override void Unpack(uint tick, PropertyValueUnpacker unpacker, Bitstream stream)
            {
                Value = unpacker.UnpackVectorXy(Info, stream);
            }
        }

        public class StringProperty : TypedProperty<string>
        {
            public StringProperty(PropertyInfo info) : base(info)
            {
            }

            public override Property Copy()
            {
                var copy = new StringProperty(Info);
                copy.Value = Value;
                return copy;
            }

            protected override void Unpack(uint tick, PropertyValueUnpacker unpacker, Bitstream stream)
            {
                Value = unpacker.UnpackString(Info, stream);
            }
        }

        public class ArrayProperty : TypedProperty<List<Property>>
        {
            public ArrayProperty(PropertyInfo info) : base(info)
            {
                Value = new List<Property>();
            }

            public override Property Copy()
            {
                var copy = new ArrayProperty(Info);

                foreach (var item in Value)
                {
                    copy.Value.Add(item.Copy());
                }

                return copy;
            }

            protected override void Unpack(uint tick, PropertyValueUnpacker unpacker, Bitstream stream)
            {
                unpacker.UnpackArray(tick, Value, Info, stream);
            }
        }

        public class Int64Property : TypedProperty<ulong>
        {
            public Int64Property(PropertyInfo info) : base(info)
            {
            }

            public override Property Copy()
            {
                var copy = new Int64Property(Info);
                copy.Value = Value;
                return copy;
            }

            protected override void Unpack(uint tick, PropertyValueUnpacker unpacker, Bitstream stream)
            {
                Value = unpacker.UnpackInt64(Info, stream);
            }
        }
    }
}