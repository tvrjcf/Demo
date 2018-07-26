using BD.Items;
using DownloadBill.Hisense.Download;
using INV.Hisense.Warehouses.Warehouses;
using Platform;
using Platform.Domain;
using SFC.Hisense.WorkOrders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WMS.Hisense;

namespace DownloadBill.Hisense.Controllers
{
    /// <summary>
    /// 下载工单BOM
    /// </summary>
    public partial class DownloadController
    {
        /// <summary>
        /// 下载工单Bom
        /// </summary>
        public virtual Result DownloadMoBom(string workOrderNo)
        {
            Result rs = new Result();
            try
            {
                var list = Download.RFC_MoBom.GetErpMoBom(workOrderNo);
                if (workOrderNo.IsNotEmpty() && list.Count() <= 0)
                {
                    rs.Success = false;
                    rs.Message = "找不到工单：" + workOrderNo;
                    return rs;
                }
                var batch_no = Guid.NewGuid().ToSafeString();
                foreach (var bom in list)
                {
                    SaveOrUpdateBom(bom, batch_no);
                }
                var ctl = DomainControllerFactory.Create<WorkOrderController>();
                //查找工单
                var workOder = ctl.GetWorkOrderByWo(workOrderNo);
                var del_ls = workOder.WorkOrderBOMList.Concrete().Where(p => p.BatchNo != batch_no);
                foreach (var item in del_ls)
                {
                    item.PersistenceStatus = PersistenceStatus.Deleted;
                    RF.Save(item);
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
        public virtual void SaveOrUpdateBom(WorkOrderBom woBom, string batch_no)
        {
            try
            {
                using (var trans = RF.TransactionScope(WMSEntityDataProvider.ConnectionStringName))
                {
                    var ctl = DomainControllerFactory.Create<WorkOrderController>();
                    //查找工单
                    var workOder = ctl.GetWorkOrderByWo(woBom.WorkOrder.WorkOrderNo);
                    if (workOder == null)
                    {
                        return;
                    }
                    var item = DomainControllerFactory.Create<ItemController>().GetByCode(woBom.Item.Code);
                    if (item == null)
                    {
                        return;
                    }
                    var wareHouse = DomainControllerFactory.Create<WarehouseController>().GetWarehouse(woBom.SourceWarehouse.Code);
                    //获取工单Bom                    
                    var existBom = ctl.FindWoBom(workOder.Id, item.Id).Concrete().FirstOrDefault() ?? woBom;

                    existBom.WorkOrderId = workOder.Id;
                    existBom.ItemId = item.Id;
                    existBom.RequireQty = woBom.RequireQty;
                    existBom.SingleQty = woBom.SingleQty;
                    existBom.Unit = woBom.Unit;
                    existBom.SourceWarehouseId = wareHouse == null ? 0 : wareHouse.Id;
                    existBom.IsRecoilItem = woBom.IsRecoilItem;
                    existBom.BatchNo = batch_no;
                    RF.Save(existBom);
                    trans.Complete();
                }
            }
            catch (Exception e)
            {
                throw new Exception("工单Bom数据下载异常:" + e.Message);
            }
        }

    }
}
