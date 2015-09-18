using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Linq;
using System.Reflection;
using System.Collections;
using System.Web.Script.Serialization;

namespace NHExt.Runtime.Web
{
    public class DirectResponseSerialize
    {
        public static NHExt.Runtime.Web.HttpHandler.DirectResponse DeSerialize<T>(JToken jt)
        {
            NHExt.Runtime.Web.HttpHandler.DirectResponse dr = new NHExt.Runtime.Web.HttpHandler.DirectResponse();
            JObject jObj = jt as JObject;
            // node节点包含的属性名称的迭代器
            PropertyInfo[] properties = typeof(NHExt.Runtime.Web.HttpHandler.DirectResponse).GetProperties();
            foreach (PropertyInfo pi in properties)
            {
                // 获取当前属性名称
                JToken token = NHExt.Runtime.Serialize.JsonSerialize.GetTokenByPropertyName(jt, pi.Name);
                if (token == null || token.Type == JTokenType.Null)
                {
                    continue;
                }
                Type clazz = null;
                if (pi.Name == "Data")
                {
                    clazz = typeof(T);
                    if (clazz == typeof(Object) || clazz == typeof(object))
                    {
                        dr.Data = token.ToString();
                        continue;
                    }
                }
                else
                {
                    clazz = pi.PropertyType;
                }
                try
                {
                    object obj = NHExt.Runtime.Serialize.JsonSerialize.DeSerialize(token, clazz);
                    if (obj != null)
                    {
                        pi.SetValue(dr, obj, null);
                    }
                }
                catch (NHExt.Runtime.Exceptions.RuntimeException ex)
                {
                    NHExt.Runtime.Logger.LoggerHelper.Error(ex);
                    throw new NHExt.Runtime.Exceptions.RuntimeException("序列化对象失败，字段名称" + pi.Name + "，类型" + dr.GetType().FullName);
                }
            }
            return dr;
        }
    }
}
