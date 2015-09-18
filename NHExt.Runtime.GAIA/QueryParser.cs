using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace NHExt.Runtime.GAIA
{
    class QueryParser : NHExt.Runtime.Query.QueryParser
    {
        private bool IgnoreDataFilter { get; set; }
        //获取实体缓存
        static NHExt.Runtime.Cache.AbstractCache<string, EntityAttribute.BussinesAttribute> dacOrgFilter;
        public QueryParser(string hql)
            : base(hql)
        {

        }

        protected override string decodeHqlExtend()
        {
            //获取数据权限
            string hqlExtend = "";

            ///只对于最后一个实体进行过滤
            NHExt.Runtime.Query.QueryTpl tpl = this.entityList[this.entityList.Count - 1];
            tpl.Hql = "1=1";
            ///调用权限服务调获取权限片段
            if (!this.IgnoreDataFilter)
            {
                tpl.Hql += " and(1!=1";
                try
                {
                    NHExt.Runtime.Cache.AbstractCache<string, string> dac = NHExt.Runtime.GAIA.Cache.CacheFactory.GetDataAuthCache();
                    if (!dac.Contains(tpl.EntityName))
                    {
                        NHExt.Runtime.Proxy.AgentInvoker invoker = new NHExt.Runtime.Proxy.AgentInvoker();
                        string proxy = NHExt.Runtime.Cfg.GetCfg<string>("DataAuthProxy");
                        if (!string.IsNullOrEmpty(proxy))
                        {
                            invoker.AssemblyName = proxy;
                            invoker.DllName = proxy.Substring(0, proxy.LastIndexOf(".")) + ".dll";
                        }
                        else
                        {
                            invoker.AssemblyName = "IWEHAVE.ERP.Auth.ServiceBP.Agent.GetDataAuthBPProxy";
                            invoker.DllName = "IWEHAVE.ERP.Auth.ServiceBP.Agent.dll";
                        }
                        invoker.AppendField(new NHExt.Runtime.Proxy.PropertyField() { FieldName = "EntityName", FieldValue = tpl.EntityName });
                        string hql = invoker.Do<string>();
                        //写入缓存
                        dac.SetCache(tpl.EntityName, hql);
                    }
                    string cacheHql = dac.GetCache(tpl.EntityName);
                    if (!string.IsNullOrEmpty(cacheHql))
                    {
                        tpl.Hql += " or (" + cacheHql + ")";
                    }
                    else
                    {
                        tpl.Hql += " or (1=1) ";
                    }
                }
                catch (NHExt.Runtime.Exceptions.BizException ex)
                {
                    NHExt.Runtime.Logger.LoggerHelper.Error(ex, NHExt.Runtime.Logger.LoggerInstance.BizLogger);
                    throw new NHExt.Runtime.Exceptions.RuntimeException("调用权限服务获取数据过滤条件错误：" + ex.Message);
                }

                tpl.Hql += ")";

            }
            tpl.Hql += " and (1=1";

            //获取实体多组织过滤标识
            NHExt.Runtime.EntityAttribute.BussinesAttribute ba = this.getEntityBA(tpl.EntityName);
            ///组织过滤和多组织查询过滤,如果当前登录组织委超级组织的话需要进行特殊处理
            AuthContext authCtx = NHExt.Runtime.Session.SessionCache.Current.AuthContext as AuthContext;
            if (authCtx.Org != 1 && authCtx.Org > 0)
            {

                if (ba.OrgFilter)
                {
                    string alias = string.IsNullOrEmpty(tpl.Alias) ? "" : (tpl.Alias + ".");
                    //如果是组织过滤数据
                    if (!NHExt.Runtime.Session.Session.Current.IgnoreRootOrgFilter)
                    {
                        tpl.Hql += " and " + alias + "Orgnization=" + authCtx.Org;
                    }
                    if (!NHExt.Runtime.Session.Session.Current.IgnoreOrgFilter)
                    {
                        if (ba.Range == NHExt.Runtime.Enums.ViewRangeEnum.ALL)
                        {
                            // tpl.Hql += " and 1=1";
                        }
                        else if (ba.Range == NHExt.Runtime.Enums.ViewRangeEnum.UPPER)
                        {
                            tpl.Hql += " and " + alias + "OrgnizationC in(-1";
                            foreach (long dataOrg in authCtx.DataOrgList)
                            {
                                tpl.Hql += ("," + dataOrg);
                            }
                            tpl.Hql += ")";
                        }
                        else if (ba.Range == NHExt.Runtime.Enums.ViewRangeEnum.OWN)
                        {
                            tpl.Hql += " and " + alias + "OrgnizationC=" + authCtx.OrgC;

                        }
                    }

                }
            }
            tpl.Hql += " )";
            NHExt.Runtime.Session.Session.Current.IgnoreRootOrgFilter = false;
            NHExt.Runtime.Session.Session.Current.IgnoreOrgFilter = false;
            NHExt.Runtime.Session.Session.Current.IgnoreDataFilter = false;

            //获取当前实体在当前登录用户的限制条件
            tpl.Parser();
            if (!string.IsNullOrEmpty(tpl.ParserHql))
            {
                hqlExtend += " ( " + tpl.ParserHql + " ) ";
            }

            return hqlExtend;
        }
        /// <summary>
        /// 获取实体属性标识
        /// </summary>
        /// <param name="entityKey"></param>
        /// <returns></returns>
        private NHExt.Runtime.EntityAttribute.BussinesAttribute getEntityBA(string entityKey)
        {
            if (QueryParser.dacOrgFilter == null)
            {
                QueryParser.dacOrgFilter = NHExt.Runtime.GAIA.Cache.CacheFactory.GetBACache();
                lock (QueryParser.dacOrgFilter)
                {
                    try
                    {
                        NHExt.Runtime.Proxy.AgentInvoker invoker = new NHExt.Runtime.Proxy.AgentInvoker();

                        string proxy = NHExt.Runtime.Cfg.GetCfg<string>("EntityStrProxy");
                        if (!string.IsNullOrEmpty(proxy))
                        {
                            invoker.AssemblyName = proxy;
                            invoker.DllName = proxy.Substring(0, proxy.LastIndexOf(".")) + ".dll";
                        }
                        else
                        {
                            invoker.AssemblyName = "IWEHAVE.ERP.Auth.ServiceBP.Agent.GetEntityStrBPProxy";
                            invoker.DllName = "IWEHAVE.ERP.Auth.ServiceBP.Agent.dll";
                        }
                        List<string> entityStrList = invoker.Do<List<string>>();
                        QueryParser.dacOrgFilter.ClearAll();
                        if (entityStrList != null)
                        {
                            foreach (string entityStr in entityStrList)
                            {
                                string[] entityStrArray = entityStr.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                                NHExt.Runtime.EntityAttribute.BussinesAttribute attribute = new EntityAttribute.BussinesAttribute();
                                attribute.EntityName = entityStrArray[0];
                                attribute.Table = entityStrArray[1];
                                attribute.OrgFilter = entityStrArray[2] == "1" ? true : false;
                                attribute.Range = (NHExt.Runtime.Enums.ViewRangeEnum)Convert.ToInt32(entityStrArray[3]);

                                QueryParser.dacOrgFilter.SetCache(attribute.EntityName, attribute);
                            }
                        }

                    }
                    catch (Exception ex)
                    {
                        NHExt.Runtime.Logger.LoggerHelper.Info(ex, NHExt.Runtime.Logger.LoggerInstance.RuntimeLogger);

                        string assemblyStr = entityKey.Substring(0, entityKey.LastIndexOf("."));
                        Assembly ass = Assembly.LoadFrom(NHExt.Runtime.Cfg.AppLibPath + assemblyStr + ".dll");
                        Type t = ass.GetType(entityKey);
                        if (t != null)
                        {
                            NHExt.Runtime.EntityAttribute.BussinesAttribute attribute = NHExt.Runtime.Util.AttributeHelper.GetClassAttr<NHExt.Runtime.EntityAttribute.BussinesAttribute>(t);
                            QueryParser.dacOrgFilter.SetCache(entityKey, attribute);
                        }
                    }
                }
            }
            NHExt.Runtime.EntityAttribute.BussinesAttribute ba = QueryParser.dacOrgFilter.GetCache(entityKey);
            if (ba == null)
            {
                string assembly = entityKey.Substring(0, entityKey.LastIndexOf(".")) + ".dll";
                Assembly asmb = Assembly.LoadFrom(NHExt.Runtime.Cfg.AppLibPath + assembly);
                if (asmb != null)
                {
                    Type supType = asmb.GetType(entityKey);
                    if (supType == null)
                    {
                        throw new NHExt.Runtime.Exceptions.RuntimeException("实体类型不存在：" + entityKey);
                    }
                    ba = NHExt.Runtime.Util.AttributeHelper.GetClassAttr<NHExt.Runtime.EntityAttribute.BussinesAttribute>(supType);
                }
                else
                {
                    throw new NHExt.Runtime.Exceptions.RuntimeException("实体“" + entityKey + "”所在的程序集不存在");
                }
            }
            return ba;
        }

        public override string ParseHql(params object[] args)
        {
            if (args.Length == 0)
            {
                throw new NHExt.Runtime.Exceptions.RuntimeException("ParseHql函数调用错误，该函数参数不能为空");
            }
            this.IgnoreDataFilter = (bool)args[0];
            if (!NHExt.Runtime.Cfg.GetCfg<bool>("IsDataAuth"))
            {
                this.IgnoreDataFilter = true;
            }
            //如果需要数据权限并且当前session中也需要数据权限的话则走数据权限
            if (!this.IgnoreDataFilter && NHExt.Runtime.Session.Session.Current.IsDataAuth)
            {
                this.IgnoreDataFilter = false;
            }
            else
            {
                this.IgnoreDataFilter = true;
            }
            return base.ParseHql(args);
        }
    }
}
