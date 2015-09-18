using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NHExt.Runtime.AOP
{
    public interface IProxyAspect
    {
        void BeforeDo(Model.IBizProxy proxy, NHExt.Runtime.Proxy.ProxyContext ctx);
        void AfterDo(Model.IBizProxy proxy, object proxyData);
    }
}
