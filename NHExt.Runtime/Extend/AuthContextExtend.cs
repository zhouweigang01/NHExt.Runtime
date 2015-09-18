using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NHExt.Runtime.Extend
{
    class AuthContextExtend : NHExt.Runtime.Auth.AuthContext
    {
        #region 内部属性

        /// <summary>
        /// 根组织
        /// </summary>
        public long Org
        {
            get
            {
                return this.GetData<long>("Org");
            }
            set
            {
                this.SetData("Org", value);
            }
        }

        /// <summary>
        /// 当前登录组织
        /// </summary>
        public long OrgC
        {
            get
            {
                return this.GetData<long>("OrgC");
            }
            set
            {
                this.SetData("OrgC", value);
            }
        }

        /// <summary>
        /// 数据组织
        /// </summary>
        public List<long> DataOrgList
        {
            get
            {
                return this.GetData<List<long>>("DataOrgList");
            }
            set
            {
                this.SetData("DataOrgList", value);
            }
        }

        #endregion

        public AuthContextExtend()
        {
            this.Org = -1;
            this.OrgC = -1;
            this.DataOrgList = new List<long>();
        }
        public override bool IsAuth()
        {
            if (this.UserID > 0 && this.OrgC > 0)
            {
                return true;
            }
            return false;
        }
        protected override void FromString(string authStr)
        {
            base.FromString(authStr);

            if (!string.IsNullOrEmpty(authStr))
            {
                string[] authArray = authStr.Split('$');
                if (authArray != null && authArray.Length > 0)
                {
                    this.UserID = long.Parse(authArray[0]);
                    this.UserCode = authArray[1];
                    this.UserName = authArray[2];
                    this.UserPwd = authArray[3];
                    this.OrgC = long.Parse(authArray[4]);
                    this.Org = long.Parse(authArray[5]);
                    this.DataOrgList.Clear();
                    string[] dataOrgStrArray = authArray[6].Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    if (dataOrgStrArray != null)
                    {
                        foreach (string dataOrgStr in dataOrgStrArray)
                        {
                            long dataOrg = Convert.ToInt64(dataOrgStr);
                            if (dataOrg > 0)
                            {
                                this.DataOrgList.Add(dataOrg);
                            }
                        }
                    }
                }
            }
        }
        public override string ToString()
        {
            string resultStr = this.UserID.ToString() + "$" + this.UserCode.ToString() + "$" + this.UserName + "$" + this.UserPwd.ToString() + "$" + this.OrgC.ToString() + "$" + this.Org.ToString();
            resultStr += "$";
            if (this.DataOrgList == null)
            {
                this.DataOrgList = new List<long>();
            }
            if (this.DataOrgList.Count == 0 && this.OrgC > 0)
            {
                this.DataOrgList.Add(this.OrgC);
            }
            foreach (long dataOrg in this.DataOrgList)
            {
                resultStr += ("," + dataOrg);
            }
            return resultStr;

        }
        protected override string GetAuthKey()
        {
            string key = this.RemoteIP + "$" + this.UserID + "$" + this.OrgC;
            return key;
        }
        public override void ValidateAuth()
        {
            try
            {
                NHExt.Runtime.Proxy.AgentInvoker authCheckInvoker = new NHExt.Runtime.Proxy.AgentInvoker();
                authCheckInvoker.AssemblyName = "IWEHAVE.ERP.OrgnizationBP.Agent.LoginBPProxy";
                authCheckInvoker.DllName = "IWEHAVE.ERP.OrgnizationBP.Agent.dll";
                authCheckInvoker.AppendField(new NHExt.Runtime.Proxy.PropertyField() { FieldName = "UserCode", FieldValue = this.UserCode });
                authCheckInvoker.AppendField(new NHExt.Runtime.Proxy.PropertyField() { FieldName = "UserPwd", FieldValue = this.UserPwd });
                authCheckInvoker.AppendField(new NHExt.Runtime.Proxy.PropertyField() { FieldName = "OrgID", FieldValue = this.OrgC });
                object obj = authCheckInvoker.Do();
                if (obj == null)
                {
                    throw new Exception("调用远程服务失败，失败原因：用户名或者密码错误！");
                }
            }
            catch (Exception ex)
            {
                Logger.LoggerInstance.RuntimeLogger.Error(ex);
                throw new Exception("调用远程服务失败，失败原因：用户名或者密码错误！");
            }


        }
        public static void SetContext(long userID, string userCode, string userName, string userPwd, long orgC, long org, string remoteIP, List<long> dataOrgList = null)
        {
            AuthContextExtend authCtx = new AuthContextExtend();
            authCtx.UserID = userID;
            authCtx.UserCode = userCode;
            authCtx.UserName = userName;
            authCtx.UserPwd = userPwd;
            authCtx.OrgC = orgC;
            authCtx.Org = org;
            authCtx.RemoteIP = remoteIP;
            authCtx.DataOrgList = new List<long>();
            NHExt.Runtime.Auth.AuthContext.SetContext(authCtx);
        }

        public static bool SetCache(long userID, string userCode, string userName, string userPwd, long orgID)
        {
            AuthContextExtend authCtx = new AuthContextExtend();
            authCtx.UserID = userID;
            authCtx.UserCode = userCode;
            authCtx.UserName = userName;
            authCtx.UserPwd = userPwd;
            authCtx.Org = orgID;
            return NHExt.Runtime.Auth.AuthContext.SetCache(authCtx);
        }

    }
}
