using System.Reflection;

namespace Dota2.Utils
{
    internal static class PropertyCopier
    {
        /// <summary>
        /// Copy property values from one object to another.
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="dest">Destination</param>
        /// <param name="src">Src</param>
        public static void CopyProperties<T>(T dest, T src)
        {
            foreach (var item in typeof(T).GetProperties())
                item.SetValue(dest, item.GetValue(src));
        }
    }
}
