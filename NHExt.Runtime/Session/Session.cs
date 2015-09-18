using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NHExt.Runtime.Enums;
using NHExt.Runtime.Model;
using NHibernate;
using NHibernate.Cfg;

namespace NHExt.Runtime.Session
{
    /// <summary>
    /// 缓存边界，处理将实体对象加入到缓存中
    /// </summary>
    public class Session : IDisposable
    {
        /// <summary>
        /// 当前缓存的所有实体
        /// </summary>
        private List<BaseEntity> _entityList;
        /// <summary>
        /// 当前事务边界对象
        /// </summary>
        private Transaction _trans;

        private string _sessionGuid;
        /// <summary>
        /// 最初始状态是否走数据权限
        /// </summary>
        private bool OrignalDataAuth { get; set; }
        /// <summary>
        /// 是否走数据权限校验
        /// </summary>
        public bool IsDataAuth { get; internal set; }

        /// <summary>
        /// 是否忽略根组织过滤
        /// </summary>
        public bool IgnoreRootOrgFilter { get; set; }
        /// <summary>
        /// 集团版中需要忽略多组织过滤条件
        /// </summary>
        public bool IgnoreOrgFilter { get; set; }
        /// <summary>
        /// 权限系统中忽略数据权限过滤
        /// </summary>
        public bool IgnoreDataFilter { get; set; }

