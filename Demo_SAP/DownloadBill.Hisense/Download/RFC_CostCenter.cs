using BD.Factories;
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
    public static class RFC_CostCenter
    {
        public static IEnumerable<Factory> GetErpCostCenter(String code)
        {
            //1.获取ERP成本中心
            //2.返回成本中心 BD_FACTORY
            List<Factory> lt = new List<Factory>();
            Hashtable import = new Hashtable();
            import.Add("I_BUKRS", PlatformEnvironment.InvOrgId);//公司代码
            import.Add("I_KOSTL", code);//成本中心
            //import.Add("I_ALL", "X"); 下载所有
            lt.AddRange(ERP_MES_CostCenter(import));
            return lt;
        }
        private static List<Factory> ERP_MES_CostCenter(Hashtable import)
        {
            RFC rfc = new RFC(ConfigurationManager.AppSettings["sapServer"].ToString());
            //RFC rfc = new RFC("SAPTest");
            var lst = new List<Factory>();
            //RFC调用函数名
            string funcName = "ZWMS_CSKS";
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
            odt1.Columns.Add(new DataColumn("KOSTL", typeof(string)));//成本中心
            odt1.Columns.Add(new DataColumn("KTEXT", typeof(string)));//一般姓名 
            ods.Tables.Add(odt1);

            //执行RFC函数
            bool ret = rfc.DownloadByRFC(funcName, import, ids, ref ods);
            //根据RFC执行后返回的数据处理业务逻辑 
            DataTable dt = ods.Tables["T_TAB"];
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                string corpCode = dt.Rows[i]["BUKRS"].ToString();
                string costCode = dt.Rows[i]["KOSTL"].ToString();
                string name = dt.Rows[i]["KTEXT"].ToString();
                var model = new Factory() { Code = costCode, Name = name };

                lst.Add(model);
            }
            return lst;
        }
    }
}
