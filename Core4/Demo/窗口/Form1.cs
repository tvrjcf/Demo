using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using Py.Windows;
using Py.Logging;
using System.Diagnostics;
using Py.Core;

namespace 窗口 {
    public partial class Form1 :Form {

        /// <summary>
        /// 初始化 <see cref="窗口.Form1"/> 的新实例。
        /// </summary>
        public Form1() {
            InitializeComponent();

            Logger.Listerner = new DebugWindowLogListener();
        }

        private void button1_Click(object sender, EventArgs e) {
            FormHelper.Shake(this, 9, 10, 50);
        }

        private void button2_Click(object sender, EventArgs e) {
            FormHelper.Slide(this, new Rectangle(500, 300, 200, 400), 900);
        }

        private void button3_Click(object sender, EventArgs e) {
            FormHelper.MoveToCenter(this);
        }

        private void button4_Click(object sender, EventArgs e) {
            FormHelper.Flash(this, true);
        }

        private void button5_Click(object sender, EventArgs e) {
            SetVisibleCore(false);

            //等待窗口隐藏 
            System.Threading.Thread.Sleep(600); 
            using(Image s = Win32Client.GetScreenshots()){
                var sfd = new SaveFileDialog() {
                    AddExtension = true,
                    CheckPathExists = true,
                    DefaultExt = ".jpg",
                    Filter = "缩略图(*.jpg)|*.jpg|位图(*.bmp)|*.bmp",
                    OverwritePrompt = true
                };
                if( sfd.ShowDialog() == DialogResult.OK)
                    s.Save(sfd.FileName);
            }
            SetVisibleCore(true);
        }

        private void button6_Click(object sender, EventArgs e) {
            FormHelper.ClipInReactange(this, new Rectangle(100, 100, 900, 600));
        }

        private void button7_Click(object sender, EventArgs e) {
            FormHelper.ToFullScreen(this);
        }

        private void button8_Click(object sender, EventArgs e) {
            Win32Client.Click(button9.Handle, false);
        }

        private void button9_Click(object sender, EventArgs e) {
            MessageBox.Show("点我");
        }

        private void button10_Click(object sender, EventArgs e) {
            Process p = Process.Start("notepad");
            Win32Client.Activate(p.Handle);
            Win32Client.SendKey("by");
            Win32Client.SendKey("{ENTER}");
            Win32Client.SendKey(" ");
            Win32Client.SendKey("xuld");
            Win32Client.SendKey("{ENTER}");
        }

        private void button11_Click(object sender, EventArgs e) {
            Hotkey hotkey = new Hotkey();
            hotkey.RegisterHotKey(Keys.U, Modifiers.Ctrl, () => {
                MessageBox.Show("试试切换其他窗口，然后调用本快捷键");
                return false;
            });
        }

        private void button12_Click(object sender, EventArgs e) {
            MessageBox.Show(Str.FormatX( "当前输入法:  {0}  ",  Ime.Current));
        }

        private void button13_Click(object sender, EventArgs e) {
            Hook hook = new Hook();
            hook.InstallHookOfKeyboard(t => {
                if (t.Key == Keys.Q) {
                    MessageBox.Show("按下 Q ");
                    t.Handled = true;
                }
            }, 0);
        }

        private void button14_Click(object sender, EventArgs e) {
            MemoryInfo mi = new MemoryInfo();
           MessageBox.Show(Str.Format( "可用内存: {0}  总的内存 : {1} G",  mi.AvailPhys, ( mi.TotalPhys + 100 ) / 1024));
        }

        private void button15_Click(object sender, EventArgs e) {
            Win32Client.SetRunOnStart(true);
        }

        private void button16_Click(object sender, EventArgs e) {
            Win32Client.SetRunOnStart(false);
        }
    }
}
