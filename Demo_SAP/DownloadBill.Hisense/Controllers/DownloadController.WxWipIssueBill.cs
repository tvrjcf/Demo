using BD.Items;
using BD.Suppliers;
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
using WMS.Hisense.WipIssueBills;

namespace DownloadBill.Hisense.Controllers
{
    public partial class DownloadController
    {
        /// <summary>
        /// 下载外协发料单
        /// </summary>
        public virtual Result DownloadWx(string OrgCode, string billNo)
        {
            Result rs = new Result();
            try
            {
                var list = RFC_WxWipIssueBill.GetWxWipIssueBill(OrgCode, billNo);
                if (billNo.IsNotEmpty() && list.Count() <= 0)
                {
                    rs.Success = false;
                    rs.Message = "找不到外协发料单：" + billNo;
                    return rs;
                }
                foreach (var mo in list)
                {
                    SaveOrUpdateWxpo(mo, billNo);
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

        public virtual void SaveOrUpdateWxpo(WxWipIssueBill bill, string billNo)
        {
            try
            {
                if (bill.No.IsNullOrWhiteSpace())
                {
                    //    throw new ArgumentNullException("No");
                    return;
                }
                using (var trans = RF.TransactionScope(WMSEntityDataProvider.ConnectionStringName))
                {
                    var existBill = DomainControllerFactory.Create<WxWipIssueBillController>()
                        .GetByNo(bill.No) ?? bill;
                    var supplier = DomainControllerFactory.Create<SupplierController>().GetSupplierByCode(bill.Supplier.Code);
                    if (supplier == null)
                    {
                        throw new EntityNotFoundException("不存在供应商[{0}]，请先下载最新供应商".Translate().FormatArgs(bill.Supplier.Code));
                    }
                    existBill.Supplier = supplier;
                    existBill.BillDate = bill.BillDate;
                    existBill.WipIssueBillType = WipIssueBillType.ISSUEOEM;
                    //todo 移动类型                                    

                    //InvOrgIdExtension.SetInvOrgId(bill, PlatformEnvironment.InvOrgId);

                    foreach (var detail in bill.WxWipIssueBillDetailList)
                    {
                        if (bill.WxWipIssueBillDetailList.Concrete().Any(p => p.WipIssueState != WipIssueState.New)) continue;
                        var existDtl = existBill.WxWipIssueBillDetailList.Concrete().FirstOrDefault(p => p.Item.Code == detail.Item.Code) ?? detail;

                        //更新物料编码                        
                        var item = DomainControllerFactory.Create<ItemController>().GetByCode(detail.Item.Code);
                        if (item == null)
                        {
                            if (billNo.IsNullOrEmpty())
                                throw new EntityNotFoundException("不存在物料[{0}]，请先下载最新物料".Translate().FormatArgs(detail.Item.Code));
                            return;
                        }
                        var warehouse = DomainControllerFactory.Create<WarehouseController>().GetWarehouse(detail.IssueWarehouse.Code);
                        //if (warehouse == null)
                        //{
                        //    throw new EntityNotFoundException("不存在仓库[{0}]，请先下载最新仓库".Translate().FormatArgs(detail.PickWarehouse.Code));
                        //}
                        existDtl.Item = item;
                        existDtl.Factory = detail.Factory;
                        existDtl.IssueWarehouse = warehouse;
                        //existDtl.PickWarehouse = warehouse;
                        existDtl.Qty = detail.Qty;
                        existDtl.Unit = detail.Unit;
                        existDtl.BillDtlDate = detail.BillDtlDate;
                        //existDtl.ActualQty = detail.ActualQty;
                        existDtl.WxWipIssueBillId = existBill.Id;
                        RF.Save(existDtl);
                    }
                    RF.Save(existBill);
                    // 排除已经下载 SAP更新后删除的明细 todo 
                    trans.Complete();
                }
            }
            catch (Exception e)
            {
                throw new Exception("外协领料单下载异常:" + e.Message);
            }
        }


    }
}
