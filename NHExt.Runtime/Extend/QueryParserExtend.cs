using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace NHExt.Runtime.Extend
{
    class QueryParserExtend : NHExt.Runtime.Query.QueryParser
    {

        private NHExt.Runtime.EntityAttribute.BussinesAttribute BA { get; set; }
        private bool IsDataAuth { get; set; }


        static NHExt.Runtime.Cache.AbstractCache<string, EntityAttribute.BussinesAttribute> dac = null;
        public QueryParserExtend(string hql)
            : base(hql)
        {

        }
        protected override string decodeHqlExtend()
        {
            string hqlExtend = "";
            foreach (NHExt.Runtime.Query.QueryTpl tpl in this.entityList)
            {
                if (this.IsDataAuth)
                {
                    ///调用权限服务调获取权限片段
                    try
                    {
                        NHExt.Runtime.Cache.AbstractCache<string, string> dac = null;
                        if (NHExt.Runtime.Cfg.GetCfg<bool>("IsEnableCache"))
                        {
                            dac = NHExt.Runtime.Extend.Cache.AbstractCacheFactory.GetDataAuthCache();
                            string cacheHql = dac.GetCache(tpl.EntityName);
                            if (!string.IsNullOrEmpty(cacheHql))
                            {
                                tpl.Hql = cacheHql;
                            }
                        }
                        if (tpl.Hql == null)
                        {
                            NHExt.Runtime.Proxy.AgentInvoker invoker = new NHExt.Runtime.Proxy.AgentInvoker();
                            invoker.AssemblyName = "IWEHAVE.ERP.PubBP.Agent.GetEntityAuthDataBPProxy";
                            invoker.DllName = "IWEHAVE.ERP.PubBP.Agent.dll";
                            invoker.AppendField(new NHExt.Runtime.Proxy.PropertyField() { FieldName = "EntityName", FieldValue = tpl.EntityName });
                            tpl.Hql = invoker.Do<string>();
                            if (NHExt.Runtime.Cfg.GetCfg<bool>("IsEnableCache"))
                            {
                                if (tpl.Hql != null && dac != null)
                                {
                                    //写入缓存
                                    dac.SetCache(tpl.EntityName, tpl.Hql);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        throw new Exception("调用权限服务获取数据过滤条件错误：" + ex.Message);
                    }
                }
                ///组织过滤和多组织查询过滤,如果当前登录组织委超级组织的话需要进行特殊处理
                AuthContextExtend authCtx = NHExt.Runtime.Session.SessionCache.Current.AuthContext as AuthContextExtend;
                if (authCtx.Org != 1 && authCtx.Org > 0)
                {

                    if (this.BA.OrgFilter)
                    {
                        string alias = string.IsNullOrEmpty(tpl.Alias) ? "" : (tpl.Alias + ".");
                        //如果是组织过滤数据
                        tpl.Hql += " " + alias + "Orgnization=" + authCtx.Org;
                        if (!NHExt.Runtime.Session.Session.Current.IgnoreOrgFilter)
                        {
                            if (this.BA.Range == NHExt.Runtime.Enums.ViewRangeEnum.ALL)
                            {
                                // tpl.Hql += " and 1=1";
                            }
                            else if (this.BA.Range == NHExt.Runtime.Enums.ViewRangeEnum.UPPER)
                            {
                                tpl.Hql += " and " + alias + "OrgnizationC in(-1";
                                foreach (long dataOrg in authCtx.DataOrgList)
                                {
                                    tpl.Hql += ("," + dataOrg);
                                }
                                tpl.Hql += ")";
                            }
                            else if (this.BA.Range == NHExt.Runtime.Enums.ViewRangeEnum.OWN)
                            {
                                tpl.Hql += " and " + alias + "OrgnizationC=" + authCtx.OrgC;

                            }
                        }

                    }
                }
                NHExt.Runtime.Session.Session.Current.IgnoreOrgFilter = false;

                //获取当前实体在当前登录用户的限制条件
                tpl.Parser();
                if (!string.IsNullOrEmpty(tpl.ParserHql))
                {
                    hqlExtend += " ( " + tpl.ParserHql + " ) ";
                }
            }
            return hqlExtend;
        }

        public override string ParseHql(params object[] args)
        {
            if (args.Length == 0)
            {
                throw new Exception("参数不正确");
            }
            this.IsDataAuth = (bool)args[0];
            if (!NHExt.Runtime.Cfg.GetCfg<bool>("IsDataAuth"))
            {
                this.IsDataAuth = false;
            }
            //如果需要数据权限并且当前session中也需要数据权限的话则走数据权限
            if (this.IsDataAuth && NHExt.Runtime.Session.Session.Current.IsDataAuth)
            {
                this.IsDataAuth = true;
            }
            else
            {
                this.IsDataAuth = false;
            }
            return base.ParseHql(args);
        }
    }
}
