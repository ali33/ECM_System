using System.Xml.Serialization;
using Ecm.Domain;
using Ecm.Mvvm;
using System.ComponentModel;
using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;

namespace Ecm.Model
{
    public class FieldMetaDataModel : BaseDependencyProperty, IDataErrorInfo
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
        private ObservableCollection<LookupMapModel> _maps;
        private ObservableCollection<TableColumnModel> _children;

        public FieldMetaDataModel()
        {
            Maps = new ObservableCollection<LookupMapModel>();
            Children = new ObservableCollection<TableColumnModel>();
            DeletedMaps = new List<LookupMapModel>();
            Picklists = new ObservableCollection<PicklistModel>();
            DeletedChildrenIds = new List<Guid>();
        }

        public FieldMetaDataModel(Action<FieldMetaDataModel> action)
        {
            ErrorChecked = action;
            Maps = new ObservableCollection<LookupMapModel>();
            Children = new ObservableCollection<TableColumnModel>();
            DeletedMaps = new List<LookupMapModel>();
            Picklists = new ObservableCollection<PicklistModel>();
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
                OnPropertyChanged("Id");
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
                OnPropertyChanged("DocTypeId");
            }
        }

        [XmlIgnore]
        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
                OnPropertyChanged("Name");
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
                    _children = new ObservableCollection<TableColumnModel>();
                }

                MaxLength = 0;
                DefaultValue = string.Empty;
                OnPropertyChanged("DataType");
            }
        }

        [XmlIgnore]
        public bool IsLookup
        {
            get { return _isLookup; }
            set
            {
                _isLookup = value;
                OnPropertyChanged("IsLookup");
            }
        }

        [XmlIgnore]
        public bool IsRequired
        {
            get { return _isRequired; }
            set
            {
                _isRequired = value;
                OnPropertyChanged("IsRequired");
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
                OnPropertyChanged("IsRestricted");
            }
        }

        [XmlIgnore]
        public int DisplayOrder
        {
            get { return _displayOrder; }
            set
            {
                _displayOrder = value;
                OnPropertyChanged("DisplayOrder");
            }
        }

        [XmlIgnore]
        public int MaxLength
        {
            get { return _maxLength; }
            set
            {
                _maxLength = value;
                OnPropertyChanged("MaxLength");
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
                OnPropertyChanged("DefaultValue");
            }
        }

        [XmlIgnore]
        public bool UseCurrentDate
        {
            get { return _useCurrentDate; }
            set
            {
                _useCurrentDate = value;
                OnPropertyChanged("UseCurrentDate");
            }
        }

        [XmlIgnore]
        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                _isSelected = value;
                OnPropertyChanged("IsSelected");
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
                OnPropertyChanged("LookupInfo");
            }
        }

        [XmlIgnore]
        public ObservableCollection<LookupMapModel> Maps
        {
            get { return _maps; }
            set
            {
                _maps = value;
                OnPropertyChanged("Maps");
            }
        }

        [XmlIgnore]
        public ObservableCollection<TableColumnModel> Children
        {
            get { return _children; }
            set
            {
                _children = value;
                OnPropertyChanged("Children");
            }
        }

        [XmlIgnore]
        public List<LookupMapModel> DeletedMaps { get; set; }

        [XmlIgnore]
        public List<Guid> DeletedChildrenIds { get; set; }

        [XmlIgnore]
        public ObservableCollection<PicklistModel> Picklists { get; set; }

        [XmlIgnore]
        public OCRTemplateZoneModel OCRTemplateZone { get; set; }

        [XmlIgnore]
        public bool HasErrorWithDefaultValue { get; set; }

        [XmlIgnore]
        public bool HasErrorWithName { get; set; }

        [XmlIgnore]
        public Dictionary<string, string> ErrorMessages { get; set; }

        [XmlIgnore]
        public string Error
        {
            get { return null; }
        }

        [XmlIgnore]
        public string this[string columnName]
        {
            get
            {
                string errorMsg = string.Empty;

                if (columnName == "DefaultValue")
                {
                    int outIntResult;
                    if (!string.IsNullOrEmpty(DefaultValue) && DataType == FieldDataType.Integer && !int.TryParse(DefaultValue, out outIntResult))
                    {
                        errorMsg = ErrorMessages["uiInvalidIntegerValue"];
                    }

                    decimal outDecResult;
                    if (!string.IsNullOrEmpty(DefaultValue) && DataType == FieldDataType.Decimal && !decimal.TryParse(DefaultValue, out outDecResult))
                    {
                        errorMsg = ErrorMessages["uiInvalidDecimalValue"];
                    }

                    HasErrorWithDefaultValue = !string.IsNullOrEmpty(errorMsg);
                }

                if (columnName == "Name")
                {
                    if (ErrorChecked != null)
                    {
                        ErrorChecked(this);
                    }

                    if (HasErrorWithName)
                    {
                        errorMsg = ErrorMessages["uiFieldNameExisted"];
                    }
                }

                return errorMsg;
            }
        }
    }
}