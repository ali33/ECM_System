using ArchiveMVC5.Utility;
using ArchiveMVC5.Models;
using ArchiveMVC5.Models.DataProvider;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using PdfSharp.Pdf;
using PdfSharp.Drawing;
using System.Web.Configuration;
using System.Net.Configuration;
using log4net;
using System.Net.Mail;
using System.Dynamic;
using System.Data;
using System.Configuration;

namespace ArchiveMVC5.Controllers
{
    public class CaptureController : BaseController
    {
        //
        // GET: /Capture/

        public ActionResult Index()
        {
            Utilities.OpenningDocument = null;
            DocumentTypeProvider docTypePro = new DocumentTypeProvider(Utilities.UserName, Utilities.Password);
            PermissionProvider pp = new PermissionProvider(Utilities.UserName, Utilities.Password);
            CaptureModel captureModel = new CaptureModel();
            List<DocumentTypeModel> captureDocumentTypes = docTypePro.GetCapturedDocumentTypes().ToList();

            if (Utilities.CacheCaptureDocuments != null)
            {
                foreach (var docCollection in Utilities.CacheCaptureDocuments.Documents)
                {
                    if (captureModel.documentCachings == null)
                    {
                        captureModel.documentCachings = new List<CapturedDocumentCaching>();
                    }

                    DocumentTypeModel docType = docTypePro.GetDocumentType(docCollection.DocumentTypeId);
                    DocumentModel doc = new DocumentModel(DateTime.Now, Utilities.UserName, docType);
                    
                    if (doc != null && !doc.DocumentType.DocumentTypePermission.AllowedSearch)
                    {
                        ViewData[Constant.KEY_ERROR] = true;
                        return View();
                    }

                    foreach (var page in docCollection.Pages)
                    {
                        PageModel pageInsert = new PageModel();
                        CacheObject fileCache = (CacheObject)System.Web.HttpContext.Current.Cache[page.ImgKey];
                        if (fileCache is CacheImage)
                        {
                            Size size = ProcessImages.GetImageSize(((CacheImage)fileCache).FileBinaries);
                            pageInsert.FileBinaries = ((CacheImage)fileCache).FileBinaries;
                            pageInsert.FileType = FileTypeModel.Image;
                            pageInsert.Width = page.PageWidth;
                            pageInsert.Height = page.PageHeight;
                            pageInsert.RotateAngle = page.RotateAngle;
                            if (page.Annotations != null)
                            {
                                pageInsert.Annotations = page.Annotations;
                                pageInsert.Annotations.ForEach(delegate(AnnotationModel annotation)
                                {
                                    annotation.ModifiedBy = annotation.CreatedBy = Utilities.UserName;
                                    annotation.ModifiedOn = annotation.CreatedOn = DateTime.Now;
                                });
                            }
                        }
                        else
                        {
                            pageInsert.FileBinaries = System.IO.File.ReadAllBytes(Server.MapPath("~/Temp/")
                                                                + ((CacheTemporaryFile)fileCache).OrginalFileName);
                            pageInsert.FileType = FileTypeModel.Native;
                        }
                        pageInsert.ContentLanguageCode = page.LanguageCode;
                        pageInsert.FileExtension =
                            Path.GetExtension(fileCache.OrginalFileName).Substring(1);//Remove dot sign (.)
                        pageInsert.OriginalFileName = fileCache.OrginalFileName;

                        doc.Pages.Add(pageInsert);
                }
                
                    //Init param to cache
                    var cTime = DateTime.Now.AddMinutes(24 * 3600);
                    var cExp = System.Web.Caching.Cache.NoSlidingExpiration;
                    var cPri = System.Web.Caching.CacheItemPriority.Normal;
                    var keyCache = Guid.NewGuid().ToString();
                    CacheImage cache =
                        new CacheImage
                        {
                            FileBinaries = doc.DocumentType.Icon,
                            ContentType = ContentTypeEnumeration.Image.PNG
                        };

                    List<CacheFileResult> list = new List<CacheFileResult>();
                    var sortedPages = doc.Pages.OrderBy(row => row.PageNumber).ToList();
                    var Pages = new List<dynamic>();
                    foreach (var page in sortedPages)
                    {
                        dynamic p = new ExpandoObject();
                        p.Width = page.Width;
                        List<AnnotationModel> anno = new List<AnnotationModel>();
                        var contentType = MimeMap.ContentTypeFromExtension(page.FileExtension);
                        var b = page.FileBinaries;
                        if (b == null)
                            return View();
                        //Cache Page
                        //Page trong DB neu la TIFF thi cung da dc split de luu vao
                        //Do do result tra ve sau khi CacheFile thuc su chi co 1 element
                        if (page.Annotations != null && page.FileType == FileTypeModel.Image)
                        {
                            var img = ProcessImages.ByteArrayToImage(page.FileBinaries);
                            img = ProcessImages.Rotate(img, (int)page.RotateAngle);
                            foreach (var a in page.Annotations)
                            {
                                if (a.Type == AnnotationTypeModel.Redaction &&
                                    !doc.DocumentType.AnnotationPermission.AllowedHideRedaction)
                                {
                                    a.Left *= img.Width / page.Width;
                                    a.Top *= img.Width / page.Width;
                                    a.Width *= img.Width / page.Width;
                                    a.Height *= img.Width / page.Width;
                                    img = ProcessImages.AddAnnotation(img, a);
                                }
                                else if ((a.Type == AnnotationTypeModel.Highlight
                                            && doc.DocumentType.AnnotationPermission.AllowedSeeHighlight)
                                     || (a.Type == AnnotationTypeModel.Text
                                            && doc.DocumentType.AnnotationPermission.AllowedSeeText)
                                     || (a.Type == AnnotationTypeModel.Redaction
                                            && doc.DocumentType.AnnotationPermission.AllowedHideRedaction))
                                {
                                    double angle = -1 * page.RotateAngle * Math.PI / 180;
                                    anno.Add(a);
                                }
                            }
                            b = ProcessImages.ImageToByteArray(ProcessImages.Rotate(img, (int)(360 - page.RotateAngle)));
                        }
                        var lst = CacheHelper.CacheFile(MimeMap.ContentTypeFromExtension(page.FileExtension),
                                                        b, page.OriginalFileName, Server.MapPath("~/Temp"));
                        p.RotateAngle = page.RotateAngle;
                        p.Info = lst.First();
                        p.Annotations = anno;
                        p.PageId = page.Id;
                        Pages.Add(p);
                    }

                    if (docCollection.FieldValues != null)
                    {
                        foreach (var fieldValue in docCollection.FieldValues)
                        {
                            if (fieldValue != null)
                            {
                                FieldValueModel fieldValueModel = doc.FieldValues.Single(p => p.Field.Id == fieldValue.Id);
                                fieldValueModel.Value = fieldValue.Value;

                                if (fieldValue.TableFieldValues != null && fieldValue.TableFieldValues.Count > 0)
                                {
                                    fieldValueModel.TableValues = new List<TableFieldValueModel>();

                                    foreach (var tableFieldValue in fieldValue.TableFieldValues)
                                    {
                                        TableFieldValueModel tableFieldValueModel = new TableFieldValueModel
                                        {
                                            FieldId = tableFieldValue.FieldId,
                                            RowNumber = tableFieldValue.RowIndex,
                                            Value = tableFieldValue.Value,
                                            Field = fieldValueModel.Field.Children.SingleOrDefault(p => p.FieldId == tableFieldValue.FieldId).Field
                                        };

                                        fieldValueModel.TableValues.Add(tableFieldValueModel);
                                    }
                                }
                            }
                        }
                    } 
                    
                    CapturedDocumentCaching capturedDocumentCaching = new CapturedDocumentCaching { DocId = doc.Id, DocumentTypeName = doc.DocumentType.Name, FieldValues = doc.FieldValues, Pages = Pages, TempId = docCollection .TempId};
                    captureModel.documentCachings.Add(capturedDocumentCaching);
                }
            }

            captureModel.CaptureDocumentTypes = captureDocumentTypes;
            return View(captureModel);
        }

