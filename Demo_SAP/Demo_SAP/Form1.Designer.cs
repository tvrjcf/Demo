namespace Demo_SAP
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
            this.button1 = new System.Windows.Forms.Button();
            this.btn_DownMo = new System.Windows.Forms.Button();
            this.txt_Mo = new System.Windows.Forms.TextBox();
            this.btn_DownLoad_Onhand = new System.Windows.Forms.Button();
            this.txt_Factory = new System.Windows.Forms.TextBox();
            this.txt_Warehouse = new System.Windows.Forms.TextBox();
            this.txt_ItemCode = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.txt_WmsNo = new System.Windows.Forms.TextBox();
            this.btn_GetMessage = new System.Windows.Forms.Button();
            this.label4 = new System.Windows.Forms.Label();
            this.txt_Message = new System.Windows.Forms.TextBox();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(12, 12);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 0;
            this.button1.Text = "测试SAP连接";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // btn_DownMo
            // 
            this.btn_DownMo.Location = new System.Drawing.Point(197, 43);
            this.btn_DownMo.Name = "btn_DownMo";
            this.btn_DownMo.Size = new System.Drawing.Size(75, 23);
            this.btn_DownMo.TabIndex = 1;
            this.btn_DownMo.Text = "下载工单";
            this.btn_DownMo.UseVisualStyleBackColor = true;
            this.btn_DownMo.Click += new System.EventHandler(this.btn_DownMo_Click);
            // 
            // txt_Mo
            // 
            this.txt_Mo.Location = new System.Drawing.Point(13, 44);
            this.txt_Mo.Name = "txt_Mo";
            this.txt_Mo.Size = new System.Drawing.Size(178, 21);
            this.txt_Mo.TabIndex = 2;
            this.txt_Mo.Text = "J17040948";
            // 
            // btn_DownLoad_Onhand
            // 
            this.btn_DownLoad_Onhand.Location = new System.Drawing.Point(196, 145);
            this.btn_DownLoad_Onhand.Name = "btn_DownLoad_Onhand";
            this.btn_DownLoad_Onhand.Size = new System.Drawing.Size(75, 23);
            this.btn_DownLoad_Onhand.TabIndex = 3;
            this.btn_DownLoad_Onhand.Text = "查询库存";
            this.btn_DownLoad_Onhand.UseVisualStyleBackColor = true;
            this.btn_DownLoad_Onhand.Click += new System.EventHandler(this.btn_DownLoad_Onhand_Click);
            // 
            // txt_Factory
            // 
            this.txt_Factory.Location = new System.Drawing.Point(45, 93);
            this.txt_Factory.Name = "txt_Factory";
            this.txt_Factory.Size = new System.Drawing.Size(145, 21);
            this.txt_Factory.TabIndex = 2;
            this.txt_Factory.Text = "4233";
            // 
            // txt_Warehouse
            // 
            this.txt_Warehouse.Location = new System.Drawing.Point(45, 120);
            this.txt_Warehouse.Name = "txt_Warehouse";
            this.txt_Warehouse.Size = new System.Drawing.Size(145, 21);
            this.txt_Warehouse.TabIndex = 2;
            this.txt_Warehouse.Text = "WA09";
            // 
            // txt_ItemCode
            // 
            this.txt_ItemCode.Location = new System.Drawing.Point(45, 147);
            this.txt_ItemCode.Name = "txt_ItemCode";
            this.txt_ItemCode.Size = new System.Drawing.Size(145, 21);
            this.txt_ItemCode.TabIndex = 2;
            this.txt_ItemCode.Text = "8100017301";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(10, 96);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(29, 12);
            this.label1.TabIndex = 4;
            this.label1.Text = "工厂";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(10, 123);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(29, 12);
            this.label2.TabIndex = 4;
            this.label2.Text = "仓库";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(10, 150);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(29, 12);
            this.label3.TabIndex = 4;
            this.label3.Text = "物料";
            // 
            // dataGridView1
            // 
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.Location = new System.Drawing.Point(12, 183);
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.RowTemplate.Height = 23;
            this.dataGridView1.Size = new System.Drawing.Size(760, 216);
            this.dataGridView1.TabIndex = 5;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.txt_Message);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.btn_GetMessage);
            this.groupBox1.Controls.Add(this.txt_WmsNo);
            this.groupBox1.Location = new System.Drawing.Point(454, 14);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(318, 154);
            this.groupBox1.TabIndex = 6;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "物料凭证查询";
            // 
            // txt_WmsNo
            // 
            this.txt_WmsNo.Location = new System.Drawing.Point(65, 32);
            this.txt_WmsNo.Name = "txt_WmsNo";
            this.txt_WmsNo.Size = new System.Drawing.Size(162, 21);
            this.txt_WmsNo.TabIndex = 3;
            this.txt_WmsNo.Text = "4230-929";
            // 
            // btn_GetMessage
            // 
            this.btn_GetMessage.Location = new System.Drawing.Point(233, 31);
            this.btn_GetMessage.Name = "btn_GetMessage";
            this.btn_GetMessage.Size = new System.Drawing.Size(75, 23);
            this.btn_GetMessage.TabIndex = 4;
            this.btn_GetMessage.Text = "查询";
            this.btn_GetMessage.UseVisualStyleBackColor = true;
            this.btn_GetMessage.Click += new System.EventHandler(this.btn_GetMessage_Click);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(6, 36);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(53, 12);
            this.label4.TabIndex = 5;
            this.label4.Text = "过账编码";
            // 
            // txt_Message
            // 
            this.txt_Message.Location = new System.Drawing.Point(8, 66);
            this.txt_Message.Multiline = true;
            this.txt_Message.Name = "txt_Message";
            this.txt_Message.Size = new System.Drawing.Size(304, 82);
            this.txt_Message.TabIndex = 6;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(799, 425);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.dataGridView1);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btn_DownLoad_Onhand);
            this.Controls.Add(this.txt_ItemCode);
            this.Controls.Add(this.txt_Warehouse);
            this.Controls.Add(this.txt_Factory);
            this.Controls.Add(this.txt_Mo);
            this.Controls.Add(this.btn_DownMo);
            this.Controls.Add(this.button1);
            this.Name = "Form1";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Form1_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button btn_DownMo;
        private System.Windows.Forms.TextBox txt_Mo;
        private System.Windows.Forms.Button btn_DownLoad_Onhand;
        private System.Windows.Forms.TextBox txt_Factory;
        private System.Windows.Forms.TextBox txt_Warehouse;
        private System.Windows.Forms.TextBox txt_ItemCode;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.DataGridView dataGridView1;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.TextBox txt_Message;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button btn_GetMessage;
        private System.Windows.Forms.TextBox txt_WmsNo;
    }
}

