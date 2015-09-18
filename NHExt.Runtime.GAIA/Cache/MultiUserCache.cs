using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NHExt.Runtime.GAIA.Cache
{
    /// <summary>
    /// 多组织多用户缓存基类
    /// </summary>
    public abstract class MultiUserCache : NHExt.Runtime.Cache.AbstractCache<string, string>
    {
        public MultiUserCache(string typeKey) : base(typeKey) { }
        public MultiUserCache() : base("NHExt.Runtime.GAIA.Cache.MultiUserCache") { }
        public override string JoinKey(string key)
        {
            NHExt.Runtime.GAIA.AuthContext ctx = NHExt.Runtime.Session.SessionCache.Current.AuthContext as NHExt.Runtime.GAIA.AuthContext;
            if (ctx == null)
            {
                throw new NHExt.Runtime.Exceptions.RuntimeException("缓存中的权限缓存类型错误");
            }
            return ctx.OrgC + "_" + ctx.UserID + "_" + key;
        }


    }
}
