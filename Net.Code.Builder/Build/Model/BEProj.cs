﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using Net.Code.Builder.Base;
using Net.Code.Builder.Util;

namespace Net.Code.Builder.Build.Model
{
    public class BEProj : AbstractPlatformComponent, IProject
    {
        public BEProj(string projPath, string guid)
            : base(guid)
        {
            //如果为空则直接将路径指到当前metadata目录
            if (string.IsNullOrEmpty(projPath))
            {
                projPath = AppDomain.CurrentDomain.BaseDirectory + PubVariable.MetaDataPath;
            }
            this._guid = guid;
            this._projPath = projPath;
            this.DataState = DataState.Add;
            this._codePath = Path.GetDirectoryName(this._projPath) + PubVariable.CodePath;
            this._metaDataPath = Path.GetDirectoryName(this._projPath) + PubVariable.MetaDataPath;
            this._appPath = Path.GetDirectoryName(this._projPath) + PubVariable.AppPath;
            this._runtimePath = Path.GetDirectoryName(this._projPath) + PubVariable.RuntimePath;
        }

        #region AbstractPlatformComponent 成员初始化
        /// <summary>
        /// 从xml文件中初始化对象
        /// </summary>
        public override void FromXML(XmlNode node)
        {
            this.DataState = DataState.UnChanged;
            this.Namespace = node.Attributes["namespace"].Value;
            this._guid = node.Attributes["Guid"].Value;
            this.ProjName = node.Attributes["Name"].Value;
            this.DataState = DataState.Update;

            this.EntityList.Clear();
            this.FloderList.Clear();
            this.DTOList.Clear();
            this.EnumList.Clear();
            this.RefrenceList.Clear();

            XmlNodeList nodeList = node.SelectNodes("RefrenceList/Refrence");
            if (nodeList != null)
            {
                foreach (XmlNode n in nodeList)
                {
                    ProjectRefrence projRef = new ProjectRefrence(this, string.Empty, string.Empty);
                    projRef.FromXML(n);
                    this.RefrenceList.Add(projRef);
                }
            }

            nodeList = node.SelectNodes("EntityList/Entity");
            //查找当前工程文件下所有的实体
            if (nodeList != null)
            {
                foreach (XmlNode n in nodeList)
                {
                    BEEntity entity = new BEEntity(this, string.Empty, string.Empty);
                    entity.FromXML(n);
                    this.EntityList.Add(entity);
                }
            }
            this.FloderList.AddRange(this.LoadFloderList(node));
            nodeList = node.SelectNodes("DTOList/DTO");
            if (nodeList != null)
            {
                foreach (XmlNode n in nodeList)
                {
                    DTOEntity dtoEntity = new DTOEntity(this, string.Empty, string.Empty);
                    dtoEntity.FromXML(n);
                    this.DTOList.Add(dtoEntity);
                }
            }
            nodeList = node.SelectNodes("EnumList/Enum");
            if (nodeList != null)
            {
                foreach (XmlNode n in nodeList)
                {
                    EnumEntity enumEntity = new EnumEntity(this, string.Empty, string.Empty);
                    enumEntity.FromXML(n);
                    this.EnumList.Add(enumEntity);
                }
            }

            this.IsChanged = false;
        }
        /// <summary>
        /// 将对象转换成xml文件
        /// </summary>
        public override void ToXML(XmlDocument xmlDoc)
        {
            //首先保存头部记录
            if (this.DataState == DataState.Add)
            {
                this._projPath = this.MetaDataPath + this.ProjName + ".be";
                XmlElement root = xmlDoc.CreateElement("EntityProj");
                root.SetAttribute("namespace", this.Namespace);
                root.SetAttribute("Guid", this.Guid);
                root.SetAttribute("Name", this.ProjName);
                XmlElement entityListNode = xmlDoc.CreateElement("EntityList");
                root.AppendChild(entityListNode);
                XmlElement refrenceListNode = xmlDoc.CreateElement("RefrenceList");
                root.AppendChild(refrenceListNode);
                xmlDoc.AppendChild(root);

                foreach (Floder floder in this.FloderList)
                {
                    floder.ToXML(xmlDoc);
                }
                foreach (BEEntity entity in this.EntityList)
                {
                    entity.ToXML(xmlDoc);
                }
                foreach (DTOEntity entity in this.DTOList)
                {
                    entity.ToXML(xmlDoc);
                }
                foreach (EnumEntity entity in this.EnumList)
                {
                    entity.ToXML(xmlDoc);
                }
                foreach (ProjectRefrence entity in this.RefrenceList)
                {
                    entity.ToXML(xmlDoc);
                }
                this.DataState = DataState.Update;
                this.IsChanged = false;
            }
            else if (this.DataState == DataState.Update)
            {
                if (this.IsChanged)
                {
                    XmlNode node = xmlDoc.SelectSingleNode("EntityProj[@Guid='" + this.Guid + "']");
                    node.Attributes["namespace"].Value = this.Namespace;
                    this.IsChanged = false;
                }

                foreach (Floder floder in this.FloderList)
                {
                    floder.ToXML(xmlDoc);
                }
                foreach (BEEntity entity in this.EntityList)
                {
                    entity.ToXML(xmlDoc);
                }
                foreach (DTOEntity entity in this.DTOList)
                {
                    entity.ToXML(xmlDoc);
                }
                foreach (EnumEntity entity in this.EnumList)
                {
                    entity.ToXML(xmlDoc);
                }
                foreach (ProjectRefrence entity in this.RefrenceList)
                {
                    entity.ToXML(xmlDoc);
                }
                for (int i = this.EntityList.Count - 1; i >= 0; i--)
                {
                    BEEntity entity = EntityList[i] as BEEntity;
                    if (entity.DataState == DataState.Delete)
                    {
                        this.EntityList.RemoveAt(i);
                    }
                }
                for (int i = this.DTOList.Count - 1; i >= 0; i--)
                {
                    DTOEntity entity = this.DTOList[i] as DTOEntity;
                    if (entity.DataState == DataState.Delete)
                    {
                        this.DTOList.RemoveAt(i);
                    }
                }
                for (int i = this.EnumList.Count - 1; i >= 0; i--)
                {
                    EnumEntity entity = this.EnumList[i] as EnumEntity;
                    if (entity.DataState == DataState.Delete)
                    {
                        this.EnumList.RemoveAt(i);
                    }
                }
                for (int i = this.RefrenceList.Count - 1; i >= 0; i--)
                {
                    ProjectRefrence entity = this.RefrenceList[i] as ProjectRefrence;
                    if (entity.DataState == DataState.Delete)
                    {
                        this.RefrenceList.RemoveAt(i);
                    }
                }
            }
        }

