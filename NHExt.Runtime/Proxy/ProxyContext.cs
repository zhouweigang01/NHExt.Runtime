using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NHExt.Runtime.Proxy
{
    [Serializable]
    public class ProxyContext
    {
        public ProxyContext()
        {

            this.ParamList = new List<object>();
            this.IsDataAuth = true;
            //默认从缓存中获取数据
            NHExt.Runtime.Auth.AuthContext authCtx = null;
            //如果外层存在session的话则从session里面取，如果不存在则从缓存里面取
            if (NHExt.Runtime.Session.Session.Current == null)
            {
                authCtx = NHExt.Runtime.Auth.AuthContext.GetInstance();
                this.ProxyStack = new List<Auth.ProxyProperty>();
                this.RemoteIP = string.Empty;
                this.SourcePage = string.Empty;
                this.CallerType = Session.CallerTypeEnum.None;
            }
            else
            {
                authCtx = NHExt.Runtime.Session.SessionCache.Current.AuthContext;
                this.ProxyStack = NHExt.Runtime.Session.SessionCache.Current.ProxyStack;
                this.CallerType = NHExt.Runtime.Session.SessionCache.Current.CallerType;
                this.RemoteIP = authCtx.RemoteIP;
                this.SourcePage = authCtx.SourcePage;
                if (NHExt.Runtime.Session.Session.Current != null)
                {
                    this.UseReadDB = NHExt.Runtime.Session.SessionCache.Current.UseReadDB;
                    this.IsDataAuth = NHExt.Runtime.Session.Session.Current.IsDataAuth;
                    //如果服务调用服务的话则不需要进行数据权限校验
                    if (this.IsDataAuth && this.ProxyStack.Count > 0)
                    {
                        if (this.CallerType == Session.CallerTypeEnum.WCF)
                        {
                            if (this.ProxyStack.Count > 1)
                            {
                                this.IsDataAuth = false;
                            }
                            else
                            {
                                this.IsDataAuth = true;
                            }
                        }
                        else
                        {
                            this.IsDataAuth = false;
                        }
                    }
                }
            }
            if (authCtx != null)
            {
                this.AuthContext = authCtx.ToString();
            }

        }
        public ProxyContext(NHExt.Runtime.Auth.AuthContext ctx)
            : this()
        {
            if (ctx != null)
            {
                this.AuthContext = ctx.ToString();
            }
        }

        /// <summary>
        /// 请求参数
        /// </summary>
        public List<object> ParamList { get; set; }

        /// <summary>
        /// 请求权限相关信息
        /// </summary>
        public string AuthContext { get; set; }

        public Session.CallerTypeEnum CallerType { get; set; }
        /// <summary>
        /// 服务调用栈
        /// </summary>
        public List<Auth.ProxyProperty> ProxyStack { get; set; }

        /// <summary>
        /// 远程是否进行权限校验
        /// </summary>
        public bool IsDataAuth { get; set; }

        /// <summary>
        /// 服务guid
        /// </summary>
        public string ProxyGuid { get; set; }
        /// <summary>
        /// 请求远程地址
        /// </summary>
        public string RemoteIP { get; set; }
        /// <summary>
        /// 请求页面编码
        /// </summary>
        public string SourcePage { get; set; }

        public bool? UseReadDB { get; set; }
    }
}
