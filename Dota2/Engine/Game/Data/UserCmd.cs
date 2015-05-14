using System;

/*
    This file heavily based off of the nora project.
    See https://github.com/dschleck/nora/blob/master/lara/state/UserCmd.cs
*/

namespace Dota2.Engine.Game.Data
{
    /// <summary>
    /// A command made by the user. Includes viewport state, etc, presumably for replays.
    /// </summary>
    public class UserCmd
    {
        public UInt32 Command { get; set; }  // 0x04
        public UInt32 Tick { get; set; } // 0x08

        public Vector ViewAngles { get; set; } // 0x0c
        public float ForwardMove { get; set; } // 0c18
        public float SideMove { get; set; } // 0x1c
        public float UpMove { get; set; } // 0x20
        public UInt32 Buttons { get; set; } // 0x24
        public byte Impulse { get; set; } // 028

        public UInt16 CursorUnitEntIndex { get; set; } // 0x2c
        public UInt16 QueryEntIndex { get; set; } // 0x30

        public UInt32 RandomSeed { get; set; } // 0x34
        public UInt16 MouseDx { get; set; } // 0x38
        public UInt16 MouseDy { get; set; } // 0x3a

        public UInt16 Offset3c { get; set; } // 0x3c
        public Int16 OrderId { get; set; } // 0x44
        public Order? Order { get; set; }
        public UInt16 CameraX { get; set; } // 0x78
        public UInt16 CameraY { get; set; } // 0x7a
        public byte ClickBehavior { get; set; } // 0x7c
        public byte SpectatorStatsSomething { get; set; } // 0x7d
        public byte ShopModeSomething { get; set; } // 0x7e
        public byte HudStatsDropdownCategoryIndex { get; set; } // 0x7f
        public byte HudStatsDropdownSortMethod { get; set; } // 0x80

        public Vector HeadAngles { get; set; } // 0x84
        public Vector HeadOffset { get; set; } // 0x90

        public Vector CrosshairTrace { get; set; } // 0x9c

        public UserCmd()
        {
            ViewAngles = new Vector();
            HeadAngles = new Vector();
            HeadOffset = new Vector();
            CrosshairTrace = new Vector();
        }
    }
}
