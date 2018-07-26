using BD;
using BD.Hisense;
using Platform.Domain;
using Platform.Domain.Common;
using Platform.MetaModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DownloadBill.Hisense.LOG
{
    /// <summary>
    /// 接口通用日志报表
    /// </summary>
    [RootEntity, Serializable]
    [ConditionQueryType(typeof(ILogReportCriteria))]
    public class ILogReport : InterfaceLog
    {
        public ILogReport() { }
    }

    // <summary>
    /// 报表 列表
    /// </summary>
    [Serializable]
    public partial class ILogReportList : EntityList<ILogReport> { }

    /// <summary>
    /// 报表 仓库
    /// </summary>
    internal partial class ILogReportRepository : BDEntityRepository<ILogReportRepository, ILogReport>
    {
        /// <summary>
        /// 单例模式，外界不可以直接构造本对象。
        /// </summary>
        protected ILogReportRepository() { }
    }
    /// <summary>
    ///  报表 实体配置
    /// </summary>
    internal class ILogReportConfig : EntityConfig<InterfaceLog>
    {
        protected override void ConfigMeta()
        {

        }
    }
}
