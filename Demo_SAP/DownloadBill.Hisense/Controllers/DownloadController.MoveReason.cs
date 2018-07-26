using BD;
using DownloadBill.Hisense.Download;
using INV.Hisense.Transactions;
using Platform;
using Platform.Domain;
using Platform.Domain.InvOrg;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DownloadBill.Hisense.Controllers
{
    public partial class DownloadController
    {
        public virtual Result DownloadReason(string code)
        {
            Result rs = new Result();
            try
            {
                var list = RFC_MoveReason.GetErpMoveReason(code);
                if (code.IsNotEmpty() && list.Count() <= 0)
                {
                    rs.Success = false;
                    rs.Message = "找不到移动原因：" + code;
                    return rs;
                }
                foreach (var org in list)
                {
                    ReceiveMoveReason(org, code);
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
        public virtual void ReceiveMoveReason(Reasons reason, string code)
        {
            try
            {
                if (reason == null) throw new ArgumentNullException("reason");
                using (var trans = RF.TransactionScope(BDEntityDataProvider.ConnectionStringName))
                {
                    Reasons model = DomainControllerFactory.Create<ReasonsController>().GetReason(reason.Name, reason.MoveType) as Reasons;
                    var existReason = model ?? reason;
                    //existReason.Name = reason.Name;
                    existReason.Description = reason.Description;
                    existReason.ReasonsType = ReasonsType.MOVER;
                    //
                    InvOrgIdExtension.SetInvOrgId(existReason, PlatformEnvironment.InvOrgId);
                    RF.Save(existReason);

                    trans.Complete();

                }
            }
            catch (Exception e)
            {
                throw new Exception("移动原因数据下载异常:" + e);
            }
        }
    }
}
