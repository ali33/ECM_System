using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Ecm.CaptureDomain
{
    [DataContract]
    public class Document
    {
        private List<DocumentFieldValue> _fieldValues;

        public Document()
        {
            FieldValues = new List<DocumentFieldValue>();
            Pages = new List<Page>();
            DeletedPages = new List<Guid>();
        }

        [DataMember]
        public Guid Id { get; set; }

        // HungLe - 2014/07/18 - Adding doc name - Start
        [DataMember]
        public string DocName { get; set; }
        // HungLe - 2014/07/18 - Adding doc name - End

        [DataMember]
        public Guid DocTypeId { get; set; }

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
        public string BinaryType { get; set; }

        [DataMember]
        public bool IsRejected { get; set; }

        [DataMember]
        public Guid BatchId { get; set; }

        [DataMember]
        public DocumentType DocumentType { get; set; }

        [DataMember]
        public bool IsUndefinedType { get; set; }

        /// <summary>
        /// Value of fields in the document of this work item
        /// </summary>
        [DataMember]
        public List<DocumentFieldValue> FieldValues
        {
            get { return _fieldValues; }
            set
            {
                if (value != null)
                {
                    _fieldValues = new List<DocumentFieldValue>(value);
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
        [DataMember]
        public List<Page> Pages { get; set; }

        /// <summary>
        /// Collection of identifiers of deleted pages
        /// </summary>
        [DataMember]
        public List<Guid> DeletedPages { get; set; }

        [DataMember]
        public List<OutlookPicture> EmbeddedPictures { get; set; }

        [DataMember]
        public AnnotationPermission AnnotationPermission { get; set; }

        [DataMember]
        public DocumentPermission DocumentPermission { get; set; }

        [DataMember]
        public int Order { get; set; }
    }
}
