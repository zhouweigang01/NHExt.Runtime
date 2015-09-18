using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;

namespace NHExt.Runtime.Model
{
    [ServiceContract]
    [XmlSerializerFormat]
    public interface IBizProxy
    {
        object Do(NHExt.Runtime.Proxy.ProxyContext context);
        [OperationContract]
        WCFCallDTO DoWCF(NHExt.Runtime.Proxy.ProxyContext context);
        string Guid { get; }
        string ProxyName { get; }
        NHExt.Runtime.Session.CallerTypeEnum CallerType { get; }

        void SetValue(object obj, string memberName);
        bool? UseReadDB { get; set; }
    }

    public abstract class BizProxy : IBizProxy
    {
        public abstract object Do(NHExt.Runtime.Proxy.ProxyContext context);
        [OperationContract]
        public abstract WCFCallDTO DoWCF(NHExt.Runtime.Proxy.ProxyContext context);
        public abstract string Guid { get; }
        public string SourcePage { get; set; }
        public abstract string ProxyName { get; }
        /// <summary>
        /// 运行时动态修改是否使用Task数据库
        /// 如果使用则为true，不使用为false
        /// 如果根据上下文动态获取设置为null
        /// </summary>
        public virtual bool? UseReadDB { get; set; }

        public abstract NHExt.Runtime.Session.CallerTypeEnum CallerType { get; }
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

        protected virtual void InitParameter(NHExt.Runtime.Proxy.ProxyContext ctx)
        {
            if (NHExt.Runtime.Session.Session.Current != null)
            {
                NHExt.Runtime.Session.Session.Current.IsDataAuth = ctx.IsDataAuth;
                NHExt.Runtime.Session.SessionCache.Current.ProxyStack = ctx.ProxyStack;
                if (ctx.ProxyStack.Count == 0)
                {
                    ctx.CallerType = this.CallerType;

                }
                NHExt.Runtime.Session.SessionCache.Current.CallerType = ctx.CallerType;

                NHExt.Runtime.Auth.AuthContext authCtx = NHExt.Runtime.Auth.AuthContext.GetInstance();
                if (!string.IsNullOrEmpty(ctx.AuthContext))
                {
                    authCtx.FromString(ctx.AuthContext);
                }
                NHExt.Runtime.Session.SessionCache.Current.AuthContext = authCtx;
                NHExt.Runtime.Session.SessionCache.Current.AuthContext.RemoteIP = ctx.RemoteIP;
                NHExt.Runtime.Session.SessionCache.Current.AuthContext.SourcePage = ctx.SourcePage;
            }
        }
    }

    public class WCFCallDTO
    {
        public bool Success { get; set; }
        public string Result { get; set; }
    }
}
