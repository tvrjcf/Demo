namespace Demo_Http
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
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.btn_Get = new System.Windows.Forms.Button();
            this.txt_Url = new System.Windows.Forms.TextBox();
            this.txt_Content = new System.Windows.Forms.TextBox();
            this.btn_save = new System.Windows.Forms.Button();
            this.BW = new System.ComponentModel.BackgroundWorker();
            this.lb_Message = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // btn_Get
            // 
            this.btn_Get.Location = new System.Drawing.Point(397, 11);
            this.btn_Get.Name = "btn_Get";
            this.btn_Get.Size = new System.Drawing.Size(75, 23);
            this.btn_Get.TabIndex = 0;
            this.btn_Get.Text = "GET";
            this.btn_Get.UseVisualStyleBackColor = true;
            this.btn_Get.Click += new System.EventHandler(this.btn_Get_Click);
            // 
            // txt_Url
            // 
            this.txt_Url.Location = new System.Drawing.Point(12, 12);
            this.txt_Url.Name = "txt_Url";
            this.txt_Url.Size = new System.Drawing.Size(379, 21);
            this.txt_Url.TabIndex = 1;
            this.txt_Url.Text = "http://a.heiyan.com/ajax/chapter/content/923725?callback=jQuery183042279977366378" +
    "296_1489487599999&_=1489487600401";
            // 
            // txt_Content
            // 
            this.txt_Content.Location = new System.Drawing.Point(12, 39);
            this.txt_Content.Multiline = true;
            this.txt_Content.Name = "txt_Content";
            this.txt_Content.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txt_Content.Size = new System.Drawing.Size(379, 224);
            this.txt_Content.TabIndex = 1;
            // 
            // btn_save
            // 
            this.btn_save.Location = new System.Drawing.Point(397, 40);
            this.btn_save.Name = "btn_save";
            this.btn_save.Size = new System.Drawing.Size(75, 23);
            this.btn_save.TabIndex = 0;
            this.btn_save.Text = "SAVE TXT";
            this.btn_save.UseVisualStyleBackColor = true;
            this.btn_save.Click += new System.EventHandler(this.btn_save_Click);
            // 
            // BW
            // 
            this.BW.WorkerReportsProgress = true;
            this.BW.WorkerSupportsCancellation = true;
            this.BW.DoWork += new System.ComponentModel.DoWorkEventHandler(this.BW_DoWork);
            this.BW.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(this.BW_ProgressChanged);
            this.BW.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.BW_RunWorkerCompleted);
            // 
            // lb_Message
            // 
            this.lb_Message.AutoSize = true;
            this.lb_Message.Location = new System.Drawing.Point(10, 280);
            this.lb_Message.Name = "lb_Message";
            this.lb_Message.Size = new System.Drawing.Size(23, 12);
            this.lb_Message.TabIndex = 2;
            this.lb_Message.Text = "---";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(484, 310);
            this.Controls.Add(this.lb_Message);
            this.Controls.Add(this.txt_Content);
            this.Controls.Add(this.txt_Url);
            this.Controls.Add(this.btn_save);
            this.Controls.Add(this.btn_Get);
            this.Name = "Form1";
            this.Text = "frm_Main";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btn_Get;
        private System.Windows.Forms.TextBox txt_Url;
        private System.Windows.Forms.TextBox txt_Content;
        private System.Windows.Forms.Button btn_save;
        private System.ComponentModel.BackgroundWorker BW;
        private System.Windows.Forms.Label lb_Message;
    }
}