        #endregion

        #region IProject接口实现

        /// <summary>
        /// 项目路径
        /// </summary>
        private string _projPath = "";
        public string ProjPath
        {
            get { return this._projPath; }
        }
        private string _metaDataPath = "";
        public string MetaDataPath
        {
            get
            {
                return _metaDataPath;
            }
        }
        private string _codePath = "";
        public string CodePath
        {
            get
            {
                return _codePath;
            }
        }
        private string _appPath = "";
        public string AppPath
        {
            get
            {
                return _appPath;
            }
        }
        private string _runtimePath = "";
        public string RuntimePath
        {
            get
            {
                return _runtimePath;
            }
        }
        /// <summary>
        /// 项目名称
        /// </summary>
        private string _projName = "";
        public string ProjName
        {
            get { return _projName; }
            set
            {
                if (_projName != value)
                {
                    _projName = value;
                    this.IsChanged = true;
                }
            }
        }

        public string FileName
        {
            get
            {
                if (string.IsNullOrEmpty(this.ProjName))
                {
                    return string.Empty;
                }
                return this.ProjName + ".be";
            }
        }
        /// <summary>
        /// 项目命名空间
        /// </summary>
        private string _namespace = "";
        public string Namespace
        {
            get { return _namespace; }
            set
            {
                if (_namespace != value)
                {
                    _namespace = value;
                    this.IsChanged = true;
                }
            }
        }

