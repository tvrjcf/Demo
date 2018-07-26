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
    /// 下载研发预留单
    /// </summary>
    public static class RFC_YfWipIssueBill
    {
        public static IEnumerable<YfWipIssueBill> GetYfWipIssueBill(string OrgCode, string billNo)
        {
            var lt = new List<YfWipIssueBill>();
            Hashtable import = new Hashtable();
            import.Add("I_BUKRS", OrgCode);//公司代码
            //import.Add("I_WERKS", "");//工厂 
            import.Add("I_RSNUM", billNo);//预留单号            
            //import.Add("I_ALL", "");//是否所有数据  X 全部  空取日志表数据            
            lt.AddRange(ERP_MES_YF(import));
            return lt;
        }
        private static List<YfWipIssueBill> ERP_MES_YF(Hashtable import)
        {
            RFC rfc = new RFC(ConfigurationManager.AppSettings["sapServer"].ToString());
            //RFC rfc = new RFC("SAPTest");
            var lst = new List<YfWipIssueBill>();
            //RFC调用函数名
            string funcName = "ZWMS_YFRESERVE";
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
            odt1.TableName = "T_HEADER";
            odt1.Columns.Add(new DataColumn("RSNUM", typeof(string)));//预留单号
            odt1.Columns.Add(new DataColumn("BDTER", typeof(string)));//组件的需求日期
            odt1.Columns.Add(new DataColumn("BWART", typeof(string)));//移动类型
            odt1.Columns.Add(new DataColumn("KOSTL", typeof(string)));//成本中心
            odt1.Columns.Add(new DataColumn("KOKRS", typeof(string)));//控制范围
            ods.Tables.Add(odt1);

            DataTable odt2 = new DataTable();
            odt2.TableName = "T_ITEM";
            odt2.Columns.Add(new DataColumn("RSNUM", typeof(string)));//预留单号
            odt2.Columns.Add(new DataColumn("RSPOS", typeof(string)));//预留单号项目编号                        
            odt2.Columns.Add(new DataColumn("WERKS", typeof(string)));//工厂
            odt2.Columns.Add(new DataColumn("MATNR", typeof(string)));//物料号
            odt2.Columns.Add(new DataColumn("LGORT", typeof(string)));//库存地点
            odt2.Columns.Add(new DataColumn("BDMNG", typeof(string)));//需求量
            odt2.Columns.Add(new DataColumn("MEINS", typeof(string)));//基本计量单位
            odt2.Columns.Add(new DataColumn("BDTER", typeof(string)));//组件的需求日期
            odt2.Columns.Add(new DataColumn("ENMNG", typeof(string)));//提货数

            ods.Tables.Add(odt2);

            //执行RFC函数
            bool ret = rfc.DownloadByRFC(funcName, import, ids, ref ods);
            //根据RFC执行后返回的数据处理业务逻辑 
            //DataTable dt = ods.Tables["T_ITEM"];
            foreach (DataRow h in ods.Tables["T_HEADER"].Rows)
            {
                var bill = new YfWipIssueBill();

                string type = h["BWART"].ToString();
                string dept = h["KOSTL"].ToString();
                string ctrl = h["KOKRS"].ToString();

                bill.No = h["RSNUM"].ToString();
                bill.BillDate = Convert.ToDateTime(h["BDTER"].ToString());
                bill.CostCenter = new BD.Factories.Factory() { Code = dept };

                foreach (DataRow i in ods.Tables["T_ITEM"].AsEnumerable().Where(p => h["RSNUM"].ToString() == p["RSNUM"].ToString()))
                {
                    var billDtl = new YfWipIssueBillDetail()
                    {
                        Item = new Item { Code = i["MATNR"].ToString() },
                        ProjectNo = i["RSPOS"].ToString(),
                        Qty = Convert.ToDouble(i["BDMNG"] ?? 0),
                        ActualQty = Convert.ToDouble(i["ENMNG"] ?? 0),
                        Unit = i["MEINS"].ToString(),
                        Factory = i["WERKS"].ToString(),
                        IssueWarehouse = new INV.Hisense.Warehouses.Warehouses.Warehouse { Code = i["LGORT"].ToString() },
                        //Warehouse = i["LGORT"].ToString(),
                    };
                    bill.YfWipIssueBillDetailList.Add(billDtl);
                }
                lst.Add(bill);
            }
            return lst;
        }
    }
}
