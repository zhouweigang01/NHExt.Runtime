using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NHExt.Runtime.EntityAttribute;
using NHExt.Runtime.Model;
using NHExt.Runtime.Session;
using NHExt.Runtime.Util;
using NHibernate;
using System.Data;
using System.Collections;

namespace NHExt.Runtime.Query
{
    public static class EntityFinder
    {
        #region 内部调用函数
        /// <summary>
        /// 内部调用函数
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="hql"></param>
        /// <param name="entityNum"></param>
        /// <param name="paramList"></param>
        /// <returns></returns>
        private static List<T> FindAll<T>(string hql, int entityNum, List<object> paramList = null, int startIndex = -1, int recordCount = -1) where T : BaseEntity
        {
            hql = EntityFinder.buildHql<T>(hql);

            //根据权限获取重新生成HQL数据
            QueryParser parser = QueryParser.GetInstance(hql);
            hql = parser.ParseHql(NHExt.Runtime.Session.Session.Current.IgnoreDataFilter);

            List<T> entityList = null;
            SessionCache cache = SessionCache.Current;
            Transaction trans = null;
            if (cache == null)
            {
                //外层没有事物的话需要手动创建事物，并手工的销毁事物
                trans = Transaction.New(Enums.TransactionSupport.Support);
                cache = SessionCache.Current;
            }
            try
            {
                ISession _curSession = cache.CurTrans.Sessi;
                if (_curSession != null && _curSession.IsOpen)
                {
                    IQuery query = _curSession.CreateQuery(hql);
                    if (entityNum > 0)
                    {
                        query.SetMaxResults(entityNum);
                    }
                    if (paramList != null && paramList.Count > 0)
                    {
                        for (int i = 0; i < paramList.Count; i++)
                        {
                            // query.SetString("fn" + i, paramList[i].ToString());
                            if (paramList[i] is System.Collections.ICollection)
                            {
                                ICollection lst = paramList[i] as ICollection;
                                if (lst != null)
                                {
                                    query.SetParameterList("fn" + i, lst);
                                }
                                else
                                {
                                    throw new NHExt.Runtime.Exceptions.RuntimeException("查询参数转换出错,出错字段位置:" + i);
                                }
                            }
                            else
                            {
                                query.SetParameter("fn" + i, paramList[i]);
                            }
                        }
                    }
                    if (startIndex >= 0)
                    {
                        query.SetFirstResult(startIndex);
                    }
                    if (recordCount >= 0)
                    {
                        query.SetMaxResults(recordCount);
                    }
                    entityList = query.SetCacheable(true).List<T>() as List<T>;
                }
                if (entityList == null)
                {
                    entityList = new List<T>();
                }
                foreach (T t in entityList)
                {
                    t.InitOrignalData();
                }
                if (trans != null)
                {
                    trans.Commit();
                }
            }
            catch
            {
                if (trans != null)
                {
                    trans.RollBack();
                }
                throw;
            }
            finally
            {
                if (trans != null)
                {
                    //手工生成的事物需要手工释放
                    trans.Dispose();
                }
            }
            return entityList;
        }
        /// <summary>
        /// 弱类型超找全部
        /// </summary>
        /// <param name="entityType"></param>
        /// <param name="hql"></param>
        /// <param name="entityNum"></param>
        /// <param name="paramList"></param>
        /// <param name="startIndex"></param>
        /// <param name="recordCount"></param>
        /// <returns></returns>
        private static List<BaseEntity> FindAll(Type entityType, string hql, int entityNum, List<object> paramList = null, int startIndex = -1, int recordCount = -1)
        {
            hql = EntityFinder.buildHql(entityType, hql);

            //根据权限获取重新生成HQL数据
            QueryParser parser = QueryParser.GetInstance(hql);
            hql = parser.ParseHql(NHExt.Runtime.Session.Session.Current.IgnoreDataFilter);

            List<BaseEntity> entityList = null;
            SessionCache cache = SessionCache.Current;
            Transaction trans = null;
            if (cache == null)
            {
                //外层没有事物的话需要手动创建事物，并手工的销毁事物
                trans = Transaction.New(Enums.TransactionSupport.Support);
                cache = SessionCache.Current;
            }
            try
            {
                ISession _curSession = cache.CurTrans.Sessi;
                if (_curSession != null && _curSession.IsOpen)
                {
                    IQuery query = _curSession.CreateQuery(hql);
                    if (entityNum > 0)
                    {
                        query.SetMaxResults(entityNum);
                    }
                    if (paramList != null && paramList.Count > 0)
                    {
                        for (int i = 0; i < paramList.Count; i++)
                        {
                            // query.SetString("fn" + i, paramList[i].ToString());
                            if (paramList[i] is System.Collections.ICollection)
                            {
                                ICollection lst = paramList[i] as ICollection;
                                if (lst != null)
                                {
                                    query.SetParameterList("fn" + i, lst);
                                }
                                else
                                {
                                    throw new NHExt.Runtime.Exceptions.RuntimeException("查询参数转换出错,出错字段位置:" + i);
                                }
                            }
                            else
                            {
                                query.SetParameter("fn" + i, paramList[i]);
                            }
                        }
                    }
                    if (startIndex >= 0)
                    {
                        query.SetFirstResult(startIndex);
                    }
                    if (recordCount >= 0)
                    {
                        query.SetMaxResults(recordCount);
                    }
                    entityList = query.SetCacheable(true).List<BaseEntity>() as List<BaseEntity>;
                }
                if (entityList == null)
                {
                    entityList = new List<BaseEntity>();
                }
                foreach (BaseEntity t in entityList)
                {
                    t.RefreshOrignalData();
                }
                if (trans != null)
                {
                    trans.Commit();
                }
            }
            catch
            {

                if (trans != null)
                {
                    trans.RollBack();
                }
                throw;
            }
            finally
            {
                if (trans != null)
                {
                    //手工生成的事物需要手工释放
                    trans.Dispose();
                }
            }
            return entityList;
        }
        private static List<BaseEntity> FindAll(string entityTypeName, string hql, int entityNum, List<object> paramList = null, int startIndex = -1, int recordCount = -1)
        {
            hql = EntityFinder.buildHql(entityTypeName, hql);

            //根据权限获取重新生成HQL数据
            QueryParser parser = QueryParser.GetInstance(hql);
            hql = parser.ParseHql(NHExt.Runtime.Session.Session.Current.IgnoreDataFilter);

            List<BaseEntity> entityList = null;
            SessionCache cache = SessionCache.Current;
            Transaction trans = null;
            if (cache == null)
            {
                //外层没有事物的话需要手动创建事物，并手工的销毁事物
                trans = Transaction.New(Enums.TransactionSupport.Support);
                cache = SessionCache.Current;
            }
            try
            {
                ISession _curSession = cache.CurTrans.Sessi;
                if (_curSession != null && _curSession.IsOpen)
                {
                    IQuery query = _curSession.CreateQuery(hql);
                    if (entityNum > 0)
                    {
                        query.SetMaxResults(entityNum);
                    }
                    if (paramList != null && paramList.Count > 0)
                    {
                        for (int i = 0; i < paramList.Count; i++)
                        {
                            // query.SetString("fn" + i, paramList[i].ToString());
                            if (paramList[i] is System.Collections.ICollection)
                            {
                                ICollection lst = paramList[i] as ICollection;
                                if (lst != null)
                                {
                                    query.SetParameterList("fn" + i, lst);
                                }
                                else
                                {
                                    throw new NHExt.Runtime.Exceptions.RuntimeException("查询参数转换出错,出错字段位置:" + i);
                                }
                            }
                            else
                            {
                                query.SetParameter("fn" + i, paramList[i]);
                            }
                        }
                    }
                    if (startIndex >= 0)
                    {
                        query.SetFirstResult(startIndex);
                    }
                    if (recordCount >= 0)
                    {
                        query.SetMaxResults(recordCount);
                    }
                    entityList = query.SetCacheable(true).List<BaseEntity>() as List<BaseEntity>;
                }
                if (entityList == null)
                {
                    entityList = new List<BaseEntity>();
                }
                foreach (BaseEntity t in entityList)
                {
                    t.RefreshOrignalData();
                }
                if (trans != null)
                {
                    trans.Commit();
                }
            }
            catch
            {
                if (trans != null)
                {
                    trans.RollBack();
                }
                throw;
            }
            finally
            {
                if (trans != null)
                {
                    //手工生成的事物需要手工释放
                    trans.Dispose();
                }
            }
            return entityList;
        }
        /// <summary>
        /// 对于传进来的hql进行预处理
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="hql"></param>
        /// <returns></returns>
        private static string buildHql<T>(string hql) where T : BaseEntity
        {
            string HQL = string.Empty;
            if (hql.IndexOf("select") >= 0
                || hql.IndexOf("SELECT") >= 0
                || hql.IndexOf("where") >= 0
                || hql.IndexOf("WHERE") >= 0
                || hql.IndexOf("from") >= 0
                || hql.IndexOf("FROM") >= 0)
            {
                HQL = hql;
            }
            else
            {
                BussinesAttribute attr = AttributeHelper.GetClassAttr<BussinesAttribute>(typeof(T));
                HQL = " from " + attr.EntityName;

                if (!string.IsNullOrEmpty(hql))
                {
                    HQL += " where " + hql;
                }
            }
            //转换hql,防止hql注入
            HQL = HqlHelper.TransferHql(HQL);
            return HQL;
        }
        private static string buildHql(Type entityType, string hql)
        {

            BussinesAttribute attr = AttributeHelper.GetClassAttr<BussinesAttribute>(entityType);
            if (attr == null)
            {
                throw new NHExt.Runtime.Exceptions.RuntimeException("hql解析失败,当前类型：" + entityType.FullName + "不是持久化实体");
            }
            return buildHql(attr.EntityName, hql);
            //转换hql,防止hql注入
        }
        private static string buildHql(string entityTypeName, string hql)
        {
            string HQL = string.Empty;
            if (hql.IndexOf("select") >= 0
                || hql.IndexOf("SELECT") >= 0
                || hql.IndexOf("where") >= 0
                || hql.IndexOf("WHERE") >= 0
                || hql.IndexOf("from") >= 0
                || hql.IndexOf("FROM") >= 0)
            {
                HQL = hql;
            }
            else
            {
                HQL = " from " + entityTypeName;

                if (!string.IsNullOrEmpty(hql))
                {
                    HQL += " where " + hql;
                }
            }
            //转换hql,防止hql注入
            HQL = HqlHelper.TransferHql(HQL);
            return HQL;
        }
        private static string subString(string filterStr, string key)
        {
            int index = getIndex(filterStr, "Select");
            if (index >= 0)
            {
                index = getIndex(filterStr, "Where");
                if (index >= 0)
                {
                    filterStr = filterStr.Substring(index).Trim();
                    filterStr = filterStr.Substring(5);
                }
            }

            index = getIndex(filterStr, key);
            if (index >= 0)
            {
                filterStr = filterStr.Substring(0, index);
            }
            return filterStr;
        }
        private static int getIndex(string filterStr, string key)
        {
            if (string.IsNullOrEmpty(filterStr))
            {
                return -1;
            }
            string tmp = filterStr.ToLower();
            key = key.ToLower();
            int index = tmp.IndexOf(key + " ");
            if (index < 0)
            {
                index = tmp.IndexOf(" " + key);
            }
            return index;
        }

