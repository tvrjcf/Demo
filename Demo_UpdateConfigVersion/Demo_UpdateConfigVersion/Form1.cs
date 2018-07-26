using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Demo_UpdateConfigVersion.Model;

namespace Demo_UpdateConfigVersion
{
    public partial class Form1 : Form
    {
        private Config ConfigJson_bak;
        private Config ConfigJson;
        private string FilePath;

        public Form1()
        {
            InitializeComponent();
            DoubleBufferDataGridView.DoubleBufferedDataGirdView(dataGridView1, true);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            FilePath = txt_Path.Text;
            LoadFile(FilePath);
        }

        private void Form1_DragDrop(object sender, DragEventArgs e)
        {
            string path = ((System.Array)e.Data.GetData(DataFormats.FileDrop)).GetValue(0).ToString();

            txt_Path.Text = path;
            FilePath = path;
            LoadFile(FilePath);
        }

        private void txt_Path_TextChanged(object sender, EventArgs e)
        {
            if (txt_Path.Text.Trim().Length == 0)
                txt_Path.Text = "config.json";
        }

        private void Form1_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.All;
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
        }

        private void Form1_DragOver(object sender, DragEventArgs e)
        {

        }

        private void btn_Save_Click(object sender, EventArgs e)
        {
            if (ConfigJson != null)
            {
                SaveFile(FilePath);
            }
        }

        public void BindData()
        {
            if (ConfigJson != null && ConfigJson.apks != null)
            {
                dataGridView1.DataSource = new BindingList<Apk>(ConfigJson.apks);
                dataGridView1.Columns[0].Width = 250;
                dataGridView1.Columns[2].Width = 250;
            }
        }

        private void btn_Read_Click(object sender, EventArgs e)
        {
            FilePath = txt_Path.Text;
            LoadFile(FilePath);
        }

        private void btn_DownVersion_Click(object sender, EventArgs e)
        {
            try
            {
                double num = double.Parse(txt_VersionAdd.Text);
                UpdateVersion(-num);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void btn_UpVersion_Click(object sender, EventArgs e)
        {
            try
            {
                double num = double.Parse(txt_VersionAdd.Text);
                UpdateVersion(num);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        public void LoadFile(string path)
        {
            try
            {//读取json文件  
                using (StreamReader sr = new StreamReader(path))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    serializer.Converters.Add(new JavaScriptDateTimeConverter());
                    serializer.NullValueHandling = NullValueHandling.Ignore;

                    //构建Json.net的读取流  
                    JsonReader reader = new JsonTextReader(sr);
                    //对读取出的Json.net的reader流进行反序列化，并装载到模型中  
                    ConfigJson = serializer.Deserialize<Config>(reader);
                    //ConfigJson_bak = serializer.Deserialize<Config>(reader);
                    tsslb_Message.Text = "读取成功";

                    BindData();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                //ex.Message.ToString();
            }
        }


        public void SaveFile(string path)
        {
            try
            {
                string backup = "";
                //备份
                if (ckb_Backup.Checked)
                {
                    if (File.Exists(path))
                    {
                        FileInfo fi = new FileInfo(path);
                        fi.MoveTo(fi.Name + "_" + DateTime.Now.ToString("yyyyMMddHHssmm"));
                        backup = "(已备份)";
                    }
                }
                //把模型数据写到文件  
                using (StreamWriter sw = new StreamWriter(path))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    serializer.Converters.Add(new JavaScriptDateTimeConverter());
                    serializer.NullValueHandling = NullValueHandling.Ignore;

                    //构建Json.net的写入流  
                    JsonWriter writer = new JsonTextWriter(sw) { Formatting = Formatting.Indented};
                    //把模型数据序列化并写入Json.net的JsonWriter流中  
                    serializer.Serialize(writer, ConfigJson);
                    //ser.Serialize(writer, ht);  
                    writer.Close();
                    sw.Close();
                    tsslb_Message.Text = "感谢我吧, 保存成功了" + backup;
                    MessageBox.Show(tsslb_Message.Text , "纯天然打造,绿色无污染!");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        public void UpdateVersion(double num)
        {
            var rows = dataGridView1.SelectedRows;
            if (rows.Count == 0)
            {
                MessageBox.Show("请选中要修改的行");
                return;
            }
            foreach (DataGridViewRow row in rows)
            {
                if (row.Cells["version"].Value == null)
                    continue;
                var version = row.Cells["version"].Value.ToString();

                row.Cells["version"].Value = Math.Round(double.Parse(version) + num, 3);
            }
        }

        private void txt_FilterRow_KeyUp(object sender, KeyEventArgs e)
        {
            string txt = txt_FilterRow.Text;
            if (txt.Length > 0 && e.KeyCode == Keys.Enter)
            {
                foreach (DataGridViewRow row in dataGridView1.Rows)
                    row.Selected = false;
                foreach (DataGridViewRow row in dataGridView1.Rows)
                {
                    var value = row.Cells[0].Value;
                    if (value != null && value.ToString().ToUpper().Contains(txt.ToUpper()))
                    {
                        row.Selected = true;
                    }
                }
            }
        }
    }
}
