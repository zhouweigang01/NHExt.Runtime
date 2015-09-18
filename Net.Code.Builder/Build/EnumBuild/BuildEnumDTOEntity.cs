using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Net.Code.Builder.Base;
using Net.Code.Builder.Build.Model;
using Net.Code.Builder.Build.Tpl;

namespace Net.Code.Builder.Build.EnumBuild
{
    class BuildEnumDTOEntity : IBuild
    {
        private string _namespace;
        private EnumEntity _entity;

        public BuildEnumDTOEntity(EnumEntity entity)
        {
            _entity = entity;
            _namespace = entity.Proj.Namespace + "." + DTOEntity.AssemblyEndTag;
        }

        public string BuildCode()
        {
            string buildCode = EnumCodeTemplate.EnumDTOClassBeginTemplate.Replace(Attributes.Class, _entity.Code + DTOEntity.AttrEndTag);
            buildCode = buildCode.Replace(Attributes.NameSpace, _namespace);

            //基类中已经实现了sysversion和id
            foreach (EnumColumn col in _entity.ColumnList)
            {
                IBuild b = new BuildEnumDTOColumn(col);
                buildCode += b.BuildCode();
            }
            buildCode += EnumCodeTemplate.EnumDTOClassEndTemplate;

            return buildCode;
        }
        public string BuildExtendCode()
        {
            string buildCode = EnumCodeTemplate.EnumDTOClassExtendTemplate.Replace(Attributes.Class, _entity.Code + DTOEntity.AttrEndTag);
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
            throw new NotImplementedException();
        }
    }
}
