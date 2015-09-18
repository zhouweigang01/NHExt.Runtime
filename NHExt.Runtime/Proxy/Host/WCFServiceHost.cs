using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Text;

namespace NHExt.Runtime.Proxy.Host
{
    public class WCFServiceHost : ServiceHost
    {
        public WCFServiceHost(Type serviceType, params Uri[] baseAddresses)
            : base(serviceType, baseAddresses)
        {
            this.AddBehaviors();
            WSHttpBinding binding = this.InitBinding();
            this.AddServiceEndpoint(
                       typeof(NHExt.Runtime.Model.IBizProxy),
                       binding,
                       "");
        }
        protected override void OnOpening()
        {

            base.OnOpening();
        }

        protected override void OnClosing()
        {
            base.OnClosing();
        }

        private void AddBehaviors()
        {
            ((ServiceBehaviorAttribute)this.Description.Behaviors[typeof(ServiceBehaviorAttribute)]).InstanceContextMode = InstanceContextMode.PerCall;//
            this.Description.Behaviors.Add(new ServiceThrottlingBehavior()
            {
                MaxConcurrentCalls = int.MaxValue,
                MaxConcurrentInstances = int.MaxValue,
                MaxConcurrentSessions = int.MaxValue

            });
            ServiceDebugBehavior debugBehavior = this.Description.Behaviors[typeof(ServiceDebugBehavior)] as ServiceDebugBehavior;
            debugBehavior.IncludeExceptionDetailInFaults = true;
            ServiceMetadataBehavior smb = this.Description.Behaviors.Find<ServiceMetadataBehavior>();
            if (smb == null)
            {
                smb = new ServiceMetadataBehavior();
                this.Description.Behaviors.Add(smb);
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

    }
}
