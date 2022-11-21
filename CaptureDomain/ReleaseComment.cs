using System;
using System.Runtime.Serialization;

namespace Ecm.CaptureDomain
{
    [DataContract]
    public class ReleaseComment
    {
        [DataMember]
        public Guid Id { get; set; }

        [DataMember]
        public Guid ReleaseInstanceId { get; set; }

        [DataMember]
        public bool IsBatchId { get; set; }

        [DataMember]
        public string Note { get; set; }

        [DataMember]
        public DateTime CreatedDate { get; set; }

        [DataMember]
        public string CreatedBy { get; set; }
    }
}
