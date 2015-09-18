using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Reflection;

namespace NHExt.Runtime.Model
{
    [Serializable]
    [DataContract]
    public abstract class BaseDTO : IDTO
    {
        private long _id;
        public long ID
        {
            get { return _id; }
            set { _id = value; }
        }

        private int _sysVersion;
        public int SysVersion
        {
            get { return _sysVersion; }
            set { _sysVersion = value; }
        }
        /// <summary>
        /// 获取object类型参数
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public virtual object GetData(string key)
        {
            string[] keyArray = key.Split(new string[] { "." }, StringSplitOptions.None);
            if (keyArray.Length == 1) return this.getData(key);
            BaseDTO baseObj = this.getData(keyArray[0]) as BaseDTO;
            for (int i = 1; i < keyArray.Length - 1; i++)
            {
                if (baseObj == null) return null;
                baseObj = baseObj.GetData(keyArray[i]) as BaseDTO;
            }
            return baseObj.getData(keyArray[keyArray.Length - 1]);
        }
        private object getData(string key)
        {
            Type t = this.GetType();
            PropertyInfo pi = t.GetProperty(key);
            if (pi == null)
            {
                return null;
            }
            object obj = pi.GetValue(this, null);
            return obj;
        }

        public virtual void SetValue(object obj, string memberName)
        {
            switch (memberName)
            {
                case "ID":
                    this._id = this.TransferValue<long>(obj);
                    break;
                case "SysVersion":
                    this._sysVersion = this.TransferValue<int>(obj);
                    break;
                default:
                    Type type = this.GetType();
                    System.Reflection.PropertyInfo pi = type.GetProperty(memberName);
                    if (pi != null)
                    {
                        //调用序列化函数，序列化json对象
                        pi.SetValue(this, obj, null);
                    }
                    break;
            }
        }
        protected T TransferValue<T>(object obj)
        {
            if (obj == null) return default(T);
            return (T)obj;
        }
    }
}
