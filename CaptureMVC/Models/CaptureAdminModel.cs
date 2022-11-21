using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Ecm.CaptureDomain;

namespace CaptureMVC.Models
{
    public class CaptureAdminModel
    {
         public CaptureAdminModel()
         {
             ListBatchType = new Dictionary<string, BatchType>();
             ListDocType = new Dictionary<string, DocumentType>();
         }
         public Dictionary<string, BatchType> ListBatchType = new Dictionary<string, BatchType>();
         public Dictionary<string, DocumentType> ListDocType = new Dictionary<string, DocumentType>();
         public Guid BatchId { get; set; }
         public Guid DocumentId { get; set; }
         public IList<string> ListDataType { get; set; }
         public string TableValue { get; set; }
         public BatchType CaptureBatchTypeModel { get; set; }
         public DocumentType CaptureDocTypeModel { get; set; }
         public IList<BatchType> ListBatchTypeModel { get; set; }
         public IList<DocumentType> ListDocTypeModel { get; set; }
         public IList<UserGroup> ListUserGroup { get; set; }
         public User CaptureUserModel { get; set; }
         public IList<Language> ListLanguages { get; set; }
         public DocumentTypePermission DocTypePermission { get; set; }
         public AnnotationPermission AnnotationPermission { get; set; }
    }

    /// <summary>
    /// Create by Hai.Hoang
    /// </summary>
    [Serializable]
    public class OCRTemplatePageSerializble
    {
        public string Key { set; get; }
        public List<OCRTemplateZone> OCRTemplateZone { set; get; }
        public int PageIndex { get; set; }
    }
    [Serializable]
    public class OCRTemplateSerializble
    {
        public List<OCRTemplatePageSerializble> OCRTemplatePages { set; get; }
        public Guid DocTypeId { set; get; }
        public Guid LangId { set; get; }

        public string FileExtension { get; set; }
    }
}