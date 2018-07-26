using BD;
using BD.Purchases;
using DownloadBill.Hisense.Download;
using Platform;
using Platform.Domain;
using Platform.Domain.Common.Security;
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
        public virtual Result DownloadPurgroup(string code)
        {
            Result rs = new Result();
            try
            {
                var list = RFC_PurchaseGroup.GetPurchases(code);
                if (code.IsNotEmpty() && list.Count() <= 0)
                {
                    rs.Success = false;
                    rs.Message = "找不到采购组：" + code;
                    return rs;
                }
                foreach (var c in list)
                {
                    ReceivePurchase(c, code);
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
        public virtual void ReceivePurchase(PurchaseGroup purchase, string code)
        {
            try
            {
                using (var trans = RF.TransactionScope(BDEntityDataProvider.ConnectionStringName))
                {
                    var existOrganization = DomainControllerFactory.Create<PurchaseGroupController>().GetPurchaseGroupByCode(purchase.Code) ?? purchase;
                    var userModel = DomainControllerFactory.Create<UserController>().FetchBy("", purchase.Name).FirstOrDefault() as User;
                    if (userModel != null)
                    {
                        existOrganization.BuyerId = userModel.Id;
                    }
                    existOrganization.Code = purchase.Code;
                    existOrganization.Name = purchase.Code;
                    existOrganization.Phone = purchase.Phone;
                    existOrganization.Fax = purchase.Fax;

                    //
                    InvOrgIdExtension.SetInvOrgId(existOrganization, PlatformEnvironment.InvOrgId);
                    RF.Save(existOrganization);

                    trans.Complete();

                }
            }
            catch (Exception e)
            {
                throw new Exception("采购组基础主数据 下载异常:" + e.Message);
            }
        }
    }
}
