using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace AutoCodeToComments
{
    public partial class Form1 : Form
    {
        private DataTable dt;
        public Form1()
        {
            InitializeComponent();
        }

        private void btn_SelectFile_Click(object sender, EventArgs e)
        {
            OpenFileDialog OFD = new OpenFileDialog();
            if (OFD.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                tb_FilePath.Text = OFD.FileName;
            }
        }

        private void btn_LoadData_Click(object sender, EventArgs e)
        {
            string path = tb_FilePath.Text;
            if (string.IsNullOrEmpty(path)) return;

            dt = new DataTable();
            dt.Columns.Add("ID");
            dt.Columns.Add("TABLE_NAME");
            dt.Columns.Add("COLUMN_NAME");
            dt.Columns.Add("COMMENTS");
            int i = 0;
            String line = "";
            try
            {

                StreamReader sr = new StreamReader(path, Encoding.Default);


                while ((line = sr.ReadLine()) != null)
                {
                    //Console.WriteLine(line.ToString());
                    string[] arr = line.Replace("\"", "").Split(',');
                    if (arr.Length == 1)
                        arr = line.Split('\t');
                    DataRow dr = dt.NewRow();
                    dr["ID"] = arr[0];
                    dr["TABLE_NAME"] = arr[1];
                    dr["COLUMN_NAME"] = arr[2];
                    dr["COMMENTS"] = arr[3];
                    dt.Rows.Add(dr);
                    i++;
                }

                dgv_List.DataSource = dt;
            }
            catch (Exception ex)
            {
                MessageBox.Show("索引行: " + i.ToString() + "\n行内容:" + line + "\n错误信息: " + ex.Message);
                //throw;
            }
        }

        private void btn_CreateCode_Click(object sender, EventArgs e)
        {
            if (dt != null && dt.Rows.Count > 0)
            {
                string strCodes = "";

                var dt1 = (DataTable)dgv_List.DataSource;
                var dv = dt1.DefaultView;
                dt = dv.ToTable();

                if (cbb_Type.SelectedItem.ToString() == "table")
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        if (!string.IsNullOrEmpty(dr["COMMENTS"].ToString()))
                            strCodes += string.Format("comment on table \"{0}\"  is '{1}';\n", dr["TABLE_NAME"], dr["COMMENTS"]);
                    }
                }
                else
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        if (!string.IsNullOrEmpty(dr["COMMENTS"].ToString()))
                            strCodes += string.Format("comment on column \"{0}\".\"{1}\"  is '{2}';\n", dr["TABLE_NAME"], dr["COLUMN_NAME"], dr["COMMENTS"]);
                    }
                }

                rtb_View.Text = strCodes;
                tabControl1.SelectTab(1);

            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            cbb_Type.SelectedIndex = 0;
        }
    }
}