        [HttpPost]
        public JsonResult Insert(DocumentCollection model)
        {
                if (model != null && model.Documents.First() != null && model.Documents.First().DocumentTypeId != Guid.Empty)
                {
                    var docUploadeds = model.Documents;
                    DocumentProvider docProvider = new DocumentProvider(Utilities.UserName, Utilities.Password);
                    List<DocumentModel> listDocItem = new List<DocumentModel>();
                    List<PageModel> listPageItem = new List<PageModel>();
                    DocumentTypeProvider docTypeProvider = new DocumentTypeProvider(Utilities.UserName, Utilities.Password);
                    try
                    {
                        foreach (var docUploaded in docUploadeds)
                        {
                            var docType = docTypeProvider.GetDocumentType(docUploaded.DocumentTypeId);
                            DocumentModel docInsert = new DocumentModel(DateTime.Now, Utilities.UserName, docType);
                            var BinaryType = FileTypeModel.Native;
                            FileTypeModel BinaryTypeBefore;
                            var isCompound = false;

                            if (docUploaded.FieldValues != null)
                            {
                                foreach (var fieldValue in docUploaded.FieldValues)
                                {
                                    if (fieldValue != null)
                                    {
                                        FieldValueModel fieldValueModel = docInsert.FieldValues.Single(p => p.Field.Id == fieldValue.Id);
                                        fieldValueModel.Value = fieldValue.Value;

                                        if (fieldValue.TableFieldValues != null && fieldValue.TableFieldValues.Count > 0)
                                        {
                                            fieldValueModel.TableValues = new List<TableFieldValueModel>();

                                            foreach (var tableFieldValue in fieldValue.TableFieldValues)
                                            {
                                                TableFieldValueModel tableFieldValueModel = new TableFieldValueModel
                                                {
                                                    FieldId = tableFieldValue.FieldId,
                                                    RowNumber = tableFieldValue.RowIndex,
                                                    Value = tableFieldValue.Value,
                                                    Field = fieldValueModel.Field.Children.SingleOrDefault(p => p.FieldId == tableFieldValue.FieldId).Field
                                                };

                                                fieldValueModel.TableValues.Add(tableFieldValueModel);
                                            }
                                        }
                                    }
                                }
                            }
                            foreach (var page in docUploaded.Pages)
                            {
                                PageModel pageInsert = new PageModel();
                                CacheObject fileCache = (CacheObject)System.Web.HttpContext.Current.Cache[page.ImgKey];
                                if (fileCache is CacheImage)
                                {
                                    Size size = ProcessImages.GetImageSize(((CacheImage)fileCache).FileBinaries);
                                    pageInsert.FileBinaries = ((CacheImage)fileCache).FileBinaries;
                                    pageInsert.FileType = FileTypeModel.Image;
                                    pageInsert.Width = page.PageWidth;
                                    pageInsert.Height = page.PageHeight;
                                    pageInsert.RotateAngle = page.RotateAngle;
                                    if (page.Annotations != null)
                                    {
                                        pageInsert.Annotations = page.Annotations;
                                        pageInsert.Annotations.ForEach(delegate(AnnotationModel annotation)
                                        {
                                            annotation.ModifiedBy = annotation.CreatedBy = Utilities.UserName;
                                            annotation.ModifiedOn = annotation.CreatedOn = DateTime.Now;
                                        });
                                    }
                                    BinaryType = FileTypeModel.Image;
                                }
                                else
                                {
                                    pageInsert.FileBinaries = System.IO.File.ReadAllBytes(Server.MapPath("~/Temp/")
                                                                        + ((CacheTemporaryFile)fileCache).OrginalFileName);
                                    pageInsert.FileType = FileTypeModel.Native;
                                    BinaryType = FileTypeModel.Native;
                                }
                                pageInsert.ContentLanguageCode = page.LanguageCode;
                                pageInsert.FileExtension =
                                    Path.GetExtension(fileCache.OrginalFileName).Substring(1);//Remove dot sign (.)
                                pageInsert.OriginalFileName = fileCache.OrginalFileName;
                                docInsert.Pages.Add(pageInsert);
                                if (docUploaded.Pages.IndexOf(page) == 0)
                                    BinaryTypeBefore = BinaryType;
                                else
                                    BinaryTypeBefore = docInsert.Pages[docUploaded.Pages.IndexOf(page) - 1].FileType;
                                if (!isCompound)
                                    isCompound = BinaryType != BinaryTypeBefore;
                            }

                            docInsert.BinaryType = isCompound ? FileTypeModel.Compound : BinaryType;
                            listDocItem.Add(docInsert);
                        }
                    }
                    catch(Exception ex){
                        ExceptionLog(ex, ex.Message);
                        return Json(new JsonMessage()
                        {
                            Code = MsgCode.Error,
                            Message = "Insert document failed!"
                        });
                    }


                try
                {
                    docProvider.InsertDocuments(listDocItem);

                    Utilities.CacheCaptureDocuments = null;
                    CacheHelper.DeleteCache(model.GetImageKeys());
                    Utilities.DeleteFile(Server.MapPath("~/Temp/Snipped"));

                    return Json(new JsonMessage()
                    {
                        Code = MsgCode.Success,
                        Message = "Insert document successful!"
                    });

                }catch(Exception e){
                    ExceptionLog(e, e.Message);
                    return Json(new JsonMessage() {
                        Code = MsgCode.Error,
                        Message = e.Message
                    });
                }
            }

            return Json(new JsonMessage() {
                Code = MsgCode.Error , 
                Message = "Insert document failed!"
            });
        }
        
