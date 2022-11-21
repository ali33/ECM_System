using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Ecm.Domain
{
    /// <summary>
    /// Represent the value of a field of document
    /// </summary>
    [DataContract]
    public sealed class FieldValue
    {
        /// <summary>
        /// Initialize the object
        /// </summary>
        public FieldValue()
        {
            TableFieldValue = new List<TableFieldValue>();
        }

        /// <summary>
        /// Identifier of the object
        /// </summary>
        [DataMember]
        public Guid Id { get; set; }

        /// <summary>
        /// Identifier of the <see cref="FieldMetaData"/> object contains the value.
        /// </summary>
        [DataMember]
        public Guid FieldId { get; set; }

        /// <summary>
        /// Identifier of the <see cref="Document"/> object.
        /// </summary>
        [DataMember]
        public Guid DocId { get; set; }

        /// <summary>
        /// The value of the field of document.
        /// </summary>
        [DataMember]
        public string Value { get; set; }

        /// <summary>
        /// The <see cref="FieldMetaData"/> object
        /// </summary>
        [DataMember]
        public FieldMetaData FieldMetaData { get; set; }

        /// <summary>
        /// Collection of values of sub-fields of the Table type field.
        /// </summary>
        [DataMember]
        public List<TableFieldValue> TableFieldValue { get; set; }
    }
}