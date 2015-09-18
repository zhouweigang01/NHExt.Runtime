using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using Net.Code.Builder.Base;
using Net.Code.Builder.Build.Tpl;

namespace Net.Code.Builder.Build.Model
{
    public class BPEntity : AbstractPlatformComponent
    {
        public static readonly string AttrEndTag = "Proxy";
        public static readonly string AssemblyEndTag = "Agent";
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
        /// 返回值类型GUID
        /// </summary>
        private string _returnGuid = string.Empty;
        public string ReturnGuid
        {
            get { return _returnGuid; }
            set
            {
                if (_returnGuid != value)
                {
                    _returnGuid = value;
                    this.IsChanged = true;
                }
            }
        }
        /// <summary>
        /// 实际返回值名称
        /// </summary>
        private string _returnName = string.Empty;
        public string ReturnName
        {
            get { return _returnName; }
            set
            {
                if (_returnName != value)
                {
                    _returnName = value;
                    this.IsChanged = true;
                }
            }
        }

        /// <summary>
        /// 代理返回值字符串
        /// </summary>
        private string _returnProxyName = string.Empty;
        public string ReturnProxyName
        {
            get { return _returnProxyName; }
            set
            {
                if (_returnProxyName != value)
                {
                    _returnProxyName = value;
                    this.IsChanged = true;
                }
            }
        }

        /// <summary>
        /// 当前返回值是否实体
        /// </summary>
        private bool _isEntity = false;
        public bool IsEntity
        {
            get { return _isEntity; }
            set
            {
                if (_isEntity != value)
                {
                    _isEntity = value;
                    this.IsChanged = true;
                }
            }
        }
        /// <summary>
        /// 返回值类型是否列表
        /// </summary>
        private bool _isList = false;
        public bool IsList
        {
            get { return _isList; }
            set
            {
                if (_isList != value)
                {
                    _isList = value;
                    this.IsChanged = true;
                }
            }
        }
        
        private bool _isAuth = true;
        public bool IsAuth
        {
            get { return _isAuth; }
            set
            {
                if (_isAuth != value)
                {
                    _isAuth = value;
                    this.IsChanged = true;
                }
            }
        }
        
        private int _trans = 1;
        public int Trans
        {
            get { return _trans; }
            set
            {
                if (_trans != value)
                {
                    _trans = value;
                    this.IsChanged = true;
                }
            }
        }

        /// <summary>
        /// 实体属性列表
        /// </summary>
        private List<BPColumn> _columnList = new List<BPColumn>();
        public List<BPColumn> ColumnList
        {
            get { return _columnList; }
        }

        /// <summary>
        /// 实体所属的项目
        /// </summary>
        private BPProj _proj;
        public BPProj Proj
        {
            get { return _proj; }
        }

