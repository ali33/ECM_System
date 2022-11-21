using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Ecm.Domain
{
    /// <summary>
    /// Represent a version of a document in CloudECM
    /// </summary>
    [DataContract]
    public class DocumentVersion
    {
        /// <summary>
        /// Intialize new object of a version of document
        /// </summary>
        public DocumentVersion()
        {
            AnnotationVersions  = new List<AnnotationVersion>();
            PageVersions = new List<PageVersion>();
            DocumentFieldVersions = new List<DocumentFieldVersion>();
        }

        /// <summary>
        /// Identifier of a version of document
        /// </summary>
        [DataMember]
        public Guid Id { get; set; }

        /// <summary>
        /// Identifier of the original document
        /// </summary>
        [DataMember]
        public Guid DocId { get; set; }

        /// <summary>
        /// Identifier of the document type the document belong to.
        /// </summary>
        [DataMember]
        public Guid DocTypeId { get; set; }

        /// <summary>
        /// Identifier of the version of the document type if any.
        /// </summary>
        [DataMember]
        public Guid? DocTypeVersionId { get; set; }

        /// <summary>
        /// The number of pages of the version of document
        /// </summary>
        [DataMember]
        public int PageCount { get; set; }

        /// <summary>
        /// The version number of the version of document
        /// </summary>
        [DataMember]
        public int Version { get; set; }

        /// <summary>
        /// The date when the document was created
        /// </summary>
        [DataMember]
        public DateTime CreatedDate { get; set; }

        /// <summary>
        /// The date when the document was modified.
        /// </summary>
        [DataMember]
        public DateTime? ModifiedDate { get; set; }

        /// <summary>
        /// The user name of the user created the document
        /// </summary>
        [DataMember]
        public string CreatedBy { get; set; }

        /// <summary>
        /// The user name of the user modified the document
        /// </summary>
        [DataMember]
        public string ModifiedBy { get; set; }

        /// <summary>
        /// Internal use only
        /// </summary>
        [DataMember]
        public int ChangeAction { get; set; }

        /// <summary>
        /// See <see cref="Document.BinaryType"/> of <see cref="Document"/> for more detail.
        /// </summary>
        [DataMember]
        public string BinaryType { get; set; }

        /// <summary>
        /// The version of annotations in this version of document
        /// </summary>
        [DataMember]
        public List<AnnotationVersion> AnnotationVersions { get; set; }

        /// <summary>
        /// The version of pages in this version of document
        /// </summary>
        [DataMember]
        public List<PageVersion> PageVersions { get; set; }

        /// <summary>
        /// The version of field values in this version of document
        /// </summary>
        [DataMember]
        public List<DocumentFieldVersion> DocumentFieldVersions
        {
            get { return _fieldValues; }
            set
            {
                if (value != null)
                {
                    _fieldValues = new List<DocumentFieldVersion>(value);
                }
                else
                {
                    _fieldValues = null;
                }
            }
        }

        /// <summary>
        /// The object of the version of <see cref="DocumentType"/>
        /// </summary>
        [DataMember]
        public DocumentTypeVersion DocumentTypeVersion { get; set; }

        private List<DocumentFieldVersion> _fieldValues;
    }
}