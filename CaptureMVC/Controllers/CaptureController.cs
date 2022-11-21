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
    public class CaptureController : BaseController
    {
        [HttpGet]
        public ActionResult Index()
        {
            var captureBatches = Session[Constant.SESSION_CAPTURE_BATCHES] as List<CaptureBatchModel>;
            return View(captureBatches);
        }

        [ChildActionOnly]
        [HttpGet]
        public PartialViewResult _CreateBatchDialog()
        {
            if (Session[Constant.SESSION_CAPTURE_BATCH_TYPES] == null)
            {
                var provider = new BatchTypeProvider();
                var models = provider.GetCaptureBatchTypes();

                Session[Constant.SESSION_CAPTURE_BATCH_TYPES] = models;
            }

            return PartialView(Session[Constant.SESSION_CAPTURE_BATCH_TYPES]);
        }

        [HttpPost]
        public JsonResult _GetCapturedBatchTypeInfo(Guid id)
        {
            var capturedBatchTypes = Session[Constant.SESSION_CAPTURE_BATCH_TYPES] as List<BatchType>;
            if (capturedBatchTypes.Count == 0)
            {
                return null;
            }

            var capturedBatchType = capturedBatchTypes.FirstOrDefault(h => h.Id == id);
            if (capturedBatchType == null)
            {
                return null;
            }

            // Get list OCR of doc
            Dictionary<Guid, List<OCRFieldModel>> orcDocs = new Dictionary<Guid, List<OCRFieldModel>>();
            foreach (var docType in capturedBatchType.DocTypes)
            {
                orcDocs.Add(docType.Id, this.GetOcrForField(docType.OCRTemplate));
            }

            // Get List Id and Name of document type
            var docTypes = capturedBatchType.DocTypes.OrderBy(h => h.Name).Select(h => new
            {
                Id = h.Id,
                Name = h.Name,
                Icon = ImageToBase64(h.Icon),
                Fields = h.Fields.Where(g => !g.IsSystemField).OrderBy(g => g.DisplayOrder).Select(g => new
                {
                    Id = g.Id,
                    Name = g.Name,
                    DataType = g.DataType,
                    DefaultValue = g.DefaultValue,
                    MaxLength = g.MaxLength,
                    UseCurrentDate = g.UseCurrentDate,
                    IsRequired = g.IsRequired,
                    IsLookup = g.IsLookup,
                    LookupInfoXml = g.IsLookup ? g.LookupInfoXml : string.Empty,
                    ValidationPattern = g.ValidationPattern,
                    Picklists = g.Picklists.Select(n => new
                    {
                        Id = n.Id,
                        Value = n.Value
                    }).ToList(),
                    Children = g.Children.OrderBy(n => n.DisplayOrder).Select(n => new
                    {
                        Id = n.Id,
                        Name = n.Name,
                        DataType = n.DataType,
                        DefaultValue = n.DefaultValue,
                        MaxLength = n.MaxLength,
                        UseCurrentDate = n.UseCurrentDate,
                    }).ToList(),
                    Ocr = orcDocs[h.Id].FirstOrDefault(n => n.FieldId == g.Id)
                })
            }).ToList();

            // Get batch field
            var fields = capturedBatchType.Fields.Where(h => !h.IsSystemField)
                                                 .OrderBy(h => h.DisplayOrder).Select(h => new
                                                 {
                                                     Id = h.Id,
                                                     Name = h.Name,
                                                     DataType = h.DataType,
                                                     DefaultValue = h.DefaultValue,
                                                     MaxLength = h.MaxLength,
                                                     UseCurrentDate = h.UseCurrentDate,
                                                     IsLookup = h.IsLookup
                                                 }).ToList();

            var result = new
            {
                Name = capturedBatchType.Name,
                DocTypes = docTypes,
                Fields = fields
            };
            return Json(result);
        }

        private string ImageToBase64(byte[] data)
        {
            #region Generate thumbnail
            try
            {
                using (MemoryStream ms = new MemoryStream(data))
                {
                    var image = Image.FromStream(ms);

                    // Figure out the ratio
                    double ratioX = (double)24 / (double)image.Width;
                    double ratioY = (double)24 / (double)image.Height;
                    // use whichever multiplier is smaller
                    double ratio = ratioX < ratioY ? ratioX : ratioY;

                    // now we can get the new height and width
                    int newHeight = Convert.ToInt32(image.Height * ratio);
                    int newWidth = Convert.ToInt32(image.Width * ratio);

                    // A holder for the thumbnail
                    var result = new Bitmap(image, newWidth, newHeight);
                    using (var jpgStream = new MemoryStream())
                    {
                        result.Save(jpgStream, ImageFormat.Png);
                        var photoBase64 = Convert.ToBase64String(jpgStream.ToArray());
                        return "data:image/png;base64," + photoBase64;
                    }
                }
            }
            catch
            {
                return null;
            }
            #endregion
        }

        private List<OCRFieldModel> GetOcrForField(OCRTemplate ocrTemplate)
        {
            var ocrFields = new List<OCRFieldModel>();

            if (ocrTemplate != null)
            {
                foreach (var ocrTemplatePage in ocrTemplate.OCRTemplatePages)
                {
                    ocrFields.AddRange(ocrTemplatePage.OCRTemplateZones.Select(h => new OCRFieldModel()
                    {
                        FieldId = h.FieldMetaDataId,
                        PageIndex = ocrTemplatePage.PageIndex,
                        Left = (int)Math.Round(h.Left),
                        Top = (int)Math.Round(h.Top),
                        Width = (int)Math.Round(h.Width),
                        Height = (int)Math.Round(h.Height)
                    }));
                }
            }

            return ocrFields;
        }

        private JsonResult GetDefaultJsonError()
        {
            return Json(new { HaveError = true });
        }

        [HttpPost]
        public PartialViewResult CreateBatch(string batchName, Guid batchTypeId)
        {
            // Get list batch type
            var batchTypes = Session[Constant.SESSION_CAPTURE_BATCH_TYPES] as List<BatchType>;
            if (batchTypes == null)
            {
                return null;
            }

            // Get batch type
            var batchType = batchTypes.FirstOrDefault(h => h.Id == batchTypeId);
            if (batchType == null)
            {
                return null;
            }

            // Create new empty batch
            var batch = new CaptureBatchModel();
            batch.Id = Guid.NewGuid();
            batch.Name = string.IsNullOrWhiteSpace(batchName) ? string.Empty : batchName.Trim();
            batch.TypeId = batchTypeId;
            batch.TypeName = batchType.Name;
            batch.CreateDate = DateTime.Now;
            batch.CreateBy = Utilities.UserName;

            // Create loose doc of batch
            var looseDoc = new CaptureDocumentModel();
            looseDoc.Id = Guid.NewGuid();
            looseDoc.IsLooseItem = true;
            batch.Documents.Add(looseDoc);

            // Get list capture batch
            var batches = Session[Constant.SESSION_CAPTURE_BATCHES] as List<CaptureBatchModel>;
            if (batches == null)
            {
                return null;
            }
            batches.Add(batch);

            return PartialView("_ThumbnailBatch", batch);
        }

        #region Insert new page

        [HttpPost]
        public JsonResult _Upload(CaptureInsertModel model)
        {
            try
            {
                if (Request.Files == null || Request.Files.Count == 0)
                {
                    return null;
                }

                var sessionFolder = Session[Constant.SESSION_FOLDER] as string;
                var sessionId = (Guid)Session[Constant.SESSION_GUID];
                var listFileName = new List<string>();
                var dicPage = new Dictionary<string, CapturePageModel>();

                for (int i = 0; i < Request.Files.Count; i++)
                {
                    var file = Request.Files[i];

                    // Get file binary
                    var fileBinary = new byte[file.InputStream.Length];
                    file.InputStream.Read(fileBinary, 0, fileBinary.Length);

                    if (ContentTypeEnumeration.Image.TIFF.Equals(file.ContentType, StringComparison.OrdinalIgnoreCase))
                    {
                        #region Image file type tiff

                        // Get list binary
                        var tiffBinaries = ImageProcessing.SplitTiff(fileBinary);
                        Image image;
                        CapturePageModel page;

                        foreach (var tiff in tiffBinaries)
                        {
                            #region
                            page = new CapturePageModel();

                            // Create new id
                            page.Id = Guid.NewGuid();
                            page.OriginalFileName = file.FileName;
                            page.FileExtension = "tiff";
                            page.IsImage = true;

                            // Try to get image from file
                            using (var ms = new MemoryStream(tiff))
                            {
                                #region

                                image = Image.FromStream(ms);

                                // Set dpi from origin image
                                page.Dpi = (int)Math.Round(image.HorizontalResolution);

                                #region Set max default view width and height

                                var minSize = Math.Min(image.Width, image.Height);
                                if (minSize > Constant.MAX_OF_MIN_SIZE)
                                {
                                    if (image.Width < image.Height)
                                    {
                                        page.Width = Constant.MAX_OF_MIN_SIZE;
                                        page.Height
                                            = Math.Round((double)(Constant.MAX_OF_MIN_SIZE * image.Height / image.Width));
                                    }
                                    else
                                    {
                                        page.Height = Constant.MAX_OF_MIN_SIZE;
                                        page.Width
                                            = Math.Round((double)(Constant.MAX_OF_MIN_SIZE * image.Width / image.Height));
                                    }
                                }
                                else
                                {
                                    page.Width = image.Width;
                                    page.Height = image.Height;
                                }

                                #endregion

                                // Show file always in PNG format
                                page.ThumbFilePath = string.Format("/CaptureFolder/{0}/thumb.{1}.png", sessionId, page.Id);
                                page.ShowFilePath = string.Format("/CaptureFolder/{0}/show.{1}.png", sessionId, page.Id);

                                // A holder for the thumbnail
                                var thumb = new Bitmap(image, Constant.MAX_SIZE_THUMBNAIL,
                                                              Constant.MAX_SIZE_THUMBNAIL);

                                using (var pngStream = new MemoryStream())
                                {
                                    thumb.Save(Server.MapPath("~" + page.ThumbFilePath));
                                    thumb.Dispose();
                                }

                                image.Save(Server.MapPath("~" + page.ShowFilePath));

                                // Save origin file
                                page.OriginFilePath = Path.Combine(sessionFolder,
                                                                   "origin."
                                                                   + page.Id.ToString()
                                                                   + ".tiff");
                                image.Save(page.OriginFilePath);
                                image.Dispose();
                                #endregion
                            }

                            dicPage.Add(page.OriginFilePath, page);
                            listFileName.Add(page.OriginFilePath);
                            #endregion
                        }
                        #endregion
                    }
                    else
                    {
                        #region File type is difference to Image file tiff

                        var page = new CapturePageModel();

                        // Create new id
                        page.Id = Guid.NewGuid();
                        page.OriginalFileName = file.FileName;

                        // Get file extension
                        var indexStartExtention = file.FileName.LastIndexOf(".") + 1;
                        if (indexStartExtention > 0 && indexStartExtention < file.FileName.Length)
                        {
                            page.FileExtension = file.FileName.Substring(indexStartExtention);
                        }

                        // Try to get image from file
                        using (var ms = new MemoryStream(fileBinary))
                        {
                            #region
                            Image image = null;

                            try
                            {
                                image = Image.FromStream(ms);
                                page.IsImage = true;
                            }
                            catch { }

                            if (page.IsImage)
                            {
                                #region
                                // Set dpi from origin image
                                page.Dpi = (int)Math.Round(image.HorizontalResolution);

                                #region Set max default view width and height

                                var minSize = Math.Min(image.Width, image.Height);
                                if (minSize > Constant.MAX_OF_MIN_SIZE)
                                {
                                    if (image.Width < image.Height)
                                    {
                                        page.Width = Constant.MAX_OF_MIN_SIZE;
                                        page.Height
                                            = Math.Round((double)(Constant.MAX_OF_MIN_SIZE * image.Height / image.Width));
                                    }
                                    else
                                    {
                                        page.Height = Constant.MAX_OF_MIN_SIZE;
                                        page.Width
                                            = Math.Round((double)(Constant.MAX_OF_MIN_SIZE * image.Width / image.Height));
                                    }
                                }
                                else
                                {
                                    page.Width = image.Width;
                                    page.Height = image.Height;
                                }

                                #endregion

                                // Show file always in PNG format
                                page.ThumbFilePath = string.Format("/CaptureFolder/{0}/thumb.{1}.png", sessionId, page.Id);
                                page.ShowFilePath = string.Format("/CaptureFolder/{0}/show.{1}.png", sessionId, page.Id);

                                // A holder for the thumbnail
                                var thumb = new Bitmap(image, Constant.MAX_SIZE_THUMBNAIL,
                                                              Constant.MAX_SIZE_THUMBNAIL);

                                using (var pngStream = new MemoryStream())
                                {
                                    thumb.Save(Server.MapPath("~" + page.ThumbFilePath));
                                    thumb.Dispose();
                                }

                                image.Save(Server.MapPath("~" + page.ShowFilePath));
                                #endregion
                            }
                            else
                            {
                                var isSupportPreview = false;
                                page.ShowFilePath = GetShowImagePath(page.FileExtension, out isSupportPreview);
                                page.IsSupportPreview = isSupportPreview;
                                page.ThumbFilePath = page.ShowFilePath;
                            }
                            #endregion
                        }

                        // Save orgin file
                        page.OriginFilePath = Path.Combine(sessionFolder,
                                                           "origin."
                                                           + page.Id.ToString()
                                                           + "."
                                                           + page.FileExtension);
                        file.SaveAs(page.OriginFilePath);

                        dicPage.Add(page.OriginFilePath, page);
                        listFileName.Add(page.OriginFilePath);
                        #endregion
                    }
                }

                var capturedBatchTypes = Session[Constant.SESSION_CAPTURE_BATCH_TYPES] as List<BatchType>;
                var capturedBatchType = capturedBatchTypes.FirstOrDefault(h => h.Id == model.BatchTypeId);
                var captureBatches = Session[Constant.SESSION_CAPTURE_BATCHES] as List<CaptureBatchModel>;
                var captureBatch = captureBatches.FirstOrDefault(h => h.Id == model.BatchId);

                #region Do barcode
                // Do barcode
                if (model.DoBarcode
                    && Utilities.Settings.EnabledBarcodeClient
                    && capturedBatchType != null
                    && capturedBatchType.BarcodeConfiguration != null)
                {
                    #region
                    var workingFolder = Server.MapPath(Utilities.GetFolderTempFiles());
                    var barcodeHelper = new BarcodeHelper(capturedBatchType,
                                                          capturedBatchType.BarcodeConfiguration,
                                                          workingFolder,
                                                          true);
                    Batch tempBatch = null;

                    // Case import classify later
                    if (model.DocTypeId == Guid.Empty)
                    {
                        var loosePages = new List<string>();
                        tempBatch = barcodeHelper.Process(listFileName, out loosePages);

                        foreach (var loosePage in loosePages)
                        {
                            captureBatch.Documents[0].Pages.Add(dicPage[loosePage]);
                        }

                        if (tempBatch != null)
                        {
                            #region Add doc
                            if (tempBatch.Documents != null)
                            {
                                CaptureDocumentModel docModel;

                                foreach (var doc in tempBatch.Documents)
                                {
                                    docModel = new CaptureDocumentModel();

                                    docModel.Id = Guid.NewGuid();
                                    docModel.Name = string.Empty;
                                    docModel.TypeId = doc.DocTypeId;
                                    docModel.TypeName = doc.DocumentType.Name;

                                    if (doc.Pages != null)
                                    {
                                        foreach (var page in doc.Pages)
                                        {
                                            docModel.Pages.Add(dicPage[page.FilePath]);
                                        }
                                    }

                                    captureBatch.Documents.Add(docModel);
                                }
                            }
                            #endregion
                        }
                    }
                    #endregion
                }
                #endregion

                #region Do OCR

                //var ocrData = new Dictionary<Guid, string>();
                //var ocrImageIds = new Dictionary<Guid, Guid>();

                //// Do Ocr
                //if (model.DoOcr && Utilities.Settings.EnabledOCRClient)
                //{
                //    #region
                //    // Generate thumbnail image for get DPI in step do OCR
                //    foreach (var pageModel in results)
                //    {
                //        if (pageModel.ContentType.StartsWith(ContentTypeEnumeration.Image.IMAGE_TYPE))
                //        {
                //            this.GetThumbnailRealImage(pageModel.OriginPage.Id);
                //        }
                //    }

                //    if (capturedBatchType != null)
                //    {
                //        var docType = capturedBatchType.DocTypes.FirstOrDefault(h => h.Id == model.DocTypeId);
                //        if (docType != null)
                //        {
                //            var ocrHelper = new OCRHelper("tessdata");
                //            var ocrImages = ocrHelper.GetCroppedBitmaps(docType, results);

                //            if (ocrImages.Count > 0)
                //            {
                //                #region Store OCR images in session
                //                var sessionOcrImages = Session[Constant.SESSION_CAPTURE_OCR_IMAGES]
                //                                        as Dictionary<Guid, Bitmap>;

                //                foreach (var ocrImage in ocrImages)
                //                {
                //                    Guid ocrImageId;
                //                    do
                //                    {
                //                        ocrImageId = Guid.NewGuid();

                //                    } while (sessionOcrImages.ContainsKey(ocrImageId));

                //                    sessionOcrImages.Add(ocrImageId, ocrImage.Value);
                //                    ocrImageIds.Add(ocrImage.Key, ocrImageId);
                //                }

                //                #endregion

                //                #region Ambiguous definition
                //                var ambiguousDefinitionsSession =
                //                    Session[Constant.SESSION_AMBIGUOUS_DEFINITION]
                //                        as Dictionary<Guid, List<CaptureAmbiguousDefinitionModel>>;

                //                var languageId = this.GetLanguageId(model.LanguageName);
                //                if (languageId != Guid.Empty || !ambiguousDefinitionsSession.ContainsKey(languageId))
                //                {
                //                    // Get list ambigous definition from DB
                //                    var ambiguousDefinitionProvider = new AmbiguousDefinitionProvider();
                //                    var ambiguousDefinitions =
                //                        ambiguousDefinitionProvider.GetAmbiguousDefinition(languageId);

                //                    // Map to get list dictionary
                //                    var ambiguousDefinitionModes =
                //                        ObjectMapper.GetAmbiguousDefinitionModels(ambiguousDefinitions);

                //                    ambiguousDefinitionsSession.Add(languageId, ambiguousDefinitionModes);
                //                }
                //                #endregion

                //                // Do OCR to get value
                //                ocrData = ocrHelper.DoOcr(docType,
                //                                          ocrImages,
                //                                          model.LanguageName,
                //                                          ambiguousDefinitionsSession[languageId]);
                //            }
                //        }
                //    }
                //    #endregion
                //}

                #endregion

                var jsonResult = new
                {
                    //Pages = results.Select(h => new
                    //{
                    //    Id = h.OriginPage.Id,
                    //    Class = h.ContentType.StartsWith(ContentTypeEnumeration.Image.IMAGE_TYPE) ? "real-image"
                    //                                                                              : "native-image"
                    //}).ToList(),
                    //OcrData = ocrData.Select(h => new { FieldId = h.Key, Value = h.Value }).ToList(),
                    //OcrImageIds = ocrImageIds.Select(h => new { FieldId = h.Key, OcrId = h.Value }).ToList()
                };

                return Json(jsonResult);
            }
            catch (Exception ex)
            {
                base.ProcessError(ex);
                return null;
            }
        }

        private string GetShowImagePath(string extension, out bool isSupportPreview)
        {
            isSupportPreview = true;
            var tempExt = string.Format("{0}", extension).Trim().ToLower();

            switch (tempExt)
            {
                case "doc":
                case "docx":
                case "odt":
                    return "/Images/Thumbnail/word.png";

                case "xls":
                case "xlsx":
                case "ods":
                    return "/Images/Thumbnail/excel.png";

                case "ppt":
                case "pptx":
                case "odp":
                    return "/Images/Thumbnail/ppoint.png";

                case "pdf":
                    return "/Images/Thumbnail/pdf.png";

                case "txt":
                    return "/Images/Thumbnail/text.png";

                default:
                    isSupportPreview = false;
                    return "/Images/Thumbnail/unknown.png";
            }
        }

        private Guid GetLanguageId(string languageName)
        {
            return Guid.Empty;
        }

        #endregion

        #region Get image and its information

        [HttpGet]
        public ActionResult GetThumbnailRealImage(Guid id)
        {
            try
            {
                // Get captured pages
                var caturedPages = Session[Constant.SESSION_CAPTURE_PAGES] as List<ViewPageModel>;
                var page = caturedPages.FirstOrDefault(h => h.OriginPage.Id == id);

                if (page == null)
                {
                    return null;
                }

                if (page.Thumbnail == null)
                {
                    using (MemoryStream ms = new MemoryStream(page.OriginPage.FileBinary))
                    {
                        var image = Image.FromStream(ms);

                        // Set dpi from origin image
                        page.Dpi = (int)Math.Round(image.HorizontalResolution);

                        #region Set max default view width and height

                        var minSize = Math.Min(image.Width, image.Height);
                        if (minSize > Constant.MAX_OF_MIN_SIZE)
                        {
                            if (image.Width < image.Height)
                            {
                                page.OriginPage.Width = Constant.MAX_OF_MIN_SIZE;
                                page.OriginPage.Height
                                    = Math.Round((double)(Constant.MAX_OF_MIN_SIZE * image.Height / image.Width));
                            }
                            else
                            {
                                page.OriginPage.Height = Constant.MAX_OF_MIN_SIZE;
                                page.OriginPage.Width
                                    = Math.Round((double)(Constant.MAX_OF_MIN_SIZE * image.Width / image.Height));
                            }
                        }
                        else
                        {
                            page.OriginPage.Width = image.Width;
                            page.OriginPage.Height = image.Height;
                        }

                        #endregion

                        //// Figure out the ratio
                        //double ratioX = (double)Constant.MAX_SIZE_THUMBNAIL / (double)image.Width;
                        //double ratioY = (double)Constant.MAX_SIZE_THUMBNAIL / (double)image.Height;
                        //// use whichever multiplier is smaller
                        //double ratio = ratioX < ratioY ? ratioX : ratioY;

                        //// now we can get the new height and width
                        //int newHeight = Convert.ToInt32(image.Height * ratio);
                        //int newWidth = Convert.ToInt32(image.Width * ratio);

                        // A holder for the thumbnail
                        var result = new Bitmap(image, Constant.MAX_SIZE_THUMBNAIL, Constant.MAX_SIZE_THUMBNAIL);

                        using (var pngStream = new MemoryStream())
                        {
                            result.Save(pngStream, ImageFormat.Png);
                            page.Thumbnail = pngStream.ToArray();
                        }
                    }
                }

                return File(page.Thumbnail, "image/png");
            }
            catch (Exception ex)
            {
                base.ProcessError(ex);
                return null;
            }
        }

        [HttpPost]
        public JsonResult GetThumbnailRealImageInfo(Guid id)
        {
            try
            {
                // Get captured pages
                var caturedPages = Session[Constant.SESSION_CAPTURE_PAGES] as List<ViewPageModel>;
                var page = caturedPages.FirstOrDefault(h => h.OriginPage.Id == id);

                if (page == null)
                {
                    return null;
                }

                return Json(new
                {
                    dpi = page.Dpi,
                    height = (int)Math.Round(page.OriginPage.Height),
                    width = (int)Math.Round(page.OriginPage.Width)
                });
            }
            catch (Exception ex)
            {
                base.ProcessError(ex);
                return null;
            }
        }

        [HttpGet]
        public ActionResult GetRealImage(Guid id)
        {
            try
            {
                // Get captured pages
                var caturedPages = Session[Constant.SESSION_CAPTURE_PAGES] as List<ViewPageModel>;
                var page = caturedPages.FirstOrDefault(h => h.OriginPage.Id == id);

                if (page == null)
                {
                    return null;
                }

                if (page.Image == null)
                {
                    using (MemoryStream ms = new MemoryStream(page.OriginPage.FileBinary))
                    {
                        var image = Image.FromStream(ms);


                        // A holder for the thumbnail
                        // A holder for the thumbnail
                        Bitmap result = new Bitmap(image,
                                                   (int)Math.Round(page.OriginPage.Width),
                                                   (int)Math.Round(page.OriginPage.Height));

                        using (MemoryStream pngStream = new MemoryStream())
                        {
                            result.Save(pngStream, ImageFormat.Png);
                            page.Image = pngStream.ToArray();
                        }
                    }
                }

                return File(page.Image, "image/png");
            }
            catch (Exception ex)
            {
                base.ProcessError(ex);
                return null;
            }
        }

        [HttpPost]
        public JsonResult GetNativeImage(Guid id)
        {
            try
            {
                // Get captured pages
                var caturedPages = Session[Constant.SESSION_CAPTURE_PAGES] as List<ViewPageModel>;
                var page = caturedPages.FirstOrDefault(h => h.OriginPage.Id == id);

                if (page == null)
                {
                    return null;
                }

                var thumbnailPath = "/Images/Thumbnail/other.png";
                var supportPreview = false;

                if (page.ContentType.Equals(ContentTypeEnumeration.PlainText.TEXT_TYPE))
                {
                    thumbnailPath = "/Images/Thumbnail/text.png";
                }
                else if (page.ContentType.Equals(ContentTypeEnumeration.Document.MSOffice.DOC) ||
                         page.ContentType.Equals(ContentTypeEnumeration.Document.MSOffice.DOCX) ||
                         page.ContentType.Equals(ContentTypeEnumeration.Document.OpenOffice.ODT))
                {
                    thumbnailPath = "/Images/Thumbnail/word.png";
                    supportPreview = true;
                }
                else if (page.ContentType.Equals(ContentTypeEnumeration.Document.MSOffice.XLS) ||
                         page.ContentType.Equals(ContentTypeEnumeration.Document.MSOffice.XLSX) ||
                         page.ContentType.Equals(ContentTypeEnumeration.Document.OpenOffice.ODS))
                {
                    thumbnailPath = "/Images/Thumbnail/excel.png";
                    supportPreview = true;

                }
                else if (page.ContentType.Equals(ContentTypeEnumeration.Document.MSOffice.PPT) ||
                         page.ContentType.Equals(ContentTypeEnumeration.Document.MSOffice.PPTX) ||
                         page.ContentType.Equals(ContentTypeEnumeration.Document.OpenOffice.ODP))
                {
                    thumbnailPath = "/Images/Thumbnail/ppoint.png";
                    supportPreview = true;
                }
                else if (page.ContentType.Equals(ContentTypeEnumeration.Document.PDF))
                {
                    thumbnailPath = "/Images/Thumbnail/pdf.png";
                    supportPreview = true;
                }

                return Json(new
                {
                    thumbnailPath = thumbnailPath,
                    supportPreview = supportPreview
                });
            }
            catch (Exception ex)
            {
                base.ProcessError(ex);
                return null;
            }
        }

        [HttpGet]
        public ActionResult GetOcrImage(Guid id)
        {
            try
            {
                var sessionOcrImages = Session[Constant.SESSION_CAPTURE_OCR_IMAGES]
                                        as Dictionary<Guid, Bitmap>;

                if (!sessionOcrImages.ContainsKey(id))
                {
                    return null;
                }

                var bitmap = sessionOcrImages[id];
                var pngStream = new MemoryStream();
                bitmap.Save(pngStream, ImageFormat.Png);

                return File(pngStream.ToArray(), "image/png");
            }
            catch (Exception ex)
            {
                base.ProcessError(ex);
                return null;
            }
        }

        #endregion

        //[HttpGet]
        //public PartialViewResult _OpenBatch(Guid batchId)
        //{
        //    try
        //    {
        //        // Get the list opened batch in session
        //        var openedBatches = Session[Constant.SESSION_OPENED_BATCHES] as OrderedDictionary;
        //        // Check list opened batch is empty
        //        if (openedBatches.Count == 0)
        //        {
        //            return null;
        //        }

        //        var stringBatchId = batchId.ToString();
        //        // Check the active opened is existed
        //        if (!openedBatches.Contains(stringBatchId))
        //        {
        //            return null;
        //        }

        //        // Get the first opened batch
        //        var openedBatch = openedBatches[stringBatchId] as ViewBatchModel;

        //        // Batch is not loaded information
        //        if (!openedBatch.IsLoaded)
        //        {
        //            var batchProvider = new WorkItemProvider();
        //            // Load information of batch from DB
        //            var batch = batchProvider.GetWorkItem(openedBatch.Id);
        //            // Mapping value for model
        //            openedBatch = ObjectMapper.GetViewBatchModel(batch);

        //            // Update value for batch
        //            openedBatches[stringBatchId] = openedBatch;
        //        }

        //        Session[Constant.SESSION_ACTIVE_OPENED_BATCH_ID] = stringBatchId;
        //        // Clear page in temp doc
        //        openedBatch.Documents[0].Pages.Clear();
        //        return PartialView(openedBatch);
        //    }
        //    catch (Exception ex)
        //    {
        //        base.ProcessError(ex);
        //        return null;
        //    }
        //}

        //#region Image and its information
        //public ActionResult GetThumbnailRealImage(Guid batchId, Guid docId, Guid pageId)
        //{
        //    try
        //    {
        //        // Get page
        //        var openedBatches = Session[Constant.SESSION_OPENED_BATCHES] as OrderedDictionary;
        //        var batch = openedBatches[batchId.ToString()] as ViewBatchModel;
        //        var doc = batch.Documents.Single(h => h.Id == docId);
        //        var page = doc.Pages.Single(h => h.OriginPage.Id == pageId);

        //        if (page.Thumbnail == null)
        //        {
        //            using (MemoryStream ms = new MemoryStream(page.OriginPage.FileBinary))
        //            {
        //                var image = Image.FromStream(ms);

        //                // Set dpi from origin image
        //                page.Dpi = (int)Math.Round(image.HorizontalResolution);

        //                // Case page is have been just added
        //                if (page.IsNew)
        //                {
        //                    var minSize = Math.Min(image.Width, image.Height);
        //                    if (minSize > Constant.MAX_OF_MIN_SIZE)
        //                    {
        //                        if (image.Width < image.Height)
        //                        {
        //                            page.OriginPage.Width = Constant.MAX_OF_MIN_SIZE;
        //                            page.OriginPage.Height = Math.Round((double)(Constant.MAX_OF_MIN_SIZE * image.Height / image.Width));
        //                        }
        //                        else
        //                        {
        //                            page.OriginPage.Height = Constant.MAX_OF_MIN_SIZE;
        //                            page.OriginPage.Width = Math.Round((double)(Constant.MAX_OF_MIN_SIZE * image.Width / image.Height));
        //                        }
        //                    }
        //                    else
        //                    {
        //                        page.OriginPage.Width = image.Width;
        //                        page.OriginPage.Height = image.Height;
        //                    }
        //                }

        //                // A holder for the thumbnail
        //                Bitmap result = new Bitmap(Constant.MAX_SIZE_THUMBNAIL, Constant.MAX_SIZE_THUMBNAIL);

        //                //use a graphics object to draw the resized image into the bitmap 
        //                using (Graphics g = Graphics.FromImage(result))
        //                {
        //                    // Set the resize quality modes to high quality 
        //                    g.CompositingQuality = CompositingQuality.HighSpeed;
        //                    g.InterpolationMode = InterpolationMode.High;
        //                    g.SmoothingMode = SmoothingMode.HighSpeed;

        //                    // Draw the image into the target bitmap 
        //                    g.DrawImage(image, 0, 0, result.Width, result.Height);

        //                    double scaleX = Constant.MAX_SIZE_THUMBNAIL / page.OriginPage.Width;
        //                    double scaleY = Constant.MAX_SIZE_THUMBNAIL / page.OriginPage.Height;

        //                    // Create color background
        //                    Brush brush = new SolidBrush(Utilities.GetRedactionBackground());

        //                    var redactions = page.OriginPage.Annotations.Where(h => h.Type == "Redaction");
        //                    foreach (var redac in page.NotHideRedactions)
        //                    {
        //                        g.FillRectangle(brush,
        //                                        (int)Math.Round((redac.Left * scaleX)),
        //                                        (int)Math.Round((redac.Top * scaleY)),
        //                                        (int)Math.Round((redac.Width * scaleX)),
        //                                        (int)Math.Round((redac.Height * scaleY)));
        //                    }
        //                }

        //                ImageConverter converter = new ImageConverter();
        //                page.Thumbnail = (byte[])converter.ConvertTo(result, typeof(byte[]));
        //            }
        //        }

        //        return File(page.Thumbnail, "image/jpeg");
        //    }
        //    catch (Exception ex)
        //    {
        //        base.ProcessError(ex);
        //        return null;
        //    }
        //}

        //public JsonResult GetThumbnailRealImageInfo(Guid batchId, Guid docId, Guid pageId)
        //{
        //    try
        //    {
        //        // Get page
        //        var openedBatches = Session[Constant.SESSION_OPENED_BATCHES] as OrderedDictionary;
        //        var batch = openedBatches[batchId.ToString()] as ViewBatchModel;
        //        var doc = batch.Documents.Single(h => h.Id == docId);
        //        var page = doc.Pages.Single(h => h.OriginPage.Id == pageId);

        //        return Json(new
        //        {
        //            dpi = page.Dpi,
        //            height = (int)Math.Round(page.OriginPage.Height),
        //            width = (int)Math.Round(page.OriginPage.Width)
        //        }, JsonRequestBehavior.AllowGet);
        //    }
        //    catch (Exception ex)
        //    {
        //        base.ProcessError(ex);
        //        return null;
        //    }
        //}

        //public ActionResult GetRealImage(Guid batchId, Guid docId, Guid pageId)
        //{
        //    try
        //    {
        //        // Get page
        //        var openedBatches = Session[Constant.SESSION_OPENED_BATCHES] as OrderedDictionary;
        //        var batch = openedBatches[batchId.ToString()] as ViewBatchModel;
        //        var doc = batch.Documents.Single(h => h.Id == docId);
        //        var page = doc.Pages.Single(h => h.OriginPage.Id == pageId);

        //        if (page.Image == null)
        //        {
        //            using (MemoryStream ms = new MemoryStream(page.OriginPage.FileBinary))
        //            {
        //                var image = Image.FromStream(ms);

        //                // A holder for the thumbnail
        //                Bitmap result = new Bitmap((int)Math.Round(page.OriginPage.Width),
        //                                           (int)Math.Round(page.OriginPage.Height));

        //                //use a graphics object to draw the resized image into the bitmap 
        //                using (Graphics g = Graphics.FromImage(result))
        //                {
        //                    // Set the resize quality modes to high quality 
        //                    g.CompositingQuality = CompositingQuality.HighQuality;
        //                    g.InterpolationMode = InterpolationMode.HighQualityBicubic;
        //                    g.SmoothingMode = SmoothingMode.HighQuality;

        //                    // Draw the image into the target bitmap 
        //                    g.DrawImage(image, 0, 0, result.Width, result.Height);

        //                    // Create color background
        //                    Brush brush = new SolidBrush(Utilities.GetRedactionBackground());

        //                    var redactions = page.OriginPage.Annotations.Where(h => h.Type == "Redaction");
        //                    foreach (var redac in page.NotHideRedactions)
        //                    {
        //                        g.FillRectangle(brush,
        //                                        (int)Math.Round((redac.Left)),
        //                                        (int)Math.Round((redac.Top)),
        //                                        (int)Math.Round((redac.Width)),
        //                                        (int)Math.Round((redac.Height)));
        //                    }
        //                }

        //                ImageConverter converter = new ImageConverter();
        //                page.Image = (byte[])converter.ConvertTo(result, typeof(byte[]));
        //            }
        //        }

        //        return File(page.Image, "image/jpeg");
        //    }
        //    catch (Exception ex)
        //    {
        //        base.ProcessError(ex);
        //        return null;
        //    }
        //}

        //public JsonResult GetNativeImage(Guid batchId, Guid docId, Guid pageId)
        //{
        //    try
        //    {
        //        // Get page
        //        var openedBatches = Session[Constant.SESSION_OPENED_BATCHES] as OrderedDictionary;
        //        var batch = openedBatches[batchId.ToString()] as ViewBatchModel;
        //        var doc = batch.Documents.Single(h => h.Id == docId);
        //        var page = doc.Pages.Single(h => h.OriginPage.Id == pageId);

        //        var thumbnailPath = "/Images/Thumbnail/other.png";
        //        var supportPreview = false;

        //        if (page.ContentType.Equals(ContentTypeEnumeration.PlainText.TEXT_TYPE))
        //        {
        //            thumbnailPath = "/Images/Thumbnail/text.png";
        //        }
        //        else if (page.ContentType.Equals(ContentTypeEnumeration.Document.MSOffice.DOC) ||
        //                 page.ContentType.Equals(ContentTypeEnumeration.Document.MSOffice.DOCX) ||
        //                 page.ContentType.Equals(ContentTypeEnumeration.Document.OpenOffice.ODT))
        //        {
        //            thumbnailPath = "/Images/Thumbnail/word.png";
        //            supportPreview = true;
        //        }
        //        else if (page.ContentType.Equals(ContentTypeEnumeration.Document.MSOffice.XLS) ||
        //                 page.ContentType.Equals(ContentTypeEnumeration.Document.MSOffice.XLSX) ||
        //                 page.ContentType.Equals(ContentTypeEnumeration.Document.OpenOffice.ODS))
        //        {
        //            thumbnailPath = "/Images/Thumbnail/excel.png";
        //            supportPreview = true;

        //        }
        //        else if (page.ContentType.Equals(ContentTypeEnumeration.Document.MSOffice.PPT) ||
        //                 page.ContentType.Equals(ContentTypeEnumeration.Document.MSOffice.PPTX) ||
        //                 page.ContentType.Equals(ContentTypeEnumeration.Document.OpenOffice.ODP))
        //        {
        //            thumbnailPath = "/Images/Thumbnail/ppoint.png";
        //            supportPreview = true;
        //        }
        //        else if (page.ContentType.Equals(ContentTypeEnumeration.Document.PDF))
        //        {
        //            thumbnailPath = "/Images/Thumbnail/pdf.png";
        //            supportPreview = true;
        //        }

        //        return Json(new
        //        {
        //            thumbnailPath = thumbnailPath,
        //            supportPreview = supportPreview
        //        }, JsonRequestBehavior.AllowGet);
        //    }
        //    catch (Exception ex)
        //    {
        //        base.ProcessError(ex);
        //        return null;
        //    }
        //}

        //public PartialViewResult _GetAnnotations(Guid batchId, Guid docId, Guid pageId)
        //{
        //    try
        //    {
        //        // Get page
        //        var openedBatches = Session[Constant.SESSION_OPENED_BATCHES] as OrderedDictionary;
        //        var batch = openedBatches[batchId.ToString()] as ViewBatchModel;
        //        var doc = batch.Documents.Single(h => h.Id == docId);
        //        var page = doc.Pages.Single(h => h.OriginPage.Id == pageId);

        //        var model = new ViewGetAnnotationsModel();
        //        model.Annotations = page.SeeAnnotations;
        //        // Set permission for highlight
        //        model.CanSeeHighlight = doc.CanSeeHighlight;
        //        model.CanAddHighlight = doc.CanAddHighlight;
        //        model.CanDeleteHighlight = doc.CanDeleteHighlight;
        //        // Set permission for redaction
        //        model.CanHideRedaction = doc.CanHideRedaction;
        //        model.CanAddRedaction = doc.CanAddRedaction;
        //        model.CanDeleteRedaction = doc.CanDeleteRedaction;
        //        // Set permission for text
        //        model.CanSeeText = doc.CanSeeText;
        //        model.CanAddText = doc.CanAddText;
        //        model.CanDeleteText = doc.CanDeleteText;

        //        return PartialView(model);
        //    }
        //    catch (Exception ex)
        //    {
        //        base.ProcessError(ex);
        //        return null;
        //    }
        //}
        //#endregion

        ////[HttpPost]
        ////public string _GetNativeFile(Guid batchId, Guid docId, Guid pageId)
        ////{
        ////    try
        ////    {
        ////        // Get the list opened batch in session
        ////        var openedBatches = Session[Constant.SESSION_OPENED_BATCHES] as OrderedDictionary;

        ////        // Get page
        ////        var activeBatch = openedBatches[batchId.ToString()] as ViewBatchModel;
        ////        var doc = activeBatch.Documents.SingleOrDefault(h => h.Id == docId);
        ////        var page = doc.Pages.SingleOrDefault(h => h.OriginPage.Id == pageId);

        ////        // Check support preview file
        ////        if (!page.SupportPreview)
        ////        {
        ////            return null;
        ////        }

        ////        // Get temp folder file path
        ////        var fileFoder = Server.MapPath("~/" + Constant.FOLDER_TEMP_FILE);

        ////        // Check file is already save to temp folder
        ////        if (!string.IsNullOrEmpty(page.FilePath) && System.IO.File.Exists(fileFoder + "/" + page.FilePath))
        ////        {
        ////            return page.FilePath;
        ////        }

        ////        // Calculate file path
        ////        var fileExtension = page.OriginPage.FileExtension.ToLower();
        ////        var originFilePath = fileFoder + "/" + pageId + "." + fileExtension;

        ////        // Save file to temp file folder
        ////        System.IO.File.WriteAllBytes(originFilePath, page.OriginPage.FileBinary);

        ////        string outputFilePath;
        ////        string returnFileName;
        ////        // Convert to pdf if it is MS office file
        ////        if (Constant.MS_OFFICE_SUPPORT_FILE_EXTENSIONS.Any(h => h == fileExtension))
        ////        {
        ////            outputFilePath = fileFoder + "/" + pageId + ".pdf";
        ////            returnFileName = pageId + ".pdf";

        ////            // Convert
        ////            PdfConverter.ConvertToPdf(originFilePath, outputFilePath);
        ////            System.IO.File.Delete(originFilePath);
        ////        }
        ////        else
        ////        {
        ////            outputFilePath = originFilePath;
        ////            returnFileName = pageId + "." + fileExtension;
        ////        }

        ////        // Store file name in session for delete at session end
        ////        var tempFiles = Session[Constant.SESSION_TEMP_FILE] as List<string>;
        ////        tempFiles.Add(returnFileName);

        ////        // Update batch
        ////        page.FilePath = Constant.FOLDER_TEMP_FILE + "/" + returnFileName;
        ////        openedBatches[batchId.ToString()] = activeBatch;

        ////        return page.FilePath;
        ////    }
        ////    catch (Exception ex)
        ////    {
        ////        return null;
        ////    }
        ////}

        //[HttpGet]
        //[ChildActionOnly]
        //public PartialViewResult _GetOpenedMenu()
        //{
        //    try
        //    {
        //        return PartialView("_GetOpenedMenu", JsonConvert.SerializeObject(GetJsonOpenedMenu()));
        //    }
        //    catch (Exception ex)
        //    {
        //        base.ProcessError(ex);
        //        return null;
        //    }
        //}

        //[HttpPost]
        //public void _UpdateActiveBatch(Guid batchId)
        //{
        //    try
        //    {
        //        // Get the list opened batch in session
        //        var openedBatches = Session[Constant.SESSION_OPENED_BATCHES] as OrderedDictionary;
        //        // Check list opened batch is empty
        //        if (openedBatches.Count == 0)
        //        {
        //            return;
        //        }

        //        var stringBatchId = batchId.ToString();
        //        // Check the active opened is existed
        //        if (!openedBatches.Contains(stringBatchId))
        //        {
        //            return;
        //        }

        //        // Get the first opened batch
        //        var activeOpenedBatch = openedBatches[stringBatchId] as ViewBatchModel;

        //        // Update value for batch
        //        openedBatches[stringBatchId] = activeOpenedBatch;
        //        Session[Constant.SESSION_ACTIVE_OPENED_BATCH_ID] = stringBatchId;
        //    }
        //    catch (Exception ex)
        //    {
        //        base.ProcessError(ex);
        //    }
        //}

        //[HttpPost]
        //public void _OpenBatchesForSearchController(List<ViewBatchModel> batches, string batchTypeName)
        //{
        //    try
        //    {
        //        // Lock batches
        //        var batchGuids = batches.Select(h => h.Id).ToList();
        //        var workProvider = new WorkItemProvider();
        //        workProvider.LockWorkItems(batchGuids);

        //        // Get the list opened batch in session
        //        var openedBatches = Session[Constant.SESSION_OPENED_BATCHES] as OrderedDictionary;

        //        // Add opened batch to session
        //        foreach (var batch in batches)
        //        {
        //            if (!openedBatches.Contains(batch.Id.ToString()))
        //            {
        //                openedBatches.Add(batch.Id.ToString(), new ViewBatchModel()
        //                {
        //                    Id = batch.Id,
        //                    BatchType = new BatchType() { Name = batchTypeName },
        //                    BlockingActivityName = batch.BlockingActivityName
        //                });
        //            }
        //        }

        //        // Set active open batch
        //        Session[Constant.SESSION_ACTIVE_OPENED_BATCH_ID] = batches[0].Id.ToString();
        //    }
        //    catch (Exception ex)
        //    {
        //        base.ProcessError(ex);
        //    }
        //}

        //[HttpPost]
        //public JsonResult _CloseBatches(Guid batchId, string closeType, bool isSave, bool isSubmit = false)
        //{
        //    try
        //    {
        //        // Get opened batches from session
        //        var openedBatches = Session[Constant.SESSION_OPENED_BATCHES] as OrderedDictionary;

        //        // Get new menu from remain opened batches
        //        var deletedBatchIds = new List<Guid>();
        //        ViewBatchModel openedBatch;

        //        switch (closeType)
        //        {
        //            case "CloseAll":
        //                for (int i = 0; i < openedBatches.Count; i++)
        //                {
        //                    openedBatch = openedBatches[i] as ViewBatchModel;
        //                    deletedBatchIds.Add(openedBatch.Id);
        //                }
        //                break;

        //            case "CloseOther":
        //                for (int i = 0; i < openedBatches.Count; i++)
        //                {
        //                    openedBatch = openedBatches[i] as ViewBatchModel;
        //                    if (openedBatch.Id != batchId)
        //                    {
        //                        deletedBatchIds.Add(openedBatch.Id);
        //                    }
        //                }
        //                break;

        //            default:
        //                deletedBatchIds.Add(batchId);
        //                break;
        //        }

        //        // Unlock batches
        //        var workItemProvider = new WorkItemProvider();

        //        if (isSave)
        //        {
        //            foreach (var item in deletedBatchIds)
        //            {
        //                _SaveBatch(item, isSubmit);
        //            }
        //        }
        //        else
        //        {
        //            workItemProvider.UnLockWorkItems(deletedBatchIds);
        //        }


        //        // Remove unlock batches from session
        //        foreach (var id in deletedBatchIds)
        //        {
        //            openedBatches.Remove(id.ToString());
        //        }

        //        // Get new menu from remain opened batches
        //        var menuBatches = this.GetJsonOpenedMenu();
        //        return Json(menuBatches);
        //    }
        //    catch (Exception ex)
        //    {
        //        base.ProcessError(ex);
        //        return null;
        //    }
        //}

        //[HttpPost]
        //public void _OpenBatchForAnotherController(Guid batchId)
        //{
        //    // Set active open batch
        //    Session[Constant.SESSION_ACTIVE_OPENED_BATCH_ID] = batchId.ToString();
        //}

        //#region Insert new page


        //[HttpPost]
        //public PartialViewResult _Camera(ViewUploadModel model)
        //{
        //    try
        //    {
        //        // Get the list opened batch in session
        //        var openedBatches = Session[Constant.SESSION_OPENED_BATCHES] as OrderedDictionary;
        //        var batch = openedBatches[model.BatchId] as ViewBatchModel;

        //        // Have no permission to modify document
        //        if (!batch.BatchPermission.CanModifyDocument)
        //        {
        //            return null;
        //        }

        //        var results = new List<ViewPageModel>();

        //        var pageModel = new ViewPageModel() { IsNew = true };
        //        var originPage = new Page();
        //        pageModel.OriginPage = originPage;
        //        pageModel.ContentType = "image/jpeg";
        //        originPage.Id = Guid.NewGuid();
        //        originPage.DocId = batch.Documents[0].Id;

        //        // Get binary data
        //        var BASE64_PNG_HEADER = "data:image/jpeg;base64,";
        //        var base64 = model.ImageData.Substring(BASE64_PNG_HEADER.Length);
        //        originPage.FileBinary = Convert.FromBase64String(base64);

        //        // Get file name and extension
        //        originPage.OriginalFileName = "Camera.jpeg";
        //        originPage.FileExtension = "jpeg";

        //        results.Add(pageModel);

        //        batch.Documents[0].Pages.AddRange(results);

        //        Session[Constant.SESSION_OPENED_BATCHES] = openedBatches;
        //        ViewBag.UploadType = model.Type;
        //        return PartialView("_Upload", results);
        //    }
        //    catch (Exception ex)
        //    {
        //        base.ProcessError(ex);
        //        return null;
        //    }
        //}

        //[HttpPost]
        //public PartialViewResult _Scan(ViewUploadModel model)
        //{
        //    try
        //    {
        //        // Get the list opened batch in session
        //        var openedBatches = Session[Constant.SESSION_OPENED_BATCHES] as OrderedDictionary;
        //        var batch = openedBatches[model.BatchId] as ViewBatchModel;

        //        var results = new List<ViewPageModel>();

        //        // Check have saved temp file folder in app config
        //        var folderTempFile = Utilities.GetFolderTempFiles();
        //        if (string.IsNullOrWhiteSpace(folderTempFile))
        //        {
        //            return null;
        //        }

        //        // Map folder
        //        folderTempFile = Server.MapPath(folderTempFile);
        //        if (!Directory.Exists(folderTempFile))
        //        {
        //            return null;
        //        }

        //        var jss = new JavaScriptSerializer();
        //        var fileNames = jss.Deserialize<List<string>>(model.FileNames);

        //        // Have no permission to modify document
        //        if (!batch.BatchPermission.CanModifyDocument)
        //        {
        //            foreach (var fileName in fileNames)
        //            {
        //                var filePath = Path.Combine(folderTempFile, fileName);
        //                System.IO.File.Delete(filePath);
        //            }
        //            return null;
        //        }

        //        for (int i = 0; i < fileNames.Count; i++)
        //        {
        //            var fileName = fileNames[i];

        //            // Get file binary
        //            var filePath = Path.Combine(folderTempFile, fileName);
        //            var fileBinary = System.IO.File.ReadAllBytes(filePath);
        //            var contentType = MimeMap.ContentTypeFromPath(fileName);

        //            //var contentType = fileName.ContentType;
        //            if (contentType == ContentTypeEnumeration.Image.TIFF)
        //            {
        //                #region Image file type tiff

        //                List<byte[]> tiffBinaries;

        //                // Get list binary
        //                tiffBinaries = ImageProcessing.SplitTiff(fileBinary);

        //                for (int j = 0; j < tiffBinaries.Count; j++)
        //                {
        //                    var tiff = tiffBinaries[j];

        //                    var pageModel = new ViewPageModel() { IsNew = true };
        //                    var originPage = new Page();
        //                    pageModel.OriginPage = originPage;
        //                    pageModel.ContentType = contentType;
        //                    originPage.Id = Guid.NewGuid();
        //                    originPage.DocId = batch.Documents[0].Id;
        //                    originPage.FileBinary = tiff;

        //                    // Get file name and extension
        //                    originPage.OriginalFileName = fileName;
        //                    originPage.FileExtension = "tiff";

        //                    results.Add(pageModel);
        //                }
        //                #endregion
        //            }
        //            else
        //            {
        //                #region File type is difference to Image file tiff
        //                var pageModel = new ViewPageModel() { IsNew = true };
        //                var originPage = new Page();
        //                pageModel.OriginPage = originPage;
        //                pageModel.ContentType = contentType;
        //                originPage.Id = Guid.NewGuid();
        //                originPage.DocId = batch.Documents[0].Id;
        //                originPage.FileBinary = fileBinary;

        //                // Get file name and extension
        //                originPage.OriginalFileName = fileName;
        //                originPage.FileExtension = string.Empty;
        //                var indexDotExtension = fileName.LastIndexOf(".");
        //                if (indexDotExtension > 0)
        //                {
        //                    originPage.FileExtension = fileName.Substring(indexDotExtension);
        //                }

        //                results.Add(pageModel);
        //                #endregion
        //            }

        //            // Delete file
        //            System.IO.File.Delete(filePath);
        //        }

        //        batch.Documents[0].Pages.AddRange(results);

        //        Session[Constant.SESSION_OPENED_BATCHES] = openedBatches;
        //        ViewBag.UploadType = model.Type;
        //        return PartialView("_Upload", results);
        //    }
        //    catch (Exception ex)
        //    {
        //        base.ProcessError(ex);
        //        return null;
        //    }
        //}

        //[AllowAnonymous]
        //[HttpPost]
        //public string _ScanUpload(string scanToken)
        //{
        //    var fileName = string.Empty;
        //    try
        //    {
        //        // Decrypt token
        //        var decryptScanToken = CryptographyHelper.DecryptUsingSymmetricAlgorithm(scanToken);
        //        decryptScanToken = decryptScanToken.Substring("yyyy-MM-dd HH:mm:ss.fff".Length);

        //        // De-serialize
        //        var serializer = new XmlSerializer(typeof(List<string>));
        //        List<string> userNameAndPassword;
        //        using (TextReader reader = new StringReader(decryptScanToken))
        //        {
        //            userNameAndPassword = (List<string>)serializer.Deserialize(reader);
        //        }

        //        // Check login
        //        var securityProvider = new SecurityProvider();
        //        var user = securityProvider.AuthoriseUser(userNameAndPassword[0], userNameAndPassword[1]);
        //        if (user == null)
        //        {
        //            return null;
        //        }

        //        // Check have upload file
        //        if (Request.Files == null || Request.Files.Count == 0)
        //        {
        //            return fileName;
        //        }

        //        // Check have saved temp file folder in app config
        //        var folderTempFile = Utilities.GetFolderTempFiles();
        //        if (string.IsNullOrWhiteSpace(folderTempFile))
        //        {
        //            return fileName;
        //        }

        //        // Map folder
        //        folderTempFile = Server.MapPath(folderTempFile);
        //        if (!Directory.Exists(folderTempFile))
        //        {
        //            return fileName;
        //        }

        //        var file = Request.Files[0];
        //        // Save file
        //        System.IO.File.WriteAllBytes(Path.Combine(folderTempFile, file.FileName), file.InputStream.ReadAllBytes());
        //        fileName = file.FileName;
        //    }
        //    catch (Exception ex)
        //    {
        //        base.ProcessError(ex);
        //    }

        //    return fileName;
        //}
        //#endregion

        //public PartialViewResult _GetComments(Guid id)
        //{
        //    try
        //    {
        //        // Get the list opened batch in session
        //        var openedBatches = Session[Constant.SESSION_OPENED_BATCHES] as OrderedDictionary;
        //        var activeOpenedBatchId = Session[Constant.SESSION_ACTIVE_OPENED_BATCH_ID] as string;
        //        var activeBatch = openedBatches[activeOpenedBatchId] as ViewBatchModel;

        //        return PartialView(activeBatch);
        //    }
        //    catch (Exception ex)
        //    {
        //        base.ProcessError(ex);
        //        return null;
        //    }
        //}

        //[HttpPost]
        //public bool _SaveTempBatch(ViewSaveBatchModel batchInfo)
        //{
        //    ViewBatchModel batch = null;
        //    try
        //    {
        //        // Get the list opened batch in session
        //        var openedBatches = Session[Constant.SESSION_OPENED_BATCHES] as OrderedDictionary;
        //        // Check list opened batch is empty
        //        if (openedBatches.Count == 0)
        //        {
        //            return false;
        //        }
        //        // Check the active opened is existed
        //        if (!openedBatches.Contains(batchInfo.Id))
        //        {
        //            return false;
        //        }

        //        var loginUserName = Utilities.UserName;

        //        // Get batch
        //        batch = openedBatches[batchInfo.Id] as ViewBatchModel;
        //        // Save batch information
        //        if (batchInfo.IsReject || batch.BatchPermission.CanReject)
        //        {
        //            batch.IsRejected = batchInfo.IsReject;
        //        }

        //        #region Save comment
        //        foreach (var item in batchInfo.Comments)
        //        {
        //            batch.Comments.Add(new Comment()
        //            {
        //                Id = Guid.Empty,
        //                CreatedBy = loginUserName,
        //                CreatedDate = item.CreateDate,
        //                Note = this.GetXmlContentText(item.Note, 10)
        //            });
        //        }
        //        #endregion

        //        #region Save index
        //        if (batch.BatchPermission.CanModifyIndexes)
        //        {
        //            foreach (var item in batchInfo.Indexes)
        //            {
        //                var field = batch.FieldValues.SingleOrDefault(h => h.Id.ToString() == item.Id);
        //                if (field != null && Utilities.ValidateFieldValue(item, field.FieldMetaData))
        //                {
        //                    field.Value = item.Value;
        //                }
        //            }
        //        }
        //        #endregion

        //        #region Save document

        //        var oldDocs = batch.Documents;
        //        var tempDoc = oldDocs[0];
        //        var looseDoc = oldDocs[1];
        //        var newDocs = new List<ViewDocumentModel>();

        //        foreach (var docInfo in batchInfo.Documents)
        //        {
        //            // Get old doc
        //            var oldDoc = oldDocs.SingleOrDefault(h => h.Id.ToString() == docInfo.Id);
        //            if (oldDoc == null)
        //            {
        //                continue;
        //            }

        //            #region Save index
        //            if (batch.BatchPermission.CanModifyIndexes)
        //            {
        //                foreach (var item in docInfo.Indexes)
        //                {
        //                    var field = oldDoc.FieldValues.SingleOrDefault(h => h.Id.ToString() == item.Id);

        //                    if (field != null)
        //                    {
        //                        if (field.FieldMetaData.DataTypeEnum == FieldDataType.Table)
        //                        {
        //                            Utilities.ValidateFieldValue(item, field.FieldMetaData, field.TableFieldValue, field.DocId);
        //                        }
        //                        else if (Utilities.ValidateFieldValue(item, field.FieldMetaData, null, Guid.Empty))
        //                        {
        //                            field.Value = item.Value;
        //                        }
        //                    }
        //                }
        //            }
        //            #endregion

        //            #region Save Page
        //            // Get old pages
        //            var oldPages = oldDoc.Pages;
        //            var newPages = new List<ViewPageModel>();

        //            for (int indexPageInfo = 0; indexPageInfo < docInfo.Pages.Count; indexPageInfo++)
        //            {
        //                var pageInfo = docInfo.Pages[indexPageInfo];
        //                var isFromLooseDoc = false;
        //                var isFromTempDoc = false;
        //                var parentDoc = oldDoc;

        //                #region Get page, and its right parent doc

        //                // Try get page from parent doc
        //                var page = oldPages.SingleOrDefault(h => h.OriginPage.Id.ToString() == pageInfo.Id);
        //                // Get no page in parent doc => try get page from loose doc
        //                if (page == null)
        //                {
        //                    page = looseDoc.Pages.SingleOrDefault(h => h.OriginPage.Id.ToString() == pageInfo.Id);
        //                    // Get no page in loose doc => try get page from temp doc (doc contain new inserted page)
        //                    if (page == null)
        //                    {
        //                        // Have no permission to add new doc => not necessary to search in temp doc
        //                        if (!batch.BatchPermission.CanModifyDocument)
        //                        {
        //                            continue;
        //                        }

        //                        page = tempDoc.Pages.SingleOrDefault(h => h.OriginPage.Id.ToString() == pageInfo.Id);
        //                        if (page == null)
        //                        {
        //                            continue;
        //                        }
        //                        isFromTempDoc = true;
        //                    }
        //                    else
        //                    {
        //                        isFromLooseDoc = true;
        //                        parentDoc = looseDoc;
        //                    }
        //                }

        //                #endregion

        //                #region Update page to be in right list doc, page width, height, rotate angle

        //                var newAdjustRotateAngle = 0;
        //                if (batch.BatchPermission.CanModifyDocument)
        //                {
        //                    // Save parent doc of page
        //                    if (isFromLooseDoc)
        //                    {
        //                        looseDoc.Pages.Remove(page);
        //                        parentDoc = oldDoc;
        //                    }
        //                    else if (isFromTempDoc)
        //                    {
        //                        tempDoc.Pages.Remove(page);
        //                    }
        //                    else
        //                    {
        //                        oldPages.Remove(page);
        //                    }
        //                    newPages.Add(page);

        //                    // Save page info
        //                    page.OriginPage.DocId = parentDoc.Id;

        //                    var newRotateAngle = Utilities.GetRotateAngle(pageInfo.RotateAngle);
        //                    if (page.OriginPage.RotateAngle != newRotateAngle && newRotateAngle % 90 == 0)
        //                    {
        //                        newAdjustRotateAngle = newRotateAngle - (int)page.OriginPage.RotateAngle;
        //                        page.AdjustRotateAngle += newAdjustRotateAngle;
        //                        page.OriginPage.RotateAngle = newRotateAngle;

        //                        switch (newAdjustRotateAngle)
        //                        {
        //                            case 90:
        //                            case -90:
        //                            case 270:
        //                            case -270:
        //                                var tempSize = page.OriginPage.Width;
        //                                page.OriginPage.Width = page.OriginPage.Height;
        //                                page.OriginPage.Height = tempSize;
        //                                break;
        //                        }
        //                    }
        //                }

        //                #endregion

        //                // Update status reject
        //                if (!pageInfo.IsReject || batch.BatchPermission.CanReject)
        //                {
        //                    page.OriginPage.IsRejected = pageInfo.IsReject;
        //                }

        //                // Get annotations
        //                var oldAnnoes = page.SeeAnnotations.Where(h => h.Id != Guid.Empty).ToList();
        //                var newAnnoes = new List<Annotation>();

        //                #region Update delete annotations

        //                // Get list id of delete annotation
        //                var delAnnoIds = string.Format("{0}", pageInfo.DeleteAnnotations)
        //                                       .Split(";".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
        //                foreach (var delAnnoId in delAnnoIds)
        //                {
        //                    var delAnno = oldAnnoes.SingleOrDefault(h => h.Id.ToString() == delAnnoId);
        //                    if (delAnno == null)
        //                    {
        //                        continue;
        //                    }

        //                    var isCanDelete = true;
        //                    // Get permission delete annotation with specify annotation type
        //                    switch (delAnno.Type)
        //                    {
        //                        case Constant.ANNO_TYPE_HIGHLIGHT:
        //                            isCanDelete = parentDoc.AnnotationPermission.CanDeleteHighlight;
        //                            break;
        //                        case Constant.ANNO_TYPE_TEXT:
        //                            isCanDelete = parentDoc.AnnotationPermission.CanDeleteText;
        //                            break;
        //                        default:
        //                            break;
        //                    }

        //                    if (!isCanDelete)
        //                    {
        //                        continue;
        //                    }

        //                    oldAnnoes.Remove(delAnno);
        //                    page.OriginPage.DeleteAnnotations.Add(delAnno.Id);
        //                }

        //                #endregion

        //                int scale = docInfo.Scale;
        //                foreach (var annoInfo in pageInfo.Annotations)
        //                {
        //                    if (string.IsNullOrWhiteSpace(annoInfo.Id) || annoInfo.Id == Guid.Empty.ToString())
        //                    {
        //                        #region Add new annotation
        //                        var isCanAddNew = false;
        //                        var rotateAngle = 0;
        //                        string content = null;
        //                        // Get permission add new annotation with specify annotation type
        //                        switch (annoInfo.Type)
        //                        {
        //                            case Constant.ANNO_TYPE_HIGHLIGHT:
        //                                isCanAddNew = parentDoc.AnnotationPermission.CanAddHighlight;
        //                                break;
        //                            case Constant.ANNO_TYPE_TEXT:
        //                                isCanAddNew = parentDoc.AnnotationPermission.CanAddText;
        //                                rotateAngle = Utilities.GetRotateAngle(annoInfo.RotateAngle);
        //                                content = this.GetXmlContentText(annoInfo.Content, scale);
        //                                break;
        //                            case Constant.ANNO_TYPE_REDACTION:
        //                                isCanAddNew = parentDoc.AnnotationPermission.CanAddRedaction;
        //                                break;
        //                            default:
        //                                break;
        //                        }
        //                        if (!isCanAddNew)
        //                        {
        //                            continue;
        //                        }

        //                        // Create new annotation
        //                        newAnnoes.Add(new Annotation()
        //                        {
        //                            Id = Guid.Empty,
        //                            PageId = new Guid(pageInfo.Id),
        //                            Type = annoInfo.Type,
        //                            Height = Math.Round(annoInfo.Height * 10 / scale),
        //                            Width = Math.Round(annoInfo.Width * 10 / scale),
        //                            Left = Math.Round(annoInfo.Left * 10 / scale),
        //                            RotateAngle = rotateAngle,
        //                            Top = Math.Round(annoInfo.Top * 10 / scale),
        //                            Content = content,
        //                            LineEndAt = "TopLeft",
        //                            LineStartAt = "TopLeft",
        //                            LineStyle = "ArrowAtEnd",
        //                            LineWeight = 0,
        //                        });
        //                        #endregion
        //                    }
        //                    else
        //                    {
        //                        #region Update annotation

        //                        var isCanUpdate = true;
        //                        // Get permission edit annotation with specify annotation type
        //                        switch (annoInfo.Type)
        //                        {
        //                            case Constant.ANNO_TYPE_HIGHLIGHT:
        //                                isCanUpdate = parentDoc.AnnotationPermission.CanDeleteHighlight;
        //                                break;
        //                            case Constant.ANNO_TYPE_TEXT:
        //                                isCanUpdate = parentDoc.AnnotationPermission.CanDeleteText;
        //                                break;
        //                            default:
        //                                break;
        //                        }

        //                        // Update annotation
        //                        var anno = oldAnnoes.SingleOrDefault(h => h.Id.ToString() == annoInfo.Id);

        //                        if (anno != null)
        //                        {
        //                            var newRotateAngle = Utilities.GetRotateAngle(annoInfo.RotateAngle);

        //                            if (isCanUpdate)
        //                            {
        //                                anno.Height = Math.Round(annoInfo.Height * 10 / scale);
        //                                anno.Width = Math.Round(annoInfo.Width * 10 / scale);
        //                                anno.Left = Math.Round(annoInfo.Left * 10 / scale);
        //                                anno.Top = Math.Round(annoInfo.Top * 10 / scale);

        //                                if (anno.Type == Constant.ANNO_TYPE_TEXT)
        //                                {
        //                                    anno.Content = this.GetXmlContentText(annoInfo.Content, scale);
        //                                    anno.RotateAngle = Utilities.GetRotateAngle((int)anno.RotateAngle
        //                                                                                + newAdjustRotateAngle);
        //                                }
        //                            }
        //                            else // Have no permission edit anno
        //                            {
        //                                #region Update top, left width and height according to rotate angle of page

        //                                switch (newAdjustRotateAngle)
        //                                {
        //                                    case 90:
        //                                    case -90:
        //                                    case 270:
        //                                    case -270:
        //                                        var tempSize = anno.Width;
        //                                        anno.Width = anno.Height;
        //                                        anno.Height = tempSize;

        //                                        double tempLeft;
        //                                        double tempTop;
        //                                        if (newAdjustRotateAngle == 90 || newAdjustRotateAngle == -90)
        //                                        {
        //                                            tempLeft = page.OriginPage.Width - anno.Top - anno.Width;
        //                                            tempTop = anno.Left;
        //                                        }
        //                                        else
        //                                        {
        //                                            tempLeft = anno.Top;
        //                                            tempTop = page.OriginPage.Height - anno.Left - anno.Height;
        //                                        }
        //                                        anno.Left = tempLeft;
        //                                        anno.Top = tempTop;
        //                                        break;

        //                                    case 180:
        //                                    case -180:
        //                                        anno.Left = page.OriginPage.Width - anno.Left - anno.Width;
        //                                        anno.Top = page.OriginPage.Height - anno.Top - anno.Height;
        //                                        break;
        //                                }

        //                                #endregion

        //                                // Update rotate angle of text
        //                                if (anno.Type == Constant.ANNO_TYPE_TEXT)
        //                                {
        //                                    anno.RotateAngle = Utilities.GetRotateAngle((int)anno.RotateAngle
        //                                                                                + newAdjustRotateAngle);
        //                                }
        //                            }

        //                            oldAnnoes.Remove(anno);
        //                            newAnnoes.Add(anno);
        //                        }
        //                        #endregion
        //                    }
        //                }

        //                newAnnoes.AddRange(oldAnnoes);
        //                page.SeeAnnotations = newAnnoes;
        //            }

        //            oldDoc.SavePages = newPages;
        //            #endregion
        //        }

        //        #endregion

        //        #region Reorder document and page

        //        var totalPage = 0;
        //        var flgHitLooseDoc = false;
        //        foreach (var docInfo in batchInfo.Documents)
        //        {
        //            var doc = oldDocs.SingleOrDefault(h => h.Id.ToString() == docInfo.Id);
        //            if (doc == null)
        //            {
        //                continue;
        //            }

        //            // Turn on flag meet loose doc
        //            if (doc.DocKind == Constant.DOC_KIND_LOOSE)
        //            {
        //                flgHitLooseDoc = true;
        //            }

        //            oldDocs.Remove(doc);
        //            newDocs.Add(doc);

        //            if (batch.BatchPermission.CanDelete)
        //            {
        //                doc.DeletedPages.AddRange(doc.Pages.Where(h => !h.IsNew).Select(h => h.OriginPage.Id));
        //            }
        //            else
        //            {
        //                doc.SavePages.AddRange(doc.Pages.Where(h => !h.IsNew));
        //            }

        //            doc.Pages = doc.SavePages;
        //            doc.PageCount = doc.Pages.Count;
        //            totalPage += doc.PageCount;
        //            doc.SavePages = new List<ViewPageModel>();

        //            // Update page number
        //            for (int i = 0; i < doc.Pages.Count; i++)
        //            {
        //                doc.Pages[i].OriginPage.PageNumber = i + 1;
        //            }
        //        }

        //        if (!flgHitLooseDoc)
        //        {
        //            if (batch.BatchPermission.CanDelete)
        //            {
        //                looseDoc.DeletedPages.AddRange(looseDoc.Pages.Where(h => !h.IsNew)
        //                                                       .Select(h => h.OriginPage.Id));
        //                looseDoc.Pages.Clear();
        //            }

        //            looseDoc.PageCount = looseDoc.Pages.Count;
        //            totalPage += looseDoc.PageCount;

        //            newDocs.Insert(0, looseDoc);
        //            oldDocs.Remove(looseDoc);
        //        }
        //        else
        //        {
        //            newDocs.Remove(looseDoc);
        //            newDocs.Insert(0, looseDoc);
        //        }


        //        batch.Documents = new List<ViewDocumentModel>();
        //        // Add new doc for insert new page
        //        oldDocs[0].Pages.Clear();
        //        batch.Documents.Add(oldDocs[0]);
        //        batch.Documents.AddRange(newDocs);

        //        if (batch.DeletedDocuments == null)
        //        {
        //            batch.DeletedDocuments = new List<Guid>();
        //        }

        //        // Remove insert new page doc
        //        oldDocs.RemoveAt(0);
        //        if (batch.BatchPermission.CanDelete)
        //        {
        //            batch.DeletedDocuments.AddRange(oldDocs.Select(h => h.Id));
        //        }
        //        else
        //        {
        //            foreach (var doc in oldDocs)
        //            {
        //                doc.PageCount = doc.Pages.Count;
        //                totalPage += doc.PageCount;

        //                // Update page number
        //                for (int i = 0; i < doc.Pages.Count; i++)
        //                {
        //                    doc.Pages[i].OriginPage.PageNumber = i + 1;
        //                }
        //            }
        //            batch.Documents.AddRange(oldDocs);
        //        }

        //        batch.DocCount = batch.Documents.Count - 1;
        //        batch.PageCount = totalPage;

        //        #endregion

        //        return true;
        //    }
        //    catch (Exception ex)
        //    {
        //        // Reset batch to force re-load from db next time
        //        if (batch != null)
        //        {
        //            batch.IsLoaded = false;
        //        }
        //        base.ProcessError(ex);
        //        return false;
        //    }
        //}

        //[HttpPost]
        //public bool _SaveBatch(Guid batchId, bool isSubmit = false)
        //{
        //    try
        //    {
        //        // Get the list opened batch in session
        //        var openedBatches = Session[Constant.SESSION_OPENED_BATCHES] as OrderedDictionary;
        //        // Check list opened batch is empty
        //        if (openedBatches.Count == 0)
        //        {
        //            return false;
        //        }

        //        var stringBatchId = batchId.ToString();
        //        // Check the active opened is existed
        //        if (!openedBatches.Contains(stringBatchId))
        //        {
        //            return false;
        //        }

        //        // Get the first opened batch
        //        var openedBatch = openedBatches[stringBatchId] as ViewBatchModel;

        //        // Map model to batch
        //        var batch = ObjectMapper.GetBatch(openedBatch);
        //        var batchProvider = new WorkItemProvider();

        //        if (isSubmit)
        //        {
        //            batchProvider.ApproveOrRejectWorkItem(batch);
        //        }
        //        else
        //        {
        //            batchProvider.SaveWorkItem(batch);
        //        }


        //        return true;
        //    }
        //    catch (Exception ex)
        //    {
        //        base.ProcessError(ex);
        //        return false;
        //    }
        //}

        //#region Private methods

        ///// <summary>
        ///// Get the json opened batch id, batch type name, blocking activity name
        ///// </summary>
        ///// <returns></returns>
        //private List<ViewContextMenuModel> GetJsonOpenedMenu()
        //{
        //    // Get the list opened batch in session
        //    var openedBatches = Session[Constant.SESSION_OPENED_BATCHES] as OrderedDictionary;
        //    if (openedBatches.Count == 0)
        //    {
        //        return new List<ViewContextMenuModel>();
        //    }


        //    // Initialize view batch value on the top menu
        //    var viewBatches = new List<ViewContextMenuModel>();
        //    ViewBatchModel openedBatch;
        //    for (int i = 0; i < openedBatches.Count; i++)
        //    {
        //        openedBatch = openedBatches[i] as ViewBatchModel;
        //        viewBatches.Add(new ViewContextMenuModel()
        //        {
        //            Key = openedBatch.Id.ToString(),
        //            Value = string.Format("{0} - {1} [{2}]",
        //                                  openedBatch.BatchType.Name, openedBatch.BlockingActivityName, openedBatch.Id)
        //        });
        //    }

        //    return viewBatches;
        //}

        //private string GetXmlContentText(string contentHtml, int scale)
        //{
        //    var xmlDoc = new XmlDocument();

        //    xmlDoc.LoadXml("<div>" + contentHtml.Replace("<br>", "<br/>") + "</div>");
        //    var div = xmlDoc.DocumentElement;

        //    var colorDefault = "color:rgb(0,0,0)";
        //    var colorPrefixLength = "color:rgb(".Length;
        //    var colorSeparator = ",)".ToCharArray();

        //    var fontSizeDefault = 12;
        //    var fontSizePrefixLength = "font-size:".Length;

        //    var fontStyleDefault = "font-style:normal";
        //    var fontStylePrefixLength = "font-style:".Length;

        //    var fontWeightDefault = "font-weight:normal";
        //    var fontWeightPrefixLength = "font-weight:".Length;

        //    var semiCommaSeparator = ";".ToCharArray();

        //    var paragraphs = new List<Paragraph>();
        //    var paragraph = new Paragraph();

        //    foreach (XmlNode spanOrBr in div.ChildNodes)
        //    {
        //        if ("span".Equals(spanOrBr.Name, StringComparison.OrdinalIgnoreCase))
        //        {
        //            if (spanOrBr.Attributes["style"] == null)
        //            {
        //                continue;
        //            }

        //            // Get attribute style of html
        //            var styleString = spanOrBr.Attributes["style"].Value.Replace(" ", string.Empty).ToLower();
        //            // Split style value
        //            var styles = styleString.Split(semiCommaSeparator, StringSplitOptions.RemoveEmptyEntries);

        //            // Create new run
        //            var run = new Run();
        //            run.Content = spanOrBr.InnerText;
        //            if (string.IsNullOrEmpty(run.Content))
        //            {
        //                continue;
        //            }
        //            paragraph.Runs.Add(run);

        //            #region Set attribute FontSize
        //            var fontSizeStyle = styles.FirstOrDefault(h => h.StartsWith("font-size:"));
        //            if (fontSizeStyle != null)
        //            {
        //                // Get font size value
        //                fontSizeStyle = fontSizeStyle.Substring(fontSizePrefixLength);
        //                fontSizeStyle = fontSizeStyle.Substring(0, fontSizeStyle.Length - 2);

        //                int value = 0;
        //                if (int.TryParse(fontSizeStyle, out value))
        //                {
        //                    value = (int)Math.Round(value * 10.0 / scale);
        //                    if (value != fontSizeDefault)
        //                    {
        //                        run.FontSize = value.ToString();
        //                    }
        //                }
        //            }
        //            #endregion

        //            #region Set attribute FontStyle
        //            var fontStyleStyle = styles.FirstOrDefault(h => h.StartsWith("font-style:"));
        //            if (fontStyleStyle != null && !fontStyleStyle.Equals(fontStyleDefault))
        //            {
        //                // Get font size value
        //                fontStyleStyle = fontStyleStyle.Substring(fontStylePrefixLength);
        //                if (fontStyleStyle == "italic")
        //                {
        //                    run.FontStyle = "Italic";
        //                }
        //            }
        //            #endregion

        //            #region Set attribute FontWeight
        //            var fontWeightStyle = styles.FirstOrDefault(h => h.StartsWith("font-weight:"));
        //            if (fontWeightStyle != null && !fontWeightStyle.Equals(fontWeightDefault))
        //            {
        //                // Get font size value
        //                fontWeightStyle = fontWeightStyle.Substring(fontWeightPrefixLength);
        //                if (fontWeightStyle == "bold")
        //                {
        //                    run.FontWeight = "Bold";
        //                }
        //            }
        //            #endregion

        //            #region Set attribute Foreground
        //            var colorStyle = styles.FirstOrDefault(h => h.StartsWith("color:"));
        //            if (colorStyle != null && !colorStyle.Equals(colorDefault))
        //            {
        //                // Get red, green and blue value
        //                colorStyle = colorStyle.Substring(colorPrefixLength);
        //                var colors = colorStyle.Split(colorSeparator, StringSplitOptions.RemoveEmptyEntries);
        //                if (colors.Length == 3)
        //                {
        //                    var foreground = string.Empty;
        //                    foreach (var colorValue in colors)
        //                    {
        //                        int value = 0;
        //                        if (!int.TryParse(colorValue, out value))
        //                        {
        //                            break;
        //                        }
        //                        foreground += value.ToString("X2");
        //                    }

        //                    if (foreground.Length == 6)
        //                    {
        //                        run.Foreground = "#FF" + foreground;
        //                    }
        //                }

        //            }
        //            #endregion
        //        }
        //        else if ("br".Equals(spanOrBr.Name, StringComparison.OrdinalIgnoreCase))
        //        {
        //            if (paragraph.Runs.Count > 0)
        //            {
        //                paragraphs.Add(paragraph);
        //                paragraph = new Paragraph();
        //            }
        //        }
        //    }

        //    XmlSerializer serializer = new XmlSerializer(typeof(List<Paragraph>));
        //    var xns = new XmlSerializerNamespaces();
        //    xns.Add(string.Empty, string.Empty);

        //    XmlWriterSettings settings = new XmlWriterSettings();
        //    settings.OmitXmlDeclaration = true;

        //    var builder = new StringBuilder();
        //    XmlWriter writer = XmlWriter.Create(builder, settings);

        //    serializer.Serialize(writer, paragraphs, xns);

        //    var prefixArrayLength = "<ArrayOfParagraph>".Length;
        //    var result = builder.ToString(prefixArrayLength,
        //                                  builder.Length - "<ArrayOfParagraph></ArrayOfParagraph>".Length);


        //    return string.Format(Constant.SECTION_TEXT_TEMPLATE, result);

        //}

        //#endregion
    }
}
