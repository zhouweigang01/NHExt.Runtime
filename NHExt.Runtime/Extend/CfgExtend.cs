using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

namespace NHExt.Runtime.Extend
{
    public class CfgExtend : Cfg
    {
        /// <summary>
        /// 远程服务器地址
        /// </summary>
        public string ServiceUrl
        {
            get
            {
                return this.Get<string>("ServiceUrl");
            }
            set
            {

                this.Set("ServiceUrl", value);
            }
        }

        /// <summary>
        /// 宿主程序本地地址
        /// </summary>
        public string HostUrl
        {
            get
            {
                return this.Get<string>("ServiceUrl");
            }
            set
            {

                this.Set("ServiceUrl", value);
            }
        }
        /// <summary>
        /// 是否UI验权
        /// </summary>
        internal bool IsUIAuth
        {
            get
            {
                return this.Get<bool>("IsUIAuth");
            }
            set
            {

                this.Set("IsUIAuth", value);
            }
        }
        /// <summary>
        /// 是否数据验权
        /// </summary>
        internal bool IsDataAuth
        {
            get
            {
                return this.Get<bool>("IsDataAuth");
            }
            set
            {

                this.Set("IsDataAuth", value);
            }
        }

        /// <summary>
        /// 是否启用数据权限缓存
        /// </summary>
        internal bool IsEnableCache
        {
            get
            {
                return this.Get<bool>("IsEnableCache");
            }
            set
            {

                this.Set("IsEnableCache", value);
            }
        }
        public CfgExtend()
        {
            //2013-12-12添加
            this.IsUIAuth = true;
            this.IsDataAuth = true;
            this.IsEnableCache = true;
            if (File.Exists(Cfg.AppCfgPath + "RuntimeConfig.xml"))
            {
                //读取配置文件
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(Cfg.AppCfgPath + "RuntimeConfig.xml");
                XmlNode cfgNode = xmlDoc.SelectSingleNode("RuntimeConfig/AuthCfg");
                if (cfgNode != null)
                {
                    XmlNode node = cfgNode.SelectSingleNode("DataAuthCfg");
                    if (node != null)
                    {
                        //是否启用数据权限
                        bool isDataAuth = true, isEnableCache = true;
                        string authStr = node.Attributes["IsDataAuth"].Value;
                        bool.TryParse(authStr, out isDataAuth);
                        this.IsDataAuth = isDataAuth;

                        authStr = node.Attributes["IsEnableCache"].Value;
                        bool.TryParse(authStr, out isEnableCache);
                        this.IsEnableCache = isEnableCache;
                    }
                    node = cfgNode.SelectSingleNode("UIAuthCfg");
                    if (node != null)
                    {
                        //是否启用数据权限
                        bool isUIAuth = true;
                        string authStr = node.Attributes["IsUIAuth"].Value;
                        bool.TryParse(authStr, out isUIAuth);
                        this.IsUIAuth = isUIAuth;
                    }
                }

                XmlNode serverNode = xmlDoc.SelectSingleNode("RuntimeConfig/Server");
                if (serverNode != null)
                {
                    //统一登录网站更新地址
                    this.ServiceUrl = serverNode.Attributes["ServiceUrl"].Value;
                }
            }
        }
    }
}
