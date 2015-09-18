using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Collections.Specialized;
using Newtonsoft.Json.Linq;
using System.Reflection;

namespace NHExt.Runtime.Web.HttpHandler
{
    class CommonHandler : AbstractHttpHander, System.Web.SessionState.IRequiresSessionState
    {
    }
}