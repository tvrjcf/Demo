using BD;
using BD.Customers;
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
        public virtual Result DownloadCustomer(string Code)
        {
            Result rs = new Result();
            try
            {
                var list = RFC_Customer.GetErpCustomer(Code);
                if (Code.IsNotEmpty() && list.Count() <= 0)
                {
                    rs.Success = false;
                    rs.Message = "找不到客户：" + Code;
                    return rs;
                }
                foreach (var c in list)
                {
                    SaveOrUpdateCustomer(c, Code);
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
        public virtual void SaveOrUpdateCustomer(Customer customer, string Code)
        {
            try
            {
                //if (corporation.IsNullOrWhiteSpace()) throw new ArgumentNullException("corporation");
                //SetInvOrgIdByCorporation(corporation);
                using (var trans = RF.TransactionScope(BDEntityDataProvider.ConnectionStringName))
                {

                    //获取客户 
                    var criteria = new CustomerCriteria()
                    {
                        PagingInfo = new PagingInfo(1, int.MaxValue)
                    };
                    criteria.Code = customer.Code;
                    var existCustomer = DomainControllerFactory.Create<CustomerController>()
                        .GetList(criteria).Concrete().FirstOrDefault() ?? customer;

                    //ItemExtendsion.SetErpUpdate(item, item.UpdateDate);
                    existCustomer.Name = customer.Name;
                    existCustomer.SalesArea = customer.SalesArea;

                    //customer.PersistenceStatus = existCustomer.PersistenceStatus;
                    //customer.Id = existCustomer.Id;
                    //organization.DataSource = EumDataSource.ERP;                    

                    InvOrgIdExtension.SetInvOrgId(customer, PlatformEnvironment.InvOrgId);
                    RF.Save(existCustomer);

                    trans.Complete();

                }
            }
            catch (Exception e)
            {
                throw new Exception("客户基础主数据下载异常:"+ e.Message);
            }
        }
    }
}
