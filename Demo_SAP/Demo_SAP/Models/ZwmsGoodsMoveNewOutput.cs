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
    public class ZwmsGoodsMoveNewOutput
    {

        public ZwmsGoodsMoveNewOutput() { }

        /// <summary>
        /// 处理标识
        /// S成功 E错误
        /// </summary>
        public string RTYPE { get; set; }
        /// <summary>
        /// 处理消息 
        /// 消息
        /// </summary>
        public string RTMSG { get; set; }
    }
}
