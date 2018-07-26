using Platform;
using Platform.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QM.Server
{
    public class DomainApp : AppImplementationBase
    {
        protected override void InitEnvironment()
        {
            PlatformEnvironment.Location.IsWebUI = false;
            PlatformEnvironment.Location.IsWPFUI = false;
            PlatformEnvironment.Location.DataPortalMode = PlatformEnvironment.Configuration.Section.DataPortalProxy == "Local" ?
                 DataPortalMode.ConnectDirectly : DataPortalMode.ThroughService;
            base.InitEnvironment();
        }
        public void Startup()
        {
            try
            {
                this.StartupApplication();
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        /// <summary>
        /// 当外部程序在完全退出时，通过领域应用程序也同时退出。
        /// </summary>
        public void NotifyExit()
        {
            try
            {
                this.OnExit();
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}
