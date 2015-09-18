using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using Net.Code.Builder.Base;
using Net.Code.Builder.Util;
using Net.Code.Builder.Enums;

namespace Net.Code.Builder.Build.Model
{
    public class ProjectRefrence : AbstractPlatformComponent
    {
        private string _refProjName = string.Empty;
        public string RefProjName
        {
            get { return _refProjName; }
            set
            {
                if (_refProjName != value)
                {
                    _refProjName = value;
                    this.IsChanged = true;
                }
            }
        }

        private RefType _refrenceType = RefType.None;
        public RefType RefrenceType
        {
            get { return _refrenceType; }
            set
            {
                if (_refrenceType != value)
                {
                    _refrenceType = value;
                    this.IsChanged = true;
                }
            }
        }

        private string _refFilePath = string.Empty;
        public string RefFilePath
        {
            get
            {
                return this._refFilePath;
            }
            set
            {
                this._refFilePath = value;
                this.IsChanged = true;
            }
        }

        private string _assemblyName;
        /// <summary>
        /// 程序集名称
        /// </summary>
        public string AssemblyName
        {
            get { return _assemblyName; }
        }
        /// <summary>
        /// 实体所属的项目
        /// </summary>
        private IProject _proj;
        public IProject Proj
        {
            get { return _proj; }
        }

        public ProjectRefrence(IProject proj, string guid, string refPath)
            : base(guid)
        {
            this._proj = proj;
            this.DataState = Base.DataState.Add;
            if (!string.IsNullOrEmpty(refPath))
            {
                this._refFilePath = refPath;
            }
        }

        public override void FromXML(XmlNode node)
        {
            this._guid = node.Attributes["Guid"].Value;
            this._refProjName = node.Attributes["RefProjName"].Value;
            this._refrenceType = (RefType)Enum.Parse(typeof(RefType), node.Attributes["RefrenceType"].Value);
            if (node.Attributes["RefFilePath"] != null)
            {
                this._refFilePath = node.Attributes["RefFilePath"].Value;
            }
            if (string.IsNullOrEmpty(this._refFilePath))
            {
                this._refFilePath = "..\\MetaData\\" + this.RefProjName;
            }
            string fullPath = AppDomain.CurrentDomain.BaseDirectory + this._refFilePath;
            if (File.Exists(fullPath))
            {
                XmlDocument xmlReadDoc = new XmlDocument();
                xmlReadDoc.Load(fullPath);
                string ns = xmlReadDoc.FirstChild.Attributes["namespace"].Value;
                SetDisplayName(ns);
            }
            else
            {
                Exception ex = new Exception("程序集引用元数据文件“" + this._refFilePath + "”不存在");
                OutPut.OutPutMsg(ex.Message);
            }
            this.DataState = Base.DataState.Update;
        }
        public override void ToXML(XmlDocument xmlDoc)
        {
            if (this.DataState == DataState.Add)
            {
                XmlNode rootNode = xmlDoc.FirstChild;
                XmlNode node = rootNode.SelectSingleNode("RefrenceList");
                if (node == null)
                {
                    node = xmlDoc.CreateElement("RefrenceList");
                    rootNode.AppendChild(node);
                }
                XmlElement el = xmlDoc.CreateElement("Refrence");
                el.SetAttribute("Guid", this._guid);
                el.SetAttribute("RefProjName", this._refProjName);
                el.SetAttribute("RefrenceType", this._refrenceType.ToString());
                el.SetAttribute("RefFilePath", this._refFilePath);
                node.AppendChild(el);
                this.DataState = DataState.Update;
                this.IsChanged = false;
            }
            else if (this.DataState == Base.DataState.Delete)
            {
                XmlNode node = xmlDoc.FirstChild;
                node = node.SelectSingleNode("RefrenceList/Refrence[@Guid='" + this.Guid + "']");
                if (node != null)
                {
                    node.ParentNode.RemoveChild(node);
                }
            }
        }
        private void SetDisplayName(string ns)
        {
            if (this._refrenceType == RefType.Deploy)
            {
                this._assemblyName = ns + ".Deploy";
            }
            else if (this._refrenceType == RefType.Agent)
            {
                this._assemblyName = ns + ".Agent";
            }
            else if (this._refrenceType == RefType.BEEntity || this._refrenceType == RefType.BPEntity)
            {
                this._assemblyName = ns;
            }
            this.Code = this.Name = this.AssemblyName + ".dll";
        }


    }
}
