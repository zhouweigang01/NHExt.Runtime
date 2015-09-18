using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using Net.Code.Builder.Base;

namespace Net.Code.Builder.Build.Model
{
    public class BEColumn : AbstractPlatformComponent
    {

        /// <summary>
        /// 字段在数据库中的名称
        /// </summary>
        public string DBCode
        {
            get { return "F_" + this.Code.ToUpper(); }
        }
        public string RefDBCode
        {
            get { return "F_" + this.RefColCode.ToUpper(); }
        }


        /// <summary>
        /// 类型分类
        /// </summary>
        private DataTypeEnum _dataType;
        public DataTypeEnum DataType
        {
            get { return this._dataType; }
            set
            {
                if (this._dataType != value)
                {
                    this._dataType = value;
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
            get { return this._typeString; }
            set
            {
                if (this._typeString != value)
                {
                    this._typeString = value;
                    this.IsChanged = true;
                }
            }
        }
        /// <summary>
        /// 字符串类型的时候字段的长度
        /// </summary>
        private int _length = 0;
        public int Length
        {
            get { return this._length; }
            set
            {
                if (this._length != value)
                {
                    this._length = value;
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
            get { return this._isnull; }
            set
            {
                if (this._isnull != value)
                {
                    this._isnull = value;
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
            get { return this._refGuid; }
            set
            {
                if (this._refGuid != value)
                {
                    this._refGuid = value;
                    this.IsChanged = true;
                }
            }
        }
        /// <summary>
        /// 引用列guid
        /// </summary>
        private string _refColGuid = string.Empty;
        public string RefColGuid
        {
            get { return this._refColGuid; }
            set
            {
                if (this._refColGuid != value)
                {
                    this._refColGuid = value;
                    this.IsChanged = true;
                }
            }
        }
        /// <summary>
        /// 引用列的数据库字段名称（这个字段只有当聚合关系时）
        /// 主实体生成NH配置文件的时候需要知道子实体的字段是什么
        /// </summary>
        private string _refColCode = string.Empty;
        public string RefColCode
        {
            get { return this._refColCode; }
        }
        /// <summary>
        /// 引用实体名称
        /// </summary>
        private string _refEntityCode = string.Empty;
        public string RefEntityCode
        {
            get { return this._refEntityCode; }
            set
            {
                if (this._refEntityCode != value)
                {
                    this._refEntityCode = value;
                    this.IsChanged = true;
                }
            }
        }
        private string _refEntityTableName = string.Empty;
        /// <summary>
        /// 引用实体表的名称
        /// </summary>
        public string RefEntityTableName
        {
            get
            {
                if (string.IsNullOrEmpty(this._refEntityTableName))
                {
                    this._refEntityTableName = "T_" + _refEntityCode;
                }
                return this._refEntityTableName;
            }
            set
            {
                if (this._refEntityTableName != value)
                {
                    this._refEntityTableName = value;
                    this.IsChanged = true;
                }
            }
        }
        private bool _isViewer = false;
        /// <summary>
        /// 是否可以浏览
        /// </summary>
        public bool IsViewer
        {
            get { return this._isViewer; }
            set
            {
                if (this._isViewer != value)
                {
                    this._isViewer = value;
                    this.IsChanged = true;
                }
            }
        }

        private bool _isBizKey = false;
        /// <summary>
        /// 是否业务主键
        /// </summary>
        public bool IsBizKey
        {
            get
            {
                return this._isBizKey;
            }
            set
            {
                if (this._isBizKey != value)
                {
                    this._isBizKey = value;
                    this.IsChanged = true;
                }
            }
        }

        /// <summary>
        /// 当前列引用是否是枚举
        /// </summary>
        private bool _isEnum = false;
        public bool IsEnum
        {
            get { return this._isEnum; }
            set { this._isEnum = value; }
        }

        /// <summary>
        /// 所属工程
        /// </summary>
        private BEProj _proj;
        public BEProj BEProj
        {
            get { return this._proj; }
        }
        /// <summary>
        /// 所属实体
        /// </summary>
        private BEEntity _entity;
        public BEEntity Entity
        {
            get { return this._entity; }
        }

        public BEColumn(string guid, BEProj proj, BEEntity entity)
            : base(guid)
        {
            this._guid = guid;
            this._proj = proj;
            this._entity = entity;
            this.DataState = DataState.Add;
        }
        /// <summary>
        /// 从xml文件中初始化对象
        /// </summary>
        /// <param name="xml"></param>
        public override void FromXML(XmlNode node)
        {
            this._guid = node.Attributes["Guid"].Value;
            this.Code = node.Attributes["Code"].Value;
            this.Name = node.Attributes["Name"].Value;
            this.IsNull = node.Attributes["IsNull"].Value.ToString() == "1" ? true : false;
            if (node.Attributes["IsViewer"] != null)
            {
                this._isViewer = node.Attributes["IsViewer"].Value.ToString() == "1" ? true : false;
            }
            else
            {
                this._isViewer = false;
            }
            if (node.Attributes["IsBizKey"] != null)
            {
                this._isBizKey = node.Attributes["IsBizKey"].Value.ToString() == "1" ? true : false;
            }
            else
            {
                this._isBizKey = false;
            }
            this._dataType = (DataTypeEnum)Enum.Parse(typeof(DataTypeEnum), node.Attributes["DataType"].Value);
            if (this._dataType != DataTypeEnum.CommonType)
            {
                this._refGuid = node.Attributes["RefGuid"].Value.ToString();
                //如果是聚合类型需要知道聚合对象的字段是哪个
                if (this._dataType == Base.DataTypeEnum.CompositionType)
                {
                    //如果出现聚合，则需要重新查找该聚合字段所对应的引用字段
                    XmlDocument xd = new XmlDocument();
                    xd.Load(this.BEProj.ProjPath);
                    XmlNode refNode = xd.SelectSingleNode("EntityProj/EntityList/Entity[@Guid='" + this.RefGuid + "']/Attributes/Attribute[@RefColGuid='" + this.Guid + "']");
                    if (refNode != null)
                    {
                        this._refColCode = refNode.Attributes["Code"].Value.ToString();
                    }
                }
            }
            if (this._dataType == DataTypeEnum.RefreceType)
            {
                if (node.Attributes["RefColGuid"] != null && node.Attributes["RefColGuid"].Value != null && !string.IsNullOrEmpty(node.Attributes["RefColGuid"].Value.ToString()))
                {
                    this._refColGuid = node.Attributes["RefColGuid"].Value.ToString();
                }
            }
            //这是自动生成的
            if (!string.IsNullOrEmpty(this._refGuid))//如果是引用类型，需要重新取数值
            {
                XmlNode refEntityNode = this.Entity.Proj.GetEntityNode(this.RefGuid);
                if (refEntityNode != null)
                {
                    if (refEntityNode.Name == "Enum")
                    {
                        this._isEnum = true;
                    }
                    this._refEntityCode = refEntityNode.Attributes["Code"].Value.ToString();
                    if (refEntityNode.Attributes["Table"] != null)
                    {
                        this._refEntityTableName = refEntityNode.Attributes["Table"].Value.ToString();
                    }
                    else
                    {
                        if (!this._isEnum)
                        {
                            this._refEntityTableName = "T_" + this.RefEntityCode.ToUpper();
                        }
                    }
                    string projNameSpace = refEntityNode.ParentNode.ParentNode.Attributes["namespace"].Value.ToString();
                    if (!string.IsNullOrEmpty(projNameSpace))
                    {
                        this._typeString = projNameSpace + "." + this.RefEntityCode;
                    }
                    else
                    {
                        this._typeString = this.RefEntityCode;
                    }
                }
                this._length = 0;
            }
            else
            {
                this._typeString = node.Attributes["Type"].Value;
                if (this._typeString == "string")
                {
                    if (node.Attributes["Length"] != null)
                    {
                        this._length = int.Parse(node.Attributes["Length"].Value);
                    }
                }
            }
            this.DataState = DataState.Update;
            this.IsChanged = false;
        }
        /// <summary>
        /// 将对象转换成xml文件
        /// </summary>
        public override void ToXML(XmlDocument xmlDoc)
        {
            if (this.DataState == DataState.Add)
            {
                XmlNode node = xmlDoc.SelectSingleNode("EntityProj/EntityList/Entity[@Guid='" + this.Entity.Guid + "']/Attributes");

                XmlElement el = xmlDoc.CreateElement("Attribute");
                el.SetAttribute("Guid", this.Guid);
                if (this.DataType != DataTypeEnum.CommonType)
                {
                    el.SetAttribute("RefGuid", this.RefGuid);
                }
                el.SetAttribute("Code", this.Code);
                el.SetAttribute("Name", this.Name);
                el.SetAttribute("DataType", this.DataType.ToString());
                if (this.DataType == DataTypeEnum.CommonType)
                {
                    el.SetAttribute("Type", this.TypeString);
                    if (this.TypeString == "string")
                    {
                        el.SetAttribute("Length", this.Length.ToString());
                    }
                }
                el.SetAttribute("IsNull", this.IsNull ? "1" : "0");
                el.SetAttribute("IsViewer", this.IsViewer ? "1" : "0");
                el.SetAttribute("IsBizKey", this.IsBizKey ? "1" : "0");
                node.AppendChild(el);

                //聚合需要到对应实体上生成一个引用树形
                addPrimaryRefNode(xmlDoc, this.RefGuid, this.Guid);

                this.DataState = DataState.Update;
                this.IsChanged = false;
            }
            else if (this.DataState == DataState.Update && this.IsChanged)
            {
                XmlNode node = xmlDoc.SelectSingleNode("EntityProj/EntityList/Entity[@Guid='" + this.Entity.Guid + "']");
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
                //如果原来是聚合并且引用对象发生了改变，则需要删除聚合引用
                if (dataType == DataTypeEnum.CompositionType && refGuid != this.RefGuid)
                {
                    removePrimaryRefNode(xmlDoc, refGuid, this.Guid);
                    if (this.DataType == DataTypeEnum.CompositionType)
                    {
                        addPrimaryRefNode(xmlDoc, this.RefGuid, this.Guid);
                    }
                }
                node.Attributes["Code"].Value = this.Code;
                node.Attributes["Name"].Value = this.Name;
                node.Attributes["DataType"].Value = this.DataType.ToString();

                if (this.DataType == DataTypeEnum.CommonType)
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
                    if (this.TypeString == "string")
                    {
                        if (node.Attributes["Length"] != null)
                        {
                            node.Attributes["Length"].Value = this._length.ToString();
                        }
                        else
                        {
                            XmlAttribute attr = xmlDoc.CreateAttribute("Length");
                            attr.Value = this._length.ToString();
                            node.Attributes.Append(attr);
                        }
                    }
                    else
                    {
                        node.Attributes.RemoveNamedItem("Length");
                    }

                    node.Attributes.RemoveNamedItem("RefGuid");
                    node.Attributes.RemoveNamedItem("RefColGuid");
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
                if (node.Attributes["IsNull"] != null)
                {
                    node.Attributes["IsNull"].Value = this.IsNull ? "1" : "0";
                }
                else
                {
                    XmlAttribute attr = xmlDoc.CreateAttribute("IsNull");
                    attr.Value = this.IsNull ? "1" : "0";
                    node.Attributes.Append(attr);
                }
                if (node.Attributes["IsViewer"] != null)
                {
                    node.Attributes["IsViewer"].Value = this.IsViewer ? "1" : "0";
                }
                else
                {
                    XmlAttribute attr = xmlDoc.CreateAttribute("IsViewer");
                    attr.Value = this.IsViewer ? "1" : "0";
                    node.Attributes.Append(attr);
                }
                if (node.Attributes["IsBizKey"] != null)
                {
                    node.Attributes["IsBizKey"].Value = this.IsBizKey ? "1" : "0";
                }
                else
                {
                    XmlAttribute attr = xmlDoc.CreateAttribute("IsBizKey");
                    attr.Value = this.IsBizKey ? "1" : "0";
                    node.Attributes.Append(attr);
                }

                if (dataType != DataTypeEnum.CompositionType && this.DataType == DataTypeEnum.CompositionType)
                {
                    addPrimaryRefNode(xmlDoc, this.RefGuid, this.Guid);
                }
                this.IsChanged = false;
            }
            else if (this.DataState == DataState.Delete)
            {
                XmlNode node = xmlDoc.SelectSingleNode("EntityProj/EntityList/Entity[@Guid='" + this.Entity.Guid + "']");
                node = node.SelectSingleNode("Attributes/Attribute[@Guid='" + this.Guid + "']");
                if (node != null)
                {
                    node.ParentNode.RemoveChild(node);
                }
                //聚合需要到对应实体上生成一个引用树形
                if (this.DataType == DataTypeEnum.CompositionType)
                {
                    removePrimaryRefNode(xmlDoc, this.RefGuid, this.Guid);
                }
            }
        }

        /// <summary>
        /// 删除当前实体下面colcode的引用
        /// </summary>
        /// <param name="entityGuid"></param>
        /// <param name="colCode"></param>
        private void removePrimaryRefNode(XmlDocument xmlDoc, string entityGuid, string colGuid)
        {
            XmlNode refreceNode = xmlDoc.SelectSingleNode("EntityProj/EntityList/Entity[@Guid='" + entityGuid + "']/Attributes");
            //查找到引用的实体
            if (refreceNode != null)
            {
                XmlNode refNode = refreceNode.SelectSingleNode("Attribute[@RefColGuid='" + colGuid + "']");
                if (refNode != null)
                {
                    refNode.ParentNode.RemoveChild(refNode);
                }

                BEEntity entity = this.Entity.Proj.Get(refreceNode.ParentNode.Attributes["Guid"].Value.ToString()) as BEEntity;
                if (entity == null)
                {
                    throw new Exception("实体没有找到Guid:" + refreceNode.ParentNode.Attributes["Guid"].Value.ToString());
                }
                BEColumn col = entity.Get(refNode.Attributes["Guid"].Value.ToString());
                if (col != null)
                {
                    entity.ColumnList.Remove(col);
                }
            }
        }
        /// <summary>
        /// 新增当前实体下面colcode的引用
        /// </summary>
        /// <param name="entityGuid"></param>
        /// <param name="colGuid"></param>
        private void addPrimaryRefNode(XmlDocument xmlDoc, string entityGuid, string colGuid)
        {
            if (this.DataType == DataTypeEnum.CompositionType)
            {
                XmlNode refreceNode = xmlDoc.SelectSingleNode("EntityProj/EntityList/Entity[@Guid='" + entityGuid + "']/Attributes");
                //查找到引用的实体
                if (refreceNode != null)
                {
                    XmlNode refNode = refreceNode.SelectSingleNode("Attribute[@RefColGuid='" + colGuid + "']");
                    if (refNode == null)
                    {
                        XmlElement newEL = xmlDoc.CreateElement("Attribute");
                        newEL.SetAttribute("Guid", System.Guid.NewGuid().ToString());
                        newEL.SetAttribute("RefGuid", this.Entity.Guid);
                        newEL.SetAttribute("RefColGuid", this.Guid);
                        newEL.SetAttribute("Code", "P" + this.Entity.Code);
                        newEL.SetAttribute("Name", this.Entity.Name);
                        newEL.SetAttribute("DataType", DataTypeEnum.RefreceType.ToString());
                        newEL.SetAttribute("IsNull", "0");
                        refreceNode.AppendChild(newEL);

                        BEEntity entity = this.Entity.Proj.Get(refreceNode.ParentNode.Attributes["Guid"].Value.ToString()) as BEEntity;
                        if (entity == null)
                        {
                            throw new Exception("实体没有找到Guid:" + refreceNode.ParentNode.Attributes["Guid"].Value.ToString());
                        }
                        //在缓存中加入当前数据
                        BEColumn col = new BEColumn(newEL.Attributes["Guid"].Value.ToString(), this.Entity.Proj as BEProj, entity);
                        col.RefGuid = newEL.Attributes["RefGuid"].Value.ToString();
                        col.RefColGuid = this.Guid;
                        col.Code = newEL.Attributes["Code"].Value.ToString();
                        col.Name = newEL.Attributes["Name"].Value.ToString();
                        col.DataType = DataTypeEnum.RefreceType;
                        col.IsNull = false;
                        col.DataState = DataState.Update;
                        entity.ColumnList.Add(col);
                    }
                }
            }
        }
    }
}
