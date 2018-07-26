using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UpLoadJob.Models
{
    public class ZwmsDnConfDetail
    {

        public ZwmsDnConfDetail()
        {
            ZWMSNO = "";
            VSTEL = "";
            DATBI = new DateTime();
            VGBEL = "";
            VGPOS = "";
            MATNR = "";
            LGORT = "";
            LFIMG = "";
            KUOZHAN1 = "";
            KUOZHAN2 = "";
            KUOZHAN3 = "";
            KUOZHAN4 = "";
        }

        /// <summary>
        /// WMS单号
        /// </summary>
        public string ZWMSNO { get; set; }
        /// <summary>
        /// 装运点/接收点 
        /// </summary>
        public string VSTEL { get; set; }
        /// <summary>
        /// 有效截至日期
        /// </summary>
        public DateTime DATBI { get; set; }
        /// <summary>
        /// 参考单据的单据编号
        /// </summary>
        public string VGBEL { get; set; }
        /// <summary>
        /// 参考项目的项目号  
        /// </summary>
        public string VGPOS { get; set; }
        /// <summary>
        /// 物料号
        /// </summary>
        public string MATNR { get; set; }
        /// <summary>
        /// 库存地点
        /// </summary>
        public string LGORT { get; set; }
        /// <summary>
        /// 实际已交货量（按销售单位）
        /// </summary>
        public string LFIMG { get; set; }
        /// <summary>
        /// 扩展字段1
        /// </summary>
        public string KUOZHAN1 { get; set; }
        /// <summary>
        /// 扩展字段2
        /// </summary>
        public string KUOZHAN2 { get; set; }
        /// <summary>
        /// 扩展字段3
        /// </summary>
        public string KUOZHAN3{ get; set; }
        /// <summary>
        /// 扩展字段4
        /// </summary>
        public string KUOZHAN4 { get; set; }


        public static string SerializeObject(ZwmsDnConfDetail detail)
        {
            return JsonConvert.SerializeObject(detail);
        }
        public static ZwmsDnConfDetail DeserializeObject(string value)
        {
            return JsonConvert.DeserializeObject<ZwmsDnConfDetail>(value);
        }
        public static string SerializeObjectT(List<ZwmsDnConfDetail> listDetail)
        {
            return JsonConvert.SerializeObject(listDetail);
        }
        public static List<ZwmsDnConfDetail> DeserializeObjectT(string value)
        {
            return JsonConvert.DeserializeObject<List<ZwmsDnConfDetail>>(value);
        }

    }
}
