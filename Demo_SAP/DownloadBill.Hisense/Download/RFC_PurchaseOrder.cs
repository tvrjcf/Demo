using BD.Items;
using PT.Hisense.PurchaseOrders;
using SAP_Class;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DownloadBill.Hisense.Download
{
    /// <summary>
    /// 下载采购单
    /// </summary>
    public static class RFC_PurchaseOrder
    {
        public static IEnumerable<PurchaseOrder> GetErpPo(string OrgCode, string PoNo)
        {
            //1.获取ERP物料
            //2.返回RiFengWms物料类型Item`
            var lt = new List<PurchaseOrder>();
            Hashtable import = new Hashtable();
            import.Add("I_BUKRS", OrgCode);//公司代码
            import.Add("I_EBELN", PoNo);//采购单号 
            //import.Add("I_START_DT", "");//开始日期
            //import.Add("I_END_DT", "");//结束日期
            lt.AddRange(ERP_MES_Po(import));
            return lt;
        }
        private static List<PurchaseOrder> ERP_MES_Po(Hashtable import)
        {
            RFC rfc = new RFC(ConfigurationManager.AppSettings["sapServer"].ToString());
            //RFC rfc = new RFC("SAPTest");
            var lst = new List<PurchaseOrder>();
            //RFC调用函数名
            string funcName = "ZWMS_PO";
            //传输传给RFC函数的DataSet
            DataSet ids = new DataSet();
            //构建RFC传入表DataTable

            //返回数据的DataSet框架
            DataSet ods = new DataSet();
            //构建Export参数DataTable
            DataTable export = new DataTable();
            export.TableName = "Export";
            export.Columns.Add(new DataColumn("RTMSG", typeof(string)));//消息文本
            ods.Tables.Add(export);
            //构建RFC传出表DataTable
            DataTable odt1 = new DataTable();
            odt1.TableName = "T_EKKO";
            odt1.Columns.Add(new DataColumn("BUKRS", typeof(string)));//公司代码
            odt1.Columns.Add(new DataColumn("EBELN", typeof(string)));//采购凭证号
            odt1.Columns.Add(new DataColumn("LIFNR", typeof(string)));//供应商或债权人的帐号
            odt1.Columns.Add(new DataColumn("ZADRESS", typeof(string)));//地址
            odt1.Columns.Add(new DataColumn("SSQSS", typeof(string)));//质检控制码
            odt1.Columns.Add(new DataColumn("AEDAT", typeof(string)));//记录的创建日期
            odt1.Columns.Add(new DataColumn("LAND1", typeof(string)));//国家代码
            odt1.Columns.Add(new DataColumn("ORT01", typeof(string)));//城市
            odt1.Columns.Add(new DataColumn("ORT02", typeof(string)));//地区
            odt1.Columns.Add(new DataColumn("STRAS", typeof(string)));//住宅号及街道
            odt1.Columns.Add(new DataColumn("LANDX", typeof(string)));//国家名称
            ods.Tables.Add(odt1);


            DataTable odt2 = new DataTable();
            odt2.TableName = "T_EKPO";
            odt2.Columns.Add(new DataColumn("EBELN", typeof(string)));//采购凭证号
            odt2.Columns.Add(new DataColumn("EBELP", typeof(string)));//采购凭证的项目编号
            odt2.Columns.Add(new DataColumn("MATNR", typeof(string)));//物料号
            odt2.Columns.Add(new DataColumn("SSQSS", typeof(string)));//质检控制码
            odt2.Columns.Add(new DataColumn("WERKS", typeof(string)));//工厂
            odt2.Columns.Add(new DataColumn("LGORT", typeof(string)));//库存地点
            odt2.Columns.Add(new DataColumn("MENGE", typeof(string)));//采购订单数量
            odt2.Columns.Add(new DataColumn("MEINS", typeof(string)));//采购订单的计量单位
            odt2.Columns.Add(new DataColumn("RETPO", typeof(string)));//退货标识 X为退货
            odt2.Columns.Add(new DataColumn("PSTYP", typeof(string)));//采购项目类别
            odt2.Columns.Add(new DataColumn("INSMK", typeof(string)));//库存类型
            odt2.Columns.Add(new DataColumn("WAMNG", typeof(string)));//发货数量
            odt2.Columns.Add(new DataColumn("LOEKZ", typeof(string)));//删除标记
            odt2.Columns.Add(new DataColumn("ELIKZ", typeof(string)));//交货完成(接收完成)
            ods.Tables.Add(odt2);

            //执行RFC函数
            bool ret = rfc.DownloadByRFC(funcName, import, ids, ref ods);
            //根据RFC执行后返回的数据处理业务逻辑 
            //DataTable dt = ods.Tables["T_ITEM"];
            foreach (DataRow h in ods.Tables["T_EKKO"].Rows)
            {
                var po = new PurchaseOrder();
                string address = h["ZADRESS"].ToString();
                po.PoNumber = h["EBELN"].ToString();
                po.Supplier = new BD.Suppliers.Supplier { Code = h["LIFNR"].ToString() };
                po.CountryCode = h["LAND1"].ToString();
                po.CountryName = h["LANDX"].ToString();
                po.City = h["ORT01"].ToString();
                po.Area = h["ORT02"].ToString();
                po.Street = h["STRAS"].ToString();
                po.CreateDate = Convert.ToDateTime(h["AEDAT"].ToString());
                foreach (DataRow i in ods.Tables["T_EKPO"].AsEnumerable().Where(p => h["EBELN"].ToString() == p["EBELN"].ToString()))
                {
                    var poDetail = new PurchaseOrderDetail()
                    {
                        IqcFlag = i["SSQSS"].ToString().Trim(),
                        Item = new Item { Code = i["MATNR"].ToString() },
                        ProjectNo = i["EBELP"].ToString().Trim(),
                        Quantity = Convert.ToDouble(i["MENGE"] ?? 0),
                        Unit = i["MEINS"].ToString(),
                        Factory = i["WERKS"].ToString(),
                        Warehouse = i["LGORT"].ToString(),
                        PurchaseType = (PurchaseType?)Convert.ToInt32(i["PSTYP"]),
                        IsReturn = i["RETPO"].ToString().ToUpper() == "X",
                        ErpReceivedQty = Convert.ToDouble(i["WAMNG"] ?? 0),
                        State = i["ELIKZ"].ToString().ToUpper() == "X" ? PoState.Received : PoState.New,
                    };
                    po.PurchaseOrderDetailList.Add(poDetail);
                }
                lst.Add(po);
            }
            return lst;
        }
    }
}
