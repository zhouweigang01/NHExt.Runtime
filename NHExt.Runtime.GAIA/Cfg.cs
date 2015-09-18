using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

namespace NHExt.Runtime.GAIA
{
    public class Cfg : NHExt.Runtime.Cfg
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
        /// 登录服务
        /// </summary>
        internal string LoginProxy
        {
            get
            {
                return this.Get<string>("LoginProxy");
            }
            set
            {
                this.Set("LoginProxy", value);
            }
        }

        /// <summary>
        /// 页面权限服务
        /// </summary>
        internal string PageAuthProxy
        {
            get
            {
                return this.Get<string>("PageAuthProxy");
            }
            set
            {
                this.Set("PageAuthProxy", value);
            }
        }

        /// <summary>
        /// 菜单权限服务
        /// </summary>
        internal string MenuItemAuthProxy
        {
            get
            {
                return this.Get<string>("MenuItemAuthProxy");
            }
            set
            {
                this.Set("MenuItemAuthProxy", value);
            }
        }


        /// <summary>
        /// 数据权限服务
        /// </summary>
        internal string DataAuthProxy
        {
            get
            {
                return this.Get<string>("DataAuthProxy");
            }
            set
            {
                this.Set("DataAuthProxy", value);
            }
        }

        /// <summary>
        /// 组织过滤
        /// </summary>
        internal string EntityStrProxy
        {
            get
            {
                return this.Get<string>("EntityStrProxy");
            }
            set
            {
                this.Set("EntityStrProxy", value);
            }
        }

        public Cfg()
        {
            //2013-12-12添加
            this.IsUIAuth = true;
            this.IsDataAuth = true;
            if (File.Exists(Cfg.AppCfgPath + "RUNTIME_CONFIG.xml"))
            {
                //读取配置文件
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(Cfg.AppCfgPath + "RUNTIME_CONFIG.xml");
                #region  控权配置
                XmlNode cfgNode = xmlDoc.SelectSingleNode("RuntimeConfig/AuthCfg");
                if (cfgNode != null)
                {
                    XmlNode node = cfgNode.SelectSingleNode("DataAuthCfg");
                    if (node != null)
                    {
                        //是否启用数据权限
                        bool isDataAuth = true;
                        string authStr = node.Attributes["IsDataAuth"].Value;
                        bool.TryParse(authStr, out isDataAuth);
                        this.IsDataAuth = isDataAuth;
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

                
                #endregion

                cfgNode = xmlDoc.SelectSingleNode("RuntimeConfig/ThreadPool");
                if (cfgNode != null)
                {
                    NHExt.Runtime.Cfg.MaxThreadCount = Convert.ToInt32(cfgNode.Attributes["MaxCount"].Value);
                }



                XmlNode serverNode = xmlDoc.SelectSingleNode("RuntimeConfig/Server");
                if (serverNode != null)
                {
                    //统一登录网站更新地址
                    this.ServiceUrl = serverNode.Attributes["ServiceUrl"].Value;
                }

                #region 调用服务配置
                this.LoginProxy = this.GetAuthProxy(xmlDoc, "LoginCfg");
                this.PageAuthProxy = this.GetAuthProxy(xmlDoc, "PageAuthCfg");
                this.MenuItemAuthProxy = this.GetAuthProxy(xmlDoc, "MenuItemAuthCfg");
                this.DataAuthProxy = this.GetAuthProxy(xmlDoc, "DataAuthCfg");
                this.EntityStrProxy = this.GetAuthProxy(xmlDoc, "EntityStrCfg");

                #endregion
            }


        }

        /// <summary>
        /// 获取配置平台调用服务配置
        /// </summary>
        /// <param name="xmlDoc"></param>
        /// <param name="nodeName"></param>
        /// <returns></returns>
        private string GetAuthProxy(XmlDocument xmlDoc, string nodeName)
        {
            string proxy = string.Empty;
            XmlNode proxyNode = xmlDoc.SelectSingleNode("RuntimeConfig/ProxyCfg");
            if (proxyNode != null)
            {
                XmlNode node = proxyNode.SelectSingleNode(nodeName);
                if (node != null)
                {
                    proxy = node.Attributes["Name"].Value;
                }
            }
            return proxy;
        }
    }
}
