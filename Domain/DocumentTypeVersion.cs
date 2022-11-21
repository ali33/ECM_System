using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Ecm.Domain
{
    /// <summary>
    /// Represent the version of the <see cref="DocumentType"/>
    /// </summary>
    [DataContract]
    public class DocumentTypeVersion
    {
        /// <summary>
        /// Initialize the object of the version
        /// </summary>
        public DocumentTypeVersion()
        {
            FieldMetaDataVersions = new List<FieldMetadataVersion>();
        }

        /// <summary>
        /// Identifier of the version
        /// </summary>
        [DataMember]
        public Guid Id { get; set; }

        /// <summary>
        /// The name of the document type in this version
        /// </summary>
        [DataMember]
        public string Name { get; set; }

        /// <summary>
        /// The backup date when the document type is created.
        /// </summary>
        [DataMember]
        public DateTime CreatedDate { get; set; }

        /// <summary>
        /// The backup date when the document type is modified.
        /// </summary>
        [DataMember]
        public DateTime ModifiedDate { get; set; }

        /// <summary>
        /// The user name of the <see cref="User"/> modify the document type.
        /// </summary>
        [DataMember]
        public string ModifiedBy { get; set; }

        /// <summary>
        /// The user name of the <see cref="User"/> create the document type.
        /// </summary>
        [DataMember]
        public string CreatedBy { get; set; }

        /// <summary>
        /// Whether this document type is used for Outlook Add-in module.
        /// </summary>
        [DataMember]
        public bool IsOutlook { get; set; }

        /// <summary>
        /// Collection of version of <see cref="FieldMetaData"/> in the document type.
        /// </summary>
        [DataMember]
        public List<FieldMetadataVersion> FieldMetaDataVersions { get; set; }
    }
}