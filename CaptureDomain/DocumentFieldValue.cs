using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Ecm.CaptureDomain
{
    /// <summary>
    /// Represent the value of a field of document
    /// </summary>
    [DataContract]
    public class DocumentFieldValue
    {
        [DataMember]
        public Guid Id { get; set; }

        [DataMember]
        public Guid DocId { get; set; }

        /// <summary>
        /// Identifier of the <see cref="DocumentFieldMetaData"/> object contains the value.
        /// </summary>
        [DataMember]
        public Guid FieldId { get; set; }

        [DataMember]
        public string Value { get; set; }

        /// <summary>
        /// The <see cref="DocumentFieldMetaData"/> object
        /// </summary>
        [DataMember]
        public DocumentFieldMetaData FieldMetaData { get; set; }

        /// <summary>
        /// Collection of values of sub-fields of the Table type field.
        /// </summary>
        [DataMember]
        public List<TableFieldValue> TableFieldValue { get; set; }

        [DataMember]
        public List<Guid> DeleteTableFieldValueIds { get; set; }

        public bool BarcodeOverride { get; set; }

        public string BarcodeValue { get; set; }
    }
}
