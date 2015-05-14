using System.Collections.Generic;
using System.Reflection;

/*
    This file heavily based off of the nora project.
    See https://github.com/dschleck/nora/blob/master/lara/state/FlatTable.cs
*/

namespace Dota2.Engine.Game.Data
{
    /// <summary>
    /// A flat network table.
    /// </summary>
    public class FlatTable
    {
        public static FlatTable CreateWith(
            string name,
            bool needsDecoder,
            List<PropertyInfo> properties)
        {
            return new FlatTable
            {
                NetTableName = name,
                NeedsDecoder = needsDecoder,
                Properties = properties
            };
        }

        public string NetTableName { get; private set; }
        public bool NeedsDecoder { get; private set; }
        public List<PropertyInfo> Properties { get; private set; }
    }
}