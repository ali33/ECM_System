using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Ecm.CaptureDomain
{
    [DataContract]
    public class Batch
    {

        private List<BatchFieldValue> _fieldValues;

        public Batch()
        {
            FieldValues = new List<BatchFieldValue>();
            Documents = new List<Document>();
            DeletedDocuments = new List<Guid>();
            DeletedLooseDocuments = new List<Guid>();
            Comments = new List<Comment>();
        }

        [DataMember]
        public Guid Id { get; set; }

        [DataMember]
        public string BatchName { get; set; }

        [DataMember]
        public Guid BatchTypeId { get; set; }

        [DataMember]
        public int DocCount { get; set; }

        [DataMember]
        public int PageCount { get; set; }

        [DataMember]
        public DateTime CreatedDate { get; set; }

        [DataMember]
        public string CreatedBy { get; set; }

        [DataMember]
        public DateTime? ModifiedDate { get; set; }

        [DataMember]
        public string ModifiedBy { get; set; }

        [DataMember]
        public string LockedBy { get; set; }

        [DataMember]
        public string DelegatedBy { get; set; }

        [DataMember]
        public string DelegatedTo { get; set; }

        [DataMember]
        public Guid WorkflowInstanceId { get; set; }

        [DataMember]
        public Guid WorkflowDefinitionId { get; set; }

        [DataMember]
        public string BlockingBookmark { get; set; }

        [DataMember]
        public string BlockingActivityName { get; set; }

        [DataMember]
        public string BlockingActivityDescription { get; set; }

        [DataMember]
        public DateTime? BlockingDate { get; set; }

        [DataMember]
        public DateTime? LastAccessedDate { get; set; }

        [DataMember]
        public string LastAccessedBy { get; set; }

        [DataMember]
        public bool IsCompleted { get; set; }

        [DataMember]
        public bool IsProcessing { get; set; }

        [DataMember]
        public bool IsRejected { get; set; }

        [DataMember]
        public bool HasError { get; set; }

        [DataMember]
        public string StatusMsg { get; set; }

        [DataMember]
        public BatchType BatchType { get; set; }

        [DataMember]
        public List<BatchFieldValue> FieldValues
        {
            get { return _fieldValues; }
            set
            {
                if (value != null)
                {
                    _fieldValues = new List<BatchFieldValue>(value);
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
        [DataMember]
        public List<Document> Documents { get; set; }

        /// <summary>
        /// Collection of identifiers of deleted documents
        /// </summary>
        [DataMember]
        public List<Guid> DeletedDocuments { get; set; }

        /// <summary>
        /// Collection of identifiers of deleted documents
        /// </summary>
        [DataMember]
        public List<Guid> DeletedLooseDocuments { get; set; }

        /// <summary>
        /// The permission of human step of batch at current time
        /// </summary>
        [DataMember]
        public BatchPermission BatchPermission { get; set; }

        [DataMember]
        public List<Comment> Comments { get; set; }

        /// <summary>
        /// Transaction Id for current open batch
        /// </summary>
        [DataMember]
        public Guid TransactionId { get; set; }

      
    }
}
