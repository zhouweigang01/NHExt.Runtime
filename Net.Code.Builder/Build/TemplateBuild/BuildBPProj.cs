using Net.Code.Builder.Base;
using Net.Code.Builder.Build.BPBuild;
using Net.Code.Builder.Build.Model;
using Net.Code.Builder.Enums;
using Net.Code.Builder.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

namespace Net.Code.Builder.Build.TemplateBuild
{
    public class BuildBPProj : IBuild
    {
        private BPProj _proj;
        public BuildBPProj(BPProj proj)
        {
            _proj = proj;
        }


        public string BuildCode()
        {
            CodeBuilder builder = new CodeBuilder();

            TemplateDTO.Solution solution = new TemplateDTO.Solution();


            string basePath = GetBasePath();
            StreamWriter sw = null;

            #region 生成 Deploy 部分代码

            #region 生成生成Deploy代码过程中可能用到的文件夹
            string projPath = basePath + @"\Deploy\";
            string assemblyPath = projPath + @"Properties\";
            string codePath = projPath + @"Entity\";
            string extendCodePath = projPath + @"Extend\";
            if (!Directory.Exists(projPath))
            {
                Directory.CreateDirectory(projPath);
            }
            if (!Directory.Exists(assemblyPath))
            {
                Directory.CreateDirectory(assemblyPath);
            }
            if (!Directory.Exists(codePath))
            {
                Directory.CreateDirectory(codePath);
            }
            if (!Directory.Exists(extendCodePath))
            {
                Directory.CreateDirectory(extendCodePath);
            }
            #endregion

            #region 生成csproj项目文件

            string projGuid = string.Empty;
            string projName = projPath + _proj.Namespace + "." + DTOEntity.AssemblyEndTag + ".csproj";
            if (File.Exists(projName))
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(projName);
                XmlNamespaceManager nsMgr = new XmlNamespaceManager(xmlDoc.NameTable);
                nsMgr.AddNamespace("ns", "http://schemas.microsoft.com/developer/msbuild/2003");
                XmlNode node = xmlDoc.SelectSingleNode("/ns:Project/ns:PropertyGroup/ns:ProjectGuid", nsMgr);
                projGuid = node.InnerText;
                projGuid = projGuid.Substring(1, projGuid.Length - 2);
            }
            if (string.IsNullOrEmpty(projGuid))
            {
                projGuid = Guid.NewGuid().ToString().ToUpper();
            }

            TemplateDTO.Project TProj = new TemplateDTO.Project(this._proj, TemplateDTO.ProjType.Deploy);
            TProj.Guid = projGuid;

            solution.AddProj(TProj);

            if (!File.Exists(projName))
            {
                string csprojString = builder.BuildCode("Project", TProj);
                sw = new StreamWriter(projName);
                sw.Write(csprojString);
                sw.Close();
            }
            //生成targets文件
            string targetsString = builder.BuildCode("ProjectTargets", TProj);
            sw = new StreamWriter(projPath + "csproj.targets");
            sw.Write(targetsString);
            sw.Close();

            #endregion

            #region 生成assembly信息
            string assemblyString = builder.BuildCode("ProjectAssembly", TProj);
            sw = new StreamWriter(assemblyPath + "AssemblyInfo.cs", false);
            sw.Write(assemblyString);
            sw.Close();
            #endregion

            #region 针对每个DTO生成代码
            foreach (DTOEntity entity in _proj.DTOList)
            {
                TemplateDTO.BEEntityDTO tEntity = new TemplateDTO.BEEntityDTO(entity);

                Net.Code.Builder.Util.OutPut.OutPutMsg("生成实体【" + entity.Code + "】代码……");

                #region 生成实体代码
                string entityCode = builder.BuildCode("EntityDTO", tEntity);
                sw = new StreamWriter(codePath + entity.Code + ".cs", false);
                sw.Write(entityCode);
                sw.Close();
                #endregion

                #region 生成实体扩展
                if (!File.Exists(extendCodePath + entity.Code + "Extend.cs"))
                {
                    string entityExtendCode = builder.BuildCode("EntityDTOExtend", tEntity);
                    sw = new StreamWriter(extendCodePath + entity.Code + "Extend.cs", false);
                    sw.Write(entityExtendCode);
                    sw.Close();
                }
                #endregion

                Net.Code.Builder.Util.OutPut.OutPutMsg("实体【" + entity.Code + "】代码生成成功……");
            }

            #endregion


            #endregion

            #region 生成 Agent 部分代码

            #region 生成生成Agent代码过程中可能用到的文件夹

            projPath = basePath + @"\Agent\";
            assemblyPath = projPath + @"Properties\";
            codePath = projPath + @"Proxy\";
            if (!Directory.Exists(projPath))
            {
                Directory.CreateDirectory(projPath);
            }
            if (!Directory.Exists(assemblyPath))
            {
                Directory.CreateDirectory(assemblyPath);
            }
            if (!Directory.Exists(codePath))
            {
                Directory.CreateDirectory(codePath);
            }
            #endregion

