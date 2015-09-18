using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NHExt.Runtime.Cache
{
    /// <summary>
    /// 运行时缓存
    /// </summary>
    class RuntimeCache : NHExt.Runtime.Cache.AbstractCache<string, object>
    {
        public RuntimeCache()
            : base("NHExt.Runtime.Cache.RuntimeCache")
        {
        }
        public RuntimeCache(string typeKey) : base(typeKey) { }
        public override string JoinKey(string key)
        {
            return key;
        }
    }
}
