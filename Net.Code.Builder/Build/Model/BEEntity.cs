using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using Net.Code.Builder.Base;
using Net.Code.Builder.Build.Tpl;

namespace Net.Code.Builder.Build.Model
{
    public class BEEntity : AbstractPlatformComponent
    {
        private ViewRangeEnum _viewRange = ViewRangeEnum.NONE;
        /// <summary>
        /// 可见范围1所有组织，2上级组织可见，3当前组织可见
        /// </summary>
        public ViewRangeEnum ViewRange
        {
            get
            {
                return this._viewRange;
            }
            set
            {
                if (this._viewRange != value)
                {
                    this._viewRange = value;
                    this.IsChanged = true;
                }

            }
        }

        private bool _orgFilter = true;

        public bool OrgFilter
        {
            get { return _orgFilter; }
            set
            {
                if (this._orgFilter != value)
                {
                    this._orgFilter = value;
                    this.IsChanged = true;
                }
            }
        }
        /// <summary>
        /// 实体对应的表的名称
        /// </summary>
        private string _tableName;
        public string TableName
        {
            get
            {
                return _tableName;
            }
            set
            {
                if (_tableName != value)
                {
                    _tableName = value;
                    this.IsChanged = true;
                }
            }
        }
        /// <summary>
        /// 继承实体的guid
        /// </summary>
        private string _inhertGuid = string.Empty;
        public string InhertGuid
        {
            get
            {
                return _inhertGuid;
            }
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
        private string _inhertName = Attributes.BaseEntity;
        public string InhertName
        {
            get
            {
                return _inhertName;
            }
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
            get
            {
                return _floderGuid;
            }
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
        private List<BEColumn> _columnList = new List<BEColumn>();
        public List<BEColumn> ColumnList
        {
            get
            {
                return _columnList;
            }
        }
        /// <summary>
        /// 基类属性列表
        /// </summary>
        private List<BEColumn> _inhertColumnList = new List<BEColumn>();
        public List<BEColumn> InhertColumnList
        {
            get { return _inhertColumnList; }
        }
        /// <summary>
        /// 实体所属的项目
        /// </summary>
        private IProject _proj;
        public IProject Proj
        {
            get { return _proj; }
        }

        public BEEntity(BEProj proj, string guid, string floderGuid)
            : base(guid)
        {
            _floderGuid = floderGuid;
            _proj = proj;
            this.DataState = DataState.Add;
            this.IsChanged = true;
        }

        #region AbstractPlatformComponent 成员初始化

        /// <summary>
        /// 从xml文件中初始化对象
        /// </summary>
        /// <param name="xml"></param>
        public override void FromXML(XmlNode node)
        {
            this._guid = node.Attributes["Guid"].Value;
            this.Code = node.Attributes["Code"].Value;
            this.Name = node.Attributes["Name"].Value;
            if (string.IsNullOrEmpty(this.Code))
            {
                this.Code = "BizEntity";
            }
            if (string.IsNullOrEmpty(this.Name))
            {
                this.Name = "未命名实体";
            }
            if (node.Attributes["FloderGuid"] != null)
            {
                this.FloderGuid = node.Attributes["FloderGuid"].Value.ToString();
            }
            if (node.Attributes["Table"] == null)
            {
                this._tableName = "T_" + this.Code;
            }
            else
            {
                this._tableName = node.Attributes["Table"].Value.ToString();
            }
            if (node.Attributes["ViewRange"] == null)
            {
                this._viewRange = ViewRangeEnum.UPPER;
            }
            else
            {
                this._viewRange = (ViewRangeEnum)Convert.ToInt32((node.Attributes["ViewRange"].Value));
            }

            if (node.Attributes["OrgFilter"] == null)
            {
                this._orgFilter = true;
            }
            else
            {
                string str = node.Attributes["OrgFilter"].Value;
                if (str.Trim() == "1")
                {
                    this._orgFilter = true;
                }
                else
                {
                    this._orgFilter = false;
                }
            }
            if (!this._orgFilter)
            {
                this._viewRange = ViewRangeEnum.NONE;
            }
            if (node.Attributes["InhertGuid"] != null)
            {
                this._inhertGuid = node.Attributes["InhertGuid"].Value;
                XmlNode entityNode = this.Proj.GetEntityNode(this.InhertGuid);
                if (entityNode != null)
                {
                    string inhertNameSpace = entityNode.ParentNode.ParentNode.Attributes["namespace"].Value.ToString();
                    if (!string.IsNullOrEmpty(inhertNameSpace))
                    {
                        this._inhertName = inhertNameSpace + ".";
                    }
                    this._inhertName += entityNode.Attributes["Code"].Value.ToString();
                }
            }
            this.DataState = DataState.Update;

            //初始化基类列成员
            this._inhertColumnList.Clear();
            //如果该类是继承下来的话，需要生成基类
            if (!string.IsNullOrEmpty(this.InhertGuid))
            {
                List<XmlNode> nodeList = this.GetInhertNodes(this.InhertGuid);
                foreach (XmlNode inhertNode in nodeList)
                {
                    BEColumn col = new BEColumn(string.Empty, this.Proj as BEProj, this);
                    col.FromXML(inhertNode);
                    _inhertColumnList.Add(col);
                }
            }
            this._columnList.Clear();
            XmlNodeList xmlNodeList = node.SelectNodes("Attributes/Attribute");
            //查找当前工程文件下所有的实体
            foreach (XmlNode n in xmlNodeList)
            {
                BEColumn col = new BEColumn(string.Empty, this.Proj as BEProj, this);
                col.FromXML(n);
                this._columnList.Add(col);
            }

            this.IsChanged = false;
        }
        /// <summary>
        /// 将对象转换成xml文件
        /// </summary>
        public override void ToXML(XmlDocument xmlDoc)
        {
            if (!this._orgFilter)
            {
                this._viewRange = ViewRangeEnum.NONE;
            }
            if (this.DataState == DataState.Add)
            {
                XmlNode listNode = xmlDoc.SelectSingleNode("EntityProj/EntityList");
                if (listNode == null)
                {
                    XmlNode root = xmlDoc.SelectSingleNode("EntityProj");
                    listNode = xmlDoc.CreateElement("EntityList");
                    root.AppendChild(listNode);
                }
                XmlElement el = xmlDoc.CreateElement("Entity");
                el.SetAttribute("Guid", this.Guid);
                el.SetAttribute("Code", this.Code);
                el.SetAttribute("Name", this.Name);
                el.SetAttribute("Table", this.TableName);
                el.SetAttribute("ViewRange", ((int)this.ViewRange).ToString());
                el.SetAttribute("OrgFilter", this.OrgFilter ? "1" : "0");
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
                foreach (BEColumn col in this.ColumnList)
                {
                    col.ToXML(xmlDoc);
                }
                //从内存中删除已经删除的节点
                for (int i = ColumnList.Count - 1; i >= 0; i--)
                {
                    BEColumn col = ColumnList[i];
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
                    XmlNode node = xmlDoc.SelectSingleNode("EntityProj/EntityList/Entity[@Guid='" + this.Guid + "']");
                    node.Attributes["Code"].Value = this.Code;
                    node.Attributes["Name"].Value = this.Name;
                    if (node.Attributes["ViewRange"] == null)
                    {
                        XmlAttribute attr = xmlDoc.CreateAttribute("ViewRange");
                        attr.Value = ((int)this.ViewRange).ToString();
                        node.Attributes.Append(attr);
                    }
                    else
                    {
                        node.Attributes["ViewRange"].Value = ((int)this.ViewRange).ToString();
                    }
                    if (node.Attributes["OrgFilter"] == null)
                    {
                        XmlAttribute attr = xmlDoc.CreateAttribute("OrgFilter");
                        attr.Value = this.OrgFilter ? "1" : "0";
                        node.Attributes.Append(attr);
                    }
                    else
                    {
                        node.Attributes["OrgFilter"].Value = this.OrgFilter ? "1" : "0";
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
                    if (node.Attributes["Table"] != null)
                    {
                        node.Attributes["Table"].Value = this.TableName;
                    }
                    else
                    {
                        XmlAttribute attr = xmlDoc.CreateAttribute("Table");
                        attr.Value = this.TableName;
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
                    BEColumn col = this.ColumnList[i];
                    col.ToXML(xmlDoc);
                }

                //从内存中删除已经删除的节点
                for (int i = ColumnList.Count - 1; i >= 0; i--)
                {
                    BEColumn col = ColumnList[i];
                    if (col.DataState == DataState.Delete)
                    {
                        ColumnList.RemoveAt(i);
                    }
                }
            }
            else if (this.DataState == DataState.Delete)
            {
                XmlNode node = xmlDoc.SelectSingleNode("EntityProj/EntityList/Entity[@Guid='" + this.Guid + "']");
                if (node != null)
                {
                    node.ParentNode.RemoveChild(node);
                }
            }
        }

        #endregion

        public BEColumn Get(string guid)
        {
            foreach (BEColumn col in ColumnList)
            {
                if (col.Guid == guid) return col;
            }
            return null;
        }

        /// <summary>
        /// 获取实体所有的继承列
        /// </summary>
        /// <param name="inhertGuid"></param>
        /// <returns></returns>
        private List<XmlNode> GetInhertNodes(string inhertGuid)
        {
            List<XmlNode> resultList = new List<XmlNode>();
            XmlNode pNode = this.Proj.GetEntityNode(inhertGuid);
            if (pNode != null)
            {
                if (pNode.Attributes["InhertGuid"] != null)
                {
                    resultList.AddRange(GetInhertNodes(pNode.Attributes["InhertGuid"].Value.ToString()));
                }
                XmlNodeList nodeList = pNode.SelectNodes("Attributes/Attribute");
                if (nodeList != null && nodeList.Count > 0)
                {
                    foreach (XmlNode node in nodeList)
                    {
                        resultList.Add(node);
                    }
                }
            }
           
            return resultList;
        }

        public string GetViewRangeStr()
        {
            if (this._viewRange == ViewRangeEnum.NONE)
            {
                return "NHExt.Runtime.Enums.ViewRangeEnum.NONE";
            }
            else if (_viewRange == ViewRangeEnum.ALL)
            {
                return "NHExt.Runtime.Enums.ViewRangeEnum.ALL";
            }
            else if (_viewRange == ViewRangeEnum.UPPER)
            {
                return "NHExt.Runtime.Enums.ViewRangeEnum.UPPER";
            }
            else if (_viewRange == ViewRangeEnum.OWN)
            {
                return "NHExt.Runtime.Enums.ViewRangeEnum.OWN";
            }
            else
            {
                throw new Exception("获取取数范围错误");
            }
        }
    }
}
