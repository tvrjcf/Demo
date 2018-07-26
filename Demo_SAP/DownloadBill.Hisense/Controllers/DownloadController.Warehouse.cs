using BD;
using BD.Organizations;
using DownloadBill.Hisense.Download;
using INV.Hisense.Warehouses.Warehouses;
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
        public virtual Result DownloadWarehouse(string code)
        {
            Result rs = new Result();
            try
            {
                var list = RFC_Warehouse.DownloadWarehouse(code);
                if (code.IsNotEmpty() && list.Count() <= 0)
                {
                    rs.Success = false;
                    rs.Message = "找不到仓库：" + code;
                    return rs;
                }
                foreach (var wh in list)
                {
                    SaveOrUpdateWarehouse(wh, code);
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
        public virtual void SaveOrUpdateWarehouse(Warehouse warehouse, string code)
        {
            try
            {
                //if (corporation.IsNullOrWhiteSpace()) throw new ArgumentNullException("invOrg");
                //SetInvOrgIdByCorporation(corporation);
                using (var trans = RF.TransactionScope(BDEntityDataProvider.ConnectionStringName))
                {
                    var factory = DomainControllerFactory.Create<OrganizationController>().GetOrganizationList(new OrganizationCriteria { Code = warehouse.Factory.Code }).FirstOrDefault() as Organization;
                    if (factory == null)
                        throw new Exception("不存在工厂:" + warehouse.Factory.Code);
                    ////获取仓库 
                    var existWarehouse = DomainControllerFactory.Create<WarehouseController>()
                        .GetWarehouseList(new WarehouseCriteria { Code = warehouse.Code }).Concrete().FirstOrDefault() ?? warehouse;

                    existWarehouse.Name = warehouse.Name;
                    existWarehouse.FactoryId = factory.Id;
                    //warehouse.PersistenceStatus = existWarehouse.PersistenceStatus;                    
                    //organization.DataSource = EumDataSource.ERP;

                    InvOrgIdExtension.SetInvOrgId(existWarehouse, PlatformEnvironment.InvOrgId);
                    RF.Save(existWarehouse);
                    trans.Complete();
                }
            }
            catch (Exception e)
            {
                throw new Exception("仓库基础主数据下载异常:" + e.Message);
            }
        }


    }
}
