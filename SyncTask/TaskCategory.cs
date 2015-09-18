using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.IO;

namespace SyncTask
{
    class TaskCategory
    {
        private int _begin;

        public int? Begin
        {
            get { return _begin; }
        }
        private int? _end;

        public int? End
        {
            get { return _end; }
        }
        private List<TaskEntity> _taskList = new List<TaskEntity>();
        private NHExt.Runtime.Auth.AuthContext _context = NHExt.Runtime.Auth.AuthContext.GetInstance();

        private void FromXml(string xml)
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(xml);
            XmlNode node = xmlDoc.SelectSingleNode("TaskCategory");
            if (node != null)
            {
                string attr = node.Attributes["Begin"].Value;
                if (!string.IsNullOrEmpty(attr))
                {

                    this._begin = Convert.ToInt32(attr);

                }
                attr = node.Attributes["End"].Value;
                if (!string.IsNullOrEmpty(attr))
                {

                    this._end = Convert.ToInt32(attr);

                }

                XmlNode authNode = node.SelectSingleNode("AuthContext");
                if (authNode != null)
                {
                    XmlNode tmpNode = authNode.SelectSingleNode("Org");
                    if (tmpNode != null)
                    {
                        this._context.SetData("Org", long.Parse(tmpNode.InnerText));
                    }
                    tmpNode = authNode.SelectSingleNode("User");
                    if (tmpNode != null)
                    {
                        this._context.UserCode = tmpNode.InnerText;
                    }
                    tmpNode = authNode.SelectSingleNode("Pwd");
                    if (tmpNode != null)
                    {
                        this._context.UserPwd = tmpNode.InnerText;
                    }
                }
                XmlNodeList nodeList = node.SelectNodes("Task");
                foreach (XmlNode taskNode in nodeList)
                {
                    TaskEntity task = new TaskEntity();
                    task.FromXml(taskNode.OuterXml);
                    this._taskList.Add(task);
                }
            }
        }

        public static List<TaskCategory> GetCategoryList()
        {
            List<TaskCategory> categoryList = new List<TaskCategory>();
            string filePath = NHExt.Runtime.Cfg.AppCfgPath + "TASK_CONFIG.xml";
            if (!File.Exists(filePath)) return categoryList;

            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(filePath);
            XmlNodeList nodeList = xmlDoc.SelectNodes("TaskList/TaskCategory");
            //查找当前工程文件下所有的实体
            if (nodeList != null)
            {
                foreach (XmlNode node in nodeList)
                {
                    TaskCategory tc = new TaskCategory();
                    tc.FromXml(node.OuterXml);
                    categoryList.Add(tc);
                }
            }
            return categoryList;
        }

        public void ExcuteTasks()
        {
            //执行前先需要登陆下看用户名和密码是否正确
            try
            {
                NHExt.Runtime.Proxy.AgentInvoker invoker = new NHExt.Runtime.Proxy.AgentInvoker();
                string loginProxy = NHExt.Runtime.Cfg.GetCfg<string>("LoginProxy");
                if (!string.IsNullOrEmpty(loginProxy))
                {
                    invoker.AssemblyName = loginProxy;
                    invoker.DllName = loginProxy.Substring(0, loginProxy.LastIndexOf(".")) + ".dll";
                }
                else
                {
                    invoker.AssemblyName = "IWEHAVE.ERP.Auth.ServiceBP.Agent.LoginBPProxy";
                    invoker.DllName = "IWEHAVE.ERP.Auth.ServiceBP.Agent.dll";
                }
                invoker.AppendField(new NHExt.Runtime.Proxy.PropertyField() { FieldName = "UserCode", FieldValue = this._context.UserCode });
                invoker.AppendField(new NHExt.Runtime.Proxy.PropertyField() { FieldName = "UserPwd", FieldValue = this._context.UserPwd });
                invoker.AppendField(new NHExt.Runtime.Proxy.PropertyField() { FieldName = "OrgID", FieldValue = this._context.GetData<long>("Org") });
                NHExt.Runtime.Model.BaseDTO userDTO = invoker.Do<NHExt.Runtime.Model.BaseDTO>();
                if (userDTO != null)
                {
                    string address = "127.0.0.1";
                    System.Net.IPHostEntry ipe = System.Net.Dns.GetHostEntry(System.Net.Dns.GetHostName());
                    foreach (System.Net.IPAddress ipa in ipe.AddressList)
                    {
                        //IPV4
                        if (ipa.AddressFamily.ToString() == "InterNetwork")
                        {
                            address = ipa.ToString();
                            break;
                        }
                    }
                    long userID = (long)userDTO.GetData("ID");
                    string userCode = (string)userDTO.GetData("Code");
                    string userName = (string)userDTO.GetData("Name");
                    string userPwd = (string)userDTO.GetData("Password");
                    long rootOrgKey = (long)userDTO.GetData("RootOrgId");
                    long orgKey = (long)userDTO.GetData("OrgId");
                    NHExt.Runtime.GAIA.AuthContext.SetContext(userID, userCode, userName, userPwd, orgKey, rootOrgKey, address);
                    NHExt.Runtime.Logger.LoggerHelper.Error("登陆信息:ORG:" + orgKey + " User:" + userCode, NHExt.Runtime.Logger.LoggerInstance.BizLogger);
                }
            }
            catch (Exception ex)
            {
                Logger.WriteLog("登陆失败,执行自动任务出错", LoggerStatusEnum.Failed);
            }
            foreach (TaskEntity te in this._taskList)
            {
                te.ExcuteTask();
            }

            NHExt.Runtime.Auth.AuthContext.ClearContext();

        }
    }
}
