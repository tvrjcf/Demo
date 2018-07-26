using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using Xuld;
using System.Threading;

namespace 百度搜索 {
    public partial class Form1 :Form {
        public Form1() {
            InitializeComponent();

            Py.Logging.Logger.Listerner = new Py.Logging.DebugWindowLogListener();
        }



        BSearch _bs = new BSearch();


        bool _editing = true;

        private void comboBox1_TextChanged(object sender, EventArgs e) {
            if (_editing) {
                if (comboBox1.Items.IndexOf(comboBox1.Text) != -1) {
                } else {
                    _editing = false;
                    for (int i = comboBox1.Items.Count - 1; i >= 1; i--)
                        comboBox1.Items.RemoveAt(i);
                    comboBox1.Items.AddRange(_bs.GetList(comboBox1.Text));
                    _editing = true;
                }
             }
        }

        private void textBox1_TextChanged(object sender, EventArgs e) {
            if (_editing) {
                if (textBox1.AutoCompleteCustomSource.IndexOf(textBox1.Text) != -1) {
                } else {
                    _editing = false;
                    for (int i = textBox1.AutoCompleteCustomSource.Count - 1; i >= 1; i--)
                        textBox1.AutoCompleteCustomSource.RemoveAt(i);
                    textBox1.AutoCompleteCustomSource.AddRange(_bs.GetList(textBox1.Text));
                    _editing = true;
                }
            }
        }

        
    }
}
