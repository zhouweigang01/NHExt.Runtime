using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using Net.Code.Builder.Base;
using Net.Code.Builder.Build.Model;
using Net.Code.Builder.Build.Tpl;
using Net.Code.Builder.Enums;


namespace Net.Code.Builder.Build.BEBuild
{
    public class BuildEntity : IBuild
    {
        private string _namespace;
        BEEntity _entity;

        public BuildEntity(BEEntity entity)
        {
            _entity = entity;
            _namespace = entity.Proj.Namespace;
        }
        /// <summary>
        /// 生成实体代码
        /// </summary>
        /// <returns></returns>
        public string BuildCode()
        {
            string buildCode = BECodeTemplate.ClassBeginTemplate.Replace(Attributes.Class, _entity.Code);
            buildCode = buildCode.Replace(Attributes.Guid, _entity.Guid);
            buildCode = buildCode.Replace(Attributes.Range, this._entity.GetViewRangeStr());
            buildCode = buildCode.Replace(Attributes.OrgFilter, _entity.OrgFilter ? "true" : "false");
            buildCode = buildCode.Replace(Attributes.Table, this._entity.TableName);
            #region 查找到当前实体的聚合实体和引用实体
            string refStr = "";
            string comStr = "";
            string pColumn = "\"\"";
            foreach (BEColumn column in this._entity.InhertColumnList)
            {
                if (column.DataType == DataTypeEnum.RefreceType && !column.IsEnum && string.IsNullOrEmpty(column.RefColGuid))
                {
                    refStr += "\"" + column.Code + "\"" + ",";
                }
                else if (column.DataType == DataTypeEnum.CompositionType)
                {
                    comStr += "\"" + column.Code + "\"" + ",";
                }
            }
            foreach (BEColumn column in this._entity.ColumnList)
            {
                if (column.DataType == DataTypeEnum.RefreceType && !column.IsEnum)
                {
                    if (string.IsNullOrEmpty(column.RefColGuid))
                    {
                        refStr += "\"" + column.Code + "\"" + ",";
                    }
                    else
                    {
                        pColumn = "\"" + column.Code + "\"";
                    }
                }
                else if (column.DataType == DataTypeEnum.CompositionType)
                {
                    comStr += "\"" + column.Code + "\"" + ",";
                }
            }
            if (!string.IsNullOrEmpty(refStr))
            {
                refStr = refStr.Substring(0, refStr.Length - 1);
            }
            if (!string.IsNullOrEmpty(comStr))
            {
                comStr = comStr.Substring(0, comStr.Length - 1);
            }
            #endregion
            buildCode = buildCode.Replace(Attributes.RefStr, refStr).Replace(Attributes.ComStr, comStr);
            buildCode = buildCode.Replace(Attributes.PColumn, pColumn);

            //生成构造函数
            buildCode += BECodeTemplate.ConstructorBeginTemplate.Replace(Attributes.Class, _entity.Code);
            //替换变量
            buildCode = buildCode.Replace(Attributes.EntityName, _namespace + "." + _entity.Code);
            buildCode = buildCode.Replace(Attributes.NameSpace, _namespace);
            buildCode = buildCode.Replace(Attributes.InhertClass, _entity.InhertName);
            //构造函数内部可以添加一些生成代码
            buildCode += BECodeTemplate.ConstructorEndTemplate;

            //生成clone函数
            buildCode += BECodeTemplate.CloneFunctionTemplate.Replace(Attributes.Class, _entity.Code) + Environment.NewLine;
            buildCode += BECodeTemplate.CloneObjBeginTemplate.Replace(Attributes.Class, _entity.Code) + Environment.NewLine;
            foreach (BEColumn col in _entity.ColumnList)
            {
                BuildEntityColumn bc = BuildEntityColumnFactory.Create(col);
                buildCode += bc.BuildCloneCode();
            }
            buildCode += BECodeTemplate.CloneObjEndTemplate + Environment.NewLine;

            //基类中已经实现了sysversion和id
            foreach (BEColumn col in _entity.ColumnList)
            {
                IBuild b = BuildEntityColumnFactory.Create(col);
                buildCode += b.BuildCode();
            }

            buildCode += this.BuildEntityToDTO();
            buildCode += this.BuildEntityFromDTO();

            buildCode += BECodeTemplate.ClassEndTemplate;

            return buildCode;
        }
        public string BuildExtendCode()
        {
            string buildCode = BECodeTemplate.ClassExtendTemplate.Replace(Attributes.Class, _entity.Code);
            buildCode = buildCode.Replace(Attributes.NameSpace, _namespace);
            return buildCode;

        }
        /// <summary>
        /// 生成NH配置文件
        /// </summary>
        /// <param name="xmlDoc"></param>
        /// <returns></returns>
        public XmlElement BuildCfgXML(XmlDocument xmlDoc)
        {
            XmlElement el_Class = xmlDoc.CreateElement("class");
            el_Class.SetAttribute("name", this._namespace + "." + this._entity.Code + "," + this._namespace);
            el_Class.SetAttribute("table", this._entity.TableName);
            el_Class.SetAttribute("dynamic-insert", "true");
            el_Class.SetAttribute("dynamic-update", "true");

            //缓存相关
            XmlElement el_cache = xmlDoc.CreateElement("cache");
            el_cache.SetAttribute("usage", "read-write");
            el_Class.AppendChild(el_cache);

            //生成ID和sysversion
            XmlElement el_id = xmlDoc.CreateElement("id");
            el_id.SetAttribute("name", "ID");
            el_id.SetAttribute("column", "F_ID");
            el_id.SetAttribute("type", "Int64");
            XmlElement el_id_generator = xmlDoc.CreateElement("generator");
            el_id_generator.SetAttribute("class", "assigned");
            el_id.AppendChild(el_id_generator);
            el_Class.AppendChild(el_id);

            XmlElement el_version = xmlDoc.CreateElement("version");
            el_version.SetAttribute("name", "SysVersion");
            el_version.SetAttribute("column", "F_SYSVERSION");
            el_version.SetAttribute("type", "integer");
            el_version.SetAttribute("unsaved-value", "0");
            el_Class.AppendChild(el_version);

            foreach (BEColumn col in this._entity.InhertColumnList)
            {
                IBuild b = BuildEntityColumnFactory.Create(col);
                el_Class.AppendChild(b.BuildCfgXML(xmlDoc));
            }
            foreach (BEColumn col in this._entity.ColumnList)
            {
                IBuild b = BuildEntityColumnFactory.Create(col);
                el_Class.AppendChild(b.BuildCfgXML(xmlDoc));
            }
            return el_Class;
        }
        /// <summary>
        /// 生成建库脚本
        /// </summary>
        /// <returns></returns>
        public string BuildMSSQL()
        {
            string sql = "IF EXISTS (SELECT * FROM sysobjects WHERE id = object_id('" + _entity.TableName + "'))" + Environment.NewLine;
            sql += "BEGIN " + Environment.NewLine;
            sql += "DROP TABLE " + _entity.TableName + Environment.NewLine;
            sql += "END " + Environment.NewLine;
            sql += "GO" + Environment.NewLine;

            string bizIndexName = this._entity.TableName + "_BizIndex";
            sql += "IF EXISTS (SELECT name FROM sysindexes WHERE name = '" + bizIndexName + "')" + Environment.NewLine;
            sql += "DROP INDEX " + this._entity.TableName + "." + bizIndexName + Environment.NewLine;
            sql += "GO" + Environment.NewLine;

            sql += "CREATE TABLE " + _entity.TableName + " (" + Environment.NewLine;
            sql += "F_ID bigint PRIMARY KEY ," + Environment.NewLine;
            sql += "F_SYSVERSION int not null ," + Environment.NewLine;

            string bizIndexColumns = string.Empty;
            bool buildIndexSQL = false;
            ///生成基类的数据库字段
            foreach (BEColumn col in this._entity.InhertColumnList)
            {
                if (col.DataType == DataTypeEnum.CompositionType)
                {
                    continue;
                }
                IBuild b = BuildEntityColumnFactory.Create(col);
                sql += b.BuildMSSQL() + ",";
                if (col.IsBizKey)
                {
                    if (!string.IsNullOrEmpty(bizIndexColumns))
                    {
                        buildIndexSQL = true;
                    }
                    bizIndexColumns += col.DBCode + ",";
                }
            }

            foreach (BEColumn col in this._entity.ColumnList)
            {
                if (col.DataType == DataTypeEnum.CompositionType)
                {
                    continue;
                }
                IBuild b = BuildEntityColumnFactory.Create(col);
                sql += b.BuildMSSQL() + ",";
                if (col.IsBizKey)
                {
                    if (!string.IsNullOrEmpty(bizIndexColumns))
                    {
                        buildIndexSQL = true;
                    }
                    bizIndexColumns += col.DBCode + ",";
                }
            }
            if (sql[sql.Length - 1] == ',')
            {
                sql = sql.Substring(0, sql.Length - 1);
            }
            sql += ")" + Environment.NewLine;

            if (buildIndexSQL)
            {
                bizIndexColumns = bizIndexColumns.Substring(0, bizIndexColumns.Length - 1);
                sql += "CREATE INDEX " + bizIndexName + Environment.NewLine;
                sql += " ON " + this._entity.TableName + " (" + bizIndexColumns + ")" + Environment.NewLine;
            }
            return sql;
        }
        /// <summary>
        /// 生成外键关联
        /// </summary>
        /// <returns></returns>
        public string BuildCreateForeignKey()
        {
            string sql = "";
            //如果该类是继承下来的话，需要生成基类
            foreach (BEColumn col in this._entity.InhertColumnList)
            {
                BuildEntityColumn bc = BuildEntityColumnFactory.Create(col);
                sql += bc.BuildForeignKey();
            }

            foreach (BEColumn col in this._entity.ColumnList)
            {
                BuildEntityColumn bc = BuildEntityColumnFactory.Create(col);
                sql += bc.BuildForeignKey();
            }
            return sql;

        }

