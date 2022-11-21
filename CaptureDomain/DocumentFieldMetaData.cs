using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Ecm.LookupDomain;

namespace Ecm.CaptureDomain
{
    [DataContract]
    public class DocumentFieldMetaData
    {
        public const string _sysPageCount = "Page count";
        public const string _sysCreatedBy = "Created by";
        public const string _sysCreatedOn = "Created on";
        public const string _sysModifiedBy = "Modified by";
        public const string _sysModifiedOn = "Modified on";
        
        public DocumentFieldMetaData()
        {
            Picklists = new List<Picklist>();
        }

        [DataMember]
        public Guid Id { get; set; }

        [DataMember]
        public Guid DocTypeId { get; set; }

        /// <summary>
        /// Allow define sub-fields of a Table type field.
        /// </summary>
        [DataMember]
        public Guid? ParentFieldId { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string DefaultValue { get; set; }

        [DataMember]
        public string DataType { get; set; }

        [DataMember]
        public FieldDataType DataTypeEnum
        {
            get { return (FieldDataType)Enum.Parse(typeof(FieldDataType), DataType); }
            set { DataType = value.ToString(); }
        }

        [DataMember]
        public bool IsRestricted { get; set; }

        [DataMember]
        public bool IsRequired { get; set; }

        [DataMember]
        public bool IsSystemField { get; set; }

        [DataMember]
        public int MaxLength { get; set; }

        [DataMember]
        public bool UseCurrentDate { get; set; }

        [DataMember]
        public int DisplayOrder { get; set; }

        [DataMember]
        public bool IsLookup { get; set; }

        [DataMember]
        public string UniqueId { get; set; }

        [DataMember]
        public List<Picklist> Picklists { get; set; }

        [DataMember]
        public LookupInfo LookupInfo { get; set; }

        [DataMember]
        public string LookupInfoXml { get; set; }

        [DataMember]
        public List<LookupMap> LookupMaps { get; set; }

        [DataMember]
        public List<string> RuntimeLookupMaps { get; set; }

        [DataMember]
        public string LookupXml { get; set; }
        //[DataMember]
        //public List<LookupMap> DeletedLookupMaps { get; set; }

        /// <summary>
        /// Contain the zone on the image whic OCR engine will extract data for this field when a document is created.
        /// </summary>
        [DataMember]
        public OCRTemplateZone OCRTemplateZone { get; set; }

        [DataMember]
        public string ValidationScript { get; set; }

        [DataMember]
        public string ValidationPattern { get; set; }

        [DataMember]
        public List<DocumentFieldMetaData> Children { get; set; }

        [DataMember]
        public List<Guid> DeleteChildIds { get; set; }

    }
}