        private static string decodeFilter(string filter)
        {
            filter = subString(filter, "Order");
            filter = subString(filter, "Group");
            filter = subString(filter, "Having");
            return filter;
        }
        #endregion

        /// <summary>
        /// 查找所有满足条件的实体列表
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="hql"></param>
        /// <param name="paramList"></param>
        /// <returns></returns>
        public static List<T> FindAll<T>(string hql = "", List<object> paramList = null, int startIndex = -1, int recordCount = -1) where T : BaseEntity
        {
            return FindAll<T>(hql, -1, paramList, startIndex, recordCount);
        }
        public static List<BaseEntity> FindAll(Type entityType, string hql = "", List<object> paramList = null, int startIndex = -1, int recordCount = -1)
        {
            return FindAll(entityType, hql, -1, paramList, startIndex, recordCount);
        }

        public static List<BaseEntity> FindAll(string entityTypeName, string hql = "", List<object> paramList = null, int startIndex = -1, int recordCount = -1)
        {
            return FindAll(entityTypeName, hql, -1, paramList, startIndex, recordCount);
        }
        /// <summary>
        /// 查找满足条件的一个实体
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="hql"></param>
        /// <param name="paramList"></param>
        /// <returns></returns>
        public static T Find<T>(string hql = "", List<object> paramList = null, int startIndex = -1) where T : BaseEntity
        {
            List<T> entityList = FindAll<T>(hql, 1, paramList, startIndex);
            if (entityList.Count > 0)
                return entityList[0];
            return null;
        }
        public static BaseEntity Find(Type entityType, string hql = "", List<object> paramList = null, int startIndex = -1)
        {
            List<BaseEntity> entityList = FindAll(entityType, hql, 1, paramList, startIndex);
            if (entityList.Count > 0)
                return entityList[0];
            return null;
        }
        public static BaseEntity Find(string entityTypeName, string hql = "", List<object> paramList = null, int startIndex = -1)
        {
            List<BaseEntity> entityList = FindAll(entityTypeName, hql, 1, paramList, startIndex);
            if (entityList.Count > 0)
                return entityList[0];
            return null;
        }
        public static T FindById<T>(long id) where T : BaseEntity
        {
            if (id < 0) return null;
            string hql = "ID=" + id;
            List<T> entityList = FindAll<T>(hql, 1, null);
            if (entityList.Count > 0)
                return entityList[0];
            return null;
        }
        public static BaseEntity FindById(Type entityType, long id)
        {
            if (id < 0) return null;
            string hql = "ID=" + id;
            return Find(entityType, hql, null);
        }
        public static BaseEntity FindById(string entityTypeName, long id)
        {
            if (id < 0) return null;
            string hql = "ID=" + id;
            return Find(entityTypeName, hql);
        }

