using ArchiveMVC5.Models;
using ArchiveMVC5.Models.DataProvider;
using ArchiveMVC5.Utility;
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

namespace ArchiveMVC5.Controllers
{
    public class ImageProcessingController : Controller
    {
        private static readonly ILog log =
            LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        //
        // GET: /ImageProcessing/
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
        public FileResult ID(String key , bool thumb = false)
        {
            
            CacheObject obj=null;
            try{
                obj = (CacheImage)System.Web.HttpContext.Current.Cache[key];
            }catch{
                //Nothing
            }

            if(obj==null)                
                obj = (CacheTemporaryFile)System.Web.HttpContext.Current.Cache[key];
            if (obj == null)
                return File("~/Images/text.png", ContentTypeEnumeration.Image.PNG);
            if (obj.ContentType == ContentTypeEnumeration.Image.TIFF
                //|| ContentTypeEnumeration.Image.TIFF.ToString().Contains(obj.ContentType)
                || obj.ContentType.StartsWith(ContentTypeEnumeration.Image.IMAGE_TYPE))
            {
                Image img = ProcessImages.ByteArrayToImage(ProcessImages.TiffToJpeg(((CacheImage)obj).FileBinaries));
                if (thumb == true)
                    img = img.GetThumbnailImage(76, 76, null, new IntPtr());
                //img = ProcessImages.Rotate(img, rote);
                //if (img.Width > Constant.LIMIT_WIDTH_OF_PAGE_IMAGE)
                //{
                //    img = ProcessImages.ResizeImage(img, Constant.LIMIT_WIDTH_OF_PAGE_IMAGE);
                //}
                byte[] arr = ProcessImages.ImageToByteArray(img,System.Drawing.Imaging.ImageFormat.Jpeg);
                
                return File(arr, ContentTypeEnumeration.Image.JPEG);
            }
            if (obj.ContentType.Equals(ContentTypeEnumeration.Document.MSOffice.DOC) ||
                obj.ContentType.Equals(ContentTypeEnumeration.Document.MSOffice.DOCX) ||
                obj.ContentType.Equals(ContentTypeEnumeration.Document.MSOffice.DOTX))
                return File("~/Images/word.png", ContentTypeEnumeration.Image.PNG);
            if (obj.ContentType.Equals(ContentTypeEnumeration.Document.MSOffice.XLS) ||
                obj.ContentType.Equals(ContentTypeEnumeration.Document.MSOffice.XLSX) ||
                obj.ContentType.Equals(ContentTypeEnumeration.Document.MSOffice.XLTX))
                    return File("~/Images/excel.png", ContentTypeEnumeration.Image.PNG);
            if (obj.ContentType.Equals(ContentTypeEnumeration.Document.MSOffice.PPT) ||
                obj.ContentType.Equals(ContentTypeEnumeration.Document.MSOffice.PPTX) ||
                obj.ContentType.Equals(ContentTypeEnumeration.Document.MSOffice.PPSX))
                    return File("~/Images/ppoint.png", ContentTypeEnumeration.Image.PNG);
            if (obj.ContentType.Equals(ContentTypeEnumeration.Document.PDF))
                    return File("~/Images/pdf.png", ContentTypeEnumeration.Image.PNG);
            return File("~/Images/text.png", ContentTypeEnumeration.Image.PNG);

        }

        [HttpGet]
        public String GetContent(String key)
        {
            CacheImage obj = (CacheImage)System.Web.HttpContext.Current.Cache[key];
            //List<TextContent> list = new List<TextContent>();
            //list.Add(new TextContent { Content = obj.FileBinaries.ToString(), FileType = ContentTypeEnumeration.PlainText.TEXT_TYPE});
            int i = 0;
            string rs = "";
            while (i < obj.FileBinaries.Length)
            {
                rs += obj.FileBinaries[i].ToString();
            }
            return rs;
        }

        [HttpPost]
        public JsonResult UpLoadFile(HttpPostedFileBase[] fileUploads, bool isFromCamera = false)
        {
            try
            {
                List<CacheFileResult> listCacheFileResults = new List<CacheFileResult>();
                foreach (HttpPostedFileBase fileUpload in fileUploads)
                {
                    byte[] fileBinariesPosted;
                    string contentType;
                    string name;
                    if (isFromCamera)
                    {
                        var BASE64_PNG_HEADER = "data:image/png;base64,";
                        var base64 = Request.Params.Get("fileUpload").Substring(BASE64_PNG_HEADER.Length);
                        fileBinariesPosted = Convert.FromBase64String(base64);
                        contentType = ContentTypeEnumeration.Image.PNG;
                        name = "Camera.png";
                    }
                    else
                    {
                        // Create a byte array of file stream length
                        fileBinariesPosted = new byte[fileUpload.InputStream.Length];
                        //Read block of bytes from stream into the byte array
                        fileUpload.InputStream.Read(fileBinariesPosted, 0,
                            System.Convert.ToInt32(fileUpload.InputStream.Length));
                        contentType = fileUpload.ContentType;
                        name = System.IO.Path.GetFileName(fileUpload.FileName);
                    }
                    List<CacheFileResult> listCacheFileResult = CacheHelper.CacheFile(contentType, fileBinariesPosted, name, Server.MapPath("~/Temp"));
                    listCacheFileResults.AddRange(listCacheFileResult);
                }

                return Json(listCacheFileResults);
            }
            catch (Exception ex)
            {
                log.Error(ex.Message, ex);
                return Json(ex.Message); 
            }

        }