        public JsonResult GetDocType()
        {
            string id;
            var cTime = DateTime.Now.AddMinutes(24 * 3600);
            var cExp = System.Web.Caching.Cache.NoSlidingExpiration;
            var cPri = System.Web.Caching.CacheItemPriority.Normal;
            DocumentTypeProvider _DocumentTypeProvider = new DocumentTypeProvider(Utilities.UserName,Utilities.Password);
            IList<DocumentTypeModel> _DocumentTypeModelList = new List<DocumentTypeModel>();
            try
            {
                _DocumentTypeModelList = _DocumentTypeProvider.GetCapturedDocumentTypes();
            }
            catch (Exception e){
                ExceptionLog(e, e.Message);
            }
            List<DocTypeResult> listDocTypeRs = new List<DocTypeResult>();

            foreach (var item in _DocumentTypeModelList)
            {
                
                if (item.Icon != null)
                {
                    id = Guid.NewGuid().ToString();
                    System.Web.HttpContext.Current.Cache.Add(id, item.Icon, null, cTime, cExp, cPri, null);
                }
                else
                {
                    id = String.Empty;
                }
                var docTypeRemovedIcon = item;
                /*** Can luoc bo mot so thuoc tinh de tang toc do tai */
                docTypeRemovedIcon.Icon = null;
                listDocTypeRs.Add(new DocTypeResult { DocType = new DocumentTypeModel { 
                    Name = item.Name, Id = item.Id, Fields = item.Fields }, IconKey = id });
            }

            //System.Web.HttpContext.Current.Cache.Add(cacheKey, listDocTypeRs, null, cTime, cExp, cPri, null);
            return Json(listDocTypeRs, JsonRequestBehavior.AllowGet);
        }

