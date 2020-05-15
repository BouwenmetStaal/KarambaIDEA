using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace KarambaIDEA.Core
{
    public static class Extensions
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
        public static T Clone<T>(this T source)
        {
            if (!typeof(T).IsSerializable)
            {
                throw new ArgumentException("The type must be serializable.", nameof(source));
            }
            // Don't serialize a null object, simply return the default for that object
            if (Object.ReferenceEquals(source, null))
            {
                return default(T);
            }
            IFormatter formatter = new BinaryFormatter();
            Stream stream = new MemoryStream();
            using (stream)
            {
                formatter.Serialize(stream, source);
                stream.Seek(0, SeekOrigin.Begin);
                return (T)formatter.Deserialize(stream);
            }
        }
    }
}
