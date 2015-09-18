using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.ServiceModel;
using System.Text;
using NHExt.Runtime.Model;
using NHExt.Runtime.Session;
using System.IO;
using System.Xml;



namespace WCFServiceServer
{
    class Program
    {
        static void Main(string[] args)
        {
            DateTime dtBegin = DateTime.Now;
            if (NHExt.Runtime.Logger.LoggerInstance.RuntimeLogger.IsInfoEnabled)
            {
                NHExt.Runtime.Logger.LoggerHelper.Info("服务开始启动，启动时间:" + dtBegin.ToString("yyyy-MM-dd hh:mm:ss fff"), NHExt.Runtime.Logger.LoggerInstance.RuntimeLogger);
            }
            else
            {
                Console.WriteLine("服务开始启动，启动时间:" + dtBegin.ToString("yyyy-MM-dd hh:mm:ss fff"));
            }
            //#if DEBUG
            //    Console.ReadLine();
            //#endif
            HostManager hostManager = new HostManager();
            hostManager.Init();
            hostManager.OpenHost();
            DateTime dtEnd = DateTime.Now;
            TimeSpan delayTime = dtEnd - dtBegin;
            if (NHExt.Runtime.Logger.LoggerInstance.RuntimeLogger.IsInfoEnabled)
            {
                NHExt.Runtime.Logger.LoggerHelper.Info("服务启动成功,时间" + dtEnd.ToString("yyyy-MM-dd hh:mm:ss fff") + ",耗时:" + delayTime.Milliseconds + "毫秒", NHExt.Runtime.Logger.LoggerInstance.RuntimeLogger);
            }
            else
            {
                Console.WriteLine("服务启动成功,时间" + dtEnd.ToString("yyyy-MM-dd hh:mm:ss fff") + ",耗时:" + delayTime.Milliseconds + "毫秒");
            }
            Program.CreateCacheThead();
            //关闭服务
            Console.ReadLine();
            Program.DestroyCacheThead();

            dtBegin = DateTime.Now;
            if (NHExt.Runtime.Logger.LoggerInstance.RuntimeLogger.IsInfoEnabled)
            {
                NHExt.Runtime.Logger.LoggerHelper.Info("开始退出程序，退出时间:" + dtBegin.ToString("yyyy-MM-dd hh:mm:ss fff"), NHExt.Runtime.Logger.LoggerInstance.RuntimeLogger);
            }
            else
            {
                Console.WriteLine("开始退出程序，退出时间:" + dtBegin.ToString("yyyy-MM-dd hh:mm:ss fff"));
            }
            hostManager.CloseHost();
            dtEnd = DateTime.Now;
            delayTime = dtEnd - dtBegin;
            if (NHExt.Runtime.Logger.LoggerInstance.RuntimeLogger.IsInfoEnabled)
            {
                NHExt.Runtime.Logger.LoggerHelper.Info("服务退出成功,时间" + dtEnd.ToString("yyyy-MM-dd hh:mm:ss fff") + ",耗时:" + delayTime.Milliseconds + "毫秒", NHExt.Runtime.Logger.LoggerInstance.RuntimeLogger);
            }
            else
            {
                Console.WriteLine("服务退出成功,时间" + dtEnd.ToString("yyyy-MM-dd hh:mm:ss fff") + ",耗时:" + delayTime.Milliseconds + "毫秒");
            }
        }

        //private static void CollectGC()
        //{
        //    while (true)
        //    {
        //        System.Threading.Thread.Sleep(60 * 1000);
        //        GC.Collect();
        //        NHExt.Runtime.Util.MsgHelper.Info("数据收集完成");
        //    }
        //}
        private static System.Threading.Thread _refreshCacheThead = null;
        private static bool _running = true;
        private static void RefreshCacheThead()
        {
            while (_running)
            {
                System.Threading.Thread.Sleep(new TimeSpan(0, 11, 0));
                NHExt.Runtime.Auth.AuthContext.RefreshCache();

            }
        }
        private static void CreateCacheThead()
        {
            _refreshCacheThead = new System.Threading.Thread(new System.Threading.ThreadStart(Program.RefreshCacheThead));
            _refreshCacheThead.IsBackground = true;
            _refreshCacheThead.Start();
        }
        private static void DestroyCacheThead()
        {
            _running = false;
            try
            {
                if (_refreshCacheThead != null && _refreshCacheThead.IsAlive)
                {
                    _refreshCacheThead.Abort();
                }
                _refreshCacheThead = null;
            }
            catch { }
        }

    }
}

