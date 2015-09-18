using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using Net.Code.Builder.Base;
using Net.Code.Builder.Build.Model;
using Net.Code.Builder.Util;
using Net.Code.Builder.Enums;

namespace Net.Code.Generator.Win
{
    public enum OperateTypeEnum
    {
        Common,//普通类型
        Refrence,//引用
        Composition,//聚合
        Inhert,//继承
        Return//返回值
    }

    public partial class SelectAttributeType : Form
    {
        private string _projPath = string.Empty;
        private OperateTypeEnum _operateType;
        private bool _isEntity;
        private List<SelectDTO> _selectTypeList = new List<SelectDTO>();
        List<ProjectRefrence> _refDlls = new List<ProjectRefrence>();
        private SelectDTO _selectedDTO;

        public SelectDTO SelectedType
        {
            get { return _selectedDTO; }
            set { _selectedDTO = value; }
        }
        public SelectAttributeType(OperateTypeEnum operateType, List<AbstractPlatformComponent> refDlls, bool isEntity)
        {
            InitializeComponent();
            _operateType = operateType;
            _isEntity = isEntity;
            if (_operateType != OperateTypeEnum.Common)
            {
                if (refDlls != null && refDlls.Count > 0)
                {
                    foreach (ProjectRefrence pr in refDlls)
                    {
                        //删除的dll不能进行选择
                        if (pr.DataState == DataState.Delete) continue;
                        if (_operateType == OperateTypeEnum.Return)
                        {
                            _refDlls.Add(pr);
                        }
                        //引用实体,聚合实体，继承实体，需要加入所有的实体
                        else if (_operateType == OperateTypeEnum.Refrence)
                        {
                            if (_isEntity)
                            {
                                if (pr.RefrenceType == RefType.BEEntity)
                                {
                                    _refDlls.Add(pr);
                                }
                            }
                            else
                            {
                                if (pr.RefrenceType == RefType.Deploy)
                                {
                                    _refDlls.Add(pr);
                                }
                            }
                        }
                        else if (_operateType == OperateTypeEnum.Composition)
                        {
                            if (_isEntity)
                            {
                                if (pr.RefrenceType == RefType.BEEntity)
                                {
                                    _refDlls.Add(pr);
                                }
                            }
                            else
                            {
                                if (pr.RefrenceType == RefType.Deploy)
                                {
                                    _refDlls.Add(pr);
                                }
                            }
                        }
                        else if (_operateType == OperateTypeEnum.Inhert)
                        {
                            if (_isEntity)
                            {
                                if (pr.RefrenceType == RefType.BEEntity)
                                {
                                    _refDlls.Add(pr);
                                }
                            }
                            else
                            {
                                if (pr.RefrenceType == RefType.Deploy)
                                {
                                    _refDlls.Add(pr);
                                }
                            }
                        }
                    }
                }
            }
        }


        /// <summary>
        /// 双击选中类型
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void lv_TypeList_DoubleClick(object sender, EventArgs e)
        {
            int index = this.lv_TypeList.SelectedIndices[0];
            this._selectedDTO = _selectTypeList[index];
            this._selectedDTO.IsList = this.ck_List.Checked;
            this.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.Close();
        }
        /// <summary>
        /// 加载数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SelectAttributeType_Load(object sender, EventArgs e)
        {
            if (_operateType == OperateTypeEnum.Return)
            {
                this.ck_List.Visible = true;
            }
            if (_operateType == OperateTypeEnum.Common)
            {
                this.cb_metadataFile.Items.Add("基础数据类型");
                this.cb_metadataFile.Enabled = false;
            }
            else
            {
                //如果不是实体引用并且当前选择的不是引用类型的话则应该能看到基础数据类型
                if (_operateType != OperateTypeEnum.Inhert)
                {
                    if (!_isEntity && _operateType != OperateTypeEnum.Refrence)
                    {
                        this.cb_metadataFile.Items.Add("基础数据类型");
                    }
                }
                foreach (ProjectRefrence pr in this._refDlls)
                {
                    this.cb_metadataFile.Items.Add(pr.Name);
                }
            }
            if (this.cb_metadataFile.Items.Count > 0)
            {
                this.cb_metadataFile.SelectedIndex = 0;
            }
        }


        private void cb_metadataFile_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.lv_TypeList.Items.Clear();
            //清空列表信息
            _selectTypeList.Clear();
            if (_operateType == OperateTypeEnum.Common)
            {
                AddCommonRecord();
            }
            else
            {
                ProjectRefrence pr = null;
                if (!_isEntity)
                {
                    if (this.cb_metadataFile.SelectedIndex == 0 && _operateType != OperateTypeEnum.Refrence && _operateType != OperateTypeEnum.Inhert)
                    {
                        AddCommonRecord();
                    }
                    else
                    {
                        if (_operateType != OperateTypeEnum.Refrence && _operateType != OperateTypeEnum.Inhert)
                        {
                            pr = this._refDlls[this.cb_metadataFile.SelectedIndex - 1];
                        }
                        else
                        {
                            pr = this._refDlls[this.cb_metadataFile.SelectedIndex];
                        }
                        string fullPath = AppDomain.CurrentDomain.BaseDirectory + pr.RefFilePath;
                        AddItemsFromProj(fullPath, pr.RefrenceType == RefType.BEEntity);
                    }
                }
                else
                {
                    pr = this._refDlls[this.cb_metadataFile.SelectedIndex];
                    string fullPath = AppDomain.CurrentDomain.BaseDirectory + pr.RefFilePath;
                    AddItemsFromProj(fullPath, pr.RefrenceType == RefType.BEEntity);
                }
            }
        }

