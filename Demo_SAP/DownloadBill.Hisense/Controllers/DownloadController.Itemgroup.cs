using DownloadBill.Hisense.Download;
using Platform;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DownloadBill.Hisense.Controllers
{
    public partial class DownloadController
    {
        public virtual Result DownloadItemgroup(string code)
        {
            Result rs = new Result();
            try
            {
                var list = RFC_ItemGroup.GetErpItem(code);
                if (code.IsNotEmpty() && list.Count() <= 0)
                {
                    rs.Success = false;
                    rs.Message = "找不到物料组：" + code;
                    return rs;
                }
                foreach (var org in list)
                {
                    ReceiveCategory(org, false);
                }
                rs.Success = true;
                return rs;
            }
            catch (Exception ex)
            {
                rs.Success = false;
                rs.Message = ex.Message;
                return rs;
            }
        }

    }
}
