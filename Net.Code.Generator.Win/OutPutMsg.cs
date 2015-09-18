using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Net.Code.Generator.Win
{
    class OutPut
    {
        private static TextBox _outputTextBox;

        public static TextBox OutputTextBox
        {
            get { return OutPut._outputTextBox; }
            set { OutPut._outputTextBox = value; }
        }

        private delegate void OutPutMsgDelegate(string msg);

        public static void OutPutMsg(string msg)
        {
            if (_outputTextBox != null)
            {
                lock (_outputTextBox)
                {
                    _outputTextBox.Invoke(new OutPutMsgDelegate(outPutMsg), msg);
                }
            }
        }

        private static void outPutMsg(string msg)
        {
            _outputTextBox.AppendText(msg + Environment.NewLine);
        }
    }
}

//this.menuStrip1.Location = new System.Drawing.Point(0, 0);
//           this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[]{
//               this.tsm_Operate
//           });
//           this.menuStrip1.Name = "menuStrip1";
//           this.menuStrip1.Size = new System.Drawing.Size(948, 25);
//           this.menuStrip1.TabIndex = 12;
//           this.menuStrip1.Text = "menuStrip1";
//           // 
//           // tsm_Operate
//           // 
//           this.tsm_Operate.Name = "tsm_Operate";
//           this.tsm_Operate.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[]{
//           this.tsm_Create,
//               this.tsm_SaveAll,
//               this.tsm_GenerateAll,
//               this.tsm_Close
//           });
//           this.tsm_Operate.Size = new System.Drawing.Size(44, 21);
//           this.tsm_Operate.Text = "操作";