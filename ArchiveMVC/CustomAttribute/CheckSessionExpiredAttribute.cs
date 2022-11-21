using ArchiveMVC.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ArchiveMVC.CustomAttribute
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class CustomAuthorizeAttribute : AuthorizeAttribute
    {
        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            if (filterContext.HttpContext.Request.IsAjaxRequest())
            {
                var redirectUrl = HttpRuntime.AppDomainAppVirtualPath;
                if (!redirectUrl.EndsWith("/"))
                    redirectUrl += "/" + (string)filterContext.RouteData.Values["controller"];
                else
                    redirectUrl += (string)filterContext.RouteData.Values["controller"];
                filterContext.Result = new JsonResult
                {
                    Data = new ECMJsonMessage { 
                        IsError = true, 
                        IsTimeOut = true, 
                        Message = "Time out" ,
                        Detail = redirectUrl
                    },
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet
                };
            }
            else
            {
                base.HandleUnauthorizedRequest(filterContext);
            }
        }

        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            if (Utility.Utilities.UserName != null)
                return true;
            return false;
        }
    }
}