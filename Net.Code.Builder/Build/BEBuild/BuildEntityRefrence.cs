using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using Net.Code.Builder.Base;
using Net.Code.Builder.Build.Model;
using Net.Code.Builder.Build.Tpl;
using Net.Code.Builder.Util;


namespace Net.Code.Builder.Build.BEBuild
{
    class BuildEntityRefrence : BuildEntityColumn
    {
        public BuildEntityRefrence(BEColumn col)
            : base(col)
        {
            if (string.IsNullOrEmpty(_col.RefColGuid))
            {
                _templateCode = BECodeTemplate.RefrenceTemplate;
            }
            else
            {
                _templateCode = BECodeTemplate.RefrenceCompositionTemplate;
            }
        }

        public override string BuildCode()
        {
            _templateCode = base.BuildCode();
            _templateCode = _templateCode.Replace(Attributes.RefrenceEntity, _col.BEProj.Namespace + "." + _col.RefEntityCode);
            if (!string.IsNullOrEmpty(_col.RefColGuid))
            {
                string staticCreateCode = BECodeTemplate.CreateFuncTemplate;
                staticCreateCode = staticCreateCode.Replace(Attributes.Class, _col.Entity.Code);
                staticCreateCode = staticCreateCode.Replace(Attributes.TypeString, _col.BEProj.Namespace + "." + _col.RefEntityCode);
                staticCreateCode = staticCreateCode.Replace(Attributes.Code, _col.Code);
                _templateCode += staticCreateCode;
            }
            return _templateCode;
        }

        public override XmlElement BuildCfgXML(XmlDocument xmlDoc)
        {
            XmlElement el = xmlDoc.CreateElement("many-to-one");
            el.SetAttribute("name", _col.Code);
            el.SetAttribute("column", _col.DBCode);
            el.SetAttribute("not-null", _col.IsNull ? "false" : "true");
            el.SetAttribute("class", _col.TypeString + "," + _col.BEProj.Namespace);
            //todo 生成外键约束
            el.SetAttribute("foreign-key", "FK_" + _col.Entity.TableName + "_" + _col.Code + "_" + _col.RefEntityCode);
            return el;
        }

        public override string BuildMSSQL()
        {
            string sql = _col.DBCode + " BIGINT";
            if (!_col.IsNull)
            {
                sql += " NOT NULL";
            }
            return sql;
        }
        public override string EntityColumnToDTOColumn()
        {
            string tpl = BECodeTemplate.EntityColumnToDTORefrenceTemplate.Replace(Attributes.Code, this._col.Code);
            return tpl;
        }
        public override string EntityColumnFromDTOColumn()
        {
            string tpl = BECodeTemplate.EntityColumnFromDTORefrenceTemplate.Replace(Attributes.Code, this._col.Code).Replace(Attributes.TypeString, this._col.TypeString);
            return tpl;
        }
        public override string BuildForeignKey()
        {
            string sql = "ALTER TABLE " + this._col.Entity.TableName + " ADD CONSTRAINT FK_" + this._col.Entity.TableName + "_" + this._col.Code + "_" + this._col.RefEntityCode + " FOREIGN KEY (" + this._col.DBCode + ") ";
            sql += " REFERENCES " + this._col.RefEntityTableName + " (F_ID) " + Environment.NewLine;
            return sql;
        }
        public override string BuildRemoveForeignKey()
        {
            string sql = "IF EXISTS(SELECT 1 from sysobjects WHERE name= 'FK_" + this._col.Entity.TableName + "_" + this._col.Code + "_" + this._col.RefEntityCode + "' and xtype= 'F')" + Environment.NewLine;
            sql += "BEGIN" + Environment.NewLine;
            sql += "ALTER TABLE " + this._col.Entity.TableName + " DROP CONSTRAINT FK_" + this._col.Entity.TableName + "_" + this._col.Code + "_" + this._col.RefEntityCode + Environment.NewLine;
            sql += "END" + Environment.NewLine;
            return sql;
        }

        public override string BuildCloneCode()
        {
            string code = BECodeTemplate.CloneRefrenceTemplate.Replace(Attributes.Code, this._col.Code).Replace(Attributes.TypeString, this._col.TypeString) + Environment.NewLine;
            return code;
        }

    }
}
