namespace Demo_NPOI
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
            this.btn_Ok = new System.Windows.Forms.Button();
            this.tb_File = new System.Windows.Forms.TextBox();
            this.tb_EV = new System.Windows.Forms.TextBox();
            this.tb_AV = new System.Windows.Forms.TextBox();
            this.tb_RR = new System.Windows.Forms.TextBox();
            this.tb_PV = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.btn_Update = new System.Windows.Forms.Button();
            this.tb_Row = new System.Windows.Forms.TextBox();
            this.tb_Column = new System.Windows.Forms.TextBox();
            this.tb_Result = new System.Windows.Forms.TextBox();
            this.btnView = new System.Windows.Forms.Button();
            this.cbbSheets = new System.Windows.Forms.ComboBox();
            this.SuspendLayout();
            // 
            // btn_Ok
            // 
            this.btn_Ok.Location = new System.Drawing.Point(163, 57);
            this.btn_Ok.Name = "btn_Ok";
            this.btn_Ok.Size = new System.Drawing.Size(109, 23);
            this.btn_Ok.TabIndex = 0;
            this.btn_Ok.Text = "读取";
            this.btn_Ok.UseVisualStyleBackColor = true;
            this.btn_Ok.Click += new System.EventHandler(this.btn_Ok_Click);
            // 
            // tb_File
            // 
            this.tb_File.Location = new System.Drawing.Point(37, 21);
            this.tb_File.Name = "tb_File";
            this.tb_File.Size = new System.Drawing.Size(235, 21);
            this.tb_File.TabIndex = 1;
            this.tb_File.Text = "grr.xlsx";
            // 
            // tb_EV
            // 
            this.tb_EV.Location = new System.Drawing.Point(37, 133);
            this.tb_EV.Name = "tb_EV";
            this.tb_EV.Size = new System.Drawing.Size(100, 21);
            this.tb_EV.TabIndex = 2;
            // 
            // tb_AV
            // 
            this.tb_AV.Location = new System.Drawing.Point(172, 133);
            this.tb_AV.Name = "tb_AV";
            this.tb_AV.Size = new System.Drawing.Size(100, 21);
            this.tb_AV.TabIndex = 2;
            // 
            // tb_RR
            // 
            this.tb_RR.Location = new System.Drawing.Point(37, 169);
            this.tb_RR.Name = "tb_RR";
            this.tb_RR.Size = new System.Drawing.Size(100, 21);
            this.tb_RR.TabIndex = 2;
            // 
            // tb_PV
            // 
            this.tb_PV.Location = new System.Drawing.Point(172, 169);
            this.tb_PV.Name = "tb_PV";
            this.tb_PV.Size = new System.Drawing.Size(100, 21);
            this.tb_PV.TabIndex = 2;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(14, 136);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(17, 12);
            this.label1.TabIndex = 3;
            this.label1.Text = "EV";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(149, 136);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(17, 12);
            this.label2.TabIndex = 3;
            this.label2.Text = "AV";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(14, 172);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(17, 12);
            this.label3.TabIndex = 3;
            this.label3.Text = "RR";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(149, 172);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(17, 12);
            this.label4.TabIndex = 3;
            this.label4.Text = "PV";
            // 
            // btn_Update
            // 
            this.btn_Update.Location = new System.Drawing.Point(163, 214);
            this.btn_Update.Name = "btn_Update";
            this.btn_Update.Size = new System.Drawing.Size(109, 23);
            this.btn_Update.TabIndex = 0;
            this.btn_Update.Text = "修改";
            this.btn_Update.UseVisualStyleBackColor = true;
            this.btn_Update.Click += new System.EventHandler(this.btn_Update_Click);
            // 
            // tb_Row
            // 
            this.tb_Row.Location = new System.Drawing.Point(16, 280);
            this.tb_Row.Name = "tb_Row";
            this.tb_Row.Size = new System.Drawing.Size(46, 21);
            this.tb_Row.TabIndex = 1;
            this.tb_Row.Text = "0";
            // 
            // tb_Column
            // 
            this.tb_Column.Location = new System.Drawing.Point(70, 280);
            this.tb_Column.Name = "tb_Column";
            this.tb_Column.Size = new System.Drawing.Size(46, 21);
            this.tb_Column.TabIndex = 1;
            this.tb_Column.Text = "0";
            // 
            // tb_Result
            // 
            this.tb_Result.Location = new System.Drawing.Point(16, 318);
            this.tb_Result.Name = "tb_Result";
            this.tb_Result.Size = new System.Drawing.Size(256, 21);
            this.tb_Result.TabIndex = 1;
            this.tb_Result.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // btnView
            // 
            this.btnView.Location = new System.Drawing.Point(163, 278);
            this.btnView.Name = "btnView";
            this.btnView.Size = new System.Drawing.Size(109, 23);
            this.btnView.TabIndex = 0;
            this.btnView.Text = "查看";
            this.btnView.UseVisualStyleBackColor = true;
            this.btnView.Click += new System.EventHandler(this.btnView_Click);
            // 
            // cbbSheets
            // 
            this.cbbSheets.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbbSheets.FormattingEnabled = true;
            this.cbbSheets.Location = new System.Drawing.Point(37, 91);
            this.cbbSheets.Name = "cbbSheets";
            this.cbbSheets.Size = new System.Drawing.Size(121, 20);
            this.cbbSheets.TabIndex = 4;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 351);
            this.Controls.Add(this.cbbSheets);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.tb_PV);
            this.Controls.Add(this.tb_AV);
            this.Controls.Add(this.tb_RR);
            this.Controls.Add(this.tb_EV);
            this.Controls.Add(this.tb_Result);
            this.Controls.Add(this.tb_Column);
            this.Controls.Add(this.tb_Row);
            this.Controls.Add(this.tb_File);
            this.Controls.Add(this.btnView);
            this.Controls.Add(this.btn_Update);
            this.Controls.Add(this.btn_Ok);
            this.Name = "Form1";
            this.Text = "Form1";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btn_Ok;
        private System.Windows.Forms.TextBox tb_File;
        private System.Windows.Forms.TextBox tb_EV;
        private System.Windows.Forms.TextBox tb_AV;
        private System.Windows.Forms.TextBox tb_RR;
        private System.Windows.Forms.TextBox tb_PV;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button btn_Update;
        private System.Windows.Forms.TextBox tb_Row;
        private System.Windows.Forms.TextBox tb_Column;
        private System.Windows.Forms.TextBox tb_Result;
        private System.Windows.Forms.Button btnView;
        private System.Windows.Forms.ComboBox cbbSheets;
    }
}

