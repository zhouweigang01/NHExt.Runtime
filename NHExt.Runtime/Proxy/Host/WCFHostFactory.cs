using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.Text;
using System.Xml;

namespace NHExt.Runtime.Proxy.Host
{

    public class WCFHostFactory : ServiceHostFactory
    {
        public override ServiceHostBase CreateServiceHost(string constructorString, Uri[] baseAddresses)
        {
            if (string.IsNullOrEmpty(constructorString))
            {
                return base.CreateServiceHost(constructorString, baseAddresses);
            }
            string dllName = constructorString.Substring(0, constructorString.LastIndexOf(".")) + ".dll";
            Assembly assembly = NHExt.Runtime.Util.AssemblyManager.GetAssembly(dllName, NHExt.Runtime.Util.AssemblyTypeEnum.BP);
            Type t = assembly.GetType(constructorString);
            if (t == null)
            {
                return base.CreateServiceHost(constructorString, baseAddresses);
            }
            return new WCFServiceHost(t, baseAddresses);
        }


        public static void Init()
        {
            string basePath = NHExt.Runtime.Cfg.AppPath + @"Services";
            List<ServiceHostDTO> hostDTOList = WCFHostFactory.InitServices();
            foreach (ServiceHostDTO sh in hostDTOList)
            {
                string assemblyStr = sh.Assembly;
                string svcFolder = basePath + @"\" + assemblyStr.Replace(".", "\\");
                if (!Directory.Exists(svcFolder))
                {
                    Directory.CreateDirectory(svcFolder);
                }
                foreach (string bp in sh.ProxyList)
                {
                    string filePath = svcFolder + "\\" + bp + "Proxy.svc";
                    StreamWriter sw = new StreamWriter(filePath);
                    sw.Write("<%@ ServiceHost Debug=\"true\" Service=\"" + sh.Assembly + "." + bp + "\" Factory=\"NHExt.Runtime.Proxy.Host.WCFHostFactory\" %>");
                    sw.Close();
                }
            }

        }

        #region 初始化服务程序配置
        /// <summary>
        /// 合并服务配置
        /// </summary>
        /// <returns></returns>
        private static List<ServiceHostDTO> InitServices()
        {
            List<ServiceHostDTO> hostDTOList = new List<ServiceHostDTO>();
            string[] svcFiles = Directory.GetFiles(NHExt.Runtime.Cfg.AppLibPath, "*.svc");
            if (svcFiles != null && svcFiles.Length > 0)
            {
                foreach (string svcFile in svcFiles)
                {
                    WCFHostFactory.MergeServiceDTO(svcFile, hostDTOList);
                }
            }
            return hostDTOList;
        }
        private static void MergeServiceDTO(string svcPath, List<ServiceHostDTO> hostDTOList)
        {
            if (hostDTOList == null) hostDTOList = new List<ServiceHostDTO>();
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(svcPath);
            XmlNode cfgNode = xmlDoc.SelectSingleNode("ServiceCfg");
            if (cfgNode != null)
            {
                bool isService = false;
                if (cfgNode.Attributes["SVC"] != null)
                {
                    isService = cfgNode.Attributes["SVC"].Value == "1" ? true : false;
                }
                if (isService)
                {
                    string assemblyName = cfgNode.Attributes["NS"].Value;
                    List<string> serviceList = new List<string>();
                    XmlNodeList nodeList = cfgNode.SelectNodes("Services/Service");
                    foreach (XmlNode node in nodeList)
                    {
                        serviceList.Add(node.Attributes["Name"].Value);
                    }
                    hostDTOList.Add(new ServiceHostDTO() { Assembly = assemblyName, ProxyList = serviceList });
                }
            }

        }
        private class ServiceHostDTO
        {
            public string Assembly { get; set; }
            public List<string> ProxyList { get; set; }
        }
        #endregion

    }
}