        public static decimal? Sum<T>(string field, string filterStr = "", List<object> paramList = null) where T : BaseEntity
        {

            filterStr = decodeFilter(filterStr);
            BussinesAttribute attr = Util.AttributeHelper.GetClassAttr<BussinesAttribute>(typeof(T));
            string hql = "select 1,sum(" + field + ") from " + attr.EntityName;
            if (!string.IsNullOrEmpty(filterStr))
            {
                hql += " where " + filterStr;
            }
            IList<object[]> queryList = NHExt.Runtime.Query.EntityQuery.ExecuteHQL(hql, paramList, -1, -1);
            if (queryList.Count > 0)
            {
                object obj = queryList[0][1];
                if (obj != null)
                {
                    return Convert.ToDecimal(obj);
                }
            }
            return 0;

        }
        public static int? Count<T>(string filterStr = "", List<object> paramList = null) where T : BaseEntity
        {
            filterStr = decodeFilter(filterStr);
            BussinesAttribute attr = Util.AttributeHelper.GetClassAttr<BussinesAttribute>(typeof(T));
            string hql = "select 1,count(ID) from " + attr.EntityName;
            if (!string.IsNullOrEmpty(filterStr))
            {
                hql += " where " + filterStr;
            }
            IList<object[]> queryList = NHExt.Runtime.Query.EntityQuery.ExecuteHQL(hql, paramList, -1, -1);
            if (queryList.Count > 0)
            {
                object obj = queryList[0][1];
                if (obj != null)
                {
                    return Convert.ToInt32(obj);
                }
            }
            return null;

        }

    }
}
