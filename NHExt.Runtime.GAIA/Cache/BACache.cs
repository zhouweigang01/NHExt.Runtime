using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NHExt.Runtime.GAIA.Cache
{
    class BACache : NHExt.Runtime.Cache.AbstractCache<string, EntityAttribute.BussinesAttribute>
    {
        public BACache() : base("NHExt.Runtime.GAIA.Cache.BACache") { }

        public override string JoinKey(string key)
        {
            return key;
        }
    }
}
