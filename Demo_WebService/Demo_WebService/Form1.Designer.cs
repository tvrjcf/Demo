namespace Demo_WebService
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
            this.label1 = new System.Windows.Forms.Label();
            this.btn_GetCurrentIP = new System.Windows.Forms.Button();
            this.btn_EncryptMD5 = new System.Windows.Forms.Button();
            this.btn_StrToMd5 = new System.Windows.Forms.Button();
            this.btn_SRM = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(41, 12);
            this.label1.TabIndex = 0;
            this.label1.Text = "label1";
            // 
            // btn_GetCurrentIP
            // 
            this.btn_GetCurrentIP.Location = new System.Drawing.Point(13, 40);
            this.btn_GetCurrentIP.Name = "btn_GetCurrentIP";
            this.btn_GetCurrentIP.Size = new System.Drawing.Size(259, 23);
            this.btn_GetCurrentIP.TabIndex = 1;
            this.btn_GetCurrentIP.Text = "GetCurrentIP";
            this.btn_GetCurrentIP.UseVisualStyleBackColor = true;
            this.btn_GetCurrentIP.Click += new System.EventHandler(this.btn_GetCurrentIP_Click);
            // 
            // btn_EncryptMD5
            // 
            this.btn_EncryptMD5.Location = new System.Drawing.Point(14, 69);
            this.btn_EncryptMD5.Name = "btn_EncryptMD5";
            this.btn_EncryptMD5.Size = new System.Drawing.Size(259, 23);
            this.btn_EncryptMD5.TabIndex = 1;
            this.btn_EncryptMD5.Text = "EncryptMD5";
            this.btn_EncryptMD5.UseVisualStyleBackColor = true;
            this.btn_EncryptMD5.Click += new System.EventHandler(this.btn_EncryptMD5_Click);
            // 
            // btn_StrToMd5
            // 
            this.btn_StrToMd5.Location = new System.Drawing.Point(14, 98);
            this.btn_StrToMd5.Name = "btn_StrToMd5";
            this.btn_StrToMd5.Size = new System.Drawing.Size(259, 23);
            this.btn_StrToMd5.TabIndex = 1;
            this.btn_StrToMd5.Text = "StrToMd5";
            this.btn_StrToMd5.UseVisualStyleBackColor = true;
            this.btn_StrToMd5.Click += new System.EventHandler(this.btn_StrToMd5_Click);
            // 
            // btn_SRM
            // 
            this.btn_SRM.Location = new System.Drawing.Point(14, 183);
            this.btn_SRM.Name = "btn_SRM";
            this.btn_SRM.Size = new System.Drawing.Size(259, 23);
            this.btn_SRM.TabIndex = 1;
            this.btn_SRM.Text = "SRM";
            this.btn_SRM.UseVisualStyleBackColor = true;
            this.btn_SRM.Click += new System.EventHandler(this.btn_SRM_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 261);
            this.Controls.Add(this.btn_SRM);
            this.Controls.Add(this.btn_StrToMd5);
            this.Controls.Add(this.btn_EncryptMD5);
            this.Controls.Add(this.btn_GetCurrentIP);
            this.Controls.Add(this.label1);
            this.Name = "Form1";
            this.Text = "Form1";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btn_GetCurrentIP;
        private System.Windows.Forms.Button btn_EncryptMD5;
        private System.Windows.Forms.Button btn_StrToMd5;
        private System.Windows.Forms.Button btn_SRM;
    }
}

