using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NHExt.Runtime.GAIA.Cache
{
    public class EntityColumnCache : NHExt.Runtime.Cache.AbstractCache<string, List<NHExt.Runtime.EntityAttribute.ColumnDescriptionAttribute>>
    {
        public EntityColumnCache()
            : base("NHExt.Runtime.Cache.EntityColumnCache")
        {
        }
        public override string JoinKey(string entityName)
        {
            return entityName;
        }
        public override void Clear(long org, List<long> userList, string key)
        {

            //删除缓存里所有含有当前实体的缓存
            lock (_lockObj)
            {
                //2.获取所有缓存数据
                Dictionary<string, List<NHExt.Runtime.EntityAttribute.ColumnDescriptionAttribute>> dic = this.GetTypeCache();
                dic.Remove(key);
            }
        }
        public override void Clear(string key)
        {
            //删除缓存里所有含有当前实体的缓存
            lock (_lockObj)
            {
                //2.获取所有缓存数据
                Dictionary<string, List<NHExt.Runtime.EntityAttribute.ColumnDescriptionAttribute>> dic = this.GetTypeCache();
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
