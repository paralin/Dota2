using System;
using System.Collections.Generic;
using Dota2.GameClient.Engine.Game.Data;
using Dota2.GameClient.Utils;

namespace Dota2.GameClient.Engine.Session.Unpackers
{
    internal class StringTableUpdater
    {
        private const int KEY_HISTORY_SIZE = 32;
        private const int MAX_KEY_SIZE = 1024;
        private const int MAX_VALUE_SIZE = 16384;

        public void Update(StringTable table, int numEntries, byte[] data)
        {
            using (var stream = Bitstream.CreateWith(data))
            {
                var option = stream.ReadBool();
                if (option)
                {
                    throw new ArgumentException("Unknown option " + option);
                }

                var keyHistory = new List<string>();

                var entryId = uint.MaxValue;
                uint read = 0;

                while (read < numEntries)
                {
                    if (!stream.ReadBool())
                    {
                        entryId = stream.ReadBits(table.EntryBits);
                    }
                    else
                    {
                        entryId = unchecked(entryId + 1);
                    }

                    var key = ReadKeyIfIncluded(stream, keyHistory);
                    var value = ReadValueIfIncluded(stream, table.UserDataFixedSize,
                        table.UserDataSizeBits);

                    if (entryId < table.Count)
                    {
                        var entry = table.Get(entryId);

                        if (value != null)
                        {
                            entry.Value = value;
                        }
                    }
                    else
                    {
                        table.Put(entryId, new StringTable.Entry(key, value));
                    }

                    ++read;
                }
            }
        }

        private string ReadKeyIfIncluded(Bitstream stream, List<string> keyHistory)
        {
            var has_key = stream.ReadBool();

            if (!has_key)
            {
                return null;
            }

            var is_substring = stream.ReadBool();

            string key;

            if (!is_substring)
            {
                key = stream.ReadString();
            }
            else
            {
                var fromIndex = (int) stream.ReadBits(5);
                var fromLength = (int) stream.ReadBits(5);
                key = keyHistory[fromIndex].Substring(0, fromLength);

                key += stream.ReadString();
            }

            if (keyHistory.Count == KEY_HISTORY_SIZE)
            {
                keyHistory.RemoveAt(0);
            }

            keyHistory.Add(key);

            return key;
        }

        private byte[] ReadValueIfIncluded(Bitstream stream, bool userDataFixedSize,
            uint userDataSizeBits)
        {
            var has_value = stream.ReadBool();

            if (!has_value)
            {
                return null;
            }

            uint length;
            uint bitLength;

            if (userDataFixedSize)
            {
                length = (userDataSizeBits + 7)/8;
                bitLength = userDataSizeBits;
            }
            else
            {
                length = stream.ReadBits(14);
                bitLength = 8*length;
            }

            return stream.ReadManyBits(bitLength);
        }
    }
}