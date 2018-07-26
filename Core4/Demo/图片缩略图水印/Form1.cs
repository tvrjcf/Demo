using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using Py.Drawing;

namespace 图片缩略图水印 {
    public partial class Form1 :Form {
        public Form1() {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e) {


            if (openFileDialog1.ShowDialog() == DialogResult.OK)

           pictureBox1.ImageLocation = openFileDialog1.FileName;
        }

        private void button2_Click(object sender, EventArgs e) {


            if (openFileDialog1.ShowDialog() == DialogResult.OK)

                pictureBox2.ImageLocation = openFileDialog1.FileName;
        }

        private void button3_Click(object sender, EventArgs e) {


            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            ImageFile.SaveWaterPictureMark(pictureBox1.Image, pictureBox2.Image, saveFileDialog1.FileName, new Rectangle(2, 4, 90, 90),0.9f, ImageFile.GetFormat(saveFileDialog1.FileName));
        }

        private void button4_Click(object sender, EventArgs e) {

            if (saveFileDialog1.ShowDialog() == DialogResult.OK)

                ImageFile.SaveWaterMark(pictureBox1.Image, saveFileDialog1.FileName, textBox1.Text, new Font("宋体", 18), Color.Black, new PointF(30, 456), ImageFile.GetFormat(saveFileDialog1.FileName));
        }

        private void button5_Click(object sender, EventArgs e) {

            if(saveFileDialog1.ShowDialog() == DialogResult.OK)
            ImageFile.SaveWaterPictureMark(pictureBox1.ImageLocation, pictureBox2.ImageLocation, saveFileDialog1.FileName, new Point(2, 4), new Size(50, 50), 120f, 1f);
        }

        private void button6_Click(object sender, EventArgs e) {

            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            ImageFile.SaveThumbnail(pictureBox1.ImageLocation, saveFileDialog1.FileName, new Size(100, 500));
        }
    }
}
