using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Reflection;

namespace NHExt.Runtime.Serialize
{
    public class JsonSerialize
    {
        /// <summary>
        /// 将对象序列化成json对象
        /// </summary>
        /// <param name="responseMsg"></param>
        /// <returns></returns>
        public static string Serialize(object responseMsg)
        {
            System.Web.Script.Serialization.JavaScriptSerializer jsConvertor = new System.Web.Script.Serialization.JavaScriptSerializer();
            jsConvertor.MaxJsonLength = 1024 * 1024 * 10;
            NHExt.Runtime.Logger.LoggerHelper.Info("序列化对象为json对象");
            string output = jsConvertor.Serialize(responseMsg);
            NHExt.Runtime.Logger.LoggerHelper.Info("序列化json对象完成,json:" + output);
            return output;
        }

        public static object DeSerialize(JToken jt, Type clazz)
        {
            if ((jt.Type != JTokenType.Object && jt.Type != JTokenType.Array) || (clazz.IsValueType || clazz.Equals(typeof(string)) || clazz.Equals(typeof(String))))
            {
                return JsonSerialize.convertObjectToValue(jt, clazz);
            }
            else
            {
                return JsonSerialize.ConvertObjectToObject(jt, clazz);
            }
        }
        public static T DeSerialize<T>(JToken jt)
        {
            Type clazz = typeof(T);
            object obj = JsonSerialize.DeSerialize(jt, clazz);
            return (T)obj;
        }
        #region 内部调用方法
        private static object convertObjectToValue(JToken jt, Type clazz)
        {
            if (jt.Type == JTokenType.Array || jt.Type == JTokenType.Object)
            {
                throw new NHExt.Runtime.Exceptions.RuntimeException("当前属性类型不是值类型");
            }
            else
            {
                if (!clazz.Equals(typeof(object)) && !clazz.Equals(typeof(Object)) && !clazz.IsValueType && !clazz.Equals(typeof(string)) && !clazz.Equals(typeof(String)))
                {
                    throw new NHExt.Runtime.Exceptions.RuntimeException("当前属性类型不是值类型");
                }
            }
            bool isJValue = jt is JValue;
            if (!isJValue)
            {
                throw new NHExt.Runtime.Exceptions.RuntimeException("当前属性类型不是值类型");
            }
            object propertyObj = jt.ToObject<object>();

            if (object.ReferenceEquals(propertyObj, null))
            {
                return null;
            }
            if (string.IsNullOrEmpty(propertyObj.ToString()))
            {
                return null;
            }
            object returnObj = propertyObj;
            if (clazz.Equals(typeof(int)) || clazz.Equals(typeof(int?)) || clazz.Equals(typeof(Int32)) || clazz.Equals(typeof(Int32?)))
            {
                returnObj = Convert.ToInt32(propertyObj);
            }
            else if (clazz.Equals(typeof(long)) || clazz.Equals(typeof(Int64)) || clazz.Equals(typeof(long?)) || clazz.Equals(typeof(Int64?)))
            {
                returnObj = Convert.ToInt64(propertyObj);
            }
            else if (clazz.Equals(typeof(short)) || clazz.Equals(typeof(Int16)) || clazz.Equals(typeof(short?)) || clazz.Equals(typeof(Int16?)))
            {
                returnObj = Convert.ToInt16(propertyObj);
            }
            else if (clazz.Equals(typeof(double)) || clazz.Equals(typeof(Double)) || clazz.Equals(typeof(double?)) || clazz.Equals(typeof(Double?)))
            {
                returnObj = Convert.ToDouble(propertyObj);
            }
            else if (clazz.Equals(typeof(float)) || clazz.Equals(typeof(Single)) || clazz.Equals(typeof(float?)) || clazz.Equals(typeof(Single?)))
            {
                returnObj = Convert.ToSingle(propertyObj);
            }
            else if (clazz == (typeof(DateTime)) || clazz == typeof(DateTime?))
            {
                returnObj = Convert.ToDateTime(propertyObj);
            }
            else if (clazz == (typeof(string)) || clazz == typeof(String))
            {
                returnObj = Convert.ToString(propertyObj);
            }
            else if (clazz == (typeof(decimal)) || clazz == typeof(Decimal) || clazz == typeof(decimal?) || clazz == typeof(Decimal?))
            {
                returnObj = Convert.ToDecimal(propertyObj);
            }
            else if (clazz == typeof(object) || clazz == typeof(Object))
            {
                returnObj = propertyObj;
            }
            else
            {
                NHExt.Runtime.Logger.LoggerHelper.Debug("json类型转换过程中值类型没有匹配", NHExt.Runtime.Logger.LoggerInstance.RuntimeLogger);
                returnObj = propertyObj;
            }
            return returnObj;
        }
        private static object ConvertObjectToObject(JToken node, Type clazz)
        {
            if (node == null)
            {
                return null;
            }
            object value = null;
            // 数组
            if (clazz.IsArray)
            {
                value = JsonSerialize.ConvertObjectToArray(node, clazz);
            }
            //泛型列表
            else if (IsCollection(clazz))
            {
                value = JsonSerialize.ConvertObjectToCollection(node, clazz);
            }
            // 实体类型
            else if (clazz.IsClass)
            {
                value = JsonSerialize.ConvertObjectToEntity(node, clazz);
            }
            else
            {
                value = JsonSerialize.convertObjectToValue(node, clazz);
            }
            return value;
        }
        private static object ConvertObjectToEntity(JToken jt, Type clazz)
        {
            if (!clazz.IsClass)
            {
                throw new NHExt.Runtime.Exceptions.RuntimeException("当前属性类型不是引用类型");
            }
            JObject jObj = jt as JObject;
            if (jObj == null) return null;

