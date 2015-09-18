using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NHExt.Runtime.Exceptions
{
    public class ProxyMarchErrorException : Exception
    {
        public ProxyMarchErrorException()
            : base("服务GUID不匹配错误")
        {

        }
    }
}
