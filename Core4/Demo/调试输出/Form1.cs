using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Py.Logging;

namespace 调试输出 {
    public partial class Form1 :Form {
        public Form1() {
            InitializeComponent();

            radioButton1_CheckedChanged(this, EventArgs.Empty);

        }

        private void button1_Click(object sender, EventArgs e) {
            Logger.Write("日志:   {0}", new int[] { 2, 3, 4 });
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e) {
            Logger.Close();
            Logger.Listerner = new DebugWindowLogListener();
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e) {
            Logger.Close();
            Logger.Listerner = new WinFormControlLogListener((Control)sender);
        }

        private void radioButton3_CheckedChanged(object sender, EventArgs e) {
            Logger.Close();
            Logger.Listerner = new FileLogListener("myfile.log");
            Logger.Listerner.AutoFlush = true;
        }
    }
}
