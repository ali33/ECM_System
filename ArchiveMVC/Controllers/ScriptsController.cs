using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ArchiveMVC.Controllers
{
    public class ScriptsController : Controller
    {
        //
        // GET: /Scripts/

        public ActionResult Index()
        {
            return View();
        }

        [ActionName("Dynamic.js")]
        public ActionResult Dynamic(string message)
        {
            return this.JavaScriptFromView(model: message);
        }

        protected override void HandleUnknownAction(string actionName)
        {
            var res = this.JavaScriptFromView();
            res.ExecuteResult(ControllerContext);
        }
    }
}
