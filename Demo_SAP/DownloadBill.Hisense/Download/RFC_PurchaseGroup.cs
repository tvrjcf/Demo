using BD.Purchases;
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
    public static class RFC_PurchaseGroup
    {
        public static IEnumerable<PurchaseGroup> GetPurchases(string code)
        {
            //1.获取ERP 采购组
            //2.返回 采购组 
            List<PurchaseGroup> lt = new List<PurchaseGroup>();
            Hashtable import = new Hashtable();
            import.Add("I_EKGRP", code);//采购组编码 
            lt.AddRange(ERP_MES_Purchase(import));
            return lt;
        }
        private static List<PurchaseGroup> ERP_MES_Purchase(Hashtable import)
        {
            RFC rfc = new RFC(ConfigurationManager.AppSettings["sapServer"].ToString());
            //RFC rfc = new RFC("SAPTest");
            var lst = new List<PurchaseGroup>();
            //RFC调用函数名
            string funcName = "ZWMS_T024";
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
            odt1.Columns.Add(new DataColumn("EKGRP", typeof(string)));
            odt1.Columns.Add(new DataColumn("EKNAM", typeof(string)));
            odt1.Columns.Add(new DataColumn("EKTEL", typeof(string)));
            odt1.Columns.Add(new DataColumn("TELFX", typeof(string)));
            ods.Tables.Add(odt1);

            //执行RFC函数
            bool ret = rfc.DownloadByRFC(funcName, import, ids, ref ods);
            //根据RFC执行后返回的数据处理业务逻辑 
            DataTable dt = ods.Tables["T_TAB"];
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                string code = dt.Rows[i]["EKGRP"].ToString();
                string employee = dt.Rows[i]["EKNAM"].ToString();//采购者
                //DomainControllerFactory.Create<EmployeeController>().get
                string phone = dt.Rows[i]["EKTEL"].ToString();
                string fax = dt.Rows[i]["TELFX"].ToString();

                var model = new PurchaseGroup() { Code = code, buyerName = employee, Phone = phone, Fax = fax };
                lst.Add(model);
            }
            return lst;
        }
    }
}