        public string BuildRemoveForeignKey()
        {
            string sql = "";
            foreach (BEColumn col in this._entity.InhertColumnList)
            {
                BuildEntityColumn bc = BuildEntityColumnFactory.Create(col);
                sql += bc.BuildRemoveForeignKey();
            }
            foreach (BEColumn col in this._entity.ColumnList)
            {
                BuildEntityColumn bc = BuildEntityColumnFactory.Create(col);
                sql += bc.BuildRemoveForeignKey();
            }
            return sql;
        }

        private string BuildEntityToDTO()
        {
            string type = this._entity.Proj.Namespace + "." + DTOEntity.AssemblyEndTag + "." + this._entity.Code + DTOEntity.AttrEndTag;

            string tpl = BECodeTemplate.EntityToDTOBeginTemplate.Replace(Attributes.TypeString, type);

            foreach (BEColumn col in this._entity.ColumnList)
            {
                BuildEntityColumn bc = BuildEntityColumnFactory.Create(col);
                tpl += bc.EntityColumnToDTOColumn();
            }
            tpl += BECodeTemplate.EntityToDTOEndTemplate;

            return tpl;
        }

        private string BuildEntityFromDTO()
        {
            string dtoClass = this._entity.Proj.Namespace + "." + DTOEntity.AssemblyEndTag + "." + this._entity.Code + DTOEntity.AttrEndTag;
            string entityClass = this._entity.Proj.Namespace + "." + this._entity.Code;
            string tpl = BECodeTemplate.EntityFromDTOBeginTemplate.Replace(Attributes.TypeString, entityClass).Replace(Attributes.Class, dtoClass);
            foreach (BEColumn col in this._entity.ColumnList)
            {
                BuildEntityColumn bc = BuildEntityColumnFactory.Create(col);
                tpl += bc.EntityColumnFromDTOColumn();
            }
            tpl += BECodeTemplate.EntityFromDTOEndTemplate;

            return tpl;
        }

