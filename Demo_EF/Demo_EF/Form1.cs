using log4net;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Demo_EF
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
 
        }

        private void btn_Add_Click(object sender, EventArgs e)
        {
            using (var db = new BlogContext())
            {
                //if (string.IsNullOrEmpty(textBox1.Text))
                //{
                //    MessageBox.Show("输入一个新的Blog名称");
                //    return;
                //}
                //var name = textBox1.Text;

                //var blog = new Blog { Name = name };
                //var post = new Post { Title = name, Content = name };
                //post.Blog = blog;
                //db.Posts.Add(post);

                //db.Blogs.Add(blog);
                //db.SaveChanges();
                Blog b = db.Blogs.Find(7);
                if (b == null)
                {
                    MessageBox.Show("no find");
                }
                else {
                    b.Code = "Code" + b.BlogId.ToString();
                    db.SaveChanges();
                    MessageBox.Show(b.Code);
                }
                //var query = from b in db.Blogs where b.BlogId ==7
                //            orderby b.Name
                //            select b;
                //MessageBox.Show(query.Count().ToString());
                //foreach (var item in query)
                //{
                //    MessageBox.Show(item.Name);
                //}
            }
        }

        private void btn_log4net_Click(object sender, EventArgs e)
        {

            ILog log = LogManager.GetLogger("log4net");
            log.Info("It's OK!"); 
        }
    }
}
