using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using Py.RunTime;
using Py.Core;

namespace 生成计算公式和计算 {
    public partial class Form1 :Form {
        public Form1() {
            InitializeComponent();
        }



        Arithmetic _am = new Arithmetic();

        private void button1_Click(object sender, EventArgs e) {
            _am.Vars["时间"] = QC.Int(textBox2.Text);
            _am.Vars["路径"] = QC.Int(textBox3.Text);
            object r = _am.Compute(textBox1.Text);
            label4.Text = "结果  " + r.ToString(  );
        }
    }
}
