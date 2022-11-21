using System.Collections.Generic;
using System.Runtime.Serialization;
using System;

namespace Ecm.Domain
{
    /// <summary>
    /// Represent the data that a document want to store.
    /// </summary>
    [DataContract]
    public sealed class FieldMetaData
    {

        public const string _sysPageCount = "Page count";
        public const string _sysCreatedBy = "Created by";
        public const string _sysCreatedOn = "Created on";
        public const string _sysModifiedBy = "Modified by";
        public const string _sysModifiedOn = "Modified on";
        public const string _sysMajorVersion = "Major version";
        public const string _sysMinorVersion = "Minor version";

        /// <summary>
        /// Initialize a new object of <see cref="FieldMetaData"/>
        /// </summary>
        public FieldMetaData()
        {
            LookupMaps = new List<LookupMap>();
            DeletedLookupMaps = new List<LookupMap>();
            Picklists = new List<Picklist>();
            DeleteChildIds = new List<Guid>();
            Children = new List<FieldMetaData>();
        }

        /// <summary>
        /// Identifier of the object
        /// </summary>
        [DataMember]
        public Guid Id { get; set; }

        /// <summary>
        /// Identifier of the document type contains this field
        /// </summary>
        [DataMember]
        public Guid DocTypeId { get; set; }

        /// <summary>
        /// Name of the field
        /// </summary>
        [DataMember]
        public string Name { get; set; }

        /// <summary>
        /// Default value of the field when a document is created.
        /// </summary>
        [DataMember]
        public string DefautValue { get; set; }

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

        /// <summary>
        /// Whether this field has lookup configuration or not.
        /// </summary>
        [DataMember]
        public bool IsLookup { get; set; }

        /// <summary>
        /// The order of the field when it's showed on the UI
        /// </summary>
        [DataMember]
        public int DisplayOrder { get; set; }

        /// <summary>
        /// Whether this field is restricted which mean that user need to have permission to view it.
        /// </summary>
        [DataMember]
        public bool IsRestricted { get; set; }

        /// <summary>
        /// Whether the value of this field requires to have value when a document is created.
        /// </summary>
        [DataMember]
        public bool IsRequired { get; set; }

        /// <summary>
        /// Whether this field is system field or not. There are some system fields automatically created when a document type is created.
        /// </summary>
        [DataMember]
        public bool IsSystemField { get; set; }

        /// <summary>
        /// This property is used internally.
        /// </summary>
       // [DataMember]
        //public string FieldUniqueId { get; set; }

        /// <summary>
        /// Define the lookup information which contains the server, datasource as well as the query,...
        /// </summary>
        [DataMember]
        public LookupInfo LookupInfo { get; set; }

        /// <summary>
        /// Collection of mappings between fields of document type and columns in the lookup query
        /// </summary>
        [DataMember]
        public List<LookupMap> LookupMaps { get; set; }

        /// <summary>
        /// Collection of deleted mappings when a field is updated.
        /// </summary>
        [DataMember]
        public List<LookupMap> DeletedLookupMaps { get; set; }

        /// <summary>
        /// Allow define sub-fields of a Table type field.
        /// </summary>
        [DataMember]
        public Guid? ParentFieldId { get; set; }

        /// <summary>
        /// Collection of values of Picklist type field.
        /// </summary>
        [DataMember]
        public List<Picklist> Picklists { get; set; }

        /// <summary>
        /// Contain the zone on the image whic OCR engine will extract data for this field when a document is created.
        /// </summary>
        [DataMember]
        public OCRTemplateZone OCRTemplateZone { get; set; }

        /// <summary>
        /// Max length of String type field.
        /// </summary>
        [DataMember]
        public int MaxLength { get; set; }

        /// <summary>
        /// Apply the current date for Date type field when a document is created.
        /// </summary>
        [DataMember]
        public bool UseCurrentDate { get; set; }

        /// <summary>
        /// List of children field
        /// </summary>
        [DataMember]
        public List<FieldMetaData> Children { get; set; }

        /// <summary>
        /// List of pending deleted children field
        /// </summary>
        [DataMember]        
        public List<Guid> DeleteChildIds { get; set; }

        /// <summary>
        /// Lookup data mapping
        /// </summary>
        [DataMember]
        public string LookupXML { get; set; }

        /// <summary>
        /// Define field can link or not
        /// </summary>
        [DataMember]
        public bool IsLinkable { get; set; }
    }
}