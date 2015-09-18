using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using NHExt.Runtime.Session;
using NHibernate;
using System.Collections;

namespace NHExt.Runtime.Query
{
    public static class EntityQuery
    {
        /// <summary>
        /// 执行sql语句
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="paramList"></param>
        /// <returns></returns>
        public static DataTable ExecuteSQL(string sql, List<object> paramList = null, int startIndex = -1, int recordCount = -1)
        {
            SessionCache manage = SessionCache.Current;
            Transaction trans = null;
            sql = buildSQL(sql);
            DataSet resultDS = null;

            if (manage == null)
            {
                //外层没有事物的话需要手动创建事物，并手工的销毁事物
                trans = Transaction.New(Enums.TransactionSupport.Support);
                manage = SessionCache.Current;
            }
            try
            {
                ISession _curSession = manage.CurTrans.Sessi;
                if (_curSession != null && _curSession.IsOpen)
                {
                    IQuery query = _curSession.CreateSQLQuery(sql);
                    if (paramList != null && paramList.Count > 0)
                    {
                        for (int i = 0; i < paramList.Count; i++)
                        {
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
                    resultDS = query.ExcuteDataSet();
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
            if (resultDS != null && resultDS.Tables.Count > 0)
            {
                return resultDS.Tables[0];
            }
            return null;

        }

        public static int ExecuteSqlUpdate(string sql, List<object> paramList = null)
        {
            SessionCache cache = SessionCache.Current;
            Transaction trans = null;
            sql = buildSQL(sql);
            int recordCount = 0;

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
                    IQuery query = _curSession.CreateSQLQuery(sql);
                    if (paramList != null && paramList.Count > 0)
                    {
                        for (int i = 0; i < paramList.Count; i++)
                        {
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

                    recordCount = query.ExecuteUpdate();
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
            return recordCount;
        }
        /// <summary>
        /// 执行hql语句查询操作，返回IList对象
        /// </summary>
        /// <param name="hql"></param>
        /// <param name="paramList"></param>
        /// <returns></returns>
        public static IList<object[]> ExecuteHQL(string hql = "", List<object> paramList = null, int startIndex = -1, int recordCount = -1)
        {
            hql = buildHql(hql);

            //根据权限获取重新生成HQL数据
            QueryParser parser = QueryParser.GetInstance(hql);
            hql = parser.ParseHql(NHExt.Runtime.Session.Session.Current.IgnoreDataFilter);
            SessionCache cache = SessionCache.Current;
            Transaction trans = null;
            IList<object[]> resultList = null;
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

                    resultList = query.List<object[]>();
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
            return resultList;

        }
        /// <summary>
        /// 执行HQL语句查询操作
        /// </summary>
        /// <param name="hql"></param>
        /// <param name="paramList"></param>
        /// <param name="startIndex"></param>
        /// <param name="recordCount"></param>
        /// <param name="isAuth"></param>
        /// <returns></returns>
        public static DataSet ExecuteHqlSelect(string hql = "", List<object> paramList = null, int startIndex = -1, int recordCount = -1)
        {
            hql = buildHql(hql);

            //根据权限获取重新生成HQL数据
            QueryParser parser = QueryParser.GetInstance(hql);
            hql = parser.ParseHql(NHExt.Runtime.Session.Session.Current.IgnoreDataFilter);

            SessionCache cache = SessionCache.Current;
            Transaction trans = null;
            DataSet resultDS = null;
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

                    resultDS = query.ExcuteDataSet();
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
            return resultDS;

        }
        /// <summary>
        /// 执行HQL语句更新删除操作
        /// </summary>
        /// <param name="hql"></param>
        /// <param name="paramList"></param>
        /// <param name="startIndex"></param>
        /// <param name="recordCount"></param>
        /// <param name="isAuth"></param>
        /// <returns></returns>
        public static int ExecuteHqlUpdate(string hql = "", List<object> paramList = null)
        {
            hql = buildHql(hql);

            //根据权限获取重新生成HQL数据
            QueryParser parser = QueryParser.GetInstance(hql);
            hql = parser.ParseHql(NHExt.Runtime.Session.Session.Current.IgnoreDataFilter);

            int rowCount = 0;
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
                    rowCount = query.ExecuteUpdate();
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
            return rowCount;

        }
        /// <summary>
        /// 调用存储过程
        /// </summary>
        /// <param name="procedureName"></param>
        /// <param name="paramList"></param>
        public static void ExecuteProcedure(string procedureName, List<DbDataParameter> paramList)
        {
            SessionCache cache = SessionCache.Current;
            Transaction trans = null;

            if (cache == null)
            {
                //外层没有事物的话需要手动创建事物，并手工的销毁事物
                trans = Transaction.New(Enums.TransactionSupport.Support);
                cache = SessionCache.Current;
            }
            ISession _curSession = cache.CurTrans.Sessi;
            if (_curSession != null && _curSession.IsOpen)
            {
                try
                {
                    IQuery query = _curSession.CreateProcedureQuery(procedureName);
                    for (int i = 0; i < paramList.Count; i++)
                    {
                        query.SetProcedureParameter(paramList[i].Name, paramList[i].DbType, paramList[i].Value, paramList[i].Direction);
                    }
                    query.ExcuteProcedure();
                    for (int i = 0; i < paramList.Count; i++)
                    {
                        DbDataParameter p = paramList[i];
                        if (p.Direction == ParameterDirection.Input) continue;
                        p.Value = query.ProcedureParamterList[i].Value;
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

                if (trans != null)
                {
                    trans.Dispose();
                }
            }
        }
        /// <summary>
        /// 对于传进来的hql进行预处理
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="hql"></param>
        /// <returns></returns>
        private static string buildHql(string hql)
        {
            string HQL = string.Empty;
            //转换hql,防止hql注入
            hql = HqlHelper.TransferHql(hql);
            return hql;
        }

        private static string buildSQL(string sql)
        {
            string SQL = string.Empty;
            SQL = HqlHelper.TransferSQL(sql);
            return SQL;
        }

    }
    public class DbDataParameter
    {
        public string Name { get; set; }
        public object Value { get; set; }
        private ParameterDirection _direction = ParameterDirection.Input;
        public ParameterDirection Direction
        {
            get { return _direction; }
            set { _direction = value; }
        }
        public DbType DbType { get; set; }
        public int Size { get; set; }

        private DbDataParameter() { }
        public static DbDataParameter Create(string name, DbType t, object v, ParameterDirection dir = ParameterDirection.Input, int size = 0)
        {
            DbDataParameter param = new DbDataParameter();
            param.Name = name;
            param.DbType = t;
            param.Value = v;
            param.Direction = dir;
            if (t == System.Data.DbType.String)
            {
                if (dir == ParameterDirection.Output || dir == ParameterDirection.InputOutput)
                {
                    param.Size = size;
                }
                else
                {
                    param.Size = v.ToString().Length;
                }
            }

            return param;
        }
    }
}
