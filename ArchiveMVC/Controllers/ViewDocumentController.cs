using ArchiveMVC.Models;
using ArchiveMVC.Models.DataProvider;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ArchiveMVC.Controllers
{
    public class ViewDocumentController : Controller
    {
        //
        // GET: /ViewDocument/

        public ActionResult Index(Guid idDoc)
        {
            
            DocumentProvider documentProvider = new DocumentProvider(Utility.Utilities.UserName,Utility.Utilities.Password);
            DocumentModel model = documentProvider.GetDocument(idDoc);
            return View(model);
        }
        public ActionResult view(Guid idDoc)
        {

            DocumentProvider documentProvider = new DocumentProvider(Utility.Utilities.UserName, Utility.Utilities.Password);
            DocumentModel model = documentProvider.GetDocument(idDoc);
            
            return View("index",model);
        }

    }
}
