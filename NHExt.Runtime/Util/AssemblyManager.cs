using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Collections;
using NHExt.Runtime.Web;
using System.IO;

namespace NHExt.Runtime.Util
{
    public enum AssemblyTypeEnum
    {
        Proxy = 1,
        BP = 2,
        PlugIn = 3
    }

    public class AssemblyLoader : MarshalByRefObject
    {
        private Assembly _assembly;

        public void LoadFrom(string dllName)
        {
            string assemblyPath;
            if (File.Exists(Cfg.AppBinPath + dllName))
            {
                assemblyPath = Cfg.AppBinPath + dllName;
            }
            else if (File.Exists(Cfg.AppLibPath + dllName))
            {
                assemblyPath = Cfg.AppLibPath + dllName;
            }
            else if (File.Exists(Cfg.AppPath + dllName))
            {
                assemblyPath = Cfg.AppPath + dllName;
            }
            else if (File.Exists(Cfg.PlugInPath + dllName))
            {
                assemblyPath = Cfg.PlugInPath + dllName;
            }
            else
            {
                throw new NHExt.Runtime.Exceptions.RuntimeException("代理程序集" + dllName + "在程序中不存在");

            }
            _assembly = Assembly.LoadFile(assemblyPath);
        }

        public string FullName
        {
            get { return _assembly.FullName; }
        }
        public Assembly Assembly
        {
            get
            {
                return _assembly;
            }
        }
    }
    public static class AssemblyManager
    {
        private static Hashtable _proxyAssemblyList = new Hashtable();
        private static Hashtable _BPAssemblyList = new Hashtable();
        private static Hashtable _plugInAssemblyList = new Hashtable();

        private static object Locker = new object();
        static AssemblyManager()
        {

        }
        public static Assembly GetAssembly(string dllName, AssemblyTypeEnum type)
        {
            AssemblyLoader loader = null;
            lock (Locker)
            {
                Hashtable tmpList = null;
                if (type == AssemblyTypeEnum.Proxy)
                {
                    tmpList = _proxyAssemblyList;
                }
                else if (type == AssemblyTypeEnum.BP)
                {
                    tmpList = _BPAssemblyList;
                }
                else if (type == AssemblyTypeEnum.PlugIn)
                {
                    tmpList = _plugInAssemblyList;
                }

                if (tmpList.ContainsKey(dllName))
                {
                    loader = tmpList[dllName] as AssemblyLoader;
                }
                else
                {
                    loader = new AssemblyLoader();
                    loader.LoadFrom(dllName);
                    tmpList.Add(dllName, loader);
                }
            }
            return loader.Assembly;
        }

        public static Type GetType(Assembly assembly, string typeName)
        {
            Type type = assembly.GetType(typeName);
            if (type == null)
            {
                string errMsg = "程序集" + assembly.FullName + "中不存在类型" + typeName;
                throw new NHExt.Runtime.Exceptions.RuntimeException(errMsg);
            }
            return type;
        }

        public static T CreateInstance<T>(Assembly assembly, string typeName) where T : class
        {
            Type type = assembly.GetType(typeName);
            if (type == null)
            {
                string errMsg = "程序集" + assembly.FullName + "中不存在类型" + typeName;
                throw new NHExt.Runtime.Exceptions.RuntimeException(errMsg);
            }
            T obj = Activator.CreateInstance(type) as T;
            return obj;
        }


        public static object CreateInstance(Assembly assembly, string typeName)
        {
            Type type = assembly.GetType(typeName);
            if (type == null)
            {
                string errMsg = "程序集" + assembly.FullName + "中不存在类型" + typeName;
                throw new NHExt.Runtime.Exceptions.RuntimeException(errMsg);
            }
            object obj = Activator.CreateInstance(type);
            return obj;
        }

    }

}