        [HttpPost]
        public JsonResult UpLoadFileSingle(HttpPostedFileBase fileUpload, bool isFromCamera = false)
        {
            try
            {
                byte[] fileBinariesPosted;
                string contentType;
                string name;
                if (isFromCamera)
                {
                    var BASE64_PNG_HEADER = "data:image/png;base64,";
                    var base64 = Request.Params.Get("fileUpload").Substring(BASE64_PNG_HEADER.Length);
                    fileBinariesPosted = Convert.FromBase64String(base64);
                    contentType = ContentTypeEnumeration.Image.PNG;
                    name = "Camera.png";
                }
                else
                {
                    // Create a byte array of file stream length
                    fileBinariesPosted = new byte[fileUpload.InputStream.Length];
                    //Read block of bytes from stream into the byte array
                    fileUpload.InputStream.Read(fileBinariesPosted, 0,
                        System.Convert.ToInt32(fileUpload.InputStream.Length));
                    contentType = fileUpload.ContentType;
                    name = System.IO.Path.GetFileName(fileUpload.FileName);
                }
                List<CacheFileResult> listCacheFileResult = CacheHelper.CacheFile(contentType, fileBinariesPosted, name, Server.MapPath("~/Temp"));
                return Json(listCacheFileResult);
            }
            catch (Exception ex)
            {
                log.Error(ex.Message, ex);
                return Json(ex.Message);
            }

        }

        [HttpGet]
        public FileResult GetIcon(String key)
        {
            byte[] obj = (byte[])System.Web.HttpContext.Current.Cache[key];
            return File(obj, ProcessImages.GetImageMimeType(obj));
            //return File("~/Images/appbar.page.check.png", ContentTypeEnumeration.Image.PNG);
        }

        [HttpPost]
        public JsonResult GetCroppedImage(string[] para)
        {
            string src = para[0].ToString();
            int top = Convert.ToInt32(para[1]);
            int left = Convert.ToInt32(para[2]);
            int width = Convert.ToInt32(para[3]);
            int height = Convert.ToInt32(para[4]);
            string fieldId = para[5];
            string snippedDir = Server.MapPath("~/Temp/Snipped");

            if (!Directory.Exists(snippedDir))
            {
                Directory.CreateDirectory(snippedDir);
            }

            string croppedSrc = Server.MapPath(string.Format("~/Temp/Snipped/{0}_{1}.bmp", src, fieldId));

            if (!System.IO.File.Exists(croppedSrc))
            {

                CacheObject obj = null;

                try
                {
                    obj = (CacheImage)System.Web.HttpContext.Current.Cache[src];
                }
                catch
                {
                    //Nothing
                }

                Bitmap croppedImage = ProcessImages.GetCroppedBitmap(((CacheImage)obj).FileBinaries, top, left, width, height);
                //croppedImage.Save(croppedSrc);
                byte[] fileBinary = new byte[0];

                croppedImage.Save(croppedSrc, System.Drawing.Imaging.ImageFormat.Bmp);
                fileBinary = System.IO.File.ReadAllBytes(croppedSrc);
            }

            FileInfo fileInfo = new FileInfo(croppedSrc);
            croppedSrc = @"\Temp\Snipped\" + fileInfo.Name;
            return Json(croppedSrc);
        }

        [HttpGet]
        public FileResult GetLoginAccountPicture()
        {
            UserModel user = Utilities.LoginUser;

            if (user == null || user.Picture == null)
            {
                return File(@"~/Images/archive_admin_icon/appbar.user.minus.svg", ContentTypeEnumeration.Image.SVG);
            }

            string userDir = Server.MapPath("~/Temp/users");

            if (!Directory.Exists(userDir))
            {
                Directory.CreateDirectory(userDir);
            }

            string imageSrc = Server.MapPath(string.Format("~/Temp/users/{0}.bmp", user.Username));

            if (!System.IO.File.Exists(imageSrc))
            {

                Bitmap userImage = ProcessImages.GetBitmap(user.Picture);
                //byte[] fileBinary = new byte[0];

                userImage.Save(imageSrc, System.Drawing.Imaging.ImageFormat.Jpeg);
                //fileBinary = System.IO.File.ReadAllBytes(imageSrc);
            }

            FileInfo fileInfo = new FileInfo(imageSrc);
            imageSrc = @"~/Temp/users/" + fileInfo.Name;
            return File(imageSrc, ContentTypeEnumeration.Image.JPEG);
        }
    }
    
    //public enum Type{
    //    TTF=0,
    //    JPG=1
    //}
}