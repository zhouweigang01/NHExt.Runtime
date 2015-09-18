using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NHExt.Runtime.Extend.Cache
{
    class MultiOrgCache : NHExt.Runtime.Cache.AbstractCache<string, EntityAttribute.BussinesAttribute>
    {
        public MultiOrgCache()
            : base("NHExt.Runtime.Cache.MultiOrgCache")
        {
        }
        public override string JoinKey(string key)
        {
            return key;
        }

        public override void ClearAll()
        {
            Dictionary<string, EntityAttribute.BussinesAttribute> dic = this.getTypeCache();
            dic.Clear();
        }
    }
}
