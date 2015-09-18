using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using NHExt.Runtime.EntityAttribute;
using NHibernate.UserTypes;
using NHibernate.SqlTypes;
using NHibernate;

namespace NHExt.Runtime.Model
{
    public class BaseEnum : IEnum, IUserType, ICloneable
    {
        private string _code;
        public string Code
        {
            get { return _code; }
            set { _code = value; }
        }

        private string _name;
        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        private int _enumValue;
        public int EnumValue
        {
            get { return _enumValue; }
            set { _enumValue = value; }
        }

        private Type _enumType;
        private Type EnumType
        {
            get
            {
                return _enumType;
            }
        }

        protected BaseEnum(int v, string code, string name, Type type)
        {
            this._enumValue = v;
            this._code = code;
            this._name = name;
            this._enumType = type;
        }
        protected BaseEnum(Type type)
        {
            this._enumValue = -1;
            this._code = string.Empty;
            this._name = string.Empty;
            this._enumType = type;
        }
        public BaseEnum GetEnum(int enumValue)
        {
            BaseEnum enumObj = null;
            List<PropertyInfo> piList = this.GetPropertyInfoArray();
            foreach (PropertyInfo pi in piList)
            {
                BaseEnum obj = pi.GetValue(null, null) as BaseEnum;
                if (obj.EnumValue == enumValue)
                {
                    enumObj = obj;
                    break;
                }
            }
            return enumObj;
        }
        public BaseEnum GetEnum(string enumCode)
        {
            BaseEnum enumObj = null;
            List<PropertyInfo> piList = this.GetPropertyInfoArray();
            foreach (PropertyInfo pi in piList)
            {
                BaseEnum obj = pi.GetValue(null, null) as BaseEnum;
                if (obj.Code == enumCode)
                {
                    enumObj = obj;
                    break;
                }
            }
            return enumObj;
        }

        public List<BaseEnum> GetEnumList()
        {
            List<BaseEnum> enumList = new List<BaseEnum>();
            List<PropertyInfo> piList = this.GetPropertyInfoArray();
            foreach (PropertyInfo pi in piList)
            {
                BaseEnum obj = pi.GetValue(null, null) as BaseEnum;
                if (obj != null)
                {
                    enumList.Add(obj);
                }
            }
            return enumList;
        }

        private List<PropertyInfo> GetPropertyInfoArray()
        {
            List<PropertyInfo> piList = new List<PropertyInfo>();

            PropertyInfo[] piArray = _enumType.GetProperties();
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

        #region 运算符重载
        public override bool Equals(object obj)
        {
            if (object.ReferenceEquals(obj, null)) return false;
            BaseEnum enumObj = obj as BaseEnum;
            if (object.ReferenceEquals(enumObj, null)) return false;
            if (this.EnumType != enumObj.EnumType) return false;
            if (this.EnumValue == enumObj.EnumValue) return true;
            return false;
        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
        public static bool operator !=(BaseEnum a, BaseEnum b)
        {
            return !(a == b);
        }
        public static bool operator ==(BaseEnum a, BaseEnum b)
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
        #endregion

        #region 基类静态成员函数

        public static T GetEnum<T>(int enumValue) where T : BaseEnum
        {
            Type type = typeof(T);
            BaseEnum enumObj = Activator.CreateInstance(type) as BaseEnum;
            enumObj = enumObj.GetEnum(enumValue);
            return enumObj as T;
        }
        public static T GetEnum<T>(string enumCode) where T : BaseEnum
        {
            Type type = typeof(T);
            BaseEnum enumObj = Activator.CreateInstance(type) as BaseEnum;
            enumObj = enumObj.GetEnum(enumCode);
            return enumObj as T;
        }

        public static List<T> GetEnumList<T>() where T : BaseEnum
        {
            Type type = typeof(T);
            BaseEnum enumObj = Activator.CreateInstance(type) as BaseEnum;
            List<BaseEnum> enumObjList = enumObj.GetEnumList();
            List<T> enumList = new List<T>();
            foreach (BaseEnum obj in enumObjList)
            {
                enumList.Add(obj as T);
            }
            return enumList;

        }

        #endregion

        #region IUserType 接口成员
        public object Assemble(object cached, object owner)
        {
            return DeepCopy(cached);
        }

        public object DeepCopy(object value)
        {
            if (value == null)
            {
                return null;
            }
            BaseEnum srcObj = value as BaseEnum;
            if (srcObj != null)
            {
                BaseEnum enumObj = Activator.CreateInstance(_enumType) as BaseEnum;
                enumObj.Code = srcObj.Code;
                enumObj.Name = srcObj.Name;
                enumObj.EnumValue = srcObj.EnumValue;
                return enumObj;
            }
            return null;
        }

        public object Disassemble(object value)
        {
            return DeepCopy(value);
        }

        public new bool Equals(object x, object y)
        {
            if (!object.ReferenceEquals(x, null))
            {
                return x.Equals(y);
            }
            else if (object.ReferenceEquals(y, null))
            {
                return true;
            }
            return false;
        }

        public int GetHashCode(object x)
        {
            return x.ToString().GetHashCode();
        }

        public bool IsMutable
        {
            get { return false; }
        }

        public object NullSafeGet(System.Data.IDataReader rs, string[] names, object owner)
        {
            int? value = (int?)NHibernateUtil.Int32.NullSafeGet(rs, names[0]);
            if (value > 0)
            {
                BaseEnum enumObj = Activator.CreateInstance(_enumType) as BaseEnum;
                enumObj = enumObj.GetEnum(value ?? -1);
                return enumObj;
            }
            else
            {
                return null;
            }
        }

        public void NullSafeSet(System.Data.IDbCommand cmd, object value, int index)
        {
            if (value != null)
            {
                int? enumValue = null;
                if (value != null)
                {
                    if (value is BaseEnum)
                    {
                        enumValue = (value as BaseEnum).EnumValue;
                    }
                    else
                    {
                        enumValue = Convert.ToInt32(value);
                    }
                }

                NHibernateUtil.Int32.NullSafeSet(cmd, enumValue, index);
            }
            else
            {
                NHibernateUtil.Int32.NullSafeSet(cmd, null, index);
            }
        }

        public object Replace(object original, object target, object owner)
        {
            return original;
        }

        public Type ReturnedType
        {
            get { return _enumType; }
        }

        public SqlType[] SqlTypes
        {
            get { return new SqlType[] { NHibernateUtil.Int32.SqlType }; }
        }

        #endregion

        #region 公共函数

        public virtual object Clone()
        {
            return Util.CloneHelper.Clone<BaseEnum>(this);
        }

        #endregion

        #region 重写函数

        public override string ToString()
        {
            return this._enumValue.ToString();
        }

        #endregion
    }
}