        //[HttpPost]
        public ActionResult OCRDocument(DocumentSerializable doc)
        {
            OCRHelper ocrHelper = new OCRHelper();
            DocumentTypeProvider docTypeProvider = new DocumentTypeProvider(Utilities.UserName, Utilities.Password);
            DocumentModel docModel = new DocumentModel(DateTime.Now, Utilities.UserName, docTypeProvider.GetDocumentType(doc.DocumentTypeId));

            if (Utilities.CacheCaptureDocuments != null && Utilities.CacheCaptureDocuments.Documents.SingleOrDefault(p => p.TempId == doc.TempId) != null)
            {
                DocumentSerializable document = Utilities.CacheCaptureDocuments.Documents.SingleOrDefault(p => p.TempId == doc.TempId);

                foreach (FieldValueModel fieldValue in docModel.FieldValues)
                {
                    FieldValueSerializable fieldValueSerial = document.FieldValues.SingleOrDefault(p => p.Id == fieldValue.Field.Id);
                    if (fieldValueSerial != null)
                    {
                        fieldValue.Value = fieldValueSerial.Value;
                    }
                }

            }
            else
            {
                docModel.BinaryType = FileTypeModel.Image;

                foreach (var page in doc.Pages)
                {
                    PageModel pageModel = new PageModel();
                    pageModel.ContentLanguageCode = page.LanguageCode;
                    CacheObject fileCache = (CacheObject)System.Web.HttpContext.Current.Cache[page.ImgKey];
                    if (fileCache is CacheImage)
                    {
                        pageModel.FileBinaries = ((CacheImage)fileCache).FileBinaries;
                    }
                    docModel.Pages.Add(pageModel);
                }

                GetFieldValues(docModel);

                try
                {
                    if (docModel.DocumentType.HasOCRTemplateDefined)
                    {
                        ocrHelper.DoOCR(docModel);
                    }
                }
                catch (Exception e)
                {
                    ExceptionLog(e, e.Message);
                    ViewData["ERROR"] = e.Message;
                }            
            }

            return PartialView("OCR", docModel.FieldValues);

        }

