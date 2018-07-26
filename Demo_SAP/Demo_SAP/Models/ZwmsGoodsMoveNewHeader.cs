using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UpLoadJob.Models
{
    /// <summary>
    /// 接口输入参数头
    /// </summary>
    public class ZwmsGoodsMoveNewHeader
    {
        public ZwmsGoodsMoveNewHeader()
        {
            MESMB = "";
            MESBS = "";
            BLDAT = "";
            BKTXT = "";
            KUNNR = "";
            LIFNR = "";
            XBLNR = "";
        }
        public ZwmsGoodsMoveNewHeader(ZwmsGoodsMoveNewType zgmnType)
        {
            MESMB = "";
            MESBS = zgmnType.ToString().Replace("_", "");
            BLDAT = "";
            BKTXT = "";
            KUNNR = "";
            LIFNR = "";
            XBLNR = "";

        }
        public ZwmsGoodsMoveNewHeader(string mesbs)
        {
            MESMB = "";
            MESBS = mesbs;
            BLDAT = "";
            BKTXT = "";
            KUNNR = "";
            LIFNR = "";
            XBLNR = "";
        }
        /// <summary>
        /// WMS移库订单号
        /// </summary>
        public string MESMB { get; set; }
        /// <summary>
        /// WMS移库流程类型 
        /// </summary>
        public string MESBS { get; set; }
        /// <summary>
        /// 凭证中的凭证日期
        /// 可选，格式YYYYMMDD
        /// </summary>
        public string BLDAT { get; set; }
        /// <summary>
        /// 凭证抬头文本
        /// </summary>
        public string BKTXT { get; set; }

        /// <summary>
        /// 客户编号1
        /// </summary>
        public string KUNNR { get; set; }

        /// <summary>
        /// 供应商或债权人的帐号
        /// </summary>
        public string LIFNR { get; set; }

        /// <summary>
        /// 参考凭证编号
        /// </summary>
        public string XBLNR { get; set; }


        public static string SerializeObject(ZwmsGoodsMoveNewHeader detail)
        {
            return JsonConvert.SerializeObject(detail);
        }
        public static ZwmsGoodsMoveNewHeader DeserializeObject(string value)
        {
            return JsonConvert.DeserializeObject<ZwmsGoodsMoveNewHeader>(value);
        }

        public static string SerializeObjectT(List<ZwmsGoodsMoveNewHeader> ListDetail)
        {
            return JsonConvert.SerializeObject(ListDetail);
        }
        public static List<ZwmsGoodsMoveNewHeader> DeserializeObjectT(string value)
        {
            return JsonConvert.DeserializeObject<List<ZwmsGoodsMoveNewHeader>>(value);
        }
    }
}
