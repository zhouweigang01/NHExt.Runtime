using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Net.Code.Generator.Win
{
    public partial class ChangeNameForm : Form
    {
        public ChangeNameForm(string name)
        {
            InitializeComponent();
            this.tbName.Text = name;
        }

        public string InputName
        {
            get { return this.tbName.Text; }
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(this.InputName) || string.IsNullOrEmpty(this.InputName))
            {
                MessageBox.Show("名称不能为空", "提示");
            }
            else
            {
                this.DialogResult = System.Windows.Forms.DialogResult.OK;
                this.Close();
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.Close();
        }
    }
}
