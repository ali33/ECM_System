using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;

using Ecm.Mvvm;
using Ecm.Domain;
using System.Resources;
using System.Reflection;
using System.Windows.Controls;

namespace Ecm.Admin.ViewModel
{
    public class TableColumnViewModel : ComponentViewModel, IDataErrorInfo
    {
        public event EventHandler RequestRemove;

        public const string _dataTypeDate = "Date";
        public const string _dataTypeDecimal = "Decimal";
        public const string _dataTypeInteger = "Integer";
        public const string _dataTypeString = "String";
        public const string _dataTypeBool = "Boolean";

        private string _columnName;
        private bool _useCurrentDate;
        private int _maxLength;
        private string _defaultValue;
        private int _defaultValueMaxLength;
        private FieldDataType _dataType;
        private bool _isRequired;
        private bool _isRestricted;

        private bool _hasError;
        private ObservableCollection<string> _supportedDataTypes;

        private RelayCommand _removeCommand;

        public TableColumnViewModel()
        {
            SetDefaultValueMaxLength();
        }

        public string Error
        {
            get
            {
                return this["ColumnName"] + this["MaxLength"];
            }
        }

        public string this[string columnName]
        {
            get
            {
                ResourceManager resource = new ResourceManager("Ecm.Admin.Resources", Assembly.GetExecutingAssembly());
                string errorMsg = string.Empty;
                _hasError = false;

                if (columnName == "MaxLength" && MaxLength > 8000)
                {
                    errorMsg = string.Format(resource.GetString("uiInvalidMaxValue"), "Max length", "8000");
                }

                if (columnName == "DefaultValue" && !string.IsNullOrEmpty(DefaultValue))
                {
                    int outIntResult;
                    if (DataType == FieldDataType.Integer && !int.TryParse(DefaultValue, out outIntResult))
                    {
                        errorMsg = string.Format("Default value is not a valid integer.");
                    }

                    decimal outDecResult;
                    if (DataType == FieldDataType.Decimal && !decimal.TryParse(DefaultValue, out outDecResult))
                    {
                        errorMsg = string.Format("Default value is not a valid decimal.");
                    }
                }

                if (columnName == "ColumnName")
                {
                    if (string.IsNullOrWhiteSpace(ColumnName))
                    {
                        errorMsg = "Column name is required";
                    }
                }

                if (DataType == FieldDataType.String && columnName == "MaxLength" && MaxLength == 0)
                {
                    errorMsg = "Max length is required";
                }

                _hasError = !string.IsNullOrEmpty(errorMsg);

                return errorMsg;
            }
        }

        public ObservableCollection<string> SupportedDataTypes
        {
            get
            {
                if (_supportedDataTypes == null)
                {
                    _supportedDataTypes = new ObservableCollection<string>();

                    _supportedDataTypes.Add(_dataTypeDate);
                    _supportedDataTypes.Add(_dataTypeDecimal);
                    _supportedDataTypes.Add(_dataTypeInteger);
                    _supportedDataTypes.Add(_dataTypeString);
                }

                return _supportedDataTypes;
            }
        }

        public Guid ColumnGuid { get; set; }

        public Guid? ParentFieldId { get; set; }

        public Guid FieldId { get; set; }

        public Guid DocTypeId { get; set; }

        public string ColumnName
        {
            get
            {
                return _columnName;
            }
            set
            {
                _columnName = value;
                OnPropertyChanged("ColumnName");
            }
        }

        public string DataTypeName
        {
            get
            {
                string dataType = string.Empty;
                switch (DataType)
                {
                    case FieldDataType.Date:
                        dataType = _dataTypeDate;
                        break;
                    case FieldDataType.Decimal:
                        dataType = _dataTypeDecimal;
                        break;
                    case FieldDataType.Integer:
                        dataType = _dataTypeInteger;
                        break;
                    case FieldDataType.String:
                        dataType = _dataTypeString;
                        break;
                    default:
                        break;
                }

                return dataType;
            }
            set
            {
                switch (value)
                {
                    case _dataTypeDate:
                        DataType = FieldDataType.Date;
                        break;
                    case _dataTypeDecimal:
                        DataType = FieldDataType.Decimal;
                        break;
                    case _dataTypeInteger:
                        DataType = FieldDataType.Integer;
                        break;
                    case _dataTypeString:
                        DataType = FieldDataType.String;
                        break;
                    default:
                        break;
                }

                if (value != _dataTypeDate)
                {
                    UseCurrentDate = false;
                }

                if (value != _dataTypeString)
                {
                    DefaultValue = string.Empty;
                }

                SetDefaultValueMaxLength();

                OnPropertyChanged("DataTypeName");
            }
        }

        public FieldDataType DataType
        {
            get
            {
                return _dataType;
            }
            set
            {
                _dataType = value;
                OnPropertyChanged("DataType");
            }
        }

        public bool IsRestricted
        {
            get { return _isRestricted; }
            set
            {
                _isRestricted = value;
                OnPropertyChanged("IsRestricted");
            }
        }

        public bool IsRequired
        {
            get { return _isRequired; }
            set
            {
                _isRequired = value;
                OnPropertyChanged("IsRequired");
            }
        }

        public bool UseCurrentDate
        {
            get
            {
                return _useCurrentDate;
            }
            set
            {
                _useCurrentDate = value;
                OnPropertyChanged("UseCurrentDate");
            }
        }

        public int MaxLength
        {
            get
            {
                return _maxLength;
            }
            set
            {
                _maxLength = value;
                OnPropertyChanged("MaxLength");
            }
        }

        public string DefaultValue
        {
            get
            {
                return _defaultValue;
            }
            set
            {
                _defaultValue = value;
                OnPropertyChanged("DefaultValue");
            }
        }

        public int DefaultValueMaxLength
        {
            get
            {
                return _defaultValueMaxLength;
            }
            set
            {
                _defaultValueMaxLength = value;
                OnPropertyChanged("DefaultValueMaxLength");
            }
        }

        public ICommand RemoveCommand
        {
            get
            {
                if (_removeCommand == null)
                {
                    _removeCommand = new RelayCommand(p => OnRequestRemove());
                }
                return _removeCommand;
            }
        }

        public string FakeGroup
        {
            get
            {
                return "FakeGroup";
            }
        }

        public bool HasError
        {
            get
            {
                if (_hasError || string.IsNullOrWhiteSpace(_columnName))
                {
                    return true;
                }
                else if (_dataType == FieldDataType.String && (_maxLength == 0 || _maxLength > 8000))
                {
                    return true;
                }

                return false;
            }
        }

        private void OnRequestRemove()
        {
            if (RequestRemove != null)
            {
                RequestRemove(this, EventArgs.Empty);
            }
        }

        private void SetDefaultValueMaxLength()
        {
            switch (DataType)
            {
                case FieldDataType.Date:
                    MaxLength = 10;
                    break;
                case FieldDataType.Decimal:
                    MaxLength = 33;
                    // 28 chars before decimal point + 1 char of decimal point + 4 chars after decimal point
                    break;
                case FieldDataType.Integer:
                    MaxLength = 10;
                    break;
                case FieldDataType.String:
                    MaxLength = 10;
                    break;
                default:
                    break;
            }
        }
    }
}
