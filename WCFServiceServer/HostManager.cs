using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.IO;
using System.Xml;
using System.Reflection;
using NHExt.Runtime.Model;
using System.ServiceModel.Description;
using System.Configuration;

namespace WCFServiceServer
{
    class HostManager
    {
        /// <summary>
        /// 服务器host宿主列表
        /// </summary>
        private List<ServiceHost> hostList = new List<ServiceHost>();
        private string baseUrl = "";

        #region 初始化服务程序配置
        /// <summary>
        /// 合并服务配置
        /// </summary>
        /// <returns></returns>
        private List<ServiceHostDTO> InitServices()
        {
            List<ServiceHostDTO> hostDTOList = new List<ServiceHostDTO>();
            string[] svcFiles = Directory.GetFiles(NHExt.Runtime.Cfg.AppPath, "*.svc");
            if (svcFiles != null && svcFiles.Length > 0)
            {
                foreach (string svcFile in svcFiles)
                {
                    this.MergeServiceDTO(svcFile, hostDTOList);
                }
            }
            return hostDTOList;
        }
        private void MergeServiceDTO(string svcPath, List<ServiceHostDTO> hostDTOList)
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

        class ServiceHostDTO
        {
            public string Assembly { get; set; }
            public List<string> ProxyList { get; set; }
        }
        #endregion

        #region  初始化Host相关
        private void InitServiceHosts()
        {
            List<ServiceHostDTO> hostDTOList = this.InitServices();
            foreach (ServiceHostDTO dto in hostDTOList)
            {
                string dllPath = NHExt.Runtime.Cfg.AppLibPath + dto.Assembly + ".dll";
                if (File.Exists(dllPath))
                {
                    Assembly assembly = Assembly.LoadFrom(dllPath);
                    foreach (string proxy in dto.ProxyList)
                    {
                        string url = baseUrl + dto.Assembly.Replace(".", "/") + "/" + proxy + "Proxy.svc";

                        try
                        {
                            Type t = assembly.GetType(dto.Assembly + "." + proxy);
                            if (t != null)
                            {

                                ServiceHost host = new ServiceHost(t, new Uri[] { new Uri(url) });
                                this.AddBehaviors(host);
                                WSHttpBinding binding = this.InitBinding();
                                host.AddServiceEndpoint(
                                    typeof(IBizProxy),
                                    binding,
                                    url);

                                hostList.Add(host);
                                NHExt.Runtime.Logger.LoggerHelper.Info("宿主在地址" + url + "创建成功", NHExt.Runtime.Logger.LoggerInstance.RuntimeLogger);
                            }
                        }
                        catch (Exception ex)
                        {
                            NHExt.Runtime.Logger.LoggerHelper.Error("宿主在地址" + url + "创建失败", NHExt.Runtime.Logger.LoggerInstance.RuntimeLogger);
                            if (ex.InnerException != null)
                            {
                                NHExt.Runtime.Logger.LoggerHelper.Error(ex.InnerException, NHExt.Runtime.Logger.LoggerInstance.RuntimeLogger);
                            }
                            else
                            {
                                NHExt.Runtime.Logger.LoggerHelper.Error(ex, NHExt.Runtime.Logger.LoggerInstance.RuntimeLogger);
                            }
                        }
                    }
                }
            }
        }

