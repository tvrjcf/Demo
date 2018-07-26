using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UpLoadJob.Models
{
    /// <summary>
    /// 接口输入参数明细
    /// </summary>
    public class ZwmsGoodsMoveNewDetail
    {
        private ZwmsGoodsMoveNewDetail _detail;
        public ZwmsGoodsMoveNewDetail()
        {
            MESBL = "";
            MATNR = "";
            UMMAT = "";
            MENGE = "";
            WERKS = "";
            LGORT = "";
            UMWRK = "";
            UMLGO = "";
            SGTXT = "";
            CHARG = "";
            KOSTL = "";
            PRCTR = "";
            GRUND = "";
            AUFNR = "";
            KFBER = "";
            EBELN = "";
            EBELP = "";
            RSNUM = "";
            RSPOS = "";
            KZEAR = "";
            //MBLNR = "";
            //ZEILE = "";
            //ZBLNR = "";
            BFWERKS = "";
            BFLGORT = "";

        }

        /// <summary>
        /// WMS移库订单项目号 
        /// </summary>
        public string MESBL { get; set; }
        /// <summary>
        /// 发货物料编号
        /// </summary>
        public string MATNR { get; set; }
        /// <summary>
        /// 收货/发货物料
        /// </summary>
        public string UMMAT { get; set; }
        /// <summary>
        /// 数量
        /// </summary>
        public string MENGE { get; set; }
        /// <summary>
        /// 发货工厂
        /// </summary>
        public string WERKS { get; set; }
        /// <summary>
        /// 发货库存地点
        /// </summary>
        public string LGORT { get; set; }
        /// <summary>
        /// 收货工厂
        /// </summary>
        public string UMWRK { get; set; }
        /// <summary>
        /// 收货库存地点
        /// </summary>
        public string UMLGO { get; set; }
        /// <summary>
        /// 项目文本
        /// </summary>
        public string SGTXT { get; set; }
        /// <summary>
        /// 批号
        /// </summary>
        public string CHARG { get; set; }
        /// <summary>
        /// 成本中心
        /// </summary>
        public string KOSTL { get; set; }
        /// <summary>
        /// 利润中心
        /// </summary>
        public string PRCTR { get; set; }
        /// <summary>
        /// 移动原因
        /// </summary>
        public string GRUND { get; set; }
        /// <summary>
        /// 工单号
        /// </summary>
        public string AUFNR { get; set; }
        /// <summary>
        /// 功能范围
        /// </summary>
        public string KFBER { get; set; }
        /// <summary>
        /// 采购订单号PO
        /// </summary>
        public string EBELN { get; set; }
        /// <summary>
        /// 采购订单项目编号 
        /// </summary>
        public string EBELP { get; set; }
        /// <summary>
        /// 预留/相关需求的编号
        /// </summary>
        public string RSNUM { get; set; }
        /// <summary>
        /// 预留/相关需求的项目编号
        /// </summary>
        public string RSPOS { get; set; }
        /// <summary>
        /// 该预留的最后发货
        /// </summary>
        public string KZEAR { get; set; }
        /// <summary>
        /// 物料凭证号（RT）
        /// </summary>
        //public string MBLNR { get; set; }
        /// <summary>
        /// 物料凭证项目号
        /// </summary>
        //public string ZEILE { get; set; }
        /// <summary>
        /// 外协单号
        /// </summary>
        //public string ZBLNR { get; set; }
        /// <summary>
        /// 报废工厂
        /// </summary>
        public string BFWERKS { get; set; }
        /// <summary>
        /// 报废库存地点
        /// </summary>
        public string BFLGORT { get; set; }


        public static string SerializeObject(ZwmsGoodsMoveNewDetail detail)
        {
            return JsonConvert.SerializeObject(detail);
        }
        public static string SerializeObjectT(List<ZwmsGoodsMoveNewDetail> listDetail)
        {
            return JsonConvert.SerializeObject(listDetail);
        }
        public static ZwmsGoodsMoveNewDetail DeserializeObject(string value)
        {
            return JsonConvert.DeserializeObject<ZwmsGoodsMoveNewDetail>(value);
        }

        public static List<ZwmsGoodsMoveNewDetail> DeserializeObjectT(string value)
        {
            return JsonConvert.DeserializeObject<List<ZwmsGoodsMoveNewDetail>>(value);
        }
    }
}
