/*
    This file heavily based off of the nora project.
    See https://github.com/dschleck/nora/blob/master/lara/state/Order.cs
*/

namespace Dota2.GameClient.Engine.Game.Data
{
    /// <summary>
    ///     An order executable by a player.
    /// </summary>
    public struct Order
    {
        public ushort[] SelectedUnits { get; private set; } // 0x48
        public ushort PrimarySelected { get; private set; } // 0x58
        public short OrderType { get; private set; } // 0x5c
        public short EntityIndex1 { get; private set; } // 0x60
        public ushort BaseNpcHandle { get; private set; } // 0x64
        public Vector PreparedOrderPoint { get; private set; } // 0x68
        public bool QueueOrder { get; private set; } // 0x74

        /// <summary>
        ///     Build an order to make a mouse click.
        /// </summary>
        /// <param name="selected">Any selected unit IDs</param>
        /// <param name="point">A vector point in the world to select</param>
        /// <returns></returns>
        public static Order MakeMouseClick(ushort[] selected, Vector point)
        {
            return new Order
            {
                OrderType = 1,
                SelectedUnits = selected,
                PreparedOrderPoint = point
            };
        }
    }
}