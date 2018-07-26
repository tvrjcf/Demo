using BD.Items;
using BD.Suppliers;
using DownloadBill.Hisense.Download;
using INV.Hisense.Warehouses.Warehouses;
using Platform;
using Platform.Domain;
using PT.Hisense.PurchaseOrders;
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
        /// 下载PO采购单
        /// </summary>
        /// <param name="workOrderNo"></param>
        /// <returns></returns>
        public virtual Result DownloadPo(string OrgCode, string PoNo)
        {
            Result rs = new Result();
            try
            {
                var list = RFC_PurchaseOrder.GetErpPo(OrgCode, PoNo);
                if (PoNo.IsNotEmpty() && list.Count() <= 0)
                {
                    rs.Success = false;
                    rs.Message = "找不到采购单：" + PoNo;
                    return rs;
                }
                foreach (var po in list)
                {
                    var batch_no = Guid.NewGuid().ToSafeString();
                    SaveOrUpdatePo(po, PoNo, batch_no);
                    //var ctl = DomainControllerFactory.Create<PurchaseOrderController>();
                    ////查找PO
                    //var purchase = ctl.GetPurchaseOrder(PoNo);
                    //var del_ls = purchase.PurchaseOrderDetailList.Concrete().Where(p => p.DownBatchNo != batch_no && p.State == PoState.New);
                    //foreach (var item in del_ls)
                    //{
                    //    item.PersistenceStatus = PersistenceStatus.Deleted;
                    //    RF.Save(item);
                    //}
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

        /// <summary>
        /// 接收ERP采购订单
        /// </summary>
        /// <param name="po">采购单</param>        
        /// <param name="invOrg">公司代码</param>
        public virtual void SaveOrUpdatePo(PurchaseOrder po, string PoNo, string batch_no)
        {
            try
            {
                var supplier = DomainControllerFactory.Create<SupplierController>().GetSupplierByCode(po.Supplier.Code);
                if (supplier == null)
                {
                    if (PoNo.IsNotEmpty())
                        throw new ArgumentNullException("供应商信息缺失".Translate());
                    return;
                }
                using (var trans = RF.TransactionScope(WMSEntityDataProvider.ConnectionStringName))
                {
                    var existPo = (DomainControllerFactory.Create<PurchaseOrderController>()
                        .GetNewStatusPoList(new PurchaseCriteria { PoNumber = po.PoNumber }).FirstOrDefault() ?? po) as PurchaseOrder;
                    existPo.Supplier = supplier;
                    existPo.CountryCode = po.CountryCode;
                    existPo.CountryName = po.CountryName;
                    existPo.City = po.City;
                    existPo.Area = po.Area;
                    existPo.Street = po.Street;

                    //InvOrgIdExtension.SetInvOrgId(po, PlatformEnvironment.InvOrgId);


                    foreach (var detail in po.PurchaseOrderDetailList)
                    {
                        //待优化
                        //if (po.PurchaseOrderDetailList.Concrete().Any(p => p.State != PoState.New)) continue;
                        var existDtl = existPo.PurchaseOrderDetailList.Concrete().FirstOrDefault(p => p.ProjectNo == detail.ProjectNo) ?? detail;

                        //更新物料编码
                        var item = DomainControllerFactory.Create<ItemController>().GetByCode(detail.Item.Code);
                        if (item == null)
                        {
                            if (PoNo.IsNotEmpty())
                                throw new EntityNotFoundException("不存在物料[{0}]，请先下载最新物料".Translate().FormatArgs(detail.Item.Code));
                            return;
                        }
                        //var warehouse = DomainControllerFactory.Create<WarehouseController>().GetWarehouse(detail.Warehouse);
                        //if (warehouse == null)
                        //{
                        //    if (PoNo.IsNotEmpty())
                        //        throw new EntityNotFoundException("不存在仓库[{0}]，请先下载最新仓库".Translate().FormatArgs(detail.Warehouse));
                        //    return;
                        //}
                        existDtl.IqcFlag = detail.IqcFlag;
                        existDtl.Item = item;
                        existDtl.Unit = detail.Unit;
                        existDtl.Factory = detail.Factory;
                        existDtl.Warehouse = detail.Warehouse;
                        existDtl.Quantity = detail.Quantity;
                        existDtl.PurchaseType = detail.PurchaseType;
                        existDtl.IsReturn = detail.IsReturn;
                        existDtl.PurchaseOrderId = existPo.Id;
                        existDtl.DownBatchNo = batch_no;
                        existDtl.ErpReceivedQty = detail.ErpReceivedQty;
                        if (detail.State == PoState.Received)
                        {
                            existDtl.State = PoState.Received;
                        }
                        RF.Save(existDtl);
                    }
                    RF.Save(existPo);
                    // 排除已经下载 SAP更新后删除的明细 todo 
                    trans.Complete();
                }
            }
            catch (Exception e)
            {
                throw new Exception("采购订单下载异常:" + e.Message);
            }
        }
    }
}
