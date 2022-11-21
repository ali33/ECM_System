using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ArchiveMVC.Models
{
    public class ArchiveAdminModel
    {
        //public IList<DocumentTypeModel> ListDocType { get; set ;}
         public ArchiveAdminModel()
         {
            ListDocType = new Dictionary<string, DocumentTypeModel>();
         }
         public Dictionary<string, DocumentTypeModel> ListDocType = new Dictionary<string, DocumentTypeModel>();
         public Guid DocumentId { get; set; }
         public IList<string> ListDataType { get; set; }
         public string TableValue { get; set; }
         public DocumentTypeModel ArChiveDocTypeModel { get; set; }
         public IList<DocumentTypeModel> ListDocTypeModel { get; set; }
         public IList<UserGroupModel> ListUserGroup { get; set; }
         public UserModel ArchiveUserModel { get; set; }
         public IList<LanguageModel> ListLanguages { get; set; }
         public DocumentTypePermissionModel DocTypePermission { get; set; }
         public AnnotationPermissionModel AnnotationPermission { get; set; }
         public AuditPermissionModel AuditPermission { get; set; }
    }

    /// <summary>
    /// Create by Tho Dinh
    /// </summary>
    [Serializable]
    public class OCRTemplatePageSerializble
    {
        public string Key { set; get; }
        public List<OCRTemplateZoneModel> OCRTemplateZone { set; get; }
        public int PageIndex { get; set; }
        public string FileExtension { get; set; }
    }
    [Serializable]
    public class OCRTemplateSerializble
    {
        public List<OCRTemplatePageSerializble> OCRTemplatePages { set; get; }
        public Guid DocTypeId { set; get; }
        public Guid LangId { set; get; }
    }
}