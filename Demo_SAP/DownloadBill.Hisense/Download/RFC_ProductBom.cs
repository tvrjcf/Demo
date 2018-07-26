using BD.Items;
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
    public static class RFC_ProductBom
    {
        public static IEnumerable<ProductBom> GetErpBom(string itemCode)
        {
            var lt = new List<ProductBom>();
            Hashtable import = new Hashtable();
            import.Add("I_BUKRS", PlatformEnvironment.InvOrgId.Value.ToSafeString());//公司编码
            //import.Add("I_WERKS", "");//工厂编码
            import.Add("I_MATNR", itemCode);//物料编码
            lt.AddRange(ERP_MES_Bom(import));
            return lt;
        }
        private static List<ProductBom> ERP_MES_Bom(Hashtable import)
        {
            RFC rfc = new RFC(ConfigurationManager.AppSettings["sapServer"].ToString());
            //RFC rfc = new RFC("SAPTest");
            var lst = new List<ProductBom>();
            //RFC调用函数名
            string funcName = "ZWMS_BOM";
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
            odt1.TableName = "T_HEADER";
            odt1.Columns.Add(new DataColumn("WERKS", typeof(string)));//工厂
            odt1.Columns.Add(new DataColumn("MATNR", typeof(string)));//物料代码
            odt1.Columns.Add(new DataColumn("VERID", typeof(string)));//生产版本
            //odt1.Columns.Add(new DataColumn("ZTEXT", typeof(string)));//BOM 文本
            //odt1.Columns.Add(new DataColumn("STKTX", typeof(string)));//可选文本
            odt1.Columns.Add(new DataColumn("LOEKZ", typeof(string)));//BOM 删除标志
            ods.Tables.Add(odt1);

            DataTable odt2 = new DataTable();
            odt2.TableName = "T_ITEM";
            odt2.Columns.Add(new DataColumn("MATNR", typeof(string)));//物料代码
            odt2.Columns.Add(new DataColumn("IDNRK", typeof(string)));//BOM 组件
            odt2.Columns.Add(new DataColumn("MAKTX", typeof(string)));//物料描述（短文本）
            odt2.Columns.Add(new DataColumn("MENGE", typeof(string)));//组件数量
            odt2.Columns.Add(new DataColumn("MEINS", typeof(string)));//组件计量单位
            odt2.Columns.Add(new DataColumn("LGORT", typeof(string)));//库存地点           
            odt2.Columns.Add(new DataColumn("RGEKZ", typeof(string)));//是否反冲           
            ods.Tables.Add(odt2);

            //执行RFC函数
            bool ret = rfc.DownloadByRFC(funcName, import, ids, ref ods);
            //根据RFC执行后返回的数据处理业务逻辑 
            //DataTable dt = ods.Tables["T_ITEM"];
            foreach (DataRow h in ods.Tables["T_HEADER"].Rows)
            {
                var bom = new ProductBom();
                string code = h["MATNR"].ToString();
                string ver = h["VERID"].ToString();
                string flag = h["LOEKZ"].ToString();
                bom.Item = new Item { Code = code };
                bom.Name = code;
                bom.Code = code;
                bom.Version = ver;
                foreach (DataRow i in ods.Tables["T_ITEM"].AsEnumerable().Where(p => h["MATNR"].ToString() == p["MATNR"].ToString()))
                {
                    var resover = i["RGEKZ"].ToString();
                    var bomDetail = new ProductBomDetail()
                    {
                        Item = new Item { Code = i["IDNRK"].ToString() },
                        UnitQty = Convert.ToDouble(i["MENGE"] ?? 0),
                        Unit = i["MEINS"].ToString(),
                        Rgekz = resover.IsNullOrEmpty() || resover == "0" ? 0 : 1,
                        Warehouse = i["LGORT"].ToSafeString()
                    };
                    bom.DetailList.Add(bomDetail);
                }
                lst.Add(bom);
            }
            return lst;
        }
    }
}
