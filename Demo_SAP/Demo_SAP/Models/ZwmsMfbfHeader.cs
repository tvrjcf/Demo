using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UpLoadJob.Models
{
    public class ZwmsMfbfHeader
    {
        public ZwmsMfbfHeader() {
            MESMB = "";
            MESBS = "";
            BLDAT = DateTime.Now.ToString("yyyyMMdd");
            I_MATNR = "";
            I_MENGE = "";
            I_LGORT = "";
            I_VERID = "";
            I_WERKS = "";
        }
        /// <summary>
        /// WMS移库订单号	MESMB	CHAR	10		必输			
        /// </summary>
        public string MESMB { get; set; }
        /// <summary>
        /// WMS移库流程类型 	MESBS	CHAR	20					
        /// </summary>
        public string MESBS { get; set; }
        /// <summary>
        /// 凭证中的凭证日期	BLDAT	CHAR	8		可选，格式YYYYMMDD 默认当前日期			
        /// </summary>
        public string BLDAT { get; set; }
        /// <summary>
        /// 物料号 	I_MATNR	CHAR	18		131 物料			
        /// </summary>
        public string I_MATNR { get; set; }
        /// <summary>
        /// 数量	I_MENGE	CHAR	13	3	131 数量			
        /// </summary>
        public string I_MENGE { get; set; }
        /// <summary>
        /// 库存地点	I_LGORT	CHAR	4		必填			
        /// </summary>
        public string I_LGORT { get; set; }
        /// <summary>
        /// 生产版本	I_VERID	CHAR	4		可选,默认0001			
        /// </summary>
        public string I_VERID { get; set; }
        /// <summary>
        /// 工厂	I_WERKS	CHAR	4		必填			
        /// </summary>
        public string I_WERKS { get; set; }

        public static string SerializeObject(ZwmsMfbfHeader detail)
        {
            return JsonConvert.SerializeObject(detail);
        }
        public static ZwmsMfbfHeader DeserializeObject(string value)
        {
            return JsonConvert.DeserializeObject<ZwmsMfbfHeader>(value);
        }
        public static string SerializeObjectT(List<ZwmsMfbfHeader> ListDetail)
        {
            return JsonConvert.SerializeObject(ListDetail);
        }
        public static List<ZwmsMfbfHeader> DeserializeObjectT(string value)
        {
            return JsonConvert.DeserializeObject<List<ZwmsMfbfHeader>>(value);
        }
    }
}
