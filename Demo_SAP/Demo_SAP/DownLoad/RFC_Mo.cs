using BD.Organizations;
using Platform;
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
    /// 工单下载
    /// </summary>
    public static class RFC_Mo
    {
        /// <summary>
        /// 根据工单号下载工单
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<WorkOrder> GetMoByNo(string workOrderNo)
        {
            //var factories = DomainControllerFactory.Create<OrganizationController>().GetList(2);
            var factory = new Organization { Code = "4200" };
            //var factories = new OrganizationList { factory };
            var ls = new List<WorkOrder>();
            //foreach (var item in factories)
            //{
                List<WorkOrder> lt = new List<WorkOrder>();
                Hashtable import = new Hashtable();
                import.Add("I_WERKS", "4200");//工厂
                import.Add("I_AUFNR", workOrderNo);//单号
                import.Add("I_START_DT", workOrderNo.IsNullOrEmpty() ? DateTime.Now.AddMonths(-1).ToString("yyyyMMdd") : "");//开始日期
                import.Add("I_END_DT", workOrderNo.IsNullOrEmpty() ? DateTime.Now.ToString("yyyyMMdd") : "");//结束日期
                lt.AddRange(ERP_Get_Mo(import));
                ls.AddRange(lt);
            //}
            return ls;
        }
        private static List<WorkOrder> ERP_Get_Mo(Hashtable import)
        {
            RFC rfc = new RFC(ConfigurationManager.AppSettings["sapServer"].ToString());
            //RFC rfc = new RFC("SAPTest");
            var lst = new List<WorkOrder>();
            //RFC调用函数名
            string funcName = "ZWMS_MO";
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
            odt1.TableName = "T_TAB";
            odt1.Columns.Add(new DataColumn("WERKS", typeof(string)));//工厂
            odt1.Columns.Add(new DataColumn("AUFNR", typeof(string)));//订单号 
            odt1.Columns.Add(new DataColumn("PLNBEZ", typeof(string)));//物料号 
            odt1.Columns.Add(new DataColumn("MAKTX", typeof(string)));//物料描述（短文本）
            odt1.Columns.Add(new DataColumn("GAMNG", typeof(string)));//订单数量
            odt1.Columns.Add(new DataColumn("VAPLZ", typeof(string)));//维护任务的工作中心  
            odt1.Columns.Add(new DataColumn("LGORT", typeof(string)));//收货库存地点  
            odt1.Columns.Add(new DataColumn("FTRMS", typeof(string)));//计划下达日期  
            odt1.Columns.Add(new DataColumn("FTRMI", typeof(string)));//实际下达日期  
            odt1.Columns.Add(new DataColumn("AUART", typeof(string)));//订单类型  
            odt1.Columns.Add(new DataColumn("ERDAT", typeof(string)));//创建日期  
            odt1.Columns.Add(new DataColumn("VERID", typeof(string)));//生产版本  
            odt1.Columns.Add(new DataColumn("GUANBI", typeof(string)));//是否关闭
            odt1.Columns.Add(new DataColumn("BEIZHU", typeof(string)));//备注
            odt1.Columns.Add(new DataColumn("WEMPF", typeof(string)));//车间
            odt1.Columns.Add(new DataColumn("STATE", typeof(string)));//状态

            ods.Tables.Add(odt1);
            //执行RFC函数
            bool ret = rfc.DownloadByRFC(funcName, import, ids, ref ods);
            //根据RFC执行后返回的数据处理业务逻辑 
            DataTable dt = ods.Tables["T_TAB"];
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                string factory = dt.Rows[i]["WERKS"].ToString();//工厂
                string work_order_no = dt.Rows[i]["AUFNR"].ToString();//工单号
                string item_code = dt.Rows[i]["PLNBEZ"].ToString();//物料编号
                string work_center = dt.Rows[i]["VAPLZ"].ToString();//工作中心
                string version = dt.Rows[i]["VERID"].ToString();//生产版本
                double order_qty = 0;
                double.TryParse(dt.Rows[i]["GAMNG"].ToString(), out order_qty);//订单数量

                string storage_location = dt.Rows[i]["LGORT"].ToString();//库存地点
                DateTime plan_release_date;
                DateTime.TryParse(dt.Rows[i]["FTRMS"].ToString(), out plan_release_date);//计划下达日期
                DateTime act_begin_date;
                DateTime.TryParse(dt.Rows[i]["FTRMI"].ToString(), out act_begin_date);//实际下达日期
                string work_order_type = dt.Rows[i]["AUART"].ToString();//订单类型
                DateTime create_date;
                DateTime.TryParse(dt.Rows[i]["ERDAT"].ToString(), out create_date);//创建日期
                bool closed = dt.Rows[i]["GUANBI"].ToString().IsNullOrEmpty() ? false : true;
                string bz = dt.Rows[i]["BEIZHU"].ToSafeString();//备注
                string sapState = dt.Rows[i]["STATE"].ToSafeString();//sap状态
                string workShop = dt.Rows[i]["WEMPF"].ToSafeString();//车间

                //var model = new WorkOrder()
                //{
                //    Factory = factory,
                //    WorkOrderNo = work_order_no,
                //    Item = new BD.Items.Item { Code = item_code },
                //    Workshop = new BD.Organizations.Organization { Code = workShop },
                //    OrderQty = (int)order_qty,
                //    ReceiptWarehouse = new INV.Hisense.Warehouses.Warehouses.Warehouse { Code = storage_location },
                //    PlanBeginDate = plan_release_date,
                //    ActuFinishDate = act_begin_date,
                //    S_Type = work_order_type,
                //    CreateDate = create_date,
                //    ProductVersion = version,
                //    Remark = bz,
                //    IsClosed = closed,
                //    Status = sapState.Contains("TECO") ? BD.WorkOrders.WorkOrderStatus.Close : BD.WorkOrders.WorkOrderStatus.Release
                //};

                //lst.Add(model);
            }
            return lst;
        }
    }
}
