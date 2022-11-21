using ArchiveMVC.Models;
using ArchiveMVC.Models.DataProvider;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace ArchiveMVC.Utility
{
    public class Utilities
    {
        public static string UserName {
            get
            {
                try
                {
                    return GetSession(Constant.UserName).ToString();
                }
                catch (System.NullReferenceException e) { 
                    return null; 
                }
            }
            set
            {
                SetSession(Constant.UserName, value);
            }
        }
        public static string Password
        {
            get
            {
                try
                {
                    return GetSession(Constant.Password).ToString();
                }
                catch (System.NullReferenceException e) {
                    return null;
                }
            }
            set
            {
                SetSession(Constant.Password, value);
            }
        }

        public static object GetSession(string key)
        {
            try
            {
                return System.Web.HttpContext.Current.Session[key];
            }
            catch (System.NullReferenceException e)
            {
                return null;
            }
        }

        public static void SetSession(string key, object value)
        {
            System.Web.HttpContext.Current.Session[key] = value;
        }

        public static bool IsAdmin { get; set; }

        //List Document user on permission
        public static IList<DocumentTypeModel> CaptureDocumentTypes { set; get; }
        public static IList<DocumentTypeModel> SearchDocumentTypes() 
        {
            DocumentTypeProvider provider = new DocumentTypeProvider(UserName, Password);
            return provider.GetDocumentTypes();
        }
        
        public static DocumentTypeModel DocumentType(Guid id)
        {
            DocumentTypeProvider provider = new DocumentTypeProvider(UserName, Password);
            return provider.GetDocumentType(id);
        }

        /// <summary>
        /// Lấy danh sách các FieldValues của document
        /// Nếu chưa OCR FieldValues của document null thì lấy danh sách OCRZone
        /// </summary>
        /// <param name="doc"></param>
        /// <returns></returns>
        //public static IList<FieldValueModel> FieldValues(DocumentModel doc)
        //{
        //    List<FieldValueModel> fieldValues = new List<FieldValueModel>();

        //    foreach (var field in doc.DocumentType.Fields)
        //    {
        //        if (field.IsSystemField)
        //        {
        //            continue;
        //        }

        //        var fieldValueModel = new FieldValueModel();
        //        fieldValueModel.Field = field;

        //        get size of template from field
        //        if (field.OCRTemplateZone != null)
        //        {
        //            var templatePage = doc.DocumentType.OCRTemplate.OCRTemplatePages.Single(
        //                p => p.Id == field.OCRTemplateZone.OCRTemplatePageId);
        //            try
        //            {
        //                var size = ProcessImages.GetImageSize(templatePage.Binary);
        //                var dpi = ProcessImages.GetHorizontalResolution(doc.Pages[templatePage.PageIndex].FileBinaries);
        //                var rate = 1;// (float)Constant.LIMIT_WIDTH_OF_PAGE_IMAGE / size.Width;
        //                ViewData[_field.Field.Id.ToString()] = templatePage.PageIndex;
        //                fieldValueModel.Field.OCRTemplateZone.Left =
        //                    ProcessImages.ToPixel(fieldValueModel.Field.OCRTemplateZone.Left, dpi) * rate;
        //                fieldValueModel.Field.OCRTemplateZone.Top =
        //                    ProcessImages.ToPixel(fieldValueModel.Field.OCRTemplateZone.Top, dpi) * rate;
        //                fieldValueModel.Field.OCRTemplateZone.Width =
        //                    ProcessImages.ToPixel(fieldValueModel.Field.OCRTemplateZone.Width, dpi) * rate;
        //                fieldValueModel.Field.OCRTemplateZone.Height =
        //                    ProcessImages.ToPixel(fieldValueModel.Field.OCRTemplateZone.Height, dpi) * rate;
        //            }
        //            catch
        //            {
        //                continue;
        //            }
        //        }


        //        fieldValueModel.Value = doc.FieldValues.SingleOrDefault(p => p.Field.Id == fieldValueModel.Field.Id) == null ? string.Empty : doc.FieldValues.SingleOrDefault(p => p.Field.Id == fieldValueModel.Field.Id).Value;

        //        fieldValues.Add(fieldValueModel);
        //    }
            
        //    return fieldValues;
        //}
        
        public static void DeleteFile(string path)
        {
            System.IO.DirectoryInfo dirInfo = new DirectoryInfo(path);

            foreach (FileInfo file in dirInfo.GetFiles())
            {
                file.Delete();
            }
            foreach (DirectoryInfo dir in dirInfo.GetDirectories())
            {
                dir.Delete(true);
            }
        }

        public static string JsonSerializer(object obj)
        {
            return JsonConvert.SerializeObject(obj, Formatting.Indented);
        }
        public static string DocumentTypeCapturePermissionJson()
        {
            Dictionary<string, DocumentTypePermissionModel> docTypePermission 
                = new Dictionary<string,DocumentTypePermissionModel>();
            foreach (var docType in Utilities.CaptureDocumentTypes)
            {
                docTypePermission.Add(docType.Id.ToString(), docType.DocumentTypePermission);
            }
            return JsonConvert.SerializeObject(docTypePermission,Formatting.Indented);
        }
        public static string AnnotationPermissionJson()
        {
            Dictionary<string, DocumentTypePermissionModel> annotationPermission
                = new Dictionary<string, DocumentTypePermissionModel>();
            foreach (var docType in Utilities.CaptureDocumentTypes)
            {
                annotationPermission.Add(docType.Id.ToString(), docType.DocumentTypePermission);
            }
            return JsonConvert.SerializeObject(annotationPermission, Formatting.Indented);
        }
        public static string DocumentTypeCaptureResultJson()
        {
            DocumentTypeProvider p = new DocumentTypeProvider(UserName, Password);
            //var docType = p.GetCapturedDocumentTypes();
            //var listDocType = new List<DocumentTypeModel>();
            //foreach (var item in docType)
            //{
            //    listDocType.Add(new DocumentTypeModel { Id = item.Id, Name = item.Name });
            //}
            return DocumentTypeResultJson(p.GetCapturedDocumentTypes());
        }
        public static string DocumentTypeResultJson()
        {
            DocumentTypeProvider p = new DocumentTypeProvider(UserName, Password);
            //var docType = p.GetDocumentTypes();
            //var listDocType = new List<DocumentTypeModel>();
            //foreach (var item in docType)
            //{
            //    listDocType.Add(new DocumentTypeModel { Id = item.Id, Name = item.Name, 
            //                    Icon = item.Icon, Fields = item.Fields,  });
            //}
            return DocumentTypeResultJson(p.GetDocumentTypes());
        }
        private static string DocumentTypeResultJson(IList<DocumentTypeModel> documents)
        {
            string id;
            var cTime = DateTime.Now.AddMinutes(24 * 3600);
            var cExp = System.Web.Caching.Cache.NoSlidingExpiration;
            var cPri = System.Web.Caching.CacheItemPriority.Normal;
            List<DocTypeResult> listDocTypeRs = new List<DocTypeResult>();

            foreach (var item in documents)
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
                listDocTypeRs.Add(new DocTypeResult
                {
                    DocType = new DocumentTypeModel
                    {
                        Name = item.Name,
                        Id = item.Id,
                        Fields = item.Fields
                    },
                    IconKey = id
                });
            }
            return JsonConvert.SerializeObject(listDocTypeRs, Formatting.Indented);
        }
        
        //Loc Ngo
        public static String RawPassword { get; set; }
        
        public static List<DocumentModel> OpenedDocuments
        {
            get
            {
                if (HttpContext.Current.Session["OpenedDocuments"] == null)
                {
                    OpenedDocuments = new List<DocumentModel>();
                }

                return (List<DocumentModel>)HttpContext.Current.Session["OpenedDocuments"];
            }
            set
            {
                HttpContext.Current.Session["OpenedDocuments"] = value;
            }
        }

        public static DocumentModel OpenningDocument
        {
            get
            {
                if (HttpContext.Current.Session["OpenningDocument"] == null)
                {
                    OpenningDocument = new DocumentModel();
                }

                return (DocumentModel)HttpContext.Current.Session["OpenningDocument"];
            }
            set
            {
                HttpContext.Current.Session["OpenningDocument"] = value;
            }
        }

        public static DocumentCollection CacheCaptureDocuments
        {
            get
            {
                if (HttpContext.Current.Session["CacheCaptureDocuments"] == null)
                {
                    CacheCaptureDocuments = null;
                }

                return (DocumentCollection)HttpContext.Current.Session["CacheCaptureDocuments"];
            }
            set
            {
                HttpContext.Current.Session["CacheCaptureDocuments"] = value;
            }
        }
    }
}