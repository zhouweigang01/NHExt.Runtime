using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Net.Code.Builder.Base;
using Net.Code.Builder.Build.Model;
using Net.Code.Builder.Build.Tpl;

namespace Net.Code.Builder.Build.EnumBuild
{
    class BuildEnumEntity : IBuild
    {
        private string _namespace;
        private EnumEntity _entity;

        public BuildEnumEntity(EnumEntity entity)
        {
            _entity = entity;
            _namespace = entity.Proj.Namespace;
        }
        public string BuildCode()
        {
            string buildCode = EnumCodeTemplate.EnumClassBeginTemplate.Replace(Attributes.Class, _entity.Code);
            buildCode = buildCode.Replace(Attributes.NameSpace, _namespace);

            //基类中已经实现了sysversion和id
            foreach (EnumColumn col in _entity.ColumnList)
            {
                IBuild b = new BuildEnumColumn(col);
                buildCode += b.BuildCode();
            }
            buildCode += EnumCodeTemplate.EnumClassEndTemplate;

            return buildCode;
        }

        public string BuildExtendCode()
        {
            string buildCode = EnumCodeTemplate.EnumClassExtendTemplate.Replace(Attributes.Class, _entity.Code);
            buildCode = buildCode.Replace(Attributes.NameSpace, _namespace);
            return buildCode;
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
            sql += "DELETE FROM T_METADATA_ENUM_COMPONENT WHERE F_GUID='" + this._entity.Guid + "'" + Environment.NewLine;
            sql += "INSERT INTO T_METADATA_ENUM_COMPONENT VALUES((SELECT ISNULL(MAX(F_ID),0)+1 FROM T_METADATA_ENUM_COMPONENT),0,'" + this._entity.Guid + "','" + this._entity.Code + "','" + this._entity.Name + "','" + this._entity.Proj.Namespace + "." + this._entity.Code + "','" + this._entity.Proj.Namespace + ".dll" + "','" + this._entity.Proj.Guid + "')" + Environment.NewLine;

            sql += "DELETE FROM T_METADATA_ENUM_COLUMN_COMPONENT WHERE F_ENUMGUID='" + this._entity.Guid + "'" + Environment.NewLine;

            foreach (EnumColumn col in this._entity.ColumnList)
            {
                IBuild b = new BuildEnumColumn(col);
                sql += b.BuildMetaData() + Environment.NewLine;
            }
            return sql;
        }
    }
}
