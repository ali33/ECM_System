using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Ecm.CaptureDomain
{
    [DataContract]
    public class ReleaseDocumentFieldValue
    {
        [DataMember]
        public Guid Id { get; set; }

        [DataMember]
        public Guid ReleaseDocId { get; set; }

        [DataMember]
        public Guid FieldId { get; set; }

        [DataMember]
        public string Value { get; set; }

        [DataMember]
        public DocumentFieldMetaData FieldMetaData { get; set; }

        /// <summary>
        /// Collection of values of sub-fields of the Table type field.
        /// </summary>
        [DataMember]
        public List<ReleaseTableFieldValue> TableFieldValue { get; set; }
    }
}
