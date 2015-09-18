using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using Net.Code.Builder.Base;

namespace Net.Code.Builder.Build.Model
{
    /// <summary>
    /// 分类文件夹
    /// </summary>
    public class Floder : AbstractPlatformComponent
    {
        /// <summary>
        /// 名称
        /// </summary>
        private string _name;
        public new string Name
        {
            get { return _name; }
            set
            {
                if (_name != value)
                {
                    _name = value;
                    this.IsChanged = true;
                }
            }
        }

        /// <summary>
        /// 上级guid
        /// </summary>
        private string _pGuid;
        public string PGuid
        {
            get { return _pGuid; }
            set
            {
                if (_pGuid != value)
                {
                    _pGuid = value;
                    this.IsChanged = true;
                }
            }
        }
        /// <summary>
        /// 实体所属的项目
        /// </summary>
        private IProject _proj;
        public IProject Proj
        {
            get { return _proj; }
        }

        public Floder(IProject proj, string guid)
            : base(guid)
        {
            this._proj = proj;

            this.DataState = Base.DataState.Add;
        }

        public override void FromXML(XmlNode node)
        {
            this._guid = node.Attributes["Guid"].Value.ToString();
            this.Name = node.Attributes["Name"].Value.ToString();
            if (node.Attributes["PGuid"] != null)
            {
                this.PGuid = node.Attributes["PGuid"].Value.ToString();
            }
            this.DataState = DataState.Update;
            this.IsChanged = false;
        }

        public override void ToXML(XmlDocument xmlDoc)
        {
            if (this.DataState == DataState.Add)
            {
                XmlNode root = xmlDoc.FirstChild;
                XmlElement el = xmlDoc.CreateElement("Floder");
                el.SetAttribute("Guid", this.Guid);
                el.SetAttribute("Name", this.Name);
                if (!string.IsNullOrEmpty(this.PGuid))
                {
                    el.SetAttribute("PGuid", this.PGuid);
                }
                root.AppendChild(el);
                this.DataState = DataState.Update;
                this.IsChanged = false;
            }
            else if (this.DataState == DataState.Update)
            {
                if (this.IsChanged)
                {
                    XmlNode node = xmlDoc.FirstChild.SelectSingleNode("Floder[@Guid='" + this.Guid + "']");
                    node.Attributes["Name"].Value = this.Name;
                    if (!string.IsNullOrEmpty(this.PGuid))
                    {
                        if (node.Attributes["PGuid"] != null)
                        {
                            node.Attributes["PGuid"].Value = this.PGuid;
                        }
                        else
                        {
                            XmlAttribute attr = xmlDoc.CreateAttribute("PGuid");
                            attr.Value = this.PGuid;
                            node.Attributes.Append(attr);
                        }
                    }
                    else
                    {
                        if (node.Attributes["PGuid"] != null)
                        {
                            node.Attributes.RemoveNamedItem("PGuid");
                        }
                    }
                    this.IsChanged = false;
                }
            }
            else if (this.DataState == DataState.Delete)
            {
                XmlNode node = xmlDoc.FirstChild.SelectSingleNode("Floder[@Guid='" + this.Guid + "']");
                if (node != null)
                {
                    node.ParentNode.RemoveChild(node);
                }
            }
        }
        //生成文件夹元数据
        public string BuildMetaData() {
            string sql = string.Empty;
            sql += "DELETE FROM T_METADATA_BP_COMPONENT WHERE F_GUID='" + this.Guid + "'" + Environment.NewLine;
            sql += "INSERT INTO T_METADATA_BP_COMPONENT VALUES((SELECT ISNULL(MAX(F_ID),0)+1 FROM T_METADATA_BP_COMPONENT),0,'" + this.Guid + "','" + this.Name + "','" + this.Name + "','" + this.Proj.Namespace + "." + this.Name + "','" + this.Proj.Namespace + ".dll" + "','" + this.Proj.Guid + "',0,'" + this.PGuid + "',1)" + Environment.NewLine;
            return sql;
        }
    }
}
