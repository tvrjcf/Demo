using BD.Items;
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
    public static class RFC_ItemGroup
    {
        public static IEnumerable<ItemSmallCategory> GetErpItem(string code)
        {
            //1.获取ERP物料
            //2.返回RiFengWms物料类型Item
            var lt = new List<ItemSmallCategory>();
            Hashtable import = new Hashtable();
            import.Add("I_MATKL", code);//物料租编码
            lt.AddRange(ERP_MES_Category(import));
            return lt;
        }

        private static List<ItemSmallCategory> ERP_MES_Category(Hashtable import)
        {
            RFC rfc = new RFC(ConfigurationManager.AppSettings["sapServer"].ToString());
            //RFC rfc = new RFC("SAPTest");
            var lst = new List<ItemSmallCategory>();
            //RFC调用函数名
            string funcName = "ZWMS_T023";
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
            odt1.Columns.Add(new DataColumn("MATKL", typeof(string)));//物料组代码
            odt1.Columns.Add(new DataColumn("WGBEZ", typeof(string)));//物料组描述 
            ods.Tables.Add(odt1);

            //执行RFC函数
            bool ret = rfc.DownloadByRFC(funcName, import, ids, ref ods);
            //根据RFC执行后返回的数据处理业务逻辑 
            DataTable dt = ods.Tables["T_TAB"];
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                string code = dt.Rows[i]["MATKL"].ToString();
                string name = dt.Rows[i]["WGBEZ"].ToString();
                name = string.IsNullOrEmpty(name) ? code : name;
                var cat = new ItemSmallCategory { Code = code, Name = name, MediumCategory = new ItemMediumCategory { Code = code, Name = name } };
                lst.Add(cat);
            }
            return lst;
        }

    }
}
