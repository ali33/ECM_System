using System.Xml.Serialization;
using Ecm.Domain;

using System.ComponentModel;
using System;

using System.Collections.Generic;

namespace ArchiveMVC5.Models
{
    public class FieldMetaDataModel
    {
        private Guid _id;
        private Guid _docTypeId;
        private string _name;
        private FieldDataType _dataType;
        private string _defaultValue;
        private bool _isLookup;
        private bool _isRequired;
        private bool _isRestricted;
        private int _displayOrder;
        private int _maxLength;
        private bool _useCurrentDate;
        private bool _isSelected;
        private LookupInfoModel _lookupInfo;
        private List<LookupMapModel> _maps;
        private List<TableColumnModel> _children;

        public FieldMetaDataModel()
        {
            Maps = new List<LookupMapModel>();
            Children = new List<TableColumnModel>();
            DeletedMaps = new List<LookupMapModel>();
            Picklists = new List<PicklistModel>();
            DeletedChildrenIds = new List<Guid>();
        }

        public FieldMetaDataModel(Action<FieldMetaDataModel> action)
        {
            ErrorChecked = action;
            Maps = new List<LookupMapModel>();
            Children = new List<TableColumnModel>();
            DeletedMaps = new List<LookupMapModel>();
            Picklists = new List<PicklistModel>();
            DeletedChildrenIds = new List<Guid>();
        }

        [XmlIgnore]
        public Action<FieldMetaDataModel> ErrorChecked { get; set; }

        public Guid Id
        {
            get { return _id; }
            set
            {
                _id = value;
            }
        }

        public Guid? ParentFieldId { get; set; }

        [XmlIgnore]
        public Guid DocTypeId
        {
            get { return _docTypeId; }
            set
            {
                _docTypeId = value;
            }
        }

        [XmlIgnore]
        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
            }
        }

        [XmlIgnore]
        public string QueryDisplayName
        {
            get
            {
                if (IsSystemField)
                {
                    return "[" + Name + "]";
                }

                return Name;
            }
        }

        [XmlIgnore]
        public FieldDataType DataType
        {
            get { return _dataType; }
            set
            {
                _dataType = value;

                if (_dataType == FieldDataType.Table)
                {
                    _children = new List<TableColumnModel>();
                }

                MaxLength = 0;
                DefaultValue = string.Empty;
            }
        }

        [XmlIgnore]
        public bool IsLookup
        {
            get { return _isLookup; }
            set
            {
                _isLookup = value;
            }
        }

        [XmlIgnore]
        public bool IsRequired
        {
            get { return _isRequired; }
            set
            {
                _isRequired = value;
            }
        }

        [XmlIgnore]
        public bool IsSystemField { get; set; }

        [XmlIgnore]
        public bool IsRestricted
        {
            get { return _isRestricted; }
            set
            {
                _isRestricted = value;
            }
        }

        [XmlIgnore]
        public int DisplayOrder
        {
            get { return _displayOrder; }
            set
            {
                _displayOrder = value;
            }
        }

        [XmlIgnore]
        public int MaxLength
        {
            get { return _maxLength; }
            set
            {
                _maxLength = value;
                DefaultValue = string.Empty;
            }
        }

        [XmlIgnore]
        public string DefaultValue
        {
            get { return _defaultValue; }
            set
            {
                _defaultValue = value;
            }
        }

        [XmlIgnore]
        public bool UseCurrentDate
        {
            get { return _useCurrentDate; }
            set
            {
                _useCurrentDate = value;
            }
        }

        [XmlIgnore]
        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                _isSelected = value;
            }
        }

        [XmlIgnore]
        public string FieldUniqueId { get; set; }

        [XmlIgnore]
        public LookupInfoModel LookupInfo
        {
            get { return _lookupInfo; }
            set
            {
                _lookupInfo = value;
            }
        }

        [XmlIgnore]
        public List<LookupMapModel> Maps
        {
            get { return _maps; }
            set
            {
                _maps = value;
            }
        }

        [XmlIgnore]
        public List<TableColumnModel> Children
        {
            get { return _children; }
            set
            {
                _children = value;
            }
        }

        [XmlIgnore]
        public List<LookupMapModel> DeletedMaps { get; set; }

        [XmlIgnore]
        public List<Guid> DeletedChildrenIds { get; set; }

        [XmlIgnore]
        public List<PicklistModel> Picklists { get; set; }

        [XmlIgnore]
        public OCRTemplateZoneModel OCRTemplateZone { get; set; }

        [XmlIgnore]
        public Dictionary<string, string> ErrorMessages { get; set; }

    }
}