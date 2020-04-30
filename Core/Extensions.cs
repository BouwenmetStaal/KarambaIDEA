using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace KarambaIDEA.Core
{
    internal static class Extensions
    {
        /// <summary>
        /// Copy all properties accesible from Source To this
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="target">this object</param>
        /// <param name="source">the source object</param>
        public static void CopyPropertiesFromSource<T>(this T target, T source) where T : class
        {
            var props = target.GetType().GetRuntimeProperties();
            var propsa = target.GetType().GetProperties();
            foreach (PropertyInfo pinfo in target.GetType().GetRuntimeProperties())
            {
                if (pinfo.CanWrite == true)
                {
                    pinfo.SetValue(target, pinfo.GetValue(source));
                }
            }
        }
    }
}
