using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Ecm.Domain
{
    /// <summary>
    /// Provide a backup version of <see cref="Page"/> object. Refer to <see cref="Page"/> class to see more detail.
    /// </summary>
    [DataContract]
    public sealed class PageVersion
    {
        public PageVersion()
        {
            AnnotationVersions = new List<AnnotationVersion>();
        }

        [DataMember]
        public Guid Id { get; set; }

        [DataMember]
        public Guid PageId { get; set; }

        [DataMember]
        public Guid DocId { get; set; }

        [DataMember]
        public Guid DocVersionId { get; set; }

        [DataMember]
        public int PageNumber { get; set; }

        [DataMember]
        public string FileExtension { get; set; }

        [DataMember]
        public byte[] FileBinary { get; set; }

        [DataMember]
        public byte[] FileHeader { get; set; }
        
        /// <summary>
        /// Path on Server of the file represent this page
        /// </summary>
        [DataMember]
        public string FilePath { get; set; }

        [DataMember]
        public string FileHash { get; set; }

        [DataMember]
        public double RotateAngle { get; set; }

        [DataMember]
        public double Width { get; set; }

        [DataMember]
        public double Height { get; set; }

        [DataMember]
        public List<AnnotationVersion> AnnotationVersions { get; set; }
    }
}