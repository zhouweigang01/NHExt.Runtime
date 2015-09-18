using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using Net.Code.Builder.Base;
using Net.Code.Builder.Build.DTOBuild;
using Net.Code.Builder.Build.EnumBuild;
using Net.Code.Builder.Build.Model;
using Net.Code.Builder.Build.Tpl;
using Net.Code.Builder.Util;
using Net.Code.Builder.Enums;

namespace Net.Code.Builder.Build.BEBuild
{
    public class BuildEntityProj : IBuild
    {
        private BEProj _proj;

        public BuildEntityProj(BEProj proj)
        {
            _proj = proj;
        }

        /// <summary>
        /// 生成代码
        /// </summary>
        /// <returns></returns>
        public string BuildCode()
        {
            string beProjectRefrence = string.Empty;
            string deployProjectRefrence = string.Empty;
            string beProjectSelection = string.Empty;
            string deployProjectSelection = string.Empty;
            string afterBuildEvent = "copy /y \"$(TargetDir){0}.dll\" \"$(SolutionDir)..\\..\\ApplicationLib\\\"" + Environment.NewLine;
            afterBuildEvent += "copy /y \"$(TargetDir){0}.pdb\" \"$(SolutionDir)..\\..\\ApplicationLib\\\"" + Environment.NewLine;
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
            deployProjectRefrence += "Project(\"{" + Guid.NewGuid().ToString() + "}\") = \"" + _proj.Namespace + "." + DTOEntity.AssemblyEndTag + "\", \"Deploy\\" + _proj.Namespace + "." + DTOEntity.AssemblyEndTag + ".csproj \", \"{" + projGuid + "}\"" + Environment.NewLine;
            deployProjectRefrence += "EndProject";
            deployProjectSelection += "{" + projGuid + "}.Debug|Any CPU.ActiveCfg = Debug|Any CPU" + Environment.NewLine;
            deployProjectSelection += "{" + projGuid + "}.Debug|Any CPU.Build.0 = Debug|Any CPU" + Environment.NewLine;
            deployProjectSelection += "{" + projGuid + "}.Release|Any CPU.ActiveCfg = Debug|Any CPU" + Environment.NewLine;
            deployProjectSelection += "{" + projGuid + "}.Release|Any CPU.Build.0 = Debug|Any CPU";



            string targetFiles = string.Empty;
            string extendFiles = string.Empty;
            foreach (BEEntity entity in _proj.EntityList)
            {
                if (entity.DataState != DataState.Delete)
                {
                    targetFiles += "<Compile Include=\"Entity\\" + entity.Code + DTOEntity.AttrEndTag + ".cs\" />" + Environment.NewLine;
                    extendFiles += "<Compile Include=\"Extend\\" + entity.Code + DTOEntity.AttrEndTag + "Extend.cs\" />" + Environment.NewLine;
                }
            }
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
                    targetFiles += "<Compile Include=\"Entity\\" + entity.Code + DTOEntity.AttrEndTag + ".cs\" />" + Environment.NewLine;
                    extendFiles += "<Compile Include=\"Extend\\" + entity.Code + DTOEntity.AttrEndTag + "Extend.cs\" />" + Environment.NewLine;
                }
            }
            //实体引用dll
            string compileDlls = "<Reference Include=\"NHExt.Runtime\">" + Environment.NewLine;
            compileDlls += "<HintPath>..\\..\\..\\Runtime\\NHExt.Runtime.dll</HintPath>" + Environment.NewLine;
            compileDlls += "</Reference>" + Environment.NewLine;
            foreach (ProjectRefrence pr in _proj.RefrenceList)
            {
                if (pr.RefrenceType == RefType.BEEntity) continue;
                //if (pr.RefProjName == _proj.ProjName + ".be")
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
                //生成后时间拷贝dll到上层目录
                csprojString = csprojString.Replace(Attributes.PostBuild, string.Format(afterBuildEvent, _proj.Namespace + "." + DTOEntity.AssemblyEndTag));

                sw = new StreamWriter(projName);
                sw.Write(csprojString);
                sw.Close();
            }
            //生成targets文件
            string targetsString = global::Net.Code.Builder.Properties.Resources.csprojtargets;
            targetsString = targetsString.Replace(Attributes.CompileFile, targetFiles);
            //将生成的资源文件嵌入到程序中去
            targetsString = targetsString.Replace(Attributes.EmbeddedResource, string.Empty);
            targetsString = targetsString.Replace(Attributes.RefrenceDll, compileDlls);

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

