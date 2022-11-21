using ArchiveMVC.Models;
using ArchiveMVC.Models.DataProvider;
using ArchiveMVC.Utility;
using System;
using System.Collections.Generic;

using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ArchiveMVC.Controllers
{
    public class HomeController : Controller
    {
        //
        // GET: /Home/

        public ActionResult Index()
        {
            SecurityProvider _SecurityProvider = new SecurityProvider();
            
            //SearchProvider ss = new SearchProvider();
            //List<SearchResultModel> list = ss.RunGlobalSearch("", 1);

            DocumentTypeProvider _DocumentTypeProvider = new DocumentTypeProvider(Utility.Utilities.UserName,Utility.Utilities.Password);
            List<DocumentTypeModel> list = _DocumentTypeProvider.GetDocumentTypes();
            DocumentProvider r = new DocumentProvider(Utility.Utilities.UserName, Utility.Utilities.Password);
            foreach (DocumentTypeModel item in list)
            {
                DocumentModel d = r.GetDocument(Guid.Empty);
                int count = d.FieldValues.Count;
            }


            return View();
        }
       
        [HttpPost]
        public FileResult UploadTest(HttpPostedFileBase file)
        {



            // Create a byte array of file stream length
            byte[] ImageData = new byte[file.InputStream.Length];

            //Read block of bytes from stream into the byte array
            file.InputStream.Read(ImageData, 0, System.Convert.ToInt32(file.InputStream.Length));

            //Close the File Stream
            // fs.Close();
            //return ImageData; //return the byte data


            #region Tiff
            List<byte[]> list = Ecm.Utility.ImageProcessing.SplitTiff(ImageData);


            //return File(list[0], "image/tiff");
            return File(ProcessImages.TiffToJpeg(list[0]), "image/jpg");

            #endregion Tiff

            #region File to Image page
            return File(ImageData, "image/png");
            #endregion File to Image page


            // return "1";
        }


    }
}
