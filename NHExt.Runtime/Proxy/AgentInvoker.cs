using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using NHExt.Runtime.Web;
using System.Reflection;

namespace NHExt.Runtime.Proxy
{
    public class AgentInvoker
    {
        public string AssemblyName { get; set; }
        public string DllName { get; set; }
        public string SourcePage { get; set; }
        public bool? UseReadDB { get; set; }
        public bool IsTask { get; set; }
        public bool AutoRun { get; set; }

        private List<PropertyField> _fieldList = new List<PropertyField>();
        public List<PropertyField> FieldList
        {
            get
            {
                return _fieldList;
            }
        }


        /// <summary>
        /// 新增或者修改field
        /// </summary>
        /// <param name="pf"></param>
        public void AppendField(PropertyField pf)
        {
            for (int i = 0; i < _fieldList.Count; i++)
            {
                if (_fieldList[i].FieldName == pf.FieldName)
                {
                    _fieldList[i] = pf;
                    return;
                }
            }
            _fieldList.Add(pf);
        }
        public T Do<T>()
        {
            T result = (T)this.Do();
            return result;
        }
        /// <summary>
        /// 线程调用放到proxyinvoker里面，这样的话就不会出现跨线程context获取不到的问题
        /// </summary>
        /// <returns></returns>
        public object Do()
        {
            Assembly assembly = NHExt.Runtime.Util.AssemblyManager.GetAssembly(this.DllName, NHExt.Runtime.Util.AssemblyTypeEnum.Proxy);
            NHExt.Runtime.Logger.LoggerHelper.Info("创建服务实例对象");
            NHExt.Runtime.Model.IBizAgent agent = NHExt.Runtime.Util.AssemblyManager.CreateInstance(assembly, this.AssemblyName) as NHExt.Runtime.Model.IBizAgent;
            NHExt.Runtime.Logger.LoggerHelper.Info("初始化服务参数");
            try
            {
                Type type = NHExt.Runtime.Util.AssemblyManager.GetType(assembly, this.AssemblyName);
                foreach (PropertyField pf in this._fieldList)
                {
                    if (pf.FieldValue is Newtonsoft.Json.Linq.JProperty)
                    {
                        Newtonsoft.Json.Linq.JProperty jp = pf.FieldValue as Newtonsoft.Json.Linq.JProperty;
                        PropertyInfo pi = type.GetProperty(jp.Name);
                        if (jp.Type != Newtonsoft.Json.Linq.JTokenType.Null && pi != null)
                        {
                            //调用序列化函数，序列化json对象
                            pf.FieldValue = NHExt.Runtime.Serialize.JsonSerialize.DeSerialize(jp.Value, pi.PropertyType);
                        }
                    }
                    if (pf.FieldValue != null)
                    {
                        agent.SetValue(pf.FieldValue, pf.FieldName);
                    }
                }
                if (!string.IsNullOrEmpty(this.SourcePage))
                {
                    agent.SourcePage = this.SourcePage;
                }
                else
                {
                    if (NHExt.Runtime.Session.Session.Current != null)
                    {
                        agent.SourcePage = NHExt.Runtime.Session.SessionCache.Current.AuthContext.SourcePage;
                    }
                }
                ///如果是调度任务的话需要使用初始化agentinvoker的时候生成的authcontext
                ///只有外层不存在事务的情况才需要使用this.task给agent赋值
                ///否则，agent需要使用当前事务环境中的是否task来确定使用哪个数据库
                agent.UseReadDB = this.UseReadDB;
                agent.IsTask = this.IsTask;
                agent.AutoRun = this.AutoRun;
            }
            catch (Exception ex)
            {
                Logger.LoggerHelper.Error(ex);
                NHExt.Runtime.Exceptions.RuntimeException e = new Exceptions.RuntimeException("调用服务初始化服务参数出错");
                throw e;
            }
            object result = agent.DoProxy();
            return result;
        }
    }

    public class PropertyField
    {
        public string FieldName { get; set; }
        public object FieldValue { get; set; }
    }
}
