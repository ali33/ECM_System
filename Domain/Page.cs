using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using System;

namespace Ecm.Domain
{
    /// <summary>
    /// Represent each page of a document.
    /// </summary>
    [DataContract]
    public class Page
    {
        /// <summary>
        /// Initialize new page object.
        /// </summary>
        public Page()
        {
            Annotations =  new List<Annotation>();
        }

        /// <summary>
        /// Identifier of this page
        /// </summary>
        [DataMember]
        [XmlElement]
        public Guid Id { get; set; }

        /// <summary>
        /// Identifier of the document contains this page
        /// </summary>
        [DataMember]
        [XmlElement]
        public Guid DocId { get; set; }

        /// <summary>
        /// Identifier of the document type which the document belong to
        /// </summary>
        [DataMember]
        [XmlElement]
        public Guid DocTypeId { get; set; }

        /// <summary>
        /// The order of this page in a document
        /// </summary>
        [DataMember]
        [XmlElement]
        public int PageNumber { get; set; }

        /// <summary>
        /// The extension of the file represent this page
        /// </summary>
        [DataMember]
        [XmlElement]
        public string FileExtension { get; set; }

        /// <summary>
        /// The binary of the file represent this page
        /// </summary>
        [DataMember]
        [XmlElement]
        public byte[] FileBinary { get; set; }

        [DataMember]
        [XmlElement]
        public byte[] FileHeader { get; set; }

        /// <summary>
        /// Path on Server of the file represent this page
        /// </summary>
        [DataMember]
        [XmlElement]
        public string FilePath { get; set; }

        
        /// <summary>
        /// The hash code which is used to track whether the file is changed
        /// </summary>
        [DataMember]
        [XmlElement]
        public string FileHash { get; set; }

        /// <summary>
        /// The angle which this page is rotated
        /// </summary>
        [DataMember]
        [XmlElement]
        public double RotateAngle { get; set; }

        /// <summary>
        /// Width of this page
        /// </summary>
        [DataMember]
        [XmlElement]
        public double Width { get; set; }

        /// <summary>
        /// Height of this page
        /// </summary>
        [DataMember]
        [XmlElement]
        public double Height { get; set; }

        /// <summary>
        /// Collection of annotations are added on this page.
        /// </summary>
        [DataMember]
        [XmlElement]
        public List<Annotation> Annotations { get; set; }

        /// <summary>
        /// Content of page.
        /// </summary>
        [DataMember]
        [XmlElement]
        public string Content { get; set; }

        /// <summary>
        /// OCR Language code of page. en-US or vi-VN
        /// </summary>
        [DataMember]
        [XmlElement]
        public string ContentLanguageCode { get; set; }

        /// <summary>
        /// Original file name of page
        /// </summary>
        [DataMember]
        [XmlElement]
        public string OriginalFileName { get; set; }

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

    }
}