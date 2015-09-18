using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.IO;

namespace SyncTask
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                if (File.Exists(NHExt.Runtime.Cfg.AppCfgPath + "LOG_CONFIG.xml"))
                {
                    log4net.Config.XmlConfigurator.ConfigureAndWatch(new System.IO.FileInfo(NHExt.Runtime.Cfg.AppCfgPath + "LOG_CONFIG.xml"));
                    NHExt.Runtime.Logger.LoggerInstance.BizLogger.Info("配置文件LogCfg.xml加载完成");
                }
                //由于本地闲置该方法只能调用WCF服务来进行更新操作
                //初始化WCF相关配置
                NHExt.Runtime.Proxy.InvokerFactory.Init();

                if (System.Configuration.ConfigurationManager.AppSettings["Debug"] == "1")
                {
                    Console.ReadLine();
                }

                List<TaskCategory> taskCategoryList = TaskCategory.GetCategoryList();


                foreach (TaskCategory tc in taskCategoryList)
                {
                    DateTime dtNow = DateTime.Now;
                    if (tc.Begin == null && tc.End == null)
                    {
                        tc.ExcuteTasks();
                    }
                    else if (tc.Begin == null)
                    {
                        if (tc.End >= dtNow.Hour)
                        {
                            tc.ExcuteTasks();
                        }
                    }
                    else if (tc.End == null)
                    {
                        if (tc.Begin <= dtNow.Hour)
                        {
                            tc.ExcuteTasks();
                        }
                    }
                    else
                    {
                        if (tc.Begin <= dtNow.Hour && tc.End >= dtNow.Hour)
                        {
                            tc.ExcuteTasks();
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                NHExt.Runtime.Logger.LoggerHelper.Error(ex, NHExt.Runtime.Logger.LoggerInstance.RuntimeLogger);
            }

            //Console.ReadLine();
        }


    }
}