        private void AddItemsFromProj(string filePath, bool entity)
        {
            if (!System.IO.File.Exists(filePath))
            {
                return;
            }
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(filePath);
            List<XmlNode> nodeList = new List<XmlNode>();
            if (entity)
            {
                XmlNodeList tmpList = xmlDoc.FirstChild.SelectNodes("EntityList/Entity");
                nodeList.AddRange(XmlNodeListToList(tmpList));
                if (_operateType == OperateTypeEnum.Refrence)
                {
                    tmpList = xmlDoc.FirstChild.SelectNodes("EnumList/Enum");
                    nodeList.AddRange(XmlNodeListToList(tmpList));
                }
            }
            else
            {
                XmlNodeList tmpList = xmlDoc.FirstChild.SelectNodes("EntityList/Entity");
                nodeList.AddRange(XmlNodeListToList(tmpList));
                if (_operateType == OperateTypeEnum.Refrence)
                {
                    tmpList = xmlDoc.FirstChild.SelectNodes("EnumList/Enum");
                    nodeList.AddRange(XmlNodeListToList(tmpList));
                }
                tmpList = xmlDoc.FirstChild.SelectNodes("DTOList/DTO");
                nodeList.AddRange(XmlNodeListToList(tmpList));

            }
            foreach (XmlNode node in nodeList)
            {
                AddRefrenceRecord(node, entity);
            }
        }
        private List<XmlNode> XmlNodeListToList(XmlNodeList nodeList)
        {
            List<XmlNode> returnList = new List<XmlNode>();
            if (nodeList != null && nodeList.Count > 0)
            {
                for (int i = 0; i < nodeList.Count; i++)
                {
                    returnList.Add(nodeList[i]);
                }
            }
            return returnList;
        }
        private void AddRefrenceRecord(XmlNode node, bool entity)
        {
            //如果是继承的话则不能继承具有聚合关系的实体
            if (_operateType == OperateTypeEnum.Inhert && _isEntity)
            {
                XmlNode attrNode = node.SelectSingleNode("Attributes/Attribute[@DataType='CompositionType']");
                if (attrNode != null) return;
            }
            SelectDTO dto = new SelectDTO();
            dto.Guid = node.Attributes["Guid"].Value.ToString();
            dto.IsEntity = entity;
            if (entity)
            {
                dto.TypeName = node.ParentNode.ParentNode.Attributes["namespace"].Value.ToString() + "." + node.Attributes["Code"].Value.ToString();
                dto.TypeFullName = node.ParentNode.ParentNode.Attributes["Name"].Value.ToString() + "." + node.Attributes["Name"].Value.ToString();
            }
            else
            {
                if (node.Name == "DTO")
                {
                    dto.TypeName = node.ParentNode.ParentNode.Attributes["namespace"].Value.ToString() + ".Deploy." + node.Attributes["Code"].Value.ToString();
                    dto.TypeFullName = node.ParentNode.ParentNode.Attributes["Name"].Value.ToString() + "." + node.Attributes["Name"].Value.ToString();
                }
                else
                {
                    dto.TypeName = node.ParentNode.ParentNode.Attributes["namespace"].Value.ToString() + ".Deploy." + node.Attributes["Code"].Value.ToString() + "DTO";
                    dto.TypeFullName = node.ParentNode.ParentNode.Attributes["Name"].Value.ToString() + "." + node.Attributes["Name"].Value.ToString() + "DTO";
                }

            }
            ListViewItem lvi = new ListViewItem();
            lvi.SubItems.Add(new System.Windows.Forms.ListViewItem.ListViewSubItem());
            lvi.SubItems.Add(new System.Windows.Forms.ListViewItem.ListViewSubItem());
            lvi.SubItems[0].Text = dto.TypeName;
            lvi.SubItems[1].Text = dto.TypeFullName;

            this.lv_TypeList.Items.Add(lvi);
            this._selectTypeList.Add(dto);
        }
        private void AddCommonRecord()
        {
            XmlDocument xmlDoc = new XmlDocument();

            xmlDoc.Load(Application.StartupPath + @"\Config\TypeMapping.xml");
            XmlNodeList nodeList = xmlDoc.SelectNodes("Mappings/Mapping");
            foreach (XmlNode node in nodeList)
            {
                SelectDTO dto = new SelectDTO();
                dto.Guid = "";
                dto.TypeName = node.Attributes["sourceType"].Value.ToString();
                dto.TypeFullName = node.Attributes["durationType"].Value.ToString();
                ListViewItem lvi = new ListViewItem();
                lvi.SubItems.Add(new System.Windows.Forms.ListViewItem.ListViewSubItem());
                lvi.SubItems.Add(new System.Windows.Forms.ListViewItem.ListViewSubItem());
                lvi.SubItems[0].Text = dto.TypeFullName;
                lvi.SubItems[1].Text = dto.TypeName;
                this.lv_TypeList.Items.Add(lvi);
                this._selectTypeList.Add(dto);
            }
        }
    }
    public class SelectDTO
    {
        private string _guid;
        public string Guid
        {
            get { return _guid; }
            set { _guid = value; }
        }
        private string _typeName;
        public string TypeName
        {
            get { return _typeName; }
            set { _typeName = value; }
        }
        private string _typeFullName;
        public string TypeFullName
        {
            get { return _typeFullName; }
            set { _typeFullName = value; }
        }
        private bool _isList = false;
        public bool IsList
        {
            get { return _isList; }
            set { _isList = value; }
        }
        private bool _isEntity = false;
        public bool IsEntity
        {
            get { return _isEntity; }
            set { _isEntity = value; }
        }

    }
}
