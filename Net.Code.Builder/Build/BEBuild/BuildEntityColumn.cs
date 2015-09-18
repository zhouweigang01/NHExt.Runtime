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
    abstract class BuildEntityColumn : IBuild
    {
        protected BEColumn _col;
        protected string _templateCode;

        public BuildEntityColumn(BEColumn col)
        {
            _col = col;
        }

        public virtual string BuildCode()
        {
            _templateCode = _templateCode.Replace(Attributes.Code, this._col.Code);
            _templateCode = _templateCode.Replace(Attributes.Memo, this._col.Name);

            _templateCode = _templateCode.Replace(Attributes.TypeString, this._col.TypeString);
            _templateCode = _templateCode.Replace(Attributes.EntityRefrence, _col.DataType == DataTypeEnum.CommonType ? "false" : "true");
            _templateCode = _templateCode.Replace(Attributes.IsViewer, _col.IsViewer ? "true" : "false");
            _templateCode = _templateCode.Replace(Attributes.IsBizKey, _col.IsBizKey ? "true" : "false");
            ///标签上用的，当前字段是否可空
            _templateCode = _templateCode.Replace(Attributes.CanNull, _col.IsNull ? "true" : "false");

            return _templateCode;
        }
        public virtual string BuildMSSQL()
        {
            throw new NotImplementedException();
        }
        public virtual XmlElement BuildCfgXML(XmlDocument xmlDoc)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// 实体引用类型
        /// </summary>
        /// <returns></returns>
        public virtual string BuildMetaData()
        {
            string sql = string.Empty;
            sql += "DELETE FROM T_METADATA_ENTITY_COLUMN_COMPONENT WHERE F_GUID='" + this._col.Guid + "' and F_ENTITYGUID='" + this._col.Entity.Guid + "'" + Environment.NewLine;
            sql += "INSERT INTO T_METADATA_ENTITY_COLUMN_COMPONENT VALUES((SELECT ISNULL(MAX(F_ID),0)+1 FROM T_METADATA_ENTITY_COLUMN_COMPONENT),0,'" + this._col.Guid + "','" + this._col.Code + "','" + this._col.Name + "','" + this._col.TypeString + "','" + this._col.RefGuid + "','" + this._col.Entity.Guid + "'," + (int)ColumnTypeEnum.EntityType + ")" + Environment.NewLine;
            return sql;
        }

        public abstract string EntityColumnToDTOColumn();
        public abstract string EntityColumnFromDTOColumn();

        public virtual string BuildForeignKey()
        {
            return string.Empty;
        }
        public virtual string BuildRemoveForeignKey()
        {
            return string.Empty;
        }
        public virtual string BuildCloneCode()
        {
            return string.Empty;
        }
    }
}
