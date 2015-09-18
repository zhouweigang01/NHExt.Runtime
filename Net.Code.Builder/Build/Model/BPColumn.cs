using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using Net.Code.Builder.Base;

namespace Net.Code.Builder.Build.Model
{
    public class BPColumn : AbstractPlatformComponent
    {
        /// <summary>
        /// 类型分类
        /// </summary>
        private DataTypeEnum _dataType;
        public DataTypeEnum DataType
        {
            get { return _dataType; }
            set
            {
                if (_dataType != value)
                {
                    _dataType = value;
                    this.IsChanged = true;
                }
            }
        }
        /// <summary>
        /// 类型
        /// </summary>
        private string _typeString;
        public string TypeString
        {
            get { return _typeString; }
            set
            {
                if (_typeString != value)
                {
                    _typeString = value;
                    this.IsChanged = true;
                }
            }
        }
        /// <summary>
        /// 是否为空
        /// </summary>
        private bool _isnull;
        public bool IsNull
        {
            get { return _isnull; }
            set
            {
                if (_isnull != value)
                {
                    _isnull = value;
                    this.IsChanged = true;
                }
            }
        }
        /// <summary>
        /// 引用实体guid
        /// </summary>
        private string _refGuid = string.Empty;
        public string RefGuid
        {
            get { return _refGuid; }
            set
            {
                if (_refGuid != value)
                {
                    _refGuid = value;
                    this.IsChanged = true;
                }
            }
        }
        /// <summary>
        /// 引用实体名称
        /// </summary>
        private string _refDTOCode = string.Empty;
        public string RefDTOCode
        {
            get { return _refDTOCode; }
            set
            {
                if (_refDTOCode != value)
                {
                    _refDTOCode = value;
                    this.IsChanged = true;
                }
            }
        }
        /// <summary>
        /// 引用是否为DTO，如果不是DTO需要在后面加上DTO
        /// </summary>
        private bool _isRefDTO = true;
        public bool IsRefDTO
        {
            get { return _isRefDTO; }
        }
        /// <summary>
        /// 所属工程
        /// </summary>
        private BPProj _proj;
        public BPProj Proj
        {
            get { return _proj; }
        }
        /// <summary>
        /// 所属实体
        /// </summary>
        private BPEntity _entity;
        public BPEntity Entity
        {
            get { return _entity; }
        }

        public BPColumn(string guid, BPProj proj, BPEntity entity)
            : base(guid)
        {
            this._guid = guid;
            this._proj = proj;
            this._entity = entity;
            this.DataState = DataState.Add;
        }

        public override void FromXML(XmlNode node)
        {
            this._guid = node.Attributes["Guid"].Value;
            this.Code = node.Attributes["Code"].Value;
            this.Name = node.Attributes["Name"].Value;
            this.IsNull = node.Attributes["IsNull"].Value.ToString() == "1" ? true : false;

            this.DataType = (DataTypeEnum)Enum.Parse(typeof(DataTypeEnum), node.Attributes["DataType"].Value);
            if (node.Attributes["RefGuid"] != null)
            {
                this.RefGuid = node.Attributes["RefGuid"].Value.ToString();
            }
            //这是自动生成的
            if (!string.IsNullOrEmpty(this.RefGuid))//如果是引用类型，需要重新取数值
            {
                XmlNode refEntityNode = this.Entity.Proj.GetEntityNode(this.RefGuid);
                if (refEntityNode != null)
                {
                    //根据引用类型判断当前dto引用的是否是实体，如果是
                    //实体在生成代码的时候需要在默认类型后面加上DTO
                    if (refEntityNode.Name == "Entity" || refEntityNode.Name == "Enum")
                    {
                        this._isRefDTO = false;
                    }
                    if (refEntityNode != null)
                    {
                        this.RefDTOCode = refEntityNode.Attributes["Code"].Value.ToString();
                        if (!_isRefDTO)
                        {
                            this.RefDTOCode += DTOEntity.AttrEndTag;
                        }
                        string projNameSpace = refEntityNode.ParentNode.ParentNode.Attributes["namespace"].Value.ToString() + "." + DTOEntity.AssemblyEndTag;
                        //如果引用DTO直接赋值就可以了

                        if (!string.IsNullOrEmpty(projNameSpace))
                        {
                            this.TypeString = projNameSpace + "." + this.RefDTOCode;
                        }
                        else
                        {
                            this.TypeString = this.RefDTOCode;
                        }
                    }
                }
            }
            else
            {
                this.TypeString = node.Attributes["Type"].Value;
            }
            this.DataState = DataState.Update;
            this.IsChanged = false;
        }

