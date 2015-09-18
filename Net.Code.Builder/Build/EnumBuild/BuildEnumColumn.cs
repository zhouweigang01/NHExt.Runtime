using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Net.Code.Builder.Base;
using Net.Code.Builder.Build.Model;
using Net.Code.Builder.Build.Tpl;

namespace Net.Code.Builder.Build.EnumBuild
{
    class BuildEnumColumn : IBuild
    {
        protected EnumColumn _col;
        protected string _templateCode;

        public BuildEnumColumn(EnumColumn col)
        {
            _col = col;

        }

        public string BuildCode()
        {
            _templateCode = EnumCodeTemplate.EnumColumnTemplate;
            _templateCode = _templateCode.Replace(Attributes.Class, _col.Entity.Code);
            _templateCode = _templateCode.Replace(Attributes.Code, _col.Code);
            _templateCode = _templateCode.Replace(Attributes.Name, _col.Name);
            _templateCode = _templateCode.Replace(Attributes.Memo, _col.Name);
            _templateCode = _templateCode.Replace(Attributes.Value, _col.EnumValue.ToString());
            return _templateCode;
        }

        public System.Xml.XmlElement BuildCfgXML(System.Xml.XmlDocument xmlDoc)
        {
            throw new NotImplementedException();
        }

        public string BuildMSSQL()
        {
            throw new NotImplementedException();
        }
        public string BuildMetaData()
        {
            string sql = string.Empty;
            sql += "DELETE FROM T_METADATA_ENUM_COLUMN_COMPONENT WHERE F_GUID='" + this._col.Guid + "'" + Environment.NewLine;
            sql += "INSERT INTO T_METADATA_ENUM_COLUMN_COMPONENT VALUES((SELECT ISNULL(MAX(F_ID),0)+1 FROM T_METADATA_ENUM_COLUMN_COMPONENT),0,'" + this._col.Guid + "','" + this._col.Code + "','" + this._col.Name + "'," + this._col.EnumValue + ",'" + this._col.Entity.Guid + "')" + Environment.NewLine;
            return sql;
        }
    }
}
