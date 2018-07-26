using BD.Organizations;
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
    public static class RFC_Org
    {
        /// <summary>
        /// 下载工厂
        /// </summary>
        /// <param name="workOrderNo"></param>
        /// <returns></returns>
        public static IEnumerable<Organization> GetOrgByCode(string InvOrg)
        {
            var lt = new List<Organization>();
            Hashtable import = new Hashtable();
            //import.Add("I_BUKRS", InvOrg.ToString());//公司代码
           // import.Add("I_WERKS", InvOrg);//工厂代码
            lt.AddRange(ERP_MES_Org(import));
            return lt;
        }
        private static List<Organization> ERP_MES_Org(Hashtable import)
        {
            RFC rfc = new RFC(ConfigurationManager.AppSettings["sapServer"].ToString());
            //RFC rfc = new RFC("SAPTest");
            var lst = new List<Organization>();
            var group = DomainControllerFactory.Create<OrganizationController>().GetList(1).Concrete().FirstOrDefault();
            //RFC调用函数名
            string funcName = "ZWMS_T001W";
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
            odt1.Columns.Add(new DataColumn("ORGID", typeof(string)));//组织编码
            odt1.Columns.Add(new DataColumn("ORGNAME", typeof(string)));//组织名称
            odt1.Columns.Add(new DataColumn("PR_ORGID", typeof(string)));//上层编码
            odt1.Columns.Add(new DataColumn("PR_ORGNAME", typeof(string)));//上层名称
            odt1.Columns.Add(new DataColumn("WTYPE", typeof(string)));//组织类型
            ods.Tables.Add(odt1);

            //执行RFC函数
            bool ret = rfc.DownloadByRFC(funcName, import, ids, ref ods);
            //根据RFC执行后返回的数据处理业务逻辑 
            DataTable dt = ods.Tables["T_TAB"];
            //for (int i = 0; i < dt.Rows.Count; i++)
            foreach (DataRow organ in ods.Tables["T_TAB"].Rows)
            {
                //string code = dt.Rows[i]["ORGID"].ToString();
                //string name = dt.Rows[i]["ORGNAME"].ToString();
                //string prCode = dt.Rows[i]["WERKS"].ToString();
                //string prName = dt.Rows[i]["NAME1"].ToString();
                //string type = dt.Rows[i]["WTYPE"].ToString();
                string code = organ["ORGID"].ToString();
                string name = organ["ORGNAME"].ToString();
                string prCode = organ["PR_ORGID"].ToString();
                string prName = organ["PR_ORGNAME"].ToString();
                string type = organ["WTYPE"].ToString().Trim();

                var org = new Organization()
                {
                    Code = code,
                    Name = name,
                    CreateBy = PlatformEnvironment.IdentityId,
                    CreateDate = DateTime.Now,
                    InvOrgId = PlatformEnvironment.InvOrgId
                };
                if (type == "公司")
                {
                    org.LevelId = 1;
                }
                else if (type == "工厂")
                {
                    org.LevelId = 2;
                    org.TreeParent = group;
                }
                lst.Add(org);
            }
            return lst;
        }
    }
}
