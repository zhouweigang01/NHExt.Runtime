using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NHExt.Runtime.Model
{
    public class EntityKey<T> where T : BaseEntity
    {
        private long _id;
        public EntityKey(long? id)
        {
            this._id = id ?? -1;
        }
        public long ID
        {
            get { return _id; }
        }
        public T ToEntity()
        {
            if (this._id <= 0) return null;
            List<object> paramList = new List<object>();
            paramList.Add(this._id);
            T t = NHExt.Runtime.Query.EntityFinder.Find<T>("ID=${0}$", paramList);
            return t;
        }

        public override bool Equals(object obj)
        {
            if (object.ReferenceEquals(obj, null))
            {
                return false;
            }
            Type t = obj.GetType();
            System.Reflection.PropertyInfo pi = t.GetProperty("ID");
            if (pi == null)
            {
                throw new NHExt.Runtime.Exceptions.RuntimeException("当前对象不是BE实体Key");
            }
            long id = (long)pi.GetValue(obj, null);
            if (this.ID == id) return true;
            return false;

        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override string ToString()
        {
            return this._id.ToString();
        }
        public static implicit operator long(EntityKey<T> key) {
            return key.ID;
        }
    }
}
