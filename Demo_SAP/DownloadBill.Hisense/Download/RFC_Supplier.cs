using BD.Suppliers;
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
    public static class RFC_Supplier
    {
        public static IEnumerable<Supplier> GetErpSupplier(string code)
        {
            //1.获取ERP供应商
            //2.返回供应商类型Supplier
            var lt = new List<Supplier>();
            Hashtable import = new Hashtable();
            import.Add("I_BUKRS", PlatformEnvironment.InvOrgId);//公司代码
            import.Add("I_LIFNR", code);//供应商编码
            import.Add("I_START_DT", "");//日期从
            import.Add("I_END_DT", "");//日期至
            import.Add("I_ALL", "");//X 全部  空取日志表数据
            lt.AddRange(ERP_MES_Supplier(import));
            return lt;
        }
        private static List<Supplier> ERP_MES_Supplier(Hashtable import)
        {
            RFC rfc = new RFC(ConfigurationManager.AppSettings["sapServer"].ToString());
            //RFC rfc = new RFC("SAPTest");
            var lst = new List<Supplier>();
            //RFC调用函数名
            string funcName = "ZWMS_LFA1";
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
            odt1.Columns.Add(new DataColumn("BUKRS", typeof(string)));//公司代码
            odt1.Columns.Add(new DataColumn("LIFNR", typeof(string)));//供应商编码
            odt1.Columns.Add(new DataColumn("TXT30", typeof(string)));//供应商类型描述
            odt1.Columns.Add(new DataColumn("NAME1", typeof(string)));//供应商名称
            odt1.Columns.Add(new DataColumn("SPERZ", typeof(string)));//付款冻结
            odt1.Columns.Add(new DataColumn("KTOKK", typeof(string)));//供应商帐户组
            ods.Tables.Add(odt1);

            //执行RFC函数
            bool ret = rfc.DownloadByRFC(funcName, import, ids, ref ods);
            //根据RFC执行后返回的数据处理业务逻辑 
            DataTable dt = ods.Tables["T_TAB"];
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                string code = dt.Rows[i]["LIFNR"].ToString();
                string type = dt.Rows[i]["TXT30"].ToString();
                string desc = dt.Rows[i]["KTOKK"].ToString();
                string name = dt.Rows[i]["NAME1"].ToString();
                name = string.IsNullOrEmpty(name) ? code : name;
                string frozen = dt.Rows[i]["SPERZ"].ToString();
                var supplier = new Supplier
                {
                    Code = code,
                    Description = desc,
                    Name = name,
                    ExpiryDate = DateTime.Now.AddYears(1000),
                    Type = type
                };
                lst.Add(supplier);
            }
            return lst;
        }
    }
}
