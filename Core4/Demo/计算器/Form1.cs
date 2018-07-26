using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using Py.RunTime;

namespace 计算器 {
    public partial class Form1 :Form {
        public Form1() {
            InitializeComponent();
        }


        Arithmetic _arith = new Arithmetic();

        private void button1_Click(object sender, EventArgs e) {

            if (textBox1.Text.Length > 0)
            textBox2.Text = _arith.Compute(textBox1.Text).ToString();
        }
    }
}
