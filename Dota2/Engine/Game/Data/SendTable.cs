using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dota2.GC.Dota.Internal;

namespace Dota2.Engine.Game.Data
{
    /// <summary>
    /// Networked send property table.
    /// </summary>
    public class SendTable
    {
        public static SendTable CreateWith(CSVCMsg_SendTable proto)
        {
            var table = new SendTable()
            {
                NetTableName = proto.net_table_name,
                NeedsDecoder = proto.needs_decoder,
            };

            foreach (var prop in proto.props)
            {
                table.Properties.Add(PropertyInfo.CreateWith(prop, table));
            }

            return table;
        }

        public string NetTableName { get; private set; }
        public bool NeedsDecoder { get; private set; }
        public List<PropertyInfo> Properties { get; private set; }

        private SendTable()
        {
            this.Properties = new List<PropertyInfo>();
        }
    }
}
