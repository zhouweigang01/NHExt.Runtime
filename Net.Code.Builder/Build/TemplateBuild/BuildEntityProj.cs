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
using Net.Code.Builder.Build.BEBuild;

namespace Net.Code.Builder.Build.TemplateBuild
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
            CodeBuilder builder = new CodeBuilder();

            string basePath = GetBasePath();
            StreamWriter sw = null;

            TemplateDTO.Solution solution = new TemplateDTO.Solution();

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
                string csprojString = builder.BuildCode("Project", TProj);//CodeBuilder.CreateProjBuider(TProj);
                sw = new StreamWriter(projName);
                sw.Write(csprojString);
                sw.Close();
            }
            //生成targets文件
            string targetsString = builder.BuildCode("ProjectTargets", TProj);//CodeBuilder.CreateProjTargetsBuider(TProj);
            sw = new StreamWriter(projPath + "csproj.targets");
            sw.Write(targetsString);
            sw.Close();

            #endregion

            #region 生成assembly信息
            string assemblyString = builder.BuildCode("ProjectAssembly", TProj);//CodeBuilder.CreateProjectAssemblyBuilder(TProj);
            sw = new StreamWriter(assemblyPath + "AssemblyInfo.cs", false);
            sw.Write(assemblyString);
            sw.Close();
            #endregion

            #region 针对每个实体生成实体代码
            foreach (BEEntity entity in _proj.EntityList)
            {
                TemplateDTO.BEEntityDTO tEntity = new TemplateDTO.BEEntityDTO(entity);
                OutPut.OutPutMsg("生成实体【" + entity.Code + "】Deploy代码……");

                #region 生成实体代码
                string entityCode = builder.BuildCode("EntityDTO", tEntity);//CodeBuilder.CreateEntityDTOBuilder(tEntity);
                sw = new StreamWriter(codePath + entity.Code + DTOEntity.AttrEndTag + ".cs", false);
                sw.Write(entityCode);
                sw.Close();
                #endregion

                #region 生成实体扩展
                if (!File.Exists(extendCodePath + entity.Code + DTOEntity.AttrEndTag + "Extend.cs"))
                {
                    string entityExtendCode = builder.BuildCode("EntityDTOExtend", tEntity);
                    sw = new StreamWriter(extendCodePath + entity.Code + DTOEntity.AttrEndTag + "Extend.cs", false);
                    sw.Write(entityExtendCode);
                    sw.Close();
                }
                #endregion

                OutPut.OutPutMsg("实体【" + entity.Code + "】Deploy代码生成成功……");
            }
            #endregion

            #region 针对每个DTO生成代码
            foreach (DTOEntity entity in _proj.DTOList)
            {
                TemplateDTO.BEEntityDTO tEntity = new TemplateDTO.BEEntityDTO(entity);

                OutPut.OutPutMsg("生成实体【" + entity.Code + "】代码……");

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

                OutPut.OutPutMsg("实体【" + entity.Code + "】代码生成成功……");
            }
            #endregion
            #region 针对每个枚举生成代码
            foreach (EnumEntity entity in _proj.EnumList)
            {
                Builder.Build.TemplateDTO.Enum tEntity = new TemplateDTO.Enum(entity, true);

                OutPut.OutPutMsg("生成枚举【" + entity.Code + "】代码……");

                #region 生成实体代码
                string entityCode = builder.BuildCode("EnumDTO", tEntity);
                sw = new StreamWriter(codePath + entity.Code + DTOEntity.AttrEndTag + ".cs", false);
                sw.Write(entityCode);
                sw.Close();
                #endregion

                #region 生成实体扩展
                if (!File.Exists(extendCodePath + entity.Code + DTOEntity.AttrEndTag + "Extend.cs"))
                {
                    string entityExtendCode = builder.BuildCode("EnumDTOExtend", tEntity);
                    sw = new StreamWriter(extendCodePath + entity.Code + DTOEntity.AttrEndTag + "Extend.cs", false);
                    sw.Write(entityExtendCode);
                    sw.Close();
                }
                #endregion

                OutPut.OutPutMsg("枚举【" + entity.Code + "】代码生成成功……");
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
            TProj = new TemplateDTO.Project(this._proj, TemplateDTO.ProjType.BE);
            TProj.Guid = projGuid;

            solution.AddProj(TProj);

            //不存在项目文件的时候才生成项目文件
            if (!File.Exists(projName))
            {
                string csprojString = builder.BuildCode("Project", TProj);
                //将生成的资源文件嵌入到程序中去
                sw = new StreamWriter(projName);
                sw.Write(csprojString);
                sw.Close();
            }

            //生成targets文件，每次必须生成
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

            #region 针对每个实体生成实体代码
            foreach (BEEntity entity in _proj.EntityList)
            {
                OutPut.OutPutMsg("生成实体【" + entity.Code + "】代码……");

                TemplateDTO.BEEntity tEntity = new TemplateDTO.BEEntity(entity);

                #region 生成实体代码
                string entityCode = assemblyString = builder.BuildCode("Entity", tEntity);
                sw = new StreamWriter(codePath + entity.Code + ".cs", false);
                sw.Write(entityCode);
                sw.Close();
                #endregion

                #region 生成实体扩展
                if (!File.Exists(extendCodePath + entity.Code + "Extend.cs"))
                {
                    string entityExtendCode = builder.BuildCode("EntityExtend", tEntity);
                    sw = new StreamWriter(extendCodePath + entity.Code + "Extend.cs", false);
                    sw.Write(entityExtendCode);
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

                TemplateDTO.Enum tEntity = new TemplateDTO.Enum(entity, false);

                #region 生成实体代码
                string entityCode = builder.BuildCode("Enum", tEntity);

                sw = new StreamWriter(codePath + entity.Code + ".cs", false);
                BuildEnumEntity bc = new BuildEnumEntity(entity);
                sw.Write(entityCode);
                sw.Close();
                #endregion

                #region 生成实体扩展
                if (!File.Exists(extendCodePath + entity.Code + "Extend.cs"))
                {
                    string entityExtendCode = builder.BuildCode("EnumExtend", tEntity);
                    sw = new StreamWriter(extendCodePath + entity.Code + "Extend.cs", false);
                    sw.Write(entityExtendCode);
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
                string solutionString = builder.BuildCode("Solution", solution);
                sw = new StreamWriter(slnName);
                sw.Write(solutionString);
                sw.Close();
            }


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
