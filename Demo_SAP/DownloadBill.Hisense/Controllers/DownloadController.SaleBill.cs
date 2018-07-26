using BD.Items;
using DownloadBill.Hisense.Download;
using INV.Hisense.Warehouses.Warehouses;
using Platform;
using Platform.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WMS.Hisense;
using WMS.Hisense.SaleBills;

namespace DownloadBill.Hisense.Controllers
{
    /// <summary>
    /// 下载销售交货单
    /// </summary>
    public partial class DownloadController
    {
        public virtual Result DownloadSaleBill(string OrgCode, string billNo)
        {
            Result rs = new Result();
            try
            {
                var list = RFC_SaleBill.GetSaleBill(OrgCode, billNo);
                if (billNo.IsNotEmpty() && list.Count() <= 0)
                {
                    rs.Success = false;
                    rs.Message = "找不到销售交货单：" + billNo;
                    return rs;
                }
                foreach (var bill in list)
                {
                    SaveOrUpdateSalebill(bill, billNo);
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
        public virtual void SaveOrUpdateSalebill(SaleBill bill, string billNo)
        {
            try
            {
                if (bill.No.IsNullOrWhiteSpace())
                {
                    //throw new ArgumentNullException("No");
                    return;
                }
                using (var trans = RF.TransactionScope(WMSEntityDataProvider.ConnectionStringName))
                {
                    var existBill = DomainControllerFactory.Create<SaleBillController>()
                        .GetSaleBillByNo(bill.No) ?? bill;
                    existBill.BillDate = bill.BillDate;
                    existBill.Location = bill.Location;
                    existBill.Org = bill.Org;
                    //bill.Location = h["LFART"].ToString();
                    existBill.PlanDate = bill.PlanDate;
                    existBill.DeliveryDate = bill.DeliveryDate;
                    //bill.State = h["WBSTK"].ToString();
                    //todo 移动类型                                    

                    //InvOrgIdExtension.SetInvOrgId(bill, PlatformEnvironment.InvOrgId);

                    foreach (var detail in bill.SaleBillDetailList)
                    {
                        //if (bill.SaleBillDetailList.Concrete().Any(p => p.WipIssueState != WipIssueState.New)) continue;
                        var existDtl = existBill.SaleBillDetailList.Concrete().FirstOrDefault(p => p.ProjectNo == detail.ProjectNo) ?? detail;

                        //更新物料编码                        
                        var item = DomainControllerFactory.Create<ItemController>().GetByCode(detail.Item.Code);
                        if (item == null)
                        {
                            if (billNo.IsNotEmpty())
                                throw new EntityNotFoundException("不存在物料[{0}]，请先下载最新物料".Translate().FormatArgs(detail.Item.Code));
                            return;
                        }
                        var warehouse = DomainControllerFactory.Create<WarehouseController>().GetWarehouse(detail.Warehouse);
                        //if (warehouse == null)
                        //{
                        //    if (billNo.IsNotEmpty())
                        //        throw new EntityNotFoundException("不存在仓库[{0}]，请先下载最新仓库".Translate().FormatArgs(detail.Warehouse));
                        //    return;
                        //}
                        existDtl.Item = item;
                        existDtl.Factory = detail.Factory;
                        existDtl.Warehouse = detail.Warehouse;
                        existDtl.IssueWarehouseId = warehouse?.Id;
                        existDtl.Qty = detail.Qty;
                        existDtl.Unit = detail.Unit;
                        existDtl.BillDtlDate = detail.BillDtlDate;
                        existDtl.ActualQty = detail.ActualQty;
                        existDtl.ProductLevel = detail.ProductLevel;
                        existDtl.SaleBillId = existBill.Id;
                        RF.Save(existDtl);
                    }
                    RF.Save(existBill);
                    // 排除已经下载 SAP更新后删除的明细 todo 
                    trans.Complete();
                }
            }
            catch (Exception e)
            {
                throw new Exception("交货单下载异常:" + e.Message);
            }
        }

    }
}
