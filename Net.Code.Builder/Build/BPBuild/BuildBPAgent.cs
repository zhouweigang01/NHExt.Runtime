using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Net.Code.Builder.Base;
using Net.Code.Builder.Build.Model;
using Net.Code.Builder.Build.Tpl;

namespace Net.Code.Builder.Build.BPBuild
{
    class BuildBPAgent : IBuild
    {
        private string _namespace;
        private string _bpNamespace;
        private BPEntity _entity;
        public BuildBPAgent(BPEntity entity)
        {
            _entity = entity;
            _namespace = entity.Proj.Namespace + ".Agent";
            _bpNamespace = entity.Proj.Namespace;
        }
        public string BuildCode()
        {
            string buildCode = BPCodeTemplate.ProxyClassBeginTemplate.Replace(Attributes.Class, _entity.Code);
            buildCode = buildCode.Replace(Attributes.NameSpace, _namespace);
            buildCode = buildCode.Replace(Attributes.Guid, _entity.Guid);
            //生成属性信息
            foreach (BPColumn col in this._entity.ColumnList)
            {
                IBuild b = null;
                if (col.DataType == DataTypeEnum.CommonType)
                {
                    b = new BuildBPCommon(col);
                }
                else if (col.DataType == DataTypeEnum.RefreceType)
                {
                    b = new BuildBPRefrence(col);
                }
                else if (col.DataType == DataTypeEnum.CompositionType)
                {
                    b = new BuildBPComposition(col);
                }
                buildCode += b.BuildCode();
            }
            //生成构造函数
            buildCode += BPCodeTemplate.ProxyConstructorTemplate.Replace(Attributes.Class, _entity.Code).Replace(Attributes.NameSpace, _bpNamespace).Replace(Attributes.Guid, _entity.Guid);

            //生成do方法
            if (_entity.IsList)
            {
                buildCode += BPCodeTemplate.ProxyDoListBeginTemplate.Replace(Attributes.Return, _entity.ReturnProxyName);
            }
            else
            {
                buildCode += BPCodeTemplate.ProxyDoCommonBeginTemplate.Replace(Attributes.Return, _entity.ReturnProxyName);
            }
            //生成属性信息
            foreach (BPColumn col in this._entity.ColumnList)
            {
                string attr = BPCodeTemplate.ProxyInitParamListTemplate.Replace(Attributes.Code, col.Code);
                buildCode += attr;
            }

            if (_entity.IsList)
            {
                buildCode += BPCodeTemplate.ProxyDoListTemplate.Replace(Attributes.Return, _entity.ReturnProxyName);
                buildCode += BPCodeTemplate.ProxyDoListEndTemplate.Replace(Attributes.Return, _entity.ReturnProxyName);
            }
            else
            {
                buildCode += BPCodeTemplate.ProxyDoCommonTemplate.Replace(Attributes.Return, _entity.ReturnProxyName);
                buildCode += BPCodeTemplate.ProxyDoCommonEndTemplate.Replace(Attributes.Return, _entity.ReturnProxyName);
            }

            //生成SetValue覆盖方法
            if (_entity.ColumnList.Count > 0)
            {
                buildCode += BPCodeTemplate.BPSetValueBeginTemplate;
                foreach (BPColumn col in this._entity.ColumnList)
                {
                    if (col.DataType == DataTypeEnum.CompositionType)
                    {
                        buildCode += BPCodeTemplate.BPSetListValueTemplate.Replace(Attributes.Code, col.Code).Replace(Attributes.TypeString, col.TypeString);
                    }
                    else
                    {
                        buildCode += BPCodeTemplate.BPSetCommonValueTemplate.Replace(Attributes.Code, col.Code).Replace(Attributes.IsNull, col.IsNull ? "?" : "").Replace(Attributes.TypeString, col.TypeString);
                    }
                }
                buildCode += BPCodeTemplate.BPSetValueEndTemplate;
            }
            buildCode += BPCodeTemplate.ProxyClassEndTemplate;

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
