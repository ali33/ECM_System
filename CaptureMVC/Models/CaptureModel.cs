using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Ecm.CaptureDomain;
using System.Xml.Serialization;
using System.Text;
using System.IO;

namespace CaptureMVC.Models
{
    public class BatchTypeResult
    {
        public BatchType BatchType;
        public string IconKey;
    }
    public class CaptureBatchModelBak
    {
        public CaptureBatchModelBak()
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

    public class CaptureInsertModel
    {
        public bool DoOcr { get; set; }
        public bool DoBarcode { get; set; }
        public Guid BatchTypeId { get; set; }
        public Guid DocTypeId { get; set; }
        public string LanguageName { get; set; }

        public Guid BatchId { get; set; }
        public Guid DocId { get; set; }
    }

    public class OCRFieldModel
    {
        public Guid FieldId { get; set; }
        public int PageIndex { get; set; }
        public int Left { get; set; }
        public int Top { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
    }

    public class CaptureAmbiguousDefinitionModel
    {
        private Dictionary<string, string> _dictionary;

        public AmbiguousDefinition AmbiguousDefinition { get; set; }

        public Dictionary<string, string> Dictionary
        {
            get
            {
                if (_dictionary == null)
                {
                    _dictionary = new Dictionary<string, string>();
                    byte[] byteArray = AmbiguousDefinition.Unicode ? Encoding.UTF8.GetBytes(AmbiguousDefinition.Text)
                                                                   : Encoding.ASCII.GetBytes(AmbiguousDefinition.Text);
                    using (var me = new MemoryStream(byteArray))
                    {
                        var sr = new StreamReader(me);
                        while (true)
                        {
                            string line = sr.ReadLine();
                            if (line != null)
                            {
                                string text = line;
                                string key = text.Substring(0, text.IndexOf("=")).TrimEnd();
                                string value = text.Substring(text.IndexOf("=") + 1).TrimEnd();
                                _dictionary.Add(key, value);
                            }
                            else
                            {
                                break;
                            }
                        }
                    }
                }

                return _dictionary;
            }
        }
    }

    public class CaptureBatchModel
    {
        public Guid Id { get; set; }
        public Guid TypeId { get; set; }
        public string Name { get; set; }
        public string TypeName { get; set; }
        public DateTime CreateDate { get; set; }
        public string CreateBy { get; set; }

        public List<CaptureDocumentModel> Documents { get; set; }

        public CaptureBatchModel()
        {
            Documents = new List<CaptureDocumentModel>();
        }
    }

    public class CaptureDocumentModel
    {
        public Guid Id { get; set; }
        public Guid TypeId { get; set; }
        public string Name { get; set; }
        public string TypeName { get; set; }
        public bool IsLooseItem { get; set; }

        public int DocNumber { get; set; }

        public List<CapturePageModel> Pages { get; set; }

        public CaptureDocumentModel()
        {
            Pages = new List<CapturePageModel>();
        }
    }

    public class CapturePageModel
    {
        public Guid Id { get; set; }
        public string OriginalFileName { get; set; }
        public string FileExtension { get; set; }

        public bool IsImage { get; set; }
        public bool IsSupportPreview { get; set; }

        public int Dpi { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }

        public int PageNumber { get; set; }

        public string OriginFilePath { get; set; }
        public string ShowFilePath { get; set; }
        public string ThumbFilePath { get; set; }
    }
}