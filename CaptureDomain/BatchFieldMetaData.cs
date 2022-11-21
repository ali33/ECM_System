using System;
using System.Runtime.Serialization;
using Ecm.LookupDomain;
using System.Collections.Generic;

namespace Ecm.CaptureDomain
{
    [DataContract]
    public class BatchFieldMetaData
    {

        public const string _sysDocCount = "Doc count";
        public const string _sysPageCount = "Page count";
        public const string _sysCreatedBy = "Created by";
        public const string _sysCreatedOn = "Created on";
        public const string _sysModifiedBy = "Modified by";
        public const string _sysModifiedOn = "Modified on";
        public const string _sysBatchName = "Batch name";

        [DataMember]
        public Guid Id { get; set; }

        [DataMember]
        public Guid BatchTypeId { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string DefaultValue { get; set; }

        /// <summary>
        /// Data type of the field. Currently CloudECM only supports data types: String, Integer, Decimal, Picklist, Boolean, Date.
        /// </summary>
        [DataMember]
        public string DataType { get; set; }

        /// <summary>
        /// Internal use only.
        /// </summary>
        [DataMember]
        public FieldDataType DataTypeEnum
        {
            get { return (FieldDataType)Enum.Parse(typeof(FieldDataType), DataType); }
            set { DataType = value.ToString(); }
        }

        [DataMember]
        public bool IsSystemField { get; set; }

        [DataMember]
        public int MaxLength { get; set; }

        [DataMember]
        public bool UseCurrentDate { get; set; }

        [DataMember]
        public int DisplayOrder { get; set; }

        [DataMember]
        public string UniqueId { get; set; }

        [DataMember]
        public bool IsLookup { get; set; }

        [DataMember]
        public string LookupXml { get; set; }

        [DataMember]
        public LookupInfo LookupInfo { get; set; }

        [DataMember]
        public string LookupInfoXml { get; set; }

        [DataMember]
        public List<LookupMap> Maps { get; set; }

        [DataMember]
        public List<string> RuntimeLookupMaps { get; set; }
    }
}
