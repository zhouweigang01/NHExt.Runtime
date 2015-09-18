using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Net.Code.Builder.Base;
using Net.Code.Builder.Build.DTOBuild;
using Net.Code.Builder.Build.EnumBuild;
using Net.Code.Builder.Build.Model;
using Net.Code.Builder.Build.Tpl;
using Net.Code.Builder.Util;
using Net.Code.Builder.Enums;
using System.Xml;

namespace Net.Code.Builder.Build.BPBuild
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
            string afterBuildEvent = "copy /y \"$(TargetDir){0}.dll\" \"$(SolutionDir)..\\..\\ApplicationLib\\\"" + Environment.NewLine;
            afterBuildEvent += "copy /y \"$(TargetDir){0}.pdb\" \"$(SolutionDir)..\\..\\ApplicationLib\\\"" + Environment.NewLine;
            string projectRefrence = string.Empty;
            string projectSelection = string.Empty;
            string deployProjectRefrence = string.Empty;
            string deployPojectSelection = string.Empty;
            string agentProjectRefrence = string.Empty;
            string agentProjectSelection = string.Empty;
            string bpProjectRefrence = string.Empty;
            string bpProjectSelection = string.Empty;
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
                XmlNode node = xmlDoc.SelectSingleNode("/ns:Project/ns:PropertyGroup/ns:ProjectGuid",nsMgr);
                projGuid = node.InnerText;
                projGuid = projGuid.Substring(1, projGuid.Length - 2);
            }
            if (string.IsNullOrEmpty(projGuid))
            {
                projGuid = Guid.NewGuid().ToString().ToUpper();
            }
            deployProjectRefrence += "Project(\"{" + Guid.NewGuid().ToString() + "}\") = \"" + _proj.Namespace + "." + DTOEntity.AssemblyEndTag + "\", \"Deploy\\" + _proj.Namespace + "." + DTOEntity.AssemblyEndTag + ".csproj \", \"{" + projGuid + "}\"" + Environment.NewLine;
            deployProjectRefrence += "EndProject";
            deployPojectSelection += "{" + projGuid + "}.Debug|Any CPU.ActiveCfg = Debug|Any CPU" + Environment.NewLine;
            deployPojectSelection += "{" + projGuid + "}.Debug|Any CPU.Build.0 = Debug|Any CPU" + Environment.NewLine;
            deployPojectSelection += "{" + projGuid + "}.Release|Any CPU.ActiveCfg = Debug|Any CPU" + Environment.NewLine;
            deployPojectSelection += "{" + projGuid + "}.Release|Any CPU.Build.0 = Debug|Any CPU";

            projectRefrence += deployProjectRefrence + Environment.NewLine;
            projectSelection += deployPojectSelection + Environment.NewLine;

            string targetFiles = string.Empty;
            string extendFiles = string.Empty;
            foreach (DTOEntity entity in _proj.DTOList)
            {
                if (entity.DataState != DataState.Delete)
                {
                    targetFiles += "<Compile Include=\"Entity\\" + entity.Code + ".cs\" />" + Environment.NewLine;
                    extendFiles += "<Compile Include=\"Extend\\" + entity.Code + "Extend.cs\" />" + Environment.NewLine;
                }
            }
            foreach (EnumEntity entity in _proj.EnumList)
            {
                if (entity.DataState != DataState.Delete)
                {
                    targetFiles += "<Compile Include=\"Entity\\" + entity.Code + ".cs\" />" + Environment.NewLine;
                    extendFiles += "<Compile Include=\"Extend\\" + entity.Code + "Extend.cs\" />" + Environment.NewLine;
                }
            }
            //实体引用dll
            string compileDlls = "<Reference Include=\"NHExt.Runtime\">" + Environment.NewLine;
            compileDlls += "<HintPath>..\\..\\..\\Runtime\\NHExt.Runtime.dll</HintPath>" + Environment.NewLine;
            compileDlls += "</Reference>" + Environment.NewLine;
            foreach (ProjectRefrence pr in _proj.RefrenceList)
            {
                //deploy中只能引用deploy和agent
                if (pr.RefrenceType == RefType.BPEntity || pr.RefrenceType == RefType.BEEntity) continue;
                //if (pr.RefProjName == _proj.ProjName + ".bp")
                if (pr.RefProjName == _proj.FileName)
                {
                    continue;
                }
                compileDlls += "<Reference Include=\"" + pr.AssemblyName + "\">" + Environment.NewLine;
                compileDlls += "<HintPath>..\\..\\..\\Runtime\\" + System.IO.Path.GetDirectoryName(pr.RefFilePath) + "\\..\\ApplicationLib\\" + pr.Name + "</HintPath>" + Environment.NewLine;
                //compileDlls += "<Private>True</Private>" + Environment.NewLine;
                compileDlls += "</Reference>" + Environment.NewLine;
            }

            if (!File.Exists(projName))
            {
                string csprojString = global::Net.Code.Builder.Properties.Resources.csproj;
                csprojString = csprojString.Replace(Attributes.Guid, projGuid);
                csprojString = csprojString.Replace(Attributes.NameSpace, _proj.Namespace + "." + DTOEntity.AssemblyEndTag);
                csprojString = csprojString.Replace(Attributes.Assembly, _proj.Namespace + "." + DTOEntity.AssemblyEndTag);
                //导入生成的代码文件
                csprojString = csprojString.Replace(Attributes.CompileFile, extendFiles);
                //csprojString = csprojString.Replace(Attributes.RefrenceDll, compileDlls);
                //生成后时间拷贝dll到上层目录
                csprojString = csprojString.Replace(Attributes.PostBuild, string.Format(afterBuildEvent, _proj.Namespace + "." + DTOEntity.AssemblyEndTag));
                //将生成的资源文件嵌入到程序中去
                csprojString = csprojString.Replace(Attributes.EmbeddedResource, string.Empty);
                sw = new StreamWriter(projName);
                sw.Write(csprojString);
                sw.Close();
            }

            //生成targets文件，每次必须生成
            string targetsString = global::Net.Code.Builder.Properties.Resources.csprojtargets;
            targetsString = targetsString.Replace(Attributes.CompileFile, targetFiles);
            targetsString = targetsString.Replace(Attributes.RefrenceDll, compileDlls);
            targetsString = targetsString.Replace(Attributes.EmbeddedResource, string.Empty);

            sw = new StreamWriter(projPath + "csproj.targets");
            sw.Write(targetsString);
            sw.Close();

            #endregion

            #region 生成assembly信息
            string assemblyString = global::Net.Code.Builder.Properties.Resources.AssemblyInfo;
            assemblyString = assemblyString.Replace(Attributes.Guid, Guid.NewGuid().ToString());
            assemblyString = assemblyString.Replace(Attributes.Assembly, _proj.Namespace + "." + DTOEntity.AssemblyEndTag);
            assemblyString = assemblyString.Replace(Attributes.ProjCode, _proj.Namespace + "." + DTOEntity.AssemblyEndTag);
            sw = new StreamWriter(assemblyPath + "AssemblyInfo.cs", false);
            sw.Write(assemblyString);
            sw.Close();
            #endregion

            #region 针对每个DTO生成代码
            foreach (DTOEntity entity in _proj.DTOList)
            {
                OutPut.OutPutMsg("生成实体【" + entity.Code + "】代码……");

                #region 生成实体代码
                sw = new StreamWriter(codePath + entity.Code + ".cs", false);
                BuildDTO bc = new BuildDTO(entity);
                sw.Write(bc.BuildCode());
                sw.Close();
                #endregion

                #region 生成实体扩展
                if (!File.Exists(extendCodePath + entity.Code + "Extend.cs"))
                {
                    sw = new StreamWriter(extendCodePath + entity.Code + "Extend.cs", false);
                    sw.Write(bc.BuildExtendCode());
                    sw.Close();
                }
                #endregion

                OutPut.OutPutMsg("实体【" + entity.Code + "】代码生成成功……");
            }
            #endregion

            //#region 针对每个枚举生成实体代码
            //foreach (EnumEntity entity in _proj.EnumList)
            //{
            //    OutPut.OutPutMsg("生成实体【" + entity.Code + "】代码……");

            //    #region 生成实体代码
            //    sw = new StreamWriter(codePath + entity.Code + ".cs", false);
            //    BuildEnumDTOEntity bc = new BuildEnumDTOEntity(entity);
            //    sw.Write(bc.BuildCode());
            //    sw.Close();
            //    #endregion

            //    #region 生成实体扩展
            //    if (!File.Exists(extendCodePath + entity.Code + "Extend.cs"))
            //    {
            //        sw = new StreamWriter(extendCodePath + entity.Code + "Extend.cs", false);
            //        sw.Write(bc.BuildExtendCode());
            //        sw.Close();
            //    }
            //    #endregion

            //    OutPut.OutPutMsg("实体【" + entity.Code + "】代码生成成功……");
            //}
            //#endregion

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
            agentProjectRefrence += "Project(\"{" + Guid.NewGuid().ToString() + "}\") = \"" + _proj.Namespace + "." + BPProj.AgentTag + "\", \"Agent\\" + _proj.Namespace + "." + BPProj.AgentTag + ".csproj \", \"{" + projGuid + "}\"" + Environment.NewLine;
            agentProjectRefrence += "EndProject";
            agentProjectSelection += "{" + projGuid + "}.Debug|Any CPU.ActiveCfg = Debug|Any CPU" + Environment.NewLine;
            agentProjectSelection += "{" + projGuid + "}.Debug|Any CPU.Build.0 = Debug|Any CPU" + Environment.NewLine;
            agentProjectSelection += "{" + projGuid + "}.Release|Any CPU.ActiveCfg = Debug|Any CPU" + Environment.NewLine;
            agentProjectSelection += "{" + projGuid + "}.Release|Any CPU.Build.0 = Debug|Any CPU";

            projectRefrence += agentProjectRefrence + Environment.NewLine;
            projectSelection += agentProjectSelection + Environment.NewLine;

            targetFiles = string.Empty;
            extendFiles = string.Empty;
            foreach (BPEntity entity in _proj.EntityList)
            {
                if (entity.DataState != DataState.Delete)
                {
                    targetFiles += "<Compile Include=\"Proxy\\" + entity.Code + "Agent.cs\" />" + Environment.NewLine;
                }
            }

            //实体引用dll
            compileDlls = "<Reference Include=\"NHExt.Runtime\">" + Environment.NewLine;
            compileDlls += "<HintPath>..\\..\\..\\Runtime\\NHExt.Runtime.dll</HintPath>" + Environment.NewLine;
            compileDlls += "</Reference>" + Environment.NewLine;
            compileDlls += "<Reference Include=\"log4net\">" + Environment.NewLine;
            compileDlls += "<HintPath>..\\..\\..\\Runtime\\log4net.dll</HintPath>" + Environment.NewLine;
            compileDlls += "</Reference>" + Environment.NewLine;
            foreach (ProjectRefrence pr in _proj.RefrenceList)
            {
                //代理只能引用deploy
                if (pr.RefrenceType != RefType.Deploy) continue;
                compileDlls += "<Reference Include=\"" + pr.AssemblyName + "\">" + Environment.NewLine;
                compileDlls += "<HintPath>..\\..\\..\\Runtime\\" + System.IO.Path.GetDirectoryName(pr.RefFilePath) + "\\..\\ApplicationLib\\" + pr.Name + "</HintPath>" + Environment.NewLine;
                //compileDlls += "<Private>True</Private>" + Environment.NewLine;
                compileDlls += "</Reference>" + Environment.NewLine;
            }



            if (!File.Exists(projName))
            {
                string csprojString = global::Net.Code.Builder.Properties.Resources.csproj;
                csprojString = csprojString.Replace(Attributes.Guid, projGuid);
                csprojString = csprojString.Replace(Attributes.NameSpace, _proj.Namespace + "." + BPProj.AgentTag);
                csprojString = csprojString.Replace(Attributes.Assembly, _proj.Namespace + "." + BPProj.AgentTag);
                //导入生成的代码文件
                csprojString = csprojString.Replace(Attributes.CompileFile, extendFiles);
                //csprojString = csprojString.Replace(Attributes.RefrenceDll, compileDlls);
                //生成后时间拷贝dll到上层目录
                csprojString = csprojString.Replace(Attributes.PostBuild, string.Format(afterBuildEvent, _proj.Namespace + "." + BPProj.AgentTag));
                //将生成的资源文件嵌入到程序中去
                csprojString = csprojString.Replace(Attributes.EmbeddedResource, string.Empty);
                sw = new StreamWriter(projName);
                sw.Write(csprojString);
                sw.Close();
            }

            //生成targets文件，每次必须生成
            targetsString = global::Net.Code.Builder.Properties.Resources.csprojtargets;
            targetsString = targetsString.Replace(Attributes.CompileFile, targetFiles);
            targetsString = targetsString.Replace(Attributes.RefrenceDll, compileDlls);
            targetsString = targetsString.Replace(Attributes.EmbeddedResource, string.Empty);

            sw = new StreamWriter(projPath + "csproj.targets");
            sw.Write(targetsString);
            sw.Close();

            #endregion

            #region 生成assembly信息
            assemblyString = global::Net.Code.Builder.Properties.Resources.AssemblyInfo;
            assemblyString = assemblyString.Replace(Attributes.Guid, Guid.NewGuid().ToString());
            assemblyString = assemblyString.Replace(Attributes.Assembly, _proj.Namespace + "." + BPProj.AgentTag);
            assemblyString = assemblyString.Replace(Attributes.ProjCode, _proj.Namespace + "." + BPProj.AgentTag);
            sw = new StreamWriter(assemblyPath + "AssemblyInfo.cs", false);
            sw.Write(assemblyString);
            sw.Close();
            #endregion

            #region 针对每个实体生成Agent代码
            foreach (BPEntity entity in _proj.EntityList)
            {
                OutPut.OutPutMsg("生成实体【" + entity.Code + "】代码……");

                #region 生成实体代码
                sw = new StreamWriter(codePath + entity.Code + "Agent.cs", false);
                BuildBPAgent bc = new BuildBPAgent(entity);
                sw.Write(bc.BuildCode());
                sw.Close();
                #endregion

                OutPut.OutPutMsg("实体【" + entity.Code + "】代码生成成功……");
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
            bpProjectRefrence += "Project(\"{" + Guid.NewGuid().ToString() + "}\") = \"" + _proj.Namespace + "\", \"BP\\" + _proj.Namespace + ".csproj \", \"{" + projGuid + "}\"" + Environment.NewLine;
            bpProjectRefrence += "EndProject";
            bpProjectSelection += "{" + projGuid + "}.Debug|Any CPU.ActiveCfg = Debug|Any CPU" + Environment.NewLine;
            bpProjectSelection += "{" + projGuid + "}.Debug|Any CPU.Build.0 = Debug|Any CPU" + Environment.NewLine;
            bpProjectSelection += "{" + projGuid + "}.Release|Any CPU.ActiveCfg = Debug|Any CPU" + Environment.NewLine;
            bpProjectSelection += "{" + projGuid + "}.Release|Any CPU.Build.0 = Debug|Any CPU";

            projectRefrence += bpProjectRefrence;
            projectSelection += bpProjectSelection;

            targetFiles = string.Empty;
            extendFiles = string.Empty;
            foreach (BPEntity entity in _proj.EntityList)
            {
                if (entity.DataState != DataState.Delete)
                {
                    targetFiles += "<Compile Include=\"BPEntity\\" + entity.Code + ".cs\" />" + Environment.NewLine;
                    extendFiles += "<Compile Include=\"Extend\\" + entity.Code + "Extend.cs\" />" + Environment.NewLine;
                }
            }
            //foreach (EnumEntity entity in _proj.EnumList)
            //{
            //    if (entity.DataState != DataState.Delete)
            //    {
            //        targetFiles += "<Compile Include=\"BPEntity\\" + entity.Code + ".cs\" />" + Environment.NewLine;
            //        extendFiles += "<Compile Include=\"Extend\\" + entity.Code + "Extend.cs\" />" + Environment.NewLine;
            //    }
            //}
            //实体引用dll
            compileDlls = "<Reference Include=\"NHExt.Runtime\">" + Environment.NewLine;
            compileDlls += "<HintPath>..\\..\\..\\Runtime\\NHExt.Runtime.dll</HintPath>" + Environment.NewLine;
            compileDlls += "</Reference>" + Environment.NewLine;
            compileDlls += "<Reference Include=\"log4net\">" + Environment.NewLine;
            compileDlls += "<HintPath>..\\..\\..\\Runtime\\log4net.dll</HintPath>" + Environment.NewLine;
            compileDlls += "</Reference>" + Environment.NewLine;
            //比较奇怪是为什么需要引用nh的dll
            compileDlls += "<Reference Include=\"NHibernate\">" + Environment.NewLine;
            compileDlls += "<HintPath>..\\..\\..\\Runtime\\NHibernate.dll</HintPath>" + Environment.NewLine;
            compileDlls += "</Reference>" + Environment.NewLine;
            foreach (ProjectRefrence pr in _proj.RefrenceList)
            {
                //不需要引用自身
                //if (pr.RefProjName == _proj.ProjName + ".bp")
                if (pr.RefProjName == _proj.FileName)
                {
                    if (pr.RefrenceType == RefType.BPEntity) continue;
                }
                compileDlls += "<Reference Include=\"" + pr.AssemblyName + "\">" + Environment.NewLine;
                compileDlls += "<HintPath>..\\..\\..\\Runtime\\" + System.IO.Path.GetDirectoryName(pr.RefFilePath) + "\\..\\ApplicationLib\\" + pr.Name + "</HintPath>" + Environment.NewLine;
                //compileDlls += "<Private>True</Private>" + Environment.NewLine;
                compileDlls += "</Reference>" + Environment.NewLine;
            }

            if (!File.Exists(projName))
            {
                string csprojString = global::Net.Code.Builder.Properties.Resources.csproj;
                csprojString = csprojString.Replace(Attributes.Guid, projGuid);
                csprojString = csprojString.Replace(Attributes.NameSpace, _proj.Namespace);
                csprojString = csprojString.Replace(Attributes.Assembly, _proj.Namespace);
                //导入生成的代码文件
                csprojString = csprojString.Replace(Attributes.CompileFile, extendFiles);
                //csprojString = csprojString.Replace(Attributes.RefrenceDll, compileDlls);
                //生成后时间拷贝dll到上层目录
                csprojString = csprojString.Replace(Attributes.PostBuild, string.Format(afterBuildEvent, _proj.Namespace));
                //将生成的资源文件嵌入到程序中去
                csprojString = csprojString.Replace(Attributes.EmbeddedResource, string.Empty);
                sw = new StreamWriter(projName);
                sw.Write(csprojString);
                sw.Close();
            }

            //生成targets文件，每次必须生成
            targetsString = global::Net.Code.Builder.Properties.Resources.csprojtargets;
            targetsString = targetsString.Replace(Attributes.CompileFile, targetFiles);
            targetsString = targetsString.Replace(Attributes.RefrenceDll, compileDlls);
            targetsString = targetsString.Replace(Attributes.EmbeddedResource, string.Empty);

            sw = new StreamWriter(projPath + "csproj.targets");
            sw.Write(targetsString);
            sw.Close();
            #endregion

            #region 生成assembly信息
            assemblyString = global::Net.Code.Builder.Properties.Resources.AssemblyInfo;
            assemblyString = assemblyString.Replace(Attributes.Guid, Guid.NewGuid().ToString());
            assemblyString = assemblyString.Replace(Attributes.Assembly, _proj.Namespace + "." + DTOEntity.AssemblyEndTag);
            assemblyString = assemblyString.Replace(Attributes.ProjCode, _proj.Namespace + "." + DTOEntity.AssemblyEndTag);
            sw = new StreamWriter(assemblyPath + "AssemblyInfo.cs", false);
            sw.Write(assemblyString);
            sw.Close();
            #endregion

            #region 针对每个BP实体生成实体代码
            foreach (BPEntity entity in _proj.EntityList)
            {
                OutPut.OutPutMsg("生成实体【" + entity.Code + "】代码……");

                #region 生成BP实体代码
                sw = new StreamWriter(codePath + entity.Code + ".cs", false);
                BuildBP bc = new BuildBP(entity);
                sw.Write(bc.BuildCode());
                sw.Close();
                #endregion

                #region 生成BP实体扩展
                if (!File.Exists(extendCodePath + entity.Code + "Extend.cs"))
                {
                    sw = new StreamWriter(extendCodePath + entity.Code + "Extend.cs", false);
                    sw.Write(bc.BuildExtendCode());
                    sw.Close();
                }
                #endregion

                OutPut.OutPutMsg("实体【" + entity.Code + "】代码生成成功……");
            }
            #endregion

            //#region 针对每个枚举生成实体代码
            //foreach (EnumEntity entity in _proj.EnumList)
            //{
            //    OutPut.OutPutMsg("生成实体【" + entity.Code + "】代码……");

            //    #region 生成实体代码
            //    sw = new StreamWriter(codePath + entity.Code + ".cs", false);
            //    BuildEnumEntity bc = new BuildEnumEntity(entity);
            //    sw.Write(bc.BuildCode());
            //    sw.Close();
            //    #endregion

            //    #region 生成实体扩展
            //    if (!File.Exists(extendCodePath + entity.Code + "Extend.cs"))
            //    {
            //        sw = new StreamWriter(extendCodePath + entity.Code + "Extend.cs", false);
            //        sw.Write(bc.BuildExtendCode());
            //        sw.Close();
            //    }
            //    #endregion

            //    OutPut.OutPutMsg("实体【" + entity.Code + "】代码生成成功……");
            //}
            //#endregion
            #endregion

            #region 生成solution文件
            string slnName = basePath + "\\" + _proj.Namespace + ".sln";
            if (File.Exists(slnName))
            {
                File.Delete(slnName);
            }
            if (!File.Exists(slnName))
            {
                string solutionString = global::Net.Code.Builder.Properties.Resources.solution;
                solutionString = solutionString.Replace(Attributes.ProjectRefrence, projectRefrence);
                solutionString = solutionString.Replace(Attributes.ProjectSelection, projectSelection);
                sw = new StreamWriter(slnName);
                sw.Write(solutionString);
                sw.Close();
            }

            //slnName = basePath + "\\" + _proj.Namespace + ".Deploy.sln";
            //if (File.Exists(slnName))
            //{
            //    File.Delete(slnName);
            //}
            //if (!File.Exists(slnName))
            //{
            //    string solutionString = global::Net.Code.Builder.Properties.Resources.solution;
            //    solutionString = solutionString.Replace(Attributes.ProjectRefrence, deployProjectRefrence);
            //    solutionString = solutionString.Replace(Attributes.ProjectSelection, deployPojectSelection);
            //    sw = new StreamWriter(slnName);
            //    sw.Write(solutionString);
            //    sw.Close();
            //}
            //slnName = basePath + "\\" + _proj.Namespace + ".Agent.sln";
            //if (File.Exists(slnName))
            //{
            //    File.Delete(slnName);
            //}
            //if (!File.Exists(slnName))
            //{
            //    string solutionString = global::Net.Code.Builder.Properties.Resources.solution;
            //    solutionString = solutionString.Replace(Attributes.ProjectRefrence, agentProjectRefrence);
            //    solutionString = solutionString.Replace(Attributes.ProjectSelection, agentProjectSelection);
            //    sw = new StreamWriter(slnName);
            //    sw.Write(solutionString);
            //    sw.Close();
            //}
            //slnName = basePath + "\\" + _proj.Namespace + ".BP.sln";
            //if (File.Exists(slnName))
            //{
            //    File.Delete(slnName);
            //}
            //if (!File.Exists(slnName))
            //{
            //    string solutionString = global::Net.Code.Builder.Properties.Resources.solution;
            //    solutionString = solutionString.Replace(Attributes.ProjectRefrence, bpProjectRefrence);
            //    solutionString = solutionString.Replace(Attributes.ProjectSelection, bpProjectSelection);
            //    sw = new StreamWriter(slnName);
            //    sw.Write(solutionString);
            //    sw.Close();
            //}
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
            el.SetAttribute("SVC", this._proj.IsService ? "1" : "0");
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
