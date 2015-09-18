using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NHExt.Runtime.Extend.Cache
{
    public static class AbstractCacheFactory
    {
        private static NHExt.Runtime.Cache.ICache<string, string> _dataAuthCache = null;
        public static NHExt.Runtime.Cache.AbstractCache<string, string> GetDataAuthCache()
        {
            if (_dataAuthCache == null)
            {
                _dataAuthCache = new DataAuthCache();
            }
            return (NHExt.Runtime.Cache.AbstractCache<string, string>)_dataAuthCache;
        }
        private static NHExt.Runtime.Cache.ICache<string, List<NHExt.Runtime.EntityAttribute.ColumnDescriptionAttribute>> _entityColumnCache = null;
        public static NHExt.Runtime.Cache.AbstractCache<string, List<NHExt.Runtime.EntityAttribute.ColumnDescriptionAttribute>> GetEntityColumnCache()
        {
            if (_entityColumnCache == null)
            {
                _entityColumnCache = new EntityColumnCache();
            }
            return (NHExt.Runtime.Cache.AbstractCache<string, List<NHExt.Runtime.EntityAttribute.ColumnDescriptionAttribute>>)_entityColumnCache;
        }

        private static NHExt.Runtime.Cache.ICache<string, EntityAttribute.BussinesAttribute> _multiOrgCache = null;

        public static NHExt.Runtime.Cache.AbstractCache<string, EntityAttribute.BussinesAttribute> GetMultiOrgCache()
        {
            if (_multiOrgCache == null)
            {
                _multiOrgCache = new MultiOrgCache();
            }
            return (NHExt.Runtime.Cache.AbstractCache<string, EntityAttribute.BussinesAttribute>)_multiOrgCache;
        }
    }
}
