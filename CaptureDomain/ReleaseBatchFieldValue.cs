using System;
using System.Runtime.Serialization;

namespace Ecm.CaptureDomain
{
    [DataContract]
    public class ReleaseBatchFieldValue
    {
        [DataMember]
        public Guid Id { get; set; }

        [DataMember]
        public Guid ReleaseBatchId { get; set; }

        [DataMember]
        public Guid FieldId { get; set; }

        [DataMember]
        public string Value { get; set; }
    }
}
