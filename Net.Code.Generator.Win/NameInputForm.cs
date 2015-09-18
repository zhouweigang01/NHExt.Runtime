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
    public partial class NameInputForm : Form
    {
        public NameInputForm()
        {
            InitializeComponent();
        }

        public string InputName
        {
            get { return this.textBox2.Text; }
        }
        public string InputNameSpace
        {
            get { return this.textBox1.Text; }
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(this.InputName) || string.IsNullOrEmpty(this.InputNameSpace))
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