            #region 针对每个实体生成实体代码
            foreach (BEEntity entity in _proj.EntityList)
            {
                OutPut.OutPutMsg("生成实体【" + entity.Code + "】Deploy代码……");

                #region 生成实体代码
                sw = new StreamWriter(codePath + entity.Code + DTOEntity.AttrEndTag + ".cs", false);
                DTOEntity dto = new DTOEntity(entity);
                BuildDTO bc = new BuildDTO(dto);
                sw.Write(bc.BuildCode());
                sw.Close();
                #endregion

                #region 生成实体扩展
                if (!File.Exists(extendCodePath + entity.Code + DTOEntity.AttrEndTag + "Extend.cs"))
                {
                    sw = new StreamWriter(extendCodePath + entity.Code + DTOEntity.AttrEndTag + "Extend.cs", false);
                    sw.Write(bc.BuildExtendCode());
                    sw.Close();
                }
                #endregion

                OutPut.OutPutMsg("实体【" + entity.Code + "】Deploy代码生成成功……");
            }
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
            #region 针对每个枚举生成代码
            foreach (EnumEntity entity in _proj.EnumList)
            {
                OutPut.OutPutMsg("生成实体【" + entity.Code + "】代码……");

                #region 生成实体代码
                sw = new StreamWriter(codePath + entity.Code + DTOEntity.AttrEndTag + ".cs", false);
                BuildEnumDTOEntity bc = new BuildEnumDTOEntity(entity);
                sw.Write(bc.BuildCode());
                sw.Close();
                #endregion

                #region 生成实体扩展
                if (!File.Exists(extendCodePath + entity.Code + DTOEntity.AttrEndTag + "Extend.cs"))
                {
                    sw = new StreamWriter(extendCodePath + entity.Code + DTOEntity.AttrEndTag + "Extend.cs", false);
                    sw.Write(bc.BuildExtendCode());
                    sw.Close();
                }
                #endregion

                OutPut.OutPutMsg("实体【" + entity.Code + "】代码生成成功……");
            }
            #endregion
            #endregion

            #region 生成 BE 部分代码

            #region 生成生成BE代码过程中可能用到的文件夹


            projPath = basePath + @"\BE\";
            assemblyPath = projPath + @"Properties\";
            codePath = projPath + @"Entity\";
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
            projGuid = Guid.NewGuid().ToString().ToUpper();
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
            beProjectRefrence += "Project(\"{" + Guid.NewGuid().ToString() + "}\") = \"" + _proj.Namespace + "\", \"BE\\" + _proj.Namespace + ".csproj \", \"{" + projGuid + "}\"" + Environment.NewLine;
            beProjectRefrence += "EndProject";
            beProjectSelection += "{" + projGuid + "}.Debug|Any CPU.ActiveCfg = Debug|Any CPU" + Environment.NewLine;
            beProjectSelection += "{" + projGuid + "}.Debug|Any CPU.Build.0 = Debug|Any CPU" + Environment.NewLine;
            beProjectSelection += "{" + projGuid + "}.Release|Any CPU.ActiveCfg = Debug|Any CPU" + Environment.NewLine;
            beProjectSelection += "{" + projGuid + "}.Release|Any CPU.Build.0 = Debug|Any CPU";

            targetFiles = string.Empty;
            extendFiles = string.Empty;
            string embeddedResourceFiles = string.Empty;

