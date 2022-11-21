using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using System.Collections.Generic;
using Ecm.Mvvm;
using Ecm.CaptureDomain;
using System.Resources;
using System.Reflection;

namespace Ecm.CaptureAdmin.ViewModel
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
        private int? _maxLength;
        private string _defaultValue;
        private bool _isRequired;
        private bool _isRestricted;
        private int _defaultValueMaxLength;
        private FieldDataType _dataType;
        private bool _hasError;
        private ObservableCollection<string> _supportedDataTypes;

        private RelayCommand _removeCommand;

        private Func<string, bool> _checkExistedColumnName;

        public TableColumnViewModel(Func<string, bool> checkExistedColumnName)
        {
            _checkExistedColumnName = checkExistedColumnName;
            _hasErrors.Add("MaxLength", false);
            _hasErrors.Add("DefaultValue", false);
            _hasErrors.Add("ColumnName", false);
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

                switch (columnName)
                {
                    case "MaxLength":
                        #region
                        if (DataType == FieldDataType.String)
                        {
                            if (!MaxLength.HasValue || MaxLength.Value == 0)
                            {
                                errorMsg = "Max length is required";
                            }
                            else if (MaxLength.Value > 8000)
                            {
                                errorMsg = string.Format(resource.GetString("uiInvalidMaxValue"), "Max length", "8000");
                            }
                        }
                        break;
                        #endregion

                    case "ColumnName":
                        #region
                        if (string.IsNullOrWhiteSpace(ColumnName))
                        {
                            errorMsg = "Column name is required";
                        }
                        else if (_checkExistedColumnName != null)
                        {
                            if (_checkExistedColumnName(ColumnName))
                            {
                                errorMsg = "Column name is already existed.";
                            }
                        }
                        break; 
                        #endregion

                    case "DefaultValue":
                        #region
                        if (DataType == FieldDataType.Integer)
                        {
                            int outIntResult;
                            if (!int.TryParse(DefaultValue, out outIntResult))
                            {
                                errorMsg = string.Format("Default value is not a valid integer.");
                            }
                        }
                        else if (DataType == FieldDataType.Decimal)
                        {
                            decimal outDecResult;
                            if (!decimal.TryParse(DefaultValue, out outDecResult))
                            {
                                errorMsg = string.Format("Default value is not a valid decimal.");
                            }
                        }
                        break; 
                        #endregion
                }

                _hasErrors[columnName] = !string.IsNullOrEmpty(errorMsg);
                _hasError = _hasErrors.ContainsValue(true);


                return errorMsg;
            }
        }

        private Dictionary<string, bool> _hasErrors = new Dictionary<string, bool>(4);

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
                    case FieldDataType.Boolean:
                        dataType = _dataTypeBool;
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
                OnPropertyChanged("MaxLength");
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

        public int? MaxLength
        {
            get
            {
                return _maxLength;
            }
            set
            {
                _maxLength = value;
                OnPropertyChanged("MaxLength");
                SetDefaultValueMaxLength();
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
                    DefaultValueMaxLength = 10;
                    break;
                case FieldDataType.Decimal:
                    DefaultValueMaxLength = 33;
                    // 28 chars before decimal point + 1 char of decimal point + 4 chars after decimal point
                    break;
                case FieldDataType.Integer:
                    DefaultValueMaxLength = 10;
                    break;
                case FieldDataType.String:
                    DefaultValueMaxLength = MaxLength.HasValue ? MaxLength.Value : 0;
                    break;
                default:
                    break;
            }
        }
    }
}
