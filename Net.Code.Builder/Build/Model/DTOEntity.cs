using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using Net.Code.Builder.Base;
using Net.Code.Builder.Build.Tpl;

namespace Net.Code.Builder.Build.Model
{
    public class DTOEntity : AbstractPlatformComponent
    {
        public static string AttrEndTag = "DTO";
        public static string AssemblyEndTag = "Deploy";

        private string _inhertGuid = string.Empty;
        public string InhertGuid
        {
            get { return _inhertGuid; }
            set
            {
                if (_inhertGuid != value)
                {
                    _inhertGuid = value;
                    this.IsChanged = true;
                }
            }
        }
        /// <summary>
        /// 继承类的全名称
        /// </summary>
        private string _inhertName = Attributes.BaseDTO;
        public string InhertName
        {
            get { return _inhertName; }
            set
            {
                if (_inhertName != value)
                {
                    _inhertName = value;
                    this.IsChanged = true;
                }
            }
        }
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
        private List<DTOColumn> _columnList = new List<DTOColumn>();
        public List<DTOColumn> ColumnList
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

        public DTOEntity(IProject proj, string guid, string floderGuid)
            : base(guid)
        {
            this._proj = proj;
            this._floderGuid = floderGuid;
            this.DataState = DataState.Add;
            this.IsChanged = true;
        }

        public DTOEntity(BEEntity entity)
            : base(entity.Guid)
        {
            this.Code = entity.Code + AttrEndTag;
            this.Name = entity.Name;
            if (entity.InhertName == Attributes.BaseEntity)
            {
                this.InhertName = Attributes.BaseDTO;
            }
            else
            {
                this.InhertName = string.Empty;
                //如果实体继承一个类的话我们生成的dto也需要继承这个类生成的dto
                string[] namespaceArray = entity.InhertName.Split('.');
                int length = namespaceArray.Length;
                for (int i = 0; i < length - 2; i++)
                {
                    this.InhertName += namespaceArray[i] + ".";
                }
                this.InhertName += namespaceArray[length - 2] + "." + DTOEntity.AssemblyEndTag + ".";
                this.InhertName += namespaceArray[length - 1] + DTOEntity.AttrEndTag;
                //this.InhertName = entity.Proj.Namespace + "." + DTOEntity.AssemblyEndTag + "." + entity.Code + DTOEntity.AttrEndTag;
            }
            this.InhertGuid = entity.InhertGuid;
            this._proj = entity.Proj;
            this._columnList.Clear();
            foreach (BEColumn col in entity.ColumnList)
            {
                DTOColumn dtoCol = new DTOColumn(this, col);
                this._columnList.Add(dtoCol);
            }
        }

        public override void FromXML(XmlNode node)
        {
            this._guid = node.Attributes["Guid"].Value;
            this.Code = node.Attributes["Code"].Value;
            if (string.IsNullOrEmpty(this.Code))
            {
                this.Code = "BizDTO";
            }
            this.Name = node.Attributes["Name"].Value;
            if (string.IsNullOrEmpty(this.Name))
            {
                this.Name = "未命名DTO";
            }
            if (node.Attributes["FloderGuid"] != null)
            {
                this.FloderGuid = node.Attributes["FloderGuid"].Value.ToString();
            }
            if (node.Attributes["InhertGuid"] != null)
            {
                this.InhertGuid = node.Attributes["InhertGuid"].Value;
                XmlNode entityNode = this.Proj.GetEntityNode(this.InhertGuid);
                if (entityNode != null)
                {
                    string inhertNameSpace = entityNode.ParentNode.ParentNode.Attributes["namespace"].Value.ToString();
                    if (!string.IsNullOrEmpty(inhertNameSpace))
                    {
                        this.InhertName = inhertNameSpace + "." + DTOEntity.AssemblyEndTag + ".";
                    }
                    if (entityNode.Name == "Entity")
                    {
                        this.InhertName += entityNode.Attributes["Code"].Value.ToString() + DTOEntity.AttrEndTag;
                    }
                    else
                    {
                        this.InhertName += entityNode.Attributes["Code"].Value.ToString();
                    }
                }
            }
            this.DataState = DataState.Update;
            this.ColumnList.Clear();
            XmlNodeList nodeList = node.SelectNodes("Attributes/Attribute");
            //查找当前工程文件下所有的实体
            foreach (XmlNode n in nodeList)
            {
                DTOColumn col = new DTOColumn(string.Empty, this.Proj, this);
                col.FromXML(n);
                this.ColumnList.Add(col);
            }
            this.IsChanged = false;
        }

        public override void ToXML(XmlDocument xmlDoc)
        {
            if (this.DataState == DataState.Add)
            {
                XmlNode listNode = xmlDoc.FirstChild.SelectSingleNode("DTOList");
                if (listNode == null)
                {
                    XmlNode root = xmlDoc.FirstChild;
                    listNode = xmlDoc.CreateElement("DTOList");
                    root.AppendChild(listNode);
                }
                XmlElement el = xmlDoc.CreateElement("DTO");
                el.SetAttribute("Guid", this.Guid);
                el.SetAttribute("Code", this.Code);
                el.SetAttribute("Name", this.Name);
                if (!string.IsNullOrEmpty(this.InhertGuid))
                {
                    el.SetAttribute("InhertGuid", this.InhertGuid);
                }
                if (!string.IsNullOrEmpty(this.FloderGuid))
                {
                    el.SetAttribute("FloderGuid", this.FloderGuid);
                }
                XmlElement el2 = xmlDoc.CreateElement("Attributes");
                el.AppendChild(el2);
                listNode.AppendChild(el);
 
                this.IsChanged = false;
                foreach (DTOColumn col in this.ColumnList)
                {
                    col.ToXML(xmlDoc);
                }
                //从内存中删除已经删除的节点
                for (int i = ColumnList.Count - 1; i >= 0; i--)
                {
                    DTOColumn col = ColumnList[i];
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
                    XmlNode node = xmlDoc.FirstChild.SelectSingleNode("DTOList/DTO[@Guid='" + this.Guid + "']");
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

                    if (string.IsNullOrEmpty(this.InhertGuid))
                    {
                        node.Attributes.RemoveNamedItem("InhertGuid");
                    }
                    else
                    {
                        if (node.Attributes["InhertGuid"] != null)
                        {
                            node.Attributes["InhertGuid"].Value = this.InhertGuid;
                        }
                        else
                        {
                            XmlAttribute attr = xmlDoc.CreateAttribute("InhertGuid");
                            attr.Value = this.InhertGuid;
                            node.Attributes.Append(attr);
                        }
                    }
                    this.IsChanged = false;
                }
                int columnCount = this.ColumnList.Count;
                for (int i = 0; i < columnCount; i++)
                {
                    if (i == this.ColumnList.Count) break;
                    DTOColumn col = this.ColumnList[i];
                    col.ToXML(xmlDoc);
                }

                //从内存中删除已经删除的节点
                for (int i = ColumnList.Count - 1; i >= 0; i--)
                {
                    DTOColumn col = ColumnList[i];
                    if (col.DataState == DataState.Delete)
                    {
                        ColumnList.RemoveAt(i);
                    }
                }
            }
            else if (this.DataState == DataState.Delete)
            {
                XmlNode node = xmlDoc.FirstChild.SelectSingleNode("DTOList/DTO[@Guid='" + this.Guid + "']");
                if (node != null)
                {
                    node.ParentNode.RemoveChild(node);
                }
            }
        }

        public DTOColumn Get(string guid)
        {
            foreach (DTOColumn col in ColumnList)
            {
                if (col.Guid == guid) return col;
            }
            return null;
        }
    }
}
