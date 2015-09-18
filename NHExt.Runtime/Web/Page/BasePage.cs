using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace NHExt.Runtime.Web.Page
{
    public abstract class BasePage : System.Web.UI.Page
    {
        public BasePage()
        {
            this.EnableViewState = false;
            this.EnableTheming = false;
        }
        public abstract string PageGuid
        {
            get;
        }
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            this.RegistClientAuth();
        }

        /// <summary>
        /// 这侧客户端权限脚本
        /// </summary>
        private void RegistClientAuth()
        {
            string key = "PageAuth_" + this.PageGuid;
            if (!Page.ClientScript.IsClientScriptBlockRegistered(key))
            {
                System.Web.Script.Serialization.JavaScriptSerializer jsConvertor = new System.Web.Script.Serialization.JavaScriptSerializer();
                string script = "<script>";
                script += "window.$PageGuid='" + this.PageGuid + "';";
                script += "</script>";
                this.ClientScript.RegisterClientScriptBlock(this.GetType(), key, script);
            }
        }
    }
}