        /// <summary>
        /// 构造函数，对缓存边界对象进行初始化
        /// </summary>
        /// <param name="guid"></param>
        /// <param name="trans"></param>
        private Session(string guid, Transaction trans)
        {
            this._entityList = new List<BaseEntity>();
            _trans = trans;
            _sessionGuid = guid;
            this.OrignalDataAuth = this.IsDataAuth = true;
            this.IgnoreRootOrgFilter = false;
            this.IgnoreOrgFilter = false;
            this.IgnoreDataFilter = false;
        }
        /// <summary>
        /// 从事务边界中取出当前事务边界，同时生成一个新的缓存边界
        /// 这里需要注意的是，在缓存边界的上方肯定存在一个事物边界
        /// 如果上层不存在的话会直接报空引用
        /// </summary>
        /// <param name="cacheGuid"></param>
        /// <returns></returns>
        internal static Session New(string cacheGuid)
        {
            SessionCache cache = SessionCache.GetCache(cacheGuid);
            Transaction trans = cache.CurTrans;
            Session s = new Session(cacheGuid, trans);
            //判断当前session是否需要权限校验
            if (cache.CurSession != null)
            {
                s.IsDataAuth = cache.CurSession.IsDataAuth;
            }
            else
            {
                s.IsDataAuth = NHExt.Runtime.Cfg.GetCfg<bool>("IsDataAuth");
            }
            cache.SessionStack.Push(s);
            cache.CurSession = s;
            return s;

        }
        /// <summary>
        /// 生成一个缓存边界
        /// </summary>
        /// <returns></returns>
        public static Session New()
        {
            return Session.New(SessionGuid.Guid);
        }
        /// <summary>
        /// 判断当前缓存中是否存在该实体对象
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public bool Contains(BaseEntity entity)
        {
            return _trans.Sessi.Contains(entity);
        }
        /// <summary>
        /// 将实体加入到当前缓存边界中，这里需要注意的是
        /// 新增的实体如果不加入的话不会将该数据提交到数据
        /// 库中去
        /// </summary>
        /// <param name="entity"></param>
        public void InList(BaseEntity entity)
        {
            if (!this._entityList.Contains(entity))
            {

                if (entity.EntityState == EntityState.UnKnow)
                {
                    if (entity.ID <= 0)
                    {
                        entity.EntityState = EntityState.Add;
                    }
                    else
                    {
                        entity.EntityState = EntityState.Update;
                    }
                }
                else if (entity.EntityState == EntityState.UnChanged)
                {
                    entity.EntityState = EntityState.Update;
                }
                this._entityList.Add(entity);
            }
        }
        /// <summary>
        /// 从当前缓存边界中删除实体
        /// </summary>
        /// <param name="entity"></param>
        public void DeList(BaseEntity entity)
        {
            this._entityList.Remove(entity);
            //深度删除，当前session缓存中删除当前对象，将对象
            //从托管态修改为自由态
            if (_trans.Sessi.Contains(entity))
            {
                _trans.Sessi.Evict(entity);
            }
        }
        /// <summary>
        /// 删除某一个实体
        /// </summary>
        /// <param name="entity"></param>
        public void Remove(BaseEntity entity)
        {
            if (entity.EntityState == EntityState.Add)
            {
                this.DeList(entity);
            }
            else
            {
                entity.EntityState = EntityState.Delete;
                this.InList(entity);
            }
        }
        /// <summary>
        /// 从缓存中获取当前实体
        /// </summary>
        /// <param name="entityName"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public BaseEntity Get(string entityName, long id)
        {
            BaseEntity entity = _trans.Sessi.Get(entityName, id) as BaseEntity;
            return entity;
        }
        /// <summary>
        /// 获取当前正在使用的缓存边界，如果当前上层不存在事物的话
        /// 这里获取到得缓存边界将会是空对象
        /// </summary>
        public static Session Current
        {
            get
            {
                SessionCache cache = SessionCache.GetCache(SessionGuid.Guid);
                if (cache != null)
                {
                    return cache.CurSession;
                }
                return null;
            }
        }
        /// <summary>
        /// 清理缓存边界，清空的过程需要将当前缓存边界从
        /// 当前栈中弹出来
        /// </summary>
        public void Dispose()
        {
            SessionCache cache = SessionCache.GetCache(this._sessionGuid);
            if (cache == null) return;
            if (cache.SessionStack != null && cache.SessionStack.Count > 0)
            {
                if (Session.Current.EntityCount > 0)
                {
                    Session.Current.Commit();
                }
                if (cache.SessionStack.Count > 0)
                {
                    cache.SessionStack.Pop();
                }
                if (cache.SessionStack.Count > 0)
                {
                    Session session = cache.SessionStack.Pop();
                    cache.SessionStack.Push(session);
                    cache.CurSession = session;
                }
                else
                {
                    cache.CurSession = null;
                }
            }
            else
            {
                cache.CurSession = null;
            }
        }
        /// <summary>
        /// 将当前缓存边界的数据提交到NH得缓存中去
        /// 注意：如果提交失败，将会清空当前缓存边界
        /// </summary>
        public void Commit()
        {
            try
            {
                //commit的时候走实体校验，实体校验不需要进行数据权限校验
                this.IsDataAuth = false;
                if (!_trans.Sessi.IsConnected || !_trans.Sessi.IsOpen)
                {
                    _trans.Sessi.Reconnect();
                }
                for (int i = 0; i < this._entityList.Count; i++)
                {
                    BaseEntity entity = this._entityList[i];
                    entity.Do();
                }
                //只有flush之后才能真正的加入到缓存中，使用NH对象查询语言才能从缓存中查询到该实体
                if (this._entityList.Count > 0)
                {
                    _trans.Sessi.Flush();

                    //提交缓存之后需要更新实体的orignaldata
                    foreach (BaseEntity entity in this._entityList)
                    {
                        //实体提交缓存之后需要将实体状态设置为未改变状态
                        //所有实体已经修改过了 所以可以设置为为修改了，
                        //当前字段不会参与CRUD，所以不会影响实体状态
                        //相当于实体变成自由态了
                        if (entity.EntityState != EntityState.Delete)
                        {
                            entity.RefreshOrignalData();
                            entity.EntityState = EntityState.UnChanged;
                        }
                    }
                    this._trans.Append(this._entityList);
                    //提交之后清空二级缓存
                    try
                    {
                        this.refreshSecondCache(false);
                    }
                    catch (Exception ex)
                    {
                        NHExt.Runtime.Logger.LoggerHelper.Error(ex, NHExt.Runtime.Logger.LoggerInstance.RuntimeLogger);
                    }

                }
                //执行完成之后需要恢复初始状态
                this.IsDataAuth = this.OrignalDataAuth;
            }
            catch (NHExt.Runtime.Exceptions.BizException ex)
            {
                this.refreshSecondCache(true);
                _trans.Sessi.Clear();

                NHExt.Runtime.Logger.LoggerHelper.Error(ex, NHExt.Runtime.Logger.LoggerInstance.BizLogger);
                throw ex;
            }
            catch (NHExt.Runtime.Exceptions.RuntimeException ex)
            {
                this.refreshSecondCache(true);
                _trans.Sessi.Clear();

                NHExt.Runtime.Logger.LoggerHelper.Error(ex);
                throw new NHExt.Runtime.Exceptions.RuntimeException("提交session发生错误");
            }
            finally
            {
                this._entityList.Clear();
            }
        }
        /// <summary>
        /// 回滚session
        /// </summary>
        public void RollBack()
        {
            if (this._entityList != null)
            {
                foreach (Model.BaseEntity be in this._entityList)
                {
                    _trans.Sessi.Evict(be);
                }

                try
                {
                    this.refreshSecondCache(true);
                }
                catch (Exception ex)
                {
                    NHExt.Runtime.Logger.LoggerHelper.Error(ex, NHExt.Runtime.Logger.LoggerInstance.RuntimeLogger);
                }

                this._entityList.Clear();
            }


        }
        /// <summary>
        /// 根据实体的状态，将实体添加到不同的缓存区域
        /// </summary>
        /// <param name="entity"></param>
        protected internal void CommitEntity(BaseEntity entity)
        {
            if (entity.EntityState == EntityState.Add)
            {
                _trans.Sessi.Save(entity);
            }
            else if (entity.EntityState == EntityState.Update)
            {
                if (entity.ID > -1 && entity.SysVersion < entity.OrignalData.SysVersion)
                {
                    throw new NHExt.Runtime.Exceptions.SysVersionErrorException(entity.EntityName, entity.ID);
                }
                _trans.Sessi.Update(entity);
            }
            else if (entity.EntityState == EntityState.Delete)
            {
                _trans.Sessi.Delete(entity);
            }
        }
        /// <summary>
        /// 当前session没有提交到缓存中的实体数量
        /// </summary>
        public int EntityCount
        {
            get
            {
                if (this._entityList == null) return 0;
                return this._entityList.Count;
            }
        }


        /// <summary>
        /// 刷新所有二级缓存中当前trans关联的实体
        /// </summary>
        internal void refreshSecondCache(bool delEntity)
        {
            foreach (Model.BaseEntity be in this._entityList)
            {
                if (delEntity)
                {
                    SecondCache.GetInstance().EvictEntity(be);
                }
                SecondCache.GetInstance().EvictCollection(be);
            }
            this._entityList.Clear();
        }

    }
}
