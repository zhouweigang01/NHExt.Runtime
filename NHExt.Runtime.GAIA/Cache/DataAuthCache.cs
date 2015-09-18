using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NHExt.Runtime.GAIA.Cache
{
    public class DataAuthCache : NHExt.Runtime.GAIA.Cache.MultiUserCache
    {
        public DataAuthCache()
            : base("NHExt.Runtime.GAIA.Cache.DataAuthCache")
        {
        }
        /// <summary>
        /// 清空包含当前key的缓存信息
        /// </summary>
        /// <param name="key"></param>
        public override void Clear(string key)
        {
            //删除缓存里所有含有当前实体的缓存
            lock (_lockObj)
            {
                Dictionary<string, string> dic = this.GetTypeCache();
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
        /// <summary>
        /// 清空当前组织中包含当前用户列表的缓存
        /// </summary>
        /// <param name="org"></param>
        /// <param name="userList"></param>
        /// <param name="key"></param>
        public virtual void Clear(long org, List<long> userList, string key)
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
                Dictionary<string, string> dic = this.GetTypeCache();

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
    }
}
