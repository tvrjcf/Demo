using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UpLoadJob.Models
{
    public class ZwmsDnConfHeader
    {
        public ZwmsDnConfHeader() {

        }

        public static string SerializeObject(ZwmsDnConfHeader detail)
        {
            return JsonConvert.SerializeObject(detail);
        }
        public static ZwmsDnConfHeader DeserializeObject(string value)
        {
            return JsonConvert.DeserializeObject<ZwmsDnConfHeader>(value);
        }
        public static string SerializeObjectT(List<ZwmsDnConfHeader> ListDetail)
        {
            return JsonConvert.SerializeObject(ListDetail);
        }
        public static List<ZwmsDnConfHeader> DeserializeObjectT(string value)
        {
            return JsonConvert.DeserializeObject<List<ZwmsDnConfHeader>>(value);
        }
    }
}
