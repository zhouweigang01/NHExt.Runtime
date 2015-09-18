using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NHExt.Runtime.Session
{
    /// <summary>
    /// 二级缓存处理相关
    /// </summary>
    public class SecondCache
    {
        private NHibernate.ISessionFactory _factory = null;
        public NHibernate.ISessionFactory Factory
        {
            get
            {
                return this._factory;
            }
        }

        private SecondCache(NHibernate.ISessionFactory factory)
        {
            this._factory = factory;
        }
        #region 二级缓存相关内容

        /// <summary>
        /// 二级缓存元数据存储缓存
        /// </summary>
        private static IDictionary<string, NHibernate.Metadata.ICollectionMetadata> MetadataCollection = null;
        private NHibernate.Metadata.ICollectionMetadata getParentEntityMetaData(NHExt.Runtime.Model.BaseEntity be)
        {
            if (SecondCache.MetadataCollection == null)
            {
                lock (this.Factory)
                {
                    // 初始化二级缓存对象
                    SecondCache.MetadataCollection = this.Factory.GetAllCollectionMetadata();
                }
            }
            foreach (System.Collections.Generic.KeyValuePair<string, NHibernate.Metadata.ICollectionMetadata> kvp in SecondCache.MetadataCollection)
            {
                if (kvp.Value.ElementType.Name == be.EntityName)
                {
                    return kvp.Value;
                }
            }
            return null;
        }
        public void EvictCollection(NHExt.Runtime.Model.BaseEntity be)
        {
            NHibernate.Persister.Collection.OneToManyPersister persister = this.getParentEntityMetaData(be) as NHibernate.Persister.Collection.OneToManyPersister;
            if (persister != null)
            {
                lock (this.Factory)
                {
                    if (persister != null)
                    {
                        long entityKey = be.GetParentKey(persister.OwnerEntityName);
                        this.Factory.EvictCollection(persister.Role, entityKey);
                        NHExt.Runtime.Logger.LoggerHelper.Debug("清除聚合字段属性缓存，实体类型：" + persister.OwnerEntityName + ",实体KEY:" + entityKey, NHExt.Runtime.Logger.LoggerInstance.RuntimeLogger);
                    }
                }
            }
        }
        public void EvictEntity(NHExt.Runtime.Model.BaseEntity be)
        {
            if (be.EntityState == Enums.EntityState.UnKnow || be.EntityState == Enums.EntityState.UnChanged)
            {
                return;
            }
            lock (this.Factory)
            {
                this.Factory.EvictEntity(be.EntityName, be.ID);
                NHExt.Runtime.Logger.LoggerHelper.Debug("清除实体缓存，实体" + be.EntityName + ",ID" + be.ID, NHExt.Runtime.Logger.LoggerInstance.RuntimeLogger);
            }
        }
        public void EvictEntity(string entityName, long entityKey)
        {
            Model.BaseEntity entity = NHExt.Runtime.Session.Session.Current.Get(entityName, entityKey);
            if (entity != null)
            {
                this.EvictEntity(entity);
                this.EvictCollection(entity);
            }
        }
        #endregion

        private static SecondCache _cacheInstance = null;
        private static object locker = new object();
        public static void InitCache(NHibernate.ISessionFactory factory)
        {
            lock (locker)
            {
                if (SecondCache._cacheInstance == null)
                {
                    SecondCache._cacheInstance = new SecondCache(factory);
                }
                else
                {
                    if (SecondCache._cacheInstance.Factory != factory)
                    {
                        SecondCache._cacheInstance = new SecondCache(factory);
                    }
                }
            }
        }
        public static SecondCache GetInstance()
        {
            lock (locker)
            {
                return SecondCache._cacheInstance;
            }
        }
    }
}
