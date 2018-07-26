using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using Py.Algorithm;

namespace 加密功能 {
    public partial class Form1 :Form {
        public Form1() {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e) {
            var a = new OpenFileDialog();
            a.ShowDialog();
            Encryption.EncryptDES(a.FileName, a.FileName + ".加密", "pa6", "102");
            
        }

        private void button2_Click(object sender, EventArgs e) {
            var a = new OpenFileDialog();
            a.ShowDialog();
            Encryption.DecryptDES(a.FileName, a.FileName + ".解密", "pa6", "102");
        }

        private void button3_Click(object sender, EventArgs e) {
            const string S = "aaa12dfgdg12212";
            MessageBox.Show(String.Format("{0}  ->  {1}",  Encryption.DecryptString( Encryption.EncryptString(S) ), Encryption.EncryptString(S)));
        }

        private void button4_Click(object sender, EventArgs e) {
            MessageBox.Show(Encryption.MD5t("qwewe", true));
        }
    }
}