        public BPEntity(BPProj proj, string guid, string floderGuid)
            : base(guid)
        {
            this._proj = proj;
            this._floderGuid = floderGuid;
            this.DataState = DataState.Add;
            this.IsChanged = true;
        }
        public override void FromXML(XmlNode node)
        { 
            this._guid = node.Attributes["Guid"].Value;
            this.Code = node.Attributes["Code"].Value;
            if (string.IsNullOrEmpty(this.Code))
            {
                this.Code = "BizBP";
            }
            this.Name = node.Attributes["Name"].Value;
            if (string.IsNullOrEmpty(this.Name))
            {
                this.Name = "未命名BP";
            }
            if (node.Attributes["FloderGuid"] != null)
            {
                this.FloderGuid = node.Attributes["FloderGuid"].Value.ToString();
            }
            else {
                this.FloderGuid = string.Empty;
            }
            if (node.Attributes["List"] != null)
            {
                this.IsList = node.Attributes["List"].Value.ToString() == "1" ? true : false;
            }
            if (node.Attributes["Auth"] != null) {
                this.IsAuth = node.Attributes["Auth"].Value.ToString() == "1" ? true : false;
            }
 
            if (node.Attributes["Trans"] != null) {
                this.Trans = Convert.ToInt32(node.Attributes["Trans"].Value);
            }
            if (node.Attributes["IsEntity"] != null)
            {
                this.IsEntity = node.Attributes["IsEntity"].Value.ToString() == "1" ? true : false;
            }
            if (node.Attributes["ReturnGuid"] != null)
            {
                this.ReturnGuid = node.Attributes["ReturnGuid"].Value;
                XmlNode entityNode = this.Proj.GetEntityNode(this.ReturnGuid);
                if (entityNode != null)
                {
                    string inhertNameSpace = entityNode.ParentNode.ParentNode.Attributes["namespace"].Value.ToString();
                    if (!string.IsNullOrEmpty(inhertNameSpace))
                    {
                        this.ReturnName = inhertNameSpace + ".";
                        this.ReturnProxyName = inhertNameSpace + "." + DTOEntity.AssemblyEndTag + ".";
                    }

                    if (entityNode.Name == "Entity")
                    {
                        this.ReturnName += entityNode.Attributes["Code"].Value.ToString();
                        this.ReturnProxyName += entityNode.Attributes["Code"].Value.ToString() + DTOEntity.AttrEndTag;
                    }
                    else
                    {
                        this.ReturnName += entityNode.Attributes["Code"].Value.ToString();
                        this.ReturnProxyName += entityNode.Attributes["Code"].Value.ToString();
                    }
                }
            }
            else if (node.Attributes["ReturnName"] != null)
            {
                this.ReturnName = node.Attributes["ReturnName"].Value.ToString();
                this.ReturnProxyName = this.ReturnName;
            }
            else
            {
                this.ReturnName = this.ReturnProxyName = "void";
            }
            //如果返回值不是实体的话则BP返回值和proxy返回值是一样的
            if (!IsEntity)
            {
                this.ReturnName = this.ReturnProxyName;
            }
            this.DataState = DataState.Update;
            this.ColumnList.Clear();
            XmlNodeList nodeList = node.SelectNodes("Attributes/Attribute");
            //查找当前工程文件下所有的实体
            foreach (XmlNode n in nodeList)
            {
                BPColumn col = new BPColumn(string.Empty, this.Proj, this);
                col.FromXML(n);
                this.ColumnList.Add(col);
            }
            this.IsChanged = false;
        }

