/*
    A lot of this math based on the Nora project.
*/

namespace Dota2.GameClient.Utils
{
    /// <summary>
    /// Misc math functions.
    /// </summary>
    public static class MiscMath
    {
        /// <summary>
        ///  Log base 2 uint, -> byte
        /// </summary>
        /// <param name="n"></param>
        /// <returns></returns>
        public static byte Log2(uint n)
        {
            byte r = 0;
            uint acc = 1;

            while (acc < n)
            {
                ++r;
                acc *= 2;
            }

            return r;
        }
    }
}
