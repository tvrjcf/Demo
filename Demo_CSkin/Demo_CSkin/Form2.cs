using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Demo_CSkin
{
    public partial class Form2 : Form
    {
        List<int> ListInt = new List<int>();
        public Form2()
        {
            InitializeComponent();
        }

        private void Form2_Load(object sender, EventArgs e)
        {
        }

        void InitData(){
            ListInt.Clear();
            for (int i = 0; i < 10; i++)
            {
                ListInt.Add(i);
            }


            string removeL = "";
            for (int i = ListInt.Count -1; i >=0; i--)
            {
                removeL += ListInt[i].ToString() + ",";
                MessageBox.Show(ListInt.Count.ToString() + " - i: " + i.ToString() + " - E: " + ListInt[i]);
                ListInt.Remove(ListInt[i]);
            }
            MessageBox.Show(removeL);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //InitData();
            System.Diagnostics.Process.Start("http://www.baidu.com");
        }
    }
}
