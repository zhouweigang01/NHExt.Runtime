using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using Net.Code.Builder.Base;
using Net.Code.Builder.Build.Model;
using Net.Code.Builder.Build.Tpl;

namespace Net.Code.Builder.Build.DTOBuild
{
    class BuildDTO : IBuild
    {
        private string _namespace;
        DTOEntity _entity;

        private List<DTOColumn> _inhertColumnList = new List<DTOColumn>();

        public BuildDTO(DTOEntity entity)
        {
            _entity = entity;
            _namespace = entity.Proj.Namespace + ".Deploy";
        }

        public string BuildCode()
        {
            string buildCode = DTOCodeTemplate.ClassBeginTemplate.Replace(Attributes.Class, _entity.Code);
            //替换变量
            buildCode = buildCode.Replace(Attributes.EntityName, _namespace + "." + _entity.Code);
            buildCode = buildCode.Replace(Attributes.NameSpace, _namespace);
            buildCode = buildCode.Replace(Attributes.InhertClass, _entity.InhertName);
            //基类中已经实现了sysversion和id
            foreach (DTOColumn col in _entity.ColumnList)
            {
                IBuild b = null;
                if (col.DataType == DataTypeEnum.CommonType)
                {
                    b = new BuildDTOCommon(col);
                }
                else if (col.DataType == DataTypeEnum.RefreceType)
                {
                    b = new BuildDTORefrence(col);
                }
                else if (col.DataType == DataTypeEnum.CompositionType)
                {
                    b = new BuildDTOComposition(col);
                }
                buildCode += b.BuildCode();
            }
            //生成setvalue函数
            if (_entity.ColumnList.Count > 0)
            {
                buildCode += DTOCodeTemplate.DTOSetValueBeginTemplate;
                foreach (DTOColumn col in _entity.ColumnList)
                {
                    if (col.DataType != DataTypeEnum.CompositionType)
                    {
                        bool isnull = (col.IsNull && col.TypeString.ToLower() != "string");
                        buildCode += DTOCodeTemplate.DTOSetCommonValueTemplate.Replace(Attributes.Code, col.Code).Replace(Attributes.IsNull, isnull ? "?" : "").Replace(Attributes.TypeString, col.TypeString);
                    }
                    else
                    {
                        buildCode += DTOCodeTemplate.DTOSetListValueTemplate.Replace(Attributes.Code, col.Code).Replace(Attributes.TypeString, col.TypeString);
                    }
                }
                buildCode += DTOCodeTemplate.DTOSetValueEndTemplate;
            }
            buildCode += DTOCodeTemplate.ClassEndTemplate;

            return buildCode;
        }

        public string BuildExtendCode()
        {
            string buildCode = DTOCodeTemplate.ClassExtendTemplate.Replace(Attributes.Class, _entity.Code);
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
