using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Ecm.Mvvm;
using System.Collections.ObjectModel;

namespace Ecm.CaptureModel
{
    public class DocumentModel : BaseDependencyProperty
    {
        private List<FieldValueModel> _fieldValues;
        private int _pageCount;
        private DateTime _createdDate;
        private string _createdBy;
        private FileTypeModel _binaryType;
        private string _modifiedBy;
        private bool _isRejected;
        private DateTime? _modifiedDate;

        public DocumentModel()
        {
            Pages = new List<PageModel>();
            FieldValues = new List<FieldValueModel>();
            DeletedPages = new List<Guid>();
        }

        public DocumentModel(DateTime createdDate, string createdBy, DocTypeModel documentType)
        {
            CreatedDate = createdDate;
            CreatedBy = createdBy;
            DocumentType = documentType;
            FieldValues = new List<FieldValueModel>();
            foreach (FieldModel field in DocumentType.Fields)
            {
                FieldValues.Add(new FieldValueModel { Field = field, Value = field.DefaultValue });
            }

            Pages = new List<PageModel>();
        }

        public Guid Id { get; set; }

        // HungLe - 2014/07/18 - Adding DocName - Start
        public String DocName { get; set; }
        // HungLe - 2014/07/18 - Adding DocName - End

        public Guid DocTypeId { get; set; }

        public int PageCount
        {
            get { return _pageCount; }
            set
            {
                _pageCount = value;
                OnPropertyChanged("PageCount");
            }
        }

        public DateTime CreatedDate
        {
            get { return _createdDate; }
            set
            {
                _createdDate = value;
                OnPropertyChanged("CreatedDate");
            }
        }

        public string CreatedBy
        {
            get { return _createdBy; }
            set
            {
                _createdBy = value;
                OnPropertyChanged("CreatedBy");
            }
        }

        public DateTime? ModifiedDate
        {
            get { return _modifiedDate; }
            set
            {
                _modifiedDate = value;
                OnPropertyChanged("ModifiedDate");
            }
        }

        public string ModifiedBy
        {
            get { return _modifiedBy; }
            set
            {
                _modifiedBy = value;
                OnPropertyChanged("ModifiedBy");
            }
        }

        public FileTypeModel BinaryType
        {
            get { return _binaryType; }
            set
            {
                _binaryType = value;
                OnPropertyChanged("BinaryType");
            }
        }

        public bool IsRejected
        {
            get { return _isRejected; }
            set
            {
                _isRejected = value;
                OnPropertyChanged("IsRejected");
            }
        }

        public Guid BatchId { get; set; }

        public DocTypeModel DocumentType { get; set; }

        /// <summary>
        /// Value of fields in the document of this work item
        /// </summary>
        public List<FieldValueModel> FieldValues
        {
            get { return _fieldValues; }
            set
            {
                if (value != null)
                {
                    _fieldValues = new List<FieldValueModel>(value);
                }
                else
                {
                    _fieldValues = null;
                }
            }
        }

        /// <summary>
        /// All pages in this document
        /// </summary>
        public List<PageModel> Pages { get; set; }

        /// <summary>
        /// Collection of identifiers of deleted pages
        /// </summary>
        public List<Guid> DeletedPages { get; set; }

        public Dictionary<string, byte[]> EmbeddedPictures { get; set; }

        public AnnotationPermissionModel AnnotationPermission { get; set; }

        public DocumentPermissionModel DocumentPermission { get; set; }

    }
}
