using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NHExt.Runtime.Extend.Cache
{
    public class DataAuthCache : NHExt.Runtime.Cache.AbstractCache<string, string>
    {
        public DataAuthCache()
            : base("NHExt.Runtime.Cache.DataAuthCache")
        {
        }
        private static object _lockObj = new object();
        public override string JoinKey(string entityName)
        {
            NHExt.Runtime.Extend.AuthContextExtend ctx = NHExt.Runtime.Session.SessionCache.Current.AuthContext as NHExt.Runtime.Extend.AuthContextExtend;
            if (ctx == null)
            {
                throw new Exception("缓存中的权限缓存类型错误");
            }
            return ctx.Org + "_" + ctx.UserID + "_" + entityName;
        }

        public override void Clear(long org, List<long> userList, string key)
        {

            //删除缓存里所有含有当前实体的缓存
            lock (_lockObj)
            {
                //1.获取所有要删除的key
                List<string> listKeys = new List<string>();
                if (userList != null && userList.Count > 0)
                {
                    foreach (long userId in userList)
                    {
                        listKeys.Add(org + "_" + userId + "_" + key);
                    }
                }

                //2.获取所有缓存数据
                Dictionary<string, string> dic = this.getTypeCache();

                //3.取出缓存数据中的key
                List<string> keys = new List<string>();
                //Dictionary里面Keys不能用索引直接访问
                foreach (string childKey in dic.Keys)
                {
                    keys.Add(childKey);
                }

                //4.去除掉要删除的key
                if (keys.Count > 0 && listKeys.Count > 0)
                {
                    foreach (string childKey in keys)
                    {
                        foreach (string removeKey in listKeys)
                        {
                            if (childKey == removeKey)
                            {
                                dic.Remove(childKey);
                                break;
                            }
                        }

                    }
                }
            }
        }

        public override void ClearAll(string key)
        {
            //删除缓存里所有含有当前实体的缓存
            lock (_lockObj)
            {
                Dictionary<string, string> dic = this.getTypeCache();
                List<string> keys = new List<string>();
                //Dictionary里面Keys不能用索引直接访问
                foreach (string childKey in dic.Keys)
                {
                    keys.Add(childKey);
                }
                if (keys.Count > 0)
                {
                    foreach (string childKey in keys)
                    {
                        if (childKey.IndexOf(key) > 0)
                        {
                            dic.Remove(childKey);
                        }
                    }
                }
            }
        }
    }
}
