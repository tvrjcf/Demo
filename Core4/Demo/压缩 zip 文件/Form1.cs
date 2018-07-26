using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Py.Zip;

namespace 压缩_zip_文件 {
    public partial class Form1 :Form {
        public Form1() {
            InitializeComponent();
        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e) {
            var a = new SaveFileDialog();
            a.Filter = "压缩文件(*.zip)|*.zip";
            if (a.ShowDialog() == DialogResult.OK)
                _zip.SaveAs(a.FileName);
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e) {

        }

        ZipFile _zip = new ZipFile();

        private void 打开ToolStripMenuItem_Click(object sender, EventArgs e) {
            var a = new OpenFileDialog();
            a.Filter = "压缩文件(*.zip)|*.zip";
            if (a.ShowDialog() == DialogResult.OK) {
                _zip.Open(a.FileName);
                ShowZipEntry();
            }
        }

        void ShowZipEntry() {


            listView1.LargeImageList = new ImageList();

            listView1.LargeImageList.Images.Add("DIRECTORY", Py.Windows.IconHelper.GetDirectoryIcon(true));




            foreach (ZipEntry ze in _zip.Entries) {
                ListViewItem item = new ListViewItem();

                if (ze.IsDirectory) {

                    item.Name = ze.Name;

                    item.Text = ze.Name.Remove(ze.Name.Length - 1);

                    item.ImageKey = "DIRECTORY";


                } else {


                    item.Text = item.Name = ze.Name;

                    listView1.LargeImageList.Images.Add(item.Name, Py.Windows.IconHelper.GetFileIcon(ze.Name, true));

                    item.ImageKey = item.Name;


                }


                listView1.Items.Add(item);

            }
        }

        private void 保存ToolStripMenuItem_Click(object sender, EventArgs e) {
            _zip.Save();
        }

        private void 关闭ToolStripMenuItem_Click(object sender, EventArgs e) {
            _zip.Close();
            listView1.Items.Clear();
        }

        private void 退出ToolStripMenuItem_Click(object sender, EventArgs e) {
            this.Close();
        }

        private void toolStripMenuItem1_Click_1(object sender, EventArgs e) {
            var a = new FolderBrowserDialog();
            a.SelectedPath = Py.Core.FileHelper.GetDirectory(_zip.FilePath);
            if (a.ShowDialog() == DialogResult.OK)
                _zip.Extract(a.SelectedPath);
        }
    }
}
