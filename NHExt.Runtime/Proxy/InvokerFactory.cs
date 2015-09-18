using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using NHExt.Runtime.Model;
using System.Reflection;
using System.Collections;

namespace NHExt.Runtime.Proxy
{
    public static class InvokerFactory
    {
        private static Hashtable remoteCfgList = new Hashtable();
        public static AbstractInvoker BuildInvoker(ProxyInvoker pi)
        {
            AbstractInvoker invoker = null;
            lock (InvokerFactory.remoteCfgList)
            {
                if (InvokerFactory.remoteCfgList.Contains(pi.NS))
                {
                    RemoteWCFCfg cfg = InvokerFactory.remoteCfgList[pi.NS] as RemoteWCFCfg;
                    //远程地址就是当前宿主的地址,则采用反射调用
                    string hostUrl = NHExt.Runtime.Cfg.HostUrl;
                    if (!string.IsNullOrEmpty(hostUrl) && hostUrl == cfg.RemoteUrl)
                    {
                        invoker = new ReflectInvoker();
                    }
                    else
                    {
                        invoker = new WCFInvoker();
                        (invoker as WCFInvoker).Url = cfg.RemoteUrl;
                    }
                }
                else
                {
                    invoker = new ReflectInvoker();
                }

                invoker.Guid = pi.Guid;
                invoker.DllName = pi.DllName;
                invoker.NS = pi.NS;
                invoker.ProxyName = pi.ProxyName;
                invoker.ParamList = pi.ParamList;
                invoker.SourcePage = pi.SourcePage;
            }
            return invoker;

        }
        private class RemoteWCFCfg
        {
            public string Assembly { get; set; }
            public string RemoteUrl { get; set; }

            public bool Init(XmlNode node)
            {
                if (node.Attributes["Assembly"] == null || node.Attributes["Url"] == null)
                {
                    return false;
                }
                this.Assembly = node.Attributes["Assembly"].Value;
                this.RemoteUrl = node.Attributes["Url"].Value;
                return true;
            }
        }

        public static void Init()
        {
            string path = NHExt.Runtime.Cfg.AppCfgPath + "WCF_CONFIG.xml";
            if (File.Exists(path))
            {
                NHExt.Runtime.Logger.LoggerHelper.Info("开始加载WCF配置,位置:" + path, NHExt.Runtime.Logger.LoggerInstance.AppLogger);
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(path);
                //划分力度修改为按照assembly来进行部署，按照单独服务部署太细了
                XmlNodeList nodeList = xmlDoc.SelectNodes("Services/Service");
                if (nodeList != null)
                {
                    foreach (XmlNode node in nodeList)
                    {
                        RemoteWCFCfg cfg = new RemoteWCFCfg();
                        if (cfg.Init(node))
                        {
                            if (!InvokerFactory.remoteCfgList.ContainsKey(cfg.Assembly))
                            {
                                InvokerFactory.remoteCfgList.Add(cfg.Assembly, cfg);
                            }
                            else
                            {
                                //如果存在的多个设置则覆盖之前的，只取最后一个配置
                                InvokerFactory.remoteCfgList[cfg.Assembly] = cfg;
                            }
                        }
                    }
                }
                NHExt.Runtime.Logger.LoggerHelper.Info("加载WCF配置完成,位置:" + path, NHExt.Runtime.Logger.LoggerInstance.AppLogger);
            }
            else
            {
                NHExt.Runtime.Logger.LoggerHelper.Info("没有配置远程WCF调用,位置:" + path, NHExt.Runtime.Logger.LoggerInstance.AppLogger);
            }
        }
        /// <summary>
        /// 判断当前是否已经存在WCF服务了
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static bool Contains(string key)
        {
            if (!InvokerFactory.remoteCfgList.ContainsKey(key))
            {
                return false;
            }
            else
            {
                return true;
            }
        }
    }
}
