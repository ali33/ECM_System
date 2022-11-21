using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Ecm.Mvvm;

namespace Ecm.Model
{
    public class DocumentModel : BaseDependencyProperty
    {
        #region Private members

        private DateTime? _modifiedDate;

        private int _pageCount;

        #endregion

        public DocumentModel(DateTime createdDate, string createdBy, DocumentTypeModel documentType)
        {
            CreatedDate = createdDate;
            CreatedBy = createdBy;
            DocumentType = documentType;
            FieldValues = new List<FieldValueModel>();
            foreach (FieldMetaDataModel field in DocumentType.Fields)
            {
                FieldValues.Add(new FieldValueModel { Field = field, Value = field.DefaultValue });
            }

            Pages = new List<PageModel>();
            DeletedPages = new List<Guid>();
            LinkDocuments = new ObservableCollection<LinkDocumentModel>();
        }

        public DocumentModel()
        {
            Pages = new List<PageModel>();
            DeletedPages = new List<Guid>();
            LinkDocuments = new ObservableCollection<LinkDocumentModel>();
            DeletedLinkDocuments = new List<Guid>();
        }

        public Guid DocVersionId { get; set; }

        public Guid Id { get; set; }

        public int PageCount
        {
            get { return _pageCount; }
            set
            {
                _pageCount = value;
                OnPropertyChanged("PageCount");
            }
        }

        public int Version { get; set; }

        public DateTime CreatedDate { get; set; }

        public string CreatedBy { get; set; }

        public DateTime? ModifiedDate
        {
            get { return _modifiedDate; }
            set
            {
                _modifiedDate = value;
                OnPropertyChanged("ModifiedDate");
            }
        }

        public string ModifiedBy { get; set; }

        public FileTypeModel BinaryType { get; set; }

        public DocumentTypeModel DocumentType { get; set; }

        public List<FieldValueModel> FieldValues { get; set; }

        public List<PageModel> Pages { get; set; }

        public List<Guid> DeletedPages { get; set; }

        public ObservableCollection<CommentModel> Comments { get; set; }

        public Dictionary<string, byte[]> EmbeddedPictures { get; set; }

        public List<Guid> DeletedLinkDocuments { get; set; }

        public ObservableCollection<LinkDocumentModel> LinkDocuments { get; set; }
    }
}
