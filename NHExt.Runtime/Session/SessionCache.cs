
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NHibernate.Cfg;
using System.Collections;
using System.IO;
using System.Reflection;
using NHibernate;
using System.Xml;

namespace NHExt.Runtime.Session
{
    public enum CallerTypeEnum
    {
        None = 10,
        Reflect = 1,
        WCF = 2
    }

    /// <summary>
    /// 当前实体的guid
    /// </summary>
    public static class SessionGuid
    {
        private static Hashtable _hashGuid = new Hashtable();
        public static string Guid
        {
            get
            {
                string key = getKey();
                string guid = getGuid(key);
                if (guid == null)
                {
                    guid = key;
                    _hashGuid.Add(key, guid);
                }
                return guid;
            }
        }

        internal static string NewGuid()
        {
            lock (_hashGuid)
            {
                string key = getKey();
                string guid = getGuid(key);
                if (guid.IndexOf("$$") <= guid.Length - 2)
                {
                    guid += "#";
                    int length = guid.Substring(guid.IndexOf("$$") + 2).Length;
                    NHExt.Runtime.Logger.LoggerHelper.Debug("进入事务，当前层级" + length + ",GUID:" + guid);
                }

                _hashGuid[key] = guid;
                return guid;
            }
        }
        /// <summary>
        /// 回退当前GUID
        /// </summary>
        internal static void BackSpaceGuid()
        {
            lock (_hashGuid)
            {
                string key = getKey();
                string guid = getGuid(key);
                if (string.IsNullOrEmpty(guid))
                {
                    return;
                }
                var index = guid.IndexOf("$$");

                if (index > 0 && index < guid.Length - 2)
                {
                    int length = guid.Substring(guid.IndexOf("$$") + 2).Length;
                    NHExt.Runtime.Logger.LoggerHelper.Debug("退出事务，当前层级" + length + ",GUID:" + guid);
                    guid = guid.Substring(0, guid.Length - 1);
                }
                _hashGuid[key] = guid;
                if (SessionCache.GetCache(guid) == null)
                {
                    RemoveGuid(guid);
                }
            }
        }

        private static void RemoveGuid(string key)
        {
            lock (_hashGuid)
            {
                if (_hashGuid.ContainsKey(key))
                {
                    _hashGuid.Remove(key);
                    NHExt.Runtime.Logger.LoggerHelper.Debug("退出事务，当前层级1,GUID:" + key);
                }
            }
        }

        private static string getGuid(string key)
        {
            lock (_hashGuid)
            {
                string guid = string.Empty;
                if (_hashGuid.ContainsKey(key))
                {
                    return _hashGuid[key].ToString();
                }
                return null;
            }
        }

        private static string getKey()
        {
            //根据当前线程key来分辨
            string key = "ManagedThreadId_" + System.Threading.Thread.CurrentThread.ManagedThreadId.ToString() + "$$";
            return key;
        }
    }

    public class SessionCache : IDisposable
    {
        private string _cacheGuid;
        /// <summary>
        /// 主数据库
        /// </summary>
        private static Configuration _mainCfg = null;
        /// <summary>
        ///读取数据库
        /// </summary>
        private static Configuration _readCfg = null;
        private Stack<Transaction> _tranStack = new Stack<Transaction>();
        private Stack<Session> _sessionStack = new Stack<Session>();
        /// <summary>
        /// 代理调用栈
        /// </summary>
        private List<NHExt.Runtime.Auth.ProxyProperty> _proxyStack = new List<NHExt.Runtime.Auth.ProxyProperty>();

        private Auth.AuthContext _authContext = null;

        private Session _curSession = null;
        private Transaction _curTrans = null;

        private static System.Collections.Hashtable _cacheContainer = new System.Collections.Hashtable();

