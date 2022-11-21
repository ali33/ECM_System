using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Xml.Serialization;
using Ecm.CaptureDomain;
using Ecm.Mvvm;

namespace Ecm.CaptureModel
{
    public class BatchFieldMetaDataModel : BaseDependencyProperty, IDataErrorInfo
    {
        private string _name;
        private FieldDataType _dataType;
        private string _defaultValue;
        private int _displayOrder;
        private int _maxLength;
        private bool _useCurrentDate;
        private bool _isSelected;

        [XmlIgnore]
        public Action<BatchFieldMetaDataModel> ErrorChecked { get; set; }

        public Guid Id { get; set; }

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

                MaxLength = 0;
                DefaultValue = string.Empty;
                OnPropertyChanged("DataType");
            }
        }

        [XmlIgnore]
        public bool IsSystemField { get; set; }

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
