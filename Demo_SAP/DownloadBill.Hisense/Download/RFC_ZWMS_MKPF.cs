using BD.Customers;
using Platform;
using SAP.Middleware.Connector;
using SAP_Class;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;

namespace DownloadBill.Hisense.Download
{
    public static class RFC_ZWMS_MKPF
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


        /// <summary>
        /// 物料凭证查询
        /// </summary>
        /// <param name="I_BKTXT">凭证抬头文本(凭证号与抬头文本至少填一个)</param>
        /// <param name="I_MBLNR">物料凭证(凭证号与抬头文本至少填一个)</param>
        /// <param name="I_MJAHR">物料凭证年度(选填)</param>
        /// <returns></returns>
        public static Result ZWMS_MKPF(string I_BKTXT, string I_MBLNR = "", string I_MJAHR = "")
        {

            Result result = new Result { Success = false };
            try
            {
                //RfcDestination rfcDest = RfcDestinationManager.GetDestination(GetSapServerString());
                RfcDestination rfcDest = RfcDestinationManager.GetDestination(ConfigurationManager.AppSettings["sapServer"].ToString());

                //选择要调用的BAPI的名称
                RfcFunctionMetadata rfMD = rfcDest.Repository.GetFunctionMetadata("ZWMS_MKPF");
                //新建调用该BAPI的一个“实例”
                IRfcFunction irF = rfMD.CreateFunction();

                //******************************
                //输入参数设置
                //******************************

                irF.SetValue("I_MBLNR", I_MBLNR);   //物料凭证
                irF.SetValue("I_MJAHR", I_MJAHR);    //物料凭证年度
                irF.SetValue("I_BKTXT", I_BKTXT);    //凭证抬头文本
                
                IRfcTable irt = irF.GetTable("O_MKPF");

                RfcSessionManager.BeginContext(rfcDest);

                irF.Invoke(rfcDest);
                //irfCommit.Invoke(rfcDest);

                RfcSessionManager.EndContext(rfcDest);

                result.Success = irF.GetString("RTYPE").ToUpper() == "S";
                result.Message = irF.GetValue("RTMSG").ToString();
                if (result.Success && irt.RowCount > 0)
                {
                    var MBLNR = "";
                    for (int i = 0; i < irt.RowCount; i++)
                    {
                        irt.CurrentIndex = i;
                        if (MBLNR.IsNotEmpty()) MBLNR += "/";
                        MBLNR += irt.GetString("MBLNR");
                    }
                    result.Message = MBLNR;
                }
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = ex.Message;
            }
            return result;
        }
    }
}
