using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Collections.Specialized;
using Newtonsoft.Json.Linq;
using System.Reflection;

namespace NHExt.Runtime.Web.HttpHandler
{
    public abstract class AbstractHttpHander : System.Web.IHttpHandler
    {
        public virtual bool IsReusable
        {
            get { return true; }
        }
        public virtual void ProcessRequest(System.Web.HttpContext context)
        {
            this.doRequest(context);
        }
        private void doRequest(HttpContext ctx)
        {
            Encoding encoding = ctx.Request.ContentEncoding;
            ctx.Response.BufferOutput = true;
            DirectResponse responseMsg = new DirectResponse();
            try
            {
                Logger.LoggerInstance.RuntimeLogger.Info("开始解析请求");
                DirectRequest dr = GetDirectRequest(ctx.Request);
                Logger.LoggerInstance.RuntimeLogger.Info("开始调用服务");

                System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
                sw.Start();
                responseMsg.Data = invokeProxy(dr, ctx);
                sw.Stop();
                //计算前端调用事件
                NHExt.Runtime.Logger.LoggerHelper.Info("前端调用" + dr.Action + "服务,耗费时间：" + sw.ElapsedMilliseconds, NHExt.Runtime.Logger.LoggerInstance.BizLogger);
            }
            catch (NHExt.Runtime.Exceptions.BizException ex)
            {
                NHExt.Runtime.Logger.LoggerHelper.Error(ex, NHExt.Runtime.Logger.LoggerInstance.BizLogger);

                responseMsg.Success = false;
                responseMsg.ResultMsg = ex.Message;
                responseMsg.ResultDetailMsg = ex.StackTrace;
            }
            catch (NHExt.Runtime.Exceptions.RuntimeException ex)
            {
                NHExt.Runtime.Logger.LoggerHelper.Error(ex, NHExt.Runtime.Logger.LoggerInstance.RuntimeLogger);

                responseMsg.Success = false;
                responseMsg.ResultMsg = ex.Message;
                responseMsg.ResultDetailMsg = ex.StackTrace;
            }

            catch (Exception ex)
            {
                NHExt.Runtime.Logger.LoggerHelper.Error(ex, NHExt.Runtime.Logger.LoggerInstance.RuntimeLogger);
                if (ex.InnerException != null)
                {
                    NHExt.Runtime.Logger.LoggerHelper.Error(ex, NHExt.Runtime.Logger.LoggerInstance.RuntimeLogger);
                    ex = ex.InnerException;
                }
                responseMsg.Success = false;
                responseMsg.ResultMsg = ex.Message;
                responseMsg.ResultDetailMsg = ex.StackTrace;
            }
            finally
            {
                this.RenderClient(ctx.Response, responseMsg);
            }

        }

        private object invokeProxy(DirectRequest dr, System.Web.HttpContext ctx = null)
        {
            NHExt.Runtime.Proxy.AgentInvoker invoker = null;
            Logger.LoggerInstance.RuntimeLogger.Info("创建服务实例对象");
            try
            {
                invoker = new Proxy.AgentInvoker();
                invoker.IsTask = dr.IsTask;
                invoker.DllName = dr.Assembly;
                invoker.AssemblyName = dr.Action;
                invoker.SourcePage = dr.SourcePage;
                Logger.LoggerInstance.RuntimeLogger.Info("初始化服务参数");
                foreach (JToken token in dr.Args)
                {
                    JProperty jp = token as JProperty;

                    if (token.Type != JTokenType.Null)
                    {
                        invoker.AppendField(new Proxy.PropertyField() { FieldName = jp.Name, FieldValue = jp });
                    }
                }
            }
            catch
            {
                NHExt.Runtime.Exceptions.RuntimeException ex = new NHExt.Runtime.Exceptions.RuntimeException("初始化服务参数失败,服务名称:" + invoker.AssemblyName);
                throw ex;
            }
            object obj = null;
            if (invoker != null)
            {
                Logger.LoggerHelper.Info("开始调用服务代理Do方法");
                this.BeforeInvokeProxy(invoker, dr, ctx);
                obj = invoker.Do();
                this.AfterInvokeProxy(invoker, obj, ctx);
                Logger.LoggerHelper.Info("调用服务代理Do方法成功，开始返回返回值");
            }

