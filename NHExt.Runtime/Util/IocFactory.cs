using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace NHExt.Runtime.Util
{

    static class IocFactory
    {
        #region spring.net IOC  封装函数
        private static Spring.Objects.Factory.IObjectFactory _IocFactory = null;
        /// <summary>
        /// 使用SPRING进行IOC返回实体，返回的值有可能为空，外面需要进行非空判断
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="arguments"></param>
        /// <returns></returns>
        public static T GetIocObject<T>(string key, params object[] arguments) where T : class
        {
            if (IocFactory._IocFactory == null)
            {
                string filePath = NHExt.Runtime.Cfg.AppCfgPath + "SPRING_CONFIG.xml";
                Spring.Context.IApplicationContext ctx = null;
                if (File.Exists(filePath))
                {

                    ctx = new Spring.Context.Support.XmlApplicationContext("file://" + filePath);
                    IocFactory._IocFactory = (Spring.Objects.Factory.IObjectFactory)ctx;
                }

            }
            if (IocFactory._IocFactory != null)
            {
                lock (IocFactory._IocFactory)
                {
                    object obj = IocFactory._IocFactory.GetObject(key, arguments);
                    return (T)obj;
                }
            }
            return null;
        }
        #endregion
    }
}