        private void AddBehaviors(ServiceHost host)
        {
            ((ServiceBehaviorAttribute)host.Description.Behaviors[typeof(ServiceBehaviorAttribute)]).InstanceContextMode = InstanceContextMode.PerCall;//
            host.Description.Behaviors.Add(new ServiceThrottlingBehavior()
            {
                MaxConcurrentCalls = int.MaxValue,
                MaxConcurrentInstances = int.MaxValue,
                MaxConcurrentSessions = int.MaxValue

            });
            ServiceDebugBehavior debugBehavior = host.Description.Behaviors[typeof(ServiceDebugBehavior)] as ServiceDebugBehavior;
            debugBehavior.IncludeExceptionDetailInFaults = true;
            ServiceMetadataBehavior smb = host.Description.Behaviors.Find<ServiceMetadataBehavior>();
            if (smb == null)
            {
                smb = new ServiceMetadataBehavior();
                host.Description.Behaviors.Add(smb);
            }
            smb.HttpGetEnabled = true;
            smb.MetadataExporter.PolicyVersion = PolicyVersion.Policy15;

        }
        private System.ServiceModel.WSHttpBinding InitBinding()
        {
            WSHttpBinding binding = new WSHttpBinding();
            binding.Security.Mode = SecurityMode.None;
            var max = 1024 * 1024 * 1024;
            binding.MaxReceivedMessageSize = max;
            binding.ReaderQuotas.MaxStringContentLength = max;
            binding.ReaderQuotas.MaxArrayLength = max;
            binding.ReceiveTimeout = new TimeSpan(0, 2, 0);
            binding.SendTimeout = new TimeSpan(0, 2, 0);
            binding.TextEncoding = Encoding.UTF8;
            if (binding.ReaderQuotas == null)
            {
                binding.ReaderQuotas = new System.Xml.XmlDictionaryReaderQuotas();
            }
            return binding;
        }
        #endregion

        public void Init()
        {
            if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["BaseUrl"]))
            {
                NHExt.Runtime.Cfg.HostUrl = ConfigurationManager.AppSettings["BaseUrl"];

            }
            else
            {
                NHExt.Runtime.Cfg.HostUrl = "http://localhost:8000/Services";
            }
            this.baseUrl = NHExt.Runtime.Cfg.HostUrl + "/";
            Console.WriteLine("准备初始化系统配置……");
            NHExt.Runtime.Cfg.Init();
            Console.WriteLine("初始化系统配置完成……");
            Console.WriteLine("准备初始化系统服务文件……");
            //WCF服务初始化
            this.InitServices();
            Console.WriteLine("初始化系统服务文件完成……");
            Console.WriteLine("开始合并系统服务……");
            this.InitServiceHosts();
            Console.WriteLine("合并系统服务完成……");
            NHExt.Runtime.Logger.LoggerHelper.Info("初始化服务宿主完成", NHExt.Runtime.Logger.LoggerInstance.RuntimeLogger);

        }
        public void OpenHost()
        {
            NHExt.Runtime.Logger.LoggerHelper.Info("开始打开服务宿主程序", NHExt.Runtime.Logger.LoggerInstance.RuntimeLogger);
            Console.WriteLine("开始打开系统服务监听……");
            foreach (ServiceHost host in hostList)
            {
                try
                {
                    host.Opened += (sender, e) =>
                    {
                        ServiceHost sh = sender as ServiceHost;
                        if (sh != null)
                        {
                            foreach (var address in sh.BaseAddresses)
                            {
                                NHExt.Runtime.Logger.LoggerHelper.Info("宿主在地址" + address + "打开成功", NHExt.Runtime.Logger.LoggerInstance.RuntimeLogger);
                            }
                        }
                    };
                    host.Faulted += (object sender, EventArgs e) => { };
                    host.Open();

                }
                catch (Exception ex)
                {
                    foreach (var address in host.BaseAddresses)
                    {
                        NHExt.Runtime.Logger.LoggerHelper.Error("宿主在地址" + address + "打开失败,失败原因", NHExt.Runtime.Logger.LoggerInstance.RuntimeLogger);
                        NHExt.Runtime.Logger.LoggerHelper.Error(ex, NHExt.Runtime.Logger.LoggerInstance.RuntimeLogger);
                    }
                }
            }
            Console.WriteLine("打开系统服务监听成功，监听服务个数：" + hostList.Count + "");
            NHExt.Runtime.Logger.LoggerHelper.Info("宿主程序打开完成", NHExt.Runtime.Logger.LoggerInstance.RuntimeLogger);
        }

        public void CloseHost()
        {
            foreach (ServiceHost host in hostList)
            {
                try
                {
                    host.Close();
                    foreach (var address in host.BaseAddresses)
                    {
                        NHExt.Runtime.Logger.LoggerHelper.Info("宿主在地址" + address + "关闭成功", NHExt.Runtime.Logger.LoggerInstance.RuntimeLogger);
                    }
                }
                catch (Exception ex)
                {
                    NHExt.Runtime.Logger.LoggerHelper.Error(ex, NHExt.Runtime.Logger.LoggerInstance.RuntimeLogger);
                }
            }
        }

    }
}
