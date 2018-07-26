using BD.Customers;
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
    public static class RFC_Customer
    {
        public static IEnumerable<Customer> GetErpCustomer(string code)
        {
            //1.获取ERP客户
            //2.返回客户类型Customer
            var lt = new List<Customer>();
            Hashtable import = new Hashtable();
            import.Add("I_BUKRS", PlatformEnvironment.InvOrgId);//公司编码
            import.Add("I_KUNNR", code);//客户编码
            import.Add("I_START_DT", "");//日期从
            import.Add("I_END_DT", "");//日期至
            lt.AddRange(ERP_MES_Customer(import));
            return lt;
        }

        private static List<Customer> ERP_MES_Customer(Hashtable import)
        {
            RFC rfc = new RFC(ConfigurationManager.AppSettings["sapServer"].ToString());
            //RFC rfc = new RFC("SAPTest");
            var lst = new List<Customer>();
            //RFC调用函数名
            string funcName = "ZWMS_KNA1";
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
            odt1.TableName = "T_KNA1";
            odt1.Columns.Add(new DataColumn("BUKRS", typeof(string)));//公司代码
            odt1.Columns.Add(new DataColumn("KUNNR", typeof(string)));//客户编号1
            odt1.Columns.Add(new DataColumn("NAME1", typeof(string)));//名称 1            
            odt1.Columns.Add(new DataColumn("BZIRK", typeof(string)));//销售地区
            odt1.Columns.Add(new DataColumn("BZTXT", typeof(string)));//区名
            odt1.Columns.Add(new DataColumn("ABRVW", typeof(string)));//使用标识
            odt1.Columns.Add(new DataColumn("SORTL", typeof(string)));//排序字段
            ods.Tables.Add(odt1);

            //执行RFC函数
            bool ret = rfc.DownloadByRFC(funcName, import, ids, ref ods);
            //根据RFC执行后返回的数据处理业务逻辑 
            DataTable dt = ods.Tables["T_KNA1"];
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                string corp = dt.Rows[i]["BUKRS"].ToString();
                string code = dt.Rows[i]["KUNNR"].ToString();
                string name = dt.Rows[i]["NAME1"].ToString();
                string areaCode = dt.Rows[i]["BZIRK"].ToString();
                string areaName = dt.Rows[i]["BZTXT"].ToString();
                string state = dt.Rows[i]["ABRVW"].ToString();
                var customer = new Customer
                {
                    Code = code,
                    Name = name,
                    SalesArea = areaCode,
                    CreateBy = PlatformEnvironment.IdentityId,
                    CreateDate = DateTime.Now
                };
                lst.Add(customer);
            }
            return lst;
        }
    }
}
