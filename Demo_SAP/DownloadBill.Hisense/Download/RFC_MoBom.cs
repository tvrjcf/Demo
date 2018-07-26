using BD.Items;
using INV.Hisense.Warehouses.Warehouses;
using Platform.Domain;
using SAP_Class;
using SFC.Hisense.WorkOrders;
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
    /// 工单Bom下载
    /// </summary>
    public static class RFC_MoBom
    {
        /// <summary>
        /// 根据工单号下载工单BOM
        /// </summary>
        /// <param name="workOrderNo"></param>
        /// <returns></returns>
        public static IEnumerable<WorkOrderBom> GetErpMoBom(string workOrderNo)
        {
            //1.获取工单Bom
            var lt = new List<WorkOrderBom>();
            Hashtable import = new Hashtable();
            //import.Add("I_BUKRS", factoryCode);//工厂编码
            import.Add("I_AUFNR", workOrderNo);//工单号
            lt.AddRange(ERP_MES_MoBom(import));
            return lt;
        }
        private static List<WorkOrderBom> ERP_MES_MoBom(Hashtable import)
        {
            RFC rfc = new RFC(ConfigurationManager.AppSettings["sapServer"].ToString());
            //RFC rfc = new RFC("SAPTest");
            var lst = new List<WorkOrderBom>();
            //RFC调用函数名
            string funcName = "ZWMS_MOBOM";
            //传输传给RFC函数的DataSet
            DataSet ids = new DataSet();
            //构建RFC传入表DataTable

            //返回数据的DataSet框架
            DataSet ods = new DataSet();
            //构建Export参数DataTable
            DataTable export = new DataTable();
            export.TableName = "Export";
            export.Columns.Add(new DataColumn("RTYPE", typeof(string)));//处理标识
            export.Columns.Add(new DataColumn("RTMSG", typeof(string)));//消息文本
            ods.Tables.Add(export);
            //构建RFC传出表DataTable
            DataTable odt1 = new DataTable();
            odt1.TableName = "T_TAB";
            // odt1.Columns.Add(new DataColumn("WERKS", typeof(string)));//工厂
            odt1.Columns.Add(new DataColumn("AUFNR", typeof(string)));//工单号
            odt1.Columns.Add(new DataColumn("MATNR", typeof(string)));//产品代码

            odt1.Columns.Add(new DataColumn("IDNRK", typeof(string)));//BOM 组件
            odt1.Columns.Add(new DataColumn("MAKTX", typeof(string)));//物料描述
            odt1.Columns.Add(new DataColumn("NOMNG", typeof(string)));//定额消耗数量
            odt1.Columns.Add(new DataColumn("ERFMG", typeof(string)));//组件数量
            odt1.Columns.Add(new DataColumn("MEINS", typeof(string)));//组件计量单位
            odt1.Columns.Add(new DataColumn("LGORT", typeof(string)));//库存地点发料
            odt1.Columns.Add(new DataColumn("XLOEK", typeof(string)));//删除标志
            odt1.Columns.Add(new DataColumn("RGEKZ", typeof(string)));//是否反冲
            ods.Tables.Add(odt1);
            //执行RFC函数
            bool ret = rfc.DownloadByRFC(funcName, import, ids, ref ods);
            //根据RFC执行后返回的数据处理业务逻辑 
            foreach (DataRow h in ods.Tables["T_TAB"].Rows)
            {
                if (h["XLOEK"].ToString().Trim().ToUpper() == "X")
                {
                    continue;
                }
                string mo = h["AUFNR"].ToString();
                string item_code = h["IDNRK"].ToString();
                string qty = h["ERFMG"].ToString();
                string singe_qty = h["NOMNG"].ToString();
                string warehouse = h["LGORT"].ToString();
                string unit = h["MEINS"].ToString();

                var bom = new WorkOrderBom();
                bom.WorkOrder = new WorkOrder { WorkOrderNo = mo };
                bom.Item = new Item { Code = item_code };
                bom.RequireQty = Convert.ToDouble(qty);
                bom.SingleQty = Convert.ToDouble(singe_qty);
                bom.SourceWarehouse = new Warehouse { Code = warehouse };
                bom.Unit = unit;
                bom.IsRecoilItem = h["RGEKZ"].ToString().Trim().ToUpper() == "X";
                lst.Add(bom);
            }
            return lst;
        }
    }
}
