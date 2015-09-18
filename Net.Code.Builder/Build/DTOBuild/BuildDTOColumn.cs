using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Net.Code.Builder.Base;
using Net.Code.Builder.Build.Model;
using Net.Code.Builder.Build.Tpl;

namespace Net.Code.Builder.Build.DTOBuild
{
    public abstract class BuildDTOColumn : IBuild
    {
        protected DTOColumn _col;
        protected string _templateCode;

        public BuildDTOColumn(DTOColumn col)
        {
            _col = col;
        }

        public virtual string BuildCode()
        {
            _templateCode = _templateCode.Replace(Attributes.Code, _col.Code);
            _templateCode = _templateCode.Replace(Attributes.Memo, _col.Name);
            _templateCode = _templateCode.Replace(Attributes.TypeString, _col.TypeString);
            _templateCode = _templateCode.Replace(Attributes.EntityRefrence, _col.EntityRefrence ? "true" : "false");
            _templateCode = _templateCode.Replace(Attributes.IsViewer, _col.IsViewer ? "true" : "false");
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
