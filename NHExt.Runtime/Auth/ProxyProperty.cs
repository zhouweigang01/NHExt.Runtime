using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NHExt.Runtime.Auth
{
    public class ProxyProperty
    {
        private string _proxyGuid = string.Empty;

        public string ProxyGuid
        {
            get { return _proxyGuid; }
            set { _proxyGuid = value; }
        }
        private string _proxyName = string.Empty;

        public string ProxyName
        {
            get { return _proxyName; }
            set { _proxyName = value; }
        }
    }
}
