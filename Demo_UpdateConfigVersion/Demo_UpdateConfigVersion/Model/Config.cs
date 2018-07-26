using System;
using System.Collections.Generic;
using System.Text;

namespace Demo_UpdateConfigVersion.Model
{
    public class Config
    {
        public List<Apk> apks { get; set; }
    }

    public class Apk {
        public string name { get; set; }
        public string type { get; set; }
        public string mainActivity { get; set; }
        public string updateTime { get; set; }
        public string version { get; set; }
    
    }
}
