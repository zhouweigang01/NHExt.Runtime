using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Collections.Specialized;
using System.Xml;
using System.IO;
using System.Collections;
using Newtonsoft.Json.Linq;
using System.Reflection;
using NHExt.Runtime.Web;

namespace NHExt.Runtime.Extend.HttpHandler
{
    class JsonHandler : NHExt.Runtime.Web.HttpHandler.AbstractHttpHander, System.Web.SessionState.IRequiresSessionState
    {
        protected override void CreateDirectParameterExtend(NHExt.Runtime.Web.HttpHandler.DirectRequest req, NameValueCollection nvc)
        {
            base.CreateDirectParameterExtend(req, nvc);
            req.SourcePage = nvc["PageGuid"];
        }

        protected override void ValidateDirectParameter(NHExt.Runtime.Web.HttpHandler.DirectRequest req)
        {
            base.ValidateDirectParameter(req);
            if (string.IsNullOrEmpty(req.SourcePage))
            {
                throw new Exception(req.Action + "服务调用失败，错误信息：来源页面不能为空");
            }
        }
        protected override void CreateAgentParameterExtend(Model.IBizAgent agent, NHExt.Runtime.Web.HttpHandler.DirectRequest dr)
        {
            base.CreateAgentParameterExtend(agent, dr);
            agent.SourcePage = dr.SourcePage;
        }
    }
}
