using SAP_Class;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UpLoadJob.Models;

namespace Demo_SAP
{
    public class SAP
    {

        public RFC GetSapServer()
        {
            string server = ConfigurationManager.AppSettings["sapServer"].ToString();
            if (string.IsNullOrEmpty(server))
                throw new Exception("sapServer 未配置");
            return new RFC(server);
        }

        /// <summary>
        /// 获取SAP上传接口启用状态
        /// </summary>
        /// <returns></returns>
        private Result GetSapState()
        {
            Result result = new Result { Success = true };
            //var isEnable = ConfigurationHelper.GetAppSettingOrDefault("IsEnableSap", "true");
            string isEnable = ConfigurationManager.AppSettings["sapServer"].ToString();
            if (string.IsNullOrEmpty(isEnable)) isEnable = "true";
            result.Success = isEnable.ToLower().Equals("true");
            string sapTime = "sap" + DateTime.Now.ToString("yyyyMMdd-HHmmssffff");
            result.Message = result.Success ? sapTime : sapTime;
            return result;
        }

        /// <summary>
        /// 获取采购单RT标签，失败返回 null
        /// </summary>
        /// <returns></returns>
        public Result GetPoRTNo(Hashtable import)
        {
            var ret = GetSapState();
            if (!ret.Success)
            {
                ret.Success = true;
                return ret;
            }
            RFC rfc = GetSapServer();
            //RFC rfc = new RFC("SAPTest");
            //var lst = new List<PurchaseOrder>();
            //RFC调用函数名
            string funcName = "ZWMS_GOODS_MOVE_NEW";
            //传输传给RFC函数的DataSet
            DataSet ids = new DataSet();
            //构建RFC传入表DataTable


            DataTable idt1 = new DataTable();
            idt1.TableName = "IO_ITEM";
            idt1.Columns.Add(new DataColumn("EBELN", typeof(string)));//订单号
            idt1.Columns.Add(new DataColumn("EBELP", typeof(string)));//采购订单项目编号 
            idt1.Columns.Add(new DataColumn("MATNR", typeof(string)));//物料编码
            idt1.Columns.Add(new DataColumn("MENGE", typeof(string)));//数量
            idt1.Columns.Add(new DataColumn("UMWRK", typeof(string)));//工厂
            idt1.Columns.Add(new DataColumn("UMLGO", typeof(string)));//仓库
            idt1.Rows.Add("4570000608", "20", "3099999998", "5", "4200", "1101");
            ids.Tables.Add(idt1);

            //返回数据的DataSet框架
            DataSet ods = new DataSet();
            //构建Export参数DataTable
            DataTable export = new DataTable();
            export.TableName = "Export";
            export.Columns.Add(new DataColumn("RTYPE", typeof(string)));//处理标识
            export.Columns.Add(new DataColumn("RTMSG", typeof(string)));//消息文本
            ods.Tables.Add(export);
            //构建RFC传出表DataTable
            //DataTable odt1 = new DataTable();
            //odt1.TableName = "RET_ITEM";
            //odt1.Columns.Add(new DataColumn("BUKRS", typeof(string)));//订单号
            //odt1.Columns.Add(new DataColumn("MESBL", typeof(string)));//采购凭证号
            //odt1.Columns.Add(new DataColumn("MBLNR", typeof(string)));//地址
            //ods.Tables.Add(odt1);

            //执行RFC函数            
            bool resultRFC = rfc.UploadByRFC(funcName, import, ids, ref ods);
            //根据RFC执行后返回的数据处理业务逻辑 

            //
            Result result = new Result { Success = false };
            string rtNo = null;
            if (resultRFC && ods.Tables["Export"].Rows.Count > 0)
            {
                result.Success = ods.Tables["Export"].Rows[0]["RTYPE"].ToString() == "S";
                result.Message = ods.Tables["Export"].Rows[0]["RTMSG"].ToString();
            }
            return result;
        }



