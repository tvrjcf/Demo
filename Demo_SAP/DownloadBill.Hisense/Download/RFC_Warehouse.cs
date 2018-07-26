using BD.Customers;
using BD.Organizations;
using INV.Hisense.Warehouses.Warehouses;
using Platform;
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
    public static class RFC_Warehouse
    {
        public static IEnumerable<Warehouse> DownloadWarehouse(string wh_code)
        {
            //1.获取ERP仓库
            //2.返回仓库类型Warehouse
            var lt = new List<Warehouse>();
            Hashtable import = new Hashtable();
            import.Add("I_BUKRS", PlatformEnvironment.InvOrgId);//公司
            import.Add("I_WERKS", "");//工厂            
            //import.Add("I_LGORT", "");//库存地点
            //import.Add("I_ALL", "X");//X 全部  空取日志表数据
            lt.AddRange(ERP_MES_Warehouse(import));
            return lt;
        }
        private static List<Warehouse> ERP_MES_Warehouse(Hashtable import)
        {
            RFC rfc = new RFC(ConfigurationManager.AppSettings["sapServer"].ToString());
            //RFC rfc = new RFC("SAPTest");
            var lst = new List<Warehouse>();
            //RFC调用函数名
            string funcName = "ZWMS_T001L";
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
            odt1.Columns.Add(new DataColumn("WERKS", typeof(string)));//工厂编码
            odt1.Columns.Add(new DataColumn("LGORT", typeof(string)));//库存地编码
            odt1.Columns.Add(new DataColumn("LGOBE", typeof(string)));//库存地描述
            odt1.Columns.Add(new DataColumn("XBUFX", typeof(string)));//冻结状态            
            ods.Tables.Add(odt1);

            //执行RFC函数
            bool ret = rfc.DownloadByRFC(funcName, import, ids, ref ods);
            //根据RFC执行后返回的数据处理业务逻辑 
            DataTable dt = ods.Tables["T_TAB"];
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                string factoryCode = dt.Rows[i]["WERKS"].ToString();
                string code = dt.Rows[i]["LGORT"].ToString();
                string name = dt.Rows[i]["LGOBE"].ToString();
                string state = dt.Rows[i]["XBUFX"].ToString();
                var warehouse = new Warehouse { Code = code, Name = name, Factory = new Organization { Code = factoryCode } };
                //if (factoryCode == "4200")
                //{
                //    warehouse.FactoryId = 2; 
                //}
                //else
                //{

                //}
                lst.Add(warehouse);
            }
            return lst;
        }

    }
}
