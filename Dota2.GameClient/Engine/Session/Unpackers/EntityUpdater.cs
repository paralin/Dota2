/*
    This file heavily based off of the nora project.
    See https://github.com/dschleck/nora/blob/master/lara/unpackers/EntityUpdater.cs
*/

using System;
using System.Collections.Generic;
using Dota2.Engine.Game;
using Dota2.Engine.Game.Data;
using Dota2.Utils;

namespace Dota2.Engine.Session.Unpackers
{
    /// <summary>
    ///     Unpacks incoming Entity updates.
    /// </summary>
    internal class EntityUpdater
    {
        private readonly DotaGameState state;
        private readonly PropertyValueUnpacker unpacker;

        public EntityUpdater(DotaGameState state)
        {
            this.state = state;
            unpacker = new PropertyValueUnpacker();
        }

        private byte ClassBitLength
        {
            get { return MiscMath.Log2((uint) state.Classes.Count); }
        }

        public void Update(Bitstream stream, uint baseline, bool updateBaseline, uint updated, bool isDelta)
        {
            var id = uint.MaxValue;
            uint found = 0;

            while (found < updated)
            {
                var flags = ReadHeader(ref id, stream);

                if (flags.HasFlag(UpdateFlag.EnterPvs))
                {
                    ReadEnterPvs(id, baseline, updateBaseline, stream);
                }
                else if (flags.HasFlag(UpdateFlag.LeavePvs))
                {
                    if (flags.HasFlag(UpdateFlag.Delete))
                    {
                        Delete(id);
                    }
                }
                else
                {
                    ReadUpdate(id, stream);
                }

                ++found;
            }

            if (isDelta)
            {
                while (stream.ReadBool())
                {
                    id = stream.ReadBits(11);
                    Delete(id);
                }
            }
        }

        private UpdateFlag ReadHeader(ref uint id, Bitstream stream)
        {
            var value = stream.ReadBits(6);

            if ((value & 0x30) > 0)
            {
                var a = (value >> 4) & 3;
                var b = (uint) ((a == 3) ? 16 : 0);

                value = (stream.ReadBits((byte) (4*a + b)) << 4) | (value & 0xF);
            }

            id = unchecked(id + value + 1);

            var flags = UpdateFlag.None;

            if (!stream.ReadBool())
            {
                if (stream.ReadBool())
                {
                    flags |= UpdateFlag.EnterPvs;
                }
            }
            else
            {
                flags |= UpdateFlag.LeavePvs;

                if (stream.ReadBool())
                {
                    flags |= UpdateFlag.Delete;
                }
            }

            return flags;
        }

        private void Delete(uint id)
        {
            state.Deleted.Add(id);
            if (!state.Slots.ContainsKey(id))
            {
                return;
            }

            var clazz = (int) state.Slots[id].Entity.Class.Id;
            foreach (var info in state.FlatTables[clazz].Properties)
            {
                state.Properties.Remove(new DotaGameState.PropertyHandle
                {
                    Entity = id,
                    Table = info.Origin.NetTableName,
                    Name = info.VarName
                });
            }
            state.Slots[id].Live = false;
        }

        private void ReadEnterPvs(uint id, uint baseline, bool update_baseline, Bitstream stream)
        {
            var clazz = state.Classes[(int) stream.ReadBits(ClassBitLength)];
            var serial = stream.ReadBits(10);

            Create(id, clazz, baseline);
            ReadAndUnpackFields(state.Slots[id].Entity, stream);

            if (update_baseline)
            {
                state.Slots[id].Baselines[1 - baseline] = state.Slots[id].Entity.Copy();
            }
        }

        private void ReadUpdate(uint id, Bitstream stream)
        {
            var entity = state.Slots[id].Entity;
            ReadAndUnpackFields(entity, stream);
        }

        private void Create(uint id, EntityClass clazz, uint baseline)
        {
            state.Created.Add(id);
            if (!state.Slots.ContainsKey(id))
            {
                state.Slots[id] = new DotaGameState.Slot(null);
            }

            var slot = state.Slots[id];
            slot.Live = true;
            if (slot.Baselines[baseline] != null && slot.Baselines[baseline].Class.Equals(clazz))
            {
                slot.Entity = slot.Baselines[baseline].Copy();
            }
            else
            {
                slot.Entity = Entity.CreateWith(id, clazz, state.FlatTables[(int) clazz.Id]);

                var table = state.Strings[state.StringsIndex["instancebaseline"]];
                using (var stream = Bitstream.CreateWith(table.Get(clazz.Id.ToString()).Value))
                {
                    ReadAndUnpackFields(slot.Entity, stream);
                }
            }

            foreach (var prop in slot.Entity.Properties)
            {
                var info = prop.Info;
                var handle = new DotaGameState.PropertyHandle
                {
                    Entity = id,
                    Table = info.Origin.NetTableName,
                    Name = info.VarName
                };
                state.Properties[handle] = prop;
            }

        }

        private void ReadAndUnpackFields(Entity entity, Bitstream stream)
        {
            var fields = ReadFieldList(stream);

            foreach (var field in fields)
            {
                entity.Properties[(int) field].Update(state.ClientTick, unpacker, stream);
            }
        }

        private List<uint> ReadFieldList(Bitstream stream)
        {
            var fields = new List<uint>();

            var field = uint.MaxValue;
            field = ReadFieldNumber(field, stream);

            while (field != uint.MaxValue)
            {
                fields.Add(field);

                field = ReadFieldNumber(field, stream);
            }

            return fields;
        }

        private uint ReadFieldNumber(uint lastField, Bitstream stream)
        {
            if (stream.ReadBool())
            {
                return unchecked(lastField + 1);
            }
            var value = stream.ReadVarUInt();

            if (value == 0x3FFF)
            {
                return uint.MaxValue;
            }
            return unchecked(lastField + value + 1);
        }

        [Flags]
        private enum UpdateFlag
        {
            None = 0,
            LeavePvs = 1 << 0,
            Delete = 1 << 1,
            EnterPvs = 1 << 2
        }
    }
}