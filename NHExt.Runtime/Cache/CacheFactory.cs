using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NHExt.Runtime.Cache
{
    public static class CacheFactory
    {
        private static object locker = new object();
        public static NHExt.Runtime.Cache.AbstractCache<string, List<NHExt.Runtime.EntityAttribute.ColumnDescriptionAttribute>> GetEntityColumnCache()
        {
            lock (locker)
            {
                NHExt.Runtime.Cache.AbstractCache<string, List<NHExt.Runtime.EntityAttribute.ColumnDescriptionAttribute>> cache = AbstractCache<string, List<NHExt.Runtime.EntityAttribute.ColumnDescriptionAttribute>>.GetInstance("NHExt.Runtime.Cache.EntityColumnCache");
                if (cache == null)
                {
                    cache = new Instance.EntityColumnCache();
                }
                return cache;
            }
        }

        public static NHExt.Runtime.Cache.AbstractCache<string, object> GetRuntimeCache()
        {
            lock (locker)
            {
                AbstractCache<string, object> cache = AbstractCache<string, object>.GetInstance("NHExt.Runtime.Cache.RuntimeCache");
                if (cache == null)
                {
                    cache = new RuntimeCache();
                }
                return cache;
            }
        }


    }
}
