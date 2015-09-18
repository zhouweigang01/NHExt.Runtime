using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Reflection;
using System.IO;
using System.Collections;

namespace NHExt.Runtime.AOP
{
    public static class AspectManager
    {
        private static string ALL = "*";

        private static Hashtable EntityAsectorCollection = new Hashtable();
        private static Hashtable ProxyAsectorCollection = new Hashtable();
        private static Hashtable AgentAsectorCollection = new Hashtable();
        /// <summary>
        /// 初始化AOP配置
        /// </summary>
        public static void Init()
        {
            if (Directory.Exists(NHExt.Runtime.Cfg.PlugInPath))
            {
                string[] aopFiles = Directory.GetFiles(NHExt.Runtime.Cfg.PlugInPath, "*.xml");
                if (aopFiles != null && aopFiles.Length > 0)
                {
                    foreach (string file in aopFiles)
                    {
                        AspectManager.DecodeXml(file);
                    }
                }
            }
        }
        private static void DecodeXml(string fileName)
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(fileName);
            XmlNode cfgNode = xmlDoc.SelectSingleNode("AopCfgs");
            if (cfgNode != null)
            {
                XmlNodeList nodeList = cfgNode.SelectNodes("ProxyAop/Aop");
                if (nodeList != null)
                {
                    foreach (XmlNode node in nodeList)
                    {
                        ProxyAopDTO aop = new ProxyAopDTO();
                        if (aop.Init(node))
                        {
                            List<ProxyAopDTO> aspectList = null;
                            string key = aop.Key;
                            if (!AspectManager.ProxyAsectorCollection.ContainsKey(key))
                            {
                                aspectList = new List<ProxyAopDTO>();
                            }
                            else
                            {
                                aspectList = AspectManager.ProxyAsectorCollection[key] as List<ProxyAopDTO>;
                            }

                            aspectList.Add(aop);
                            AspectManager.ProxyAsectorCollection[key] = aspectList;
                        }
                    }
                }
                nodeList = cfgNode.SelectNodes("EntityAop/Aop");
                if (nodeList != null)
                {
                    foreach (XmlNode node in nodeList)
                    {
                        EntityAopDTO aop = new EntityAopDTO();
                        if (aop.Init(node))
                        {
                            List<EntityAopDTO> aspectList = null;
                            string key = aop.Key + "$" + aop.AopPosition;
                            if (!AspectManager.EntityAsectorCollection.ContainsKey(key))
                            {
                                aspectList = new List<EntityAopDTO>();
                            }
                            else
                            {
                                aspectList = AspectManager.EntityAsectorCollection[key] as List<EntityAopDTO>;
                            }

                            aspectList.Add(aop);
                            AspectManager.EntityAsectorCollection[key] = aspectList;
                        }
                    }
                }
                nodeList = cfgNode.SelectNodes("AgentAop/Aop");
                if (nodeList != null)
                {
                    foreach (XmlNode node in nodeList)
                    {
                        AgentAopDTO aop = new AgentAopDTO();
                        if (aop.Init(node))
                        {
                            List<AgentAopDTO> aspectList = null;
                            string key = aop.Key;
                            if (!AspectManager.AgentAsectorCollection.ContainsKey(key))
                            {
                                aspectList = new List<AgentAopDTO>();
                            }
                            else
                            {
                                aspectList = AspectManager.AgentAsectorCollection[key] as List<AgentAopDTO>;
                            }

                            aspectList.Add(aop);
                            AspectManager.AgentAsectorCollection[key] = aspectList;
                        }
                    }
                }
            }
        }
        private class EntityAopDTO : ProxyAopDTO
        {
            public AopPositionEnum AopPosition { get; set; }
            public override bool Init(XmlNode el)
            {
                if (!base.Init(el))
                {
                    return false;
                }
                string pos = el.Attributes["Position"].Value.ToLower();
                if (pos == "inserting")
                {
                    this.AopPosition = AopPositionEnum.Inserting;
                }
                else if (pos == "inserted")
                {
                    this.AopPosition = AopPositionEnum.Inserted;
                }
                else if (pos == "updating")
                {
                    this.AopPosition = AopPositionEnum.Updating;
                }
                else if (pos == "updated")
                {
                    this.AopPosition = AopPositionEnum.Updated;
                }
                else if (pos == "deleting")
                {
                    this.AopPosition = AopPositionEnum.Deleting;
                }
                else if (pos == "deleted")
                {
                    this.AopPosition = AopPositionEnum.Deleted;
                }
                else
                {
                    NHExt.Runtime.Logger.LoggerInstance.RuntimeLogger.Error("没有找到AOP匹配类型");
                    return false;
                }
                return true;
            }

