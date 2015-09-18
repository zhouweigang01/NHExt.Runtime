using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using Newtonsoft.Json.Linq;
using System.Reflection;
using System.Collections.Specialized;

namespace NHExt.Runtime.Extend.HttpHandler
{
    class WCFCenterHandler : NHExt.Runtime.Web.HttpHandler.AbstractHttpHander, System.Web.SessionState.IRequiresSessionState
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
    }
}
