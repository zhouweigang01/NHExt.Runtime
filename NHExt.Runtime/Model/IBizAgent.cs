using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NHExt.Runtime.Model
{
    public interface IBizAgent
    {
        string Guid { get; }
        string ProxyName { get; }
        object DoProxy();
        void SetValue(object obj, string memberName);
        string RemoteIP { get; set; }
        string SourcePage { get; set; }
        bool? UseReadDB { get; set; }
        bool IsTask { get; set; }
        bool AutoRun { get; set; }
    }

    public abstract class BizAgent : IBizAgent
    {
        protected NHExt.Runtime.Proxy.ProxyInvoker invoker = null;
        public BizAgent()
        {
            this.RemoteIP = this.GetRemoteIP();
            this.invoker = new NHExt.Runtime.Proxy.ProxyInvoker();
            this.invoker.Guid = this.Guid;
            this.invoker.Ctx = NHExt.Runtime.Auth.AuthContext.GetInstance();
            if (NHExt.Runtime.Session.Session.Current != null)
            {
                this.invoker.UseReadDB = NHExt.Runtime.Session.SessionCache.Current.UseReadDB;
            }
        }
        public NHExt.Runtime.Auth.AuthContext Ctx { get; set; }
        public string RemoteIP { get; set; }
        public string SourcePage { get; set; }
        public abstract string Guid { get; }
        public bool IsTask
        {
            get
            {
                return this.invoker.IsTask;
            }
            set
            {
                this.invoker.IsTask = value;
            }
        }
        public bool AutoRun
        {
            get
            {
                return this.invoker.AutoRun;
            }
            set
            {
                this.invoker.AutoRun = value;
            }
        }
        public bool? UseReadDB
        {
            get
            {
                return this.invoker.UseReadDB;
            }
            set
            {
                this.invoker.UseReadDB = value;
            }
        }
        public abstract string ProxyName { get; }

        public abstract object DoProxy();
        public virtual void SetValue(object obj, string memberName)
        {
            Type type = this.GetType();
            System.Reflection.PropertyInfo pi = type.GetProperty(memberName);
            if (pi != null)
            {
                //调用序列化函数，序列化json对象
                pi.SetValue(this, obj, null);
            }
        }
        protected T TransferValue<T>(object obj)
        {
            if (obj == null) return default(T);
            return (T)obj;
        }
        /// <summary>
        /// 获取远程调用地址
        /// </summary>
        /// <returns></returns>
        protected string GetRemoteIP()
        {
            ///存在session环境的话从session里面获取
            ///不存在的话就从本地获取
            string remoteIP = string.Empty;
            if (NHExt.Runtime.Session.Session.Current != null)
            {
                Auth.AuthContext authCtx = NHExt.Runtime.Session.SessionCache.Current.AuthContext;
                if (authCtx != null && !string.IsNullOrEmpty(authCtx.RemoteIP))
                {
                    remoteIP = authCtx.RemoteIP;
                }
                else
                {
                    remoteIP = this.getLocalIP(System.Web.HttpContext.Current);
                }
            }
            else
            {
                remoteIP = this.getLocalIP(System.Web.HttpContext.Current);
            }
            return remoteIP;

        }
        /// <summary>
        /// 获取本地地址
        /// </summary>
        /// <param name="ctx"></param>
        /// <returns></returns>
        private string getLocalIP(System.Web.HttpContext ctx)
        {
            string remoteIP = "127.0.0.1";
            if (ctx == null)
            {
                ctx = System.Web.HttpContext.Current;
            }
            if (ctx != null)
            {
                System.Web.HttpRequest req = ctx.Request;
                if (req != null)
                {
                    remoteIP = req.UserHostAddress;
                }
            }
            else
            {
                System.Net.IPHostEntry ipe = System.Net.Dns.GetHostEntry(System.Net.Dns.GetHostName());
                foreach (System.Net.IPAddress ipa in ipe.AddressList)
                {
                    //IPV4
                    if (ipa.AddressFamily.ToString() == "InterNetwork")
                    {
                        remoteIP = ipa.ToString();
                        break;
                    }
                }

            }
            return remoteIP;
        }
    }
}
