using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Net.Code.Builder.Base;
using Net.Code.Builder.Build.Model;
using Net.Code.Builder.Build.Tpl;

namespace Net.Code.Builder.Build.EnumBuild
{
    class BuildEnumDTOColumn : IBuild
    {
        protected EnumColumn _col;
        protected string _templateCode;

        public BuildEnumDTOColumn(EnumColumn col)
        {
            _col = col;
        }

        public string BuildCode()
        {
            _templateCode = EnumCodeTemplate.EnumDTOColumnTemplate;
            _templateCode = _templateCode.Replace(Attributes.Class, _col.Entity.Code + DTOEntity.AttrEndTag);
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
            throw new NotImplementedException();
        }
    }
}
