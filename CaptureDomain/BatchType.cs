using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Ecm.BarcodeDomain;

namespace Ecm.CaptureDomain
{
    [DataContract]
    public class BatchType
    {
        public BatchType()
        {
            Fields = new List<BatchFieldMetaData>();
            DeletedFields = new List<Guid>();
            DocTypes = new List<DocumentType>();
            DeletedDocTypes = new List<Guid>();
        }

        [DataMember]
        public Guid Id { get; set; }

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
        public Guid WorkflowDefinitionId { get; set; }

        [DataMember]
        public bool IsWorkflowDefined { get; set; }

        [DataMember]
        public List<BatchFieldMetaData> Fields { get; set; }

        [DataMember]
        public List<Guid> DeletedFields { get; set; }

        [DataMember]
        public BatchTypePermission BatchTypePermission { get; set; }

        [DataMember]
        public List<DocumentType> DocTypes { get; set; }

        [DataMember]
        public List<Guid> DeletedDocTypes { get; set; }

        [DataMember]
        public string BarcodeConfigurationXml { get; set; }

        [DataMember]
        public BatchBarcodeConfiguration BarcodeConfiguration { get; set; }

        [DataMember]
        public bool IsApplyForOutlook { get; set; }
    }

    [DataContract]
    public class BatchTypeMobile
    {
        [DataMember]
        public Guid Id { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public List<BatchFieldMetaData> Fields { get; set; }

        [DataMember]
        public BatchTypePermission BatchTypePermission { get; set; }

        [DataMember]
        public List<DocumentType> DocTypes { get; set; }
    }

}
