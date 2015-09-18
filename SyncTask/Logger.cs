using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace SyncTask
{
    public enum LoggerStatusEnum
    {
        Success = 1,
        Failed = 2
    }
    public class Logger
    {
        /// <summary>
        /// 插入日志到数据库
        /// </summary>
        /// <param name="log"></param>
        /// <param name="type"></param>
        public static void WriteLog(string log, LoggerStatusEnum status)
        {
            NHExt.Runtime.Proxy.AgentInvoker invoker = new NHExt.Runtime.Proxy.AgentInvoker();
            invoker.AssemblyName = "IWEHAVE.ERP.PubBP.Agent.InsertTaskLogBPProxy";
            invoker.DllName = "IWEHAVE.ERP.PubBP.Agent.dll";
            invoker.AppendField(new NHExt.Runtime.Proxy.PropertyField() { FieldName = "RecordTime", FieldValue = DateTime.Now });
            invoker.AppendField(new NHExt.Runtime.Proxy.PropertyField() { FieldName = "Content", FieldValue = log });
            invoker.AppendField(new NHExt.Runtime.Proxy.PropertyField() { FieldName = "Type", FieldValue = (int)status });
            invoker.Do<bool>();
        }
    }
}
