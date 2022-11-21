using System;
using System.Runtime.Serialization;

namespace Ecm.CaptureDomain
{
    [DataContract]
    public class ReleaseAnnotation
    {
        [DataMember]
        public Guid Id { get; set; }

        [DataMember]
        public Guid ReleasePageId { get; set; }

        [DataMember]
        public string Type { get; set; }

        [DataMember]
        public double Height { get; set; }

        [DataMember]
        public double Width { get; set; }

        [DataMember]
        public double Left { get; set; }

        [DataMember]
        public string LineEndAt { get; set; }

        [DataMember]
        public string LineStartAt { get; set; }

        [DataMember]
        public string LineStyle { get; set; }

        [DataMember]
        public int LineWeight { get; set; }

        [DataMember]
        public double RotateAngle { get; set; }

        [DataMember]
        public double Top { get; set; }

        [DataMember]
        public string LineColor { get; set; }

        [DataMember]
        public string Content { get; set; }

        [DataMember]
        public string CreatedBy { get; set; }

        [DataMember]
        public DateTime CreatedOn { get; set; }
    }
}
