using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Ecm.CaptureDomain;
using System.Xml.Serialization;

namespace CaptureMVC.Models
{
    public class ViewBatchModel
    {
        public ViewBatchModel()
        {
            this.FieldValues = new List<BatchFieldValue>();
            this.DeletedDocuments = new List<Guid>();
            this.DeletedLooseDocuments = new List<Guid>();
            this.Comments = new List<Comment>();
            this.Documents = new List<ViewDocumentModel>();
        }

        #region Origin

        public Guid Id { get; set; }
        public string BatchName { get; set; }
        public Guid BatchTypeId { get; set; }
        public int DocCount { get; set; }
        public int PageCount { get; set; }
        public DateTime CreatedDate { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public string ModifiedBy { get; set; }
        public string LockedBy { get; set; }
        public string DelegatedBy { get; set; }
        public string DelegatedTo { get; set; }
        public Guid WorkflowInstanceId { get; set; }
        public Guid WorkflowDefinitionId { get; set; }
        public string BlockingBookmark { get; set; }
        public string BlockingActivityName { get; set; }
        public string BlockingActivityDescription { get; set; }
        public DateTime? BlockingDate { get; set; }
        public DateTime? LastAccessedDate { get; set; }
        public string LastAccessedBy { get; set; }
        public bool IsCompleted { get; set; }
        public bool IsProcessing { get; set; }
        public bool IsRejected { get; set; }
        public bool HasError { get; set; }
        public string StatusMsg { get; set; }
        public BatchType BatchType { get; set; }
        public List<BatchFieldValue> FieldValues { get; set; }
        public List<Guid> DeletedDocuments { get; set; }
        public List<Guid> DeletedLooseDocuments { get; set; }
        public BatchPermission BatchPermission { get; set; }
        public List<Comment> Comments { get; set; }

        #endregion

        #region Addition

        public List<ViewDocumentModel> Documents { get; set; }
        public bool IsLoaded { get; set; }

        #endregion
    }

    public class ViewDocumentModel
    {
        public ViewDocumentModel()
        {
            this.FieldValues = new List<DocumentFieldValue>();
            this.DeletedPages = new List<Guid>();
            this.EmbeddedPictures = new List<OutlookPicture>();
            this.Pages = new List<ViewPageModel>();
            this.SavePages = new List<ViewPageModel>();
            this.Scale = 10;
        }

        #region Origin

        public Guid Id { get; set; }
        public string DocName { get; set; }
        public Guid DocTypeId { get; set; }
        public int PageCount { get; set; }
        public DateTime CreatedDate { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public string ModifiedBy { get; set; }
        public string BinaryType { get; set; }
        public bool IsRejected { get; set; }
        public Guid BatchId { get; set; }
        public DocumentType DocumentType { get; set; }
        public bool IsUndefinedType { get; set; }
        public List<DocumentFieldValue> FieldValues { get; set; }
        public List<Guid> DeletedPages { get; set; }
        public List<OutlookPicture> EmbeddedPictures { get; set; }
        public AnnotationPermission AnnotationPermission { get; set; }

        #endregion

        #region Addition

        public List<ViewPageModel> Pages { get; set; }
        public List<ViewPageModel> SavePages { get; set; }

        public bool CanSeeHighlight { get; set; }
        public bool CanAddHighlight { get; set; }
        public bool CanDeleteHighlight { get; set; }

        public bool CanHideRedaction { get; set; }
        public bool CanAddRedaction { get; set; }
        public bool CanDeleteRedaction { get; set; }

        public bool CanSeeText { get; set; }
        public bool CanAddText { get; set; }
        public bool CanDeleteText { get; set; }

        public string DocKind { get; set; }
        public int Scale { get; set; }

        #endregion
    }

    public class ViewPageModel
    {
        public ViewPageModel()
        {
            this.NotHideRedactions = new List<Annotation>();
            this.SeeAnnotations = new List<Annotation>();
            this.NotSeeAnnotations = new List<Annotation>();
        }

        public Page OriginPage { get; set; }

        public string ContentType { get; set; }
        public byte[] Thumbnail { get; set; }
        public byte[] Image { get; set; }
        public int Dpi { get; set; }
        public bool IsNew { get; set; }
        public int AdjustRotateAngle { get; set; }

        public List<Annotation> NotHideRedactions { get; set; }
        public List<Annotation> SeeAnnotations { get; set; }
        public List<Annotation> NotSeeAnnotations { get; set; }

        public string ThumbFilePath { get; set; }
        public string ShowFilePath { get; set; }
        public string OriginFilePath { get; set; }
    }

