using System;
using Platform.Controller;
using Platform.Domain;

namespace DownloadBill.Hisense.LOG
{
    public class ILogReportController : DomainController
    {
        public virtual EntityList GetList(ILogReportCriteria criteria)
        {
            var repo = RF.Concrete<ILogReportRepository>();
            var q = repo.CreateEntityQueryer();

            if (criteria.ContextInfo.IsNotEmpty())
                q.Where(p => p.ContextInfo.Contains( criteria.ContextInfo));
            if (criteria.InterfaceName.IsNotEmpty())
                q.Where(p => p.InterfaceName.Contains(criteria.InterfaceName));
            if (criteria.InterfaceType.IsNotEmpty())
                q.Where(p => p.InterfaceType.Contains(criteria.InterfaceType));
            if (criteria.IsSuccess.HasValue)
                q.Where(p => p.IsSuccess == criteria.IsSuccess);
            if (criteria.LogLevel.HasValue)
                q.Where(p => p.LogLevel == criteria.LogLevel);
            if (criteria.LogException.IsNotEmpty())
                q.Where(p => p.LogException.Contains(criteria.LogException));
            if (criteria.MainInfo.IsNotEmpty())
                q.Where(p => p.MainInfo.Contains(criteria.MainInfo));
            if (criteria.LogDate.BeginValue.HasValue)
            {
                q.Where(p => p.LogDate >= criteria.LogDate.BeginValue);
            }
            if (criteria.LogDate.EndValue.HasValue)
            {
                q.Where(p => p.LogDate <= criteria.LogDate.EndValue);
            }

            return repo.QueryList(q,criteria.PagingInfo) as ILogReportList;
        }
        
    }
}
