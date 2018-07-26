using Demo_Http.models;
using Newtonsoft.Json;
using RDIFramework.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Demo_Http
{
    public partial class Form1 : Form
    {
        string bookStr = "";
        string first = "923725";
        string url = @"http://a.heiyan.com/ajax/chapter/content/{0}?callback=jQuery183042279977366378296_1489487599999&_=1489487600401";

        string currChapter = "-1";
        HttpHelper httper = new HttpHelper();

        public Form1()
        {
            InitializeComponent();
        }

        private void btn_Get_Click(object sender, EventArgs e)
        {
            currChapter = first;
            BW.RunWorkerAsync();


        }

        private void btn_save_Click(object sender, EventArgs e)
        {
            try
            {
                if (!string.IsNullOrEmpty(bookStr))
                {
                    //string file = FileHelper.ReadFile("C:\\book.txt");
                    FileHelper.WriteFile("C:\\book.txt", bookStr);

                    MessageBox.Show("保存成功");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void BW_DoWork(object sender, DoWorkEventArgs e)
        {
            while (currChapter != "-1")
            {
                try
                {
                    //string url = txt_Url.Text;
                    //BW.ReportProgress(0, "开始下载...");
                    string url = @"http://a.heiyan.com/ajax/chapter/content/{0}?callback=jQuery183042279977366378296_1489487599999&_=1489487600401";

                    url = string.Format(url, currChapter);

                    if (!string.IsNullOrEmpty(url))
                    {
                        string html = httper.GetHtml(new HttpItem() { URL = url });
                        //txt_Content.Text = HttpHelper.GetUrl(url);
                        if (!string.IsNullOrEmpty(html))
                        {
                            int startIndex = html.IndexOf("({") + 1;
                            html = html.Substring(startIndex, html.Length - 1 - startIndex);
                            var book_chapter = JsonConvert.DeserializeObject<heiyan_chapter>(html);
                            var title = book_chapter.chapter.title;
                            var content = book_chapter.chapter.htmlContent;
                            content = content.Replace("&ldquo;", "“").Replace("&rdquo;", "”");
                            content = content.Replace("<p>", "").Replace(@"</p>", "\r\n");
                            content = content.Replace("&hellip;", "...").Replace(@"&mdash;", "-");
                            bookStr += string.Format("{0}\r\n\r\n{1}\r\n\r\n", title, content);
                            //txt_Content.Text = bookStr;
                            currChapter = book_chapter.nextChapterId;
                            BW.ReportProgress(100, book_chapter.chapter.title);
                        }

                    }
                }
                catch (Exception ex)
                {
                    //MessageBox.Show(ex.Message);
                    currChapter = "-1";
                }

            }
        }

        private void BW_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            lb_Message.Text = string.Format("已下载: {0}", e.UserState.ToString());
        }

        private void BW_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            lb_Message.Text = "下载完成";
            txt_Content.Text = bookStr;
        }
    }
}
