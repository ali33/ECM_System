using CaptureMVC.DataProvider;
using CaptureMVC.Models;
using Ecm.CaptureDomain;
using log4net;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Hosting;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace CaptureMVC
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : System.Web.HttpApplication
    {
        protected static ILog log;


        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();

            //WebApiConfig.Register(GlobalConfiguration.Configuration);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            //AuthConfig.RegisterAuth();
            log4net.Config.XmlConfigurator.Configure();
            log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        }

        protected void Session_Start(object sender, EventArgs e)
        {
            Session[Constant.SESSION_OPENED_BATCHES] = new OrderedDictionary();
            Session[Constant.SESSION_ACTIVE_OPENED_BATCH_ID] = Guid.Empty;
            Session[Constant.SESSION_TEMP_FILE] = new List<string>();

            Session[Constant.SESSION_CAPTURE_BATCHES] = new List<CaptureBatchModel>();
            //Session[Constant.SESSION_CAPTURE_BATCH_TYPES] = new List<BatchType>();
            Session[Constant.SESSION_CAPTURE_PAGES] = new List<ViewPageModel>();
            Session[Constant.SESSION_AMBIGUOUS_DEFINITION] =
                new Dictionary<Guid, List<CaptureAmbiguousDefinitionModel>>();
            Session[Constant.SESSION_CAPTURE_OCR_IMAGES] =
                new Dictionary<Guid, Bitmap>();

            #region Create the working folder for session

            try
            {
                var sessionGuid = Guid.NewGuid();
                var sessionFolder = Path.Combine(Server.MapPath("~/CaptureFolder/"), sessionGuid.ToString());
                Directory.CreateDirectory(sessionFolder);
                Session[Constant.SESSION_FOLDER] = sessionFolder;
                Session[Constant.SESSION_GUID] = sessionGuid;
            }
            catch { }

            #endregion

        }

        protected void Session_End(object sender, EventArgs e)
        {
            #region Delete temp file
            try
            {
                var tempFiles = Session[Constant.SESSION_TEMP_FILE] as List<string>;
                var fileFoder = HostingEnvironment.MapPath("~/" + Constant.APP_KEY_FOLDER_TEMP_FILES);

                foreach (var file in tempFiles)
                {
                    System.IO.File.Delete(fileFoder + "/" + file);
                }
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
            #endregion

            #region Unlock batch
            try
            {
                // Get opened batches from session
                var openedBatches = Session[Constant.SESSION_OPENED_BATCHES] as OrderedDictionary;

                // Get list batch id to unlock
                var unlockBatchIds = new List<Guid>();
                ViewBatchModel openedBatch;
                for (int i = 0; i < openedBatches.Count; i++)
                {
                    openedBatch = openedBatches[i] as ViewBatchModel;
                    unlockBatchIds.Add(openedBatch.Id);
                }

                // Unlock batches
                var workItemProvider = new WorkItemProvider();
                workItemProvider.UnLockWorkItems(unlockBatchIds);
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
            #endregion

            #region Delete session folder
            try
            {
                var sessionFolder = Session[Constant.SESSION_FOLDER] as string;
                Directory.Delete(sessionFolder, true);
                Session[Constant.SESSION_FOLDER] = true;
            }
            catch { }
            #endregion
        }

    }
}