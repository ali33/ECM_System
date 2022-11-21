using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Ecm.Domain
{
    /// <summary>
    /// Represent the document type or the content type that the enterprise want to manage.
    /// </summary>
    [DataContract]
    public class DocumentType
    {
        /// <summary>
        /// Initialize new object for <see cref="DocumentType"/>
        /// </summary>
        public DocumentType()
        {
            FieldMetaDatas = new List<FieldMetaData>();
            DeletedFields = new List<FieldMetaData>();
            BarcodeConfigurations = new List<BarcodeConfiguration>();
        }

        /// <summary>
        /// Identifier for document type
        /// </summary>
        [DataMember]
        public Guid Id { get; set; }
        
        /// <summary>
        /// Name of the document type
        /// </summary>
        [DataMember]
        public string Name { get; set; }

        /// <summary>
        /// The time when the document type is created
        /// </summary>
        [DataMember]
        public DateTime CreatedDate { get; set; }

        /// <summary>
        /// The time when the document type is modified
        /// </summary>
        [DataMember]
        public DateTime ModifiedDate { get; set; }

        /// <summary>
        /// The user name of the user modify the documen type
        /// </summary>
        [DataMember]
        public string ModifiedBy { get; set; }

        /// <summary>
        /// The user name of the user create the document type
        /// </summary>
        [DataMember]
        public string CreatedBy { get; set; }

        /// <summary>
        /// The picture represent for the documen type
        /// </summary>
        [DataMember]
        public byte[] Icon { get; set; }

        /// <summary>
        /// Whether this document type is used for Outlook add-in module.
        /// </summary>
        [DataMember]
        public bool IsOutlook { get; set; }

        /// <summary>
        /// Contains the OCR template for the documen type.
        /// </summary>
        [DataMember]
        public OCRTemplate OCRTemplate { get; set; }

        /// <summary>
        /// Contains the annotation permission of the login user retrieve the document type
        /// </summary>
        [DataMember]
        public AnnotationPermission AnnotationPermission { get; set; }

        /// <summary>
        /// Contains the operational permission of the login user retrieve the document type
        /// </summary>
        [DataMember]
        public DocumentTypePermission DocumentTypePermission { get; set; }

        /// <summary>
        /// Contains the collections of <see cref="FieldMetaData"/> objects of the document type
        /// </summary>
        [DataMember]
        public List<FieldMetaData> FieldMetaDatas { get; set; }

        /// <summary>
        /// Contains the collections of deleted <see cref="FieldMetaData"/> objects of the document type. This property is used for performance boost when the document type is updated.
        /// </summary>
        [DataMember]
        public List<FieldMetaData> DeletedFields { get; set; }

        /// <summary>
        /// Contains the collections of <see cref="FieldMetaData"/> barcode configuration. See <see cref="BarcodeConfiguration"/> for more information.
        /// </summary>
        [DataMember]
        public List<BarcodeConfiguration> BarcodeConfigurations { get; set; }
    }
}