            #region 生成csproj项目文件
            projGuid = string.Empty;
            projName = projPath + _proj.Namespace + "." + BPProj.AgentTag + ".csproj";

            if (File.Exists(projName))
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(projName);
                XmlNamespaceManager nsMgr = new XmlNamespaceManager(xmlDoc.NameTable);
                nsMgr.AddNamespace("ns", "http://schemas.microsoft.com/developer/msbuild/2003");
                XmlNode node = xmlDoc.SelectSingleNode("/ns:Project/ns:PropertyGroup/ns:ProjectGuid", nsMgr);
                projGuid = node.InnerText;
                projGuid = projGuid.Substring(1, projGuid.Length - 2);
            }
            if (string.IsNullOrEmpty(projGuid))
            {
                projGuid = Guid.NewGuid().ToString().ToUpper();
            }

            TProj = new TemplateDTO.Project(this._proj, TemplateDTO.ProjType.Agent);
            TProj.Guid = projGuid;

            solution.AddProj(TProj);

            if (!File.Exists(projName))
            {
                string csprojString = builder.BuildCode("Project", TProj);
                sw = new StreamWriter(projName);
                sw.Write(csprojString);
                sw.Close();
            }
            //生成targets文件
            targetsString = builder.BuildCode("ProjectTargets", TProj);
            sw = new StreamWriter(projPath + "csproj.targets");
            sw.Write(targetsString);
            sw.Close();

            #endregion

            #region 生成assembly信息
            assemblyString = builder.BuildCode("ProjectAssembly", TProj);
            sw = new StreamWriter(assemblyPath + "AssemblyInfo.cs", false);
            sw.Write(assemblyString);
            sw.Close();
            #endregion

            #region 针对每个实体生成Agent代码
            foreach (BPEntity entity in _proj.EntityList)
            {
                Net.Code.Builder.Util.OutPut.OutPutMsg("生成实体【" + entity.Code + "】代码……");

                TemplateDTO.BPEntity tEntity = new TemplateDTO.BPEntity(entity);

                #region 生成实体代码

                string agentCode = builder.BuildCode("BPAgent", tEntity);
                sw = new StreamWriter(codePath + entity.Code + "Agent.cs", false);
                sw.Write(agentCode);
                sw.Close();
                #endregion

                Net.Code.Builder.Util.OutPut.OutPutMsg("实体【" + entity.Code + "】代码生成成功……");
            }
            #endregion

            #endregion

            #region 生成BP部分代码


            #region 生成生成BP代码过程中可能用到的文件夹

            projPath = basePath + @"\BP\";
            assemblyPath = projPath + @"Properties\";
            codePath = projPath + @"BPEntity\";
            extendCodePath = projPath + @"Extend\";
            if (!Directory.Exists(projPath))
            {
                Directory.CreateDirectory(projPath);
            }
            if (!Directory.Exists(assemblyPath))
            {
                Directory.CreateDirectory(assemblyPath);
            }
            if (!Directory.Exists(codePath))
            {
                Directory.CreateDirectory(codePath);
            }
            if (!Directory.Exists(extendCodePath))
            {
                Directory.CreateDirectory(extendCodePath);
            }
            #endregion

            #region 生成csproj项目文件

