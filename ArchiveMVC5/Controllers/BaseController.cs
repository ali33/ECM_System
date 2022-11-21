using ArchiveMVC5.Models;
using ArchiveMVC5.Utility;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;

[assembly: log4net.Config.XmlConfigurator(Watch = true, ConfigFile = @"log4net.xml")]

namespace ArchiveMVC5.Controllers
{
    public class BaseController : Controller
    {
        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (Utilities.LoginUser == null)
            {
                filterContext.Result = RedirectToAction("Login", "Account", new LoginModel { ReturnUrl = Request.Url.AbsoluteUri });
            }

            Utility.Utilities.ProductVersion = new Models.ProductInfoModel();
            Utility.Utilities.ProductVersion.CopyRight = @"MIA Solutioninc " + DateTime.Today.Year;
            Utility.Utilities.ProductVersion.Version = Assembly.GetExecutingAssembly().GetName().Version.ToString();

        }

        private ILog _log = LogManager.GetLogger(typeof(CacheFileResult));

        protected void ExceptionLog(Exception ex, string errorMessage)
        {
            _log.Error(errorMessage, ex);
        }


    }
}