            return obj;
        }

        #region 初始化请求参数
        private DirectRequest GetDirectRequest(HttpRequest request)
        {
            string contentType = request.ContentType;
            DirectRequest dr = null;
            if (string.Compare(request.HttpMethod, "POST", true) == 0)
            {
                dr = this.CreateDirectParameterByForm(request);
            }
            else
            {
                dr = this.CreateDirectParameterByQuery(request);
            }
            Logger.LoggerInstance.RuntimeLogger.Info("请求信息解析完成，请求信息为:");
            Logger.LoggerInstance.RuntimeLogger.Info("action:" + dr.Action);
            Logger.LoggerInstance.RuntimeLogger.Info("assembly:" + dr.Assembly);
            return dr;
        }
        private DirectRequest CreateDirectParameterByQuery(HttpRequest request)
        {
            NameValueCollection nvc = request.QueryString;
            DirectRequest dp = DirectRequest.GetInstance();
            return this.CreateDirectParameter(dp, nvc);
        }
        private DirectRequest CreateDirectParameterByForm(HttpRequest request)
        {
            NameValueCollection nvc = request.Form;
            DirectRequest dp = DirectRequest.GetInstance();
            return CreateDirectParameter(dp, nvc);
        }

        protected virtual DirectRequest CreateDirectParameter(DirectRequest req, NameValueCollection nvc)
        {
            string asyncTask = nvc["async"];
            if (!string.IsNullOrEmpty(asyncTask))
            {
                req.IsTask = true;
            }
            string uiActionName = nvc["action"].Trim();
            string uiAssembly = uiActionName.Substring(0, uiActionName.LastIndexOf("."));
            string strArgs = nvc["args"];
            if (string.IsNullOrEmpty(strArgs))
            {
                strArgs = "{}";
            }
            JToken jToken = JToken.Parse(strArgs);

            //这个里面添加的肯定都是属性
            foreach (JToken t in jToken.Children())
            {
                req.Args.Add(t);
            }
            req.Action = uiActionName;
            req.Assembly = uiAssembly + ".dll";
            this.CreateDirectParameterExtend(req, nvc);
            this.ValidateDirectParameter(req);
            Logger.LoggerInstance.RuntimeLogger.Info("请求解析完成action：" + req.Action + "程序集为:" + req.Assembly);
            return req;
        }


        #endregion

        protected virtual void CreateDirectParameterExtend(DirectRequest req, NameValueCollection nvc)
        {

        }
        protected virtual void ValidateDirectParameter(DirectRequest req)
        {
            if (string.IsNullOrEmpty(req.Action))
            {
                throw new NHExt.Runtime.Exceptions.RuntimeException("调用服务名称不能为空");
            }
            if (string.IsNullOrEmpty(req.Assembly))
            {
                throw new NHExt.Runtime.Exceptions.RuntimeException("调用程序集不能为空");
            }
        }
        protected virtual void CreateAgentParameterExtend(Model.IBizAgent agent, DirectRequest dr)
        {

        }
        protected virtual void RenderClient(HttpResponse response, DirectResponse responseMsg)
        {
            response.Write(NHExt.Runtime.Serialize.JsonSerialize.Serialize(responseMsg));
        }

        protected virtual void BeforeInvokeProxy(NHExt.Runtime.Proxy.AgentInvoker invoker, DirectRequest dr, HttpContext ctx) { }
        protected virtual void AfterInvokeProxy(NHExt.Runtime.Proxy.AgentInvoker invoker, object obj, HttpContext ctx) { }
    }
}
