using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Xml.Serialization;
using Ecm.Mvvm;
using System.ComponentModel;

namespace Ecm.ContentViewer.Model
{
    public class FieldModel : BaseDependencyProperty, IDataErrorInfo
    {
        private string _name;
        private string _displayName;
        private FieldDataType _dataType;
        private string _defaultValue;
        private bool _isLookup;
        private bool _isRequired;
        private bool _isRestricted;
        private int _displayOrder;
        private int _maxLength;
        private bool _useCurrentDate;
        private bool _isSelected;
        private bool _isValueValid;
        private string _validationScript;
        private string _validationPattern;
        private LookupInfoModel _lookupInfo;
        private ObservableCollection<LookupMapModel> _maps = new ObservableCollection<LookupMapModel>();
        private List<string> _runtimeLookupMap = new List<string>();
        private ObservableCollection<TableColumnModel> _children;

        public FieldModel()
        {
            DeletedChildrenIds = new List<Guid>();
            Picklists = new ObservableCollection<PicklistModel>();
        }

        public FieldModel(Action<FieldModel> action)
        {
            ErrorChecked = action;
            DeletedChildrenIds = new List<Guid>();
            Picklists = new ObservableCollection<PicklistModel>();
        }

        [XmlIgnore]
        public Action<FieldModel> ErrorChecked { get; set; }

        [XmlIgnore]
        public Guid Id { get; set; }

        [XmlIgnore]
        public Guid? ParentFieldId { get; set; }

        [XmlIgnore]
        public Guid DocTypeId { get; set; }

        [XmlIgnore]
        public Guid BatchTypeId { get; set; }

        [XmlIgnore]
        public string UniqueId { get; set; }

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
        public string DisplayName
        {
            get { return _displayName; }
            set
            {
                _displayName = value;
                OnPropertyChanged("DisplayName");
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
                else if (IsBatchProperty)
                {
                    return "[" + DisplayName + "]";
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
        public bool IsValueValid
        {
            get { return _isValueValid; }
            set
            {
                _isValueValid = value;
                OnPropertyChanged("IsValueValid");
            }
        }

        [XmlIgnore]
        public bool IsBatchProperty { get; set; }

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
        public List<string> RuntimeLookupMaps
        {
            get { return _runtimeLookupMap; }
            set
            {
                _runtimeLookupMap = value;
                OnPropertyChanged("RuntimeLookupMaps");
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
        public IList<LookupMapModel> DeletedMaps { get; set; }

        //public List<Picklist> DeletedPicklist { get; set; }

        [XmlIgnore]
        public List<Guid> DeletedChildrenIds { get; set; }

        [XmlIgnore]
        public ObservableCollection<PicklistModel> Picklists { get; set; }

        [XmlIgnore]
        public OCRTemplateZoneModel OCRTemplateZone { get; set; }

        [XmlIgnore]
        public string ValidationScript
        {
            get { return _validationScript; }
            set
            {
                _validationScript = value;
                OnPropertyChanged("ValidationScript");
            }
        }

        [XmlIgnore]
        public string ValidationPattern
        {
            get { return _validationPattern; }
            set
            {
                _validationPattern = value;
                OnPropertyChanged("ValidationPattern");
            }
        }

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