            public new IEntityAspect BuildAspect()
            {
                string assemblyFullName = this.AssemblyFullName;
                if (!string.IsNullOrEmpty(assemblyFullName))
                {
                    string[] assemblyArray = assemblyFullName.Split(',');
                    string dllName = assemblyArray[0] + ".dll";
                    Assembly assembly = NHExt.Runtime.Util.AssemblyManager.GetAssembly(dllName, NHExt.Runtime.Util.AssemblyTypeEnum.PlugIn);
                    IEntityAspect aspect = NHExt.Runtime.Util.AssemblyManager.CreateInstance<IEntityAspect>(assembly, assemblyArray[1]);
                    if (aspect == null)
                    {
                        NHExt.Runtime.Logger.LoggerInstance.RuntimeLogger.Error("AOP创建实例创建错误，在动态库" + dllName + "中反射类型：" + assemblyArray[1] + "不是IEntityInsector类型");
                        return null;
                    }
                    return aspect;
                }
                return null;
            }
        }
        private class ProxyAopDTO
        {
            public string Key { get; set; }
            public string AssemblyFullName { get; set; }
            public virtual bool Init(XmlNode el)
            {
                try
                {
                    this.Key = el.Attributes["Name"].Value;
                    this.AssemblyFullName = el.Attributes["Assembly"].Value;
                }
                catch (Exception ex)
                {
                    NHExt.Runtime.Logger.LoggerInstance.RuntimeLogger.Error(ex);
                    return false;
                }
                return true;
            }
            public IProxyAspect BuildAspect()
            {
                string assemblyFullName = this.AssemblyFullName;
                if (!string.IsNullOrEmpty(assemblyFullName))
                {
                    string[] assemblyArray = assemblyFullName.Split(',');
                    string dllName = assemblyArray[0] + ".dll";

                    Assembly assembly = NHExt.Runtime.Util.AssemblyManager.GetAssembly(dllName, NHExt.Runtime.Util.AssemblyTypeEnum.PlugIn);
                    IProxyAspect aspect = NHExt.Runtime.Util.AssemblyManager.CreateInstance<IProxyAspect>(assembly, assemblyArray[1]);
                    if (aspect == null)
                    {
                        NHExt.Runtime.Logger.LoggerInstance.RuntimeLogger.Error("AOP创建实例创建错误，在动态库" + dllName + "中反射类型：" + assemblyArray[1] + "不是IProxyInsector类型");
                        return null;
                    }
                    return aspect;
                }
                return null;
            }
        }

        private class AgentAopDTO
        {
            public string Key { get; set; }
            public string AssemblyFullName { get; set; }
            public virtual bool Init(XmlNode el)
            {
                try
                {
                    this.Key = el.Attributes["Name"].Value;
                    this.AssemblyFullName = el.Attributes["Assembly"].Value;
                }
                catch (Exception ex)
                {
                    NHExt.Runtime.Logger.LoggerInstance.RuntimeLogger.Error(ex);
                    return false;
                }
                return true;
            }
            public IAgentAspect BuildAspect()
            {
                string assemblyFullName = this.AssemblyFullName;
                if (!string.IsNullOrEmpty(assemblyFullName))
                {
                    string[] assemblyArray = assemblyFullName.Split(',');
                    string dllName = assemblyArray[0] + ".dll";

                    Assembly assembly = NHExt.Runtime.Util.AssemblyManager.GetAssembly(dllName, NHExt.Runtime.Util.AssemblyTypeEnum.PlugIn);
                    IAgentAspect aspect = NHExt.Runtime.Util.AssemblyManager.CreateInstance<IAgentAspect>(assembly, assemblyArray[1]);
 
                    if (aspect == null)
                    {
                        NHExt.Runtime.Logger.LoggerInstance.RuntimeLogger.Error("AOP创建实例创建错误，在动态库" + dllName + "中反射类型：" + assemblyArray[1] + "不是IProxyInsector类型");
                        return null;
                    }
                    return aspect;
                }
                return null;
            }
        }

