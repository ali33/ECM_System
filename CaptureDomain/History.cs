using System;
using System.Runtime.Serialization;

namespace Ecm.CaptureDomain
{
    [DataContract]
    public class History
    {
        [DataMember]
        public Guid Id { get; set; }

        [DataMember]
        public Guid BatchId { get; set; }

        [DataMember]
        public string Action { get; set; }

        [DataMember]
        public DateTime ActionDate { get; set; }

        [DataMember]
        public string WorkflowStep { get; set; }

        [DataMember]
        public string CustomMsg { get; set; }
    }
}
