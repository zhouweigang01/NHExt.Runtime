using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using NHExt.Runtime.EntityAttribute;

namespace NHExt.Runtime.Model
{

    public class BaseEnumDTO : IEnumDTO
    {
        private int _enumValue;
        public int EnumValue
        {
            get { return _enumValue; }
            set { _enumValue = value; }
        }

        private string _typeString;
        protected string TypeString
        {
            get { return _typeString; }
            set { _typeString = value; }
        }

        public BaseEnumDTO() { }
        protected BaseEnumDTO(int v, string typeStr)
        {
            this._enumValue = v;
            this._typeString = typeStr;
        }
        protected BaseEnumDTO(string typeStr)
        {
            this._enumValue = -1;
            this._typeString = typeStr;
        }

        public static BaseEnumDTO Empty
        {
            get
            {
                BaseEnumDTO enumObj = new BaseEnumDTO("NHExt.Runtime.Model.BaseEnumDTO");
                return enumObj;
            }
        }
        public static T GetEnum<T>(int enumValue) where T : BaseEnumDTO
        {
            T enumObj = null;
            List<PropertyInfo> piList = BaseEnumDTO.GetPropertyInfoArray<T>();
            foreach (PropertyInfo pi in piList)
            {
                T obj = pi.GetValue(null, null) as T;
                if (obj.EnumValue == enumValue)
                {
                    enumObj = obj;
                    break;
                }
            }
            return enumObj;
        }
        private static List<PropertyInfo> GetPropertyInfoArray<T>()
        {
            List<PropertyInfo> piList = new List<PropertyInfo>();
            Type t = typeof(T);
            PropertyInfo[] piArray = t.GetProperties();
            foreach (PropertyInfo pi in piArray)
            {
                foreach (Attribute attr in pi.GetCustomAttributes(true))
                {
                    if (attr is EnumPropertyAttribute)
                    {
                        piList.Add(pi);
                    }
                }
            }
            return piList;
        }
        public string GetEnumName()
        {
            PropertyInfo[] piArray = this.GetType().GetProperties();
            foreach (PropertyInfo pi in piArray)
            {
                object[] attrArray = pi.GetCustomAttributes(true);
                bool equal = false;
                foreach (Attribute attr in attrArray)
                {
                    if (attr is EnumPropertyAttribute)
                    {
                        BaseEnumDTO enumDTO = pi.GetValue(null, null) as BaseEnumDTO;
                        if (enumDTO != null && enumDTO == this)
                        {
                            equal = true;
                            break;
                        }
                    }
                }
                if (equal)
                {
                    foreach (Attribute attr in attrArray)
                    {
                        if (attr is NHExt.Runtime.EntityAttribute.ColumnDescriptionAttribute)
                        {
                            return (attr as NHExt.Runtime.EntityAttribute.ColumnDescriptionAttribute).Description;
                        }
                    }
                }

            }
            return null;
        }
        public override bool Equals(object obj)
        {
            if (object.ReferenceEquals(obj, null)) return false;
            BaseEnumDTO enumObj = obj as BaseEnumDTO;
            if (object.ReferenceEquals(enumObj, null)) return false;
            if (this.TypeString != enumObj.TypeString) return false;
            if (this.EnumValue == enumObj.EnumValue) return true;
            return false;
        }
        public static bool operator !=(BaseEnumDTO a, BaseEnumDTO b)
        {
            return !(a == b);
        }

        public static bool operator ==(BaseEnumDTO a, BaseEnumDTO b)
        {
            if (!object.ReferenceEquals(a, null))
            {
                return a.Equals(b);
            }
            else if (!object.ReferenceEquals(b, null))
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}

