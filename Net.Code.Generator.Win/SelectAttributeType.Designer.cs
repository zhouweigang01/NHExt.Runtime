namespace Net.Code.Generator.Win
{
    partial class SelectAttributeType
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.lv_TypeList = new System.Windows.Forms.ListView();
            this.ch_TypeName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.ch_TypeMemo = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.cb_metadataFile = new System.Windows.Forms.ComboBox();
            this.Label1 = new System.Windows.Forms.Label();
            this.ck_List = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // lv_TypeList
            // 
            this.lv_TypeList.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.ch_TypeName,
            this.ch_TypeMemo});
            this.lv_TypeList.FullRowSelect = true;
            this.lv_TypeList.Location = new System.Drawing.Point(8, 31);
            this.lv_TypeList.MultiSelect = false;
            this.lv_TypeList.Name = "lv_TypeList";
            this.lv_TypeList.Size = new System.Drawing.Size(625, 362);
            this.lv_TypeList.TabIndex = 2;
            this.lv_TypeList.UseCompatibleStateImageBehavior = false;
            this.lv_TypeList.View = System.Windows.Forms.View.Details;
            this.lv_TypeList.DoubleClick += new System.EventHandler(this.lv_TypeList_DoubleClick);
            // 
            // ch_TypeName
            // 
            this.ch_TypeName.Text = "类型名称";
            this.ch_TypeName.Width = 300;
            // 
            // ch_TypeMemo
            // 
            this.ch_TypeMemo.Text = "类型全名";
            this.ch_TypeMemo.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.ch_TypeMemo.Width = 300;
            // 
            // cb_metadataFile
            // 
            this.cb_metadataFile.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cb_metadataFile.FormattingEnabled = true;
            this.cb_metadataFile.Location = new System.Drawing.Point(60, 5);
            this.cb_metadataFile.Name = "cb_metadataFile";
            this.cb_metadataFile.Size = new System.Drawing.Size(219, 20);
            this.cb_metadataFile.TabIndex = 3;
            this.cb_metadataFile.SelectedIndexChanged += new System.EventHandler(this.cb_metadataFile_SelectedIndexChanged);
            // 
            // Label1
            // 
            this.Label1.AutoSize = true;
            this.Label1.Location = new System.Drawing.Point(12, 8);
            this.Label1.Name = "Label1";
            this.Label1.Size = new System.Drawing.Size(41, 12);
            this.Label1.TabIndex = 4;
            this.Label1.Text = "分类：";
            // 
            // ck_List
            // 
            this.ck_List.AutoSize = true;
            this.ck_List.Location = new System.Drawing.Point(304, 8);
            this.ck_List.Name = "ck_List";
            this.ck_List.Size = new System.Drawing.Size(96, 16);
            this.ck_List.TabIndex = 5;
            this.ck_List.Text = "是否集合类型";
            this.ck_List.UseVisualStyleBackColor = true;
            this.ck_List.Visible = false;
            // 
            // SelectAttributeType
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(639, 400);
            this.Controls.Add(this.ck_List);
            this.Controls.Add(this.Label1);
            this.Controls.Add(this.cb_metadataFile);
            this.Controls.Add(this.lv_TypeList);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SelectAttributeType";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "选择数据类型";
            this.Load += new System.EventHandler(this.SelectAttributeType_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListView lv_TypeList;
        private System.Windows.Forms.ColumnHeader ch_TypeName;
        private System.Windows.Forms.ColumnHeader ch_TypeMemo;
        private System.Windows.Forms.ComboBox cb_metadataFile;
        private System.Windows.Forms.Label Label1;
        private System.Windows.Forms.CheckBox ck_List;
    }
}