using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using Net.Code.Builder.Base;

namespace Net.Code.Builder.Build.Model
{
    public class EnumEntity : AbstractPlatformComponent
    {
        /// <summary>
        /// 实体所属于的分类文件夹guid
        /// </summary>
        private string _floderGuid = string.Empty;
        public string FloderGuid
        {
            get { return _floderGuid; }
            set
            {
                if (_floderGuid != value)
                {
                    _floderGuid = value;
                    this.IsChanged = true;
                }
            }
        }

        /// <summary>
        /// 实体属性列表
        /// </summary>
        private List<EnumColumn> _columnList = new List<EnumColumn>();
        public List<EnumColumn> ColumnList
        {
            get { return _columnList; }
        }

        /// <summary>
        /// 实体所属的项目
        /// </summary>
        private IProject _proj;
        public IProject Proj
        {
            get { return _proj; }
        }

        public EnumEntity(IProject proj, string guid, string floderGuid)
            : base(guid)
        {
            this._proj = proj;
            this._floderGuid = floderGuid;
            this.DataState = DataState.Add;
            this.IsChanged = true;
        }

        public EnumEntity(EnumEntity entity)
            : base(entity.Guid)
        {
            this.Code = entity.Code;
            this.Name = entity.Name;

            this._proj = entity.Proj;
            this._columnList.Clear();
            foreach (EnumColumn col in entity.ColumnList)
            {
                EnumColumn enumCol = new EnumColumn(this, col);
                this._columnList.Add(enumCol);
            }
        }

        public override void FromXML(XmlNode node)
        {
            this._guid = node.Attributes["Guid"].Value;
            this.Code = node.Attributes["Code"].Value;
            if (string.IsNullOrEmpty(this.Code))
            {
                this.Code = "BizEnum";
            }
            this.Name = node.Attributes["Name"].Value;
            if (string.IsNullOrEmpty(this.Name))
            {
                this.Name = "未命名枚举";
            }
            if (node.Attributes["FloderGuid"] != null)
            {
                this.FloderGuid = node.Attributes["FloderGuid"].Value.ToString();
            }
            this.DataState = DataState.Update;
            this.ColumnList.Clear();
            XmlNodeList nodeList = node.SelectNodes("Attributes/Attribute");
            //查找当前工程文件下所有的实体
            foreach (XmlNode n in nodeList)
            {
                EnumColumn col = new EnumColumn(string.Empty, this.Proj, this);
                col.FromXML(n);
                this.ColumnList.Add(col);
            }
            this.IsChanged = false;
        }

        public override void ToXML(XmlDocument xmlDoc)
        {
            if (this.DataState == DataState.Add)
            {
                XmlNode listNode = xmlDoc.FirstChild.SelectSingleNode("EnumList");
                if (listNode == null)
                {
                    XmlNode root = xmlDoc.FirstChild;
                    listNode = xmlDoc.CreateElement("EnumList");
                    root.AppendChild(listNode);
                }
                XmlElement el = xmlDoc.CreateElement("Enum");
                el.SetAttribute("Guid", this.Guid);
                el.SetAttribute("Code", this.Code);
                el.SetAttribute("Name", this.Name);
                if (!string.IsNullOrEmpty(this.FloderGuid))
                {
                    el.SetAttribute("FloderGuid", this.FloderGuid);
                }
                XmlElement el2 = xmlDoc.CreateElement("Attributes");
                el.AppendChild(el2);
                listNode.AppendChild(el);
                this.IsChanged = false;
                foreach (EnumColumn col in this.ColumnList)
                {
                    col.ToXML(xmlDoc);
                }
                //从内存中删除已经删除的节点
                for (int i = ColumnList.Count - 1; i >= 0; i--)
                {
                    EnumColumn col = ColumnList[i];
                    if (col.DataState == DataState.Delete)
                    {
                        ColumnList.RemoveAt(i);
                    }
                }
                this.DataState = DataState.Update;
                this.IsChanged = false;
            }
            else if (this.DataState == DataState.Update)
            {
                if (this.IsChanged)
                {
                    XmlNode node = xmlDoc.FirstChild.SelectSingleNode("EnumList/Enum[@Guid='" + this.Guid + "']");
                    node.Attributes["Code"].Value = this.Code;
                    node.Attributes["Name"].Value = this.Name;
                    if (string.IsNullOrEmpty(this.FloderGuid))
                    {
                        if (node.Attributes["FloderGuid"] != null)
                        {
                            node.Attributes.RemoveNamedItem("FloderGuid");
                        }
                    }
                    else
                    {
                        XmlAttribute attr = xmlDoc.CreateAttribute("FloderGuid");
                        attr.Value = this.FloderGuid;
                        node.Attributes.Append(attr);
                    }
                    this.IsChanged = false;
                }
                int columnCount = this.ColumnList.Count;
                for (int i = 0; i < columnCount; i++)
                {
                    if (i == this.ColumnList.Count) break;
                    EnumColumn col = this.ColumnList[i];
                    col.ToXML(xmlDoc);
                }

                //从内存中删除已经删除的节点
                for (int i = ColumnList.Count - 1; i >= 0; i--)
                {
                    EnumColumn col = ColumnList[i];
                    if (col.DataState == DataState.Delete)
                    {
                        ColumnList.RemoveAt(i);
                    }
                }
            }
            else if (this.DataState == DataState.Delete)
            {
                XmlNode node = xmlDoc.FirstChild.SelectSingleNode("EnumList/Enum[@Guid='" + this.Guid + "']");
                if (node != null)
                {
                    node.ParentNode.RemoveChild(node);
                }
            }
        }

        public EnumColumn Get(string guid)
        {
            foreach (EnumColumn col in ColumnList)
            {
                if (col.Guid == guid) return col;
            }
            return null;
        }
    }
}
