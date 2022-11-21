using System;
using System.Runtime.Serialization;

namespace Ecm.Domain
{
    /// <summary>
    /// Represent the barcode configuration on a <see cref="DocumentType"/>. The barcode can be used to classify documents has the same type or can be used to do the lookup base on the value within the barcode.
    /// </summary>
    [DataContract]
    public class BarcodeConfiguration
    {
        /// <summary>
        /// Identifier of the configuration
        /// </summary>
        [DataMember]
        public Guid Id { get; set; }

        /// <summary>
        /// Identifier of the document type contains the configuration
        /// </summary>
        [DataMember]
        public Guid DocumentTypeId
        {
            get;
            set;
        }

        /// <summary>
        /// Include: CODABAR, CODE128, CODE25, CODE25NI, CODE39, CODE93, DATABAR, EAN8, DATAMATRIX, EAN13, MICROPDF417, PATCH, PDF417, SHORTCODE128, UPCA, UPCE
        /// </summary>
        [DataMember]
        public string BarcodeType
        {
            get;
            set;
        }

        /// <summary>
        /// The position in the document at wich the barcode will be used to do the action.
        /// </summary>
        [DataMember]
        public int BarcodePosition
        {
            get;
            set;
        }

        /// <summary>
        /// Whether the barcode is used to separate the documents.
        /// </summary>
        [DataMember]
        public bool IsDocumentSeparator
        {
            get;
            set;
        }

        /// <summary>
        /// Whether the separator page is removed out of document.
        /// </summary>
        [DataMember]
        public bool RemoveSeparatorPage
        {
            get;
            set;
        }

        /// <summary>
        /// Whether the value of barcode will be used to do lookup.
        /// </summary>
        [DataMember]
        public bool HasDoLookup
        {
            get;
            set;
        }

        /// <summary>
        /// Identifier of <see cref="FieldMetaData"/> in <see cref="DocumentType"/> will contain the barcode value.
        /// </summary>
        [DataMember]
        public Guid? MapValueToFieldId
        {
            get;
            set;
        }

        /// <summary>
        /// The <see cref="FieldMetaData"/> object contains the barcode value if any.
        /// </summary>
        [DataMember]
        public FieldMetaData FieldMetaData { get; set; }
    }
}