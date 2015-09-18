using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NHExt.Runtime.Enums;
using NHibernate;
using NHibernate.Cfg;

namespace NHExt.Runtime.Session
{
    /// <summary>
    /// 事务边界类，事务中保存了当前的session缓存和transaction事物
    /// </summary>
    public class Transaction : IDisposable
    {
        #region 类内部调用变量
        /// <summary>
        /// 当前事物中保存了当前的session缓存和transaction事物
        /// </summary>
        private ISession _session;
        private ITransaction _trans;
        //事物GUID
        private string _guid;
        //是否新生成的事物
        private bool _newTrans = true;
        //顶层事务有可能和下层事务使用的是一个session但是不是一个事务管理
        private bool _buildSession = false;

        private Transaction _PTrans = null;

        private List<NHExt.Runtime.Model.BaseEntity> _entityList = new List<Model.BaseEntity>();

        /// <summary>
        /// 浅度的拷贝构造，目的是生成当前transaction的一个副本
        /// </summary>
        /// <param name="trans"></param>
        private Transaction(Transaction trans, TransactionSupport support)
        {
            this._guid = trans._guid;
            this._session = trans.Sessi;

            this._trans = trans.Trans;

            this._newTrans = false;
            this._buildSession = false;
            this._PTrans = trans;
        }

