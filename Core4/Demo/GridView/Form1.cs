using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using Py.Sql;

namespace GridView {
    public partial class Form1 :Form {
        public Form1() {
            InitializeComponent();

            _sql = new OleDbHelper();
            _sql.SetConnectionString(@"F:\wz\Py\Py.Core\预览\Demo\Data/DbHelper.mdb");
            _sql.SqlTable = "TableName";
            _sql.SqlColumn = "ID, [Value], [Name], [Time], [Content], Sort, ModelId";
            _sql.CreateCommandText(SqlOperation.Select);

            button1_Click(this, EventArgs.Empty);
        }


        DbHelper _sql;


        private void button1_Click(object sender, EventArgs e) {
            dataGridView1.DataSource = _sql.DataSource;
        }

        private void button2_Click(object sender, EventArgs e) {
            _sql.DataSource = dataGridView1.DataSource;
        }

        private void button3_Click(object sender, EventArgs e) {
            DataTable dt = (DataTable)dataGridView1.DataSource;
            DataRow dr = dt.NewRow();
            dr[2] = "加";
            dt.Rows.Add(dr);
        }

        private void button4_Click(object sender, EventArgs e) {
            dataGridView1.SelectAll();
        }

        private void button5_Click(object sender, EventArgs e) {
            dataGridView1.Sort(dataGridView1.Columns["Sort"], ListSortDirection.Ascending);
        }

        private void button6_Click(object sender, EventArgs e) {
            if (dataGridView1.SelectedRows.Count > 0) {
                dataGridView1.Rows.Remove(dataGridView1.SelectedRows[0]);

            }
        }

        private void button7_Click(object sender, EventArgs e) {
            dataGridView1.CancelEdit();
        }
    }
}
