using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace Ecm.Domain
{
    /// <summary>
    /// Represent the document in CloudECM.
    /// </summary>
    [DataContract]
    public class Document
    {
        /// <summary>
        /// Initialize the object of a document.
        /// </summary>
        public Document()
        {
            FieldValues = new List<FieldValue>();
            Pages = new List<Page>();
            DeletedPages = new List<Guid>();
        }

        /// <summary>
        /// Identifier of the document
        /// </summary>
        [DataMember]
        [XmlElement]
        public Guid Id { get; set; }

        /// <summary>
        /// Identifier of the document type that the document belong to
        /// </summary>
        [DataMember]
        public Guid DocTypeId { get; set; }

        /// <summary>
        /// Number of pages in the document. This property will be calculated automatically when document is inserted or updated.
        /// </summary>
        [DataMember]
        [XmlElement]
        public int PageCount { get; set; }

        /// <summary>
        /// The current version of the document.
        /// </summary>
        [DataMember]
        [XmlElement]
        public int Version { get; set; }

        /// <summary>
        /// The time when this document is created
        /// </summary>
        [DataMember]
        [XmlElement]
        public DateTime CreatedDate { get; set; }

        /// <summary>
        /// The time when this document is modified
        /// </summary>
        [DataMember]
        [XmlElement]
        public DateTime? ModifiedDate { get; set; }

        /// <summary>
        /// The user name of <see cref="User"/> create this document
        /// </summary>
        [DataMember]
        [XmlElement]
        public string CreatedBy { get; set; }

        /// <summary>
        /// The user name of <see cref="User"/> modify this document
        /// </summary>
        [DataMember]
        [XmlElement]
        public string ModifiedBy { get; set; }

        /// <summary>
        /// The value can be: Image, Native, Media, Compound.
        /// </summary>
        /// <remarks>
        /// <list type="bullet">
        /// <item>
        ///     <description>BinaryType = Image mean all pages in document are image.</description>
        /// </item>
        /// <item>
        ///     <description>BinaryType = Native mean all pages in document are not Image and not Media.</description>
        /// </item>
        /// <item>
        ///     <description>BinaryType = Media mean all pages in document are music or video.</description>
        /// </item>
        /// <item>
        ///     <description>BinaryType = Compoud mean all pages in document include 2 or more binary type.</description>
        /// </item>
        /// </list>
        /// </remarks>
        [DataMember]
        [XmlElement]
        public string BinaryType { get; set; }

        /// <summary>
        /// The <see cref="DocumentType"/> object that this document belong to
        /// </summary>
        [DataMember]
        [XmlElement]
        public DocumentType DocumentType { get; set; }

        /// <summary>
        /// Contains a collection of values of <see cref="FieldMetaData"/> defined by <see cref="DocumentType"/>
        /// </summary>
        [DataMember]
        public List<FieldValue> FieldValues
        {
            get { return _fieldValues; }
            set
            {
                if (value != null)
                {
                    _fieldValues = new List<FieldValue>(value);
                }
                else
                {
                    _fieldValues = null;
                }
            }
        }

        /// <summary>
        /// Contains all pages of the document
        /// </summary>
        [DataMember]
        public List<Page> Pages { get; set; }

        /// <summary>
        /// Contains all identifier of deleted pages. This property is used for performance boost when the document is updated.
        /// </summary>
        [DataMember]
        public List<Guid> DeletedPages { get; set; }

        /// <summary>
        /// Embedded pictures list
        /// </summary>
        [DataMember]
        public List<OutlookPicture> EmbeddedPictures { get; set; }

        /// <summary>
        /// Linked document list
        /// </summary>
        [DataMember]
        public List<LinkDocument> LinkDocuments { get; set; }

        /// <summary>
        /// The list of pending deleted link document
        /// </summary>
        [DataMember]
        public List<Guid> DeletedLinkDocuments { get; set; }

        private List<FieldValue> _fieldValues;
    }
}