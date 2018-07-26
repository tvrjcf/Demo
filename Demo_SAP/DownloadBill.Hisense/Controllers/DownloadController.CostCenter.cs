using BD;
using BD.Factories;
using DownloadBill.Hisense.Download;
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
        public virtual Result DownloadCostCenter(string code)
        {
            Result rs = new Result();
            try
            {
                var list = RFC_CostCenter.GetErpCostCenter(code);
                if (code.IsNotEmpty() && list.Count() <= 0)
                {
                    rs.Success = false;
                    rs.Message = "找不到成本中心：" + code;
                    return rs;
                }
                foreach (var c in list)
                {
                    SaveOrUpdateCost(c, code);
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
        public virtual void SaveOrUpdateCost(Factory factory, string code)
        {
            try
            {
                // SetInvOrgIdByCorporation(inv_org);
                using (var trans = RF.TransactionScope(BDEntityDataProvider.ConnectionStringName))
                {

                    var existFactory = (DomainControllerFactory.Create<FactoryController>().GetList(new FactoryCriteria() { Code = factory.Code }).Concrete()).FirstOrDefault() ?? factory;

                    factory = existFactory;

                    InvOrgIdExtension.SetInvOrgId(factory, PlatformEnvironment.InvOrgId);
                    RF.Save(factory);

                    trans.Complete();

                }
            }
            catch (Exception e)
            {
                throw new Exception("成本中心数据下载异常:" + e.Message);
            }
        }

    }
}
