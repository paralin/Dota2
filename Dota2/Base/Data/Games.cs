using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dota2.Base.Data
{
    /// <summary>
    /// Potential game IDs usable by DOTA GC handler.
    /// </summary>
    public enum Games : uint
    {
        /// <summary>
        /// Main DOTA 2 client.
        /// </summary>
        DOTA2 = 570,

        /// <summary>
        /// DOTA 2 test.
        /// </summary>
        DOTA2TEST = 205790
    }
}