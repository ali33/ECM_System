using ArchiveMVC.Models;
using ArchiveMVC.Utility;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ArchiveMVC.Controllers
{
    public class PermissionController : Controller
    {
        //
        // GET: /Permission/

        public ActionResult Index()
        {
            return View();
        }
        /// <summary>
        /// Get DocumentTypePermission and AnnotationPermission on DocumentType can be Capture
        /// </summary>
        /// <returns>Json</returns>
        public JsonResult CaptureDocumentTypes()
        {
            var docTypes = Utilities.CaptureDocumentTypes;
            var per = new Dictionary<string, Dictionary<string, dynamic>>();
            var annoPermission = new Dictionary<string, AnnotationPermissionModel>();
            dynamic docTypePermission = new Dictionary<string, DocumentTypePermissionModel>();
            foreach (var type in docTypes)
            {
                annoPermission.Add(type.Id.ToString(), type.AnnotationPermission);
                docTypePermission.Add(type.Id.ToString(), type.DocumentTypePermission);
            }
            //per.Add("annotation", annotations);
            //per.Add("documentType", (dynamic)docType);

            return new JsonResult { 
                Data = new { 
                    annotation = annoPermission,
                    documentType = docTypePermission
                },
                ContentType = "application/json",
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }
    }
}
