using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using System;
using System.Reflection;
namespace NHExt.Runtime.Web.HttpHandler
{
    [System.Serializable]
    public class DirectResponse
    {

        private bool _success = true;
        private string resultMsg = string.Empty;
        private string resultDetailMsg = string.Empty;


        private object data = null;

        public bool Success
        {
            get { return this._success; }
            set { this._success = value; }
        }
        public string ResultMsg
        {
            get { return this.resultMsg; }
            set { this.resultMsg = value; }
        }
        public string ResultDetailMsg
        {
            get { return this.resultDetailMsg; }
            set { this.resultDetailMsg = value; }
        }

        public object Data
        {
            get { return this.data; }
            set { this.data = value; }
        }

        public void DeSerialize<T>(string jsonStr)
        {
            JToken jt = JToken.Parse(jsonStr);
            JObject jObj = jt as JObject;
            if (jObj == null)
            {
                throw new NHExt.Runtime.Exceptions.RuntimeException(jsonStr + ":不是DirectResponse对象");
            }
            DirectResponse dr = NHExt.Runtime.Web.DirectResponseSerialize.DeSerialize<T>(jt);
            this.Success = dr.Success;
            this.ResultMsg = dr.ResultMsg;
            this.ResultDetailMsg = dr.ResultDetailMsg;
            this.Data = dr.Data;
        }


    }
}