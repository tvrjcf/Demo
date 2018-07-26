using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UpLoadJob.Models
{
    public class ZmesRfcProdordconfHeader
    {
        public ZmesRfcProdordconfHeader() {

        }
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="Aufnr">订单号/工单号 (必填)</param>
        /// <param name="Menge">数量 (数量与报废数量 至少填一个)</param>
        /// <param name="Werks">工厂 (必填)</param>
        /// <param name="Mesmb">MES移库订单号 (必填)</param>
        /// <param name="Bfmenge">报废数量 (数量与报废数量 至少填一个)</param>
        public ZmesRfcProdordconfHeader(string Aufnr, string Menge, string Werks, string Mesmb, string Bfmenge)
        {
            AUFNR = Aufnr;
            MENGE = Menge;
            WERKS = Werks;
            MESMB = Mesmb;
            BFMENGE = Bfmenge;
            BUDAT = "";
            CHARG = "";
            ZCONFTYPE = "";
            BFMATNR = "";
            LGORT = "";

        }

        /// <summary>
        /// 订单号/工单号
        /// </summary>
        public string AUFNR { get; set; }
        /// <summary>
        /// 数量
        /// </summary>
        public string MENGE { get; set; }
        /// <summary>
        /// 凭证中的过帐日期
        /// </summary>
        public string BUDAT { get; set; }
        /// <summary>
        /// 批号
        /// </summary>
        public string CHARG { get; set; }
        /// <summary>
        /// 工厂
        /// </summary>
        public string WERKS { get; set; }
        /// <summary>
        /// MES移库订单号
        /// </summary>
        public string MESMB { get; set; }
        /// <summary>
        /// 报废数量
        /// </summary>
        public string BFMENGE { get; set; }
        /// <summary>
        /// 报工类型 1 报废成品收货 2 报废散料 3散料转成品
        /// </summary>
        public string ZCONFTYPE { get; set; }
        /// <summary>
        /// 报废成品编码
        /// </summary>
        public string BFMATNR { get; set; }
        /// <summary>
        /// 维修仓
        /// </summary>
        public string LGORT { get; set; }


        public static string SerializeObject(ZmesRfcProdordconfHeader detail)
        {
            return JsonConvert.SerializeObject(detail);
        }
        public static ZmesRfcProdordconfHeader DeserializeObject(string value)
        {
            return JsonConvert.DeserializeObject<ZmesRfcProdordconfHeader>(value);
        }
        public static string SerializeObjectT(List<ZmesRfcProdordconfHeader> ListDetail)
        {
            return JsonConvert.SerializeObject(ListDetail);
        }
        public static List<ZmesRfcProdordconfHeader> DeserializeObjectT(string value)
        {
            return JsonConvert.DeserializeObject<List<ZmesRfcProdordconfHeader>>(value);
        }
    }
}
