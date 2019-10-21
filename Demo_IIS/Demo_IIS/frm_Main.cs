using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Demo_IIS
{
    public partial class frm_Main : Form
    {
        private string exePath = @"%windir%/system32/inetsrv/appcmd";//调用安装好的
        public frm_Main()
        {
            InitializeComponent();
        }

        private void btnExecute_Click(object sender, EventArgs e)
        {
            var siteName = txtSiteName.Text;
            var bingingInfo = string.Format("*:{0}:{1}", txtPort.Text, "");
            var physicalPath = txtDir.Text;// @"E:\JZ\CHM\czolgame\dr.czolgame.com\";
            try
            {
                if (!IISHelper.VerifyWebSiteIsExist(siteName))
                {
                    IISHelper.CreateSite(siteName, bingingInfo, physicalPath);
                    MessageBox.Show("站点[" + siteName + "]已创建完成");
                }
                else
                {
                    MessageBox.Show("已存在站点[" + siteName + "]");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.GetBaseException().Message);
            }

            //string arg = string.Format("list app \"Default Web Site/\"");
            //var r = RunCmd(arg);

        }


        static string RunCmd(string command)
        {
            // string exePath = Directory.GetCurrentDirectory()+ @"\haoya";// 复制好压的文件到exe下,haoya文件夹内
            string exePath = @"C:\Windows\System32\inetsrv\appcmd.exe";//调用安装好的

            Process p = new Process();
            //设置要启动的应用程序
            p.StartInfo.FileName = "cmd";
            //是否使用操作系统shell启动
            p.StartInfo.UseShellExecute = false;
            // 接受来自调用程序的输入信息
            p.StartInfo.RedirectStandardInput = true;
            //输出信息
            p.StartInfo.RedirectStandardOutput = true;
            // 输出错误
            p.StartInfo.RedirectStandardError = true;
            //不显示程序窗口
            p.StartInfo.CreateNoWindow = false;
            //启动程序
            p.Start();
            //向cmd窗口发送输入信息
            //p.StandardInput.WriteLine("cd " + exePath);
            p.StandardInput.WriteLine("cls");
            p.StandardInput.WriteLine(exePath + " " + command + "");
            p.StandardInput.WriteLine("exit");
            p.StandardInput.AutoFlush = true;
            //获取输出信息
            string strOuput = p.StandardOutput.ReadToEnd();
            //等待程序执行完退出进程
            p.WaitForExit();
            //释放与此组件关联的所有资源
            p.Close();
            Console.WriteLine(strOuput);
            return strOuput;
        }

        private void btnDeleteSite_Click(object sender, EventArgs e)
        {
            var siteName = txtSiteName.Text;
            try
            {
                if (IISHelper.VerifyWebSiteIsExist(siteName))
                {
                    IISHelper.DeleteSite(siteName);
                    MessageBox.Show("站点[" + siteName + "]已删除");
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.GetBaseException().Message);
            }
        }
    }


}
