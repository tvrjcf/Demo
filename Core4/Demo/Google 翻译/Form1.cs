using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Xuld;

namespace Google_翻译 {
    public partial class Form1 :Form {
        public Form1() {
            InitializeComponent();
        }

        GoogleTranslate _t = new GoogleTranslate();

        private void textBox1_TextChanged(object sender, EventArgs e) {
            if(textBox1.Text.Length == 0)
                return;
            char c = textBox1.Text[textBox1.Text.Length - 1];
            if (char.IsWhiteSpace(c))
                textBox2.Text = _t.Translate(textBox1.Text, true);
            else if(Py.Core.Check.IsChinese(textBox1.Text))
                textBox2.Text = _t.Translate(textBox1.Text, true);
        }
    }
}
