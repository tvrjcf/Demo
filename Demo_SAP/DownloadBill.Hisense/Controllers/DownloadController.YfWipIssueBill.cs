using BD.Hisense;
using DownloadBill.Hisense.Download;
using Platform;
using System;
using System.Linq;
using static WMS.Hisense.Download.RFC_YfWipIssueBill;
using static WMS.Hisense.Download.YfBillDowmloadHelp;

namespace DownloadBill.Hisense.Controllers
{
    public partial class DownloadController
    {
        /// <summary>
        /// 下载研发预留单
        /// </summary>
        /// <param name="OrgCode"></param>
        /// <param name="billNo"></param>
        /// <returns></returns>
        public virtual Result DownloadYfBill(string OrgCode, string factory, string billNo)
        {
            Result rs = new Result();
            try
            {
                var list = DownloadYfWipIssueBill(OrgCode, factory, billNo);
                if (billNo.IsNotEmpty() && list.Count() <= 0)
                {
                    rs.Success = false;
                    rs.Message = "找不到研发预留单：" + billNo;
                    return rs;
                }
                foreach (var bill in list)
                {
                    try
                    {
                        SaveOrUpdateYfbill(bill);
                    }
                    catch (Exception ex)
                    {
                        LogController.Error("研发预留单下载异常: ", "预留单号：[{0}]".FormatArgs(bill.No), ex.GetBaseException().Message);
                        if (billNo.IsNotEmpty())//单个下载 把异常抛出到界面
                        {
                            throw ex;
                        }
                    }
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
