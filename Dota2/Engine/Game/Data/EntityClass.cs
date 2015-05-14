/*
    This file heavily based off of the nora project.
    See https://github.com/dschleck/nora/blob/master/lara/state/EntityClass.cs
*/

using Dota2.GC.Dota.Internal;

namespace Dota2.Engine.Game.Data
{
    /// <summary>
    /// An entity class.
    /// </summary>
    public struct EntityClass
    {
        /// <summary>
        /// Parse a ClassInfo proto.
        /// </summary>
        /// <param name="proto">Class info proto</param>
        /// <returns>EntityClass instance</returns>
        public static EntityClass CreateWith(CSVCMsg_ClassInfo.class_t proto)
        {
            return new EntityClass()
            {
                Id = (uint) proto.class_id,
                DataTableName = proto.data_table_name,
                ClassName = proto.class_name,
            };
        }

        public uint Id { get; private set; }
        public string DataTableName { get; private set; }
        public string ClassName { get; private set; }

        public override bool Equals(object obj)
        {
            if (!(obj is EntityClass))
            {
                return false;
            }

            var o = (EntityClass) obj;
            return o.Id == Id;
        }
    }
}