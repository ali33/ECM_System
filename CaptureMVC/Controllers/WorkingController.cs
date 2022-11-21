using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CaptureMVC.Utility;
using CaptureMVC.Models;
using System.Collections.ObjectModel;
using System.Collections;
using System.Web.Security;
using Ecm.CaptureDomain;
using CaptureMVC.DataProvider;
using CaptureMVC.Resources;
using Newtonsoft.Json;
using System.Collections.Specialized;
using System.IO;
//using Ecm.ContentExtractor.OpenOffice;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using Ecm.Utility;
using System.Web.Script.Serialization;
using System.Xml.Serialization;
using System.Transactions;
using System.Xml;
using System.Text;

namespace CaptureMVC.Controllers
{
    public class WorkingController : BaseController
    {
        [HttpGet]
        public ActionResult Index()
        {
            try
            {
                var captureBatches = Session[Constant.SESSION_CAPTURE_BATCHES] as List<CaptureBatchModel>;



                return View(captureBatches);



            }
            catch (Exception ex)
            {
                base.ProcessError(ex);
                return null;
            }
        }
    }
}
