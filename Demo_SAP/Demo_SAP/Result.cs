using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Demo_SAP
{
    public class Result
    {
        public object Data { get; set; }
        public string Message { get; set; }
        public int StatusCode { get; set; }
        public bool Success { get; set; }
    }
}