        /// <summary>
        /// ZWMS_GOODS_MOVE_NEW接口
        /// </summary>
        /// <param name="inputheader">输入参数头</param>
        /// <param name="inputDetail">接口明细定义</param>
        /// <returns></returns>
        public Result ZWMS_GOODS_MOVE_NEW(ZwmsGoodsMoveNewHeader inputheader, ZwmsGoodsMoveNewDetail inputDetail)
        {
            var ret = GetSapState();
            if (!ret.Success)
            {
                ret.Success = true;
                return ret;
            }

            RFC rfc = GetSapServer();
            //RFC调用函数名
            string funcName = "ZWMS_GOODS_MOVE_NEW";
            //import
            Hashtable import = new Hashtable();
            import.Add("WMSNO", inputheader.MESMB);//WMS移库订单号
            import.Add("WMSTY", inputheader.MESBS);//WMS移库流程类型 
            import.Add("BLDAT", inputheader.BLDAT);//凭证中的凭证日期,可选
            import.Add("BKTXT", inputheader.BKTXT);//凭证抬头文本,可选  
            import.Add("KUNNR", inputheader.KUNNR);//凭证抬头文本,可选   
            import.Add("LIFNR", inputheader.LIFNR);//凭证抬头文本,可选   
            import.Add("XBLNR", inputheader.XBLNR);//凭证抬头文本,可选    

            //传输传给RFC函数的DataSet
            DataSet ids = new DataSet();
            //构建RFC传入表DataTable
            DataTable idt1 = new DataTable();

            List<ZwmsGoodsMoveNewDetail> listDetail = new List<ZwmsGoodsMoveNewDetail>() { inputDetail };
            idt1 = DataTableExtensions.ToDataTable<ZwmsGoodsMoveNewDetail>(listDetail);
            idt1.TableName = "IO_ITEM";

            ids.Tables.Add(idt1);

            //返回数据的DataSet框架
            DataSet ods = new DataSet();
            //构建Export参数DataTable
            DataTable export = new DataTable();
            export.TableName = "Export";
            export.Columns.Add(new DataColumn("RTYPE", typeof(string)));//处理标识
            export.Columns.Add(new DataColumn("RTMSG", typeof(string)));//消息文本
            ods.Tables.Add(export);

            Result result = new Result { Success = false };
            try
            {
                //执行RFC函数            
                bool resultRFC = rfc.UploadByRFC(funcName, import, ids, ref ods);
                //根据RFC执行后返回的数据处理业务逻辑 

                if (resultRFC && ods.Tables["Export"].Rows.Count > 0)
                {
                    result.Success = ods.Tables["Export"].Rows[0]["RTYPE"].ToString() == "S";
                    result.Message = ods.Tables["Export"].Rows[0]["RTMSG"].ToString();
                }

            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = ex.Message;
            }
            return result;
        }

        /// <summary>
        /// ZWMS_GOODS_MOVE_NEW接口
        /// </summary>
        /// <param name="inputheader">输入参数头</param>
        /// <param name="listInputDetail">接口明细定义列表</param>
        /// <returns></returns>
        public Result ZWMS_GOODS_MOVE_NEW(ZwmsGoodsMoveNewHeader inputheader, List<ZwmsGoodsMoveNewDetail> listInputDetail)
        {
            var ret = GetSapState();
            if (!ret.Success)
            {
                ret.Success = true;
                return ret;
            }

            RFC rfc = GetSapServer();
            //RFC调用函数名
            string funcName = "ZWMS_GOODS_MOVE_NEW";
            //import
            Hashtable import = new Hashtable();
            import.Add("WMSNO", inputheader.MESMB);//WMS移库订单号
            import.Add("WMSTY", inputheader.MESBS);//WMS移库流程类型 
            import.Add("BLDAT", inputheader.BLDAT);//凭证中的凭证日期,可选
            import.Add("BKTXT", inputheader.BKTXT);//凭证抬头文本,可选   
            import.Add("KUNNR", inputheader.KUNNR);//凭证抬头文本,可选   
            import.Add("LIFNR", inputheader.LIFNR);//凭证抬头文本,可选   
            import.Add("XBLNR", inputheader.XBLNR);//凭证抬头文本,可选   

            //传输传给RFC函数的DataSet
            DataSet ids = new DataSet();
            //构建RFC传入表DataTable
            DataTable idt1 = new DataTable();

            //List<ZwmsGoodsMoveNewDetail> listDetail = new List<ZwmsGoodsMoveNewDetail>() { inputDetail };
            idt1 = DataTableExtensions.ToDataTable<ZwmsGoodsMoveNewDetail>(listInputDetail);
            idt1.TableName = "IO_ITEM";

            ids.Tables.Add(idt1);

            //返回数据的DataSet框架
            DataSet ods = new DataSet();
            //构建Export参数DataTable
            DataTable export = new DataTable();
            export.TableName = "Export";
            export.Columns.Add(new DataColumn("RTYPE", typeof(string)));//处理标识
            export.Columns.Add(new DataColumn("RTMSG", typeof(string)));//消息文本
            ods.Tables.Add(export);

            Result result = new Result { Success = false };
            try
            {
                //执行RFC函数            
                bool resultRFC = rfc.UploadByRFC(funcName, import, ids, ref ods);
                //根据RFC执行后返回的数据处理业务逻辑 

                if (resultRFC && ods.Tables["Export"].Rows.Count > 0)
                {
                    result.Success = ods.Tables["Export"].Rows[0]["RTYPE"].ToString() == "S";
                    result.Message = ods.Tables["Export"].Rows[0]["RTMSG"].ToString();
                }

            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = ex.Message;
            }
            return result;
        }