            object entityInstance = Activator.CreateInstance(clazz);
            NHExt.Runtime.Model.BaseDTO entityDTO = entityInstance as NHExt.Runtime.Model.BaseDTO;
            // node节点包含的属性名称的迭代器
            PropertyInfo[] properties = clazz.GetProperties();
            foreach (PropertyInfo pi in properties)
            {
                // 获取当前属性名称
                JToken token = GetTokenByPropertyName(jt, pi.Name);
                if (token == null || token.Type == JTokenType.Null)
                {
                    continue;
                }
                Type dt = pi.PropertyType;
                try
                {
                    object obj = null;
                    if ((token.Type != JTokenType.Object && token.Type != JTokenType.Array) || (dt.IsValueType || dt.Equals(typeof(string)) || dt.Equals(typeof(String))))
                    {
                        obj = JsonSerialize.convertObjectToValue(token, dt);
                    }
                    else
                    {
                        // 集合属性，一对多关系
                        obj = JsonSerialize.ConvertObjectToObject(token, dt);
                    }
                    if (obj != null)
                    {
                        if (entityDTO == null)
                        {
                            pi.SetValue(entityInstance, obj, null);
                        }
                        else
                        {
                            entityDTO.SetValue(obj, pi.Name);
                        }
                    }
                }
                catch (NHExt.Runtime.Exceptions.RuntimeException e)
                {
                    NHExt.Runtime.Logger.LoggerHelper.Error(e);
                    NHExt.Runtime.Exceptions.RuntimeException ex = new NHExt.Runtime.Exceptions.RuntimeException("序列化对象失败，字段名称" + pi.Name + "，类型" + clazz.FullName);
                    throw ex;
                }
            }
            return entityInstance;

        }
        private static IList ConvertObjectToCollection(JToken jt, Type clazz)
        {
            if (jt == null)
            {
                return null;
            }
            if (!(jt is JArray))
            {
                return null;
            }
            JArray arrNode = (JArray)jt;
            int size = arrNode.Count;
            IList list = null;
            Type elementType = null;
            if (clazz.IsInterface)
            {
                elementType = clazz.IsGenericType ? clazz.GetGenericArguments()[0] : typeof(object);
                Type collType = typeof(List<>).MakeGenericType(elementType);
                list = (IList)Activator.CreateInstance(collType, size);
            }
            else if (clazz.IsClass)
            {
                MethodInfo addInfo = clazz.GetMethod("Add");
                if (addInfo == null)
                {
                    throw new NHExt.Runtime.Exceptions.RuntimeException("无法获取到泛型函数的add方法");
                }
                ParameterInfo[] parameters = addInfo.GetParameters();
                elementType = parameters[0].ParameterType;
                list = (IList)Activator.CreateInstance(clazz);
            }

            if (elementType == null || list == null)
            {
                throw new NHExt.Runtime.Exceptions.RuntimeException("反序列化过程失败，动态创建泛型错误");
            }

            for (int i = 0; i < size; i++)
            {
                object item = JsonSerialize.DeSerialize(arrNode[i], elementType);
                if (item != null)
                {
                    list.Add(item);
                }
            }
            return list;
        }
        private static object ConvertObjectToArray(object node, Type clazz)
        {
            if (node == null)
            {
                return null;
            }
            if (!(node is JArray))
            {
                return null;
            }
            JArray arrNode = (JArray)node;
            int size = arrNode.Count;

            Type elementType = clazz.GetElementType();
            Array arr = Array.CreateInstance(elementType, size);

            for (int i = 0; i < size; i++)
            {
                object item = JsonSerialize.ConvertObjectToObject(arrNode[i], elementType);
                if (item != null)
                {
                    arr.SetValue(item, i);
                }
            }
            return arr;
        }

        /// <summary>
        /// 判断一个类型是否是集合
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private static bool IsCollection(Type type)
        {
            bool isCollection = false;

            if (type.IsGenericType)
            {
                Type t = null;
                Type genericTemplateType = typeof(ICollection<>);
                Type genericTypeParameter = type.GetGenericArguments()[0];
                t = genericTemplateType.MakeGenericType(genericTypeParameter);

                if (t.IsAssignableFrom(type))
                {
                    isCollection = true;
                }
            }
            else
            {
                if (typeof(ICollection).IsAssignableFrom(type))
                {
                    isCollection = true;
                }
            }
            return isCollection;
        }

        #endregion

        public static JToken GetTokenByPropertyName(JToken token, string propName)
        {
            //如果存在的话返回回去的应该是一个JValue或者JObject或者JArray
            if (token is JObject)
            {
                return token[propName];
            }
            return null;
        }

    }
}
