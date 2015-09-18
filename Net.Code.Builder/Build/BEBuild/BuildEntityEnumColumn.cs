using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using Net.Code.Builder.Build.Model;
using Net.Code.Builder.Build.Tpl;
using Net.Code.Builder.Enums;

namespace Net.Code.Builder.Build.BEBuild
{
    class BuildEntityEnumColumn : BuildEntityColumn
    {
        public BuildEntityEnumColumn(BEColumn col)
            : base(col)
        {
            _templateCode = BECodeTemplate.EnumRefrenceTemplate;
        }

        public override string BuildCode()
        {
            _templateCode = base.BuildCode();
            _templateCode = _templateCode.Replace(Attributes.RefrenceEntity, _col.BEProj.Namespace + "." + _col.RefEntityCode);
            return _templateCode;
        }

        public override XmlElement BuildCfgXML(XmlDocument xmlDoc)
        {
            XmlElement el = xmlDoc.CreateElement("property");
            el.SetAttribute("name", _col.Code);
            el.SetAttribute("column", _col.DBCode);
            el.SetAttribute("type", _col.TypeString + "," + _col.BEProj.Namespace);
            // HHSoft.LandSupply.DataEntity.NHibernateType.MyDateTime,HHSoft.LandSupply.DataEntity
            return el;
        }

        public override string BuildMSSQL()
        {
            string sql = _col.DBCode + " INT";
            if (!_col.IsNull)
            {
                sql += " NOT NULL";
            }
            return sql;
        }

        public override string BuildMetaData()
        {
            string sql = string.Empty;
            sql += "DELETE FROM T_METADATA_ENTITY_COLUMN_COMPONENT WHERE F_GUID='" + this._col.Guid + "' and F_ENTITYGUID='" + this._col.Entity.Guid + "'" + Environment.NewLine;
            sql += "INSERT INTO T_METADATA_ENTITY_COLUMN_COMPONENT VALUES((SELECT ISNULL(MAX(F_ID),0)+1 FROM T_METADATA_ENTITY_COLUMN_COMPONENT),0,'" + this._col.Guid + "','" + this._col.Code + "','" + this._col.Name + "','" + this._col.TypeString + "','" + this._col.RefGuid + "','" + this._col.Entity.Guid + "'," + (int)ColumnTypeEnum.EnumType + ")" + Environment.NewLine;
            return sql;
        }
        public override string EntityColumnToDTOColumn()
        {
            string tpl = BECodeTemplate.EntityColumnToDTOEnumTemplate.Replace(Attributes.Code, this._col.Code);
            return tpl;
        }
        public override string EntityColumnFromDTOColumn()
        {
            string tpl = BECodeTemplate.EntityColumnFromDTOEnumTemplate.Replace(Attributes.Code, this._col.Code).Replace(Attributes.TypeString, this._col.TypeString);
            return tpl;
        }
        public override string BuildCloneCode()
        {
            string code = BECodeTemplate.CloneEnumTemplate.Replace(Attributes.Code, this._col.Code).Replace(Attributes.TypeString, this._col.TypeString) + Environment.NewLine;
            return code;
        }
    }
}
