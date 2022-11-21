using System;
using System.Runtime.Serialization;
using System.Collections.Generic;

namespace Ecm.CaptureDomain
{
    [DataContract]
    public class ReleaseDocument
    {
        [DataMember]
        public Guid Id { get; set; }

        [DataMember]
        public Guid DocTypeId { get; set; }

        [DataMember]
        public int PageCount { get; set; }

        [DataMember]
        public DateTime CreatedDate { get; set; }

        [DataMember]
        public string CreatedBy { get; set; }

        [DataMember]
        public string BinaryType { get; set; }

        [DataMember]
        public Guid ReleaseBatchId { get; set; }

        [DataMember]
        public List<ReleaseDocumentFieldValue> ReleaseFieldValues { get; set; }

        [DataMember]
        public List<ReleasePage> ReleasePage { get; set; }
    }
}
