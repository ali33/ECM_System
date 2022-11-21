using System;
using System.Runtime.Serialization;
using System.Collections.Generic;

namespace Ecm.CaptureDomain
{
    [DataContract]
    public class ReleaseBatch
    {
        [DataMember]
        public Guid Id { get; set; }

        [DataMember]
        public Guid BatchTypeId { get; set; }

        [DataMember]
        public int DocCount { get; set; }

        [DataMember]
        public int PageCount { get; set; }

        [DataMember]
        public DateTime CreatedDate { get; set; }

        [DataMember]
        public string CreatedBy { get; set; }

        [DataMember]
        public DateTime ModifiedDate { get; set; }

        [DataMember]
        public string ModifiedBy { get; set; }

        [DataMember]
        public List<ReleaseDocument> ReleaseDocuments { get; set; }

        [DataMember]
        public List<ReleaseBatchFieldValue> ReleaseFieldValue { get; set; }

        [DataMember]
        public bool IsRejected { get; set; }
    }
}
