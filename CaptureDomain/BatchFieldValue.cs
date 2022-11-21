using System;
using System.Runtime.Serialization;

namespace Ecm.CaptureDomain
{
    /// <summary>
    /// Represent the value of a field of batch
    /// </summary>
    [DataContract]
    public class BatchFieldValue
    {
        [DataMember]
        public Guid Id { get; set; }

        [DataMember]
        public Guid BatchId { get; set; }

        /// <summary>
        /// Identifier of the <see cref="BatchFieldMetaData"/> object contains the value.
        /// </summary>
        [DataMember]
        public Guid FieldId { get; set; }

        [DataMember]
        public string Value { get; set; }

        /// <summary>
        /// The <see cref="BatchFieldMetaData"/> object
        /// </summary>
        [DataMember]
        public BatchFieldMetaData FieldMetaData { get; set; }

        public bool BarcodeOverride { get; set; }

        public string BarcodeValue { get; set; }
    }
}