        public override void ToXML(XmlDocument xmlDoc)
        {
            if (this.DataState == DataState.Add)
            {
                XmlNode listNode = xmlDoc.SelectSingleNode("BPProj/BPList");
                if (listNode == null)
                {
                    XmlNode root = xmlDoc.SelectSingleNode("BPProj");
                    listNode = xmlDoc.CreateElement("BPList");
                    root.AppendChild(listNode);
                }
                XmlElement el = xmlDoc.CreateElement("BP");
                el.SetAttribute("Guid", this.Guid);
                el.SetAttribute("Code", this.Code);
                el.SetAttribute("Name", this.Name);
                el.SetAttribute("List", this.IsList ? "1" : "0");
                el.SetAttribute("Auth", this.IsAuth ? "1" : "0");
                el.SetAttribute("Trans", this.Trans.ToString());
                if (!string.IsNullOrEmpty(this.ReturnGuid))
                {
                    el.SetAttribute("ReturnGuid", this.ReturnGuid);
                    el.SetAttribute("IsEntity", this.IsEntity ? "1" : "0");
                }
                else
                {
                    el.SetAttribute("ReturnName", this.ReturnName);
                }
                if (!string.IsNullOrEmpty(this.FloderGuid))
                {
                    el.SetAttribute("FloderGuid", this.FloderGuid);
                }
                XmlElement el2 = xmlDoc.CreateElement("Attributes");
                el.AppendChild(el2);
                listNode.AppendChild(el);
                this.IsChanged = false;
                foreach (BPColumn col in this.ColumnList)
                {
                    col.ToXML(xmlDoc);
                }
                //从内存中删除已经删除的节点
                for (int i = ColumnList.Count - 1; i >= 0; i--)
                {
                    BPColumn col = ColumnList[i];
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
                    XmlNode node = xmlDoc.SelectSingleNode("BPProj/BPList/BP[@Guid='" + this.Guid + "']");
                    node.Attributes["Code"].Value = this.Code;
                    node.Attributes["Name"].Value = this.Name;
                    if (node.Attributes["List"] != null)
                    {
                        node.Attributes["List"].Value = this.IsList ? "1" : "0";
                    }
                    else
                    {
                        XmlAttribute attr = xmlDoc.CreateAttribute("List");
                        attr.Value = this.IsList ? "1" : "0";
                        node.Attributes.Append(attr);
                    }
                    if (node.Attributes["Auth"] != null)
                    {
                        node.Attributes["Auth"].Value = this.IsAuth ? "1" : "0";
                    }
                    else
                    {
                        XmlAttribute attr = xmlDoc.CreateAttribute("Auth");
                        attr.Value = this.IsAuth ? "1" : "0";
                        node.Attributes.Append(attr);
                    }
                    
                    if (node.Attributes["Trans"] != null)
                    {
                        node.Attributes["Trans"].Value = this.Trans.ToString();
                    }
                    else
                    {
                        XmlAttribute attr = xmlDoc.CreateAttribute("Trans");
                        attr.Value = this.Trans.ToString();
                        node.Attributes.Append(attr);
                    }
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

                    if (string.IsNullOrEmpty(this.ReturnGuid))
                    {
                        node.Attributes.RemoveNamedItem("ReturnGuid");
                        if (string.IsNullOrEmpty(this.ReturnName))
                        {
                            this.ReturnName = "void";
                        }
                        if (node.Attributes["ReturnName"] != null)
                        {
                            node.Attributes["ReturnName"].Value = this.ReturnName;
                        }
                        else
                        {
                            XmlAttribute attr = xmlDoc.CreateAttribute("ReturnName");
                            attr.Value = this.ReturnName;
                            node.Attributes.Append(attr);
                        }
                        node.Attributes.RemoveNamedItem("IsEntity");
                    }
                    else
                    {
                        node.Attributes.RemoveNamedItem("ReturnName");
                        if (node.Attributes["ReturnGuid"] != null)
                        {
                            node.Attributes["ReturnGuid"].Value = this.ReturnGuid;
                        }
                        else
                        {
                            XmlAttribute attr = xmlDoc.CreateAttribute("ReturnGuid");
                            attr.Value = this.ReturnGuid;
                            node.Attributes.Append(attr);
                        }
                        if (node.Attributes["IsEntity"] != null)
                        {
                            node.Attributes["IsEntity"].Value = this.IsEntity ? "1" : "0";
                        }
                        else
                        {
                            XmlAttribute attr = xmlDoc.CreateAttribute("IsEntity");
                            attr.Value = this.IsEntity ? "1" : "0";
                            node.Attributes.Append(attr);
                        }
                    }
                    this.IsChanged = false;
                }
                int columnCount = this.ColumnList.Count;
                for (int i = 0; i < columnCount; i++)
                {
                    if (i == this.ColumnList.Count) break;
                    BPColumn col = this.ColumnList[i];
                    col.ToXML(xmlDoc);
                }

                //从内存中删除已经删除的节点
                for (int i = ColumnList.Count - 1; i >= 0; i--)
                {
                    BPColumn col = ColumnList[i];
                    if (col.DataState == DataState.Delete)
                    {
                        ColumnList.RemoveAt(i);
                    }
                }
            }
            else if (this.DataState == DataState.Delete)
            {
                XmlNode node = xmlDoc.SelectSingleNode("BPProj/BPList/BP[@Guid='" + this.Guid + "']");
                if (node != null)
                {
                    node.ParentNode.RemoveChild(node);
                }
            }
        }

        public BPColumn Get(string guid)
        {
            foreach (BPColumn col in ColumnList)
            {
                if (col.Guid == guid) return col;
            }
            return null;
        }
    }
}