            foreach (BEEntity entity in _proj.EntityList)
            {
                if (entity.DataState != DataState.Delete)
                {
                    targetFiles += "<Compile Include=\"Entity\\" + entity.Code + ".cs\" />" + Environment.NewLine;
                    extendFiles += "<Compile Include=\"Extend\\" + entity.Code + "Extend.cs\" />" + Environment.NewLine;
                    embeddedResourceFiles += "<EmbeddedResource Include=\"ConfigFiles\\" + entity.Code + ".hbm.xml\"><SubType>Designer</SubType></EmbeddedResource>" + Environment.NewLine;
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
                // if (pr.RefProjName == _proj.ProjName + ".be")
                if (pr.RefProjName == _proj.FileName)
                {
                    if (pr.RefrenceType == RefType.BEEntity) continue;
                }
                compileDlls += "<Reference Include=\"" + pr.AssemblyName + "\">" + Environment.NewLine;
                compileDlls += "<HintPath>..\\..\\..\\Runtime\\" + System.IO.Path.GetDirectoryName(pr.RefFilePath) + "\\..\\ApplicationLib\\" + pr.Name + "</HintPath>" + Environment.NewLine;
                //compileDlls += "<Private>True</Private>" + Environment.NewLine;
                compileDlls += "</Reference>" + Environment.NewLine;
            }



            //不存在项目文件的时候才生成项目文件
            if (!File.Exists(projName))
            {
                string csprojString = global::Net.Code.Builder.Properties.Resources.csproj;

                csprojString = csprojString.Replace(Attributes.Guid, projGuid);
                csprojString = csprojString.Replace(Attributes.NameSpace, _proj.Namespace);
                csprojString = csprojString.Replace(Attributes.Assembly, _proj.Namespace);
                //导入生成的代码文件
                csprojString = csprojString.Replace(Attributes.CompileFile, extendFiles);
                //生成后时间拷贝dll到上层目录
                csprojString = csprojString.Replace(Attributes.PostBuild, string.Format(afterBuildEvent, _proj.Namespace));
                //将生成的资源文件嵌入到程序中去
                sw = new StreamWriter(projName);
                sw.Write(csprojString);
                sw.Close();
            }

            //生成targets文件，每次必须生成
            targetsString = global::Net.Code.Builder.Properties.Resources.csprojtargets;
            targetsString = targetsString.Replace(Attributes.CompileFile, targetFiles);
            //将生成的资源文件嵌入到程序中去
            targetsString = targetsString.Replace(Attributes.EmbeddedResource, embeddedResourceFiles);
            targetsString = targetsString.Replace(Attributes.RefrenceDll, compileDlls);

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

            #region 针对每个实体生成实体代码
            foreach (BEEntity entity in _proj.EntityList)
            {
                OutPut.OutPutMsg("生成实体【" + entity.Code + "】代码……");

                #region 生成实体代码
                sw = new StreamWriter(codePath + entity.Code + ".cs", false);
                BuildEntity bc = new BuildEntity(entity);
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

            #region 针对每个枚举生成实体代码
            foreach (EnumEntity entity in _proj.EnumList)
            {
                OutPut.OutPutMsg("生成实体【" + entity.Code + "】代码……");

                #region 生成实体代码
                sw = new StreamWriter(codePath + entity.Code + ".cs", false);
                BuildEnumEntity bc = new BuildEnumEntity(entity);
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

            #region 生成实体对应的NH配置文件代码
            this.BuildCfgXML(null);
            #endregion

            #endregion

            #region 生成solution文件
            string slnName = basePath + "\\" + _proj.Namespace + ".sln";
            if (!File.Exists(slnName))
            {
                string solutionString = global::Net.Code.Builder.Properties.Resources.solution;
                solutionString = solutionString.Replace(Attributes.ProjectRefrence, deployProjectRefrence + Environment.NewLine + beProjectRefrence);
                solutionString = solutionString.Replace(Attributes.ProjectSelection, deployProjectSelection + Environment.NewLine + beProjectSelection);
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
            //    solutionString = solutionString.Replace(Attributes.ProjectSelection, deployProjectSelection);
            //    sw = new StreamWriter(slnName);
            //    sw.Write(solutionString);
            //    sw.Close();
            //}

            //slnName = basePath + "\\" + _proj.Namespace + ".BE.sln";
            //if (File.Exists(slnName))
            //{
            //    File.Delete(slnName);
            //}
            //if (!File.Exists(slnName))
            //{
            //    string solutionString = global::Net.Code.Builder.Properties.Resources.solution;
            //    solutionString = solutionString.Replace(Attributes.ProjectRefrence, beProjectRefrence);
            //    solutionString = solutionString.Replace(Attributes.ProjectSelection, beProjectSelection);
            //    sw = new StreamWriter(slnName);
            //    sw.Write(solutionString);
            //    sw.Close();
            //}
            #endregion

            return string.Empty;
        }
        /// <summary>
        /// 生成NH配置文件,在生成代码的时候调用
        /// </summary>
        /// <returns></returns>
        public XmlElement BuildCfgXML(XmlDocument xmlDoc)
        {
            #region 生成生成代码过程中可能用到的文件夹

            string basePath = GetBasePath();

            string cfgPath = basePath + @"\BE\ConfigFiles\";
            if (!Directory.Exists(cfgPath))
            {
                Directory.CreateDirectory(cfgPath);
            }

            #endregion

            foreach (BEEntity entity in _proj.EntityList)
            {
                xmlDoc = new XmlDocument();
                XmlDeclaration xmlDeclar = xmlDoc.CreateXmlDeclaration("1.0", "UTF-8", "yes");
                xmlDoc.AppendChild(xmlDeclar);

                XmlElement el = xmlDoc.CreateElement("hibernate-mapping");
                el.SetAttribute("xmlns", "urn:nhibernate-mapping-2.2");

                BuildEntity bc = new BuildEntity(entity);
                el.AppendChild(bc.BuildCfgXML(xmlDoc));

                xmlDoc.AppendChild(el);
                xmlDoc.Save(cfgPath + entity.Code + ".hbm.xml");
            }

            return null;
        }

        public string BuildMSSQL()
        {
            string basePath = GetBasePath();

            string scriptPath = basePath + @"\..\..\Script\";
            if (!Directory.Exists(scriptPath))
            {
                Directory.CreateDirectory(scriptPath);
            }
            StreamWriter sw = new StreamWriter(scriptPath + this._proj.ProjName + ".sql", false, Encoding.Unicode);
            string buildSqlFK = string.Empty;
            string removeSqlFK = string.Empty;
            string buildSql = string.Empty;

            List<BEEntity> entityList = new List<BEEntity>();
            foreach (BEEntity entity in this._proj.EntityList)
            {
                entityList.Add(entity);

            }
            entityList = this.ReSortBE(entityList, this._proj);
            foreach (BEEntity entity in entityList)
            {
                BuildEntity be = new BuildEntity(entity);
                removeSqlFK += be.BuildRemoveForeignKey();
                buildSql += be.BuildMSSQL();
                buildSqlFK += be.BuildCreateForeignKey();
            }
            //生成外键
            removeSqlFK += "GO " + Environment.NewLine;
            buildSqlFK += "GO" + Environment.NewLine;
            sw.Write(removeSqlFK);
            sw.Write(buildSql);
            sw.Write(buildSqlFK);
            sw.Close();
            return string.Empty;
        }

        private List<BEEntity> ReSortBE(List<BEEntity> entityList, BEProj proj)
        {
            List<BEEntity> hasSelectedList = new List<BEEntity>();
            int index = 0;
            while (entityList.Count > 0)
            {
                index = index % entityList.Count;
                BEEntity entity = entityList[index];
                bool hasRefrenece = false;
                foreach (BEColumn col in entity.ColumnList)
                {
                    if (col.DataType == DataTypeEnum.RefreceType && !col.IsEnum)
                    {
                        //自引用的不需要处理
                        if (col.RefGuid == entity.Guid)
                        {
                            continue;
                        }
                        string ns = col.TypeString.Substring(0, col.TypeString.LastIndexOf('.'));
                        if (ns == proj.Namespace)
                        {
                            bool contain = false;
                            foreach (BEEntity be in hasSelectedList)
                            {
                                string fullName = proj.Namespace + "." + be.Code;
                                if (fullName == col.TypeString)
                                {
                                    contain = true;
                                }
                            }
                            if (!contain)
                            {
                                hasRefrenece = true;
                                break;
                            }
                        }
                    }
                }
                if (!hasRefrenece)
                {
                    entityList.RemoveAt(index);
                    hasSelectedList.Add(entity);
                }
                else
                {
                    index++;
                }
            }
            return hasSelectedList;

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
            string basePath = this.GetBasePath();

            string scriptPath = basePath + @"\..\..\Script\";
            if (!Directory.Exists(scriptPath))
            {
                Directory.CreateDirectory(scriptPath);
            }

            string buildSql = string.Empty;
            buildSql += "--实体项目元数据sql" + Environment.NewLine;
            buildSql += "DELETE FROM T_METADATA_PROJECT_COMPONENT WHERE F_GUID='" + this._proj.Guid + "'" + Environment.NewLine;
            buildSql += "INSERT INTO T_METADATA_PROJECT_COMPONENT VALUES((SELECT ISNULL(MAX(F_ID),0)+1 FROM T_METADATA_PROJECT_COMPONENT),0,'" + this._proj.Guid + "','" + this._proj.ProjName + "','" + this._proj.Namespace + "',0,'" + this._proj.Namespace + ".dll" + "')" + Environment.NewLine;
            buildSql += "--生成单个实体的元数据sql" + Environment.NewLine;
            foreach (BEEntity entity in this._proj.EntityList)
            {
                BuildEntity be = new BuildEntity(entity);
                buildSql += be.BuildMetaData();
            }
            foreach (EnumEntity entity in this._proj.EnumList)
            {
                BuildEnumEntity bee = new BuildEnumEntity(entity);
                buildSql += bee.BuildMetaData();
            }
            StreamWriter sw = new StreamWriter(scriptPath + this._proj.ProjName + "MetaData.sql", false, Encoding.Unicode);
            sw.Write(buildSql);
            sw.Close();
            return string.Empty;
        }
    }
}
