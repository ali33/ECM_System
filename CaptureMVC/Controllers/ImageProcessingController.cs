using CaptureMVC.Models;
using CaptureMVC.Utility;
using log4net;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Web;
using System.Web.Caching;
using System.Web.Mvc;

namespace ArchiveMVC.Controllers
{
    public class ImageProcessingController : Controller
    {
        private static readonly ILog log =
            LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        ////
        //// GET: /ImageProcessing/
        public ActionResult Index()
        {
            //if (Utilities.UserName == null || Utilities.Password == null)
            //{
            //    var login = new LoginModel { RedirectUrl = Request.RawUrl };
            //    RedirectToAction(Constant.ACTION_INDEX, Constant.CONTROLLER_LOGIN, login);
            //}
            return View();
        }
        [HttpGet]
        public FileResult GetIcon(String key)
        {
            byte[] obj = (byte[])System.Web.HttpContext.Current.Cache[key];
            return File(obj, ProcessImages.GetStringImage(obj));
            //return File("~/Images/appbar.page.check.png", ContentTypeEnumeration.Image.PNG);
        }
        //[HttpGet]
        //public FileResult ID(String key , bool thumb = false)
        //{
            
        //    CacheObject obj=null;
        //    try{
        //        obj = (CacheImage)System.Web.HttpContext.Current.Cache[key];
        //    }catch{
        //        //Nothing
        //    }

        //    if(obj==null)                
        //        obj = (CacheTemporaryFile)System.Web.HttpContext.Current.Cache[key];
        //    if (obj == null)
        //        return File("~/Images/text.png", ContentTypeEnumeration.Image.PNG);
        //    if (obj.ContentType == ContentTypeEnumeration.Image.TIFF
        //        //|| ContentTypeEnumeration.Image.TIFF.ToString().Contains(obj.ContentType)
        //        || obj.ContentType.StartsWith(ContentTypeEnumeration.Image.IMAGE_TYPE))
        //    {
        //        Image img = ProcessImages.ByteArrayToImage(ProcessImages.TiffToJpeg(((CacheImage)obj).FileBinaries));
        //        if (thumb == true)
        //            img = img.GetThumbnailImage(76, 76, null, new IntPtr());
        //        //img = ProcessImages.Rotate(img, rote);
        //        //if (img.Width > Constant.LIMIT_WIDTH_OF_PAGE_IMAGE)
        //        //{
        //        //    img = ProcessImages.ResizeImage(img, Constant.LIMIT_WIDTH_OF_PAGE_IMAGE);
        //        //}
        //        byte[] arr = ProcessImages.ImageToByteArray(img,System.Drawing.Imaging.ImageFormat.Jpeg);
                
        //        return File(arr, ContentTypeEnumeration.Image.JPEG);
        //    }
        //    if (obj.ContentType.Equals(ContentTypeEnumeration.Document.MSOffice.DOC) ||
        //        obj.ContentType.Equals(ContentTypeEnumeration.Document.MSOffice.DOCX) ||
        //        obj.ContentType.Equals(ContentTypeEnumeration.Document.MSOffice.DOTX))
        //        return File("~/Images/word.png", ContentTypeEnumeration.Image.PNG);
        //    if (obj.ContentType.Equals(ContentTypeEnumeration.Document.MSOffice.XLS) ||
        //        obj.ContentType.Equals(ContentTypeEnumeration.Document.MSOffice.XLSX) ||
        //        obj.ContentType.Equals(ContentTypeEnumeration.Document.MSOffice.XLTX))
        //            return File("~/Images/excel.png", ContentTypeEnumeration.Image.PNG);
        //    if (obj.ContentType.Equals(ContentTypeEnumeration.Document.MSOffice.PPT) ||
        //        obj.ContentType.Equals(ContentTypeEnumeration.Document.MSOffice.PPTX) ||
        //        obj.ContentType.Equals(ContentTypeEnumeration.Document.MSOffice.PPSX))
        //            return File("~/Images/ppoint.png", ContentTypeEnumeration.Image.PNG);
        //    if (obj.ContentType.Equals(ContentTypeEnumeration.Document.PDF))
        //            return File("~/Images/pdf.png", ContentTypeEnumeration.Image.PNG);
        //    return File("~/Images/text.png", ContentTypeEnumeration.Image.PNG);

        //}

        //[HttpGet]
        //public String GetContent(String key)
        //{
        //    CacheImage obj = (CacheImage)System.Web.HttpContext.Current.Cache[key];
        //    //List<TextContent> list = new List<TextContent>();
        //    //list.Add(new TextContent { Content = obj.FileBinaries.ToString(), FileType = ContentTypeEnumeration.PlainText.TEXT_TYPE});
        //    int i = 0;
        //    string rs = "";
        //    while (i < obj.FileBinaries.Length)
        //    {
        //        rs += obj.FileBinaries[i].ToString();
        //    }
        //    return rs;
        //}

        //[HttpPost]
        //public JsonResult UpLoadFile(HttpPostedFileBase fileUpload, bool isFromCamera = false)
        //{
        //    try
        //    {
        //        byte[] fileBinariesPosted;
        //        string contentType;
        //        string name;
        //        if (isFromCamera)
        //        {
        //            var BASE64_PNG_HEADER = "data:image/png;base64,";
        //            var base64 = Request.Params.Get("fileUpload").Substring(BASE64_PNG_HEADER.Length);
        //            fileBinariesPosted = Convert.FromBase64String(base64);
        //            contentType = ContentTypeEnumeration.Image.PNG;
        //            name = "Camera.png";
        //        }
        //        else
        //        {
        //            // Create a byte array of file stream length
        //            fileBinariesPosted = new byte[fileUpload.InputStream.Length];
        //            //Read block of bytes from stream into the byte array
        //            fileUpload.InputStream.Read(fileBinariesPosted, 0,
        //                System.Convert.ToInt32(fileUpload.InputStream.Length));
        //            contentType = fileUpload.ContentType;
        //            name = System.IO.Path.GetFileName(fileUpload.FileName);
        //        }
        //        List<CacheFileResult> listCacheFileResult =
        //                                        CacheHelper.CacheFile(contentType, 
        //                                                              fileBinariesPosted, 
        //                                                              name, 
        //                                                              Server.MapPath("~/Temp"));
        //        return Json(listCacheFileResult);
        //    }
        //    catch (Exception ex)
        //    {
        //        log.Error(ex.Message, ex);
        //        return Json(ex.Message); 
        //    }
            
        //}
    }
}