    public class ViewAnnotationModel
    {
        #region Origin
        public Guid Id { get; set; }
        public Guid PageId { get; set; }
        public string Type { get; set; }
        public double Height { get; set; }
        public double Width { get; set; }
        public double Left { get; set; }
        public double RotateAngle { get; set; }
        public double Top { get; set; }
        public string Content { get; set; }
        public DateTime CreatedOn { get; set; }
        public string CreatedBy { get; set; }
        public DateTime ModifiedOn { get; set; }
        public string ModifiedBy { get; set; }

        #endregion

        #region Addition
        public bool IsUpdate { get; set; }
        #endregion
    }


    public class ViewContextMenuModel
    {
        public string Key { get; set; }
        public string Value { get; set; }
    }

    public class ViewGetAnnotationsModel
    {
        public ViewGetAnnotationsModel()
        {
            this.Annotations = new List<Annotation>();
        }

        public bool CanSeeHighlight { get; set; }
        public bool CanAddHighlight { get; set; }
        public bool CanDeleteHighlight { get; set; }

        public bool CanHideRedaction { get; set; }
        public bool CanAddRedaction { get; set; }
        public bool CanDeleteRedaction { get; set; }

        public bool CanSeeText { get; set; }
        public bool CanAddText { get; set; }
        public bool CanDeleteText { get; set; }

        public List<Annotation> Annotations { get; set; }
    }

    public class ViewUploadModel
    {
        public string PageId { get; set; }
        public string DocId { get; set; }
        public string BatchId { get; set; }
        public string Type { get; set; }

        /// <summary>
        /// Image string base 64 data user for Camera
        /// </summary>
        public string ImageData { get; set; }

        /// <summary>
        /// List file name use for Scan
        /// </summary>
        public string FileNames { get; set; }
    }


    public class ViewSaveBatchModel
    {
        public ViewSaveBatchModel()
        {
            this.Indexes = new List<ViewSaveIndexModel>();
            this.Comments = new List<ViewSaveCommentModel>();
            this.Documents = new List<ViewSaveDocModel>();
        }
        public string Id { get; set; }
        public string Name { get; set; }
        public bool IsReject { get; set; }
        public List<ViewSaveIndexModel> Indexes { get; set; }
        public List<ViewSaveCommentModel> Comments { get; set; }
        public List<ViewSaveDocModel> Documents { get; set; }
    }

    public class ViewSaveDocModel
    {
        public ViewSaveDocModel()
        {
            this.Indexes = new List<ViewSaveIndexModel>();
            this.Pages = new List<ViewSavePageModel>();
        }

        public string Id { get; set; }
        public string Name { get; set; }
        public bool IsReject { get; set; }
        public int Scale { get; set; }
        public List<ViewSaveIndexModel> Indexes { get; set; }
        public List<ViewSavePageModel> Pages { get; set; }
    }

    public class ViewSavePageModel
    {
        public ViewSavePageModel()
        {
            this.Annotations = new List<ViewSaveAnnoModel>();
        }

        public string Id { get; set; }
        public string OldDocId { get; set; }
        public bool IsReject { get; set; }
        public string LanguageCode { get; set; }
        public int RotateAngle { get; set; }
        public List<ViewSaveAnnoModel> Annotations { get; set; }
        public string DeleteAnnotations { get; set; }
    }

    public class ViewSaveAnnoModel
    {
        public string Id { get; set; }
        public string Type { get; set; }
        public double Left { get; set; }
        public double Top { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public int RotateAngle { get; set; }
        public string Content { get; set; }
    }

    public class ViewSaveIndexModel
    {
        public ViewSaveIndexModel()
        {
            this.Rows = new List<ViewSaveIndexRowsModel>();
        }

        public string Id { get; set; }
        public string Value { get; set; }
        public string FieldId { get; set; }
        public List<ViewSaveIndexRowsModel> Rows { get; set; }
    }

    public class ViewSaveIndexRowsModel
    {
        public ViewSaveIndexRowsModel()
        {
            this.Cols = new List<ViewSaveIndexModel>();
        }

        public List<ViewSaveIndexModel> Cols { get; set; }
    }

    public class ViewSaveCommentModel
    {
        public DateTime CreateDate { get; set; }
        public string Note { get; set; }
    }

    public class Paragraph
    {
        [XmlElement("Run")]
        public List<Run> Runs { get; set; }

        public Paragraph()
        {
            Runs = new List<Run>();
        }
    }

    public class Run
    {
        [XmlAttribute]
        public string FontSize { get; set; }

        [XmlAttribute]
        public string FontStyle { get; set; }

        [XmlAttribute]
        public string FontWeight { get; set; }

        [XmlAttribute]
        public string Foreground { get; set; }

        [XmlText]
        public string Content { get; set; }
    }


}