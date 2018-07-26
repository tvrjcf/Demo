using CCWin;
using CCWin.SkinControl;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Demo_CSkin
{
    public partial class Form1 : CCSkinMain
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

            ChatListItem item = new ChatListItem("我的好友");
            ChatListSubItem sub = new ChatListSubItem();
            sub.NicName = "乔克斯";
            sub.DisplayName = "小乔";
            sub.Status = ChatListSubItem.UserStatus.QMe;
            item.SubItems.Add(sub);
            this.chatListBox1.Items.Add(item);
            ChatListItem item2 = new ChatListItem("亲人");
            ChatListSubItem sub2 = new ChatListSubItem();
            sub2.NicName = "乔克斯1";
            sub2.DisplayName = "小乔1";
            sub2.Status = ChatListSubItem.UserStatus.QMe;
            item2.SubItems.Add(sub2);
            this.chatListBox1.Items.Add(item2);
        }

        private void chatListBox1_DoubleClickSubItem(object sender, ChatListEventArgs e, MouseEventArgs es)
        {
            MessageBox.Show(e.SelectSubItem.NicName);
        }
    }
}
