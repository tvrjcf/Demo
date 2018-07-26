namespace VS2008ToVS2005.UI
{
    partial class frmMain
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
            this.txtSolutionFile = new System.Windows.Forms.TextBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.btnClose = new System.Windows.Forms.Button();
            this.btnConvert = new System.Windows.Forms.Button();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.rbtnVS2005ToVS2008 = new System.Windows.Forms.RadioButton();
            this.rbtnVS2008ToVS2005 = new System.Windows.Forms.RadioButton();
            this.btnBrowser = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // txtSolutionFile
            // 
            this.txtSolutionFile.Location = new System.Drawing.Point(12, 32);
            this.txtSolutionFile.Name = "txtSolutionFile";
            this.txtSolutionFile.ReadOnly = true;
            this.txtSolutionFile.Size = new System.Drawing.Size(361, 21);
            this.txtSolutionFile.TabIndex = 1;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.btnClose);
            this.groupBox1.Controls.Add(this.btnConvert);
            this.groupBox1.Controls.Add(this.groupBox2);
            this.groupBox1.Controls.Add(this.btnBrowser);
            this.groupBox1.Controls.Add(this.txtSolutionFile);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox1.Location = new System.Drawing.Point(0, 0);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(477, 185);
            this.groupBox1.TabIndex = 2;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "加载Visual Studio解决方案";
            // 
            // btnClose
            // 
            this.btnClose.Location = new System.Drawing.Point(386, 145);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(75, 23);
            this.btnClose.TabIndex = 4;
            this.btnClose.Text = "关 闭";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // btnConvert
            // 
            this.btnConvert.Location = new System.Drawing.Point(298, 145);
            this.btnConvert.Name = "btnConvert";
            this.btnConvert.Size = new System.Drawing.Size(75, 23);
            this.btnConvert.TabIndex = 4;
            this.btnConvert.Text = "开始转换";
            this.btnConvert.UseVisualStyleBackColor = true;
            this.btnConvert.Click += new System.EventHandler(this.btnConvert_Click);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.rbtnVS2005ToVS2008);
            this.groupBox2.Controls.Add(this.rbtnVS2008ToVS2005);
            this.groupBox2.Location = new System.Drawing.Point(12, 65);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(453, 66);
            this.groupBox2.TabIndex = 3;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "转换类型";
            // 
            // rbtnVS2005ToVS2008
            // 
            this.rbtnVS2005ToVS2008.AutoSize = true;
            this.rbtnVS2005ToVS2008.Location = new System.Drawing.Point(167, 27);
            this.rbtnVS2005ToVS2008.Name = "rbtnVS2005ToVS2008";
            this.rbtnVS2005ToVS2008.Size = new System.Drawing.Size(125, 16);
            this.rbtnVS2005ToVS2008.TabIndex = 0;
            this.rbtnVS2005ToVS2008.Tag = "VS2005ToVS2008";
            this.rbtnVS2005ToVS2008.Text = "VS2005 TO VS2008 ";
            this.rbtnVS2005ToVS2008.UseVisualStyleBackColor = true;
            // 
            // rbtnVS2008ToVS2005
            // 
            this.rbtnVS2008ToVS2005.AutoSize = true;
            this.rbtnVS2008ToVS2005.Checked = true;
            this.rbtnVS2008ToVS2005.Location = new System.Drawing.Point(22, 27);
            this.rbtnVS2008ToVS2005.Name = "rbtnVS2008ToVS2005";
            this.rbtnVS2008ToVS2005.Size = new System.Drawing.Size(125, 16);
            this.rbtnVS2008ToVS2005.TabIndex = 0;
            this.rbtnVS2008ToVS2005.TabStop = true;
            this.rbtnVS2008ToVS2005.Tag = "VS2008ToVS2005";
            this.rbtnVS2008ToVS2005.Text = "VS2008 TO VS2005 ";
            this.rbtnVS2008ToVS2005.UseVisualStyleBackColor = true;
            // 
            // btnBrowser
            // 
            this.btnBrowser.Location = new System.Drawing.Point(386, 30);
            this.btnBrowser.Name = "btnBrowser";
            this.btnBrowser.Size = new System.Drawing.Size(75, 23);
            this.btnBrowser.TabIndex = 2;
            this.btnBrowser.Text = "浏 览";
            this.btnBrowser.UseVisualStyleBackColor = true;
            this.btnBrowser.Click += new System.EventHandler(this.btnBrowser_Click);
            // 
            // frmMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(477, 185);
            this.Controls.Add(this.groupBox1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "frmMain";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Visual Studio解决方案转换器";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TextBox txtSolutionFile;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button btnBrowser;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.RadioButton rbtnVS2005ToVS2008;
        private System.Windows.Forms.RadioButton rbtnVS2008ToVS2005;
        private System.Windows.Forms.Button btnConvert;
        private System.Windows.Forms.Button btnClose;
    }
}