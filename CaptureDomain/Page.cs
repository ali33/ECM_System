using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Ecm.CaptureDomain
{
    [DataContract]
    public class Page
    {
        public Page()
        {
            Annotations = new List<Annotation>();
            DeleteAnnotations = new List<Guid>();
        }

        [DataMember]
        public Guid Id { get; set; }

        [DataMember]
        public Guid DocId { get; set; }

        /// <summary>
        /// The order of this page in a document
        /// </summary>
        [DataMember]
        public int PageNumber { get; set; }

        [DataMember]
        public string FileExtension { get; set; }

        [DataMember]
        public byte[] FileBinary { get; set; }

        /// <summary>
        /// The Base64 string of image document using for Mobile.
        /// </summary>
        [DataMember]
        public string FileBinaryBase64 { get; set; }

        [DataMember]
        public byte[] FileHeader { get; set; }

        [DataMember]
        public string FilePath { get; set; }

        /// <summary>
        /// The hash code which is used to track whether the file is changed
        /// </summary>
        [DataMember]
        public string FileHash { get; set; }

        /// <summary>
        /// The angle which this page is rotated
        /// </summary>
        [DataMember]
        public double RotateAngle { get; set; }

        [DataMember]
        public double Width { get; set; }

        [DataMember]
        public double Height { get; set; }

        [DataMember]
        public bool IsRejected { get; set; }

        /// <summary>
        /// Collection of annotations are added on this page.
        /// </summary>
        [DataMember]
        public List<Annotation> Annotations { get; set; }

        [DataMember]
        public List<Guid> DeleteAnnotations { get; set; }

        [DataMember]
        public string Content { get; set; }

        [DataMember]
        public string ContentLanguageCode { get; set; }

        [DataMember]
        public string OriginalFileName { get; set; }

        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
