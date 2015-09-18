using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using Net.Code.Builder.Base;

namespace Net.Code.Builder.Build.Model
{
    public class EnumColumn : AbstractPlatformComponent
    {
        /// <summary>
        /// 类型
        /// </summary>
        private int _enumValue;
        public int EnumValue
        {
            get { return _enumValue; }
            set
            {
                if (_enumValue != value)
                {
                    _enumValue = value;
                    this.IsChanged = true;
                }
            }
        }
        /// <summary>
        /// 所属工程
        /// </summary>
        private IProject _proj;
        public IProject Proj
        {
            get { return _proj; }
        }
        /// <summary>
        /// 所属实体
        /// </summary>
        private EnumEntity _entity;
        public EnumEntity Entity
        {
            get { return _entity; }
        }

        public EnumColumn(string guid, IProject proj, EnumEntity entity)
            : base(guid)
        {
            this._proj = proj;
            this._entity = entity;
            this.DataState = DataState.Add;
            this.IsChanged = true;
        }
        public EnumColumn(EnumEntity entity, EnumColumn col)
            : base(col.Guid)
        {
            this._proj = col.Proj;
            this._entity = entity;
            this.Code = col.Code;
            this.Name = col.Name;
            this.EnumValue = col.EnumValue;
        }


        public override void FromXML(XmlNode node)
        {
            this._guid = node.Attributes["Guid"].Value;
            this.Code = node.Attributes["Code"].Value;
            this.Name = node.Attributes["Name"].Value;
            this.EnumValue = int.Parse(node.Attributes["Value"].Value);
            this.DataState = DataState.Update;
            this.IsChanged = false;
        }

        public override void ToXML(XmlDocument xmlDoc)
        {
            if (this.DataState == DataState.Add)
            {
                XmlNode node = xmlDoc.FirstChild.SelectSingleNode("EnumList/Enum[@Guid='" + this.Entity.Guid + "']");
                node = node.SelectSingleNode("Attributes");

                XmlElement el = xmlDoc.CreateElement("Attribute");
                el.SetAttribute("Guid", this.Guid);
                el.SetAttribute("Value", this.EnumValue.ToString());
                el.SetAttribute("Code", this.Code);
                el.SetAttribute("Name", this.Name);
                node.AppendChild(el);

                this.DataState = DataState.Update;
                this.IsChanged = false;
            }
            else if (this.DataState == DataState.Update && this.IsChanged)
            {
                XmlNode node = xmlDoc.FirstChild.SelectSingleNode("EnumList/Enum[@Guid='" + this.Entity.Guid + "']");
                node = node.SelectSingleNode("Attributes/Attribute[@Guid='" + this.Guid + "']");
                if (node == null)
                {
                    throw new Exception("更新的枚举属性节点没有找到");
                }

                //获取原来的类型
                node.Attributes["Code"].Value = this.Code;
                node.Attributes["Name"].Value = this.Name;
                //如果没有引用实体则需要记录typestring
                if (node.Attributes["Value"] != null)
                {
                    node.Attributes["Value"].Value = this.EnumValue.ToString();
                }
                else
                {
                    XmlAttribute attr = xmlDoc.CreateAttribute("Type");
                    attr.Value = this.EnumValue.ToString();
                    node.Attributes.Append(attr);
                }
                this.IsChanged = false;
            }
            else if (this.DataState == DataState.Delete)
            {
                XmlNode node = xmlDoc.FirstChild.SelectSingleNode("EnumList/Enum[@Guid='" + this.Entity.Guid + "']");
                node = node.SelectSingleNode("Attributes/Attribute[@Guid='" + this.Guid + "']");
                if (node != null)
                {
                    node.ParentNode.RemoveChild(node);
                }
            }
        }
    }
}
