using BD.Items;
using SAP_Class;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WMS.Hisense.WipIssueBills;

namespace DownloadBill.Hisense.Download
{
    /// <summary>
    /// 下载外协发料单
    /// </summary>
    public static class RFC_WxWipIssueBill
    {
        public static IEnumerable<WxWipIssueBill> GetWxWipIssueBill(string OrgCode, string billNo)
        {
            var lt = new List<WxWipIssueBill>();
            Hashtable import = new Hashtable();
            import.Add("I_BUKRS", OrgCode);//公司代码
            //import.Add("I_WERKS", "");//工厂
            import.Add("I_ZBLNR", billNo);//单号
            lt.AddRange(ERP_MES_WX(import));
            return lt;
        }
        private static List<WxWipIssueBill> ERP_MES_WX(Hashtable import)
        {
            RFC rfc = new RFC(ConfigurationManager.AppSettings["sapServer"].ToString());
            //RFC rfc = new RFC("SAPTest");
            var lst = new List<WxWipIssueBill>();
            //RFC调用函数名
            string funcName = "ZWMS_WXFLD";
            //传输传给RFC函数的DataSet
            DataSet ids = new DataSet();
            //构建RFC传入表DataTable

            //返回数据的DataSet框架
            DataSet ods = new DataSet();
            //构建Export参数DataTable
            DataTable export = new DataTable();
            export.TableName = "Export";
            export.Columns.Add(new DataColumn("RTYPE", typeof(string)));//消息文本
            export.Columns.Add(new DataColumn("RTMSG", typeof(string)));//消息文本
            ods.Tables.Add(export);
            //构建RFC传出表DataTable
            DataTable odt1 = new DataTable();
            odt1.TableName = "T_TAB";
            odt1.Columns.Add(new DataColumn("ZBLNR", typeof(string)));//单号
            odt1.Columns.Add(new DataColumn("LIFNR", typeof(string)));//供应商
            odt1.Columns.Add(new DataColumn("LGORT", typeof(string)));//库存地点            
            odt1.Columns.Add(new DataColumn("WERKS", typeof(string)));//工厂
            odt1.Columns.Add(new DataColumn("MATNR", typeof(string)));//物料号
            odt1.Columns.Add(new DataColumn("ZXLSL", typeof(string)));//数量
            odt1.Columns.Add(new DataColumn("CHARG", typeof(string)));//批号
            odt1.Columns.Add(new DataColumn("MEINS", typeof(string)));//基本计量单位
            ods.Tables.Add(odt1);

            //DataTable odt2 = new DataTable();
            //odt2.TableName = "T_TAB2";
            //odt2.Columns.Add(new DataColumn("ZBLNR", typeof(string)));//单号
            //odt2.Columns.Add(new DataColumn("LIFNR", typeof(string)));//供应商                     
            //ods.Tables.Add(odt2);


            //执行RFC函数
            bool ret = rfc.DownloadByRFC(funcName, import, ids, ref ods);
            //根据RFC执行后返回的数据处理业务逻辑 

            var dtItem = ods.Tables["T_TAB"];
            var view = dtItem.DefaultView;
            var dtHead = view.ToTable("T_HEAD", true, "ZBLNR", "LIFNR");

            foreach (DataRow h in dtHead.Rows)
            {
                var bill = new WxWipIssueBill();

                bill.No = h["ZBLNR"].ToString().Trim();
                bill.Supplier = new BD.Suppliers.Supplier() { Code = h["LIFNR"].ToString() };


                foreach (DataRow i in dtItem.AsEnumerable().Where(p => h["ZBLNR"].ToString() == p["ZBLNR"].ToString()))
                {
                    var billDtl = new WxWipIssueBillDetail()
                    {
                        Item = new Item { Code = i["MATNR"].ToString() },
                        //ProjectNo = i["EBELP"].ToString(),
                        Qty = Convert.ToDouble(i["ZXLSL"] ?? 0),
                        Unit = i["MEINS"].ToString(),
                        Factory = i["WERKS"].ToString(),
                        BatchNo = i["CHARG"].ToString(),
                        IssueWarehouse = new INV.Hisense.Warehouses.Warehouses.Warehouse { Code = i["LGORT"].ToString() },
                        //PickWarehouse = new INV.Hisense.Warehouses.Warehouses.Warehouse { Code = i["LGORT"].ToString() },                        
                    };
                    bill.WxWipIssueBillDetailList.Add(billDtl);
                }
                lst.Add(bill);
            }
            return lst;
        }

    }
}
