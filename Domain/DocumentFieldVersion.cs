using System;
using System.Runtime.Serialization;

namespace Ecm.Domain
{
    /// <summary>
    /// Represent the version of the value of <see cref="FieldMetaData"/> in each version of document.
    /// </summary>
    [DataContract]
    public class DocumentFieldVersion
    {
        /// <summary>
        /// Identifier of the version of document
        /// </summary>
        [DataMember]
        public Guid DocVersionID { get; set; }

        /// <summary>
        /// Identifier of the value
        /// </summary>
        [DataMember]
        public Guid Id { get; set; }

        /// <summary>
        /// Identifier of the original document
        /// </summary>
        [DataMember]
        public Guid DocId
        {
            get;
            set;
        }

        /// <summary>
        /// Identifier of <see cref="FieldMetaData"/> contains the value.
        /// </summary>
        [DataMember]
        public Guid FieldId
        {
            get;
            set;
        }

        /// <summary>
        /// The value of the version
        /// </summary>
        [DataMember]
        public string Value
        {
            get;
            set;
        }

        /// <summary>
        /// The version object of <see cref="FieldMetaData"/>
        /// </summary>
        [DataMember]
        public FieldMetadataVersion FieldMetadataVersion { get; set; }
    }
}