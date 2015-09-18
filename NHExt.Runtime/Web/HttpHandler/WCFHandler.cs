using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using Newtonsoft.Json.Linq;
using System.Reflection;
using System.Collections.Specialized;

namespace NHExt.Runtime.Web.HttpHandler
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
    }
}
