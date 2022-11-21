using CaptureMVC.Utility;
using log4net;
using System;
using System.Globalization;
using System.Threading;
using System.Web.Mvc;

namespace CaptureMVC.Controllers
{
    /// <summary>
    /// Class base of ECM controller
    /// </summary>
    public class BaseController : Controller
    {
        protected static readonly ILog log =
            LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Override this for change language
        /// </summary>
        /// <param name="requestContext"></param>
        protected override void Initialize(System.Web.Routing.RequestContext requestContext)
        {
            var language = Utilities.Language;
            if (language != null)
            {
                var cultureInfo = new CultureInfo(language.Format);

                // Set date time pattern
                cultureInfo.DateTimeFormat.FullDateTimePattern = language.DateFormat + " " + language.TimeFormat;
                cultureInfo.DateTimeFormat.ShortDatePattern = language.DateFormat;
                cultureInfo.DateTimeFormat.ShortTimePattern = language.TimeFormat;
                // Set number pattern
                cultureInfo.NumberFormat.NumberDecimalSeparator = language.DecimalChar;
                cultureInfo.NumberFormat.NumberGroupSeparator = language.ThousandChar;

                Thread.CurrentThread.CurrentUICulture = cultureInfo;
                Thread.CurrentThread.CurrentCulture = cultureInfo;
            }
            else
            {
                // Set default language is English
                var cultureInfo = new CultureInfo("en-US");

                // Set date time pattern
                cultureInfo.DateTimeFormat.FullDateTimePattern = "MM/dd/yyyy HH:mm:ss";
                cultureInfo.DateTimeFormat.ShortDatePattern = "MM/dd/yyyy";
                cultureInfo.DateTimeFormat.ShortTimePattern = "HH:mm:ss";
                // Set number pattern
                cultureInfo.NumberFormat.NumberDecimalSeparator = ".";
                cultureInfo.NumberFormat.NumberGroupSeparator = ",";

                Thread.CurrentThread.CurrentUICulture = cultureInfo;
                Thread.CurrentThread.CurrentCulture = cultureInfo;
            }


            base.Initialize(requestContext);
        }

        private void LogError(object error)
        {
            var message = string.Format("[{0}] - Error in controller [{1}], action [{2}]"
                                        + Environment.NewLine + "   {3}",
                                        Utilities.UserName,
                                        this.ControllerContext.RouteData.Values["controller"],
                                        this.ControllerContext.RouteData.Values["action"],
                                        error);

            log.Error(message);
        }

        protected void ProcessError(object error)
        {
            this.LogError(error);
        }
    }
}
