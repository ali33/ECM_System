using System;
using System.Runtime.Serialization;

namespace Ecm.Domain
{
    /// <summary>
    /// Represent the datasource of the Picklist type <see cref="FieldMetaData"/>
    /// </summary>
    [DataContract]
    public sealed class Picklist
    {
        /// <summary>
        /// Identifier of the object
        /// </summary>
        [DataMember]
        public Guid Id { get; set; }

        /// <summary>
        /// Identifier of the <see cref="FieldMetaData"/> object
        /// </summary>
        [DataMember]
        public Guid FieldId { get; set; }

        /// <summary>
        /// Value of each pick list item
        /// </summary>
        [DataMember]
        public string Value { get; set; }
    }
}