using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Net.Code.Builder.Base;
using Net.Code.Builder.Build.Model;
using Net.Code.Builder.Build.Tpl;

namespace Net.Code.Builder.Build.BPBuild
{
    abstract class BuildBPColumn : IBuild
    {
        protected BPColumn _col;
        protected string _templateCode;

        public BuildBPColumn(BPColumn col)
        {
            _col = col;
        }

        public virtual string BuildCode()
        {
            _templateCode = _templateCode.Replace(Attributes.Code, _col.Code);
            _templateCode = _templateCode.Replace(Attributes.Memo, _col.Name);
            _templateCode = _templateCode.Replace(Attributes.TypeString, _col.TypeString);
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
            sql += "INSERT INTO T_METADATA_BP_COLUMN_COMPONENT VALUES((SELECT ISNULL(MAX(F_ID),0)+1 FROM T_METADATA_BP_COLUMN_COMPONENT),0,'" + this._col.Code + "','" + this._col.Name + "','" + this._col.TypeString + "','" + this._col.RefGuid + "','" + this._col.Entity.Guid + "'," + (this._col.IsNull ? "1" : "0") + ")" + Environment.NewLine;
            return sql;
        }
    }
}
