using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Collections.Specialized;
using System.Reflection;
using Newtonsoft.Json.Linq;


namespace NHExt.Runtime.GAIA.HttpHandler
{
    class WCFHandler : NHExt.Runtime.Web.HttpHandler.AbstractHttpHander, System.Web.SessionState.IRequiresSessionState
    {
        public override void ProcessRequest(System.Web.HttpContext context)
        {
            try
            {
                base.ProcessRequest(context);
            }
            catch (NHExt.Runtime.Exceptions.BizException ex)
            {
                NHExt.Runtime.Logger.LoggerHelper.Error(ex, NHExt.Runtime.Logger.LoggerInstance.BizLogger);
            }
            catch (NHExt.Runtime.Exceptions.RuntimeException ex)
            {
                NHExt.Runtime.Logger.LoggerHelper.Error(ex, NHExt.Runtime.Logger.LoggerInstance.RuntimeLogger);
            }
            catch (Exception ex)
            {
                NHExt.Runtime.Logger.LoggerHelper.Error(ex, NHExt.Runtime.Logger.LoggerInstance.RuntimeLogger);
            }
            finally
            {
                Auth.AuthContext.ClearContext();
            }
            NHExt.Runtime.Logger.LoggerHelper.Info("前端请求处理完成", NHExt.Runtime.Logger.LoggerInstance.RuntimeLogger);
        }
        protected override void BeforeInvokeProxy(NHExt.Runtime.Proxy.AgentInvoker invoker, NHExt.Runtime.Web.HttpHandler.DirectRequest req, HttpContext ctx)
        {
            base.BeforeInvokeProxy(invoker, req, ctx);
            NHExt.Runtime.Auth.AuthContext authCtx = NHExt.Runtime.Auth.AuthContext.GetInstance();
            Type authType = authCtx.GetType();

            NHExt.Runtime.GAIA.HttpHandler.DirectRequest reqExtend = req as NHExt.Runtime.GAIA.HttpHandler.DirectRequest;

            foreach (JToken token in reqExtend.Auth)
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
            NHExt.Runtime.GAIA.HttpHandler.DirectRequest reqExtend = req as NHExt.Runtime.GAIA.HttpHandler.DirectRequest;
            foreach (JToken t in jToken.Children())
            {
                reqExtend.Auth.Add(t);
            }
        }
    }
}