        public string BuildMetaData()
        {
            string sql = string.Empty;
            sql += "DELETE FROM T_METADATA_ENTITY_COMPONENT WHERE F_GUID='" + this._entity.Guid + "'" + Environment.NewLine;
            sql += "INSERT INTO T_METADATA_ENTITY_COMPONENT VALUES((SELECT ISNULL(MAX(F_ID),0)+1 FROM T_METADATA_ENTITY_COMPONENT),0,'" + this._entity.Guid + "','" + this._entity.Code + "','" + this._entity.Name + "','" + this._entity.Proj.Namespace + "." + this._entity.Code + "','" + this._entity.Proj.Namespace + ".dll" + "','" + this._entity.Proj.Guid + "','" + this._entity.TableName + "'," + (this._entity.OrgFilter ? "1" : "0") + "," + (int)this._entity.ViewRange + ")" + Environment.NewLine;

            sql += "DELETE FROM T_METADATA_ENTITY_COLUMN_COMPONENT WHERE F_ENTITYGUID='" + this._entity.Guid + "'" + Environment.NewLine;

            //生成ID列,ID列的GUID为实体的GUID
            sql += "DELETE FROM T_METADATA_ENTITY_COLUMN_COMPONENT WHERE F_GUID='" + this._entity.Guid + "'" + Environment.NewLine;
            sql += "INSERT INTO T_METADATA_ENTITY_COLUMN_COMPONENT VALUES((SELECT ISNULL(MAX(F_ID),0)+1 FROM T_METADATA_ENTITY_COLUMN_COMPONENT),0,'" + this._entity.Guid + "','ID','ID','long','','" + this._entity.Guid + "'," + (int)ColumnTypeEnum.CommonType + ")" + Environment.NewLine;
            //生成实体列SQL
            foreach (BEColumn col in this._entity.InhertColumnList)
            {
                IBuild b = BuildEntityColumnFactory.Create(col);
                sql += b.BuildMetaData();
            }
            foreach (BEColumn col in this._entity.ColumnList)
            {
                IBuild b = BuildEntityColumnFactory.Create(col);
                sql += b.BuildMetaData();
            }
            return sql;
        }
    }
}
