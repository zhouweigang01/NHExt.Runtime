using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using NHExt.Runtime.Model;
using System.IO;
using NHExt.Runtime.Web;



namespace NHExt.Runtime.Proxy
{
    class ReflectInvoker : AbstractInvoker
    {

        public override object InvokeProxy()
        {
            Assembly assembly = NHExt.Runtime.Util.AssemblyManager.GetAssembly(this.DllName, NHExt.Runtime.Util.AssemblyTypeEnum.BP);
            IBizProxy proxy = NHExt.Runtime.Util.AssemblyManager.CreateInstance<IBizProxy>(assembly, this.NS + "." + this.ProxyName);
            //将复制操作转移到业务中，保证接口的统一性
            NHExt.Runtime.Logger.LoggerHelper.Info("反射服务初始化完成程序集：" + NHExt.Runtime.Cfg.AppLibPath + this.DllName + "命名空间为:" + this.NS + "." + this.ProxyName);
            NHExt.Runtime.Logger.LoggerHelper.Info("开始反射调用服务Do方法");
            object obj = proxy.Do(this.Ctx);
            NHExt.Runtime.Logger.LoggerHelper.Info("反射调用服务Do方法成功");
            return obj;
        }

        public override Session.CallerTypeEnum CallerType
        {
            get { return NHExt.Runtime.Session.CallerTypeEnum.Reflect; }
        }
    }
}