        private SessionCache(string cacheGuid)
        {
            _cacheGuid = cacheGuid;
        }
        internal static SessionCache GetCache(string cacheGuid, bool IsAdd = false)
        {
            SessionCache cache = null;
            lock (_cacheContainer)
            {
                if (!_cacheContainer.ContainsKey(cacheGuid))
                {
                    if (IsAdd)
                    {
                        cache = SessionCache.CreateCache(cacheGuid);
                    }
                }
                else
                {
                    cache = SessionCache._cacheContainer[cacheGuid] as SessionCache;
                }
            }
            return cache;
        }
        private static SessionCache CreateCache(string cacheGuid)
        {
            SessionCache cache = new SessionCache(cacheGuid);
            SessionCache._cacheContainer.Add(cacheGuid, cache);
            return cache;
        }
        private void RemoveCache(string cacheGuid)
        {
            if (_cacheContainer.ContainsKey(cacheGuid))
            {
                lock (SessionCache._cacheContainer)
                {
                    SessionCache cache = _cacheContainer[cacheGuid] as SessionCache;
                    cache.CurSession = null;
                    cache.CurSession = null;
                    SessionCache._cacheContainer.Remove(cacheGuid);
                    cache = null;
                }
            }
        }
        public static SessionCache Current
        {
            get
            {
                return SessionCache.GetCache(SessionGuid.Guid);
            }
        }

        internal Stack<Transaction> TranStack
        {
            get { return _tranStack; }
        }
        internal Stack<Session> SessionStack
        {
            get { return _sessionStack; }
        }
        internal Session CurSession
        {
            get { return _curSession; }
            set { _curSession = value; }
        }
        internal Transaction CurTrans
        {
            get { return _curTrans; }
            set { _curTrans = value; }
        }
        /// <summary>
        /// 服务调用栈
        /// </summary>
        public List<NHExt.Runtime.Auth.ProxyProperty> ProxyStack
        {
            get
            {
                return _proxyStack;
            }
            set
            {
                _proxyStack = value;
            }
        }
        /// <summary>
        /// 当前函数最初调用方式
        /// </summary>
        public CallerTypeEnum CallerType { get; set; }

        public Auth.AuthContext AuthContext
        {
            get
            {
                return _authContext;
            }
            set
            {
                _authContext = value;
            }
        }


        /// <summary>
        /// 是否调度任务，后台使用
        /// </summary>
        public bool UseReadDB { get; set; }


        public static Configuration MainCfg
        {
            get
            {
                if (_mainCfg == null)
                {
                    _mainCfg = new Configuration();
                    _mainCfg.SetInterceptor(new NHExt.Runtime.Session.MyInterceptor());
                }
                return _mainCfg;
            }
        }

        public static Configuration ReadCfg
        {
            get
            {
                if (SessionCache._readCfg == null)
                {
                    SessionCache._readCfg = new Configuration();
                    SessionCache._readCfg.SetInterceptor(new NHExt.Runtime.Session.MyInterceptor());
                }
                return SessionCache._readCfg;
            }
            set
            {
                SessionCache._readCfg = value;
            }
        }

