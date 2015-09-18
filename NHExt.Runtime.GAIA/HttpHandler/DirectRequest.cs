using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NHExt.Runtime.GAIA.HttpHandler
{
    public class DirectRequest : NHExt.Runtime.Web.HttpHandler.DirectRequest
    {

        private IList<Newtonsoft.Json.Linq.JToken> auth;
        /// <summary>
        /// WCFHandler调用使用
        /// </summary>
        public IList<Newtonsoft.Json.Linq.JToken> Auth
        {
            get { return auth; }
            set { auth = value; }
        }

        public DirectRequest()
        {
            this.auth = new List<Newtonsoft.Json.Linq.JToken>();
        }
    }

 
}
