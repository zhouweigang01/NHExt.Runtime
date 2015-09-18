using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.IO;

using System.Reflection;
using Newtonsoft.Json.Linq;

namespace SyncTask
{
    public class TaskEntity
    {
        //服务全路径
        private string _assmblyName;
        //服务名称
        private string _dllName;
        public string _displayName;

        private List<Newtonsoft.Json.Linq.JToken> _tokenList;

        public void FromXml(string xml)
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(xml);
            XmlNode node = xmlDoc.SelectSingleNode("Task");
            if (node != null)
            {
                this._assmblyName = node.Attributes["Assembly"].Value;
                this._dllName = node.Attributes["DllName"].Value;
                this._displayName = node.Attributes["DisplayName"].Value;

                node = node.SelectSingleNode("Param");
                if (node != null)
                {
                    ParamEntity param = new ParamEntity(node.InnerText);
                    this._tokenList = param.GetTokens();
                }
            }
        }

        public void ExcuteTask()
        {
            try
            {
                Assembly assembly = NHExt.Runtime.Util.AssemblyManager.GetAssembly(this._dllName, NHExt.Runtime.Util.AssemblyTypeEnum.Proxy);
                Type type = NHExt.Runtime.Util.AssemblyManager.GetType(assembly, this._assmblyName);
                NHExt.Runtime.Model.IBizAgent agent = NHExt.Runtime.Util.AssemblyManager.CreateInstance(assembly, this._assmblyName) as NHExt.Runtime.Model.IBizAgent;
                try
                {
                    foreach (JToken token in this._tokenList)
                    {
                        JProperty jp = token as JProperty;
                        PropertyInfo pi = type.GetProperty(jp.Name);
                        if (token.Type != JTokenType.Null && pi != null)
                        {
                            //调用序列化函数，序列化json对象
                            object desObj = NHExt.Runtime.Serialize.JsonSerialize.DeSerialize(jp.Value, pi.PropertyType);
                            //pi.SetValue(agent, desObj, null);
                            agent.SetValue(desObj, jp.Name);
                        }
                    }
                }
                catch (Exception ex)
                {
                    string errMsg = "初始化服务参数失败，错误信息:" + ex.Message + this._assmblyName;
                    Logger.WriteLog(errMsg, LoggerStatusEnum.Failed);
                    NHExt.Runtime.Logger.LoggerInstance.BizLogger.Error(errMsg);
                    return;
                }
                try
                {
                    object result = agent.DoProxy();//type.InvokeMember("Do", BindingFlags.InvokeMethod, null, obj, null);
                    string errMsg = "执行服务过程中成功，服务名称:" + this._assmblyName;
                    Logger.WriteLog(errMsg, LoggerStatusEnum.Success);
                    NHExt.Runtime.Logger.LoggerInstance.BizLogger.Info(errMsg);
                }
                catch (Exception ex)
                {
                    string errMsg = "执行服务过程中错误，错误信息:" + ex.Message + this._assmblyName;
                    Logger.WriteLog(errMsg, LoggerStatusEnum.Failed);
                    NHExt.Runtime.Logger.LoggerInstance.BizLogger.Error(errMsg);
                }
            }
            catch (Exception ex)
            {
                Logger.WriteLog(ex.Message, LoggerStatusEnum.Failed);
                NHExt.Runtime.Logger.LoggerInstance.BizLogger.Error(ex);
            }
        }
    }
}
