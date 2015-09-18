using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Collections.Specialized;
using System.Reflection;
using Newtonsoft.Json.Linq;


namespace NHExt.Runtime.Extend.HttpHandler
{
    class WCFHandler : NHExt.Runtime.Web.HttpHandler.AbstractHttpHander, System.Web.SessionState.IRequiresSessionState
    {
        public override void ProcessRequest(System.Web.HttpContext context)
        {
            try
            {
                base.ProcessRequest(context);
            }
            catch (Exception ex)
            {
                Logger.LoggerInstance.RuntimeLogger.Error(ex);
            }
            finally
            {
                Auth.AuthContext.ClearContext();
            }
            Logger.LoggerInstance.RuntimeLogger.Info("前端请求处理完成");
        }
        protected override void BeforeInvokeProxy(NHExt.Runtime.Proxy.AgentInvoker invoker, NHExt.Runtime.Web.HttpHandler.DirectRequest dr, HttpContext ctx)
        {
            base.BeforeInvokeProxy(invoker, dr, ctx);
            NHExt.Runtime.Auth.AuthContext authCtx = NHExt.Runtime.Auth.AuthContextFactory.GetAuthContext();
            Type authType = authCtx.GetType();
            foreach (JToken token in dr.Auth)
            {
                JProperty jp = token as JProperty;
                PropertyInfo pi = authType.GetProperty(jp.Name);
                if (token.Type != JTokenType.Null && pi != null)
                {
                    //调用序列化函数，序列化json对象
                    var desObj = NHExt.Runtime.Serialize.JsonSerialize.DeSerialize(jp.Value, pi.PropertyType);
                    pi.SetValue(authCtx, desObj, null);
                }
            }
            if (!NHExt.Runtime.Auth.AuthContext.IsAuthenticated(ctx))
            {
                authCtx.ValidateAuth();
            }

        }
        protected override void CreateDirectParameterExtend(NHExt.Runtime.Web.HttpHandler.DirectRequest req, NameValueCollection coll)
        {
            string strAuth = coll["Auth"];
            JToken jToken = JToken.Parse(strAuth);
            foreach (JToken t in jToken.Children())
            {
                req.Auth.Add(t);
            }
        }
    }
}