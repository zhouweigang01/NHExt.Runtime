using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace NHExt.Runtime.Auth
{
    [Serializable]
    public class AuthContext
    {
        public AuthContext()
        {
            this.UserID = 0;
            this.UserCode = string.Empty;
            this.UserName = string.Empty;
            this.UserPwd = string.Empty;
            this.SourcePage = string.Empty;
            this.RemoteIP = string.Empty;
        }
        #region 内部变量

        /// <summary>
        /// 用户ID
        /// </summary>
        public long UserID
        {
            get
            {
                return this.GetData<long>("UserID");
            }
            set
            {
                this.SetData("UserID", value);
            }
        }

        /// <summary>
        /// 用户编码
        /// </summary>
        public string UserCode
        {
            get
            {
                return this.GetData<string>("UserCode");
            }
            set
            {
                this.SetData("UserCode", value);
            }
        }

        /// <summary>
        /// 用户名称
        /// </summary>
        public string UserName
        {
            get
            {
                return this.GetData<string>("UserName");
            }
            set
            {
                this.SetData("UserName", value);
            }
        }

        /// <summary>
        /// 用户密码
        /// </summary>
        public string UserPwd
        {
            get
            {
                return this.GetData<string>("UserPwd");
            }
            set
            {
                this.SetData("UserPwd", value);
            }
        }

        /// <summary>
        /// 远程调用的IP地址,不需要写入缓存中
        /// </summary>
        public string RemoteIP
        {
            get
            {
                return this.GetData<string>("RemoteIP");
            }
            set
            {
                this.SetData("RemoteIP", value);
            }
        }

        /// <summary>
        /// 服务调用值时候使用，来源调用页面
        /// </summary>
        public string SourcePage
        {
            get
            {
                return this.GetData<string>("SourcePage");
            }
            set
            {
                this.SetData("SourcePage", value);
            }
        }
        #endregion

        #region 自定义参数


        private System.Collections.Hashtable _cusDefineParams = new System.Collections.Hashtable();
        public T GetData<T>(string key)
        {
            if (!this._cusDefineParams.ContainsKey(key))
            {
                throw new NHExt.Runtime.Exceptions.RuntimeException("用户自定义参数不存在，参数key：" + key);
            }
            return (T)this._cusDefineParams[key];

        }
        public void SetData(string key, object obj)
        {
            if (!this._cusDefineParams.ContainsKey(key))
            {
                this._cusDefineParams.Add(key, obj);
            }
            else
            {
                this._cusDefineParams[key] = obj;
            }
        }
        #endregion

        private static Dictionary<string, AuthContext> _authKeys = new Dictionary<string, AuthContext>();

        /// <summary>
        /// 内部静态变量，在winform登陆的时候需要
        /// </summary>
        private static AuthContext _context = null;

        /// <summary>
        /// 获取当前登录用户的authcontext对象，如果没有登录使用默认值
        /// </summary>
        /// <returns></returns>
        [Obsolete("请用GetInstance替代，即将废弃的方法")]
        public static AuthContext GetContext()
        {
            AuthContext ctx = null;
            if (NHExt.Runtime.Session.Session.Current == null)
            {
                ctx = Auth.AuthContext.GetContext(System.Web.HttpContext.Current);
            }
            else
            {
                ctx = NHExt.Runtime.Session.SessionCache.Current.AuthContext;
            }
            if (ctx == null)
            {
                ctx = AuthContextFactory.GetAuthContext();
            }
            return ctx;
        }
        public static AuthContext GetInstance()
        {
            AuthContext ctx = null;
            if (NHExt.Runtime.Session.Session.Current == null)
            {
                ctx = Auth.AuthContext.GetContext(System.Web.HttpContext.Current);
            }
            else
            {
                ctx = NHExt.Runtime.Session.SessionCache.Current.AuthContext;
                if (ctx == null)
                {
                    ctx = NHExt.Runtime.Auth.AuthContextFactory.GetAuthContext();
                }
            }
            if (ctx == null)
            {
                ctx = AuthContextFactory.GetAuthContext();
            }
            return ctx;
        }
        /// <summary>
        /// 从系统缓存中获取context数据
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        private static AuthContext GetContext(System.Web.HttpContext context)
        {
            AuthContext authCtx = null;
            if (context != null)
            {
                authCtx = AuthContextFactory.GetAuthContext();
                if (AuthContext.IsAuthenticated(context))
                {
                    string authStr = context.User.Identity.Name;
                    authCtx.FromString(authStr);
                }
                else//当前逻辑值为了WCF调用的时候服务的
                {
                    string key = GetTheadKey();
                    if (_authKeys.ContainsKey(key))
                    {
                        authCtx = _authKeys[key];
                    }
                }
            }
            else
            {
                authCtx = AuthContext._context;
            }
            return authCtx;
        }

        public static void SetContext(long userID, string userCode, string userName, string userPwd, string remoteIP)
        {
            AuthContext authCtx = new AuthContext();
            authCtx.UserID = userID;
            authCtx.UserCode = userCode;
            authCtx.UserName = userName;
            authCtx.UserPwd = userPwd;
            authCtx.RemoteIP = remoteIP;
            NHExt.Runtime.Auth.AuthContext.SetContext(authCtx);
        }

        public static bool SetCache(long userID, string userCode, string userName, string userPwd)
        {
            AuthContext authCtx = new AuthContext();
            authCtx.UserID = userID;
            authCtx.UserCode = userCode;
            authCtx.UserName = userName;
            authCtx.UserPwd = userPwd;
            return NHExt.Runtime.Auth.AuthContext.SetCache(authCtx);
        }

        public static void SetContext(AuthContext ctx)
        {
            if (System.Web.HttpContext.Current == null)
            {
                AuthContext._context = ctx;
            }
            else
            {
                if (NHExt.Runtime.Auth.AuthContext.IsAuthenticated(null))
                {
                    System.Web.Security.FormsAuthentication.SignOut();
                }
                if (ctx != null)
                {
                    System.Web.Security.FormsAuthentication.SetAuthCookie(ctx.ToString(), false);

                    string key = GetTheadKey();
                    lock (_authKeys)
                    {
                        if (_authKeys.ContainsKey(key))
                        {
                            _authKeys.Remove(key);
                        }
                        _authKeys.Add(key, ctx);
                    }
                }
            }
        }

        public static bool IsAuthenticated(System.Web.HttpContext ctx)
        {
            if (ctx == null)
            {
                ctx = System.Web.HttpContext.Current;
            }
            if (ctx != null)
            {
                if (ctx.User != null && ctx.User.Identity != null && ctx.User.Identity.IsAuthenticated)
                {
                    return true;
                }
            }
            return false;
        }

        protected virtual string GetAuthKey()
        {
            return this.RemoteIP;
        }
        /// <summary>
        /// 是否已经权限校验通过
        /// </summary>
        /// <returns></returns>
        public virtual bool IsAuth()
        {
            if (this.UserID > 0)
            {
                return true;
            }
            return false;
        }
        /// <summary>
        /// 校验权限，客户需要自定义校验逻辑
        /// </summary>
        /// <returns></returns>
        public virtual void ValidateAuth()
        {

        }

        public static string GetTheadKey()
        {
            string key = "ManagedThreadId_" + System.Threading.Thread.CurrentThread.ManagedThreadId.ToString();
            return key;
        }
        public static void ClearContext()
        {
            if (System.Web.HttpContext.Current == null)
            {
                _context = Auth.AuthContext.GetInstance();
            }
            else
            {
                System.Web.Security.FormsAuthentication.SignOut();
                string key = GetTheadKey();
                lock (_authKeys)
                {
                    if (_authKeys.ContainsKey(key))
                    {
                        _authKeys.Remove(key);
                    }
                }
            }
        }

        public virtual void FromString(string authStr)
        {
            if (!string.IsNullOrEmpty(authStr))
            {
                string[] authArray = authStr.Split('$');
                if (authArray != null && authArray.Length >= 4)
                {
                    this.UserID = long.Parse(authArray[0]);
                    this.UserCode = authArray[1];
                    this.UserName = authArray[2];
                    this.UserPwd = authArray[3];
                }
            }
        }

        public override string ToString()
        {
            string resultStr = this.UserID.ToString() + "$" + this.UserCode.ToString() + "$" + this.UserName + "$" + this.UserPwd.ToString();
            return resultStr;
        }


        #region cache相关
        /// <summary>
        /// 有效期为10分钟
        /// </summary>
        private static int _timeSpan = 10 * 60;
        private static Dictionary<string, CacheDTO> _cacheDictionary = new Dictionary<string, CacheDTO>();
        private class CacheDTO
        {
            public string Key { get; set; }
            public AuthContext Context { get; set; }
            public DateTime LastDate { get; set; }
        }
        public static bool SetCache(NHExt.Runtime.Auth.AuthContext authCtx)
        {
            lock (_cacheDictionary)
            {
                bool _exist = false;
                if (string.IsNullOrEmpty(authCtx.RemoteIP))
                {
                    if (System.Web.HttpContext.Current != null)
                    {
                        System.Web.HttpRequest req = System.Web.HttpContext.Current.Request;
                        if (req != null)
                        {
                            authCtx.RemoteIP = req.UserHostAddress;
                        }
                    }
                    else
                    {
                        System.ServiceModel.OperationContext context = System.ServiceModel.OperationContext.Current;
                        if (context != null)
                        {
                            System.ServiceModel.Channels.MessageProperties properties = context.IncomingMessageProperties;
                            System.ServiceModel.Channels.RemoteEndpointMessageProperty endpoint = properties[System.ServiceModel.Channels.RemoteEndpointMessageProperty.Name] as System.ServiceModel.Channels.RemoteEndpointMessageProperty;
                            authCtx.RemoteIP = endpoint.Address;
                        }
                    }
                }
                if (string.IsNullOrEmpty(authCtx.RemoteIP))
                {
                    return false;
                }
                string key = authCtx.GetAuthKey();
                CacheDTO cache = null;

                _cacheDictionary.TryGetValue(key, out cache);
                if (cache == null || string.IsNullOrEmpty(cache.Key))
                {
                    cache = new CacheDTO();
                    cache.Key = key;
                    cache.Context = authCtx;
                    _cacheDictionary.Add(key, cache);
                }
                else
                {
                    _exist = true;
                }
                cache.LastDate = DateTime.Now;
                return _exist;
            }
        }

        public static void RefreshCache()
        {
            lock (_cacheDictionary)
            {
                NHExt.Runtime.Logger.LoggerHelper.Info("清空过期Cache开始##########################################", NHExt.Runtime.Logger.LoggerInstance.RuntimeLogger);
                DateTime dtNow = DateTime.Now;
                List<string> keyList = new List<string>();
                foreach (KeyValuePair<string, CacheDTO> kvp in _cacheDictionary)
                {
                    TimeSpan ts = dtNow - kvp.Value.LastDate;
                    if (ts.TotalSeconds > _timeSpan)
                    {
                        keyList.Add(kvp.Key);
                    }
                }
                foreach (string key in keyList)
                {
                    _cacheDictionary.Remove(key);
                    NHExt.Runtime.Logger.LoggerHelper.Info("Cache过期，key：" + key, NHExt.Runtime.Logger.LoggerInstance.RuntimeLogger);
                }
                if (keyList.Count > 0)
                {
                    NHExt.Runtime.Logger.LoggerHelper.Info("清空过期Cache完成,清除数量：" + keyList.Count + "个" + "##########################################", NHExt.Runtime.Logger.LoggerInstance.RuntimeLogger);
                }
            }
        }
        public static void RefreshCache(long userID, long orgID, string address = "")
        {
            lock (_cacheDictionary)
            {
                if (string.IsNullOrEmpty(address))
                {
                    if (System.Web.HttpContext.Current != null)
                    {
                        System.Web.HttpRequest req = System.Web.HttpContext.Current.Request;
                        if (req != null)
                        {
                            address = req.UserHostAddress;
                        }
                    }
                    else
                    {
                        System.ServiceModel.OperationContext context = System.ServiceModel.OperationContext.Current;
                        if (context != null)
                        {
                            System.ServiceModel.Channels.MessageProperties properties = context.IncomingMessageProperties;
                            System.ServiceModel.Channels.RemoteEndpointMessageProperty endpoint = properties[System.ServiceModel.Channels.RemoteEndpointMessageProperty.Name] as System.ServiceModel.Channels.RemoteEndpointMessageProperty;
                            address = endpoint.Address;
                        }
                    }
                }
                if (string.IsNullOrEmpty(address)) return;
                string key = address + "$" + userID + "$" + orgID;
                bool exist = _cacheDictionary.ContainsKey(key);
                if (exist)
                {
                    _cacheDictionary[key].LastDate = DateTime.Now;
                }
            }
        }
        public static bool ExistCache(long userID, long orgID, string address = "")
        {
            lock (_cacheDictionary)
            {
                if (string.IsNullOrEmpty(address))
                {
                    if (System.Web.HttpContext.Current != null)
                    {
                        System.Web.HttpRequest req = System.Web.HttpContext.Current.Request;
                        if (req != null)
                        {
                            address = req.UserHostAddress;
                        }
                    }
                    else
                    {
                        System.ServiceModel.OperationContext context = System.ServiceModel.OperationContext.Current;
                        if (context != null)
                        {
                            System.ServiceModel.Channels.MessageProperties properties = context.IncomingMessageProperties;
                            System.ServiceModel.Channels.RemoteEndpointMessageProperty endpoint = properties[System.ServiceModel.Channels.RemoteEndpointMessageProperty.Name] as System.ServiceModel.Channels.RemoteEndpointMessageProperty;
                            address = endpoint.Address;
                        }
                    }
                }
                if (string.IsNullOrEmpty(address)) return false;
                string key = address + "$" + userID + "$" + orgID;
                return _cacheDictionary.ContainsKey(key);
            }
        }



        #endregion

    }

    static class AuthContextFactory
    {
        public static AuthContext GetAuthContext()
        {

            NHExt.Runtime.Auth.AuthContext ctx = NHExt.Runtime.Util.IocFactory.GetIocObject<NHExt.Runtime.Auth.AuthContext>("auth_extend");
            if (ctx == null)
            {
                ctx = new AuthContext();
            }
            return ctx;
        }
    }
}
