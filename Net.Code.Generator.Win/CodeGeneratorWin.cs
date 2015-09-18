using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Xml;
using Net.Code.Builder.Base;
using Net.Code.Builder.Build;
using Net.Code.Builder.Util;
using Net.Code.Builder.Build.Tpl;
using Net.Code.Builder.Build.Model;
using Net.Code.Builder.Build.BEBuild;
using Net.Code.Builder.Build.BPBuild;
using Net.Code.Builder.Enums;


namespace Net.Code.Generator.Win
{
    public partial class CodeGeneraterWin : Form
    {
        public static List<string> MetaDataPathList = null;
        public static string MetaPath = string.Empty;
        public static string CodePath = string.Empty;
        public static string AppPath = string.Empty;
        #region 构造函数
        public CodeGeneraterWin()
        {
            InitializeComponent();
            Net.Code.Builder.Util.OutPut.OutPutMsgHandler = OutPut.OutPutMsg;
            this.groupBox3.Enabled = false;
            this.tbNameSpace.ReadOnly = true;
            OutPut.OutputTextBox = this.tbOutPut;
            string basePath = Application.StartupPath;

            CodeGeneraterWin.CodePath = basePath + PubVariable.CodePath;
            CodeGeneraterWin.AppPath = basePath + PubVariable.AppPath;
            CodeGeneraterWin.MetaPath = basePath + PubVariable.MetaDataPath;
            if (!System.IO.Directory.Exists(CodeGeneraterWin.MetaPath))
            {
                System.IO.Directory.CreateDirectory(CodeGeneraterWin.MetaPath);
            }
            if (!System.IO.Directory.Exists(CodeGeneraterWin.CodePath))
            {
                System.IO.Directory.CreateDirectory(CodeGeneraterWin.CodePath);
            }
            if (!System.IO.Directory.Exists(CodeGeneraterWin.AppPath))
            {
                System.IO.Directory.CreateDirectory(CodeGeneraterWin.AppPath);
            }

            CodeGeneraterWin.MetaDataPathList = new List<string>();
            string metaPathStr = System.Configuration.ConfigurationManager.AppSettings["metaPath"];
            string[] metaPathArray = metaPathStr.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            if (metaPathArray != null)
            {
                foreach (string metaPath in metaPathArray)
                {
                    CodeGeneraterWin.MetaDataPathList.Add(basePath + metaPath);
                }
            }
            else
            {
                CodeGeneraterWin.MetaDataPathList.Add(CodeGeneraterWin.MetaPath);
            }
        }
        #endregion

        #region 内部使用变量
        /// <summary>
        /// 项目列表
        /// </summary>
        List<IProject> _projectList = new List<IProject>();
        /// <summary>
        /// 当前项目节点
        /// </summary>
        private ComponentTreeNode _currentProjNode;
        /// <summary>
        /// 当前项目
        /// </summary>
        private IProject _currentProj;
        /// <summary>
        /// 当前获取交点的节点
        /// </summary>
        private ComponentTreeNode _bindDataNode = null;
        /// <summary>
        /// 需要进行数据收集的节点
        /// </summary>
        private ComponentTreeNode _collectDataNode = null;

        private int _searchIndex = 0;
        private string _searchKey = string.Empty;
        private List<AbstractPlatformComponent> _searchComponents = new List<AbstractPlatformComponent>();

        /// <summary>
        /// 生成代码线程对象
        /// </summary>
        private Thread _generateCodeThread = null;
        #endregion

        #region 主面板操作事件函数

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            ComponentTreeNode node = e.Node as ComponentTreeNode;
            this._collectDataNode = this._bindDataNode;
            if (node != null)
            {
                if (this.DataCollect())
                {
                    this.btnInhert.Enabled = true;
                    this.btnClearInhert.Enabled = true;
                    this.dataGridView1.Columns[3].Visible = true;
                    this.dataGridView1.Columns[4].Visible = true;
                    this.dataGridView1.Columns[5].Visible = true;
                    this.dataGridView1.Columns[6].Visible = false;
                    this.dataGridView1.Columns[7].Visible = false;
                    this.dataGridView1.Columns[8].Visible = false;
                    this.dataGridView1.Columns[9].Visible = false;
                    this.dataGridView1.Columns[10].Visible = false;
                    if (node.NodeType == NodeType.BEProj || node.NodeType == NodeType.BPProj)
                    {
                        if (this._currentProjNode == null || this._currentProjNode.Guid != node.Guid)
                        {
                            this._searchIndex = 0;
                            this._currentProjNode = node;
                        }
                        foreach (IProject proj in this._projectList)
                        {
                            AbstractPlatformComponent componentProj = proj as AbstractPlatformComponent;
                            if (componentProj.Guid == node.Guid)
                            {
                                this._currentProj = proj;
                                break;
                            }
                        }

                        this.groupBox3.Enabled = false;
                        this.tbNameSpace.ReadOnly = false;
                        this.tbCode.Text = string.Empty;
                        this.tbDisplayName.Text = string.Empty;
                        this.tbTableName.Text = string.Empty;
                        this.tbInhertClass.Text = string.Empty;
                        this.tbInhertGuid.Text = string.Empty;
                        this.dataGridView1.Rows.Clear();
                    }
                    else if (node.NodeType == NodeType.BEEntity)
                    {
                        ComponentTreeNode rootNode = GetRootNode(node);
                        if (this._currentProjNode == null || this._currentProjNode.Guid != rootNode.Guid)
                        {
                            this._searchIndex = 0;
                            this._currentProjNode = rootNode;
                        }

                        foreach (IProject proj in this._projectList)
                        {
                            AbstractPlatformComponent componentProj = proj as AbstractPlatformComponent;
                            if (componentProj.Guid == this._currentProjNode.Guid)
                            {
                                this._currentProj = proj;
                                break;
                            }
                        }
                        this.dataGridView1.Columns[7].Visible = true;
                        this.dataGridView1.Columns[9].Visible = true;
                        this.dataGridView1.Columns[10].Visible = true;
                        this.groupBox3.Enabled = true;
                        this.tbNameSpace.ReadOnly = true;
                        this.tbTableName.ReadOnly = false;
                    }
                    else if (node.NodeType == NodeType.DTOEntity)
                    {

                        ComponentTreeNode rootNode = GetRootNode(node);
                        if (this._currentProjNode == null || this._currentProjNode.Guid != rootNode.Guid)
                        {
                            this._searchIndex = 0;
                            this._currentProjNode = rootNode;
                        }

                        foreach (IProject proj in this._projectList)
                        {
                            AbstractPlatformComponent componentProj = proj as AbstractPlatformComponent;
                            if (componentProj.Guid == this._currentProjNode.Guid)
                            {
                                this._currentProj = proj;
                                break;
                            }
                        }
                        this.dataGridView1.Columns[9].Visible = true;
                        this.groupBox3.Enabled = true;
                        this.tbTableName.Text = string.Empty;
                        this.tbNameSpace.ReadOnly = true;
                        this.tbTableName.ReadOnly = true;
                    }
                    else if (node.NodeType == NodeType.EnumEntity)
                    {
                        ComponentTreeNode rootNode = GetRootNode(node);
                        if (this._currentProjNode == null || this._currentProjNode.Guid != rootNode.Guid)
                        {
                            this._searchIndex = 0;
                            this._currentProjNode = rootNode;
                        }

                        foreach (IProject proj in this._projectList)
                        {
                            AbstractPlatformComponent componentProj = proj as AbstractPlatformComponent;
                            if (componentProj.Guid == this._currentProjNode.Guid)
                            {
                                this._currentProj = proj;
                                break;
                            }
                        }
                        this.groupBox3.Enabled = true;
                        this.tbTableName.Text = string.Empty;
                        this.tbInhertClass.Text = string.Empty;
                        this.tbInhertGuid.Text = string.Empty;
                        this.tbNameSpace.ReadOnly = true;
                        this.tbTableName.ReadOnly = true;
                        this.btnInhert.Enabled = false;
                        this.btnClearInhert.Enabled = false;
                        this.dataGridView1.Columns[3].Visible = false;
                        this.dataGridView1.Columns[4].Visible = false;
                        this.dataGridView1.Columns[5].Visible = false;
                        this.dataGridView1.Columns[6].Visible = false;
                        this.dataGridView1.Columns[8].Visible = true;
                    }
                    else if (node.NodeType == NodeType.BPEntity)
                    {

                        ComponentTreeNode rootNode = GetRootNode(node);
                        if (this._currentProjNode == null || this._currentProjNode.Guid != rootNode.Guid)
                        {
                            this._searchIndex = 0;
                            this._currentProjNode = rootNode;
                        }

                        foreach (IProject proj in this._projectList)
                        {
                            AbstractPlatformComponent componentProj = proj as AbstractPlatformComponent;
                            if (componentProj.Guid == this._currentProjNode.Guid)
                            {
                                this._currentProj = proj;
                                break;
                            }
                        }
                        this.groupBox3.Enabled = true;
                        this.tbTableName.Text = string.Empty;
                        this.tbNameSpace.ReadOnly = true;
                        this.tbTableName.ReadOnly = true;
                    }
                    else if (node.NodeType == NodeType.Floder)
                    {
                        ComponentTreeNode rootNode = GetRootNode(node);
                        if (this._currentProjNode == null || this._currentProjNode.Guid != rootNode.Guid)
                        {
                            this._searchIndex = 0;
                            this._currentProjNode = rootNode;
                        }

                        foreach (IProject proj in this._projectList)
                        {
                            AbstractPlatformComponent componentProj = proj as AbstractPlatformComponent;
                            if (componentProj.Guid == this._currentProjNode.Guid)
                            {
                                this._currentProj = proj;
                                break;
                            }
                        }

                        this.groupBox3.Enabled = false;
                        this.tbNameSpace.ReadOnly = true;
                        this.tbCode.Text = string.Empty;
                        this.tbDisplayName.Text = string.Empty;
                        this.tbTableName.Text = string.Empty;
                        this.tbInhertClass.Text = string.Empty;
                        this.tbInhertGuid.Text = string.Empty;
                        this.dataGridView1.Rows.Clear();
                    }
                    else if (node.NodeType == NodeType.Refrence || node.NodeType == NodeType.RefrenceDll)
                    {
                        ComponentTreeNode rootNode = GetRootNode(node);
                        if (this._currentProjNode == null || this._currentProjNode.Guid != rootNode.Guid)
                        {
                            this._searchIndex = 0;
                            this._currentProjNode = rootNode;
                        }
                        foreach (IProject proj in this._projectList)
                        {
                            AbstractPlatformComponent componentProj = proj as AbstractPlatformComponent;
                            if (componentProj.Guid == node.Guid)
                            {
                                this._currentProj = proj;
                                break;
                            }
                        }

                        this.groupBox3.Enabled = false;
                        this.tbNameSpace.ReadOnly = true;
                        this.tbCode.Text = string.Empty;
                        this.tbDisplayName.Text = string.Empty;
                        this.tbTableName.Text = string.Empty;
                        this.tbInhertClass.Text = string.Empty;
                        this.tbInhertGuid.Text = string.Empty;
                        this.dataGridView1.Rows.Clear();
                    }
                }
            }