        /// <summary>
        /// 创建服务AOP对象
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static List<IProxyAspect> BuildProxyAspect(string key)
        {
            List<ProxyAopDTO> proxyAsectorALL = AspectManager.ProxyAsectorCollection[AspectManager.ALL] as List<ProxyAopDTO>;
            List<ProxyAopDTO> proxyAsectorKey = AspectManager.ProxyAsectorCollection[key] as List<ProxyAopDTO>;
            List<IProxyAspect> aspectorList = new List<IProxyAspect>();
            if (proxyAsectorALL != null)
            {
                foreach (ProxyAopDTO aopDTO in proxyAsectorALL)
                {
                    IProxyAspect insector = aopDTO.BuildAspect();
                    if (insector != null)
                    {
                        aspectorList.Add(insector);
                    }
                }
            }
            if (proxyAsectorKey != null)
            {
                foreach (ProxyAopDTO aopDTO in proxyAsectorKey)
                {
                    IProxyAspect insector = aopDTO.BuildAspect();
                    if (insector != null)
                    {
                        aspectorList.Add(insector);
                    }
                }
            }

            return aspectorList;
        }

        /// <summary>
        /// 创建实体AOP对象
        /// </summary>
        /// <param name="key"></param>
        /// <param name="aopPosition"></param>
        /// <returns></returns>
        public static List<IEntityAspect> BuildEntityAspect(string key, AopPositionEnum aopPosition)
        {

            List<EntityAopDTO> entityAsectorALL = AspectManager.EntityAsectorCollection[AspectManager.ALL + "$" + aopPosition] as List<EntityAopDTO>;
            List<EntityAopDTO> entityAsectorKey = AspectManager.EntityAsectorCollection[key + "$" + aopPosition] as List<EntityAopDTO>;
            List<IEntityAspect> aspectorList = new List<IEntityAspect>();
            if (entityAsectorALL != null)
            {
                foreach (EntityAopDTO aopDTO in entityAsectorALL)
                {
                    IEntityAspect insector = aopDTO.BuildAspect();
                    if (insector != null)
                    {
                        aspectorList.Add(insector);
                    }
                }
            }
            if (entityAsectorKey != null)
            {
                foreach (EntityAopDTO aopDTO in entityAsectorKey)
                {
                    IEntityAspect insector = aopDTO.BuildAspect();
                    if (insector != null)
                    {
                        aspectorList.Add(insector);
                    }
                }
            }

            return aspectorList;
        }

        /// <summary>
        /// 创建代理AOP对象
        /// </summary>
        /// <param name="key"></param>
        /// <param name="aopPosition"></param>
        /// <returns></returns>
        public static List<IAgentAspect> BuildAgentAspect(string key)
        {

            List<AgentAopDTO> entityAsectorALL = AspectManager.AgentAsectorCollection[AspectManager.ALL] as List<AgentAopDTO>;
            List<AgentAopDTO> entityAsectorKey = AspectManager.AgentAsectorCollection[key] as List<AgentAopDTO>;
            List<IAgentAspect> aspectorList = new List<IAgentAspect>();
            if (entityAsectorALL != null)
            {
                foreach (AgentAopDTO aopDTO in entityAsectorALL)
                {
                    IAgentAspect insector = aopDTO.BuildAspect();
                    if (insector != null)
                    {
                        aspectorList.Add(insector);
                    }
                }
            }
            if (entityAsectorKey != null)
            {
                foreach (AgentAopDTO aopDTO in entityAsectorKey)
                {
                    IAgentAspect insector = aopDTO.BuildAspect();
                    if (insector != null)
                    {
                        aspectorList.Add(insector);
                    }
                }
            }

            return aspectorList;
        }

    }

    /// <summary>
    /// 实体AOP的时机
    /// </summary>
    public enum AopPositionEnum
    {
        Inserting,
        Inserted,
        Updating,
        Updated,
        Deleting,
        Deleted
    }
}