        public void Dispose()
        {
            while (this._tranStack.Count > 0)
            {
                Transaction trans = this._tranStack.Pop();
                trans = null;
            }
            while (this._sessionStack.Count > 0)
            {
                Session session = this._sessionStack.Pop();
                session = null;
            }
            this.RemoveCache(_cacheGuid);
        }
        /// <summary>
        /// 初始化session相关配置
        /// </summary>
        public static void Init()
        {
            List<string> assemblyFileList = new List<string>();

            if (Directory.Exists(NHExt.Runtime.Cfg.AppLibPath))
            {
                string[] svcFiles = Directory.GetFiles(NHExt.Runtime.Cfg.AppLibPath, "*.svc");
                if (svcFiles != null)
                {
                    foreach (string svcFile in svcFiles)
                    {
                        string fileName = System.IO.Path.GetFileName(svcFile);
                        int index = fileName.LastIndexOf(".");
                        string assemblyStr = fileName.Substring(0, index);
                        //首先判断是否是HOST服务，如果是服务的话是需要加载BE的
                        if (!string.IsNullOrEmpty(NHExt.Runtime.Cfg.HostUrl))
                        {
                            SessionCache.MergeAssemblyRefrence(svcFile, assemblyFileList);
                        }//如果不是HOST的话则判断当前的程序集是否通过服务来发布的，如果是通过服务来发布的话则不需要加载程序集
                        else if (!NHExt.Runtime.Proxy.InvokerFactory.Contains(assemblyStr))
                        {
                            SessionCache.MergeAssemblyRefrence(svcFile, assemblyFileList);
                        }
                    }
                }
            }
            else if (Directory.Exists(NHExt.Runtime.Cfg.EntityPath))  //如果applicationLib下不存在svc文件的话则从entityLib下面去加载
            {

                string[] files = Directory.GetFiles(NHExt.Runtime.Cfg.EntityPath, "*.dll");
                if (files != null)
                {
                    assemblyFileList.AddRange(files);
                }
            }
            if (assemblyFileList != null && assemblyFileList.Count > 0)
            {
                foreach (string file in assemblyFileList)
                {
                    if (File.Exists(file))
                    {
                        Assembly assembly = Assembly.LoadFrom(file);
                        // 在新会话启动时运行的代码

                        NHExt.Runtime.Session.SessionCache.MainCfg.AddAssembly(assembly);
                        if (NHExt.Runtime.Session.SessionCache.MainCfg != NHExt.Runtime.Session.SessionCache.ReadCfg)
                        {
                            NHExt.Runtime.Session.SessionCache.ReadCfg.AddAssembly(assembly);
                        }
                        NHExt.Runtime.Logger.LoggerHelper.Debug("程序集" + file + "加载成功！", NHExt.Runtime.Logger.LoggerInstance.AppLogger);

                    }
                    else
                    {
                        NHExt.Runtime.Logger.LoggerHelper.Error("程序集" + file + "不存在", NHExt.Runtime.Logger.LoggerInstance.AppLogger);
                    }
                }
            }

        }
        private static void MergeAssemblyRefrence(string svcPath, List<string> assemblyFileList)
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(svcPath);
            XmlNode cfgNode = xmlDoc.SelectSingleNode("ServiceCfg");
            if (cfgNode != null)
            {
                XmlNodeList nodeList = cfgNode.SelectNodes("Refrences/Refrence");
                foreach (XmlNode node in nodeList)
                {
                    string filePath = Path.GetDirectoryName(svcPath) + "\\" + node.Attributes["Assembly"].Value + ".dll";
                    if (!assemblyFileList.Contains(filePath))
                    {
                        assemblyFileList.Add(filePath);
                    }
                }
            }
        }

        private static object _lockObj = new object();
        private static ISessionFactory _readfactory = null;
        private static ISessionFactory _mainFactory = null;
        internal static ISessionFactory GetSessionFactory(bool userReadDB)
        {
            //SessionFactory进行多线程闲置，只能有一个线程来初始化当前共享资源
            ISessionFactory factory = null;
            lock (_lockObj)
            {
                //使用读取数据库的条件是配置了读取数据库并且使用读取数据库
                if (SessionCache._mainCfg != SessionCache._readCfg && userReadDB)
                {
                    factory = SessionCache.getSessionFactory(ref SessionCache._readfactory, SessionCache._readCfg);
                    NHExt.Runtime.Logger.LoggerHelper.Debug("配置了读写数据库，使用只读数据库", NHExt.Runtime.Logger.LoggerInstance.BizLogger);
                }
                else
                {
                    factory = SessionCache.getSessionFactory(ref SessionCache._mainFactory, SessionCache._mainCfg);
                    SecondCache.InitCache(SessionCache._mainFactory);
                }
            }
            return factory;
        }
        private static ISessionFactory getSessionFactory(ref ISessionFactory factory, NHibernate.Cfg.Configuration cfg)
        {
            if (factory != null)
            {
                if (factory.IsClosed)
                {
                    factory.Dispose();
                    factory = null;
                }
            }

            if (factory == null)
            {
                factory = cfg.BuildSessionFactory();

            }
            return factory;
        }

    }
}
