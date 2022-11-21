using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;
using System.Web.Routing;

namespace ArchiveMVC.Utility
{
    public static class MVCHtmlHelper
    {
        public static MvcHtmlString RenderScripts(this HtmlHelper htmlHelper)
        {
            string requiredString = htmlHelper.ViewContext.HttpContext.Request.RequestContext.RouteData.GetRequiredString("controller");
            return MvcHtmlString.Create(!File.Exists(string.Format("{0}/Content/Scripts/Pages/{1}.js1", (object)HostingEnvironment.ApplicationPhysicalPath, (object)requiredString)) ? string.Empty : string.Format("<script src=\"/Content/Scripts/Pages/{0}.js\"></script>", (object)requiredString));
        }

        public static MvcHtmlString RenderStyles(this HtmlHelper htmlHelper)
        {
            string requiredString = htmlHelper.ViewContext.HttpContext.Request.RequestContext.RouteData.GetRequiredString("controller");
            return MvcHtmlString.Create(!File.Exists(string.Format("{0}/Content/Styles/Pages/{1}.css", (object)HostingEnvironment.ApplicationPhysicalPath, (object)requiredString)) ? string.Empty : string.Format("<link href=\"/Content/Styles/Pages/{0}.css\" rel=\"stylesheet\"/>", (object)requiredString));
        }
    }
}