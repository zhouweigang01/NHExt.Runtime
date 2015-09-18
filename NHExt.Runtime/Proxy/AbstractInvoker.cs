using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NHExt.Runtime.Session;

namespace NHExt.Runtime.Proxy
{
    public abstract class AbstractInvoker
    {
        private string _guid;
        public string Guid
        {
            get { return _guid; }
            set { _guid = value; }
        }

        private string _dllName;
        public string DllName
        {
            get { return _dllName; }
            set { _dllName = value; }
        }
        /// <summary>
        /// 命名空间
        /// </summary>
        private string _ns;
        public string NS
        {
            get { return _ns; }
            set { _ns = value; }
        }
        /// <summary>
        /// 选择器名称
        /// </summary>
        private string _proxyName;
        public string ProxyName
        {
            get { return _proxyName; }
            set { _proxyName = value; }
        }


        public NHExt.Runtime.Proxy.ProxyContext Ctx { get; set; }

        /// <summary>
        /// 调用方式
        /// </summary>
        public abstract CallerTypeEnum CallerType { get; }

        private List<object> paramList = new List<object>();
        public List<object> ParamList
        {
            get { return paramList; }
            set { paramList = value; }
        }
        public string SourcePage { get; set; }
        public abstract object InvokeProxy();
    }
}
