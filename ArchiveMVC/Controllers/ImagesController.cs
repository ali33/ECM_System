using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Ecm.ArchiveMVC.Controllers
{
    public class ImagesController : Controller
    {
        //
        // GET: /Images/

        public ActionResult Index()
        {
            
            return View();
        
        }

        public FileResult ID()
        {
            System.IO.FileStream fs = new System.IO.FileStream(@"D:\Downloads\01.gif", System.IO.FileMode.Open, System.IO.FileAccess.Read);

            // Create a byte array of file stream length
            byte[] ImageData = new byte[fs.Length];

            //Read block of bytes from stream into the byte array
            fs.Read(ImageData, 0, System.Convert.ToInt32(fs.Length));

            //Close the File Stream
            fs.Close();
            //return ImageData; //return the byte data

            return File(ImageData, "image/png");
        }

    }
}
