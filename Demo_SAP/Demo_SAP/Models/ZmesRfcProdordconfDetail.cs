using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UpLoadJob.Models
{
    public class ZmesRfcProdordconfDetail
    {

        public ZmesRfcProdordconfDetail() {
            RTYPE = "";
            RTMSG = "";
            MATNR = "";
            MAKTX = "";
            XUQIU = 0;
            LABST = 0;
            DUANQ = 0;
            MEINS = "";

        }

        /// <summary>
        /// 消息类型: S 成功, E 错误,W 警告, I 信息,A 中断
        /// </summary>
        public string RTYPE { get; set; }
        /// <summary>
        /// 消息文本
        /// </summary>
        public string RTMSG { get; set; }
        /// <summary>
        /// 物料号 
        /// </summary>
        public string MATNR { get; set; }
        /// <summary>
        /// 物料描述（短文本） 
        /// </summary>
        public string MAKTX { get; set; }
        /// <summary>
        /// 需求数量 
        /// </summary>
        public double XUQIU { get; set; }
        /// <summary>
        /// 非限制使用的估价的库存
        /// </summary>
        public double LABST { get; set; }
        /// <summary>
        /// 短缺数量
        /// </summary>
        public double DUANQ { get; set; }
        /// <summary>
        /// 基本计量单位
        /// </summary>
        public string MEINS { get; set; }


        public static string SerializeObject(ZmesRfcProdordconfDetail detail)
        {
            return JsonConvert.SerializeObject(detail);
        }
        public static ZmesRfcProdordconfDetail DeserializeObject(string value)
        {
            return JsonConvert.DeserializeObject<ZmesRfcProdordconfDetail>(value);
        }
        public static string SerializeObjectT(ZmesRfcProdordconfDetail ListDetail)
        {
            return JsonConvert.SerializeObject(ListDetail);
        }
        public static List<ZmesRfcProdordconfDetail> DeserializeObjectT(string value)
        {
            return JsonConvert.DeserializeObject<List<ZmesRfcProdordconfDetail>>(value);
        }

    }
}
