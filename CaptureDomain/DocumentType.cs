using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Ecm.CaptureDomain
{
    [DataContract]
    public class DocumentType
    {
        public DocumentType()
        {
            Fields = new List<DocumentFieldMetaData>();
            DeletedFields = new List<Guid>();
        }

        [DataMember]
        public Guid Id { get; set; }

        [DataMember]
        public Guid BatchTypeId { get; set; }

        [DataMember]
        public string UniqueId { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string Description { get; set; }

        [DataMember]
        public byte[] Icon { get; set; }

        /// <summary>
        /// The Base64 string of icon batch type using for Mobile.
        /// </summary>
        [DataMember]
        public string IconBase64 { get; set; }

        [DataMember]
        public DateTime CreatedDate { get; set; }

        [DataMember]
        public string CreatedBy { get; set; }

        [DataMember]
        public DateTime? ModifiedDate { get; set; }

        [DataMember]
        public string ModifiedBy { get; set; }

        [DataMember]
        public List<DocumentFieldMetaData> Fields { get; set; }

        [DataMember]
        public List<Guid> DeletedFields { get; set; }

        /// <summary>
        /// Contains the OCR template for the documen type.
        /// </summary>
        [DataMember]
        public OCRTemplate OCRTemplate { get; set; }

        ///// <summary>
        ///// Contains the collections of <see cref="DocumentFieldMetaData"/> barcode configuration. See <see cref="BarcodeConfiguration"/> for more information.
        ///// </summary>
        //[DataMember]
        //public List<BarcodeConfiguration> BarcodeConfigurations { get; set; }

        [DataMember]
        public DocumentTypePermission DocumentTypePermission { get; set; }

        /// <summary>
        /// This function is use for set default value of type DateTime to universal time (for Json serialization not throw exception). 
        /// </summary>
        /// <param name="context"></param>
        [OnSerializing()]
        internal void OnSerializingMethod(StreamingContext context)
        {
            // Adjust all member have type of DateTime

            if (DateTime.MinValue == CreatedDate)
            {
                CreatedDate = DateTime.MinValue.ToUniversalTime();
            }
        }
    }
}
