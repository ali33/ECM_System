using ArchiveMVC5.Controllers;
using ArchiveMVC5.Models;
using ArchiveMVC5.Models.DataProvider;
using ArchiveMVC5.Utility;
using log4net;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ArchiveMVC5.Controllers
{
    public class ViewController : BaseController
    {
        public ActionResult Index(Guid id, bool partal = false)
        {
            DocumentModel doc = null;
            DocumentProvider docProvider = new DocumentProvider(Utilities.UserName, Utilities.Password);
            PermissionProvider pp = new PermissionProvider(Utilities.UserName, Utilities.Password);

            if(Utilities.OpenedDocuments.SingleOrDefault(p=> p.Id == id) != null)
            {
                doc = Utilities.OpenedDocuments.SingleOrDefault(p=> p.Id == id);
            }
            else
            {
                doc = docProvider.GetDocument(id);
                Utilities.OpenedDocuments.Add(doc);
            }

            Utilities.OpenningDocument = doc;

            if (doc != null && !doc.DocumentType.DocumentTypePermission.AllowedSearch)
            {
                ViewData[Constant.KEY_ERROR] = true;
                return View();
            }
            ViewData[Constant.KEY_ANOTATION_PERMISSION] = doc.DocumentType.AnnotationPermission;
            ViewData[Constant.KEY_DOCTYPE_PERMISSION] = doc.DocumentType.DocumentTypePermission;
            //Init param to cache
            var cTime = DateTime.Now.AddMinutes(24 * 3600);
            var cExp = System.Web.Caching.Cache.NoSlidingExpiration;
            var keyCache = Guid.NewGuid().ToString();
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

                page.FileType = Utilities.GetFileType(page.FileExtension);
                p.FileType = page.FileType;
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
                    p.RotateAngle = page.RotateAngle;
                    p.Annotations = anno;
                }
                var lst = CacheHelper.CacheFile(MimeMap.ContentTypeFromExtension(page.FileExtension),
                                                b, page.OriginalFileName, Server.MapPath("~/Temp"));
                p.Info = lst.First();
                p.PageId = page.Id;
                Pages.Add(p);
            }

            foreach (var field in doc.DocumentType.Fields)
            {
                if (doc.FieldValues.FirstOrDefault(p => p.Field.Id == field.Id) == null)
                {
                    var fieldValue = new FieldValueModel { Field = field, Value = field.DefaultValue };

                    if (field.DataType == Ecm.Domain.FieldDataType.Date && field.UseCurrentDate)
                    {
                        fieldValue.Value = DateTime.Now.ToShortDateString();
                    }

                    doc.FieldValues.Add(fieldValue);
                }
            }

            ViewData[Constant.KEY_PAGE_LIST] = Pages;

            TempData[Constant.KEY_FIELDS_VALUE] = doc.FieldValues;
            ViewData[Constant.KEY_DOCUMENT_ID] = doc.Id;
            ViewData[Constant.KEY_DOCUMENT_NAME] = doc.DocumentType.Name;
            
            if (partal)
                return PartialView();
            return View();
        }

        public ActionResult GetHtml(String key)
        {
            CacheTemporaryFile file = (CacheTemporaryFile)System.Web.HttpContext.Current.Cache[key];
            if (file != null && file.FileName != null)
            {
                var _path = "~/Temp/" + System.IO.Path.GetFileName(file.FileName);
                ViewData["PATH"] = _path;
            }
            return PartialView();
        }

        public ActionResult GetDocument(string key)
        {
            CacheTemporaryFile file = (CacheTemporaryFile)System.Web.HttpContext.Current.Cache[key];
            if (file != null && file.FileName != null)
            {
                var path = "~/Temp/" + System.IO.Path.GetFileName(file.FileName);
                var filename = file.OrginalFileName;
                if (filename.Contains("pdf") || filename.Contains("PDF"))
                {
                    ViewData["FILE_TYPE"] = "PDF";
                }
                ViewData["PATH"] = path;

            }
            return PartialView();
        }

        public FileStreamResult ViewPdf(string path)
        {
            if (!String.IsNullOrEmpty(path))
            {
                FileStream result = new FileStream(Server.MapPath(path), FileMode.Open, FileAccess.Read);
                return File(result, "application/pdf");
            }
            else
            {
                return null;
            }
        }

        public ActionResult Download(Guid docId, int pageId)
        {
            try
            {
                DocumentProvider docProvider = new DocumentProvider(Utility.Utilities.UserName, Utility.Utilities.Password);
                var doc = docProvider.GetDocument(docId);
                var page = doc.Pages[pageId];
                byte[] binaries = page.FileBinaries;
                return File(binaries, MimeMap.ContentTypeFromExtension(page.FileExtension), page.OriginalFileName);
            }
            catch(Exception ex) {
                ExceptionLog(ex, ex.Message);
            }
            //File not found
            return View();
        }

        public ActionResult ImageViewer(string key)
        {
            CacheObject image = (CacheObject)System.Web.HttpContext.Current.Cache[key];
            if (typeof(CacheImage).Equals(image.GetType()))
            {
                ViewData["ImageKey"] = key;
                return PartialView();
            }
            return RedirectToAction("Index", "Login");
        }

        public class DocumentCaching
        {
            public DocumentModel Document { set; get; }
            public string IconKey { set; get; }
            public List<string> ImagePageKey { set; get; }
        }

        public ActionResult GetFieldValues(DocumentSerializable doc)
        {
            if (TempData[Constant.KEY_FIELDS_VALUE] != null)
            {
                return PartialView("OCR", (List<FieldValueModel>)TempData[Constant.KEY_FIELDS_VALUE]);
            }

            return null;
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

        public ActionResult LoadTableFieldValue(DocumentFieldValueSerializable para)
        {
            FieldValueModel fieldValue = new FieldValueModel();
            DocumentProvider documentProvider = new DocumentProvider(Utilities.UserName, Utilities.Password);
            DocumentModel document = documentProvider.GetDocument(para.DocumentId);

            fieldValue = document.FieldValues.SingleOrDefault(p => p.Field.Id == para.FieldId);

            if (fieldValue == null)
            {
                var field = document.DocumentType.Fields.SingleOrDefault(p => p.Id == para.FieldId);
                fieldValue = new FieldValueModel { Field = field, Value = field.DefaultValue };
            }

            return PartialView("TableField", fieldValue);
        }

        [HttpPost]
        public JsonResult Update(DocumentCollection model)
        {
            var docUploaded = model.Documents[0];
            DocumentProvider docProvider = new DocumentProvider(Utilities.UserName, Utilities.Password);
            List<DocumentModel> listDocItem = new List<DocumentModel>();
            List<PageModel> listPageItem = new List<PageModel>();
            DocumentTypeProvider docTypeProvider = new DocumentTypeProvider(Utilities.UserName, Utilities.Password);
            DocumentModel docUpdate = docProvider.GetDocument(docUploaded.DocumentId);
            List<PageModel> pages = docUpdate.Pages;

            docUpdate.DeletedPages = new List<Guid>();
            docUpdate.PageCount = docUploaded.Pages.Count;
            var BinaryType = FileTypeModel.Native;
            FileTypeModel BinaryTypeBefore;
            var isCompound = false;

            foreach (var fieldValue in docUploaded.FieldValues)
            {
                FieldValueModel fieldValueModel = docUpdate.FieldValues.SingleOrDefault(p => p.Field.Id == fieldValue.Id);

                if (fieldValueModel == null)
                {
                    continue;
                }

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

            #region Update, append or replace page base on docData.Pages

            foreach (var pageUploaded in docUploaded.Pages)
            {
                var pageNumber = docUploaded.Pages.IndexOf(pageUploaded);
                var page = docUploaded.Pages.ElementAtOrDefault(pageNumber);
                var pageUpdateOrNew = new PageModel();

                if (page.PageId != Guid.Empty)
                {
                    pageUpdateOrNew = docUpdate.Pages.FirstOrDefault(row => row.Id == page.PageId);
                    pageUpdateOrNew.PageNumber = pageNumber;
                }
                else
                {
                    pageUpdateOrNew.PageNumber = pageNumber - 1;
                }

                CacheObject fileCache = (CacheObject)System.Web.HttpContext.Current.Cache[pageUploaded.ImgKey];

                if (fileCache is CacheImage)
                {
                    if (pageUpdateOrNew.Id == Guid.Empty)
                    {
                        pageUpdateOrNew.FileBinaries = ((CacheImage)fileCache).FileBinaries;
                        pageUpdateOrNew.ContentLanguageCode = pageUploaded.LanguageCode;
                        pageUpdateOrNew.FileExtension = Path.GetExtension(fileCache.OrginalFileName).Substring(1);//Remove dot sign (.)
                        pageUpdateOrNew.OriginalFileName = fileCache.OrginalFileName;
                    }

                    pageUpdateOrNew.FileType = FileTypeModel.Image;
                    pageUpdateOrNew.Width = pageUploaded.PageWidth;
                    pageUpdateOrNew.Height = pageUploaded.PageHeight;
                    pageUpdateOrNew.RotateAngle = pageUploaded.RotateAngle;
                    pageUpdateOrNew.Annotations = new List<AnnotationModel>();

                    if (pageUploaded.Annotations != null)
                    {
                        pageUploaded.Annotations.ForEach(delegate(AnnotationModel annotation)
                        {
                            annotation.CreatedBy = annotation.ModifiedBy = Utilities.UserName;
                            annotation.ModifiedOn = annotation.CreatedOn = DateTime.Now;
                            pageUpdateOrNew.Annotations.Add(annotation);
                        });
                    }

                    BinaryType = FileTypeModel.Image;
                }
                else
                {
                    pageUpdateOrNew.FileBinaries = System.IO.File.ReadAllBytes(Server.MapPath("~/Temp/") + ((CacheTemporaryFile)fileCache).OrginalFileName);
                    pageUpdateOrNew.FileType = FileTypeModel.Native;
                    BinaryType = FileTypeModel.Native;
                }

                if (pageUpdateOrNew.Id == Guid.Empty)
                {
                    docUpdate.Pages.Add(pageUpdateOrNew);
                }


                if (docUploaded.Pages.IndexOf(pageUploaded) == 0)
                {
                    BinaryTypeBefore = BinaryType;
                }
                else
                {
                    BinaryTypeBefore = docUpdate.Pages[docUploaded.Pages.IndexOf(pageUploaded) - 1].FileType;
                }

                if (!isCompound)
                {
                    isCompound = BinaryType != BinaryTypeBefore;
                }
            }
            #endregion

            var listPageToDelete = new List<Guid>();

            foreach (var pageDelete in docUpdate.Pages)
            {
                var pageUploaded = docUploaded.Pages.FirstOrDefault(row => row.PageId == pageDelete.Id);

                if (pageUploaded == null)
                {
                    docUpdate.DeletedPages.Add(pageDelete.Id);
                }
            }

            var sortedPages = docUpdate.Pages.OrderBy(row => row.PageNumber);
            docUpdate.Pages = sortedPages.ToList();
            listDocItem.Add(docUpdate);
            //docModel.BinaryType = isCompound ? FileTypeModel.Compound : BinaryType;

            try
            {
                docProvider.UpdateDocuments(listDocItem);
                CacheHelper.DeleteCache(model.GetImageKeys());
                Utilities.DeleteFile(Server.MapPath("~/Temp/Snipped"));

                DocumentModel updatedDoc = docProvider.GetDocument(listDocItem[0].Id);
                var exitingDoc = Utilities.OpenedDocuments.SingleOrDefault(i => i.Id == listDocItem[0].Id);

                if (exitingDoc != null)
                {
                    exitingDoc = updatedDoc;
                }

                Utilities.OpenningDocument = updatedDoc;


                return Json(new JsonMessage()
                {
                    Code = MsgCode.Success,
                    Message = "Update document successful!"
                });
            }
            catch (Exception e)
            {
                ExceptionLog(e, e.Message);
                return Json(new JsonMessage()
                {
                    Code = MsgCode.Error,
                    Message = "Update document failed!"
                });

            }
        }

        public ActionResult CloseAllContentItem()
        {
            Utilities.OpenedDocuments.Clear();
            return RedirectToAction("Index", "Search");
        }

        public ActionResult CloseOtherContentItem()
        {
            Utilities.OpenedDocuments.Clear();
            Utilities.OpenedDocuments.Add(Utilities.OpenningDocument);

            return RedirectToAction("Index", "View", new { id = Utilities.OpenningDocument.Id, partal = false });
        }

        public ActionResult CloseContentItem(Guid id)
        {
            Utilities.OpenedDocuments.Remove(Utilities.OpenedDocuments.SingleOrDefault(p => p.Id == id));

            if (Utilities.OpenedDocuments.Count > 0)
            {
                return RedirectToAction("Index", "View", new { id = Utilities.OpenedDocuments.FirstOrDefault().Id, partal = false });
            }
            else
            {
                return RedirectToAction("Index", "Search");
            }
        }
    }
}
