using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NHExt.Runtime.Cache
{
    public interface ICache<TKey, TValue>
    {
        /// <summary>
        /// 设置缓存
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        void SetCache(TKey key, TValue value);

        /// <summary>
        /// 获取缓存
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        TValue GetCache(TKey key);


        /// <summary>
        /// 缓存中是否包含cache
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        bool Contains(TKey key);


        ///// <summary>
        ///// 清空指定缓存
        ///// </summary>
        //void Clear(long org, List<long> userList, TKey key);

        /// <summary>
        /// 清空当前登录用户的缓存
        /// </summary>
        /// <param name="key"></param>
        void Clear(TKey key);

        /// <summary>
        /// 清除所有
        /// </summary>
        void ClearAll();

        /// <summary>
        /// 定义缓存Key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        TKey JoinKey(TKey key);

        /// <summary>
        /// 缓存大小
        /// </summary>
        int Size { get; set; }
    }
}
