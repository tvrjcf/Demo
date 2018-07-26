using BD;
using BD.Organizations;
using DownloadBill.Hisense.Download;
using Platform;
using Platform.Domain;
using Platform.Domain.InvOrg;
using Platform.Domain.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DownloadBill.Hisense.Controllers
{
    public partial class DownloadController
    {
        public virtual Result DownloadOrg(string orgCode)
        {
            Result rs = new Result();
            try
            {
                var list = RFC_Org.GetOrgByCode(orgCode);
                if (orgCode.IsNotEmpty() && list.Count() <= 0)
                {
                    rs.Success = false;
                    rs.Message = "找不到工厂：" + orgCode;
                    return rs;
                }
                foreach (var org in list)
                {
                    SaveOrUpdateOrg(org, orgCode);
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
        public virtual void SaveOrUpdateOrg(Organization org, string orgCode)
        {
            try
            {
                //if (corporation.IsNullOrWhiteSpace()) throw new ArgumentNullException("invOrg");
                //SetInvOrgIdByCorporation(invOrg);
                using (var trans = RF.TransactionScope(BDEntityDataProvider.ConnectionStringName))
                {
                    var ctl = DomainControllerFactory.Create<OrganizationController>();
                    var corp = ctl.GetOrganizationByType(OrganizationType.Group, null, null).Concrete().FirstOrDefault();
                    if (corp == null)
                    {
                        throw new ValidationException("缺少公司(集团)信息".Translate());
                    }
                    Organization existOrg = null;
                    if (org.LevelId == 1)
                    {
                        existOrg = ctl.GetOrganizationByType(OrganizationType.Group, null, org.Code).Concrete().FirstOrDefault() ?? org;
                    }
                    else
                    {
                        //获取工厂
                        existOrg = ctl.GetOrganizationByType(OrganizationType.Plant, null, org.Code).Concrete().FirstOrDefault() ?? org;
                        existOrg.TreeParent = corp;
                    }
                    existOrg.Name = org.Name;
                    InvOrgIdExtension.SetInvOrgId(existOrg, PlatformEnvironment.InvOrgId);
                    RF.Save(existOrg);
                    trans.Complete();
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }
    }


}
