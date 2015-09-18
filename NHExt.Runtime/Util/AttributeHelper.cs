using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace NHExt.Runtime.Util
{
    public static class AttributeHelper
    {
        public static T GetClassAttr<T>(Type classType) where T : Attribute
        {
            foreach (Attribute attr in classType.GetCustomAttributes(true))
            {
                if (attr is T)
                {
                    return attr as T;
                }
            }
            return null;
        }
        public static List<T> GetPropertyByAttrList<T>(Type classType) where T : Attribute
        {
            List<T> attrList = new List<T>();
            PropertyInfo[] piArray = classType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            if (piArray == null) return null;
            foreach (PropertyInfo pi in piArray)
            {
                object[] attrArray = pi.GetCustomAttributes(typeof(T), true);
                if (attrArray != null && attrArray.Length > 0)
                {
                    foreach (T attr in attrArray)
                    {
                        if (attr is T)
                        {
                            attrList.Add(attr);
                        }
                    }
                }
            }
            return attrList;
        }
        public static List<PropertyInfo> GetPropertyByAttr<T>(Type classType, T compareAttr, Comparison<T> compare) where T : Attribute
        {
            List<PropertyInfo> piList = new List<PropertyInfo>();
            PropertyInfo[] piArray = classType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            if (piArray == null) return null;
            foreach (PropertyInfo pi in piArray)
            {
                object[] attrArray = pi.GetCustomAttributes(typeof(T), true);
                if (attrArray != null && attrArray.Length > 0)
                {
                    foreach (T attr in attrArray)
                    {
                        if (compare(compareAttr, attr) == 0)
                        {
                            piList.Add(pi);
                        }
                    }
                }
            }
            return piList;
        }
    }
}
