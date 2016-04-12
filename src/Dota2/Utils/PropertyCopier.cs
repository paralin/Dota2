using System.ComponentModel;

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
            foreach (PropertyDescriptor item in TypeDescriptor.GetProperties(src))
                item.SetValue(dest, item.GetValue(src));
        }
    }
}
