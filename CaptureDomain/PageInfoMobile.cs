using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Ecm.CaptureDomain
{
    [DataContract]
    public class PageInfoMobile
    {
        public PageInfoMobile()
        {
            Annotations = new List<Annotation>();
        }

        [DataMember]
        public Guid Id { get; set; }

        [DataMember]
        public int PageNumber { get; set; }

        [DataMember]
        public string FileExtension { get; set; }

        [DataMember]
        public string MimeType { get; set; }

        [DataMember]
        public string FileBinaryBase64 { get; set; }

        [DataMember]
        public double RotateAngle { get; set; }

        [DataMember]
        public double Width { get; set; }

        [DataMember]
        public double Height { get; set; }

        [DataMember]
        public bool IsRejected { get; set; }

        [DataMember]
        public List<Annotation> Annotations { get; set; }
    }
}
