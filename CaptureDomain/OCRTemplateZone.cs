using System;
using System.Runtime.Serialization;

namespace Ecm.CaptureDomain
{
    [DataContract]
    public class OCRTemplateZone
    {
        [DataMember]
        public Guid Id { get; set; }

        [DataMember]
        public Guid OCRTemplatePageId { get; set; }

        [DataMember]
        public Guid FieldMetaDataId { get; set; }

        [DataMember]
        public double Top { get; set; }

        [DataMember]
        public double Left { get; set; }

        [DataMember]
        public double Width { get; set; }

        [DataMember]
        public double Height { get; set; }

        [DataMember]
        public string CreatedBy { get; set; }

        [DataMember]
        public DateTime? CreatedOn { get; set; }

        [DataMember]
        public string ModifiedBy { get; set; }

        [DataMember]
        public DateTime ModifiedOn { get; set; }

        [DataMember]
        public DocumentFieldMetaData FieldMetaData { get; set; }

        /// <summary>
        /// This function is use for set default value of type DateTime to universal time (for Json serialization not throw exception). 
        /// </summary>
        /// <param name="context"></param>
        [OnSerializing()]
        internal void OnSerializingMethod(StreamingContext context)
        {
            // Adjust all member have type of DateTime

            if (DateTime.MinValue == ModifiedOn)
            {
                ModifiedOn = DateTime.MinValue.ToUniversalTime();
            }
        }
    }
}
