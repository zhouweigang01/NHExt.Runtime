using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;
using System.Collections;

namespace NHExt.Runtime
{
    public class Cfg
    {
        #region 内部配置数据

        private Hashtable cfgCache = new Hashtable();
        private static Cfg _instance = null;
        #endregion
        /// <summary>
        /// 当前Cfg初始化实例对象
        /// </summary>
        protected static Cfg Instance
        {
            get
            {
                return Cfg._instance;
            }
        }
        /// <summary>
        /// 设置当前配置的值
        /// </summary>
        /// <param name="key"></param>
        /// <param name="obj"></param>
        public static void SetCfg(string key, object obj)
        {
            Cfg.Instance.Set(key, obj);
        }
        /// <summary>
        /// 获取当前配置值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public static T GetCfg<T>(string key)
        {
            return Cfg.Instance.Get<T>(key);
        }
        /// <summary>
        /// 设置配置值
        /// </summary>
        /// <param name="key"></param>
        /// <param name="obj"></param>
        protected void Set(string key, object obj)
        {
            if (!this.cfgCache.ContainsKey(key))
            {
                this.cfgCache.Add(key, obj);
            }
            else
            {
                this.cfgCache[key] = obj;
            }
        }
        /// <summary>
        /// 获取配置值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        protected T Get<T>(string key)
        {
            if (!this.cfgCache.ContainsKey(key))
            {
                return default(T);
            }
            else
            {
                return (T)this.cfgCache[key];
            }
        }

        /// <summary>
        /// 运行目录
        /// </summary>
        public static string AppPath { get; set; }
        /// <summary>
        /// 配置文件放置目录
        /// </summary>
        public static string AppCfgPath { get; set; }

        /// <summary>
        /// ApplicationLib目录
        /// </summary>
        public static string AppLibPath { get; set; }
        /// <summary>
        /// ApplicationLib的bin目录
        /// </summary>
        public static string AppBinPath { get; set; }
        /// <summary>
        /// 实体目录
        /// </summary>
        public static string EntityPath { get; set; }
        /// <summary>
        /// 插件目录
        /// </summary>
        public static string PlugInPath { get; set; }

        public static string HostUrl { get; set; }

        /// <summary>
        /// 最大线程数
        /// </summary>
        public static int MaxThreadCount { get; set; }
        static Cfg()
        {
            Cfg.AppPath = AppDomain.CurrentDomain.BaseDirectory;
            if (Directory.Exists(Cfg.AppPath + @"Config\"))
            {
                Cfg.AppCfgPath = Cfg.AppPath + @"Config\";
            }
            else
            {
                Cfg.AppCfgPath = Cfg.AppPath;
            }
            if (Directory.Exists(Cfg.AppPath + @"bin\"))
            {
                Cfg.AppBinPath = Cfg.AppPath + @"bin\";
            }
            else
            {
                Cfg.AppBinPath = Cfg.AppPath;
            }
            if (Directory.Exists(Cfg.AppPath + @"ApplicationLib\"))
            {
                Cfg.AppLibPath = Cfg.AppPath + @"ApplicationLib\";
            }
            else
            {
                Cfg.AppLibPath = Cfg.AppPath;
            }
            if (Directory.Exists(Cfg.AppPath + @"EntityLib\"))
            {
                Cfg.EntityPath = Cfg.AppPath + @"EntityLib\";
            }
            else
            {
                Cfg.EntityPath = Cfg.AppPath;
            }
            if (Directory.Exists(Cfg.AppLibPath + @"PlugIn\"))
            {
                Cfg.PlugInPath = Cfg.AppLibPath + @"PlugIn\";
            }
            else
            {
                Cfg.PlugInPath = Cfg.AppPath + @"PlugIn\";
            }

            if (File.Exists(Cfg.AppCfgPath + "LOG_CONFIG.xml"))
            {
                log4net.Config.XmlConfigurator.ConfigureAndWatch(new System.IO.FileInfo(NHExt.Runtime.Cfg.AppCfgPath + "LOG_CONFIG.xml"));
                Logger.LoggerHelper.Info("配置文件LOG_CONFIG.xml加载完成", NHExt.Runtime.Logger.LoggerInstance.AppLogger);
            }

            if (File.Exists(Cfg.AppCfgPath + "NH_CONFIG.xml"))
            {
                NHExt.Runtime.Session.SessionCache.MainCfg.Configure(NHExt.Runtime.Cfg.AppCfgPath + "NH_CONFIG.xml");
                Logger.LoggerHelper.Info("配置文件NH_CONFIG.xml加载完成", NHExt.Runtime.Logger.LoggerInstance.AppLogger);
            }
            else
            {
                Logger.LoggerHelper.Error("配置文件HibernateConfig.xml不存在", NHExt.Runtime.Logger.LoggerInstance.AppLogger);

            }
            if (File.Exists(Cfg.AppCfgPath + "NH_READ_CONFIG.xml"))
            {
                NHExt.Runtime.Session.SessionCache.ReadCfg.Configure(NHExt.Runtime.Cfg.AppCfgPath + "NH_READ_CONFIG.xml");
                Logger.LoggerHelper.Info("配置文件NH_READ_CONFIG.xml加载完成", NHExt.Runtime.Logger.LoggerInstance.AppLogger);
            }
            else
            {
                NHExt.Runtime.Session.SessionCache.ReadCfg = NHExt.Runtime.Session.SessionCache.MainCfg;
            }

            Cfg.MaxThreadCount = 5;

        }

        /// <summary>
        /// 应用程序运行目录
        /// </summary>
        public static string RootPath
        {
            get
            {
                string path = AppDomain.CurrentDomain.BaseDirectory;
                if (System.Web.HttpContext.Current == null)
                {
                    path = Directory.GetParent(path).Parent.FullName;
                }
                if (path[path.Length - 1] == '\\')
                {
                    path = path.Substring(0, path.Length - 1);
                }
                return path;
            }
        }

        public static Cfg GetInstance()
        {
            Cfg cfg = NHExt.Runtime.Util.IocFactory.GetIocObject<Cfg>("params_extend");
            if (cfg == null)
            {
                cfg = new Cfg();
            }
            return cfg;
        }

        /// <summary>
        /// 初始化nhibernate，如果配置了路径的话就自动加载，如果没有配置路径需要手动加载程序集信息
        /// </summary>
        public static void Init()
        {
            Cfg._instance = NHExt.Runtime.Cfg.GetInstance();
            //初始化WCF相关配置
            NHExt.Runtime.Proxy.InvokerFactory.Init();
            //初始化AOP相关配置
            NHExt.Runtime.AOP.AspectManager.Init();
            ///加载程序集信息
            NHExt.Runtime.Session.SessionCache.Init();
            //初始化SVC文件
            NHExt.Runtime.Proxy.Host.WCFHostFactory.Init();
        }
    }



}
