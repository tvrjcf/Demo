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
using WMS.Hisense.SaleBills;

namespace DownloadBill.Hisense.Download
{
    /// <summary>
    /// 下载销售交货单
    /// </summary>
    public static class RFC_SaleBill
    {
        public static IEnumerable<SaleBill> GetSaleBill(string OrgCode, string BillNo)
        {
            var lt = new List<SaleBill>();
            Hashtable import = new Hashtable();
            import.Add("I_BUKRS", OrgCode);//公司代码
            import.Add("I_VBELN", BillNo);//交货
            import.Add("I_DATAB", "");//起始日期
            import.Add("I_DATBI", "");//结束日期

            lt.AddRange(ERP_MES_JHD(import));
            return lt;
        }
        private static List<SaleBill> ERP_MES_JHD(Hashtable import)
        {
            RFC rfc = new RFC(ConfigurationManager.AppSettings["sapServer"].ToString());
            //RFC rfc = new RFC("SAPTest");
            var lst = new List<SaleBill>();
            //RFC调用函数名
            string funcName = "ZWMS_JHD";
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
            odt1.Columns.Add(new DataColumn("VBELN", typeof(string)));//交货
            odt1.Columns.Add(new DataColumn("VSTEL", typeof(string)));//装运点/接收点
            odt1.Columns.Add(new DataColumn("VKORG", typeof(string)));//销售机构
            odt1.Columns.Add(new DataColumn("LFART", typeof(string)));//交货类型
            //odt1.Columns.Add(new DataColumn("KOKRS", typeof(string)));//客户编号
            odt1.Columns.Add(new DataColumn("WADAT", typeof(string)));//计划货物移动日期
            odt1.Columns.Add(new DataColumn("LFDAT", typeof(string)));//交货日期
            odt1.Columns.Add(new DataColumn("WBSTK", typeof(string)));//货物移动状态总计
            ods.Tables.Add(odt1);


            DataTable odt2 = new DataTable();
            odt2.TableName = "T_ITEM";
            odt2.Columns.Add(new DataColumn("VBELN", typeof(string)));//交货
            odt2.Columns.Add(new DataColumn("POSNR", typeof(string)));//交货项目
            odt2.Columns.Add(new DataColumn("PSTYV", typeof(string)));//交货项目类别
            odt2.Columns.Add(new DataColumn("WERKS", typeof(string)));//工厂
            odt2.Columns.Add(new DataColumn("MATNR", typeof(string)));//物料号
            odt2.Columns.Add(new DataColumn("LGORT", typeof(string)));//库存地点

            odt2.Columns.Add(new DataColumn("CHARG", typeof(string)));//批号
            odt2.Columns.Add(new DataColumn("LICHN", typeof(string)));//供应商的批次
            odt2.Columns.Add(new DataColumn("KDMAT", typeof(string)));//客户物料
            odt2.Columns.Add(new DataColumn("PRODH", typeof(string)));//产品层次
            odt2.Columns.Add(new DataColumn("LFIMG", typeof(string)));//实际已交货量（按销售单位）
            odt2.Columns.Add(new DataColumn("VRKME", typeof(string)));//销售单位

            ods.Tables.Add(odt2);

            //执行RFC函数
            bool ret = rfc.DownloadByRFC(funcName, import, ids, ref ods);
            //根据RFC执行后返回的数据处理业务逻辑 
            //DataTable dt = ods.Tables["T_ITEM"];
            foreach (DataRow h in ods.Tables["T_HEADER"].Rows)
            {
                var bill = new SaleBill();
                try
                {
                    string type = h["LFART"].ToString();
                    string state = h["WBSTK"].ToString();

                    bill.No = h["VBELN"].ToString();
                    bill.BillDate = DateTime.Now;
                    bill.Location = h["VSTEL"].ToString();
                    bill.Org = h["VKORG"].ToString();
                    //bill.Location = h["LFART"].ToString();
                    DateTime planDate, deliveryDate;
                    if (DateTime.TryParse(h["WADAT"].ToString(), out planDate))
                        bill.PlanDate = Convert.ToDateTime(h["WADAT"].ToString());
                    //else
                    //    bill.PlanDate = Convert.ToDateTime(h["WADAT"].ToString());

                    if (DateTime.TryParse(h["LFDAT"].ToString(), out deliveryDate))
                        bill.DeliveryDate = Convert.ToDateTime(h["LFDAT"].ToString());
                    //bill.State = h["WBSTK"].ToString();

                    foreach (DataRow i in ods.Tables["T_ITEM"].AsEnumerable().Where(p => h["VBELN"].ToString() == p["VBELN"].ToString()))
                    {
                        var billDtl = new SaleBillDetail()
                        {
                            Item = new Item { Code = i["MATNR"].ToString() },
                            ProjectNo = i["POSNR"].ToString(),
                            Qty = Convert.ToDouble(i["LFIMG"] ?? 0),
                            Unit = i["VRKME"].ToString(),
                            Factory = i["WERKS"].ToString(),
                            BatchNo = i["CHARG"].ToString(),
                            LotNo = i["LICHN"].ToString(),
                            Warehouse = i["LGORT"].ToString(),
                            BillType = i["PSTYV"].ToString(),
                            ProductLevel = i["PRODH"].ToString(),
                        };
                        bill.SaleBillDetailList.Add(billDtl);
                    }
                    lst.Add(bill);

                }
                catch (Exception e)
                {
                    continue;
                }
            }
            return lst;
        }

    }
}
