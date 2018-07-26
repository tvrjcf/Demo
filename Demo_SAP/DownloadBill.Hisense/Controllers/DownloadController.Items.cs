using BD;
using BD.Items;
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
        public virtual Result DownloadItems(string itemCode)
        {
            Result rs = new Result();
            try
            {
                var list = RFC_Items.GetItemByCode(itemCode);
                if (itemCode.IsNotEmpty() && list.Count() <= 0)
                {
                    rs.Success = false;
                    rs.Message = "找不到物料：" + itemCode;
                    return rs;
                }
                foreach (var item in list)
                {
                    SaveOrUpdateItem(item, itemCode);
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
        public virtual void SaveOrUpdateItem(Item item, string itemCode)
        {
            try
            {
                //if (factoryCode.IsNullOrWhiteSpace()) throw new ArgumentNullException("factoryCode");
                //SetInvOrgIdByFactoryCode(factoryCode);
                using (var trans = RF.TransactionScope(BDEntityDataProvider.ConnectionStringName))
                {
                    //获取物料
                    var existItem = DomainControllerFactory.Create<ItemController>().GetByCode(item.Code) ?? item;

                    //if (ItemExtendsion.GetErpUpdate(existItem) == item.UpdateDate && !isForce) return;                    
                    //ItemExtendsion.SetErpUpdate(item, item.UpdateDate);
                    //existItem.PersistenceStatus = item.PersistenceStatus;

                    existItem.DataSource = EumDataSource.ERP;
                    existItem.Name = string.IsNullOrEmpty(item.Name) ? item.Code : item.Name;
                    existItem.Description = item.Description;
                    existItem.MeasurementUnit = item.MeasurementUnit;
                    existItem.ItemType = item.ItemType;
                    existItem.State = item.State;
                    existItem.CustomerType = item.CustomerType;
                    existItem.BaseModel = item.BaseModel;
                    existItem.InerCode = item.InerCode;
                    existItem.ReplaceItem = item.ReplaceItem;
                    existItem.MdmItemCode = item.MdmItemCode;
                    existItem.SmallCategory = item.SmallCategory;
                    //物料关联物料组，并保存
                    ItemSmallCategory existCategory = UpdateOrAdd(item.Category);
                    existItem.CategoryId = existCategory.Id;
                    //InvOrgIdExtension.SetInvOrgId(item, PlatformEnvironment.InvOrgId);
                    RF.Save(existItem);

                    trans.Complete();
                }
            }
            catch (Exception e)
            {
                
            }
        }
        /// <summary>
        /// 更新或新建物料组
        /// </summary>
        /// <param name="category">物料组</param>
        /// <returns></returns>
        private ItemSmallCategory UpdateOrAdd(ItemSmallCategory category)
        {
            var existCategory = FindCategoryByCode(category.Code);
            if (existCategory != null)
            {
                existCategory.Name = category.Name;
                existCategory.Code = category.Code;
            }
            else
            {
                existCategory = category;
            }

            var existMidCategory = CreateMediumCategory(category);

            var existBigCategory = CreateLargeCategory(category);

            InvOrgIdExtension.SetInvOrgId(existBigCategory, PlatformEnvironment.InvOrgId);
            RF.Save(existBigCategory);

            InvOrgIdExtension.SetInvOrgId(existMidCategory, PlatformEnvironment.InvOrgId);
            existMidCategory.LargeCategoryId = existBigCategory.Id;
            RF.Save(existMidCategory);

            InvOrgIdExtension.SetInvOrgId(existCategory, PlatformEnvironment.InvOrgId);
            existCategory.MediumCategoryId = existMidCategory.Id;
            RF.Save(existCategory);

            return existCategory;
        }
        /// <summary>
        /// 通过小类创建大类
        /// </summary>
        /// <param name="category"></param>
        /// <returns></returns>
        private ItemLargeCategory CreateLargeCategory(ItemSmallCategory category)
        {
            var bigCategory = new ItemLargeCategory { Code = category.Code, Name = category.Name };
            var existBigCategory = FindLargeCategoryByCode(category.Code);
            if (existBigCategory != null)
            {
                existBigCategory.Name = bigCategory.Name;
                existBigCategory.Code = bigCategory.Code;
            }
            else
            {
                existBigCategory = bigCategory;
            }
            return existBigCategory;
        }
        /// <summary>
        /// 通过小类创建中类
        /// </summary>
        /// <param name="category"></param>
        /// <returns></returns>
        private ItemMediumCategory CreateMediumCategory(ItemSmallCategory category)
        {
            var midCategory = new ItemMediumCategory { Code = category.Code, Name = category.Name };
            var existMidCategory = FindMediumCategoryByCode(category.Code);
            if (existMidCategory != null)
            {
                existMidCategory.Name = midCategory.Name;
                existMidCategory.Code = midCategory.Code;
            }
            else
            {
                existMidCategory = midCategory;
            }
            return existMidCategory;
        }

        private ItemSmallCategory UpdateOrAddExt(ItemSmallCategory category)
        {
            var existMidCategory = CreateMediumCategory(category);

            var existBigCategory = CreateLargeCategory(category);

            InvOrgIdExtension.SetInvOrgId(existBigCategory, PlatformEnvironment.InvOrgId);
            RF.Save(existBigCategory);

            InvOrgIdExtension.SetInvOrgId(existMidCategory, PlatformEnvironment.InvOrgId);
            existMidCategory.LargeCategoryId = existBigCategory.Id;
            RF.Save(existMidCategory);

            category.MediumCategoryId = existMidCategory.Id;

            return category;
        }

        /// <summary>
        /// 更新物料小类
        /// </summary>
        /// <param name="category"></param>
        /// <param name="isForce"></param>
        public virtual void ReceiveCategory(ItemSmallCategory category, bool isForce = false)
        {
            try
            {
                using (var trans = RF.TransactionScope(BDEntityDataProvider.ConnectionStringName))
                {
                    // SetInvOrgIdByCorporation(PlatformEnvironment.InvOrgId.ToString());
                    //获取物料小类

                    var existCat = FindCategoryByCode(category.Code) ?? category;

                    //if (ItemExtendsion.GetErpUpdate(existItem) == item.UpdateDate && !isForce) return;
                    existCat.Name = category.Name;
                    existCat = UpdateOrAddExt(existCat);
                    //ItemExtendsion.SetErpUpdate(item, item.UpdateDate);
                    //category.PersistenceStatus = existCat.PersistenceStatus;                                        

                    //更新或新建物料组

                    //InvOrgIdExtension.SetInvOrgId(category, PlatformEnvironment.InvOrgId);
                    RF.Save(existCat);

                    trans.Complete();
                }
            }
            catch (Exception e)
            {
                
            }
        }

        /// <summary>
        /// 根据物料小类代码查找物料小类
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        private ItemSmallCategory FindCategoryByCode(string code)
        {
            var categoryRepo = RF.Find<ItemSmallCategory>();
            var categoryQueryer = categoryRepo.CreateEntityQueryer<ItemSmallCategory>();
            categoryQueryer.Where(f => f.Code == code);
            return categoryRepo.QueryFirst(categoryQueryer) as ItemSmallCategory;
        }

        /// <summary>
        /// 根据物料中类代码查找物料中类
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        private ItemMediumCategory FindMediumCategoryByCode(string code)
        {
            var categoryRepo = RF.Find<ItemMediumCategory>();
            var categoryQueryer = categoryRepo.CreateEntityQueryer<ItemMediumCategory>();
            categoryQueryer.Where(f => f.Code == code);
            return categoryRepo.QueryFirst(categoryQueryer) as ItemMediumCategory;
        }

        /// <summary>
        /// 根据物料大类代码查找物料大类
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        private ItemLargeCategory FindLargeCategoryByCode(string code)
        {
            var categoryRepo = RF.Find<ItemLargeCategory>();
            var categoryQueryer = categoryRepo.CreateEntityQueryer<ItemLargeCategory>();
            categoryQueryer.Where(f => f.Code == code);
            return categoryRepo.QueryFirst(categoryQueryer) as ItemLargeCategory;
        }
    }
}
