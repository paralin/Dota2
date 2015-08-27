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
    ///     A networked string table.
    /// </summary>
    public class StringTable
    {
        [Flags]
        public enum MultiFlag : byte
        {
            None = 0,
            Precache = 1,
            What = 2,
            FixedLength = 8
        }

        private readonly Dictionary<string, int> dict;
        private readonly List<Entry> entries;

        private StringTable(string name, uint max_entries, bool user_data_fixed_size,
            uint user_data_size, uint user_data_size_bits, MultiFlag flags)
        {
            Name = name;
            MaxEntries = max_entries;
            UserDataFixedSize = user_data_fixed_size;
            UserDataSize = user_data_size;
            UserDataSizeBits = user_data_size_bits;
            Flags = flags;

            EntryBits = MiscMath.Log2(max_entries);
            Count = 0;

            dict = new Dictionary<string, int>();
            entries = new List<Entry>();
        }

        public byte EntryBits { get; private set; }
        public uint MaxEntries { get; private set; }
        public string Name { get; private set; }
        public bool UserDataFixedSize { get; private set; }
        public uint UserDataSize { get; private set; }
        public uint UserDataSizeBits { get; private set; }
        public MultiFlag Flags { get; private set; }
        public uint Count { get; private set; }

        public static StringTable Create(CSVCMsg_CreateStringTable proto)
        {
            var flags = MultiFlag.None;

            if ((proto.flags & (uint) MultiFlag.Precache) > 0)
            {
                flags |= MultiFlag.Precache;
            }

            if ((proto.flags & (uint) MultiFlag.What) > 0)
            {
                flags |= MultiFlag.What;
            }

            if ((proto.flags & (uint) MultiFlag.FixedLength) > 0)
            {
                flags |= MultiFlag.FixedLength;
            }

            return new StringTable(proto.name, (uint) proto.max_entries,
                proto.user_data_fixed_size, (uint) proto.user_data_size,
                (uint) proto.user_data_size_bits, flags);
        }

        public Entry Get(string key)
        {
            return entries[dict[key]];
        }

        public Entry Get(uint index)
        {
            return entries[(int) index];
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
            public Entry(string key, byte[] value)
            {
                Key = key;
                Value = value;
            }

            public string Key { get; }
            public byte[] Value { get; set; }
        }
    }
}