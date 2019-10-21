using Microsoft.Web.Administration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Demo_IIS
{
    public partial class IISHelper
    {
        /// <summary>
        /// 停止一个站点
        /// </summary>

        public static BaseResult StopSite(string site)
        {
            BaseResult result = BaseResult.Fail("未知错误");
            try
            {
                ServerManager iisManager = new ServerManager();
                if (iisManager.Sites[site].State == ObjectState.Stopped || iisManager.Sites[site].State == ObjectState.Stopping)
                {
                    result.Status = true;
                    result.StatusMessage = site + "：站点已停止";
                }
                else
                {
                    ObjectState state = iisManager.Sites[site].Stop();
                    if (state == ObjectState.Stopping || state == ObjectState.Stopped)
                    {
                        result.Status = true;
                        result.StatusMessage = site + "：站点已停止";
                    }
                    else
                    {
                        result.Status = false;
                        result.StatusMessage = site + "：站点停止失败";
                    }
                }
            }
            catch (Exception ex)
            {
                //NLogHelper.Warn(ex, " public static BaseResult StopSite(string site)");
                result.Status = false;
                result.StatusMessage = site + "：站点停止出现异常：" + ex.Message;
            }

            return result;
        }

        /// <summary>
        /// 启动一个站点
        /// </summary>

        public static BaseResult StartSite(string site)
        {
            BaseResult result = BaseResult.Fail("未知错误");
            try
            {
                ServerManager iisManager = new ServerManager();
                if (iisManager.Sites[site].State != ObjectState.Started || iisManager.Sites[site].State != ObjectState.Starting)
                {
                    ObjectState state = iisManager.Sites[site].Start();
                    if (state == ObjectState.Starting || state == ObjectState.Started)
                    {
                        result.Status = true;
                        result.StatusMessage = site + "：站点已启动";
                    }
                    else
                    {
                        result.Status = false;
                        result.StatusMessage = site + "：站点停止失败";
                    }
                }
                else
                {
                    result.Status = true;
                    result.StatusMessage = site + "：站点已是启动状态";
                }
            }
            catch (Exception ex)
            {
                //NLogHelper.Warn(ex, " public static BaseResult StartSite(string site)");
                result.Status = false;
                result.StatusMessage = site + "：站点停止出现异常：" + ex.Message;
            }

            return result;
        }

        /// <summary>
        /// 停止应用程序池
        /// </summary>
        /// <param name="appPool"></param>
        public static BaseResult StopApplicationPool(string appPool = null)
        {
            BaseResult result = BaseResult.Fail("未知错误");
            try
            {
                if (string.IsNullOrWhiteSpace(appPool))
                {
                    appPool = Environment.GetEnvironmentVariable("APP_POOL_ID", EnvironmentVariableTarget.Process);
                }
                ServerManager iisManager = new ServerManager();
                if (iisManager.ApplicationPools[appPool].State == ObjectState.Stopped || iisManager.ApplicationPools[appPool].State == ObjectState.Stopping)
                {
                    result.Status = true;
                    result.StatusMessage = appPool + "：应用程序池已停止";
                }
                else
                {
                    ObjectState state = iisManager.ApplicationPools[appPool].Stop();
                    if (state == ObjectState.Stopping || state == ObjectState.Stopped)
                    {
                        result.Status = true;
                        result.StatusMessage = appPool + "：应用程序池已停止";
                    }
                    else
                    {
                        result.Status = false;
                        result.StatusMessage = appPool + "：应用程序池停止失败，请重试";
                    }
                }
            }
            catch (Exception ex)
            {
                //NLogHelper.Warn(ex, "  public static BaseResult StopApplicationPool(string appPool = null)");
                result.Status = false;
                result.StatusMessage = appPool + "：应用程序池失败出现异常：" + ex.Message;
            }

            return result;
        }

        /// <summary>
        /// 启动应用程序池
        /// </summary>
        /// <param name="appPool"></param>
        public static BaseResult StartApplicationPool(string appPool = null)
        {
            BaseResult result = BaseResult.Fail("未知错误");
            try
            {
                if (string.IsNullOrWhiteSpace(appPool))
                {
                    appPool = Environment.GetEnvironmentVariable("APP_POOL_ID", EnvironmentVariableTarget.Process);
                }
                ServerManager iisManager = new ServerManager();
                if (iisManager.ApplicationPools[appPool].State != ObjectState.Started || iisManager.ApplicationPools[appPool].State != ObjectState.Starting)
                {
                    ObjectState state = iisManager.ApplicationPools[appPool].Start();
                    if (state == ObjectState.Starting || state == ObjectState.Started)
                    {
                        result.Status = true;
                        result.StatusMessage = appPool + "：应用程序池已启动";
                    }
                    else
                    {
                        result.Status = false;
                        result.StatusMessage = appPool + "：应用程序池启动失败，请重试";
                    }
                }
                else
                {
                    result.Status = true;
                    result.StatusMessage = appPool + "：应用程序池已是启动状态";
                }
            }
            catch (Exception ex)
            {
                //NLogHelper.Warn(ex, "  public static BaseResult StartApplicationPool(string appPool = null)");
                result.Status = false;
                result.StatusMessage = appPool + "：应用程序池启动出现异常：" + ex.Message;
            }

            return result;
        }

        /// <summary>
        /// 回收应用程序池
        /// </summary>
        /// <param name="appPool"></param>
        public static BaseResult RecycleApplicationPool(string appPool = null)
        {
            BaseResult result = BaseResult.Fail("未知错误");
            try
            {
                if (string.IsNullOrWhiteSpace(appPool))
                {
                    appPool = Environment.GetEnvironmentVariable("APP_POOL_ID", EnvironmentVariableTarget.Process);
                }
                ServerManager iisManager = new ServerManager();
                ObjectState state = iisManager.ApplicationPools[appPool].Recycle();

                result.Status = true;
                result.StatusMessage = appPool + "：应用程序池回收成功：" + state;

            }
            catch (Exception ex)
            {
                //NLogHelper.Warn(ex, "  public static BaseResult StartApplicationPool(string appPool = null)");
                result.Status = false;
                result.StatusMessage = appPool + "：应用程序池回收出现异常：" + ex.Message;
            }

            return result;
        }

        /// <summary>
        /// 运行时控制：得到当前正在处理的请求
        /// </summary>
        /// <param name="appPool"></param>
        public static void GetWorking(string appPool)
        {
            ServerManager iisManager = new ServerManager();
            foreach (WorkerProcess w3wp in iisManager.WorkerProcesses)
            {
                Console.WriteLine("W3WP ({0})", w3wp.ProcessId);
                foreach (Request request in w3wp.GetRequests(0))
                {
                    Console.WriteLine("{0} - {1},{2},{3}",
                                request.Url,
                                request.ClientIPAddr,
                                request.TimeElapsed,
                                request.TimeInState);
                }
            }
        }

        /// <summary>
        /// 获取IIS日志文件路径
        /// </summary>
        /// <returns></returns>

        public static string GetIISLogPath()
        {
            ServerManager manager = new ServerManager();
            // 获取IIS配置文件：applicationHost.config
            var config = manager.GetApplicationHostConfiguration();
            var log = config.GetSection("system.applicationHost/log");
            var logFile = log.GetChildElement("centralW3CLogFile");
            //获取网站日志文件保存路径
            var logPath = logFile.GetAttributeValue("directory").ToString();
            return logPath;
        }

        /// <summary>
        ///创建新站点
        /// </summary>
        /// <param name="siteName"></param>
        /// <param name="bindingInfo">"*:&lt;port&gt;:&lt;hostname&gt;" <example>"*:80:myhost.com"</example></param>
        /// <param name="physicalPath"></param>
        public static void CreateSite(string siteName, string bindingInfo, string physicalPath)
        {
            createSite(siteName, "http", bindingInfo, physicalPath, true, siteName + "Pool", ProcessModelIdentityType.NetworkService, null, null, ManagedPipelineMode.Integrated, null);
        }

        /// <summary>
        /// 创建新站点
        /// </summary>
        /// <param name="siteName"></param>
        /// <param name="protocol"></param>
        /// <param name="bindingInformation"></param>
        /// <param name="physicalPath"></param>
        /// <param name="createAppPool"></param>
        /// <param name="appPoolName"></param>
        /// <param name="identityType"></param>
        /// <param name="appPoolUserName"></param>
        /// <param name="appPoolPassword"></param>
        /// <param name="appPoolPipelineMode"></param>
        /// <param name="managedRuntimeVersion"></param>
        private static void createSite(string siteName, string protocol, string bindingInformation, string physicalPath,
                bool createAppPool, string appPoolName, ProcessModelIdentityType identityType,
                string appPoolUserName, string appPoolPassword, ManagedPipelineMode appPoolPipelineMode, string managedRuntimeVersion)
        {
            using (ServerManager mgr = new ServerManager())
            {
                Site site = mgr.Sites.Add(siteName, protocol, bindingInformation, physicalPath);

                // PROVISION APPPOOL IF NEEDED
                if (createAppPool)
                {
                    ApplicationPool pool = mgr.ApplicationPools.FirstOrDefault(p => p.Name == appPoolName);
                    if (pool == null)
                    {
                        pool = mgr.ApplicationPools.Add(appPoolName);
                        if (pool.ProcessModel.IdentityType != identityType)
                        {
                            pool.ProcessModel.IdentityType = identityType;
                        }
                        if (!String.IsNullOrEmpty(appPoolUserName))
                        {
                            pool.ProcessModel.UserName = appPoolUserName;
                            pool.ProcessModel.Password = appPoolPassword;
                        }
                        if (appPoolPipelineMode != pool.ManagedPipelineMode)
                        {
                            pool.ManagedPipelineMode = appPoolPipelineMode;
                        }
                    }

                    site.Applications["/"].ApplicationPoolName = pool.Name;
                }

                mgr.CommitChanges();
            }
        }

        /// <summary>
        /// Delete an existent web site.
        /// </summary>
        /// <param name="siteName">Site name.</param>
        public static void DeleteSite(string siteName)
        {
            using (ServerManager mgr = new ServerManager())
            {
                Site site = mgr.Sites[siteName];
                if (site != null)
                {
                    mgr.Sites.Remove(site);
                    mgr.CommitChanges();
                }
            }
        }

        /// <summary>
        /// 创建虚拟目录
        /// </summary>
        /// <param name="siteName"></param>
        /// <param name="vDirName"></param>
        /// <param name="physicalPath"></param>
        public static void CreateVDir(string siteName, string vDirName, string physicalPath)
        {
            using (ServerManager mgr = new ServerManager())
            {
                Site site = mgr.Sites[siteName];
                if (site == null)
                {
                    throw new ApplicationException(String.Format("Web site {0} does not exist", siteName));
                }
                site.Applications.Add("/" + vDirName, physicalPath);
                mgr.CommitChanges();
            }
        }

        /// <summary>
        /// 删除虚拟目录
        /// </summary>
        /// <param name="siteName"></param>
        /// <param name="vDirName"></param>
        public static void DeleteVDir(string siteName, string vDirName)
        {
            using (ServerManager mgr = new ServerManager())
            {
                Site site = mgr.Sites[siteName];
                if (site != null)
                {
                    Microsoft.Web.Administration.Application app = site.Applications["/" + vDirName];
                    if (app != null)
                    {
                        site.Applications.Remove(app);
                        mgr.CommitChanges();
                    }
                }
            }
        }

        /// <summary>
        /// Delete an existent web site app pool.
        /// </summary>
        /// <param name="appPoolName">App pool name for deletion.</param>
        public static void DeletePool(string appPoolName)
        {
            using (ServerManager mgr = new ServerManager())
            {
                ApplicationPool pool = mgr.ApplicationPools[appPoolName];
                if (pool != null)
                {
                    mgr.ApplicationPools.Remove(pool);
                    mgr.CommitChanges();
                }
            }
        }

        /// <summary>
        /// 在站点上添加默认文档。
        /// </summary>
        /// <param name="siteName"></param>
        /// <param name="defaultDocName"></param>
        public static void AddDefaultDocument(string siteName, string defaultDocName)
        {
            using (ServerManager mgr = new ServerManager())
            {
                Configuration cfg = mgr.GetWebConfiguration(siteName);
                ConfigurationSection defaultDocumentSection = cfg.GetSection("system.webServer/defaultDocument");
                ConfigurationElement filesElement = defaultDocumentSection.GetChildElement("files");
                ConfigurationElementCollection filesCollection = filesElement.GetCollection();

                foreach (ConfigurationElement elt in filesCollection)
                {
                    if (elt.Attributes["value"].Value.ToString() == defaultDocName)
                    {
                        return;
                    }
                }

                try
                {
                    ConfigurationElement docElement = filesCollection.CreateElement();
                    docElement.SetAttributeValue("value", defaultDocName);
                    filesCollection.Add(docElement);
                }
                catch (Exception) { }   //this will fail if existing

                mgr.CommitChanges();
            }
        }

        /// <summary>
        ///   检查虚拟目录是否存在。
        /// </summary>
        /// <param name="siteName"></param>
        /// <param name="path"></param>
        /// <returns></returns>

        public static bool VerifyVirtualPathIsExist(string siteName, string path)
        {
            using (ServerManager mgr = new ServerManager())
            {
                Site site = mgr.Sites[siteName];
                if (site != null)
                {
                    foreach (Microsoft.Web.Administration.Application app in site.Applications)
                    {
                        if (app.Path.ToUpper().Equals(path.ToUpper()))
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        /// <summary>
        ///  检查站点是否存在。
        /// </summary>
        /// <param name="siteName"></param>
        /// <returns></returns>
        public static bool VerifyWebSiteIsExist(string siteName)
        {
            using (ServerManager mgr = new ServerManager())
            {
                for (int i = 0; i < mgr.Sites.Count; i++)
                {
                    if (mgr.Sites[i].Name.ToUpper().Equals(siteName.ToUpper()))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        ///   检查Bindings信息。
        /// </summary>
        /// <param name="bindingInfo"></param>
        /// <returns></returns>
        public static bool VerifyWebSiteBindingsIsExist(string bindingInfo)
        {
            string temp = string.Empty;
            using (ServerManager mgr = new ServerManager())
            {
                for (int i = 0; i < mgr.Sites.Count; i++)
                {
                    foreach (Microsoft.Web.Administration.Binding b in mgr.Sites[i].Bindings)
                    {
                        temp = b.BindingInformation;
                        if (temp.IndexOf('*') < 0)
                        {
                            temp = "*" + temp;
                        }
                        if (temp.Equals(bindingInfo))
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

    }

    public class BaseResult
    {

        public bool Status = false;
        public string StatusMessage = string.Empty;
        public BaseResult(bool status, string message)
        {
            Status = status;
            StatusMessage = message;
        }
        public static BaseResult Fail(string error)
        {
            return new BaseResult(false, error);
        }
    }
}
