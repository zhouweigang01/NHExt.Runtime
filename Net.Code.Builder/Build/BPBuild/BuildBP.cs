using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Net.Code.Builder.Base;
using Net.Code.Builder.Build.Model;
using Net.Code.Builder.Build.Tpl;
using System.Xml;

namespace Net.Code.Builder.Build.BPBuild
{
    class BuildBP : IBuild
    {
        private string _namespace;
        private BPEntity _entity;
        public BuildBP(BPEntity entity)
        {
            _entity = entity;
            _namespace = entity.Proj.Namespace;
        }

        public string BuildCode()
        {
            string buildCode = BPCodeTemplate.BPClassBeginTemplate.Replace(Attributes.Class, _entity.Code);
            buildCode = buildCode.Replace(Attributes.NameSpace, _namespace);
            buildCode = buildCode.Replace(Attributes.Guid, _entity.Guid);
            //生成属性信息
            foreach (BPColumn col in this._entity.ColumnList)
            {
                IBuild b = BuildBPColumnFactory.Create(col);
                buildCode += b.BuildCode();
            }
            #region BP内部调用Do方法


            string tpl = BPCodeTemplate.BPDoExtendObjectTemplate.Replace(Attributes.Trans, getTransTemplate(_entity.Trans));
            if (_entity.IsList)
            {
                tpl = tpl.Replace(Attributes.Return, "List<" + _entity.ReturnName + ">");
            }
            else
            {
                tpl = tpl.Replace(Attributes.Return, _entity.ReturnName);
            }
            buildCode += tpl;

            #endregion

            #region BP外部调用Do方法


            tpl = BPCodeTemplate.BPDoObjectObjectTemplate.Replace(Attributes.Trans, getTransTemplate(_entity.Trans));
            if (_entity.IsList)
            {
                tpl = tpl.Replace(Attributes.Return, "List<" + _entity.ReturnName + ">");
            }
            else
            {
                tpl = tpl.Replace(Attributes.Return, _entity.ReturnName);
            }
            buildCode += tpl;

            #endregion

            #region 通过WCF调用Do方法
            //生成DoWcf函数
            buildCode += BPCodeTemplate.BPDoWcfObjectTemplate.Replace(Attributes.Trans, getTransTemplate(_entity.Trans));
            #endregion

            #region 通用DoCommon方法
            //生成Do函数

            if (_entity.IsList)
            {
                buildCode += BPCodeTemplate.BPDoCommonObjectTemplate.Replace(Attributes.Trans, getTransTemplate(_entity.Trans)).Replace(Attributes.Return, "List<" + _entity.ReturnName + ">");
            }
            else
            {
                buildCode += BPCodeTemplate.BPDoCommonObjectTemplate.Replace(Attributes.Trans, getTransTemplate(_entity.Trans)).Replace(Attributes.Return, _entity.ReturnName);
            }

            #endregion

            #region 注册类型转换函数
            if (_entity.ReturnName != "void")
            {
                if (_entity.IsList)
                {
                    buildCode += BPCodeTemplate.BPTypeConvertBeginTemplate.Replace(Attributes.Return, "List<" + _entity.ReturnName + ">").Replace(Attributes.ProxyReturn, "List<" + _entity.ReturnProxyName + ">");
                }
                else
                {
                    buildCode += BPCodeTemplate.BPTypeConvertBeginTemplate.Replace(Attributes.Return, _entity.ReturnName).Replace(Attributes.ProxyReturn, _entity.ReturnProxyName);
                }
                if (_entity.IsEntity)
                {
                    if (_entity.IsList)
                    {
                        buildCode += BPCodeTemplate.BPTypeConvertListToDTOTemplate.Replace(Attributes.Return, _entity.ReturnName);
                    }
                    else
                    {
                        buildCode += BPCodeTemplate.BPTypeConvertToDTOTemplate;
                    }
                }
                else
                {
                    buildCode += BPCodeTemplate.BPTypeConvertCommonTemplate;
                }
                buildCode += BPCodeTemplate.BPTypeConvertEndTemplate;
            }
            #endregion

            #region 生成初始化属性列表函数
            buildCode += BPCodeTemplate.BPClassInitParamBeginTemplate;
            for (int i = 0; i < this._entity.ColumnList.Count; i++)
            {
                BPColumn col = this._entity.ColumnList[i];
                string codeTmp = string.Empty;
                //如果是聚合的话
                if (col.DataType == DataTypeEnum.CompositionType)
                {
                    codeTmp = BPCodeTemplate.BPClassInitParamCompositionTemplate;
                }
                else
                {
                    codeTmp = BPCodeTemplate.BPClassInitParamCommonTemplate;
                }
                codeTmp = codeTmp.Replace(Attributes.Code, col.Code).Replace(Attributes.Index, i.ToString()).Replace(Attributes.TypeString, col.TypeString);

                if (!string.IsNullOrEmpty(col.RefGuid))
                {
                    codeTmp = codeTmp.Replace(Attributes.IsNull, "");
                }
                else
                {
                    if (col.TypeString.ToLower() == "string")
                    {
                        codeTmp = codeTmp.Replace(Attributes.IsNull, "");
                    }
                    else
                    {
                        codeTmp = codeTmp.Replace(Attributes.IsNull, col.IsNull ? "?" : "");
                    }
                }
                buildCode += codeTmp;
            }
            buildCode += BPCodeTemplate.BPClassInitParamEndTemplate;
            #endregion

            #region 生成SetValue覆盖方法
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
            #endregion

            buildCode += BPCodeTemplate.BPClassEndTemplate;

            return buildCode;
        }

