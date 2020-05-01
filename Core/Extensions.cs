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
        public static object Deepclone(this object objSource)

        {
            //Get the type of source object and create a new instance of that type
            Type typeSource = objSource.GetType();
            object objTarget = Activator.CreateInstance(typeSource);
            //Get all the properties of source object type
            PropertyInfo[] propertyInfo = typeSource.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            //Assign all source property to taget object 's properties
            foreach (PropertyInfo property in propertyInfo)
            {
                //Check whether property can be written to
                if (property.CanWrite)
                {
                    //check whether property type is value type, enum or string type
                    if (property.PropertyType.IsValueType || property.PropertyType.IsEnum || property.PropertyType.Equals(typeof(System.String)))
                    {
                        property.SetValue(objTarget, property.GetValue(objSource, null), null);
                    }
                    //else property type is object/complex types, so need to recursively call this method until the end of the tree is reached
                    else
                    {
                        object objPropertyValue = property.GetValue(objSource, null);
                        if (objPropertyValue == null)
                        {
                            property.SetValue(objTarget, null, null);
                        }
                        else
                        {

                            property.SetValue(objTarget, objPropertyValue.Deepclone(), null);
                        }
                    }
                }
            }
            return objTarget;
        }
    }
}