        /// <summary>
        /// ZMES_RFC_PRODORDCONF接口
        /// 成品-MO报工入库 
        /// </summary>
        /// <param name="inputheader">输入参数头</param>
        /// <param name="inputDetail">接口明细定义</param>
        /// <returns></returns>
        public Result ZMES_RFC_PRODORDCONF(ZmesRfcProdordconfHeader inputheader, ZmesRfcProdordconfDetail inputDetail)
        {
            var ret = GetSapState();
            if (!ret.Success)
            {
                ret.Success = true;
                return ret;
            }

            //inputheader = new ZmesRfcProdordconfHeader("12345", "1", "4200", "637", "0");
            RFC rfc = GetSapServer();
            //RFC调用函数名
            string funcName = "ZMES_RFC_PRODORDCONF";
            //import
            Hashtable import = new Hashtable();
            import.Add("AUFNR", inputheader.AUFNR);//订单号(必填)
            import.Add("MENGE", inputheader.MENGE);//数量 (数量与报废数量 至少填一个) 1
            import.Add("BUDAT", inputheader.BUDAT);//凭证中的过帐日期
            import.Add("CHARG", inputheader.CHARG);//批号  
            import.Add("WERKS", inputheader.WERKS);//工厂(必填)
            import.Add("MESMB", inputheader.MESMB);//MES移库订单号(必填)
            import.Add("BFMENGE", inputheader.BFMENGE);//报废数量数量(数量与报废数量 至少填一个)
            import.Add("ZCONFTYPE", inputheader.ZCONFTYPE);//报工类型 1 报废成品收货 2 报废散料 3散料转成品
            import.Add("BFMATNR", inputheader.BFMATNR);//报废成品编码
            import.Add("LGORT", inputheader.LGORT);//维修仓  



            //传输传给RFC函数的DataSet
            DataSet ids = new DataSet();
            //构建RFC传入表DataTable
            DataTable idt1 = new DataTable();

            List<ZmesRfcProdordconfDetail> listDetail = new List<ZmesRfcProdordconfDetail>() { inputDetail };
            idt1 = DataTableExtensions.ToDataTable<ZmesRfcProdordconfDetail>(listDetail);
            idt1.TableName = "ERROR_RETURN";

            ids.Tables.Add(idt1);

            //返回数据的DataSet框架
            DataSet ods = new DataSet();
            //构建Export参数DataTable
            DataTable export = new DataTable();
            export.TableName = "Export";
            export.Columns.Add(new DataColumn("CONF_NO", typeof(string)));//操作完成的确认编号 
            export.Columns.Add(new DataColumn("CONF_CNT", typeof(string)));//确认计数器 
            export.Columns.Add(new DataColumn("RTYPE", typeof(string)));//消息类型: S 成功,E 错误
            export.Columns.Add(new DataColumn("RTMSG", typeof(string)));//消息文本
            export.Columns.Add(new DataColumn("MBLNR", typeof(string)));//物料凭证编号
            export.Columns.Add(new DataColumn("RAUFNR", typeof(string)));//物料凭证编号
            ods.Tables.Add(export);

            Result result = new Result { Success = false };
            try
            {
                //执行RFC函数            
                bool resultRFC = rfc.UploadByRFC(funcName, import, ids, ref ods);
                //根据RFC执行后返回的数据处理业务逻辑 

                if (resultRFC && ods.Tables["Export"].Rows.Count > 0)
                {
                    result.Success = ods.Tables["Export"].Rows[0]["RTYPE"].ToString() == "S";
                    if (result.Success)
                        result.Message = ods.Tables["Export"].Rows[0]["MBLNR"].ToString();
                    else
                        result.Message = ods.Tables["Export"].Rows[0]["RTMSG"].ToString();
                }

            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = ex.Message;
            }
            return result;
        }

