using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using Net.Code.Builder.Base;
using Net.Code.Builder.Build.Model;
using Net.Code.Builder.Build.Tpl;
using Net.Code.Builder.Util;
using Net.Code.Builder.Enums;

namespace Net.Code.Builder.Build.BEBuild
{
    class BuildEntityCommon : BuildEntityColumn
    {
        public BuildEntityCommon(BEColumn col)
            : base(col)
        {
            _templateCode = BECodeTemplate.CommonTemplate;
        }

        public override string BuildCode()
        {
            _templateCode = base.BuildCode();
            //string 类型比较特殊，不能设置为可空类型
            if (this._col.IsNull && this._col.TypeString.ToLower() != "string")
            {
                _templateCode = _templateCode.Replace(Attributes.IsNull, "?");
            }
            else
            {
                _templateCode = _templateCode.Replace(Attributes.IsNull, "");
            }
            return _templateCode;
        }

        public override XmlElement BuildCfgXML(XmlDocument xmlDoc)
        {
            XmlElement el = xmlDoc.CreateElement("property");
            el.SetAttribute("name", _col.Code);
            el.SetAttribute("column", _col.DBCode);
            el.SetAttribute("type", ORMTypeMapping.Get(_col.TypeString));
            if (_col.Length > 0)
            {
                el.SetAttribute("length", _col.Length.ToString());
            }

            return el;
        }

        public override string BuildMSSQL()
        {
            string dbType = UtilHelper.GetDBType(_col.TypeString);

            if (_col.TypeString == "string")
            {
                dbType += "(" + _col.Length + ")";
            }
            string sql = _col.DBCode + " " + dbType.ToUpper();
            if (!_col.IsNull)
            {
                sql += " not null";
            }
            return sql;
        }
        /// <summary>
        /// 普通类型
        /// </summary>
        /// <returns></returns>
        public override string BuildMetaData()
        {
            string sql = string.Empty;
            sql += "DELETE FROM T_METADATA_ENTITY_COLUMN_COMPONENT WHERE F_GUID='" + this._col.Guid + "' and F_ENTITYGUID='" + this._col.Entity.Guid + "'" + Environment.NewLine;
            sql += "INSERT INTO T_METADATA_ENTITY_COLUMN_COMPONENT VALUES((SELECT ISNULL(MAX(F_ID),0)+1 FROM T_METADATA_ENTITY_COLUMN_COMPONENT),0,'" + this._col.Guid + "','" + this._col.Code + "','" + this._col.Name + "','" + this._col.TypeString + "','','" + this._col.Entity.Guid + "'," + (int)ColumnTypeEnum.CommonType + ")" + Environment.NewLine;
            return sql;
        }

        public override string EntityColumnToDTOColumn()
        {
            string tpl = BECodeTemplate.EntityColumnToDTOCommonTemplate.Replace(Attributes.Code, this._col.Code);
            return tpl;
        }
        public override string EntityColumnFromDTOColumn()
        {
            string tpl = BECodeTemplate.EntityColumnFromDTOCommonTemplate.Replace(Attributes.Code, this._col.Code);
            return tpl;
        }

        public override string BuildCloneCode()
        {
            string code = BECodeTemplate.CloneCommonTemplate.Replace(Attributes.Code, this._col.Code) + Environment.NewLine;
            return code;
        }
    }
}
