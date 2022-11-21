using System;
using System.Runtime.Serialization;

namespace Ecm.CaptureDomain
{
    [DataContract]
    public class Picklist
    {
        [DataMember]
        public Guid Id { get; set; }

        /// <summary>
        /// Identifier of the <see cref="DocumentFieldMetaData"/> object
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
