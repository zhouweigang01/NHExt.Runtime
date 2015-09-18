using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using System.ServiceModel.Description;
using System.CodeDom.Compiler;

namespace NHExt.Runtime.Web
{
    public class WebServiceInvoker
    {
        /// < summary> 
        /// 动态调用web服务 
        /// < /summary> 
        /// < param name="url">WSDL服务地址< /param> 
        /// < param name="methodname">方法名< /param> 
        /// < param name="args">参数< /param> 
        /// < returns>< /returns> 
        public static object Invoke(string url, string methodname, object[] args)
        {
            return WebServiceInvoker.Invoke(url, null, methodname, args);
        }
        /// < summary> 
        /// 动态调用web服务 
        /// < /summary> 
        /// < param name="url">WSDL服务地址< /param> 
        /// < param name="classname">类名< /param> 
        /// < param name="methodname">方法名< /param> 
        /// < param name="args">参数< /param> 
        /// < returns>< /returns> 
        public static object Invoke(string url, string classname, string methodname, object[] args)
        {
            string ns = "EnterpriseServerBase.WebService.DynamicWebCalling";
            if ((classname == null) || (classname == ""))
            {
                classname = WebServiceInvoker.GetWsClassName(url);
            }
            System.Net.WebClient wc = new System.Net.WebClient();
            System.IO.Stream stream = wc.OpenRead(url + "?WSDL");
            System.Web.Services.Description.ServiceDescription sd = System.Web.Services.Description.ServiceDescription.Read(stream);
            System.Web.Services.Description.ServiceDescriptionImporter sdi = new System.Web.Services.Description.ServiceDescriptionImporter();
            sdi.AddServiceDescription(sd, "", "");
            System.CodeDom.CodeNamespace cn = new System.CodeDom.CodeNamespace(ns);
            System.CodeDom.CodeCompileUnit ccu = new System.CodeDom.CodeCompileUnit();
            ccu.Namespaces.Add(cn);
            sdi.Import(cn, ccu);

            Microsoft.CSharp.CSharpCodeProvider csc = new Microsoft.CSharp.CSharpCodeProvider();
            // System.CodeDom.Compiler.ICodeCompiler csc = .CreateCompiler();

            System.CodeDom.Compiler.CompilerParameters cplist = new System.CodeDom.Compiler.CompilerParameters();
            cplist.GenerateExecutable = false;
            cplist.GenerateInMemory = true;
            cplist.ReferencedAssemblies.Add("System.dll");
            cplist.ReferencedAssemblies.Add("System.XML.dll");
            cplist.ReferencedAssemblies.Add("System.Web.Services.dll");
            cplist.ReferencedAssemblies.Add("System.Data.dll");

            System.CodeDom.Compiler.CompilerResults cr = csc.CompileAssemblyFromDom(cplist, ccu);
            if (true == cr.Errors.HasErrors)
            {
                System.Text.StringBuilder sb = new System.Text.StringBuilder();
                foreach (System.CodeDom.Compiler.CompilerError ce in cr.Errors)
                {
                    sb.Append(ce.ToString());
                    sb.Append(System.Environment.NewLine);
                }
                throw new NHExt.Runtime.Exceptions.RuntimeException("调用WebService开始编译发生错误:" + sb.ToString());
            }
            System.Reflection.Assembly assembly = cr.CompiledAssembly;
            Type t = assembly.GetType(ns + "." + classname, true, true);
            object obj = Activator.CreateInstance(t);
            System.Reflection.MethodInfo mi = t.GetMethod(methodname);
            return mi.Invoke(obj, args);
        }
        private static string GetWsClassName(string wsUrl)
        {
            string[] parts = wsUrl.Split('/');
            string[] pps = parts[parts.Length - 1].Split('.');
            return pps[0];
        }
    }
}

