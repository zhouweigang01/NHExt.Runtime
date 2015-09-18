using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TextTemplating;
using System.IO;
using System.CodeDom.Compiler;
using System.Windows.Forms;
using System.Configuration;

namespace Net.Code.Builder.Build
{
    public class CodeBuilder
    {
        protected static Dictionary<string, string> TemplateCache = new Dictionary<string, string>();
        protected static Dictionary<string, string> TemplatePath = new Dictionary<string, string>();
        private string key = string.Empty;
        private string getTpl(string key)
        {
            string tpl = string.Empty;
            lock (CodeBuilder.TemplateCache)
            {
                if (!CodeBuilder.TemplateCache.ContainsKey(key))
                {
                    string filePath = ConfigurationManager.AppSettings[key];
                    if (!string.IsNullOrEmpty(filePath))
                    {
                        if (File.Exists(filePath))
                        {
                            tpl = File.ReadAllText(filePath);
                            CodeBuilder.TemplateCache.Add(key, tpl);
                        }
                    }
                }
                else
                {
                    tpl = CodeBuilder.TemplateCache[key];
                }
            }
            return tpl;
        }
        private string getFilePath(string key)
        {
            string filePath = string.Empty;
            lock (CodeBuilder.TemplatePath)
            {
                if (!CodeBuilder.TemplatePath.ContainsKey(key))
                {
                    filePath = ConfigurationManager.AppSettings[key];
                    if (!string.IsNullOrEmpty(filePath))
                    {
                        if (File.Exists(filePath))
                        {
                            CodeBuilder.TemplatePath.Add(key, filePath);
                        }
                    }
                }
                else
                {
                    filePath = CodeBuilder.TemplatePath[key];
                }
            }
            return filePath;
        }

        public string BuildCode(string key, object entity)
        {
            TextEngineHost host = new TextEngineHost();
            host.TemplateFileValue = this.getFilePath(key);
            host.Session = new TextTemplatingSession();

            host.Session.Add("entity", entity);
            string tpl = this.getTpl(key);
            string output = new Engine().ProcessTemplate(tpl, host);
            if (host.Errors.Count > 0)
            {
                StringBuilder errorWarn = new StringBuilder();
                foreach (CompilerError error in host.Errors)
                {
                    errorWarn.Append(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss ------------------")).Append(error.Line).Append(":").AppendLine(error.ErrorText);
                }
                if (!File.Exists("Error.log"))
                {
                    File.Create("Error.log");
                }
                StreamWriter sw = new StreamWriter("Error.log", true);
                sw.Write(errorWarn.ToString());
                sw.Close();
            }
            return output;
        }
    }
}
