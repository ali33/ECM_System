using System;
using System.Runtime.Serialization;
using System.Collections.Generic;

namespace Ecm.Domain
{
    /// <summary>
    /// Represent a backup version of <see cref="FieldMetaData"/> when document type is updated. See <see cref="FieldMetaData"/> class to get the detail information of properties of this class.
    /// </summary>
    [DataContract]
    public sealed class FieldMetadataVersion
    {
        public FieldMetadataVersion()
        {
            LookupMaps = new List<LookupMap>();
            DeletedLookupMaps = new List<LookupMap>();
            Picklists = new List<Picklist>();
            DeletedPicklists = new List<Picklist>();
        }

        [DataMember]
        public Guid Id { get; set; }

        [DataMember]
        public Guid DocTypeId { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string DefautValue { get; set; }

        [DataMember]
        public string DataType { get; set; }

        [DataMember]
        public FieldDataType DataTypeEnum
        {
            get { return (FieldDataType)Enum.Parse(typeof(FieldDataType), DataType); }
            set { DataType = value.ToString(); }
        }

        [DataMember]
        public bool IsLookup { get; set; }

        [DataMember]
        public int DisplayOrder { get; set; }

        [DataMember]
        public bool IsRestricted { get; set; }

        [DataMember]
        public bool IsRequired { get; set; }

        [DataMember]
        public bool IsSystemField { get; set; }

        //[DataMember]
        //public string FieldUniqueId { get; set; }

        [DataMember]
        public LookupInfo LookupInfo { get; set; }

        [DataMember]
        public List<LookupMap> LookupMaps { get; set; }

        [DataMember]
        public List<LookupMap> DeletedLookupMaps { get; set; }

        [DataMember]
        public Guid? ParentFieldID { get; set; }

        [DataMember]
        public List<Picklist> Picklists { get; set; }

        [DataMember]
        public List<Picklist> DeletedPicklists { get; set; }

        [DataMember]
        public int MaxLength { get; set; }
    }
}