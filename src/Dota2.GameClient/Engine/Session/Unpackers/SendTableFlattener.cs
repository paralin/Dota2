/*
    This code heavily based on the Nora project.
    See https://github.com/dschleck/nora/blob/master/lara/Bitstream.cs
*/

using System.Collections.Generic;
using System.Linq;
using Dota2.GameClient.Engine.Game.Data;

namespace Dota2.GameClient.Engine.Session.Unpackers
{
    /// <summary>
    ///     Flattens network SendTables
    /// </summary>
    public class SendTableFlattener
    {
        public List<FlatTable> Flatten(List<SendTable> sendTables)
        {
            var flattened = new List<FlatTable>();

            var nameMap =
                sendTables.ToDictionary(t => t.NetTableName, t => t);

            foreach (var table in sendTables)
            {
                var excluding = new HashSet<string>();
                GatherExcludes(table, nameMap, excluding);

                var properties = new List<PropertyInfo>();
                BuildHierarchy(properties, table, nameMap, excluding);
                SortProperties(properties);

                flattened.Add(
                    FlatTable.CreateWith(table.NetTableName, table.NeedsDecoder, properties));
            }

            return flattened;
        }

        private void GatherExcludes(
            SendTable table,
            Dictionary<string, SendTable> all,
            HashSet<string> excluding)
        {
            foreach (var property in table.Properties)
            {
                if (property.Flags.HasFlag(PropertyInfo.MultiFlag.Exclude))
                {
                    excluding.Add(QualifyProperty(property.DtName, property));
                }
                else if (property.Type == PropertyInfo.PropertyType.DataTable)
                {
                    GatherExcludes(all[property.DtName], all, excluding);
                }
            }
        }

        private void BuildHierarchy(
            List<PropertyInfo> properties,
            SendTable send,
            Dictionary<string, SendTable> all,
            HashSet<string> excluding)
        {
            var nonDtProps = new List<PropertyInfo>();
            GatherProperties(properties, send, all, nonDtProps, excluding);

            properties.AddRange(nonDtProps);
        }

        private void GatherProperties(
            List<PropertyInfo> properties,
            SendTable send,
            Dictionary<string, SendTable> all,
            List<PropertyInfo> nonDtProps,
            HashSet<string> excluding)
        {
            var skipOn = PropertyInfo.MultiFlag.Exclude | PropertyInfo.MultiFlag.InsideArray;

            foreach (var property in send.Properties)
            {
                if ((uint) (property.Flags & skipOn) > 0)
                {
                    continue;
                }
                if (excluding.Contains(QualifyProperty(property.Origin.NetTableName, property)))
                {
                    continue;
                }

                if (property.Type == PropertyInfo.PropertyType.DataTable)
                {
                    var pointsAt = all[property.DtName];

                    if (property.Flags.HasFlag(PropertyInfo.MultiFlag.Collapsible))
                    {
                        GatherProperties(properties, pointsAt, all, nonDtProps, excluding);
                    }
                    else
                    {
                        BuildHierarchy(properties, pointsAt, all, excluding);
                    }
                }
                else
                {
                    nonDtProps.Add(property);
                }
            }
        }

        private void SortProperties(List<PropertyInfo> properties)
        {
            var priorities = new List<uint>();
            priorities.Add(64);

            foreach (var property in properties)
            {
                if (!priorities.Contains(property.Priority))
                {
                    priorities.Add(property.Priority);
                }
            }

            priorities.Sort();

            var offset = 0;
            foreach (var priority in priorities)
            {
                var hole = offset;
                var cursor = hole;

                while (cursor < properties.Count)
                {
                    var prop = properties[cursor];

                    var cutLine = priority == 64 &&
                                  prop.Flags.HasFlag(PropertyInfo.MultiFlag.ChangesOften);
                    if (prop.Priority == priority || cutLine)
                    {
                        properties[cursor] = properties[hole];
                        properties[hole] = prop;

                        ++hole;
                        ++offset;
                    }

                    ++cursor;
                }
            }
        }

        private string QualifyProperty(string table, PropertyInfo property)
        {
            return string.Format("{0}.{1}", table, property.VarName);
        }
    }
}