        /// <summary>
        /// 获取实体工程项目中guid为guid的实体对象
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        public AbstractPlatformComponent Get(string guid)
        {
            foreach (BEEntity entity in this._entityList)
            {
                if (entity.Guid == guid) return entity;
            }
            foreach (DTOEntity dto in this._dtoList)
            {
                if (dto.Guid == guid) return dto;
            }
            foreach (EnumEntity dto in this._enumList)
            {
                if (dto.Guid == guid) return dto;
            }
            foreach (Floder floder in this._floderList)
            {
                if (floder.Guid == guid) return floder;
            }
            foreach (ProjectRefrence pr in this._refrenceList)
            {
                if (pr.Guid == guid) return pr;
            }
            return null;
        }

        public List<AbstractPlatformComponent> Find(string key)
        {
            List<AbstractPlatformComponent> _apfcList = new List<AbstractPlatformComponent>();

            foreach (BEEntity entity in this._entityList)
            {
                if (entity.Guid.IndexOf(key) >= 0 || entity.Code.IndexOf(key) >= 0 || entity.Name.IndexOf(key) >= 0)
                {
                    _apfcList.Add(entity);
                }
            }
            foreach (EnumEntity dto in this._enumList)
            {
                if (dto.Guid.IndexOf(key) >= 0 || dto.Code.IndexOf(key) >= 0 || dto.Name.IndexOf(key) >= 0)
                {
                    _apfcList.Add(dto);
                }
            }
            foreach (Floder floder in this._floderList)
            {
                if (floder.Guid.IndexOf(key) >= 0 || floder.Name.IndexOf(key) >= 0)
                {
                    _apfcList.Add(floder);
                }
            }

            return _apfcList;
        }


        /// <summary>
        /// 从当前实体项目xml文件中获取guid的实体的attrcode属性的值
        /// 如果当前实体中部存在则从basemetadata文件夹中寻找
        /// </summary>
        /// <param name="guid"></param>
        /// <param name="attrCode"></param>
        /// <returns></returns>
        public XmlNode GetEntityNode(string guid)
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(this._projPath);
            XmlNode node = xmlDoc.FirstChild.SelectSingleNode("EntityList/Entity[@Guid='" + guid + "']");
            if (node == null)
            {
                node = xmlDoc.FirstChild.SelectSingleNode("EnumList/Enum[@Guid='" + guid + "']");
                if (node == null)
                {
                    node = xmlDoc.FirstChild.SelectSingleNode("DTOList/DTO[@Guid='" + guid + "']");
                    if (node == null)
                    {
                        foreach (ProjectRefrence pr in this.RefrenceList)
                        {
                            string fullPath = AppDomain.CurrentDomain.BaseDirectory + pr.RefFilePath;
                            if (!System.IO.File.Exists(fullPath))
                            {
                                OutPut.OutPutMsg("引用程序集" + pr.AssemblyName + "关联的元数据文件不存在，文件路径为:" + fullPath);
                                continue;
                            }
                            xmlDoc.Load(fullPath);
                            node = xmlDoc.FirstChild.SelectSingleNode("EntityList/Entity[@Guid='" + guid + "']");
                            if (node == null)
                            {
                                node = xmlDoc.FirstChild.SelectSingleNode("EnumList/Enum[@Guid='" + guid + "']");
                            }
                            if (node == null)
                            {
                                node = xmlDoc.FirstChild.SelectSingleNode("DTOList/DTO[@Guid='" + guid + "']");
                            }
                            if (node != null) return node;
                        }
                    }
                }
            }
            return node;
        }

