using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace NHExt.Runtime.Web.HttpHandler
{
    public class DirectRequest
    {
        public DirectRequest()
        {
            this.args = new List<Newtonsoft.Json.Linq.JToken>();
        }

        private string action;
        /// <summary>
        /// 调用服务名称
        /// </summary>
        public string Action
        {
            set { this.action = value; }
            get { return action; }
        }
        private string assembly;
        /// <summary>
        /// 关联程序集
        /// </summary>
        public string Assembly
        {
            get { return assembly; }
            set { assembly = value; }
        }

        private IList<Newtonsoft.Json.Linq.JToken> args;
        /// <summary>
        /// 调用请求传递的参数
        /// </summary>
        public IList<Newtonsoft.Json.Linq.JToken> Args
        {
            set { this.args = value; }
            get { return args; }
        }

        private string sourcePage;
        public string SourcePage
        {
            get { return this.sourcePage; }
            set { this.sourcePage = value; }
        }

        private bool asyncTask = false;
        /// <summary>
        /// 是否异步调用
        /// </summary>
        public bool IsTask
        {
            get { return asyncTask; }
            set { asyncTask = value; }
        }
        /// <summary>
        /// 获取反射实体
        /// </summary>
        /// <returns></returns>
        public static DirectRequest GetInstance()
        {
            return DirectRequestFactory.GetDirectRequest();
        }
    }

    static class DirectRequestFactory
    {
        public static DirectRequest GetDirectRequest()
        {
            NHExt.Runtime.Web.HttpHandler.DirectRequest req = NHExt.Runtime.Util.IocFactory.GetIocObject<NHExt.Runtime.Web.HttpHandler.DirectRequest>("req_extend");
            if (req == null)
            {
                req = new NHExt.Runtime.Web.HttpHandler.DirectRequest();
            }
            return req;
        }
    }
}