        public override void ToXML(XmlDocument xmlDoc)
        {
            if (this.DataState == DataState.Add)
            {
                XmlNode node = xmlDoc.SelectSingleNode("BPProj/BPList/BP[@Guid='" + this.Entity.Guid + "']");
                node = node.SelectSingleNode("Attributes");

                XmlElement el = xmlDoc.CreateElement("Attribute");
                el.SetAttribute("Guid", this.Guid);
                el.SetAttribute("Code", this.Code);
                el.SetAttribute("Name", this.Name);
                el.SetAttribute("DataType", this.DataType.ToString());
                if (string.IsNullOrEmpty(this.RefGuid))
                {
                    el.SetAttribute("Type", this.TypeString);
                }
                else
                {
                    el.SetAttribute("RefGuid", this.RefGuid);
                }
                el.SetAttribute("IsNull", this.IsNull ? "1" : "0");
                node.AppendChild(el);

                this.DataState = DataState.Update;
                this.IsChanged = false;
            }
            else if (this.DataState == DataState.Update && this.IsChanged)
            {
                XmlNode node = xmlDoc.SelectSingleNode("BPProj/BPList/BP[@Guid='" + this.Entity.Guid + "']");
                node = node.SelectSingleNode("Attributes/Attribute[@Guid='" + this.Guid + "']");
                if (node == null)
                {
                    throw new Exception("更新的实体属性节点没有找到");
                }
                string refGuid = string.Empty;
                if (node.Attributes["RefGuid"] != null && node.Attributes["RefGuid"].Value != null && !string.IsNullOrEmpty(node.Attributes["RefGuid"].Value.ToString()))
                {
                    refGuid = node.Attributes["RefGuid"].Value.ToString();
                }
                //获取原来的类型
                DataTypeEnum dataType = (DataTypeEnum)Enum.Parse(typeof(DataTypeEnum), node.Attributes["DataType"].Value);

                node.Attributes["Code"].Value = this.Code;
                node.Attributes["Name"].Value = this.Name;
                node.Attributes["DataType"].Value = this.DataType.ToString();

                if (string.IsNullOrEmpty(this.RefGuid))
                {
                    if (node.Attributes["Type"] != null)
                    {
                        node.Attributes["Type"].Value = this.TypeString;
                    }
                    else
                    {
                        XmlAttribute attr = xmlDoc.CreateAttribute("Type");
                        attr.Value = this.TypeString;
                        node.Attributes.Append(attr);
                    }
                    node.Attributes.RemoveNamedItem("RefGuid");
                }
                else
                {
                    if (node.Attributes["RefGuid"] != null)
                    {
                        node.Attributes["RefGuid"].Value = this.RefGuid;
                    }
                    else
                    {
                        XmlAttribute attr = xmlDoc.CreateAttribute("RefGuid");
                        attr.Value = this.RefGuid;
                        node.Attributes.Append(attr);
                    }
                    node.Attributes.RemoveNamedItem("Type");
                }
                node.Attributes["IsNull"].Value = this.IsNull ? "1" : "0";

                this.IsChanged = false;
            }
            else if (this.DataState == DataState.Delete)
            {
                XmlNode node = xmlDoc.SelectSingleNode("BPProj/BPList/BP[@Guid='" + this.Entity.Guid + "']");
                node = node.SelectSingleNode("Attributes/Attribute[@Guid='" + this.Guid + "']");
                if (node != null)
                {
                    node.ParentNode.RemoveChild(node);
                }
            }
        }
    }
}
