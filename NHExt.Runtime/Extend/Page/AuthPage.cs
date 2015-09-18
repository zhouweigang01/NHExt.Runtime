using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace NHExt.Runtime.Extend.Page
{
    public abstract class AuthPage : NHExt.Runtime.Web.Page.BasePage
    {

        public override void ProcessRequest(HttpContext context)
        {
            if (!Cfg.GetCfg<bool>("IsUIAuth"))
            {
                base.ProcessRequest(context);
            }
            else
            {
                Auth.AuthContext authCtx = null;
                try
                {
                    authCtx = Auth.AuthContext.GetContext();
                }
                catch (Exception ex)
                {
                    Logger.LoggerInstance.RuntimeLogger.Error(ex);
                }
                if (!authCtx.IsAuth() || !CheckPageAuth(authCtx))
                {
                    context.Response.Redirect("/AuthErrorPage.aspx");
                }
                else
                {
                    base.ProcessRequest(context);
                }
            }
        }
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            if (Cfg.GetCfg<bool>("IsUIAuth"))
            {
                this.RegistClientAuth();
            }
        }
        //检测当前用户是否有当前页面权限
        private bool CheckPageAuth(Auth.AuthContext ctx)
        {
            try
            {
                NHExt.Runtime.Proxy.AgentInvoker invoker = new NHExt.Runtime.Proxy.AgentInvoker();
                invoker.AssemblyName = "IWEHAVE.ERP.AuthBP.Agent.GetPageAuthByGUIDBPProxy";
                invoker.DllName = "IWEHAVE.ERP.AuthBP.Agent.dll";
                invoker.AppendField(new NHExt.Runtime.Proxy.PropertyField() { FieldName = "PageGUID", FieldValue = this.PageGuid });
                invoker.SourcePage = this.PageGuid;
                return invoker.Do<bool>();
            }
            catch (Exception ex)
            {
                NHExt.Runtime.Logger.LoggerHelper.Error(ex, NHExt.Runtime.Logger.LoggerInstance.BizLogger);
            }
            return false;
        }
        /// <summary>
        /// 这侧客户端权限脚本
        /// </summary>
        private void RegistClientAuth()
        {
            string script = "<script>";
            string key = "MenuAuth_" + this.PageGuid;
            try
            {
                NHExt.Runtime.Proxy.AgentInvoker invoker = new NHExt.Runtime.Proxy.AgentInvoker();
                invoker.AssemblyName = "IWEHAVE.ERP.AuthBP.Agent.GetMenuItemAuthByPageGUIDBPProxy";
                invoker.DllName = "IWEHAVE.ERP.AuthBP.Agent.dll";
                invoker.AppendField(new NHExt.Runtime.Proxy.PropertyField() { FieldName = "PageGUID", FieldValue = this.PageGuid });
                invoker.SourcePage = this.PageGuid;
                object obj = invoker.Do();
                if (obj != null)
                {
                    if (!Page.ClientScript.IsClientScriptBlockRegistered(key))
                    {
                        System.Web.Script.Serialization.JavaScriptSerializer jsConvertor = new System.Web.Script.Serialization.JavaScriptSerializer();
                        script += "window.$Auth=" + jsConvertor.Serialize(obj) + ";";
                        script += "$(document).ready(function(){if(window.Auth){setTimeout(function(){window.Auth.reset();},500);}});";

                    }
                }
            }
            catch (Exception ex)
            {
                NHExt.Runtime.Logger.LoggerHelper.Error(ex, NHExt.Runtime.Logger.LoggerInstance.BizLogger);
            }
            script += "</script>";
            this.ClientScript.RegisterClientScriptBlock(this.GetType(), key, script);

        }
    }
}