        public List<Floder> LoadFloderList(XmlNode node)
        {
            XmlNodeList nodeList = node.SelectNodes("Floder");
            List<Floder> sourceList = new List<Floder>();
            List<Floder> resultList = new List<Floder>();
            //查找当前工程下所有的文件夹
            if (nodeList != null)
            {
                foreach (XmlNode n in nodeList)
                {
                    Floder floder = new Floder(this, string.Empty);
                    floder.FromXML(n);
                    sourceList.Add(floder);
                }
            }
            //先加载根节点的
            for (var i = sourceList.Count - 1; i >= 0; i--)
            {
                Floder floder = sourceList[i];
                if (string.IsNullOrEmpty(floder.PGuid))
                {
                    resultList.Add(floder);
                    sourceList.RemoveAt(i);
                }
            }
            while (sourceList.Count > 0)
            {

                for (int j = 0; j < resultList.Count; j++)
                {
                    for (int i = sourceList.Count - 1; i >= 0; i--)
                    {
                        if (sourceList[i].PGuid == resultList[j].Guid)
                        {
                            resultList.Add(sourceList[i]);
                            sourceList.RemoveAt(i);
                        }
                    }
                }
            }
            return resultList;
        }

        /// <summary>
        /// 项目中所包含的实体列表
        /// </summary>
        private List<AbstractPlatformComponent> _entityList = new List<AbstractPlatformComponent>();
        public List<AbstractPlatformComponent> EntityList
        {
            get { return _entityList; }
        }

        private List<AbstractPlatformComponent> _dtoList = new List<AbstractPlatformComponent>();
        public List<AbstractPlatformComponent> DTOList
        {
            get { return _dtoList; }
        }

        private List<AbstractPlatformComponent> _floderList = new List<AbstractPlatformComponent>();
        public List<AbstractPlatformComponent> FloderList
        {
            get
            {
                return _floderList;
            }
        }

        private List<AbstractPlatformComponent> _enumList = new List<AbstractPlatformComponent>();
        public List<AbstractPlatformComponent> EnumList
        {
            get { return _enumList; }
        }

        private List<AbstractPlatformComponent> _refrenceList = new List<AbstractPlatformComponent>();
        public List<AbstractPlatformComponent> RefrenceList
        {
            get
            {
                return _refrenceList;
            }
        }

        private ProjectTypeEnum _projType = ProjectTypeEnum.BEProj;
        public ProjectTypeEnum ProjType
        {
            get { return _projType; }
        }

        #endregion

        public Floder GetFloder(string guid)
        {
            foreach (Floder floder in this.FloderList)
            {
                if (floder.Guid == guid) return floder;
            }
            return null;
        }


        public static IProject CreateProject(string ns, string projName)
        {
            BEProj proj = new BEProj(string.Empty, System.Guid.NewGuid().ToString());
            proj.ProjName = projName;
            proj.Namespace = ns;
            //添加自我引用
            ProjectRefrence pr = new ProjectRefrence(proj, System.Guid.NewGuid().ToString(), string.Empty);
            pr.RefProjName = proj.ProjName + ".be";
            pr.Code = pr.Name = proj.Namespace + ".dll";
            pr.RefrenceType = Net.Code.Builder.Enums.RefType.BEEntity;
            proj.RefrenceList.Add(pr);

            pr = new ProjectRefrence(proj, System.Guid.NewGuid().ToString(), string.Empty);
            pr.RefProjName = proj.ProjName + ".be";
            pr.Code = pr.Name = proj.Namespace + ".Deploy.dll";
            pr.RefrenceType = Net.Code.Builder.Enums.RefType.Deploy;
            proj.RefrenceList.Add(pr);

            proj.ToFile();

            return proj;
        }

        public void Load()
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(this._projPath);
            this.FromXML(xmlDoc.FirstChild);
        }
        public void FromFile(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                throw new Exception("文件地址不能为空");
            }
            if (!System.IO.File.Exists(filePath))
            {
                throw new Exception("文件不存在:" + filePath);
            }

            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(filePath);
            this.FromXML(xmlDoc.FirstChild);
        }

        public void ToFile()
        {
            XmlDocument xmlDoc = new XmlDocument();
            if (this.DataState != Base.DataState.Add)
            {
                xmlDoc.Load(this._projPath);
            }
            this.ToXML(xmlDoc);
            xmlDoc.Save(this._projPath);
        }
    }
}
