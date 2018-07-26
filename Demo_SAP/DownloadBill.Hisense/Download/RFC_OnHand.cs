using BD.Items;
using BD.Organizations;
using BD.Suppliers;
using INV.Hisense.WarehouseOnhands;
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
    /// 库存下载
    /// </summary>
    public static class RFC_OnHand
    {
        /// <summary>
        /// 下载SAP库存
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<WorkOrder> GetOnhand(string[] factories, string warehouseCode = "", string itemCode = "", string supplier = "", string sobkz = "")
        {
            //var factories = DomainControllerFactory.Create<OrganizationController>().GetList(2);
            //var factory = new Organization ();
            //factory.Code = "4230";
            //var factories = new OrganizationList { factory };
            var ls = new List<WorkOrder>();
            foreach (var factory in factories)
            {
                List<WorkOrder> lt = new List<WorkOrder>();
                Hashtable import = new Hashtable();
                import.Add("IWERKS", factory);    //工厂
                import.Add("ILGORT", warehouseCode);  //仓位
                import.Add("IMATNR", itemCode);   //物料编码
                import.Add("ILIFNR", supplier);   //供应商
                import.Add("ISOBKZ", sobkz);   //特殊库存标注
                lt.AddRange(ERP_Get_Onhand(import));
                ls.AddRange(lt);
                
            }
            return ls;
        }
        private static List<WorkOrder> ERP_Get_Onhand(Hashtable import)
        {
            RFC rfc = new RFC(ConfigurationManager.AppSettings["sapServer"].ToString());
            //RFC rfc = new RFC("SAPTest");
            var lst = new List<WorkOrder>();
            //RFC调用函数名
            string funcName = "ZMM_GETSTOCK_QTY";
            //传输传给RFC函数的DataSet
            DataSet ids = new DataSet();
            //构建RFC传入表DataTable

            //返回数据的DataSet框架
            DataSet ods = new DataSet();
            //构建Export参数DataTable
            //DataTable export = new DataTable();
            //export.TableName = "Export";
            //export.Columns.Add(new DataColumn("RTMSG", typeof(string)));//消息文本
            //ods.Tables.Add(export);
            //构建RFC传出表DataTable
            DataTable odt1 = new DataTable();
            odt1.TableName = "ZSTOCK";
            odt1.Columns.Add(new DataColumn("WERKS", typeof(string)));//工厂 
            odt1.Columns.Add(new DataColumn("LGORT", typeof(string)));//仓位  
            odt1.Columns.Add(new DataColumn("MATNR", typeof(string)));//物料号 
            odt1.Columns.Add(new DataColumn("LIFNR", typeof(string)));//供应商编码  
            odt1.Columns.Add(new DataColumn("SOBKZ", typeof(string)));//特殊库存  特殊库存标志（SOBKZ==K时，再看LGORT是否包含J，若包含J则是JIT物料，否则是客供。SOBKZ不等于K则是正常物料。）
            odt1.Columns.Add(new DataColumn("LABST", typeof(string)));//UU库存  
            odt1.Columns.Add(new DataColumn("INSME", typeof(string)));//QI库存  
            odt1.Columns.Add(new DataColumn("SPEME", typeof(string)));//冻结库存  
            //odt1.Columns.Add(new DataColumn("GUANBI", typeof(string)));//物料类型
            

            ods.Tables.Add(odt1);
            //执行RFC函数
            bool ret = rfc.DownloadByRFC(funcName, import, ids, ref ods);
            //根据RFC执行后返回的数据处理业务逻辑 
            DataTable dt = ods.Tables["ZSTOCK"];
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                string factory = dt.Rows[i]["WERKS"].ToString();//工厂
                string warehouse = dt.Rows[i]["LGORT"].ToString();//仓位
                string itemCode = dt.Rows[i]["MATNR"].ToString();//物料编号
                string supplier = dt.Rows[i]["LIFNR"].ToString();//供应商编码
                double k_qty = 0;
                double uu_qty = 0;
                double qi_qty = 0;
                double freeze_qty = 0;
                double.TryParse(dt.Rows[i]["SOBKZ"].ToString(), out k_qty);//特殊库存
                double.TryParse(dt.Rows[i]["LABST"].ToString(), out uu_qty);//UU库存
                double.TryParse(dt.Rows[i]["INSME"].ToString(), out qi_qty);//QI库存
                double.TryParse(dt.Rows[i]["SPEME"].ToString(), out freeze_qty);//冻结库存
                

                var model = new WorkOrder()
                {
                    
                };

                lst.Add(model);
            }
            return lst;
        }


        public static IEnumerable<ErpOnhand> GetErpOnhand(string[] factories, string[] warehouses = null, string[] items = null, string[] suppliers = null)
        {
            var ls = new List<ErpOnhand>();

            DataSet ids = new DataSet();
            if (factories != null && factories.Length > 0)   //工厂
            {
                var dt = new DataTable("T_WERKS");
                dt.Columns.Add("WERKS");
                foreach (var item in factories)
                {
                    dt.Rows.Add(item);
                }
                ids.Tables.Add(dt);
            }
            if (warehouses != null && warehouses.Length > 0)  //仓库
            {
                var dt = new DataTable("T_LGORT");
                dt.Columns.Add("LOC");
                foreach (var item in warehouses)
                {
                    dt.Rows.Add(item);
                }
                ids.Tables.Add(dt);
            }
            if (items != null && items.Length > 0)   //物料
            {
                var dt = new DataTable("T_MATNR");
                dt.Columns.Add("MAT");
                foreach (var item in items)
                {
                    dt.Rows.Add(item);
                }
                ids.Tables.Add(dt);
            }
            if (suppliers != null && suppliers.Length > 0)   //供应商
            {
                var dt = new DataTable("T_VENDOR");
                dt.Columns.Add("VENDOR");
                foreach (var item in suppliers)
                {
                    dt.Rows.Add(item);
                }
                ids.Tables.Add(dt);
            }

            ls.AddRange(DownloadErpOnhand(new Hashtable(), ids));
            return ls;
        }

        private static List<ErpOnhand> DownloadErpOnhand(Hashtable import, DataSet ids)
        {
            RFC rfc = new RFC(ConfigurationManager.AppSettings["sapServer"].ToString());
            //RFC rfc = new RFC("SAPTest");
            var lst = new List<ErpOnhand>();
            //RFC调用函数名
            string funcName = "Z_RFC_GET_INVENTORY";
            //传输传给RFC函数的DataSet
            //DataSet ids = new DataSet();

            ////工厂
            //DataTable idt1 = new DataTable();
            //idt1.TableName = "T_WERKS";
            //idt1.Columns.Add("WERKS");
            //idt1.Rows.Add(idt1.NewRow());
            //idt1.Rows[0][0] = "4233";
            //ids.Tables.Add(idt1);

            //仓库
            //DataTable idt2 = new DataTable();
            //idt2.TableName = "T_LGORT";
            //idt2.Columns.Add("LOC");
            //idt2.Rows.Add(idt2.NewRow());
            //idt2.Rows[0][0] = "A001";
            //idt2.Rows.Add(idt2.NewRow());
            //idt2.Rows[1][0] = "H001";
            //idt2.Rows.Add(idt2.NewRow());
            //idt2.Rows[2][0] = "WA09";
            //ids.Tables.Add(idt2);

            //物料
            //DataTable idt3 = new DataTable();
            //idt3.TableName = "T_MATNR";
            //idt3.Columns.Add("MAT");
            //idt3.Rows.Add(idt3.NewRow());
            //idt3.Rows[0][0] = "1030020101";
            //ids.Tables.Add(idt3);

            //供应商
            //DataTable idt4 = new DataTable();
            //idt4.TableName = "T_VENDOR";
            //idt4.Columns.Add("VENDOR");
            //idt4.Rows.Add(idt4.NewRow());
            //idt4.Rows[0][0] = "";
            //ids.Tables.Add(idt4);

            //构建RFC传入表DataTable

            //返回数据的DataSet框架
            DataSet ods = new DataSet();
            //构建Export参数DataTable
            //DataTable export = new DataTable();
            //export.TableName = "Export";
            //export.Columns.Add(new DataColumn("RTMSG", typeof(string)));//消息文本
            //ods.Tables.Add(export);
            //构建RFC传出表DataTable
            DataTable odt1 = new DataTable();
            odt1.TableName = "ET_STOCK";
            odt1.Columns.Add(new DataColumn("WERKS", typeof(string)));//工厂 
            odt1.Columns.Add(new DataColumn("LGORT", typeof(string)));//仓位  
            odt1.Columns.Add(new DataColumn("MATNR", typeof(string)));//物料 
            odt1.Columns.Add(new DataColumn("MAKTX", typeof(string)));//物料描述 
            odt1.Columns.Add(new DataColumn("LIFNR", typeof(string)));//供应商编码  
            //odt1.Columns.Add(new DataColumn("SOBKZ", typeof(string)));//特殊库存  特殊库存标志（SOBKZ==K时，再看LGORT是否包含J，若包含J则是JIT物料，否则是客供。SOBKZ不等于K则是正常物料。）
            odt1.Columns.Add(new DataColumn("LABST", typeof(string)));//UU库存  
            odt1.Columns.Add(new DataColumn("INSME", typeof(string)));//QI库存  
            odt1.Columns.Add(new DataColumn("SPEME", typeof(string)));//冻结库存  
            odt1.Columns.Add(new DataColumn("STOCK_T", typeof(string)));//库存类型


            ods.Tables.Add(odt1);
            //执行RFC函数
            bool ret = rfc.DownloadByRFC(funcName, new Hashtable(), ids, ref ods);
            //根据RFC执行后返回的数据处理业务逻辑 
            DataTable dt = ods.Tables["ET_STOCK"];
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                string factory = dt.Rows[i]["WERKS"].ToString();//工厂
                string warehouse = dt.Rows[i]["LGORT"].ToString();//仓位
                string itemCode = dt.Rows[i]["MATNR"].ToString();//物料编号
                string itemName = dt.Rows[i]["MAKTX"].ToString();//物料描述
                string supplierCode = dt.Rows[i]["LIFNR"].ToString();//供应商编码
                double k_qty = 0;
                double uu_qty = 0;
                double qi_qty = 0;
                double freeze_qty = 0;
                //double.TryParse(dt.Rows[i]["SOBKZ"].ToString(), out k_qty);//特殊库存
                double.TryParse(dt.Rows[i]["LABST"].ToString(), out uu_qty);//UU库存
                double.TryParse(dt.Rows[i]["INSME"].ToString(), out qi_qty);//QI库存
                double.TryParse(dt.Rows[i]["SPEME"].ToString(), out freeze_qty);//冻结库存
                string onhandType = dt.Rows[i]["STOCK_T"].ToString();//供应商编码

                var item = "";// DomainControllerFactory.Create<ItemController>().GetByCode(itemCode);
                var unit = ""; //item == null ? "" : item.MeasurementUnit;
                var supplier = ""; //DomainControllerFactory.Create<SupplierController>().GetSupplierByCode(supplierCode);
                var supplierName = "";// supplier == null ? "" : supplier.Name;

                var model = new ErpOnhand()
                {
                    Factory = factory,
                    ItemCode = itemCode,
                    ItemName = itemName,
                    WarehouseCode = warehouse,
                    Onhand = uu_qty,
                    CheckedOnhand = qi_qty,
                    FreezeOnhand = freeze_qty,
                    SupplierCode = supplierCode,
                    SupplierName = supplierName,
                    Unit = unit,
                    OnhandType = onhandType
                };

                lst.Add(model);
            }
            return lst;
        }
    }


    public static class RFC_OnHand2
    {
        /// <summary>
        /// 下载SAP库存
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<WorkOrder> GetOnhand(string[] factories, string[] warehouses, string[] items, string[] suppliers)
        {
            var ls = new List<WorkOrder>();

            DataSet ids = new DataSet();
            if (factories.Length > 0) {     //工厂
                var dt = new DataTable();
                dt.TableName = "T_WERKS";
                dt.Columns.Add("WERKS");
                foreach (var item in factories)
                {
                    var row = dt.NewRow();
                    row[0] = item;
                    dt.Rows.Add(row);
                }
                ids.Tables.Add(dt);
            }
            if (warehouses.Length > 0)
            {
                var dt = new DataTable();
                dt.TableName = "T_LGORT";
                dt.Columns.Add("LOC");
                foreach (var item in warehouses)
                {
                    var row = dt.NewRow();
                    row[0] = item;
                    dt.Rows.Add(row);
                }
                ids.Tables.Add(dt);
            }
            if (items.Length > 0)
            {
                var dt = new DataTable();
                dt.TableName = "T_MATNR";
                dt.Columns.Add("MAT");
                foreach (var item in items)
                {
                    var row = dt.NewRow();
                    row[0] = item;
                    dt.Rows.Add(row);
                }
                ids.Tables.Add(dt);
            }
            if (suppliers.Length > 0)
            {
                var dt = new DataTable();
                dt.TableName = "T_VENDOR";
                dt.Columns.Add("VENDOR");
                foreach (var item in suppliers)
                {
                    var row = dt.NewRow();
                    row[0] = item;
                    dt.Rows.Add(row);
                }
                ids.Tables.Add(dt);
            }

            ls = ERP_Get_Onhand(new Hashtable(), ids);
            return ls;
        }
        private static List<WorkOrder> ERP_Get_Onhand(Hashtable import, DataSet ids)
        {
            RFC rfc = new RFC(ConfigurationManager.AppSettings["sapServer"].ToString());
            //RFC rfc = new RFC("SAPTest");
            var lst = new List<WorkOrder>();
            //RFC调用函数名
            string funcName = "Z_RFC_GET_INVENTORY";
            //传输传给RFC函数的DataSet
            //DataSet ids = new DataSet();

            ////工厂
            //DataTable idt1 = new DataTable();
            //idt1.TableName = "T_WERKS";
            //idt1.Columns.Add("WERKS");
            //idt1.Rows.Add(idt1.NewRow());
            //idt1.Rows[0][0] = "4233";
            //ids.Tables.Add(idt1);

            //仓库
            //DataTable idt2 = new DataTable();
            //idt2.TableName = "T_LGORT";
            //idt2.Columns.Add("LOC");
            //idt2.Rows.Add(idt2.NewRow());
            //idt2.Rows[0][0] = "A001";
            //idt2.Rows.Add(idt2.NewRow());
            //idt2.Rows[1][0] = "H001";
            //idt2.Rows.Add(idt2.NewRow());
            //idt2.Rows[2][0] = "WA09";
            //ids.Tables.Add(idt2);

            //物料
            //DataTable idt3 = new DataTable();
            //idt3.TableName = "T_MATNR";
            //idt3.Columns.Add("MAT");
            //idt3.Rows.Add(idt3.NewRow());
            //idt3.Rows[0][0] = "1030020101";
            //ids.Tables.Add(idt3);

            //供应商
            //DataTable idt4 = new DataTable();
            //idt4.TableName = "T_VENDOR";
            //idt4.Columns.Add("VENDOR");
            //idt4.Rows.Add(idt4.NewRow());
            //idt4.Rows[0][0] = "";
            //ids.Tables.Add(idt4);

            //构建RFC传入表DataTable

            //返回数据的DataSet框架
            DataSet ods = new DataSet();
            //构建Export参数DataTable
            //DataTable export = new DataTable();
            //export.TableName = "Export";
            //export.Columns.Add(new DataColumn("RTMSG", typeof(string)));//消息文本
            //ods.Tables.Add(export);
            //构建RFC传出表DataTable
            DataTable odt1 = new DataTable();
            odt1.TableName = "ET_STOCK";
            odt1.Columns.Add(new DataColumn("WERKS", typeof(string)));//工厂 
            odt1.Columns.Add(new DataColumn("LGORT", typeof(string)));//仓位  
            odt1.Columns.Add(new DataColumn("MATNR", typeof(string)));//物料 
            odt1.Columns.Add(new DataColumn("MAKTX", typeof(string)));//物料描述 
            odt1.Columns.Add(new DataColumn("LIFNR", typeof(string)));//供应商编码  
            //odt1.Columns.Add(new DataColumn("SOBKZ", typeof(string)));//特殊库存  特殊库存标志（SOBKZ==K时，再看LGORT是否包含J，若包含J则是JIT物料，否则是客供。SOBKZ不等于K则是正常物料。）
            odt1.Columns.Add(new DataColumn("LABST", typeof(string)));//UU库存  
            odt1.Columns.Add(new DataColumn("INSME", typeof(string)));//QI库存  
            odt1.Columns.Add(new DataColumn("SPEME", typeof(string)));//冻结库存  
            odt1.Columns.Add(new DataColumn("STOCK_T", typeof(string)));//库存类型


            ods.Tables.Add(odt1);
            //执行RFC函数
            bool ret = rfc.DownloadByRFC(funcName, new Hashtable(), ids, ref ods);
            //根据RFC执行后返回的数据处理业务逻辑 
            DataTable dt = ods.Tables["ET_STOCK"];
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                string factory = dt.Rows[i]["WERKS"].ToString();//工厂
                string warehouse = dt.Rows[i]["LGORT"].ToString();//仓位
                string itemCode = dt.Rows[i]["MATNR"].ToString();//物料编号
                string supplier = dt.Rows[i]["LIFNR"].ToString();//供应商编码
                double k_qty = 0;
                double uu_qty = 0;
                double qi_qty = 0;
                double freeze_qty = 0;
                //double.TryParse(dt.Rows[i]["SOBKZ"].ToString(), out k_qty);//特殊库存
                double.TryParse(dt.Rows[i]["LABST"].ToString(), out uu_qty);//UU库存
                double.TryParse(dt.Rows[i]["INSME"].ToString(), out qi_qty);//QI库存
                double.TryParse(dt.Rows[i]["SPEME"].ToString(), out freeze_qty);//冻结库存


                var model = new WorkOrder()
                {

                };

                lst.Add(model);
            }
            return lst;
        }
    }
}