        public FileResult Get(string key)
        {
            var f = (CacheFilesBinary)System.Web.HttpContext.Current.Cache[key];
            return File(f.FileBinaries, f.ContentType, f.OrginalFileName);
        }

        public JsonResult SendMail(SaveOption mailOptions)
        {
            //mail.SMTP = new SmtpClient { EnableSsl = true };
            var doc = mailOptions.Document;
            var b = ProcessImages.ConvertTo(doc, mailOptions.Format, mailOptions.Range, mailOptions.Pages);
            var fileName = Server.MapPath("~/Temp/") + Guid.NewGuid().ToString() + "." + mailOptions.Format;
            System.IO.File.WriteAllBytes(fileName, b);

            SmtpClient smtp = new SmtpClient();

            MailMessage mail = new MailMessage();
            Attachment attch = new Attachment(fileName);
            mail.Attachments.Add(attch);
            mail.IsBodyHtml = true;

            if (string.IsNullOrEmpty(mailOptions.Subject))
            {
                mail.Subject = "Archive Notification";
            }
            else
            {
                mail.Subject = mailOptions.Subject;
            }

            if (string.IsNullOrEmpty(mailOptions.Body))
            {
                mail.Body = "No reply this email";
            }
            else
            {
                mail.Body = mailOptions.Body;
            }

            if (mailOptions.MailTo != null)
            {
                var to = mailOptions.MailTo.Split(';');
                for (int i = 0; i < to.Length; i++)
                {
                    mail.To.Add(to[i]);
                }
            }

            if (mailOptions.CC != null)
            {
                var cc = mailOptions.CC.Split(';');
                for (int i = 0; i < cc.Length; i++)
                {
                    mail.CC.Add(cc[i]);
                }
            }

            if (mailOptions.BCC != null)
            {
                var bcc = mailOptions.BCC.Split(';');
                for (int i = 0; i < bcc.Length; i++)
                {
                    mail.Bcc.Add(bcc[i]);
                }
            }

            try
            {
                smtp.Send(mail);
            }
            catch(Exception ex) {
                ExceptionLog(ex, ex.Message);
                return Json("Failed to send your email!"); 
            }

            return Json("Your email has been send");

        }

        public ActionResult SaveLocal(SaveOption mailOptions)
        {
            var document = mailOptions.Document;
            var buffer = ProcessImages.ConvertTo(document, mailOptions.Format, mailOptions.Range, mailOptions.Pages);
            var key = Guid.NewGuid().ToString();
            try
            {
                CacheFilesBinary cache = new CacheFilesBinary
                {
                    FileBinaries = buffer,
                    ContentType = (mailOptions.Format.Equals("pdf") ? ArchiveMVC5.Utility.ContentTypeEnumeration.Document.PDF :
                                                          ArchiveMVC5.Utility.ContentTypeEnumeration.Image.TIFF),
                    OrginalFileName = key + "." + mailOptions.Format
                };
                System.Web.HttpContext.Current.Cache.Add(key,
                    cache, null, DateTime.Now.AddMinutes(15),
                    System.Web.Caching.Cache.NoSlidingExpiration,
                    System.Web.Caching.CacheItemPriority.Default,
                    null);
            }
            catch (Exception ex)
            {
                ExceptionLog(ex, ex.Message);
                throw ex;
            }

            return Json(key);
        }

