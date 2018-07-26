using DownloadBill.Hisense.Download;
using Platform;
using Platform.Domain;
using Platform.Utils;
using SAP_Class;
using System;
using System.Linq;
using System.Windows.Forms;
using INV.Hisense.WarehouseOnhands;
using SAP.Middleware.Connector;

namespace Demo_SAP
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            SAP sap = new SAP();
            RFC rfc = sap.GetSapServer();
            if (rfc.TestConnection())
                MessageBox.Show("连接成功");
            else
                MessageBox.Show("连接失败");

        }

        private void btn_DownMo_Click(object sender, EventArgs e)
        {
            //string sapState = "TECO";
            //string aa = sapState.Contains("TECO") || sapState.Contains("CLSD") ? "1" : "0";
            //MessageBox.Show(string.Format("测试：{0} \n{1}", aa, sapState));
            //return;

            Result rs = new Result() { Success = true };
            string workOrderNo = txt_Mo.Text.Trim();
            var list = RFC_Mo.GetMoByNo(workOrderNo);
            if (workOrderNo.IsNotEmpty() && list.Count() <= 0)
            {
                rs.Success = false;
                rs.Message = "找不到工单：" + workOrderNo;

            }
            MessageBox.Show(string.Format("下载：{0} \n{1}", rs.Success, rs.Message));
        }

        private void Form1_Load(object sender, EventArgs e)
        {

            //PlatformEnvironment.Provider.IsDebuggingEnabled = ConfigurationHelper.GetAppSettingOrDefault("IsDebuggingEnabled", false);
            try
            {

                var app = new DomainApp();
                app.Startup();
            }
            catch (Exception ex)
            {

                MessageBox.Show(string.Format("错误：{0} \n{1}", ex.Message));
            }
        }

        private void btn_DownLoad_Onhand_Click(object sender, EventArgs e)
        {
            Result rs = new Result() { Success = true };
            string factory = txt_Factory.Text.Trim();
            string wh = txt_Warehouse.Text.Trim();
            string item = txt_ItemCode.Text.Trim();
            string supplier = "";//txt_SupplierCode.Text.Trim(); ;
            var list = RFC_OnHand.GetErpOnhand(
                factory.IsNullOrEmpty() ? null : new string[] { factory },
                wh.IsNullOrEmpty() ? null : new string[] { wh },
                item.IsNullOrEmpty() ? null : new string[] { item },
                supplier.IsNullOrEmpty() ? null : new string[] { });
            if (factory.IsNotEmpty() && list.Count() <= 0)
            {
                rs.Success = false;
                rs.Message = string.Format("{0} 库存查询返回：{1}", factory, list.Count().ToString());

            }
            else
            {
                dataGridView1.DataSource = list;
                MessageBox.Show(string.Format("查询：{0}, 记录数 {1}", rs.Success, list.Count()));
            }
            //MessageBox.Show(string.Format("查询：{0} \n{1}", rs.Success, rs.Message));

        }

        private void btn_GetMessage_Click(object sender, EventArgs e)
        {
            txt_Message.Text = "";
            var ret = new Platform.Result();
            string wmsNo = txt_WmsNo.Text.Trim();
            if (wmsNo.Length > 0)
            {
                ret = RFC_ZWMS_MKPF.ZWMS_MKPF("", "", wmsNo);
            }
            txt_Message.Text = "{0}: {1}".Translate().FormatArgs(ret.Success, ret.Message);
        }
    }
}
