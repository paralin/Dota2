/*
    This file heavily based off of the nora project.
    See https://github.com/dschleck/nora/blob/master/lara/state/StringTable.cs
*/

using System;
using System.Collections.Generic;
using Dota2.GC.Dota.Internal;
using Dota2.Utils;

namespace Dota2.Engine.Game.Data
{
    /// <summary>
    /// A networked string table.
    /// </summary>
    public class StringTable
    {
        public static StringTable Create(CSVCMsg_CreateStringTable proto)
        {
            MultiFlag flags = MultiFlag.None;

            if ((proto.flags & (uint)MultiFlag.Precache) > 0)
            {
                flags |= MultiFlag.Precache;
            }

            if ((proto.flags & (uint)MultiFlag.What) > 0)
            {
                flags |= MultiFlag.What;
            }

            if ((proto.flags & (uint)MultiFlag.FixedLength) > 0)
            {
                flags |= MultiFlag.FixedLength;
            }

            return new StringTable(proto.name, (uint)proto.max_entries,
                proto.user_data_fixed_size, (uint)proto.user_data_size,
                (uint)proto.user_data_size_bits, flags);
        }

        public byte EntryBits { get; private set; }
        public uint MaxEntries { get; private set; }
        public string Name { get; private set; }
        public bool UserDataFixedSize { get; private set; }
        public uint UserDataSize { get; private set; }
        public uint UserDataSizeBits { get; private set; }
        public MultiFlag Flags { get; private set; }

        public uint Count { get; private set; }

        private Dictionary<string, int> dict;
        private List<Entry> entries;

        private StringTable(string name, uint max_entries, bool user_data_fixed_size,
                uint user_data_size, uint user_data_size_bits, MultiFlag flags)
        {
            this.Name = name;
            this.MaxEntries = max_entries;
            this.UserDataFixedSize = user_data_fixed_size;
            this.UserDataSize = user_data_size;
            this.UserDataSizeBits = user_data_size_bits;
            this.Flags = flags;

            this.EntryBits = MiscMath.Log2(max_entries);
            this.Count = 0;

            this.dict = new Dictionary<string, int>();
            this.entries = new List<Entry>();
        }

        public Entry Get(string key)
        {
            return entries[dict[key]];
        }

        public Entry Get(uint index)
        {
            return entries[(int)index];
        }

        public void Put(uint index, Entry entry)
        {
            if (dict.ContainsKey(entry.Key))
            {
                entries[dict[entry.Key]] = entry;
            }
            else
            {
                Count = Math.Max(index + 1, Count);
                dict[entry.Key] = entries.Count;
                entries.Add(entry);
            }
        }

        public class Entry
        {

            public string Key { get; private set; }
            public byte[] Value { get; set; }

            public Entry(string key, byte[] value)
            {
                this.Key = key;
                this.Value = value;
            }
        }

        [FlagsAttribute]
        public enum MultiFlag : byte
        {
            None = 0,
            Precache = 1,
            What = 2,
            FixedLength = 8,
        }
    }
}
}