        /// <summary>
        /// ZMES_RFC_PRODORDCONF接口
        /// 成品-MO报工入库 
        /// </summary>
        /// <param name="inputheader">输入参数头</param>
        /// <param name="listInputDetail">接口明细定义列表</param>
        /// <returns></returns>
        public Result ZMES_RFC_PRODORDCONF(ZmesRfcProdordconfHeader inputheader, List<ZmesRfcProdordconfDetail> listInputDetail)
        {
            var ret = GetSapState();
            if (!ret.Success)
            {
                ret.Success = true;
                return ret;
            }

            //inputheader = new ZmesRfcProdordconfHeader("12345", "1", "4200", "637", "0");
            RFC rfc = GetSapServer();
            //RFC调用函数名
            string funcName = "ZMES_RFC_PRODORDCONF";
            //import
            Hashtable import = new Hashtable();
            import.Add("AUFNR", inputheader.AUFNR);//订单号(必填)
            import.Add("MENGE", inputheader.MENGE);//数量 (数量与报废数量 至少填一个) 1
            import.Add("BUDAT", inputheader.BUDAT);//凭证中的过帐日期
            import.Add("CHARG", inputheader.CHARG);//批号  
            import.Add("WERKS", inputheader.WERKS);//工厂(必填)
            import.Add("MESMB", inputheader.MESMB);//MES移库订单号(必填)
            import.Add("BFMENGE", inputheader.BFMENGE);//报废数量数量(数量与报废数量 至少填一个)
            import.Add("ZCONFTYPE", inputheader.ZCONFTYPE);//报工类型 1 报废成品收货 2 报废散料 3散料转成品
            import.Add("BFMATNR", inputheader.BFMATNR);//报废成品编码
            import.Add("LGORT", inputheader.LGORT);//维修仓  



            //传输传给RFC函数的DataSet
            DataSet ids = new DataSet();
            //构建RFC传入表DataTable
            DataTable idt1 = new DataTable();

            // List<ZmesRfcProdordconfDetail> listDetail = new List<ZmesRfcProdordconfDetail>() { inputDetail };
            idt1 = DataTableExtensions.ToDataTable<ZmesRfcProdordconfDetail>(listInputDetail);
            idt1.TableName = "ERROR_RETURN";

            ids.Tables.Add(idt1);

            //返回数据的DataSet框架
            DataSet ods = new DataSet();
            //构建Export参数DataTable
            DataTable export = new DataTable();
            export.TableName = "Export";
            export.Columns.Add(new DataColumn("CONF_NO", typeof(string)));//操作完成的确认编号 
            export.Columns.Add(new DataColumn("CONF_CNT", typeof(string)));//确认计数器 
            export.Columns.Add(new DataColumn("RTYPE", typeof(string)));//消息类型: S 成功,E 错误
            export.Columns.Add(new DataColumn("RTMSG", typeof(string)));//消息文本
            export.Columns.Add(new DataColumn("MBLNR", typeof(string)));//物料凭证编号
            export.Columns.Add(new DataColumn("RAUFNR", typeof(string)));//物料凭证编号
            ods.Tables.Add(export);

            Result result = new Result { Success = false };
            try
            {
                //执行RFC函数            
                bool resultRFC = rfc.UploadByRFC(funcName, import, ids, ref ods);
                //根据RFC执行后返回的数据处理业务逻辑 

                if (resultRFC && ods.Tables["Export"].Rows.Count > 0)
                {
                    result.Success = ods.Tables["Export"].Rows[0]["RTYPE"].ToString() == "S";
                    if (result.Success)
                        result.Message = ods.Tables["Export"].Rows[0]["MBLNR"].ToString();
                    else
                        result.Message = ods.Tables["Export"].Rows[0]["RTMSG"].ToString();
                }

            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = ex.Message;
            }
            return result;
        }

        /// <summary>
        /// ZWMS_DN_CONF接口
        /// 成品发货
        /// </summary>
        /// <param name="inputheader">输入参数头</param>
        /// <param name="listInputDetail">接口明细定义列表</param>
        /// <returns></returns>
        public Result ZWMS_DN_CONF(ZwmsDnConfHeader inputheader, List<ZwmsDnConfDetail> listInputDetail)
        {
            var ret = GetSapState();
            if (!ret.Success)
            {
                ret.Success = true;
                return ret;
            }

            RFC rfc = GetSapServer();
            //RFC调用函数名
            string funcName = "ZWMS_DN_CONF";
            //import
            Hashtable import = new Hashtable();


            //传输传给RFC函数的DataSet
            DataSet ids = new DataSet();
            //构建RFC传入表DataTable
            DataTable idt1 = new DataTable();

            idt1 = DataTableExtensions.ToDataTable<ZwmsDnConfDetail>(listInputDetail);
            idt1.TableName = "T_TAB";

            ids.Tables.Add(idt1);

            //返回数据的DataSet框架
            DataSet ods = new DataSet();
            //构建Export参数DataTable
            DataTable export = new DataTable();
            export.TableName = "Export";
            export.Columns.Add(new DataColumn("RTYPE", typeof(string)));//消息类型: S 成功,E 错误
            export.Columns.Add(new DataColumn("RTMSG", typeof(string)));//消息文本
            ods.Tables.Add(export);

            Result result = new Result { Success = false };
            try
            {
                //执行RFC函数            
                bool resultRFC = rfc.UploadByRFC(funcName, import, ids, ref ods);

                //根据RFC执行后返回的数据处理业务逻辑 
                if (resultRFC && ods.Tables["Export"].Rows.Count > 0)
                {
                    result.Success = ods.Tables["Export"].Rows[0]["RTYPE"].ToString() == "S";
                    result.Message = ods.Tables["Export"].Rows[0]["RTMSG"].ToString();
                }
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = ex.Message;
            }
            return result;
        }

