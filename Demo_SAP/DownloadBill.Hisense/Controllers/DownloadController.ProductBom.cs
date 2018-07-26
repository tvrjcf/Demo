using BD.Items;
using DownloadBill.Hisense.Download;
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
    public partial class DownloadController
    {
        public virtual Result DownloadProBom(string itemCode)
        {
            Result rs = new Result();
            try
            {
                var list = RFC_ProductBom.GetErpBom(itemCode);
                if (itemCode.IsNotEmpty() && list.Count() <= 0)
                {
                    rs.Success = false;
                    rs.Message = "找不到物料：" + itemCode + " 的BOM信息";
                    return rs;
                }
                foreach (var bom in list)
                {
                    var batch_no = Guid.NewGuid().ToSafeString();
                    var flag = ReceiveBom(bom, itemCode, batch_no);
                    if (!flag)
                        continue;
                    var model = DomainControllerFactory.Create<WorkOrderController>()
                        .FindProductBom(bom.Item.Code);
                    var del_ls = model.DetailList.Concrete().Where(p => p.DownBatchNo != batch_no);
                    foreach (var item in del_ls)
                    {
                        item.PersistenceStatus = PersistenceStatus.Deleted;
                        RF.Save(item);
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
        int count = 0;
        public virtual bool ReceiveBom(ProductBom bom, string itemCode, string batch_no)
        {
            count++;
            try
            {
                using (var trans = RF.TransactionScope(WMSEntityDataProvider.ConnectionStringName))
                {
                    //
                    var bomItem = DomainControllerFactory.Create<ItemController>().GetByCode(bom.Item.Code);
                    if (bomItem == null)
                    {
                        if (itemCode.IsNotEmpty())//单个下载就报错
                        {
                            throw new EntityNotFoundException(string.Format("不存在物料{0}，请先下载最新物料".Translate(), bom.Item.Code));
                        }
                        else { return false; }
                    }
                    var existBom = DomainControllerFactory.Create<WorkOrderController>()
                        .FindProductBom(bomItem) ?? bom;

                    existBom.Item = bomItem;
                    existBom.ItemId = bomItem.Id;
                    existBom.Code = bom.Code;
                    existBom.Name = bom.Name;
                    existBom.Version = bom.Version;

                    //EntityPhantomExtension.SetIsPhantom(bom, true);                    
                    //supplier.PersistenceStatus = existSupplier.PersistenceStatus;

                    foreach (var detail in bom.DetailList)
                    {
                        var existDtl = existBom.DetailList.Concrete().FirstOrDefault(p => p.Item.Code == detail.Item.Code) ?? detail;

                        //更新物料编码
                        var item = DomainControllerFactory.Create<ItemController>().GetByCode(detail.Item.Code);
                        if (item == null)
                        {
                            if (itemCode.IsNotEmpty())//单个下载就报错
                            {
                                throw new EntityNotFoundException(string.Format("不存在物料{0}，请先下载最新物料".Translate(), detail.Item.Code));
                            }
                            else { return false; }
                        }

                        existDtl.Item = item;
                        existDtl.ItemId = item.Id;
                        existDtl.UnitQty = detail.UnitQty;
                        existDtl.Warehouse = detail.Warehouse;
                        existDtl.Rgekz = detail.Rgekz;
                        existDtl.Unit = detail.Unit;
                        existDtl.ProductBomId = existBom.Id;
                        existDtl.DownBatchNo = batch_no;
                        RF.Save(existDtl);
                    }
                    var ls = new List<ProductBomDetail>();
                    var list = existBom.DetailList.Concrete().Where(p => p.Id <= 0);
                    ls.AddRange(list);
                    foreach (var item in ls)
                    {
                        existBom.DetailList.Remove(item);
                    };

                    RF.Save(existBom);
                    trans.Complete();
                }
                return true;
            }
            catch (Exception e)
            {
                throw new Exception("标准Bom数据下载异常:" + e.Message);
            }
        }
    }
}
