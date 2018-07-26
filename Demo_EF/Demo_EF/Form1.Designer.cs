namespace Demo_EF
{
    partial class Form1
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.btn_Add = new System.Windows.Forms.Button();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.btn_UpdateDB = new System.Windows.Forms.Button();
            this.btn_log4net = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // btn_Add
            // 
            this.btn_Add.Location = new System.Drawing.Point(38, 74);
            this.btn_Add.Name = "btn_Add";
            this.btn_Add.Size = new System.Drawing.Size(75, 23);
            this.btn_Add.TabIndex = 0;
            this.btn_Add.Text = "增加";
            this.btn_Add.UseVisualStyleBackColor = true;
            this.btn_Add.Click += new System.EventHandler(this.btn_Add_Click);
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(109, 35);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(100, 21);
            this.textBox1.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(36, 38);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(53, 12);
            this.label1.TabIndex = 2;
            this.label1.Text = "BlogName";
            // 
            // btn_UpdateDB
            // 
            this.btn_UpdateDB.Location = new System.Drawing.Point(186, 226);
            this.btn_UpdateDB.Name = "btn_UpdateDB";
            this.btn_UpdateDB.Size = new System.Drawing.Size(75, 23);
            this.btn_UpdateDB.TabIndex = 0;
            this.btn_UpdateDB.Text = "更新数据库";
            this.btn_UpdateDB.UseVisualStyleBackColor = true;
            this.btn_UpdateDB.Click += new System.EventHandler(this.button1_Click);
            // 
            // btn_log4net
            // 
            this.btn_log4net.Location = new System.Drawing.Point(38, 226);
            this.btn_log4net.Name = "btn_log4net";
            this.btn_log4net.Size = new System.Drawing.Size(85, 23);
            this.btn_log4net.TabIndex = 0;
            this.btn_log4net.Text = "lon4net测试";
            this.btn_log4net.UseVisualStyleBackColor = true;
            this.btn_log4net.Click += new System.EventHandler(this.btn_log4net_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 261);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.btn_UpdateDB);
            this.Controls.Add(this.btn_log4net);
            this.Controls.Add(this.btn_Add);
            this.Name = "Form1";
            this.Text = "Form1";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btn_Add;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btn_UpdateDB;
        private System.Windows.Forms.Button btn_log4net;
    }
}

