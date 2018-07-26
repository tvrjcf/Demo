using BD.Hisense;
using BD.Items;
using INV.Hisense.Warehouses.StorageAreas;
using INV.Hisense.Warehouses.StorageLocations;
using INV.Hisense.Warehouses.Warehouses;
using Platform;
using Platform.Domain;
using Platform.ManagedProperty;
using Platform.MetaModel;
using Platform.ObjectModel;
using System;

namespace DownloadBill.Hisense.LOG
{
    [QueryEntity, Serializable]
    public class ILogReportCriteria : Criteria<ILogReportCriteria>
    {
        #region 构造函数

        public ILogReportCriteria()
        {
            this.LogDate = new DateRange() { DateRangeType = DateRangeType.Today };
        }

        #endregion

        #region ContextInfo

        public static readonly Property<string> ContextInfoProperty = P<ILogReportCriteria>.Register(e => e.ContextInfo);
        /// <summary>
        /// 信息
        /// </summary>
        public string ContextInfo
        {
            get { return this.GetProperty(ContextInfoProperty); }
            set { this.SetProperty(ContextInfoProperty, value); }
        }
        #endregion

        #region InterfaceName

        /// <summary>
        /// 接口名称
        /// </summary>
        public static readonly Property<string> InterfaceNameProperty = P<ILogReportCriteria>.Register(e => e.InterfaceName);
        /// <summary>
        /// 接口名称
        /// </summary>
        public string InterfaceName
        {
            get { return this.GetProperty(InterfaceNameProperty); }
            set { this.SetProperty(InterfaceNameProperty, value); }
        }
        #endregion

        #region LogDate

        /// <summary>
        /// 信息日期
        /// </summary>
        public static readonly Property<DateRange> LogDateProperty = P<ILogReportCriteria>.Register(e => e.LogDate);
        /// <summary>
        /// 信息日期
        /// </summary>
        public DateRange LogDate
        {
            get { return this.GetProperty(LogDateProperty); }
            set { this.SetProperty(LogDateProperty, value); }
        }

        #endregion

        #region LogLevel

        public static readonly Property<LogLevel?> LogLevelProperty = P<ILogReportCriteria>.Register(e => e.LogLevel);
        /// <summary>
        /// 信息级别
        /// </summary>
        public LogLevel? LogLevel
        {
            get { return this.GetProperty(LogLevelProperty); }
            set { this.SetProperty(LogLevelProperty, value); }
        }

        #endregion

        #region LogException

        /// <summary>
        /// 异常信息
        /// </summary>
        public static readonly Property<string> LogExceptionProperty = P<ILogReportCriteria>.Register(e => e.LogException);
        /// <summary>
        /// 异常信息
        /// </summary>
        public string LogException
        {
            get { return this.GetProperty(LogExceptionProperty); }
            set { this.SetProperty(LogExceptionProperty, value); }
        }
        #endregion

        #region InterfaceType

        /// <summary>
        /// 接口类型
        /// </summary>
        public static readonly Property<string> InterfaceTypeProperty = P<ILogReportCriteria>.Register(e => e.InterfaceType);
        /// <summary>
        /// 接口类型
        /// </summary>
        public string InterfaceType
        {
            get { return this.GetProperty(InterfaceTypeProperty); }
            set { this.SetProperty(InterfaceTypeProperty, value); }
        }

        #endregion

        #region MainInfo

        /// <summary>
        /// 主要信息
        /// </summary>
        public static readonly Property<string> MainInfoProperty = P<ILogReportCriteria>.Register(e => e.MainInfo);
        /// <summary>
        /// 主要信息
        /// </summary>
        public string MainInfo
        {
            get { return this.GetProperty(MainInfoProperty); }
            set { this.SetProperty(MainInfoProperty, value); }
        }

        #endregion

        #region IsSuccess

        /// <summary>
        /// 是否成功 1 成功
        /// </summary>
        public static readonly Property<bool?> IsSuccessProperty = P<ILogReportCriteria>.Register(e => e.IsSuccess);
        /// <summary>
        /// 是否成功 1 成功
        /// </summary>
        public bool? IsSuccess
        {
            get { return this.GetProperty(IsSuccessProperty); }
            set { this.SetProperty(IsSuccessProperty, value); }
        }

        #endregion

        protected override EntityList Fetch()
        {
            var ctl = DomainControllerFactory.Create<ILogReportController>();
            return ctl.GetList(this);
        }
    }
}
