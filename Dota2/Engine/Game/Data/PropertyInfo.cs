/*
    This file heavily based off of the nora project.
    See https://github.com/dschleck/nora/blob/master/lara/state/PropertyInfo.cs
*/

using System;
using Dota2.GC.Dota.Internal;

namespace Dota2.Engine.Game.Data
{
    /// <summary>
    /// Information about a networked property.
    /// </summary>
    public class PropertyInfo
    {
        public static PropertyInfo CreateWith(CSVCMsg_SendTable.sendprop_t proto, SendTable origin)
        {
            return new PropertyInfo()
            {
                Type = (PropertyType) proto.type,
                VarName = proto.var_name,
                Flags = (MultiFlag) proto.flags,
                Priority = (uint) proto.priority,
                DtName = proto.dt_name,
                NumElements = (uint) proto.num_elements,
                LowValue = proto.low_value,
                HighValue = proto.high_value,
                NumBits = (byte) proto.num_bits,
                Origin = origin,
            };
        }

        public PropertyType Type { get; private set; }
        public string VarName { get; private set; }
        public MultiFlag Flags { get; private set; }
        public uint Priority { get; private set; }
        public string DtName { get; private set; }
        public uint NumElements { get; private set; }
        public float LowValue { get; private set; }
        public float HighValue { get; private set; }
        public byte NumBits { get; private set; }

        public PropertyInfo ArrayProp { get; set; }
        public SendTable Origin { get; private set; }

        public enum PropertyType
        {
            Int = 0,
            Float = 1,
            Vector = 2,
            VectorXy = 3,
            String = 4,
            Array = 5,
            DataTable = 6,
            Int64 = 7,
        }

        [Flags]
        public enum MultiFlag
        {
            Unsigned = 1 << 0,
            Coord = 1 << 1,
            NoScale = 1 << 2,
            RoundDown = 1 << 3,
            RoundUp = 1 << 4,
            Normal = 1 << 5,
            Exclude = 1 << 6,
            Xyze = 1 << 7,
            InsideArray = 1 << 8,
            Collapsible = 1 << 11,
            CoordMp = 1 << 12,
            CoordMpLowPrecision = 1 << 13,
            CoordMpIntegral = 1 << 14,
            CellCoord = 1 << 15,
            CellCoordLowPrecision = 1 << 16,
            CellCoordIntegral = 1 << 17,
            ChangesOften = 1 << 18,
            EncodedAgainstTickcount = 1 << 19,
        }

        private PropertyInfo()
        {
        }
    }
}