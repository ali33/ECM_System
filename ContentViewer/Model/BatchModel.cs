using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Xml.Serialization;

using Ecm.Mvvm;

namespace Ecm.ContentViewer.Model
{
    public class BatchModel : BaseDependencyProperty
    {
        private List<FieldValueModel> _fieldValues = new List<FieldValueModel>();
        private bool _isSelected;

        public BatchModel()
        {
            Documents = new ObservableCollection<ContentModel>();
            Comments = new ObservableCollection<CommentModel>();
        }

        public BatchModel(Guid id, DateTime createdDate, string createdBy, BatchTypeModel batchType)
        {
            Id = id;
            BatchType = batchType;
            FieldValues = new List<FieldValueModel>();
            foreach (FieldModel field in batchType.Fields)
            {
                FieldValues.Add(new FieldValueModel { Field = field, Value = field.DefaultValue });
            }

            CreatedDate = createdDate;
            CreatedBy = createdBy;
            Documents = new ObservableCollection<ContentModel>();
            Comments = new ObservableCollection<CommentModel>();
        }

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

        public BatchTypeModel BatchType { get; set; }

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
        /// All documents in this work item
        /// </summary>
        public ObservableCollection<ContentModel> Documents { get; set; }

        /// <summary>
        /// Collection of identifiers of deleted documents
        /// </summary>
        public List<Guid> DeletedDocuments { get; set; }

        /// <summary>
        /// Collection of identifiers of deleted documents
        /// </summary>
        public List<Guid> DeletedPages { get; set; }

        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                _isSelected = value;
                OnPropertyChanged("IsSelected");
            }
        }

        public ObservableCollection<CommentModel> Comments { get; set; }

        public BatchPermissionModel Permission { get; set; }
    }
}
