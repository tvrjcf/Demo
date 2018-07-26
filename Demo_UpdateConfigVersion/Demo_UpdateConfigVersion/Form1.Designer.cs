namespace Demo_UpdateConfigVersion
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.txt_Path = new System.Windows.Forms.TextBox();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.tsslb_Message = new System.Windows.Forms.ToolStripStatusLabel();
            this.btn_Save = new System.Windows.Forms.Button();
            this.btn_Read = new System.Windows.Forms.Button();
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.btn_DownVersion = new System.Windows.Forms.Button();
            this.txt_VersionAdd = new System.Windows.Forms.TextBox();
            this.btn_UpVersion = new System.Windows.Forms.Button();
            this.txt_FilterRow = new System.Windows.Forms.TextBox();
            this.ckb_Backup = new System.Windows.Forms.CheckBox();
            this.statusStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            this.SuspendLayout();
            // 
            // txt_Path
            // 
            this.txt_Path.AllowDrop = true;
            this.txt_Path.Location = new System.Drawing.Point(12, 11);
            this.txt_Path.Name = "txt_Path";
            this.txt_Path.Size = new System.Drawing.Size(260, 21);
            this.txt_Path.TabIndex = 0;
            this.txt_Path.Text = "config.json";
            this.txt_Path.TextChanged += new System.EventHandler(this.txt_Path_TextChanged);
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsslb_Message});
            this.statusStrip1.Location = new System.Drawing.Point(0, 380);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(892, 22);
            this.statusStrip1.TabIndex = 1;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // tsslb_Message
            // 
            this.tsslb_Message.ForeColor = System.Drawing.Color.Blue;
            this.tsslb_Message.Name = "tsslb_Message";
            this.tsslb_Message.Size = new System.Drawing.Size(32, 17);
            this.tsslb_Message.Text = "就绪";
            // 
            // btn_Save
            // 
            this.btn_Save.Location = new System.Drawing.Point(360, 10);
            this.btn_Save.Name = "btn_Save";
            this.btn_Save.Size = new System.Drawing.Size(75, 23);
            this.btn_Save.TabIndex = 2;
            this.btn_Save.Text = "保存";
            this.btn_Save.UseVisualStyleBackColor = true;
            this.btn_Save.Click += new System.EventHandler(this.btn_Save_Click);
            // 
            // btn_Read
            // 
            this.btn_Read.Location = new System.Drawing.Point(278, 10);
            this.btn_Read.Name = "btn_Read";
            this.btn_Read.Size = new System.Drawing.Size(75, 23);
            this.btn_Read.TabIndex = 3;
            this.btn_Read.Text = "读取";
            this.btn_Read.UseVisualStyleBackColor = true;
            this.btn_Read.Click += new System.EventHandler(this.btn_Read_Click);
            // 
            // dataGridView1
            // 
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.Location = new System.Drawing.Point(12, 38);
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.RowTemplate.Height = 23;
            this.dataGridView1.Size = new System.Drawing.Size(868, 339);
            this.dataGridView1.TabIndex = 4;
            // 
            // btn_DownVersion
            // 
            this.btn_DownVersion.Location = new System.Drawing.Point(509, 10);
            this.btn_DownVersion.Name = "btn_DownVersion";
            this.btn_DownVersion.Size = new System.Drawing.Size(40, 23);
            this.btn_DownVersion.TabIndex = 3;
            this.btn_DownVersion.Text = "-";
            this.btn_DownVersion.UseVisualStyleBackColor = true;
            this.btn_DownVersion.Click += new System.EventHandler(this.btn_DownVersion_Click);
            // 
            // txt_VersionAdd
            // 
            this.txt_VersionAdd.Location = new System.Drawing.Point(555, 11);
            this.txt_VersionAdd.Name = "txt_VersionAdd";
            this.txt_VersionAdd.Size = new System.Drawing.Size(40, 21);
            this.txt_VersionAdd.TabIndex = 5;
            this.txt_VersionAdd.Text = "0.001";
            // 
            // btn_UpVersion
            // 
            this.btn_UpVersion.Location = new System.Drawing.Point(601, 10);
            this.btn_UpVersion.Name = "btn_UpVersion";
            this.btn_UpVersion.Size = new System.Drawing.Size(40, 23);
            this.btn_UpVersion.TabIndex = 3;
            this.btn_UpVersion.Text = "+";
            this.btn_UpVersion.UseVisualStyleBackColor = true;
            this.btn_UpVersion.Click += new System.EventHandler(this.btn_UpVersion_Click);
            // 
            // txt_FilterRow
            // 
            this.txt_FilterRow.AllowDrop = true;
            this.txt_FilterRow.Location = new System.Drawing.Point(648, 11);
            this.txt_FilterRow.Name = "txt_FilterRow";
            this.txt_FilterRow.Size = new System.Drawing.Size(232, 21);
            this.txt_FilterRow.TabIndex = 0;
            this.txt_FilterRow.TextChanged += new System.EventHandler(this.txt_Path_TextChanged);
            this.txt_FilterRow.KeyUp += new System.Windows.Forms.KeyEventHandler(this.txt_FilterRow_KeyUp);
            // 
            // ckb_Backup
            // 
            this.ckb_Backup.AutoSize = true;
            this.ckb_Backup.Checked = true;
            this.ckb_Backup.CheckState = System.Windows.Forms.CheckState.Checked;
            this.ckb_Backup.Location = new System.Drawing.Point(442, 13);
            this.ckb_Backup.Name = "ckb_Backup";
            this.ckb_Backup.Size = new System.Drawing.Size(48, 16);
            this.ckb_Backup.TabIndex = 6;
            this.ckb_Backup.Text = "备份";
            this.ckb_Backup.UseVisualStyleBackColor = true;
            // 
            // Form1
            // 
            this.AllowDrop = true;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(892, 402);
            this.Controls.Add(this.ckb_Backup);
            this.Controls.Add(this.txt_VersionAdd);
            this.Controls.Add(this.dataGridView1);
            this.Controls.Add(this.btn_UpVersion);
            this.Controls.Add(this.btn_DownVersion);
            this.Controls.Add(this.btn_Read);
            this.Controls.Add(this.btn_Save);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.txt_FilterRow);
            this.Controls.Add(this.txt_Path);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "PDA版本发布 SIE";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.DragDrop += new System.Windows.Forms.DragEventHandler(this.Form1_DragDrop);
            this.DragEnter += new System.Windows.Forms.DragEventHandler(this.Form1_DragEnter);
            this.DragOver += new System.Windows.Forms.DragEventHandler(this.Form1_DragOver);
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox txt_Path;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel tsslb_Message;
        private System.Windows.Forms.Button btn_Save;
        private System.Windows.Forms.Button btn_Read;
        private System.Windows.Forms.DataGridView dataGridView1;
        private System.Windows.Forms.Button btn_DownVersion;
        private System.Windows.Forms.TextBox txt_VersionAdd;
        private System.Windows.Forms.Button btn_UpVersion;
        private System.Windows.Forms.TextBox txt_FilterRow;
        private System.Windows.Forms.CheckBox ckb_Backup;
    }
}

