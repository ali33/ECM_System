using System;
using System.Runtime.Serialization;
using System.Collections.Generic;

namespace Ecm.CaptureDomain
{
    [DataContract]
    public class ReleasePage
    {
        [DataMember]
        public Guid Id { get; set; }

        [DataMember]
        public Guid ReleaseDocId { get; set; }

        [DataMember]
        public int PageNumber { get; set; }

        [DataMember]
        public string FileExtension { get; set; }

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
        public List<ReleaseAnnotation> ReleaseAnnotations { get; set; }

        [DataMember]
        public byte[] FileBinary { get; set; }

        [DataMember]
        public byte[] FileHeader { get; set; }
    }
}
