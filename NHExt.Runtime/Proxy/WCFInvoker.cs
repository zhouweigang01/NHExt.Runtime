using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using NHExt.Runtime.Model;

namespace NHExt.Runtime.Proxy
{
    class WCFInvoker : AbstractInvoker
    {
        private string _url;
        public string Url
        {
            get { return _url; }
            set { _url = value; }
        }
        public override object InvokeProxy()
        {
            string address = this.Url + "/" + this.NS.Replace(".", "/") + "/" + this.ProxyName + "Proxy.svc";
            WSHttpBinding binding = this.InitBinding();
            ChannelFactory<IBizProxy> factory =
                new ChannelFactory<IBizProxy>(
                    binding,
                    new EndpointAddress(
                        new Uri(address)));
            NHExt.Runtime.Logger.LoggerHelper.Info("开始创建wcf远程服务url:" + address, NHExt.Runtime.Logger.LoggerInstance.RuntimeLogger);
            IBizProxy proxy = factory.CreateChannel();
            NHExt.Runtime.Logger.LoggerHelper.Info("开始调用WCF服务Do方法", NHExt.Runtime.Logger.LoggerInstance.RuntimeLogger);
            object obj = null;
            try
            {
                WCFCallDTO callDTO = proxy.DoWCF(this.Ctx);

                if (callDTO != null)
                {
                    if (callDTO.Success)
                    {
                        obj = callDTO.Result;
                    }
                    else
                    {
                        NHExt.Runtime.Logger.LoggerHelper.Error("调用" + address + "服务异常：" + callDTO.Result, NHExt.Runtime.Logger.LoggerInstance.RuntimeLogger);
                        NHExt.Runtime.Exceptions.BizException ex = new NHExt.Runtime.Exceptions.BizException(callDTO.Result);
                        throw ex;
                    }
                }
            }
            catch (System.ServiceModel.EndpointNotFoundException ex)
            {
                NHExt.Runtime.Logger.LoggerHelper.Error(ex, NHExt.Runtime.Logger.LoggerInstance.RuntimeLogger);
                throw new NHExt.Runtime.Exceptions.RuntimeException("远程服务已经关闭或者正在维护，请稍后重试！");
            }
            NHExt.Runtime.Logger.LoggerHelper.Info("调用WCF服务Do方法成功", NHExt.Runtime.Logger.LoggerInstance.RuntimeLogger);
            return obj;
        }

        private WSHttpBinding InitBinding()
        {
            WSHttpBinding binding = new WSHttpBinding();
            binding.Security.Mode = SecurityMode.None;
            var max = 1024 * 1024 * 1024;
            binding.MaxReceivedMessageSize = max;
            binding.ReaderQuotas.MaxStringContentLength = max;
            binding.ReaderQuotas.MaxArrayLength = max;
            binding.ReceiveTimeout = new TimeSpan(0, 10, 0);
            binding.SendTimeout = new TimeSpan(0, 5, 0);
            binding.TextEncoding = Encoding.UTF8;

            if (binding.ReaderQuotas == null)
            {
                binding.ReaderQuotas = new System.Xml.XmlDictionaryReaderQuotas();
            }
            return binding;
        }

        public override Session.CallerTypeEnum CallerType
        {
            get { return NHExt.Runtime.Session.CallerTypeEnum.WCF; }
        }
    }
}
