using BD;
using BD.Suppliers;
using DownloadBill.Hisense.Download;
using Platform;
using Platform.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DownloadBill.Hisense.Controllers
{
    public partial class DownloadController
    {
        public virtual Result DownloadSupplier(string Code)
        {
            Result rs = new Result();
            try
            {
                var list = RFC_Supplier.GetErpSupplier(Code);
                if (Code.IsNotEmpty() && list.Count() <= 0)
                {
                    rs.Success = false;
                    rs.Message = "找不到供应商：" + Code;
                    return rs;
                }
                foreach (var c in list)
                {
                    SaveOrUpdateSupplier(c, Code);
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
        public virtual void SaveOrUpdateSupplier(Supplier supplier, string code)
        {
            try
            {
                //if (corporation.IsNullOrWhiteSpace()) throw new ArgumentNullException("corporation");
                //SetInvOrgIdByCorporation(corporation);
                using (var trans = RF.TransactionScope(BDEntityDataProvider.ConnectionStringName))
                {

                    ////获取客户 
                    var existSupplier = DomainControllerFactory.Create<SupplierController>()
                        .GetSupplierByCode(supplier.Code) ?? supplier;

                    //ItemExtendsion.SetErpUpdate(item, item.UpdateDate);
                    existSupplier.Name = supplier.Name;
                    existSupplier.Description = supplier.Description;
                    //supplier.PersistenceStatus = existSupplier.PersistenceStatus;                   

                    //InvOrgIdExtension.SetInvOrgId(supplier, PlatformEnvironment.InvOrgId);
                    RF.Save(existSupplier);

                    trans.Complete();

                }
            }
            catch (Exception e)
            {
                throw new Exception("供应商基础主数据下载异常:" + e.Message);
            }
        }
    }
}
