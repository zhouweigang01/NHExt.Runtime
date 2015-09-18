using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Caching;
using System.Collections;

namespace NHExt.Runtime.Cache
{
    public abstract class AbstractCache<TKey, TValue> : ICache<TKey, TValue>
    {

        public AbstractCache(string typeKey)
        {

            this.TypeKey = typeKey;
            this.Size = 500;
            if (AbstractCache<TKey, TValue>._cacheList == null)
            {
                AbstractCache<TKey, TValue>._cacheList = new Dictionary<string, Dictionary<TKey, TValue>>();
            }
            if (!AbstractCache<TKey, TValue>._cacheList.ContainsKey(TypeKey))
            {
                AbstractCache<TKey, TValue>._cacheList.Add(this.TypeKey, new Dictionary<TKey, TValue>());
            }
            if (AbstractCache<TKey, TValue>.CacheInstances.ContainsKey(this.TypeKey))
            {
                throw new NHExt.Runtime.Exceptions.RuntimeException("添加缓存实例对象出错，当前TypeKey：" + this.TypeKey + "所关联的实例已经存在");
            }
            else
            {
                AbstractCache<TKey, TValue>.CacheInstances.Add(this.TypeKey, this);
            }
        }
        private static Dictionary<string, Dictionary<TKey, TValue>> _cacheList = new Dictionary<string, Dictionary<TKey, TValue>>();
        /// <summary>
        /// Cache类型key
        /// </summary>
        public string TypeKey { get; private set; }
        /// <summary>
        /// Cache的大小
        /// </summary>
        public int Size { get; set; }

        public virtual void SetCache(TKey key, TValue value)
        {
            Dictionary<TKey, TValue> dic = this.GetTypeCache();
            lock (AbstractCache<TKey, TValue>._cacheList)
            {
                TKey fullKey = this.JoinKey(key);
                if (!dic.ContainsKey(fullKey))
                {
                    ///超出缓存大小则清空缓存
                    if (dic.Count >= this.Size)
                    {
                        if (dic.Keys.Count > 0)
                        {
                            TKey tmpKey = default(TKey);
                            foreach (TKey k in dic.Keys)
                            {
                                tmpKey = k;
                                break;
                            }
                            dic.Remove(tmpKey);
                        }
                    }
                    if (value != null)
                    {
                        dic.Add(fullKey, value);
                    }
                    else
                    {
                        dic.Add(fullKey, default(TValue));
                    }
                }
                else
                {
                    if (value != null)
                    {
                        dic[fullKey] = value;
                    }
                    else
                    {
                        dic[fullKey] = default(TValue);
                    }
                }
            }
        }


        public virtual TValue GetCache(TKey key)
        {
            Dictionary<TKey, TValue> cache = this.GetTypeCache();
            TKey fullKey = this.JoinKey(key);
            if (cache.ContainsKey(fullKey))
            {
                return cache[fullKey];
            }
            return default(TValue);
        }

        public virtual bool Contains(TKey key)
        {
            Dictionary<TKey, TValue> dic = this.GetTypeCache();
            TKey fullKey = this.JoinKey(key);
            if (dic.ContainsKey(fullKey))
            {
                return true;
            }
            return false;
        }
        protected static object _lockObj = new object();
        /// <summary>
        /// 清除条数据
        /// </summary>
        /// <param name="key"></param>
        public virtual void Clear(TKey key)
        {
            //删除缓存里所有含有当前实体的缓存
            lock (_lockObj)
            {
                //2.获取所有缓存数据
                Dictionary<TKey, TValue> cache = this.GetTypeCache();

                TKey fullKey = this.JoinKey(key);

                if (cache.ContainsKey(fullKey))
                {
                    cache.Remove(fullKey);
                }

            }
        }
        public virtual void ClearAll()
        {
            //删除缓存里所有含有当前实体的缓存
            lock (_lockObj)
            {
                //2.获取所有缓存数据
                Dictionary<TKey, TValue> cache = this.GetTypeCache();
                cache.Clear();
            }
        }

        public abstract TKey JoinKey(TKey key);


        /// <summary>
        /// 获取当前类型的缓存数据
        /// </summary>
        /// <returns></returns>
        protected Dictionary<TKey, TValue> GetTypeCache()
        {
            Dictionary<TKey, TValue> dic = AbstractCache<TKey, TValue>._cacheList[this.TypeKey];
            if (dic == null)
            {
                throw new NHExt.Runtime.Exceptions.RuntimeException("Cache初始化错误");
            }
            return dic;
        }


        private static Dictionary<string, object> CacheInstances = new Dictionary<string, object>();
        /// <summary>
        /// 获取缓存对象实例
        /// </summary>
        /// <param name="typeKey"></param>
        /// <returns></returns>
        public static AbstractCache<TKey, TValue> GetInstance(string typeKey)
        {
            if (AbstractCache<TKey, TValue>.CacheInstances.ContainsKey(typeKey))
            {
                return AbstractCache<TKey, TValue>.CacheInstances[typeKey] as AbstractCache<TKey, TValue>;
            }
            else
            {
                return null;
            }
        }
    }
}
