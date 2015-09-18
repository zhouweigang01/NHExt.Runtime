using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NHExt.Runtime.GAIA.Cache
{
    public static class CacheFactory
    {
        private static object locker = new object();
        public static NHExt.Runtime.Cache.AbstractCache<string, string> GetDataAuthCache()
        {
            lock (locker)
            {
                NHExt.Runtime.Cache.AbstractCache<string, string> cache = NHExt.Runtime.Cache.AbstractCache<string, string>.GetInstance("NHExt.Runtime.GAIA.Cache.DataAuthCache"); ;
                if (cache == null)
                {
                    cache = new DataAuthCache();
                    cache.Size = 1000;
                }
                return cache;
            }
        }



        public static NHExt.Runtime.Cache.AbstractCache<string, EntityAttribute.BussinesAttribute> GetBACache()
        {
            lock (locker)
            {
                NHExt.Runtime.Cache.AbstractCache<string, EntityAttribute.BussinesAttribute> cache = NHExt.Runtime.Cache.AbstractCache<string, EntityAttribute.BussinesAttribute>.GetInstance("NHExt.Runtime.GAIA.Cache.MultiOrgCache"); ;
                if (cache == null)
                {
                    cache = new BACache();
                }
                return cache;
            }
        }
    }
}