        /// <summary>
        /// ZWMS_MFBF 接口
        /// 重复制造成品反冲入库（MFBF）
        /// </summary>
        /// <param name="inputheader">输入参数头</param>
        /// <param name="listInputDetail">接口明细定义列表</param>
        /// <returns></returns>
        public Result ZWMS_MFBF(ZwmsMfbfHeader inputheader, List<ZwmsGoodsMoveNewDetail> listInputDetail)
        {
            var ret = GetSapState();
            if (!ret.Success)
            {
                ret.Success = true;
                return ret;
            }

            RFC rfc = GetSapServer();
            //RFC调用函数名
            string funcName = "ZWMS_DN_CONF";
            //import
            Hashtable import = new Hashtable();
            import.Add("WMSNO", inputheader.MESMB);//WMS移库订单号,必输
            import.Add("WMSTY", inputheader.MESBS);//WMS移库流程类型 
            import.Add("BLDAT", inputheader.BLDAT);//可选，格式YYYYMMDD 默认当前日期
            import.Add("I_MATNR", inputheader.I_MATNR);//物料号,131 物料   
            import.Add("I_MENGE", inputheader.I_MENGE);//数量,131 数量   
            import.Add("I_LGORT", inputheader.I_LGORT);//库存地点,必填   
            import.Add("I_VERID", inputheader.I_VERID);//生产版本,可选,默认0001  
            import.Add("I_WERKS", inputheader.I_WERKS);//工厂,必填

            //传输传给RFC函数的DataSet
            DataSet ids = new DataSet();
            //构建RFC传入表DataTable
            DataTable idt1 = new DataTable();

            idt1 = DataTableExtensions.ToDataTable<ZwmsGoodsMoveNewDetail>(listInputDetail);
            idt1.TableName = "IO_ITEM";

            ids.Tables.Add(idt1);

            //返回数据的DataSet框架
            DataSet ods = new DataSet();
            //构建Export参数DataTable
            DataTable export = new DataTable();
            export.TableName = "Export";
            export.Columns.Add(new DataColumn("RTYPE", typeof(string)));//消息类型: S 成功,E 错误
            export.Columns.Add(new DataColumn("RTMSG", typeof(string)));//消息文本
            ods.Tables.Add(export);

            Result result = new Result { Success = false };
            try
            {
                //执行RFC函数            
                bool resultRFC = rfc.UploadByRFC(funcName, import, ids, ref ods);

                //根据RFC执行后返回的数据处理业务逻辑 
                if (resultRFC && ods.Tables["Export"].Rows.Count > 0)
                {
                    result.Success = ods.Tables["Export"].Rows[0]["RTYPE"].ToString() == "S";
                    result.Message = ods.Tables["Export"].Rows[0]["RTMSG"].ToString();
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

    public static class DataTableExtensions
    {
        /// <summary>    
        /// 转化一个DataTable    
        /// </summary>    
        /// <typeparam name="T"></typeparam>    
        /// <param name="list"></param>    
        /// <returns></returns>    
        public static DataTable ToDataTable<T>(this IEnumerable<T> list)
        {

            //创建属性的集合    
            List<PropertyInfo> pList = new List<PropertyInfo>();
            //获得反射的入口    

            Type type = typeof(T);
            DataTable dt = new DataTable();
            //把所有的public属性加入到集合 并添加DataTable的列    
            Array.ForEach<PropertyInfo>(type.GetProperties(), p => { pList.Add(p); dt.Columns.Add(p.Name, p.PropertyType); });
            foreach (var item in list)
            {
                //创建一个DataRow实例    
                DataRow row = dt.NewRow();
                //给row 赋值    
                pList.ForEach(p => row[p.Name] = p.GetValue(item, null));
                //加入到DataTable    
                dt.Rows.Add(row);
            }
            return dt;
        }
    }
}
