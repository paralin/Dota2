 /*
    This file heavily based off of the nora project.
    See https://github.com/dschleck/nora/blob/master/lara/state/UserCmd.cs
*/

namespace Dota2.Engine.Game.Data
{
    /// <summary>
    ///     A command made by the user. Includes viewport state, etc, presumably for replays.
    /// </summary>
    public class UserCmd
    {
        public UserCmd()
        {
            ViewAngles = new Vector();
            HeadAngles = new Vector();
            HeadOffset = new Vector();
            CrosshairTrace = new Vector();
        }

        public uint Command { get; set; } // 0x04
        public uint Tick { get; set; } // 0x08
        public Vector ViewAngles { get; set; } // 0x0c
        public float ForwardMove { get; set; } // 0c18
        public float SideMove { get; set; } // 0x1c
        public float UpMove { get; set; } // 0x20
        public uint Buttons { get; set; } // 0x24
        public byte Impulse { get; set; } // 028
        public ushort CursorUnitEntIndex { get; set; } // 0x2c
        public ushort QueryEntIndex { get; set; } // 0x30
        public uint RandomSeed { get; set; } // 0x34
        public ushort MouseDx { get; set; } // 0x38
        public ushort MouseDy { get; set; } // 0x3a
        public ushort Offset3c { get; set; } // 0x3c
        public short OrderId { get; set; } // 0x44
        public Order? Order { get; set; }
        public ushort CameraX { get; set; } // 0x78
        public ushort CameraY { get; set; } // 0x7a
        public byte ClickBehavior { get; set; } // 0x7c
        public byte SpectatorStatsSomething { get; set; } // 0x7d
        public byte ShopModeSomething { get; set; } // 0x7e
        public byte HudStatsDropdownCategoryIndex { get; set; } // 0x7f
        public byte HudStatsDropdownSortMethod { get; set; } // 0x80
        public Vector HeadAngles { get; set; } // 0x84
        public Vector HeadOffset { get; set; } // 0x90
        public Vector CrosshairTrace { get; set; } // 0x9c
    }
}