        private Transaction(string guid, ISession session, TransactionSupport ts, SessionCache manage)
        {
            this._guid = guid;
            this._session = session;
            if (ts != TransactionSupport.Support)
            {
                this._trans = _session.BeginTransaction();
                this._newTrans = true;
            }
            else
            {
                this._trans = null;
                this._newTrans = false;
            }
            manage.CurTrans = this;
            this._buildSession = true;
        }
        /// <summary>
        /// 当前事务边界使用的事务对象
        /// </summary>
        internal ITransaction Trans
        {
            get
            {
                return this._trans;
            }
        }
        /// <summary>
        /// 当前缓存边界使用的缓存对象
        /// </summary>
        internal ISession Sessi
        {
            get
            {
                return this._session;
            }
        }
        #endregion
        /// <summary>
        /// 根据事物类型生成一个新的事物
        /// </summary>
        /// <param name="cacheGuid"></param>
        /// <param name="ts"></param>
        /// <returns></returns>
        internal static Transaction New(string cacheGuid, TransactionSupport ts, bool? useReadDB)
        {
            SessionCache cache = SessionCache.GetCache(cacheGuid, false);
            if (cache != null)
            {
                if (cache.CurTrans == null || cache.TranStack.Count == 0
                    || cache.CurSession == null || cache.SessionStack.Count == 0
                    || cache.CurTrans._session.SessionFactory.IsClosed)
                {
                    cache.Dispose();
                    cache = null;
                }
            }

            Transaction trans = null;
            ///如果当前外层没有session包裹则从参数中获取，如果外层有事务则
            ///根据外层事务判断当前到底使用哪个数据库
            //首先根据事务来判断是否使用读取数据库
            bool isReadDB = (ts == TransactionSupport.Support ? true : false);
            //如果存在cache则从cache中取
            if (cache != null)
            {
                isReadDB = cache.UseReadDB;
            }
            //如果显示设置了是否需要读取数据库从来源来获取
            if (useReadDB != null)
            {
                isReadDB = useReadDB ?? true;
            }
            if (cache == null)
            {
                //新的事务，重新生成cacheGuid
                cacheGuid = SessionGuid.NewGuid();
                cache = SessionCache.GetCache(cacheGuid, true);
                cache.UseReadDB = isReadDB;
                cache.AuthContext = Auth.AuthContext.GetInstance();
                ISessionFactory factory = SessionCache.GetSessionFactory(isReadDB);
                ISession session = factory.OpenSession();
                session.FlushMode = FlushMode.Commit;
                trans = new Transaction(cacheGuid, session, ts, cache);
                //创建新事务的时候需要初始化authcontext
                //当前是第一个事务
            }
            else
            {
                if (useReadDB == null)
                {
                    useReadDB = cache.UseReadDB;
                }
                if (ts == TransactionSupport.RequiredNew || (ts == TransactionSupport.Required && cache.CurTrans._trans == null) || isReadDB != cache.UseReadDB)
                {
                    ///如果当前存在事务的话并且上面代码没有显式要求使用哪一个数据库的话则
                    ///默认从缓存中获取当前数据库
                    NHExt.Runtime.Auth.AuthContext ctx = cache.AuthContext;
                    //新的事务，重新生成cacheGuid
                    cacheGuid = SessionGuid.NewGuid();
                    cache = SessionCache.GetCache(cacheGuid, true);
                    cache.AuthContext = ctx;
                    cache.UseReadDB = isReadDB;
                    ISessionFactory factory = SessionCache.GetSessionFactory(isReadDB);
                    ISession session = factory.OpenSession();
                    session.FlushMode = FlushMode.Commit;
                    trans = new Transaction(cacheGuid, session, ts, cache);
                }
                else
                {
                    //上层存在事务的话就不用考虑是否重新生成session
                    trans = new Transaction(cache.CurTrans, ts);
                }
            }
            cache.TranStack.Push(trans);
            cache.CurTrans = trans;
            ////生成事物之后自动创建一个session边界
            Session.New(cacheGuid);

            return trans;
        }
        /// <summary>
        /// 根据事物类型生成一个新的事物
        /// </summary>
        /// <param name="ts"></param>
        /// <returns></returns>
        public static Transaction New(TransactionSupport ts, bool? useReadDB = null)
        {
            return Transaction.New(SessionGuid.Guid, ts, useReadDB);
        }
        /// <summary>
        /// 提交事物，如果是使用的是上层事物的话就不做任何处理，根据上层的事物来决定当前
        /// </summary>
        public void Commit()
        {
            if (Session.Current != null)
            {
                if (Session.Current.EntityCount > 0)
                {
                    Session.Current.Commit();
                }
            }
            if (this._newTrans)
            {
                if (this.Trans != null && !this.Trans.WasCommitted && !this.Trans.WasRolledBack)
                {
                    this.Trans.Commit();
                    this.Trans.Dispose();
                    this._trans = null;
                }
            }
        }
        /// <summary>
        /// 回滚事物，如果当前使用的是上层事物则将当前缓存清空，具体事物是否提交需要根据上层事物来确定
        /// </summary>
        public void RollBack()
        {
            //清空session缓存
            if (Session.Current != null)
            {
                if (Session.Current.EntityCount > 0)
                {
                    Session.Current.RollBack();
                }
            }

            if (this.Trans != null && !this.Trans.WasCommitted && !this.Trans.WasRolledBack)
            {
                this.Trans.Rollback();
                this.Trans.Dispose();
                this._trans = null;

            }
            //回滚的时候需要从二级缓存中删除已经更改的数据
            this.refreshSecondCache(true);

        }
        /// <summary>
        /// 事物声明周期结束的时候调用函数，主要释放缓存数据以及保证
        /// 事务栈和缓存栈的生命周期的一致性，如果使用的是新事物的话
        /// 则关闭缓存如果不是新事务的话则只是保证事务栈和缓存栈的一致性
        /// </summary>
        public void Dispose()
        {
            this.Commit();
            SessionCache cache = SessionCache.GetCache(_guid);

            //因为生成事物的时候生成了一个新的事物，所以在销毁的时候需要去掉这个事物
            if (Session.Current != null)
            {
                Session.Current.Dispose();
            }

            //新生成的事务要释放session
            if (this._buildSession)
            {
                if (this._session != null)
                {
                    if (this._session.IsOpen)
                    {
                        this._session.Clear();
                        this._session.Close();
                    }
                    this._session.Dispose();
                    this._session = null;
                }

                SessionGuid.BackSpaceGuid();
                cache.Dispose();
            }
            else
            {
                if (cache.TranStack.Count > 0)
                {
                    Transaction trans = cache.TranStack.Pop();
                    trans = null;
                }
                if (cache.TranStack.Count > 0)
                {
                    Transaction trans = cache.TranStack.Pop();
                    cache.CurTrans = trans;
                    cache.TranStack.Push(trans);
                }
            }
        }

        internal void Append(Model.BaseEntity be)
        {
            _entityList.Add(be);
            if (this._PTrans != null)
            {
                this._PTrans.Append(be);
            }
        }

        internal void Append(List<Model.BaseEntity> beList)
        {
            _entityList.AddRange(beList);
            if (this._PTrans != null)
            {
                this._PTrans.Append(beList);
            }
        }
        internal void Remove(Model.BaseEntity be)
        {
            this._entityList.Remove(be);
            if (this._PTrans != null)
            {
                this._PTrans.Remove(be);
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
                if (this._PTrans != null)
                {
                    this._PTrans.Remove(be);
                }
            }
            this._entityList.Clear();
        }
    }
}
