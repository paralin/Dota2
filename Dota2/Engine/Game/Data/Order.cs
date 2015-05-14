/*
    This file heavily based off of the nora project.
    See https://github.com/dschleck/nora/blob/master/lara/state/Order.cs
*/

using System;

namespace Dota2.Engine.Game.Data
{
    /// <summary>
    /// An order executable by a player.
    /// </summary>
    public struct Order
    {
        /// <summary>
        /// Build an order to make a mouse click.
        /// </summary>
        /// <param name="selected">Any selected unit IDs</param>
        /// <param name="point">A vector point in the world to select</param>
        /// <returns></returns>
        public static Order MakeMouseClick(UInt16[] selected, Vector point)
        {
            return new Order()
            {
                OrderType = 1,
                SelectedUnits = selected,
                PreparedOrderPoint = point
            };
        }

        public ushort[] SelectedUnits { get; private set; } // 0x48
        public ushort PrimarySelected { get; private set; } // 0x58
        public Int16 OrderType { get; private set; } // 0x5c
        public Int16 EntityIndex1 { get; private set; } // 0x60
        public UInt16 BaseNpcHandle { get; private set; } // 0x64
        public Vector PreparedOrderPoint { get; private set; } // 0x68
        public bool QueueOrder { get; private set; } // 0x74
    }
}