        public ActionResult LoadToolbar(string id)
        {
            DocumentTypeProvider provider = new DocumentTypeProvider(Utilities.UserName, Utilities.Password);

            if (id != string.Empty)
            {
                Guid docTypeId = Guid.Parse(id);
                //var docType = Utilities.CaptureDocumentTypes.Where(d => d.Id == docTypeId).First();
                var docType = provider.GetCapturedDocumentTypes().Where(d => d.Id == docTypeId).First();
                if (docType != null)
                {
                    ViewData[Constant.KEY_ANOTATION_PERMISSION] = docType.AnnotationPermission;
                    ViewData[Constant.KEY_DOCTYPE_PERMISSION] = docType.DocumentTypePermission;
                }
            }
            else
            {
                ViewData[Constant.KEY_ANOTATION_PERMISSION] = null;
                ViewData[Constant.KEY_DOCTYPE_PERMISSION] = null;
            }

            return PartialView("Toolbar");
        }

        public ActionResult LoadTableFieldValue(FieldSerializable fieldData)
        {
            DocumentTypeProvider docTypeProvider = new DocumentTypeProvider(Utilities.UserName, Utilities.Password);
            DocumentTypeModel docType = docTypeProvider.GetDocumentType(fieldData.DocumentTypeId);
            FieldMetaDataModel field = docType.Fields.SingleOrDefault(p => p.Id == fieldData.FieldId);

            if (field.Children != null && field.Children.Count > 0)
            {
                return PartialView("TableField", field.Children);
            }

            return PartialView("TableField");
        }

        public JsonResult PersitCaptureDocument(DocumentCollection model)
        {
            Utilities.CacheCaptureDocuments = model;

            return Json(new JsonMessage()
            {
                Code = MsgCode.Success,
                Message = "Cache captured documents successful!"
            }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult LookupData(LookupInfoModel lookupInfo, string text)
        {
            LookupProvider lookupProvider = new LookupProvider(Utilities.UserName, Utilities.Password);
            DataTable lookupData = lookupProvider.GetLookupData(lookupInfo, text);

            return PartialView("LookupData", lookupData);
        }
        
        private void GetFieldValues(DocumentModel doc)
        {

            foreach (var field in doc.DocumentType.Fields)
            {
                if (field.IsSystemField)
                {
                    continue;
                }

                var fieldValueModel = doc.FieldValues.SingleOrDefault(p => p.Field.Id == field.Id);

                //get size of template from field
                if (field.OCRTemplateZone != null)
                {
                    var templatePage = doc.DocumentType.OCRTemplate.OCRTemplatePages.Single(
                        p => p.Id == field.OCRTemplateZone.OCRTemplatePageId);
                    try
                    {
                        var size = ProcessImages.GetImageSize(templatePage.Binary);
                        var dpi = ProcessImages.GetHorizontalResolution(doc.Pages[templatePage.PageIndex].FileBinaries);
                        var rate = 1;
                        fieldValueModel.Field.OCRTemplateZone.Left =
                            ProcessImages.ToPixel(fieldValueModel.Field.OCRTemplateZone.Left, dpi) * rate;
                        fieldValueModel.Field.OCRTemplateZone.Top =
                            ProcessImages.ToPixel(fieldValueModel.Field.OCRTemplateZone.Top, dpi) * rate;
                        fieldValueModel.Field.OCRTemplateZone.Width =
                            ProcessImages.ToPixel(fieldValueModel.Field.OCRTemplateZone.Width, dpi) * rate;
                        fieldValueModel.Field.OCRTemplateZone.Height =
                            ProcessImages.ToPixel(fieldValueModel.Field.OCRTemplateZone.Height, dpi) * rate;
                    }
                    catch(Exception ex)
                    {
                        ExceptionLog(ex, ex.Message);
                        continue;
                    }
                }
            }
        }
    }
}