            if (node.NodeType == NodeType.Floder)
            {
                if (node.IsExpanded)
                {
                    node.ImageIndex = 4;
                    node.SelectedImageIndex = 4;
                }
                else
                {
                    node.ImageIndex = 3;
                    node.SelectedImageIndex = 3;
                }
            }
            this._bindDataNode = node;

            DataBind();
            this._collectDataNode = node;
        }
        private void treeView1_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            ComponentTreeNode node = e.Node as ComponentTreeNode;
            if (node.NodeType == NodeType.Floder)
            {
                Floder floder = this._currentProj.GetFloder(node.Guid);
                if (floder != null)
                {
                    ChangeNameForm cnf = new ChangeNameForm(node.Text);
                    if (cnf.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        floder.Name = cnf.InputName;
                        node.Text = floder.Name;
                    }
                }
            }
        }
        private void treeView1_AfterCollapse(object sender, TreeViewEventArgs e)
        {
            ComponentTreeNode node = e.Node as ComponentTreeNode;
            if (node.NodeType == NodeType.Floder)
            {
                node.ImageIndex = node.SelectedImageIndex = 3;
            }
            else if (node.NodeType == NodeType.Refrence)
            {
                node.ImageIndex = node.SelectedImageIndex = 6;
            }
        }
        private void treeView1_AfterExpand(object sender, TreeViewEventArgs e)
        {
            ComponentTreeNode node = e.Node as ComponentTreeNode;
            if (node.NodeType == NodeType.Floder)
            {
                node.ImageIndex = node.SelectedImageIndex = 4;
            }
            else if (node.NodeType == NodeType.Refrence)
            {
                node.ImageIndex = node.SelectedImageIndex = 7;
            }
        }
        private void CodeGenerater_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_generateCodeThread != null && _generateCodeThread.ThreadState == ThreadState.Running)
            {
                e.Cancel = true;
                MessageBox.Show("当前正在生成代码，无法关闭!", "提示");
            }
        }
        /// <summary>
        /// 选择引用类型
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnRefType_Click(object sender, EventArgs e)
        {

        }
        private void dataGridView1_CellMouseDown(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.ColumnIndex == 4 && e.RowIndex >= 0)
            {
                DataGridViewRow r = this.dataGridView1.Rows[e.RowIndex];
                //如果当前编码不能编辑，则无需弹出菜单
                if (r.Cells[1].ReadOnly) return;
                if (r.Cells[3].EditedFormattedValue != null)
                {
                    string strDataType = r.Cells[3].EditedFormattedValue.ToString();
                    if (string.IsNullOrEmpty(strDataType))
                    {
                        MessageBox.Show("请选择属性类型");
                    }
                    else
                    {
                        bool isEntity = IsEntity();
                        SelectAttributeType sat = null;
                        if (strDataType == "基础类型")
                        {
                            sat = new SelectAttributeType(OperateTypeEnum.Common, this._currentProj.RefrenceList, isEntity);
                        }
                        else if (strDataType == "引用类型")
                        {
                            sat = new SelectAttributeType(OperateTypeEnum.Refrence, this._currentProj.RefrenceList, isEntity);
                        }
                        else if (strDataType == "聚合类型")
                        {
                            sat = new SelectAttributeType(OperateTypeEnum.Composition, this._currentProj.RefrenceList, isEntity);
                        }
                        if (sat.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                        {
                            r.Cells[4].Value = sat.SelectedType.TypeName;
                            r.Cells[6].Value = sat.SelectedType.Guid;
                            if (string.IsNullOrEmpty(sat.SelectedType.Guid) && sat.SelectedType.TypeName == "string")
                            {
                                r.Cells[7].ReadOnly = false;
                                r.Cells[7].Value = "100";
                            }
                            else
                            {
                                r.Cells[7].ReadOnly = true;
                                r.Cells[7].Value = string.Empty;
                            }
                        }
                    }
                }
                else
                {
                    MessageBox.Show("请选择属性类型");
                }
            }

        }
        /// <summary>
        /// 判断当前是否需要使用实体类型
        /// </summary>
        /// <returns></returns>
        private bool IsEntity()
        {
            bool isEntity;
            if (this._currentProj is BPProj)
            {
                isEntity = false;
            }
            else
            {
                if (this._bindDataNode.NodeType == NodeType.BEEntity)
                {
                    isEntity = true;
                }
                else
                {
                    isEntity = false;
                }
            }
            return isEntity;
        }
        private void tbDisplayName_Leave(object sender, EventArgs e)
        {
            if (this._bindDataNode != null)
            {
                this._bindDataNode.Text = this.tbDisplayName.Text;
            }
        }
        private void tbCode_Leave(object sender, EventArgs e)
        {
            if (this._bindDataNode != null && this._bindDataNode.NodeType == NodeType.BEEntity)
            {
                if (string.IsNullOrEmpty(this.tbTableName.Text))
                {
                    this.tbTableName.Text = "T_" + this.tbCode.Text.ToUpper();
                }
            }
            else
            {
                this.tbTableName.Text = string.Empty;
            }
        }
        /// <summary>
        /// 弹出继承选择对话框
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnInhert_Click(object sender, EventArgs e)
        {
            bool isEntity = IsEntity();
            SelectAttributeType sat = null;
            if (this._bindDataNode.NodeType == NodeType.BPEntity)
            {
                sat = new SelectAttributeType(OperateTypeEnum.Return, this._currentProj.RefrenceList, false);
                if (sat.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    this.tbInhertGuid.Text = sat.SelectedType.Guid;
                    this.cbList.Checked = sat.SelectedType.IsList;
                    this.ckIsEntity.Checked = sat.SelectedType.IsEntity;
                    this.tbReturnType.Text = sat.SelectedType.TypeName;
                    if (sat.SelectedType.IsList)
                    {
                        this.tbInhertClass.Text = "List<" + sat.SelectedType.TypeName + ">";
                    }
                    else
                    {
                        this.tbInhertClass.Text = sat.SelectedType.TypeName;
                    }
                }
            }
            else
            {
                sat = new SelectAttributeType(OperateTypeEnum.Inhert, this._currentProj.RefrenceList, isEntity);
                if (sat.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    if (sat.SelectedType.Guid != this._bindDataNode.Guid)
                    {
                        this.tbInhertGuid.Text = sat.SelectedType.Guid;
                        this.tbInhertClass.Text = sat.SelectedType.TypeName;
                    }
                    else
                    {
                        MessageBox.Show("模型不能继承自身", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }
        private void btnClearInhert_Click(object sender, EventArgs e)
        {
            //如果存在guid则需要清空guid并设置默认值
            if (!string.IsNullOrEmpty(this.tbInhertGuid.Text))
            {
                this.tbInhertGuid.Text = string.Empty;
            }
            if (this._bindDataNode.NodeType == NodeType.BEEntity)
            {
                this.tbInhertClass.Text = Attributes.BaseEntity;
            }
            else if (this._bindDataNode.NodeType == NodeType.DTOEntity)
            {
                this.tbInhertClass.Text = Attributes.BaseDTO;
            }
            else if (this._bindDataNode.NodeType == NodeType.BPEntity)
            {
                this.tbInhertClass.Text = "void";
                this.tbReturnType.Text = "void";
                this.cbList.Checked = false;
            }

        }
        #endregion

        #region 弹出菜单操作事件函数
        /// <summary>
        /// 新增一个datagird行
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cms_Add_Click(object sender, EventArgs e)
        {
            DataGridViewRow row = new DataGridViewRow();  //新建一个DataGridViewRow 
            foreach (DataGridViewColumn c in this.dataGridView1.Columns)
            {
                row.Cells.Add(c.CellTemplate.Clone() as DataGridViewCell);//给行添加单元格
            }
            row.Cells[0].Value = Guid.NewGuid().ToString();
            this.dataGridView1.Rows.Add(row);
        }
        /// <summary>
        /// 删除一个datagird行
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cms_Delete_Click(object sender, EventArgs e)
        {
            if (this.dataGridView1.SelectedRows != null && this.dataGridView1.SelectedRows.Count > 0)
            {
                if (this._bindDataNode != null && this._currentProj != null)
                {
                    if (this._bindDataNode.NodeType == NodeType.BEEntity)
                    {
                        BEEntity currentEntity = this._currentProj.Get(this._bindDataNode.Guid) as BEEntity;
                        if (currentEntity != null)
                        {
                            DataGridViewRow r = this.dataGridView1.SelectedRows[0];
                            //将实体数据中的列设置为删除
                            string guid = r.Cells[0].Value.ToString();
                            BEColumn col = currentEntity.Get(guid);
                            if (!string.IsNullOrEmpty(col.RefColGuid))
                            {
                                return;
                            }
                            if (col != null)
                            {
                                if (col.DataState == DataState.Update)
                                {
                                    col.DataState = DataState.Delete;
                                }
                                else
                                {
                                    currentEntity.ColumnList.Remove(col);
                                }
                            }
                            this.dataGridView1.Rows.Remove(r);
                        }

                    }
                    else if (this._bindDataNode.NodeType == NodeType.BPEntity)
                    {
                        BPEntity currentEntity = this._currentProj.Get(this._bindDataNode.Guid) as BPEntity;
                        if (currentEntity != null)
                        {
                            DataGridViewRow r = this.dataGridView1.SelectedRows[0];
                            //将实体数据中的列设置为删除
                            string guid = r.Cells[0].Value.ToString();
                            BPColumn col = currentEntity.Get(guid);
                            if (col != null)
                            {
                                if (col.DataState == DataState.Update)
                                {
                                    col.DataState = DataState.Delete;
                                }
                                else
                                {
                                    currentEntity.ColumnList.Remove(col);
                                }
                            }
                            this.dataGridView1.Rows.Remove(r);
                        }
                    }
                    else if (this._bindDataNode.NodeType == NodeType.DTOEntity)
                    {
                        DTOEntity currentEntity = this._currentProj.Get(this._bindDataNode.Guid) as DTOEntity;
                        if (currentEntity != null)
                        {
                            DataGridViewRow r = this.dataGridView1.SelectedRows[0];
                            //将实体数据中的列设置为删除
                            string guid = r.Cells[0].Value.ToString();
                            DTOColumn col = currentEntity.Get(guid);
                            if (col != null)
                            {
                                if (col.DataState == DataState.Update)
                                {
                                    col.DataState = DataState.Delete;
                                }
                                else
                                {
                                    currentEntity.ColumnList.Remove(col);
                                }
                            }
                            this.dataGridView1.Rows.Remove(r);
                        }
                    }
                    else if (this._bindDataNode.NodeType == NodeType.EnumEntity)
                    {
                        EnumEntity currentEntity = this._currentProj.Get(this._bindDataNode.Guid) as EnumEntity;
                        if (currentEntity != null)
                        {
                            DataGridViewRow r = this.dataGridView1.SelectedRows[0];
                            //将实体数据中的列设置为删除
                            string guid = r.Cells[0].Value.ToString();
                            EnumColumn col = currentEntity.Get(guid);
                            if (col != null)
                            {
                                if (col.DataState == DataState.Update)
                                {
                                    col.DataState = DataState.Delete;
                                }
                                else
                                {
                                    currentEntity.ColumnList.Remove(col);
                                }
                            }
                            this.dataGridView1.Rows.Remove(r);
                        }
                    }

                }
            }
        }

        #endregion

        #region 菜单操作事件列表

        private void tsm_CreateBEProj_Click(object sender, EventArgs e)
        {
            NameInputForm f = new NameInputForm();
            if (f.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                IProject proj = BEProj.CreateProject(f.InputNameSpace, f.InputName);
                this.AddProjNode(proj);
                this._projectList.Add(proj);
            }
        }
        private void tsm_CreateBPProj_Click(object sender, EventArgs e)
        {
            NameInputForm f = new NameInputForm();
            if (f.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                IProject proj = BPProj.CreateBPProject(f.InputNameSpace, f.InputName);
                this.AddProjNode(proj);
                this._projectList.Add(proj);
            }
        }
        private void tsm_CreateSVProj_Click(object sender, EventArgs e)
        {
            NameInputForm f = new NameInputForm();
            if (f.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                IProject proj = BPProj.CreateSVCProject(f.InputNameSpace, f.InputName);
                this.AddProjNode(proj);
                this._projectList.Add(proj);
            }
        }
        private void tsm_SaveAll_Click(object sender, EventArgs e)
        {
            //保存前收集当前节点的数据
            this._collectDataNode = this._bindDataNode;

            if (!this.DataCollect()) return;

            foreach (IProject proj in this._projectList)
            {
                proj.ToFile();
                OutPut.OutPutMsg("保存项目【" + proj.ProjName + "】成功!");
            }
            foreach (IProject proj in this._projectList)
            {
                proj.Load();
                //AbstractPlatformComponent projComponent = proj as AbstractPlatformComponent;
                //projComponent.from(null);
            }
            //重新保存后需要重新绑定数据
            if (this._currentProj != null)
            {
                foreach (AbstractPlatformComponent proj in this._projectList)
                {
                    AbstractPlatformComponent projComponent = this._currentProj as AbstractPlatformComponent;
                    if (projComponent.Guid == proj.Guid)
                    {
                        this._currentProj = null;
                        this._currentProj = proj as IProject;
                        break;
                    }
                }
            }
            DataBind();
        }
        private void tsm_LoadProj_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Multiselect = true;
            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                try
                {
                    foreach (string fileName in ofd.FileNames)
                    {
                        LoadProj(fileName);
                    }
                }
                catch (Exception ex)
                {

                }
            }
        }
        private void LoadProj(string fileName)
        {
            IProject project = ProjectFactory.BuildProj(fileName);

            project.Load();
            foreach (AbstractPlatformComponent proj in this._projectList)
            {
                if (proj.Guid == project.Guid)
                {
                    return;//该项目已经载入到项目中了
                }
            }
            this._projectList.Add(project);
            AddProjNode(project);

            OutPut.OutPutMsg("载入项目【" + project.ProjName + "】成功!");
        }
        private void tsm_GenerateCode_Click(object sender, EventArgs e)
        {
            tsm_SaveAll_Click(null, null);
            _generateCodeThread = new Thread(new ThreadStart(batchGenerateCode));
            _generateCodeThread.Start();
        }
        private void tsm_GenerateAll_Click(object sender, EventArgs e)
        {
            tsm_SaveAll_Click(null, null);
            _generateCodeThread = new Thread(new ThreadStart(batchGenerateCodeWithCompile));
            _generateCodeThread.Start();
        }
        private void tsm_Close_Click(object sender, EventArgs e)
        {
            if (this._projectList.Count > 0)
            {
                DialogResult dr = MessageBox.Show("是否保存当前更改?", "确认", MessageBoxButtons.YesNoCancel);

                if (dr == System.Windows.Forms.DialogResult.Yes)
                {
                    tsm_SaveAll_Click(null, null);
                }
                if (dr != System.Windows.Forms.DialogResult.Cancel)
                {
                    this.Close();
                }
            }
            else
            {
                this.Close();
            }
        }
        /// <summary>
        /// 搜索功能
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSearch_Click(object sender, EventArgs e)
        {
            if (this._currentProj == null)
            {
                MessageBox.Show("没有选中任何项目", "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            else
            {
                if (this._searchKey != this.txtSearchKey.Text)
                {
                    this._searchKey = this.txtSearchKey.Text;
                    this._searchIndex = 0;
                }
                if (!string.IsNullOrEmpty(this._searchKey))
                {
                    this.tvProject.Focus();
                    if (this._searchIndex == 0)
                    {
                        this._searchComponents = this._currentProj.Find(this._searchKey);
                        this._searchIndex = 1;
                    }
                    else if (this._searchIndex > this._searchComponents.Count)
                    {
                        this._searchIndex = 1;
                    }
                    if (this._searchComponents.Count > 0)
                    {
                        AbstractPlatformComponent node = this._searchComponents[this._searchIndex - 1];
                        ComponentTreeNode treeNode = this.findNode(this._currentProjNode, node.Guid);
                        this.tvProject.SelectedNode = treeNode;
                        System.Windows.Forms.TreeNode tn = treeNode.Parent;
                        while (tn != null)
                        {
                            if (!tn.IsExpanded)
                            {
                                tn.Expand();
                            }
                            tn = tn.Parent;
                        }
                        this.tvProject.SelectedNode = treeNode;
                        this._searchIndex++;
                    }
                }

            }
        }


        #endregion

        #region 自定义页面操作函数

        private void AddProjNode(IProject proj)
        {
            AbstractPlatformComponent projComponent = proj as AbstractPlatformComponent;
            ComponentTreeNode projNode = null;
            //先添加根节点
            if (proj.ProjType == ProjectTypeEnum.BEProj)
            {
                projNode = new ComponentTreeNode(NodeType.BEProj, projComponent.Guid);
                projNode.ImageIndex = projNode.SelectedImageIndex = 0;
                projNode.Text = proj.ProjName;
            }
            else if (proj.ProjType == ProjectTypeEnum.BPProj)
            {
                projNode = new ComponentTreeNode(NodeType.BPProj, projComponent.Guid);
                projNode.ImageIndex = projNode.SelectedImageIndex = 0;
                projNode.Text = proj.ProjName;
            }

            this.tvProject.Nodes.Add(projNode);
            //增加引用节点
            ComponentTreeNode refrenceNode = new ComponentTreeNode(NodeType.Refrence, string.Empty);
            refrenceNode.Text = "引用";
            refrenceNode.ImageIndex = refrenceNode.SelectedImageIndex = 6;
            projNode.Nodes.Add(refrenceNode);
            foreach (ProjectRefrence pr in proj.RefrenceList)
            {
                ComponentTreeNode rootNode = projNode;

                ComponentTreeNode refrenceDllNode = new ComponentTreeNode(NodeType.RefrenceDll, pr.Guid);
                refrenceDllNode.ImageIndex = refrenceDllNode.SelectedImageIndex = 8;
                refrenceDllNode.Text = pr.Name;
                refrenceNode.Nodes.Add(refrenceDllNode);
            }
            //增加文件夹节点
            foreach (Floder floder in proj.FloderList)
            {
                ComponentTreeNode rootNode = projNode;
                if (!string.IsNullOrEmpty(floder.PGuid))
                {
                    rootNode = findNode(rootNode, floder.PGuid);
                    if (rootNode == null) throw new Exception("Guid:" + floder.PGuid + "的树节点对象没有找到");
                }
                ComponentTreeNode floderNode = new ComponentTreeNode(NodeType.Floder, floder.Guid);
                floderNode.ImageIndex = floderNode.SelectedImageIndex = 3;
                floderNode.Text = floder.Name;
                rootNode.Nodes.Add(floderNode);
            }
            //增加实体节点
            if (proj.ProjType == ProjectTypeEnum.BEProj)
            {
                foreach (BEEntity entity in proj.EntityList)
                {
                    ComponentTreeNode entityNode = new ComponentTreeNode(NodeType.BEEntity, entity.Guid);
                    entityNode.ImageIndex = entityNode.SelectedImageIndex = 1;
                    entityNode.Text = entity.Name;
                    ComponentTreeNode rootNode = projNode;
                    if (!string.IsNullOrEmpty(entity.FloderGuid))
                    {
                        rootNode = findNode(projNode, entity.FloderGuid);
                        if (rootNode == null) throw new Exception("Guid:" + entity.FloderGuid + "的树节点对象没有找到");
                    }
                    rootNode.Nodes.Add(entityNode);
                }
            }
            //增加实体节点
            if (proj.ProjType == ProjectTypeEnum.BPProj)
            {
                foreach (BPEntity entity in proj.EntityList)
                {
                    ComponentTreeNode entityNode = new ComponentTreeNode(NodeType.BPEntity, entity.Guid);
                    entityNode.ImageIndex = entityNode.SelectedImageIndex = 2;
                    entityNode.Text = entity.Name;
                    ComponentTreeNode rootNode = projNode;
                    if (!string.IsNullOrEmpty(entity.FloderGuid))
                    {
                        rootNode = findNode(projNode, entity.FloderGuid);
                        if (rootNode == null) throw new Exception("Guid:" + entity.FloderGuid + "的树节点对象没有找到");
                    }
                    rootNode.Nodes.Add(entityNode);
                }
            }
            //增加DTO实体节点
            foreach (DTOEntity entity in proj.DTOList)
            {
                ComponentTreeNode entityNode = new ComponentTreeNode(NodeType.DTOEntity, entity.Guid);
                entityNode.ImageIndex = entityNode.SelectedImageIndex = 5;
                entityNode.Text = entity.Name;
                ComponentTreeNode rootNode = projNode;
                if (!string.IsNullOrEmpty(entity.FloderGuid))
                {
                    rootNode = findNode(projNode, entity.FloderGuid);
                    if (rootNode == null) throw new Exception("Guid:" + entity.FloderGuid + "的树节点对象没有找到");
                }
                rootNode.Nodes.Add(entityNode);
            }

            //增加DTO实体节点
            foreach (EnumEntity entity in proj.EnumList)
            {
                ComponentTreeNode entityNode = new ComponentTreeNode(NodeType.EnumEntity, entity.Guid);
                entityNode.ImageIndex = entityNode.SelectedImageIndex = 9;
                entityNode.Text = entity.Name;
                ComponentTreeNode rootNode = projNode;
                if (!string.IsNullOrEmpty(entity.FloderGuid))
                {
                    rootNode = findNode(projNode, entity.FloderGuid);
                    if (rootNode == null) throw new Exception("Guid:" + entity.FloderGuid + "的树节点对象没有找到");
                }
                rootNode.Nodes.Add(entityNode);
            }

            this._currentProj = proj;
            this._currentProjNode = projNode;
            this._bindDataNode = this._currentProjNode;
            this.DataBind();
            this.tvProject.SelectedNode = this._currentProjNode;
        }

        private ComponentTreeNode findNode(TreeNode node, string guid)
        {

            ComponentTreeNode ctn = node as ComponentTreeNode;
            if (ctn.Guid == guid) return ctn;


            foreach (TreeNode subNode in node.Nodes)
            {
                if (node.Nodes != null)
                {
                    ComponentTreeNode tmp = findNode(subNode, guid);
                    if (tmp != null) return tmp;
                }
            }
            return null;
        }

        #endregion

        #region 树形结构快捷菜单
        /// <summary>
        /// 右键菜单弹出前初始化工作
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void treeView1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Right)
            {
                this.tsm_AddEntity.Enabled = false;
                this.tsm_AddBPEntity.Enabled = false;
                this.tsm_AddDTOEntity.Enabled = false;
                this.tsm_AddEnum.Enabled = false;
                this.tsm_AddFloder.Enabled = false;
                this.tsm_RemoveNode.Enabled = false;
                this.tsm_BuildCode.Enabled = false;
                this.tsm_RemoveProject.Enabled = false;
                this.tsm_SaveProject.Enabled = false;
                if (this._bindDataNode != null)
                {

                    if (this._bindDataNode.NodeType == NodeType.BEProj
                        || this._bindDataNode.NodeType == NodeType.BPProj)
                    {
                        this.tsm_RemoveProject.Enabled = true;
                        this.tsm_BuildCode.Enabled = true;
                        this.tsm_SaveProject.Enabled = true;
                    }
                    if (this._bindDataNode.NodeType != NodeType.BEProj
                        && this._bindDataNode.NodeType != NodeType.BPProj)
                    {
                        this.tsm_RemoveNode.Enabled = true;
                    }

                    if (this._bindDataNode.NodeType == NodeType.BEEntity
                        || this._bindDataNode.NodeType == NodeType.BPEntity
                        || this._bindDataNode.NodeType == NodeType.DTOEntity)
                    {

                    }
                    else
                    {
                        this.tsm_AddFloder.Enabled = true;
                        this.tsm_AddDTOEntity.Enabled = true;
                    }
                    if (this._bindDataNode.NodeType == NodeType.BEProj)
                    {
                        this.tsm_AddEnum.Enabled = true;
                        this.tsm_AddEntity.Enabled = true;
                    }
                    if (this._bindDataNode.NodeType == NodeType.BPProj)
                    {
                        this.tsm_AddBPEntity.Enabled = true;
                    }
                    if (this._bindDataNode.NodeType == NodeType.Floder)
                    {
                        if (this._currentProj is BEProj)
                        {
                            this.tsm_AddEnum.Enabled = true;
                            this.tsm_AddEntity.Enabled = true;
                        }
                        else if (this._currentProj is BPProj)
                        {
                            this.tsm_AddBPEntity.Enabled = true;
                        }
                    }
                }

            }
        }
        /// <summary>
        /// 新增实体
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tsm_AddEntity_Click(object sender, EventArgs e)
        {
            if (this._currentProjNode == null || this._currentProjNode.NodeType != NodeType.BEProj)
            {
                MessageBox.Show("请选择一个实体工程");
            }
            else
            {
                ComponentTreeNode tn = new ComponentTreeNode(NodeType.BEEntity, Guid.NewGuid().ToString());
                tn.Text = "新增实体";
                tn.ImageIndex = tn.SelectedImageIndex = 1;
                BEEntity entity = new BEEntity(this._currentProj as BEProj, tn.Guid, string.Empty);
                if (this._bindDataNode.NodeType == NodeType.Floder)
                {
                    entity.FloderGuid = this._bindDataNode.Guid;
                }
                entity.Code = string.Empty;
                entity.Name = string.Empty;
                entity.TableName = string.Empty;
                if (this._bindDataNode.NodeType == NodeType.Floder)
                {
                    entity.FloderGuid = this._bindDataNode.Guid;
                }

                this._currentProj.EntityList.Add(entity);

                this._bindDataNode.Nodes.Add(tn);

                this.groupBox3.Enabled = true;

                this.dataGridView1.Rows.Clear();

                this.tvProject.SelectedNode = tn;

            }
        }
        /// <summary>
        /// 新增BP
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tsm_AddBPEntity_Click(object sender, EventArgs e)
        {
            if (this._currentProjNode == null || this._currentProjNode.NodeType != NodeType.BPProj)
            {
                MessageBox.Show("请选择一个BP工程");
            }
            else
            {
                ComponentTreeNode tn = new ComponentTreeNode(NodeType.BPEntity, Guid.NewGuid().ToString());
                tn.Text = "新增BP";
                tn.ImageIndex = tn.SelectedImageIndex = 2;
                BPEntity entity = new BPEntity(this._currentProj as BPProj, tn.Guid, string.Empty);
                if (this._bindDataNode.NodeType == NodeType.Floder)
                {
                    entity.FloderGuid = this._bindDataNode.Guid;
                }
                entity.Code = string.Empty;
                entity.Name = string.Empty;
                if (this._bindDataNode.NodeType == NodeType.Floder)
                {
                    entity.FloderGuid = this._bindDataNode.Guid;
                }
                entity.ReturnName = entity.ReturnProxyName = "void";
                entity.ReturnGuid = string.Empty;
                this._currentProj.EntityList.Add(entity);

                this._bindDataNode.Nodes.Add(tn);

                this.groupBox3.Enabled = true;

                this.dataGridView1.Rows.Clear();

                this.tvProject.SelectedNode = tn;

            }
        }
        /// <summary>
        /// 新增枚举
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tsm_AddEnum_Click(object sender, EventArgs e)
        {
            if (this._currentProjNode == null)
            {
                MessageBox.Show("请选择一个工程");
            }
            else
            {
                ComponentTreeNode tn = new ComponentTreeNode(NodeType.EnumEntity, Guid.NewGuid().ToString());
                tn.Text = "新增Enum";
                tn.ImageIndex = tn.SelectedImageIndex = 9;
                EnumEntity entity = new EnumEntity(this._currentProj, tn.Guid, string.Empty);
                if (this._bindDataNode.NodeType == NodeType.Floder)
                {
                    entity.FloderGuid = this._bindDataNode.Guid;
                }
                entity.Code = string.Empty;
                entity.Name = tbDisplayName.Text;
                if (this._bindDataNode.NodeType == NodeType.Floder)
                {
                    entity.FloderGuid = this._bindDataNode.Guid;
                }

                this._currentProj.EnumList.Add(entity);

                this._bindDataNode.Nodes.Add(tn);

                this.tvProject.SelectedNode = tn;
            }
        }
        /// <summary>
        /// 新增DTO
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tsm_AddDTOEntity_Click(object sender, EventArgs e)
        {
            if (this._currentProjNode == null)
            {
                MessageBox.Show("请选择一个工程");
            }
            else
            {
                ComponentTreeNode tn = new ComponentTreeNode(NodeType.DTOEntity, Guid.NewGuid().ToString());
                tn.Text = "新增DTO";
                tn.ImageIndex = tn.SelectedImageIndex = 5;
                DTOEntity entity = new DTOEntity(this._currentProj, tn.Guid, string.Empty);
                if (this._bindDataNode.NodeType == NodeType.Floder)
                {
                    entity.FloderGuid = this._bindDataNode.Guid;
                }
                entity.Code = string.Empty;
                entity.Name = tbDisplayName.Text;
                if (this._bindDataNode.NodeType == NodeType.Floder)
                {
                    entity.FloderGuid = this._bindDataNode.Guid;
                }

                this._currentProj.DTOList.Add(entity);

                this._bindDataNode.Nodes.Add(tn);

                this.tvProject.SelectedNode = tn;
            }
        }
        /// <summary>
        /// 新增分类
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tsm_AddFloder_Click(object sender, EventArgs e)
        {
            if (this._bindDataNode == null)
            {
                MessageBox.Show("请选择你要添加分类的父节点"); return;
            }
            if (this._bindDataNode.NodeType == NodeType.BEProj
                || this._bindDataNode.NodeType == NodeType.BPProj
                || this._bindDataNode.NodeType == NodeType.Floder)
            {
                if (this._currentProj != null)
                {
                    ComponentTreeNode tn = new ComponentTreeNode(NodeType.Floder, Guid.NewGuid().ToString());
                    tn.Text = "新增分类";
                    tn.ImageIndex = tn.SelectedImageIndex = 3;
                    this._bindDataNode.Nodes.Add(tn);

                    Floder floder = new Floder(this._currentProj, tn.Guid);
                    floder.Name = tn.Text;
                    if (this._bindDataNode.NodeType == NodeType.Floder)
                    {
                        floder.PGuid = this._bindDataNode.Guid;
                    }
                    this._currentProj.FloderList.Add(floder);

                    this.tvProject.SelectedNode = tn;
                }
            }
        }
        /// <summary>
        /// 保存项目
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tsm_SaveProject_Click(object sender, EventArgs e)
        {
            if (this._currentProjNode != null && this._currentProj != null)
            {
                if (!this.DataCollect())
                {
                    MessageBox.Show("数据保存失败", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                foreach (IProject proj in this._projectList)
                {
                    if (proj.Guid != this._currentProjNode.Guid) continue;
                    try
                    {
                        proj.ToFile();
                        proj.Load();
                        break;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("数据保存失败,错误信息：" + ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                }
                DataBind();
            }
        }
        /// <summary>
        /// 移除节点
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tsm_RemoveNode_Click(object sender, EventArgs e)
        {
            if (this._bindDataNode != null)
            {
                if (this._bindDataNode.NodeType == NodeType.Floder)
                {
                    if (this._bindDataNode.Nodes != null && this._bindDataNode.Nodes.Count > 0)
                    {
                        MessageBox.Show("当前分类“" + this._bindDataNode.Text + "”有下级节点，不能删除");
                        return;
                    }
                    Floder floder = this._currentProj.GetFloder(this._bindDataNode.Guid);
                    if (floder != null)
                    {
                        floder.DataState = DataState.Delete;
                    }
                    this._bindDataNode.Parent.Nodes.Remove(this._bindDataNode);
                }
                else if (this._bindDataNode.NodeType == NodeType.BEEntity)///删除当前实体
                {
                    BEEntity currentEntity = this._currentProj.Get(this._bindDataNode.Guid) as BEEntity;
                    if (MessageBox.Show("确认删除实体“" + currentEntity.Name + "”?", "删除", MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
                    {
                        if (currentEntity.DataState == DataState.Add)
                        {
                            this._currentProj.EntityList.Remove(currentEntity);
                        }
                        else
                        {
                            currentEntity.DataState = DataState.Delete;
                        }
                        this._bindDataNode.Parent.Nodes.Remove(this._bindDataNode);
                    }
                }
                else if (this._bindDataNode.NodeType == NodeType.BPEntity)
                {
                    BPEntity currentEntity = this._currentProj.Get(this._bindDataNode.Guid) as BPEntity;
                    if (MessageBox.Show("确认删除服务“" + currentEntity.Name + "”?", "删除", MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
                    {
                        if (currentEntity.DataState == DataState.Add)
                        {
                            this._currentProj.EntityList.Remove(currentEntity);
                        }
                        else
                        {
                            currentEntity.DataState = DataState.Delete;
                        }
                        this._bindDataNode.Parent.Nodes.Remove(this._bindDataNode);
                    }
                }
                else if (this._bindDataNode.NodeType == NodeType.DTOEntity)
                {
                    DTOEntity currentEntity = this._currentProj.Get(this._bindDataNode.Guid) as DTOEntity;
                    if (MessageBox.Show("确认删除DTO“" + currentEntity.Name + "”?", "删除", MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
                    {
                        if (currentEntity.DataState == DataState.Add)
                        {
                            this._currentProj.EntityList.Remove(currentEntity);
                        }
                        else
                        {
                            currentEntity.DataState = DataState.Delete;
                        }
                        this._bindDataNode.Parent.Nodes.Remove(this._bindDataNode);
                    }
                }
                else if (this._bindDataNode.NodeType == NodeType.EnumEntity)
                {
                    EnumEntity currentEntity = this._currentProj.Get(this._bindDataNode.Guid) as EnumEntity;
                    if (MessageBox.Show("确认删除枚举“" + currentEntity.Name + "”?", "删除", MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
                    {
                        if (currentEntity.DataState == DataState.Add)
                        {
                            this._currentProj.EntityList.Remove(currentEntity);
                        }
                        else
                        {
                            currentEntity.DataState = DataState.Delete;
                        }
                        this._bindDataNode.Parent.Nodes.Remove(this._bindDataNode);
                    }
                }
                else if (this._bindDataNode.NodeType == NodeType.RefrenceDll)
                {
                    ProjectRefrence currentEntity = this._currentProj.Get(this._bindDataNode.Guid) as ProjectRefrence;
                    //当前实体工程自身的deploy不能删除
                    if (this._currentProj is BEProj)
                    {
                        if (currentEntity.RefrenceType == RefType.Deploy || currentEntity.RefrenceType == RefType.BEEntity)
                        {
                            //if (currentEntity.RefProjName == currentEntity.Proj.ProjName + ".be")
                            if (currentEntity.RefProjName == currentEntity.Proj.FileName)
                            {
                                MessageBox.Show("当前引用不能删除");
                                return;
                            }
                        }
                    }
                    if (MessageBox.Show("确认删除引用“" + currentEntity.Name + "”?", "删除", MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
                    {
                        if (currentEntity.DataState == DataState.Add)
                        {
                            this._currentProj.EntityList.Remove(currentEntity);
                        }
                        else
                        {
                            currentEntity.DataState = DataState.Delete;
                        }
                        ComponentTreeNode refrenceNode = this._bindDataNode.Parent as ComponentTreeNode;
                        refrenceNode.Nodes.Remove(this._bindDataNode);
                        if (refrenceNode.Nodes == null || refrenceNode.Nodes.Count <= 0)
                        {
                            refrenceNode.ImageIndex = refrenceNode.SelectedImageIndex = 6;
                        }
                    }
                }
            }
        }
        /// <summary>
        /// 载入工程
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tsm_LoadProject_Click(object sender, EventArgs e)
        {
            tsm_LoadProj_Click(null, null);
        }
        /// <summary>
        /// 生成代码
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tsm_BuildCode_Click(object sender, EventArgs e)
        {
            tsm_SaveAll_Click(null, null);
            if (this._currentProjNode != null && this._currentProj != null)
            {
                if (MessageBox.Show("确认生成项目“" + this._currentProj.ProjName + "”代码?", "确认", MessageBoxButtons.OKCancel) == System.Windows.Forms.DialogResult.OK)
                {
                    if (!this.DataCollect()) return;
                    this._currentProj.ToFile();
                    OutPut.OutPutMsg("保存项目【" + this._currentProj.ProjName + "】成功!");
                    GenerateProjCodeWithCompile(this._currentProj);
                }
            }
        }
        /// <summary>
        /// 移除项目
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tsm_RemoveProject_Click(object sender, EventArgs e)
        {
            if (this._currentProj != null)
            {
                if (MessageBox.Show("移除当前项目“" + this._currentProj.ProjName + "”，是否保存?", "移除", MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
                {
                    this._collectDataNode = this._bindDataNode;
                    this.DataCollect();
                    this._currentProj.ToFile();
                }
                this._projectList.Remove(this._currentProj);
                this.tvProject.Nodes.Remove(this._currentProjNode);
                this._currentProj = null;
                this._currentProjNode = null;
                this._bindDataNode = null;
                if (this._projectList == null || this._projectList.Count == 0)
                {
                    this.tbNameSpace.Text = string.Empty;
                    this.tbNameSpace.Enabled = false;
                }
                else
                {
                    ComponentTreeNode node = this.GetRootNode(this._projectList[0].Guid);
                    if (node != null)
                    {
                        this.tvProject.Focus();
                        this.tvProject.SelectedNode = node;
                    }

                }
            }
        }




        #endregion

        #region 自定义函数
        private bool DataCollect()
        {
            if (this._currentProj == null || this._currentProjNode == null) return true;
            this._currentProj.Namespace = this.tbNameSpace.Text;
            if (this._collectDataNode == null) return true;
            try
            {
                #region 实体类型
                if (this._collectDataNode.NodeType == NodeType.BEEntity)
                {
                    BEEntity prevEntity = this._currentProj.Get(this._collectDataNode.Guid) as BEEntity;
                    if (prevEntity != null)
                    {
                        if (prevEntity == null) return true;
                        prevEntity.ViewRange = (ViewRangeEnum)this.cbViewRange.SelectedIndex;
                        prevEntity.OrgFilter = this.cbOrgFilter.Checked;
                        prevEntity.Code = this.tbCode.Text;
                        prevEntity.Name = this.tbDisplayName.Text;
                        prevEntity.TableName = this.tbTableName.Text;
                        prevEntity.InhertGuid = this.tbInhertGuid.Text;
                        prevEntity.InhertName = this.tbInhertClass.Text;
                        foreach (BEEntity entity in this._currentProj.EntityList)
                        {
                            if (entity.Guid != prevEntity.Guid)
                            {
                                if (prevEntity.Code == entity.Code)
                                {
                                    MessageBox.Show("当前实体的编码不能与项目中已有的实体编码相同,实体名称:" + entity.Name, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    //return false;
                                }
                            }
                        }
                        for (int i = 0; i < this.dataGridView1.Rows.Count; i++)
                        {
                            DataGridViewRow r = this.dataGridView1.Rows[i];
                            if (r.Cells[1].EditedFormattedValue == null || string.IsNullOrEmpty(r.Cells[1].EditedFormattedValue.ToString()))
                            {
                                continue;
                            }
                            string guid = r.Cells[0].Value == null ? string.Empty : r.Cells[0].Value.ToString();
                            BEColumn col = prevEntity.Get(guid);
                            if (col == null)
                            {
                                col = new BEColumn(r.Cells[0].Value.ToString(), this._currentProj as BEProj, prevEntity);
                                prevEntity.ColumnList.Add(col);
                            }
                            if (r.Cells[1].EditedFormattedValue == null || string.IsNullOrEmpty(r.Cells[1].EditedFormattedValue.ToString()))
                            {
                                MessageBox.Show("属性编码不能为空,行号:" + i, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                //return false;
                            }
                            col.Code = r.Cells[1].EditedFormattedValue.ToString();
                            if (r.Cells[2].EditedFormattedValue == null)
                            {
                                r.Cells[2].Value = string.Empty;
                            }
                            col.Name = r.Cells[2].EditedFormattedValue.ToString();
                            string strDataType = string.Empty;
                            if (r.Cells[3].EditedFormattedValue != null)
                            {
                                strDataType = r.Cells[3].EditedFormattedValue.ToString();
                            }
                            if (string.IsNullOrEmpty(strDataType))
                            {
                                MessageBox.Show("属性类型不能为空,属性:" + col.Name, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                // return false;
                            }
                            //基础类型
                            //引用类型
                            //聚合类型
                            if (strDataType == "基础类型")
                            {
                                col.DataType = DataTypeEnum.CommonType;
                            }
                            else if (strDataType == "引用类型")
                            {
                                col.DataType = DataTypeEnum.RefreceType;
                            }
                            else
                            {
                                col.DataType = DataTypeEnum.CompositionType;
                            }
                            if (r.Cells[4].EditedFormattedValue == null || string.IsNullOrEmpty(r.Cells[4].EditedFormattedValue.ToString()))
                            {
                                MessageBox.Show("属性类型名称不能为空,属性:" + col.Name, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                // return false;
                            }
                            col.TypeString = r.Cells[4].EditedFormattedValue.ToString();
                            col.IsNull = (bool)r.Cells[5].EditedFormattedValue;
                            col.IsViewer = (bool)r.Cells[9].EditedFormattedValue;
                            col.IsBizKey = (bool)r.Cells[10].EditedFormattedValue;

                            col.RefGuid = r.Cells[6].EditedFormattedValue.ToString();

                            if (string.IsNullOrEmpty(col.RefGuid) && col.TypeString == "string")
                            {
                                try
                                {
                                    if (string.IsNullOrEmpty(r.Cells[7].EditedFormattedValue.ToString()))
                                    {
                                        col.Length = 100;
                                    }
                                    else
                                    {
                                        col.Length = int.Parse(r.Cells[7].EditedFormattedValue.ToString());
                                    }
                                }
                                catch (Exception ex)
                                {
                                    col.Length = 100;
                                }
                            }
                            else
                            {
                                col.Length = 0;
                            }
                        }
                    }

                }
                #endregion

                #region DTO类型
                else if (this._bindDataNode.NodeType == NodeType.DTOEntity)
                {
                    DTOEntity prevEntity = this._currentProj.Get(this._collectDataNode.Guid) as DTOEntity;
                    if (prevEntity != null)
                    {
                        prevEntity.Code = this.tbCode.Text;
                        prevEntity.Name = this.tbDisplayName.Text;
                        prevEntity.InhertGuid = this.tbInhertGuid.Text;
                        prevEntity.InhertName = this.tbInhertClass.Text;
                        foreach (DTOEntity entity in this._currentProj.DTOList)
                        {
                            if (entity.Guid != prevEntity.Guid)
                            {
                                if (prevEntity.Code == entity.Code)
                                {
                                    MessageBox.Show("当前DTO的编码不能与项目中已有的DTO编码相同,DTO名称:" + entity.Name, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    //   return false;
                                }
                            }
                        }
                        for (int i = 0; i < this.dataGridView1.Rows.Count; i++)
                        {
                            DataGridViewRow r = this.dataGridView1.Rows[i];
                            if (r.Cells[1].EditedFormattedValue == null || string.IsNullOrEmpty(r.Cells[1].EditedFormattedValue.ToString()))
                            {
                                continue;
                            }
                            string guid = r.Cells[0].Value == null ? string.Empty : r.Cells[0].Value.ToString();
                            DTOColumn col = prevEntity.Get(guid);
                            if (col == null)
                            {
                                col = new DTOColumn(r.Cells[0].Value.ToString(), this._currentProj, prevEntity);
                                prevEntity.ColumnList.Add(col);
                            }
                            if (r.Cells[1].EditedFormattedValue == null || string.IsNullOrEmpty(r.Cells[1].EditedFormattedValue.ToString()))
                            {
                                MessageBox.Show("属性编码不能为空,行号:" + i, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                //  return false;
                            }
                            col.Code = r.Cells[1].EditedFormattedValue.ToString();
                            if (r.Cells[2].EditedFormattedValue == null)
                            {
                                r.Cells[2].Value = string.Empty;
                            }
                            col.Name = r.Cells[2].EditedFormattedValue.ToString();
                            string strDataType = string.Empty;
                            if (r.Cells[3].EditedFormattedValue != null)
                            {
                                strDataType = r.Cells[3].EditedFormattedValue.ToString();
                            }
                            if (string.IsNullOrEmpty(strDataType))
                            {
                                MessageBox.Show("属性类型不能为空,属性:" + col.Name, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                //  return false;
                            }
                            //基础类型
                            //引用类型
                            //聚合类型
                            if (strDataType == "基础类型")
                            {
                                col.DataType = DataTypeEnum.CommonType;
                            }
                            else if (strDataType == "引用类型")
                            {
                                col.DataType = DataTypeEnum.RefreceType;
                            }
                            else
                            {
                                col.DataType = DataTypeEnum.CompositionType;
                            }
                            if (r.Cells[4].EditedFormattedValue == null || string.IsNullOrEmpty(r.Cells[4].EditedFormattedValue.ToString()))
                            {
                                MessageBox.Show("属性类型名称不能为空,属性:" + col.Name, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                //   return false;
                            }
                            col.TypeString = r.Cells[4].EditedFormattedValue.ToString();
                            col.IsNull = (bool)r.Cells[5].EditedFormattedValue;
                            col.IsViewer = (bool)r.Cells[9].EditedFormattedValue;
                            col.RefGuid = r.Cells[6].EditedFormattedValue.ToString();
                        }
                    }
                }
                #endregion

                #region BP类型
                else if (this._bindDataNode.NodeType == NodeType.BPEntity)
                {
                    BPEntity prevEntity = this._currentProj.Get(this._collectDataNode.Guid) as BPEntity;
                    if (prevEntity != null)
                    {
                        prevEntity.Code = this.tbCode.Text;
                        prevEntity.Name = this.tbDisplayName.Text;
                        prevEntity.ReturnGuid = this.tbInhertGuid.Text;
                        prevEntity.ReturnName = this.tbReturnType.Text;
                        prevEntity.IsList = this.cbList.Checked;
                        prevEntity.IsAuth = this.cbAuth.Checked;
                        prevEntity.Trans = this.cbTrans.SelectedIndex;
                        prevEntity.IsEntity = this.ckIsEntity.Checked;
                        foreach (BPEntity entity in this._currentProj.EntityList)
                        {
                            if (entity.Guid != prevEntity.Guid)
                            {
                                if (prevEntity.Code == entity.Code)
                                {
                                    MessageBox.Show("当前BP的编码不能与项目中已有的BP编码相同,BP名称:" + entity.Name, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    //   return false;
                                }
                            }
                        }
                        for (int i = 0; i < this.dataGridView1.Rows.Count; i++)
                        {
                            DataGridViewRow r = this.dataGridView1.Rows[i];
                            if (r.Cells[1].EditedFormattedValue == null || string.IsNullOrEmpty(r.Cells[1].EditedFormattedValue.ToString()))
                            {
                                continue;
                            }
                            string guid = r.Cells[0].Value == null ? string.Empty : r.Cells[0].Value.ToString();
                            BPColumn col = prevEntity.Get(guid);
                            if (col == null)
                            {
                                col = new BPColumn(r.Cells[0].Value.ToString(), this._currentProj as BPProj, prevEntity);
                                prevEntity.ColumnList.Add(col);
                            }
                            if (r.Cells[1].EditedFormattedValue == null || string.IsNullOrEmpty(r.Cells[1].EditedFormattedValue.ToString()))
                            {
                                MessageBox.Show("属性编码不能为空,行号:" + i, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                //  return false;
                            }
                            col.Code = r.Cells[1].EditedFormattedValue.ToString();
                            if (r.Cells[2].EditedFormattedValue == null)
                            {
                                r.Cells[2].Value = string.Empty;
                            }
                            col.Name = r.Cells[2].EditedFormattedValue.ToString();
                            string strDataType = string.Empty;
                            if (r.Cells[3].EditedFormattedValue != null)
                            {
                                strDataType = r.Cells[3].EditedFormattedValue.ToString();
                            }
                            if (string.IsNullOrEmpty(strDataType))
                            {
                                MessageBox.Show("属性类型不能为空,属性:" + col.Name, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                //  return false;
                            }
                            //基础类型
                            //引用类型
                            //聚合类型
                            if (strDataType == "基础类型")
                            {
                                col.DataType = DataTypeEnum.CommonType;
                            }
                            else if (strDataType == "引用类型")
                            {
                                col.DataType = DataTypeEnum.RefreceType;
                            }
                            else
                            {
                                col.DataType = DataTypeEnum.CompositionType;
                            }
                            if (r.Cells[4].EditedFormattedValue == null || string.IsNullOrEmpty(r.Cells[4].EditedFormattedValue.ToString()))
                            {
                                MessageBox.Show("属性类型名称不能为空,属性:" + col.Name, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                //   return false;
                            }
                            col.TypeString = r.Cells[4].EditedFormattedValue.ToString();
                            col.IsNull = (bool)r.Cells[5].EditedFormattedValue;
                            col.RefGuid = r.Cells[6].EditedFormattedValue.ToString();
                        }
                    }

                }
                #endregion

                #region 枚举类型
                else if (this._bindDataNode.NodeType == NodeType.EnumEntity)
                {
                    EnumEntity prevEntity = this._currentProj.Get(this._collectDataNode.Guid) as EnumEntity;
                    if (prevEntity != null)
                    {
                        prevEntity.Code = this.tbCode.Text;
                        prevEntity.Name = this.tbDisplayName.Text;

                        foreach (EnumEntity entity in this._currentProj.EnumList)
                        {
                            if (entity.Guid != prevEntity.Guid)
                            {
                                if (prevEntity.Code == entity.Code)
                                {
                                    MessageBox.Show("当前Enum的编码不能与项目中已有的Enum编码相同,DTO名称:" + entity.Name, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    //   return false;
                                }
                            }
                        }
                        for (int i = 0; i < this.dataGridView1.Rows.Count; i++)
                        {
                            DataGridViewRow r = this.dataGridView1.Rows[i];
                            if (r.Cells[1].EditedFormattedValue == null || string.IsNullOrEmpty(r.Cells[1].EditedFormattedValue.ToString()))
                            {
                                continue;
                            }
                            string guid = r.Cells[0].Value == null ? string.Empty : r.Cells[0].Value.ToString();
                            EnumColumn col = prevEntity.Get(guid);
                            if (col == null)
                            {
                                col = new EnumColumn(r.Cells[0].Value.ToString(), this._currentProj, prevEntity);
                                prevEntity.ColumnList.Add(col);
                            }
                            if (r.Cells[1].EditedFormattedValue == null || string.IsNullOrEmpty(r.Cells[1].EditedFormattedValue.ToString()))
                            {
                                MessageBox.Show("属性编码不能为空,行号:" + i, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                //  return false;
                            }
                            col.Code = r.Cells[1].EditedFormattedValue.ToString();
                            if (r.Cells[2].EditedFormattedValue == null)
                            {
                                r.Cells[2].Value = string.Empty;
                            }
                            col.Name = r.Cells[2].EditedFormattedValue.ToString();
                            col.EnumValue = int.Parse(r.Cells[8].EditedFormattedValue.ToString());
                        }
                    }
                }
                #endregion
            }
            catch (Exception ex)
            {
                MessageBox.Show("录入数据有误！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            return true;
        }
        private bool DataBind()
        {
            this.tbNameSpace.Text = string.Empty;
            //数据清空，重新收集
            this.dataGridView1.Rows.Clear();

            this.cbAuth.Visible = false;
            this.cbTrans.Visible = false;
            this.cbViewRange.Visible = false;
            this.cbOrgFilter.Visible = false;
            if (this._currentProjNode == null || this._currentProj == null)
            {
                return true;
            }
            this.tbNameSpace.Text = this._currentProj.Namespace;
            if (this._bindDataNode == null) return true;

            if (this._bindDataNode.NodeType == NodeType.BPEntity)
            {
                this.btnInhert.Text = "返回值";
                this.btnClearInhert.Text = "清除返回值";
            }
            else
            {
                this.btnInhert.Text = "基类";
                this.btnClearInhert.Text = "清除继承";
            }

            try
            {
                #region 实体类型
                if (this._bindDataNode.NodeType == NodeType.BEEntity)
                {
                    BEEntity currentEntity = this._currentProj.Get(this._bindDataNode.Guid) as BEEntity;
                    this.cbViewRange.Visible = true;
                    this.cbOrgFilter.Visible = true;
                    this.cbOrgFilter.Checked = currentEntity.OrgFilter;
                    if (this.cbOrgFilter.Checked)
                    {
                        this.cbViewRange.Enabled = true;
                    }
                    else
                    {
                        this.cbViewRange.Enabled = false;
                    }
                    this.cbViewRange.SelectedIndex = (int)currentEntity.ViewRange;
                    this.tbCode.Text = currentEntity.Code;
                    this.tbDisplayName.Text = currentEntity.Name;
                    this.tbTableName.Text = currentEntity.TableName;
                    this.tbInhertGuid.Text = currentEntity.InhertGuid;
                    this.tbInhertClass.Text = currentEntity.InhertName;
                    for (int i = 0; i < currentEntity.ColumnList.Count; i++)
                    {
                        BEColumn col = currentEntity.ColumnList[i];
                        DataGridViewRow row = new DataGridViewRow();  //新建一个DataGridViewRow 

                        foreach (DataGridViewColumn c in this.dataGridView1.Columns)
                        {
                            row.Cells.Add(c.CellTemplate.Clone() as DataGridViewCell);//给行添加单元格
                        }
                        row.Cells[0].Value = col.Guid;
                        row.Cells[1].Value = col.Code;  //给DataGridViewRow添加数据
                        row.Cells[2].Value = col.Name;  //给DataGridViewRow添加数据
                        string strDataType = "";
                        //基础类型
                        //引用类型
                        //聚合类型
                        if (col.DataType == DataTypeEnum.CommonType)
                        {
                            strDataType = "基础类型";
                        }
                        else if (col.DataType == DataTypeEnum.RefreceType)
                        {
                            strDataType = "引用类型";
                        }
                        else
                        {
                            strDataType = "聚合类型";
                        }

                        row.Cells[3].Value = strDataType;  //给DataGridViewRow添加数据
                        row.Cells[4].Value = col.TypeString;
                        row.Cells[5].Value = col.IsNull;
                        row.Cells[6].Value = col.RefGuid;
                        row.Cells[9].Value = col.IsViewer;
                        row.Cells[10].Value = col.IsBizKey;
                        //如果是自动生成的列，不能编辑
                        if (!string.IsNullOrEmpty(col.RefColGuid))
                        {
                            row.Cells[1].ReadOnly = true;
                            row.Cells[2].ReadOnly = true;
                            row.Cells[3].ReadOnly = true;
                            row.Cells[4].ReadOnly = true;
                            row.Cells[6].ReadOnly = true;
                            row.Cells[10].ReadOnly = true;
                            row.DefaultCellStyle.BackColor = Color.Yellow;
                        }
                        if (string.IsNullOrEmpty(col.RefGuid) && col.TypeString == "string")
                        {
                            row.Cells[7].ReadOnly = false;
                            if (col.Length <= 0) col.Length = 100;
                            row.Cells[7].Value = col.Length;
                        }
                        else
                        {
                            row.Cells[7].ReadOnly = true;
                            row.Cells[7].Value = string.Empty;
                            col.Length = 0;
                        }

                        this.dataGridView1.Rows.Add(row);   //添加进DataGridView 
                    }
                }
                #endregion

                #region DTO类型
                else if (this._bindDataNode.NodeType == NodeType.DTOEntity)
                {
                    DTOEntity currentEntity = this._currentProj.Get(this._bindDataNode.Guid) as DTOEntity;

                    this.tbCode.Text = currentEntity.Code;
                    this.tbDisplayName.Text = currentEntity.Name;
                    this.tbInhertGuid.Text = currentEntity.InhertGuid;
                    this.tbInhertClass.Text = currentEntity.InhertName;
                    for (int i = 0; i < currentEntity.ColumnList.Count; i++)
                    {
                        DTOColumn col = currentEntity.ColumnList[i];
                        DataGridViewRow row = new DataGridViewRow();  //新建一个DataGridViewRow 

                        foreach (DataGridViewColumn c in this.dataGridView1.Columns)
                        {
                            row.Cells.Add(c.CellTemplate.Clone() as DataGridViewCell);//给行添加单元格
                        }
                        row.Cells[0].Value = col.Guid;
                        row.Cells[1].Value = col.Code;  //给DataGridViewRow添加数据
                        row.Cells[2].Value = col.Name;  //给DataGridViewRow添加数据
                        string strDataType = "";
                        //基础类型
                        //引用类型
                        //聚合类型
                        if (col.DataType == DataTypeEnum.CommonType)
                        {
                            strDataType = "基础类型";
                        }
                        else if (col.DataType == DataTypeEnum.RefreceType)
                        {
                            strDataType = "引用类型";
                        }
                        else
                        {
                            strDataType = "聚合类型";
                        }

                        row.Cells[3].Value = strDataType;  //给DataGridViewRow添加数据
                        row.Cells[4].Value = col.TypeString;
                        row.Cells[5].Value = col.IsNull;
                        row.Cells[6].Value = col.RefGuid;
                        row.Cells[9].Value = col.IsViewer;
                        this.dataGridView1.Rows.Add(row);   //添加进DataGridView 
                    }
                }
                #endregion

                #region BP类型
                else if (this._bindDataNode.NodeType == NodeType.BPEntity)
                {
                    BPEntity currentEntity = this._currentProj.Get(this._bindDataNode.Guid) as BPEntity;
                    this.cbAuth.Visible = true;
                    this.cbViewRange.Visible = false;
                    this.cbOrgFilter.Visible = false;
                    this.cbTrans.Visible = true;
                    this.tbCode.Text = currentEntity.Code;
                    this.tbDisplayName.Text = currentEntity.Name;
                    this.tbInhertGuid.Text = currentEntity.ReturnGuid;
                    this.tbReturnType.Text = currentEntity.ReturnName;
                    this.tbInhertClass.Text = currentEntity.ReturnName;
                    if (currentEntity.IsList)
                    {
                        this.tbInhertClass.Text = "List<" + currentEntity.ReturnName + ">";
                    }
                    this.cbList.Checked = currentEntity.IsList;
                    this.cbAuth.Checked = currentEntity.IsAuth;
                    this.cbTrans.SelectedIndex = currentEntity.Trans;
                    this.ckIsEntity.Checked = currentEntity.IsEntity;
                    for (int i = 0; i < currentEntity.ColumnList.Count; i++)
                    {
                        BPColumn col = currentEntity.ColumnList[i];
                        DataGridViewRow row = new DataGridViewRow();  //新建一个DataGridViewRow 

                        foreach (DataGridViewColumn c in this.dataGridView1.Columns)
                        {
                            row.Cells.Add(c.CellTemplate.Clone() as DataGridViewCell);//给行添加单元格
                        }
                        row.Cells[0].Value = col.Guid;
                        row.Cells[1].Value = col.Code;  //给DataGridViewRow添加数据
                        row.Cells[2].Value = col.Name;  //给DataGridViewRow添加数据
                        string strDataType = "";
                        //基础类型
                        //引用类型
                        //聚合类型
                        if (col.DataType == DataTypeEnum.CommonType)
                        {
                            strDataType = "基础类型";
                        }
                        else if (col.DataType == DataTypeEnum.RefreceType)
                        {
                            strDataType = "引用类型";
                        }
                        else
                        {
                            strDataType = "聚合类型";
                        }

                        row.Cells[3].Value = strDataType;  //给DataGridViewRow添加数据
                        row.Cells[4].Value = col.TypeString;
                        row.Cells[5].Value = col.IsNull;
                        row.Cells[6].Value = col.RefGuid;
                        this.dataGridView1.Rows.Add(row);   //添加进DataGridView 
                    }
                }
                #endregion

                else if (this._bindDataNode.NodeType == NodeType.EnumEntity)
                {
                    EnumEntity currentEntity = this._currentProj.Get(this._bindDataNode.Guid) as EnumEntity;

                    this.tbCode.Text = currentEntity.Code;
                    this.tbDisplayName.Text = currentEntity.Name;
                    for (int i = 0; i < currentEntity.ColumnList.Count; i++)
                    {
                        EnumColumn col = currentEntity.ColumnList[i];
                        DataGridViewRow row = new DataGridViewRow();  //新建一个DataGridViewRow 

                        foreach (DataGridViewColumn c in this.dataGridView1.Columns)
                        {
                            row.Cells.Add(c.CellTemplate.Clone() as DataGridViewCell);//给行添加单元格
                        }
                        row.Cells[0].Value = col.Guid;
                        row.Cells[1].Value = col.Code;  //给DataGridViewRow添加数据
                        row.Cells[2].Value = col.Name;  //给DataGridViewRow添加数据
                        row.Cells[8].Value = col.EnumValue;
                        this.dataGridView1.Rows.Add(row);   //添加进DataGridView 
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "错误");
                return false;
            }
            return true;
        }
        /// <summary>
        /// 对于所有实体进行校验
        /// </summary>
        /// <returns></returns>
        private bool Validate()
        {
            return true;
        }

        private IProject GetProject(string guid)
        {
            foreach (IProject proj in this._projectList)
            {
                AbstractPlatformComponent projComponent = proj as AbstractPlatformComponent;
                if (projComponent.Guid == guid)
                {
                    return proj;
                }
            }
            return null;
        }
        #endregion

        #region 生成代码自定义函数
        /// <summary>
        /// 生成单个项目文件的代码
        /// </summary>
        /// <param name="proj"></param>
        private void GenerateProjCodeWithCompile(IProject proj)
        {
            OutPut.OutPutMsg("####################开始生成项目【" + proj.ProjName + ".sln】代码");
            if (proj is BEProj)
            {
                Net.Code.Builder.Build.BEBuild.BuildEntityProj builder = new BuildEntityProj(proj as BEProj);
                builder.BuildCode();
                builder.BuildMSSQL();
                builder.BuildMetaData();
            }
            else
            {
                Net.Code.Builder.Build.BPBuild.BuildBPProj builder = new BuildBPProj(proj as BPProj);
                builder.BuildCode();
                builder.BuildMetaData();
            }
            OutPut.OutPutMsg("####################生成项目【" + proj.ProjName + ".sln】代码成功");
            compilerCode(proj);
        }
        private void GenerateProjCode(IProject proj)
        {
            OutPut.OutPutMsg("####################开始生成项目【" + proj.ProjName + ".sln】代码");
            if (proj is BEProj)
            {
                BuildEntityProj bp = new BuildEntityProj(proj as BEProj);
                bp.BuildCode();
                bp.BuildMSSQL();
                bp.BuildMetaData();
            }
            else
            {
                BuildBPProj bp = new BuildBPProj(proj as BPProj);
                bp.BuildCode();
                bp.BuildMetaData();
            }
            OutPut.OutPutMsg("####################生成项目【" + proj.ProjName + ".sln】代码成功");
            // compilerCode(proj);
        }

        /// <summary>
        /// 自动编译代码
        /// </summary>
        /// <param name="proj"></param>
        private void compilerCode(IProject proj)
        {
            string baseDir = "C:\\Windows\\Microsoft.NET\\Framework\\";
            string[] dirArray = Directory.GetDirectories(baseDir, "v4.0.*");
            if (dirArray == null || dirArray.Length == 0)
            {
                OutPut.OutPutMsg("没有找到v4.0的msbuild,编译代码失败");
                return;
            }
            OutPut.OutPutMsg("####################【" + proj.ProjName + ".sln】开始编译");
            string msbuildDir = dirArray[0] + "\\MSBuild.exe";
            string command = msbuildDir + " " + proj.CodePath + proj.Namespace + "\\" + proj.Namespace + ".sln /property:Configuration=\"Release\"";
            System.Diagnostics.Process p = new System.Diagnostics.Process();
            p.StartInfo.FileName = "cmd.exe";//要执行的程序名称 
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardInput = true;//可能接受来自调用程序的输入信息 
            p.StartInfo.RedirectStandardOutput = true;//由调用程序获取输出信息 
            p.StartInfo.RedirectStandardError = true;
            p.StartInfo.CreateNoWindow = true;//不显示程序窗口 
            p.Start();//启动程序 
            p.StandardInput.WriteLine(command); //10秒后重启（C#中可不好做哦） 
            p.StandardInput.WriteLine("exit");        //不过要记得加上Exit
            //获取CMD窗口的输出信息： 
            string msg = p.StandardOutput.ReadToEnd();
            OutPut.OutPutMsg(msg);
            OutPut.OutPutMsg("####################【" + proj.ProjName + ".sln】编译完成");

        }

        /// <summary>
        /// 批量生成项目文件代码
        /// </summary>
        private void batchGenerateCodeWithCompile()
        {
            foreach (IProject proj in this._projectList)
            {
                this.GenerateProjCodeWithCompile(proj);
            }
        }
        private void batchGenerateCode()
        {
            foreach (IProject proj in this._projectList)
            {
                this.GenerateProjCode(proj);
            }
        }
        #endregion

        #region 树形结构拖拽功能
        private void treeView1_ItemDrag(object sender, ItemDragEventArgs e)
        {
            DoDragDrop(e.Item, DragDropEffects.Move);
        }
        private void treeView1_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(ComponentTreeNode)))
            {
                e.Effect = DragDropEffects.Move;
            }
            else if (e.Data.GetDataPresent(typeof(MetaDataTreeNode)))
            {
                e.Effect = DragDropEffects.Move;
            }
            else
            {
                e.Effect = DragDropEffects.Copy;
            }

        }
        private Point pos = new Point(0, 0);
        private void treeView1_DragDrop(object sender, DragEventArgs e)
        {
            TreeNode node = null;
            if (e.Data.GetDataPresent(typeof(MetaDataTreeNode)))
            {
                node = (MetaDataTreeNode)(e.Data.GetData(typeof(MetaDataTreeNode)));
            }
            else if (e.Data.GetDataPresent(typeof(ComponentTreeNode)))
            {
                node = (ComponentTreeNode)(e.Data.GetData(typeof(ComponentTreeNode)));
            }
            else
            {
                Array aryFiles = ((System.Array)e.Data.GetData(DataFormats.FileDrop));
                if (aryFiles != null)
                {
                    foreach (string file in aryFiles)
                    {
                        LoadProj(file);
                    }
                }
                return;
            }
            pos.X = e.X;
            pos.Y = e.Y;
            pos = tvProject.PointToClient(pos);
            ComponentTreeNode dropNode = this.tvProject.GetNodeAt(pos) as ComponentTreeNode;
            if (dropNode == null) dropNode = this._currentProjNode;
            if (node is MetaDataTreeNode)
            {
                ComponentTreeNode rootNode = GetRootNode(dropNode);
                ComponentTreeNode refrenceNode = rootNode.Nodes[0] as ComponentTreeNode;

                MetaDataTreeNode dragNode = node as MetaDataTreeNode;
                //如果dll已经引用则不需要引用
                foreach (ComponentTreeNode subNode in refrenceNode.Nodes)
                {
                    if (subNode.Text == dragNode.Text) return;
                }
                ComponentTreeNode newNode = new ComponentTreeNode(NodeType.RefrenceDll, Guid.NewGuid().ToString());
                newNode.RefType = dragNode.RefType;
                newNode.Text = dragNode.Text;
                newNode.FileName = dragNode.FileName;
                newNode.ImageIndex = newNode.SelectedImageIndex = 8;
                refrenceNode.Nodes.Add(newNode);
                refrenceNode.Expand();
                this.tvProject.SelectedNode = refrenceNode;

                IProject currentProj = this.GetProject(rootNode.Guid);
                ProjectRefrence projRefrence = new ProjectRefrence(currentProj, newNode.Guid, this.getRelativePath(dragNode.FilePath));
                projRefrence.RefProjName = newNode.FileName;
                projRefrence.RefrenceType = newNode.RefType;
                projRefrence.Code = projRefrence.Name = newNode.Text;
                currentProj.RefrenceList.Add(projRefrence);
            }
            else if (node is ComponentTreeNode)
            {
                ComponentTreeNode dragNode = node as ComponentTreeNode;
                // 如果目标节点不存在，即拖拽的位置不存在节点，那么就将被拖拽节点放在根节点之下
                if (node is ComponentTreeNode)
                {
                    if (dragNode.NodeType == NodeType.BEProj || dragNode.NodeType == NodeType.BPProj)
                    {
                        return;
                    }
                }
                //不能将节点拖拽到实体节点上
                if (dropNode.NodeType == NodeType.BEEntity
                    || dropNode.NodeType == NodeType.BPEntity
                    || dropNode.NodeType == NodeType.Refrence
                    || dropNode.NodeType == NodeType.RefrenceDll
                    || dropNode.NodeType == NodeType.EnumEntity)
                {
                    return;
                }

                ComponentTreeNode rootNode = GetRootNode(dropNode);
                //不能够跨项目进行拖拽
                AbstractPlatformComponent currentProj = this._currentProj as AbstractPlatformComponent;
                if (rootNode.Guid != currentProj.Guid)
                {
                    return;
                }
                // 1.目标节点不是空。2.目标节点不是被拖拽接点的字节点。3.目标节点不是被拖拽节点本身
                if (dropNode != null && dropNode.Parent != node && dropNode != node)
                {
                    // 将被拖拽节点从原来位置删除。
                    node.Remove();
                    // 在目标节点下增加被拖拽节点
                    dropNode.Nodes.Add(dragNode);
                }
                if (dropNode.NodeType == NodeType.BEProj || dropNode.NodeType == NodeType.BPProj)
                {
                    if (dragNode.NodeType == NodeType.BEEntity)
                    {
                        BEEntity entity = this._currentProj.Get(dragNode.Guid) as BEEntity;
                        if (entity != null)
                        {
                            entity.FloderGuid = string.Empty;
                        }
                    }
                    else if (dragNode.NodeType == NodeType.BPEntity)
                    {
                        BPEntity entity = this._currentProj.Get(dragNode.Guid) as BPEntity;
                        if (entity != null)
                        {
                            entity.FloderGuid = string.Empty;
                        }
                    }
                    else if (dragNode.NodeType == NodeType.DTOEntity)
                    {
                        DTOEntity entity = this._currentProj.Get(dragNode.Guid) as DTOEntity;
                        if (entity != null)
                        {
                            entity.FloderGuid = string.Empty;
                        }
                    }
                    else if (dragNode.NodeType == NodeType.EnumEntity)
                    {
                        EnumEntity entity = this._currentProj.Get(dragNode.Guid) as EnumEntity;
                        if (entity != null)
                        {
                            entity.FloderGuid = string.Empty;
                        }
                    }
                    else if (dragNode.NodeType == NodeType.Floder)
                    {
                        Floder floder = this._currentProj.GetFloder(dragNode.Guid);
                        floder.PGuid = string.Empty;
                    }

                }
                else if (dropNode.NodeType == NodeType.Floder)
                {
                    if (dragNode.NodeType == NodeType.BEEntity)
                    {
                        BEEntity entity = this._currentProj.Get(dragNode.Guid) as BEEntity;
                        if (entity != null)
                        {
                            entity.FloderGuid = dropNode.Guid;
                        }
                    }
                    else if (dragNode.NodeType == NodeType.BPEntity)
                    {
                        BPEntity entity = this._currentProj.Get(dragNode.Guid) as BPEntity;
                        if (entity != null)
                        {
                            entity.FloderGuid = dropNode.Guid;
                        }
                    }
                    else if (dragNode.NodeType == NodeType.DTOEntity)
                    {
                        DTOEntity entity = this._currentProj.Get(dragNode.Guid) as DTOEntity;
                        if (entity != null)
                        {
                            entity.FloderGuid = dropNode.Guid;
                        }
                    }
                    else if (dragNode.NodeType == NodeType.EnumEntity)
                    {
                        EnumEntity entity = this._currentProj.Get(dragNode.Guid) as EnumEntity;
                        if (entity != null)
                        {
                            entity.FloderGuid = dropNode.Guid;
                        }
                    }
                    else if (dragNode.NodeType == NodeType.Floder)
                    {
                        Floder floder = this._currentProj.GetFloder(dragNode.Guid);
                        floder.PGuid = dropNode.Guid;
                    }
                }
            }
        }

        private static ComponentTreeNode GetRootNode(ComponentTreeNode node)
        {
            ComponentTreeNode ctn = node;
            while (true)
            {
                if (ctn == null) break;
                if (ctn.NodeType == NodeType.BEProj || ctn.NodeType == NodeType.BPProj)
                {
                    break;
                }
                ctn = ctn.Parent as ComponentTreeNode;
            }
            return ctn;
        }
        private ComponentTreeNode GetRootNode(string guid)
        {
            foreach (TreeNode node in this.tvProject.Nodes)
            {
                ComponentTreeNode tn = node as ComponentTreeNode;
                if (tn.Guid == guid)
                {
                    return tn;
                }
            }
            return null;
        }

        private void tvMetaData_ItemDrag(object sender, ItemDragEventArgs e)
        {
            DoDragDrop(e.Item, DragDropEffects.Move);
        }
        #endregion

        #region 加载元数据函数

        private void CodeGenerater_Load(object sender, EventArgs e)
        {
            Thread t = new Thread(new ThreadStart(this.LoadMetaData));
            t.IsBackground = true;
            t.Start();
        }
        /// <summary>
        /// 刷新菜单调用函数
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tsm_Refresh_Click(object sender, EventArgs e)
        {
            Thread t = new Thread(new ThreadStart(this.LoadMetaData));
            t.IsBackground = true;
            t.Start();
        }
        private void LoadMetaData()
        {
            List<MetaData> metaDataList = new List<MetaData>();
            foreach (string filePath in CodeGeneraterWin.MetaDataPathList)
            {
                if (!System.IO.Directory.Exists(filePath))
                {
                    continue;
                }
                string[] files = Directory.GetFiles(filePath, "*.b?");
                if (files != null)
                {
                    foreach (string file in files)
                    {
                        XmlDocument xmlDoc = new XmlDocument();
                        xmlDoc.Load(file);
                        var node = xmlDoc.FirstChild;
                        if (node != null)
                        {
                            string strNameSpace = node.Attributes["namespace"].Value;

                            if (node.Name == "EntityProj")
                            {
                                MetaData meta = new MetaData();
                                meta.FullName = strNameSpace + ".dll";
                                meta.FilePath = file;
                                meta.NodeType = RefType.BEEntity;
                                metaDataList.Add(meta);
                                //deploy
                                meta = new MetaData();
                                meta.FullName = strNameSpace + ".Deploy.dll";
                                meta.FilePath = file;
                                meta.NodeType = RefType.Deploy;
                                metaDataList.Add(meta);
                            }
                            else if (node.Name == "BPProj")
                            {
                                MetaData meta = new MetaData();
                                meta.FullName = strNameSpace + ".dll";
                                meta.FilePath = file;
                                meta.NodeType = RefType.BPEntity;
                                metaDataList.Add(meta);
                                //deploy
                                meta = new MetaData();
                                meta.FullName = strNameSpace + ".Deploy.dll";
                                meta.FilePath = file;
                                meta.NodeType = RefType.Deploy;
                                metaDataList.Add(meta);
                                //agent
                                meta = new MetaData();
                                meta.FullName = strNameSpace + ".Agent.dll";
                                meta.FilePath = file;
                                meta.NodeType = RefType.Agent;
                                metaDataList.Add(meta);
                            }
                        }
                    }
                }
            }
            this.RefreshEventDelegate = new RefreshMetaDataHandler(this.RefreshMetaData);
            this.tvMetaData.Invoke(this.RefreshEventDelegate, metaDataList);
        }
        private delegate void RefreshMetaDataHandler(List<MetaData> metaDataList);
        private RefreshMetaDataHandler RefreshEventDelegate;
        private void RefreshMetaData(List<MetaData> metaDataList)
        {
            this.tvMetaData.BeginUpdate();
            this.tvMetaData.Nodes.Clear();
            foreach (MetaData meta in metaDataList)
            {
                MetaDataTreeNode node = new MetaDataTreeNode(meta.NodeType, meta.FilePath);
                node.Text = meta.FullName;
                node.ImageIndex = node.SelectedImageIndex = 8;
                this.tvMetaData.Nodes.Add(node);
            }
            this.tvMetaData.EndUpdate();
        }
        private class MetaData
        {
            public string FullName = string.Empty;
            public string NameSpace = string.Empty;
            public string FilePath = string.Empty;
            public RefType NodeType = RefType.None;
        }

        #endregion

        #region 输出信息相关操作

        private void tsmi_Clear_Click(object sender, EventArgs e)
        {
            this.tbOutPut.Text = string.Empty;
        }

        private void tsmi_SelectAll_Click(object sender, EventArgs e)
        {
            this.tbOutPut.Select();
            this.tbOutPut.SelectAll();
        }

        #endregion

        private void cbOrgFilter_CheckedChanged(object sender, EventArgs e)
        {
            if (!this.cbOrgFilter.Checked)
            {
                this.cbViewRange.SelectedIndex = 0;
                this.cbViewRange.Enabled = false;
            }
            else
            {
                this.cbViewRange.SelectedIndex = 2;
                this.cbViewRange.Enabled = true;
            }
        }


        private string getRelativePath(string toPath)
        {
            string fromPath = System.IO.Directory.GetCurrentDirectory() + @"\";// AppDomain.CurrentDomain.SetupInformation.ApplicationBase;

            if (String.IsNullOrEmpty(toPath)) throw new ArgumentNullException("toPath");

            Uri fromUri = new Uri(fromPath);
            Uri toUri = new Uri(toPath);

            Uri relativeUri = fromUri.MakeRelativeUri(toUri);
            String relativePath = Uri.UnescapeDataString(relativeUri.ToString());

            return relativePath.Replace('/', Path.DirectorySeparatorChar);
        }


    }
}
