using INV.Hisense.Transactions;
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
    public static class RFC_MoveReason
    {
        public static IEnumerable<Reasons> GetErpMoveReason(string code)
        {
            //1.获取ERP 移动原因
            //2.返回移动原因 TNS_REASONS
            List<Reasons> lt = new List<Reasons>();
            Hashtable import = new Hashtable();
            import.Add("I_BWART", "");//移动类型
            import.Add("I_GRUND", code);//移动原因
            lt.AddRange(ERP_MES_MoveReason(import));
            return lt;
        }
        private static List<Reasons> ERP_MES_MoveReason(Hashtable import)
        {
            RFC rfc = new RFC(ConfigurationManager.AppSettings["sapServer"].ToString());
            //RFC rfc = new RFC("SAPTest");
            var lst = new List<Reasons>();
            //RFC调用函数名
            string funcName = "ZWMS_T157D";
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
            odt1.Columns.Add(new DataColumn("BWART", typeof(string)));//移动类型
            odt1.Columns.Add(new DataColumn("GRUND", typeof(string)));//移动原因
            odt1.Columns.Add(new DataColumn("GRTXT", typeof(string)));//移动原因 
            ods.Tables.Add(odt1);

            //执行RFC函数
            bool ret = rfc.DownloadByRFC(funcName, import, ids, ref ods);
            //根据RFC执行后返回的数据处理业务逻辑 
            DataTable dt = ods.Tables["T_TAB"];
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                string type = dt.Rows[i]["BWART"].ToString();
                string reason = dt.Rows[i]["GRUND"].ToString();
                string txt = dt.Rows[i]["GRTXT"].ToString();
                var model = new Reasons() { Description = txt, Name = reason, MoveType = type };
                lst.Add(model);
            }
            return lst;
        }
    }
}