            projGuid = string.Empty;
            projName = projPath + _proj.Namespace + ".csproj";
            if (File.Exists(projName))
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(projName);
                XmlNamespaceManager nsMgr = new XmlNamespaceManager(xmlDoc.NameTable);
                nsMgr.AddNamespace("ns", "http://schemas.microsoft.com/developer/msbuild/2003");
                XmlNode node = xmlDoc.SelectSingleNode("/ns:Project/ns:PropertyGroup/ns:ProjectGuid", nsMgr);
                projGuid = node.InnerText;
                projGuid = projGuid.Substring(1, projGuid.Length - 2);
            }
            if (string.IsNullOrEmpty(projGuid))
            {
                projGuid = Guid.NewGuid().ToString().ToUpper();
            }

            TProj = new TemplateDTO.Project(this._proj, TemplateDTO.ProjType.BP);
            TProj.Guid = projGuid;

            solution.AddProj(TProj);

            if (!File.Exists(projName))
            {
                string csprojString = builder.BuildCode("Project", TProj);
                sw = new StreamWriter(projName);
                sw.Write(csprojString);
                sw.Close();
            }
            //生成targets文件
            targetsString = builder.BuildCode("ProjectTargets", TProj);
            sw = new StreamWriter(projPath + "csproj.targets");
            sw.Write(targetsString);
            sw.Close();

            #endregion

            #region 生成assembly信息
            assemblyString = builder.BuildCode("ProjectAssembly", TProj);
            sw = new StreamWriter(assemblyPath + "AssemblyInfo.cs", false);
            sw.Write(assemblyString);
            sw.Close();
            #endregion

            #region 针对每个BP实体生成实体代码
            foreach (BPEntity entity in _proj.EntityList)
            {
                OutPut.OutPutMsg("生成实体【" + entity.Code + "】代码……");

                TemplateDTO.BPEntity tEntity = new TemplateDTO.BPEntity(entity);

                string bpCode = builder.BuildCode("BPProxy", tEntity);

                #region 生成BP实体代码
                sw = new StreamWriter(codePath + entity.Code + ".cs", false);
                sw.Write(bpCode);
                sw.Close();
                #endregion

                #region 生成BP实体扩展
                if (!File.Exists(extendCodePath + entity.Code + "Extend.cs"))
                {
                    string bpCodeExtend = builder.BuildCode("BPExtend", tEntity);
                    sw = new StreamWriter(extendCodePath + entity.Code + "Extend.cs", false);
                    sw.Write(bpCodeExtend);
                    sw.Close();
                }
                #endregion

                OutPut.OutPutMsg("实体【" + entity.Code + "】代码生成成功……");
            }
            #endregion


            #endregion

            #region 生成solution文件
            string slnName = basePath + "\\" + _proj.Namespace + ".sln";
            if (!File.Exists(slnName))
            {
                string solutionString = builder.BuildCode("Solution", solution);
                sw = new StreamWriter(slnName);
                sw.Write(solutionString);
                sw.Close();
            }
            #endregion

            this.BuildCfgXML(null);

            return string.Empty;
        }
        /// <summary>
        /// 生成BP服务配置文件，该文件独立于BP存在
        /// </summary>
        /// <param name="xmlDoc"></param>
        /// <returns></returns>
        public System.Xml.XmlElement BuildCfgXML(System.Xml.XmlDocument xmlDoc)
        {
            #region 生成生成代码过程中可能用到的文件夹

            string basePath = GetBasePath();

            string cfgPath = basePath + @"\..\..\Script\";
            if (!Directory.Exists(cfgPath))
            {
                Directory.CreateDirectory(cfgPath);
            }

            #endregion
            xmlDoc = new XmlDocument();
            XmlDeclaration xmlDeclar = xmlDoc.CreateXmlDeclaration("1.0", "UTF-8", "yes");
            xmlDoc.AppendChild(xmlDeclar);
            XmlElement el = xmlDoc.CreateElement("ServiceCfg");
            el.SetAttribute("NS", this._proj.Namespace);
            XmlElement refEls = xmlDoc.CreateElement("Refrences");
            foreach (ProjectRefrence pr in _proj.RefrenceList)
            {
                if (pr.RefrenceType != RefType.BEEntity) continue;

                XmlElement refEl = xmlDoc.CreateElement("Refrence");
                refEl.SetAttribute("Assembly", pr.AssemblyName);
                refEls.AppendChild(refEl);
            }
            el.AppendChild(refEls);
            XmlElement svcEls = xmlDoc.CreateElement("Services");
            foreach (BPEntity entity in this._proj.EntityList)
            {
                BuildBP bc = new BuildBP(entity);
                svcEls.AppendChild(bc.BuildCfgXML(xmlDoc));
            }
            el.AppendChild(svcEls);
            xmlDoc.AppendChild(el);
            xmlDoc.Save(cfgPath + _proj.Namespace + ".svc");
            return null;
        }

        public string BuildMSSQL()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 获取实体代码的基本目录
        /// </summary>
        /// <returns></returns>
        private string GetBasePath()
        {
            string basePath = this._proj.ProjPath;
            //  basePath = Path.GetDirectoryName(basePath);
            return this._proj.CodePath + _proj.Namespace;
        }

        public string BuildMetaData()
        {
            string basePath = GetBasePath();

            string scriptPath = basePath + @"\..\..\Script\";
            if (!Directory.Exists(scriptPath))
            {
                Directory.CreateDirectory(scriptPath);
            }

            string buildSql = string.Empty;

            buildSql += "--实体项目元数据sql" + Environment.NewLine;
            buildSql += "DELETE FROM T_METADATA_PROJECT_COMPONENT WHERE F_GUID='" + this._proj.Guid + "'" + Environment.NewLine;
            buildSql += "INSERT INTO T_METADATA_PROJECT_COMPONENT VALUES((SELECT ISNULL(MAX(F_ID),0)+1 FROM T_METADATA_PROJECT_COMPONENT),0,'" + this._proj.Guid + "','" + this._proj.ProjName + "','" + this._proj.Namespace + "',1,'" + this._proj.Namespace + ".dll" + "')" + Environment.NewLine;
            buildSql += "--生成单个实体的元数据sql" + Environment.NewLine;
            foreach (Floder floder in _proj.FloderList)
            {
                buildSql += floder.BuildMetaData();
            }
            foreach (BPEntity entity in _proj.EntityList)
            {
                BuildBP be = new BuildBP(entity);
                buildSql += be.BuildMetaData();
            }
            StreamWriter sw = new StreamWriter(scriptPath + this._proj.ProjName + "MetaData.sql", false, Encoding.Unicode);
            sw.Write(buildSql);
            sw.Close();
            return string.Empty;
        }
    }
}
