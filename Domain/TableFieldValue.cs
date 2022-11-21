using System;
using System.Runtime.Serialization;

namespace Ecm.Domain
{
    /// <summary>
    /// Represent the value of sub-fields in the specific Table <see cref="FieldMetaData"/> of a <see cref="DocumentType"/>
    /// </summary>
    [DataContract]
    public sealed class TableFieldValue
    {
        /// <summary>
        /// Identifier of the field
        /// </summary>
        [DataMember]
        public Guid Id { get; set; }

        /// <summary>
        /// Document identifier
        /// </summary>
        [DataMember]
        public Guid DocId { get; set; }

        /// <summary>
        /// The row number of the value
        /// </summary>
        [DataMember]
        public int RowNumber { get; set; }

        /// <summary>
        /// Value of the sub-field
        /// </summary>
        [DataMember]
        public string Value { get; set; }

        [DataMember]
        public Guid FieldId { get; set; }

        /// <summary>
        /// Sub field that is defined the value in document
        /// </summary>
        [DataMember]
        public FieldMetaData Field { get; set; }
    }
}