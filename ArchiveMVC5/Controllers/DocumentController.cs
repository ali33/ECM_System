using ArchiveMVC5.Models;
using ArchiveMVC5.Models.DataProvider;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ArchiveMVC5.Controllers
{
    public class DocumentController : Controller
    {
        //
        // GET: /Document/

        public ActionResult Index()
        {
            //if(!Checking.CheckUserLogin()) return RedirectToAction("Index", "Login");
            return View();
        }

        public ActionResult GetDoc(Guid id)
        {
            //if(!Checking.CheckUserLogin()) return RedirectToAction("Index", "Login");
            DocumentProvider docProvider = new DocumentProvider(Utility.Utilities.UserName, Utility.Utilities.Password);
            DocumentModel doc = docProvider.GetDocument(id);
            return View(doc);
        }

        public ActionResult GetDocs(String id)
        {
            //if(!Checking.CheckUserLogin()) return RedirectToAction("Index", "Login");
            DocumentProvider docProvider = new DocumentProvider(Utility.Utilities.UserName, Utility.Utilities.Password);
            String[] idArray = id.Split(new Char[]{' '});
            List<Guid> lstId = new List<Guid>();
            foreach (var _id in idArray)
            {
                lstId.Add(Guid.Parse(_id));
            }
            List<DocumentModel> docs = docProvider.GetDocuments(lstId);
            return View(docs);
        }
    }
}
