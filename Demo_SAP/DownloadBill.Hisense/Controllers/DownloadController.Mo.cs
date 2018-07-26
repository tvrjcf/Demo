using BD.Items;
using BD.Organizations;
using DownloadBill.Hisense.Download;
using INV.Hisense.Warehouses.Warehouses;
using Platform;
using Platform.Domain;
using Platform.Domain.Validation;
using SFC.Hisense.WorkOrders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WMS.Hisense;

namespace DownloadBill.Hisense.Controllers
{
    public partial class DownloadController
    {
        /// <summary>
        /// 下载Mo工单
        /// </summary>
        public virtual Result DownloadMo(string workOrderNo)
        {
            Result rs = new Result();
            try
            {
                var list = Download.RFC_Mo.GetMoByNo(workOrderNo);
                if (workOrderNo.IsNotEmpty() && list.Count() <= 0)
                {
                    rs.Success = false;
                    rs.Message = "找不到工单：" + workOrderNo;
                    return rs;
                }
                foreach (var mo in list)
                {
                    SaveOrUpdateMo(mo, workOrderNo);
                    var rst = DownloadMoBom(mo.WorkOrderNo);
                    //单个下载则提示错误
                    if (workOrderNo.IsNotEmpty() && !rst.Success)
                    {
                        rs.Success = false;
                        rs.Message = rst.Message;
                        return rs;
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
        public virtual void SaveOrUpdateMo(WorkOrder workOrder, string workOrderNo)
        {
            try
            {
                using (var trans = RF.TransactionScope(WMSEntityDataProvider.ConnectionStringName))
                {
                    var existWorkOrder = DomainControllerFactory.Create<WorkOrderController>()
                        .GetWorkOrderByWo(workOrder.WorkOrderNo) ?? workOrder;
                    OrganizationList list = new OrganizationList();
                    if (workOrder.Workshop.Code.IsNotEmpty())
                        list = DomainControllerFactory.Create<OrganizationController>().GetOrganizationList(new OrganizationCriteria() { Code = workOrder.Workshop.Code }) as OrganizationList;
                    Organization org = list.Concrete().FirstOrDefault() as Organization ?? workOrder.Workshop;
                    //if (org == null)
                    //{
                    //    throw new ValidationException("找不到车间[{0}]".Translate().FormatArgs(workOrder.Workshop.Code));
                    //}
                    Item item = DomainControllerFactory.Create<ItemController>().GetByCode(workOrder.Item.Code);
                    if (item == null)
                    {
                        //单个下载就报错
                        if (workOrderNo.IsNotEmpty())
                        {
                            throw new ValidationException("找不到物料[{0}]".Translate().FormatArgs(workOrder.Item.Code));
                        }
                        else { return; }
                    }
                    Warehouse house = DomainControllerFactory.Create<WarehouseController>().GetWarehouse(workOrder.ReceiptWarehouse.Code);
                    if (house == null)
                    {
                        existWorkOrder.ReceiptWarehouseId = null;
                        ////单个下载就报错
                        //if (workOrderNo.IsNotEmpty())
                        //{
                        //    throw new ValidationException("找不到收货库存地点[{0}]".Translate().FormatArgs(workOrder.ReceiptWarehouse.Code));
                        //}
                        //else { return; }
                    }
                    else
                    {
                        existWorkOrder.ReceiptWarehouseId = house.Id;
                    }
                    existWorkOrder.Factory = workOrder.Factory;
                    existWorkOrder.Item = item;
                    existWorkOrder.WorkshopId = org?.Id;
                    existWorkOrder.WorkOrderNo = workOrder.WorkOrderNo;
                    existWorkOrder.OrderQty = workOrder.OrderQty;
                    existWorkOrder.PlanBeginDate = workOrder.PlanBeginDate;
                    existWorkOrder.ActuFinishDate = workOrder.ActuFinishDate;
                    existWorkOrder.S_Type = workOrder.S_Type;
                    existWorkOrder.ProductVersion = workOrder.ProductVersion;
                    existWorkOrder.CreateDate = workOrder.CreateDate;
                    existWorkOrder.IsClosed = workOrder.IsClosed;
                    existWorkOrder.Remark = workOrder.Remark;
                    existWorkOrder.Status = workOrder.Status;
                    RF.Save(existWorkOrder);
                    trans.Complete();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