        public string BuildExtendCode()
        {
            string buildCode = BPCodeTemplate.BPClassExtendTemplate.Replace(Attributes.Class, _entity.Code);
            buildCode = buildCode.Replace(Attributes.NameSpace, _namespace);
            if (_entity.IsList)
            {
                buildCode = buildCode.Replace(Attributes.Return, "List<" + _entity.ReturnName + ">");
            }
            else
            {
                buildCode = buildCode.Replace(Attributes.Return, _entity.ReturnName);
            }
            return buildCode;
        }

        public System.Xml.XmlElement BuildCfgXML(System.Xml.XmlDocument xmlDoc)
        {
            XmlElement el = xmlDoc.CreateElement("Service");
            el.SetAttribute("Name", this._entity.Code);
            return el;
        }

        public string BuildMSSQL()
        {
            throw new NotImplementedException();
        }

        public string BuildMetaData()
        {
            string sql = string.Empty;
            sql += "DELETE FROM T_METADATA_BP_COMPONENT WHERE F_GUID='" + this._entity.Guid + "'" + Environment.NewLine;
            sql += "INSERT INTO T_METADATA_BP_COMPONENT VALUES((SELECT ISNULL(MAX(F_ID),0)+1 FROM T_METADATA_BP_COMPONENT),0,'" + this._entity.Guid + "','" + this._entity.Code + "','" + this._entity.Name + "','" + this._entity.Proj.Namespace + "." + this._entity.Code + "','" + this._entity.Proj.Namespace + ".dll" + "','" + this._entity.Proj.Guid + "'," + (this._entity.IsAuth ? "1" : "0") + ",'" + this._entity.FloderGuid + "',0)" + Environment.NewLine;
            sql += "DELETE FROM T_METADATA_BP_COLUMN_COMPONENT WHERE F_BPGUID='" + this._entity.Guid + "'" + Environment.NewLine;
            foreach (BPColumn col in this._entity.ColumnList)
            {
                Build.BPBuild.BuildBPCommon columnBuilder = new BuildBPCommon(col);
                sql += columnBuilder.BuildMetaData();
            }
            return sql;
        }

        private string getTransTemplate(int v)
        {
            if (v == 0) return BPCodeTemplate.TransSupportTemplate;
            if (v == 1) return BPCodeTemplate.TransRequiredTemplate;
            if (v == 2) return BPCodeTemplate.TransRequiredNewTemplate;
            throw new Exception("没有定义事务代码片段");
        }
    }
}
