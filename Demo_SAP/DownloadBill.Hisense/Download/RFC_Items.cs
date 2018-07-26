using BD.Items;
using Platform.Domain.Common;
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
    /// <summary>
    /// 下载物料
    /// </summary>
    public static class RFC_Items
    {
        public static IEnumerable<Item> GetItemByCode(string itemCode)
        {
            List<Item> lt = new List<Item>();
            Hashtable import = new Hashtable();
            import.Add("I_MATNR", itemCode);//物料编码
            //import.Add("I_DATAB", "");//开始日期
            //import.Add("I_LAEDA", "");//结束日期
            if (itemCode.IsNullOrEmpty())
                import.Add("I_ALL", "X");//X 全部  空取日志表数据
            lt.AddRange(ERP_Get_Item(import));
            return lt;
        }
        private static List<Item> ERP_Get_Item(Hashtable import)
        {
            RFC rfc = new RFC(ConfigurationManager.AppSettings["sapServer"].ToString());
            //RFC rfc = new RFC("SAPTest");
            var lst = new List<Item>();
            //RFC调用函数名
            string funcName = "ZWMS_MARA";
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
            odt1.TableName = "T_ITEM";
            odt1.Columns.Add(new DataColumn("MATNR", typeof(string)));//物料代码
            odt1.Columns.Add(new DataColumn("MAKTX", typeof(string)));//物料描述
            odt1.Columns.Add(new DataColumn("MEINS", typeof(string)));//基本单位
            odt1.Columns.Add(new DataColumn("MATKL", typeof(string)));//物料组
            odt1.Columns.Add(new DataColumn("WGBEZ", typeof(string)));//物料组名称
            odt1.Columns.Add(new DataColumn("MTART", typeof(string)));//物料类型
            odt1.Columns.Add(new DataColumn("DEL_FLG", typeof(string)));//删除标志
            odt1.Columns.Add(new DataColumn("CR_DATE", typeof(string)));//创建日期
            odt1.Columns.Add(new DataColumn("MDF_DATE", typeof(string)));//修改日期
            odt1.Columns.Add(new DataColumn("ZKHJX", typeof(string)));//客户型号
            odt1.Columns.Add(new DataColumn("ZJCXH", typeof(string)));//基本型号
            odt1.Columns.Add(new DataColumn("ZNM", typeof(string)));//内码
            odt1.Columns.Add(new DataColumn("ZFMATNR", typeof(string)));//成品替代料 
            odt1.Columns.Add(new DataColumn("ZZMATNR", typeof(string)));//MDM物料编码 
            odt1.Columns.Add(new DataColumn("ZCATEGORY", typeof(string)));//品类(小) 
            ods.Tables.Add(odt1);

            //执行RFC函数
            bool ret = rfc.DownloadByRFC(funcName, import, ids, ref ods);
            //根据RFC执行后返回的数据处理业务逻辑           

            foreach (DataRow item in ods.Tables["T_ITEM"].Rows)
            //for (int i = 0; i < dt.Rows.Count; i++)
            {
                string code = item["MATNR"].ToString();
                string desc = item["MAKTX"].ToString().Trim();
                string unit = item["MEINS"].ToString();
                string categoryCode = item["MATKL"].ToString();
                string categoryName = item["WGBEZ"].ToString();
                string type = item["MTART"].ToString();
                State state = item["MTART"].ToString() != "4" ? State.Enable : State.Disable;
                //DateTime crateDate = item["CR_DATE"].ToString() == "0000-00-00" ? Convert.ToDateTime("1900-1-1") : Convert.ToDateTime(item["CR_DATE"].ToString());
                //DateTime _MDF_DATE = item["MDF_DATE"].ToString() == "0000-00-00" ? (item["CR_DATE"].ToString() == "0000-0-0" ? Convert.ToDateTime("1900-1-1") : _CR_DATE) : Convert.ToDateTime(item["MDF_DATE"].ToString());
                var newItem = new Item
                {
                    Code = code,
                    Name = desc,
                    Description = desc,
                    MeasurementUnit = unit,
                    Category = new ItemSmallCategory { Code = categoryCode, Name = categoryName },
                    ItemType = type,
                    State = state,
                    CustomerType = item["ZKHJX"].ToString(),
                    BaseModel = item["ZJCXH"].ToString(),
                    InerCode = item["ZNM"].ToString(),
                    ReplaceItem = item["ZFMATNR"].ToString(),
                    MdmItemCode = item["ZZMATNR"].ToString(),
                    SmallCategory = item["ZCATEGORY"].ToString(),
                };
                lst.Add(newItem);
            }
            return lst;
        }
    }
}
