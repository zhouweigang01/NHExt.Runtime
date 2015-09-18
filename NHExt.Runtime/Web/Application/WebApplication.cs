using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Reflection;
using System.Xml;
using System.IO;

namespace NHExt.Runtime.Web
{
    public class WebApplication : System.Web.HttpApplication
    {
        void Application_Start(object sender, EventArgs e)
        {
            try
            {
                NHExt.Runtime.Logger.LoggerHelper.Info("开始初始化应用程序配置", NHExt.Runtime.Logger.LoggerInstance.AppLogger);
                NHExt.Runtime.Cfg.Init();
                NHExt.Runtime.Logger.LoggerHelper.Info("初始化应用程序配置完成", NHExt.Runtime.Logger.LoggerInstance.AppLogger);
            }
            catch (NHExt.Runtime.Exceptions.AppException ex)
            {
                Logger.LoggerHelper.Error(ex, NHExt.Runtime.Logger.LoggerInstance.AppLogger);
                throw ex;
            }
            catch (NHExt.Runtime.Exceptions.RuntimeException ex)
            {
                Logger.LoggerHelper.Error(ex, NHExt.Runtime.Logger.LoggerInstance.RuntimeLogger);
                throw ex;
            }


        }
        void Application_End(object sender, EventArgs e)
        {


        }

        void Application_Error(object sender, EventArgs e)
        {
            // 在出现未处理的错误时运行的代码

        }

        void Session_Start(object sender, EventArgs e)
        {


        }

        void Session_End(object sender, EventArgs e)
        {

        }


    }
}
