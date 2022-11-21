using System;
using Ecm.Mvvm;
using System.ComponentModel;
using System.Collections.Generic;

namespace Ecm.Model
{
    public class BarcodeConfigurationModel : BaseDependencyProperty, IDataErrorInfo
    {
        private BarcodeTypeModel? _barcodeType;
        private bool _isDocumentSeparator;
        private bool _removeSeparatorPage;
        private bool _hasDoLookup;
        private int _barcodePosition;
        private Guid? _mapValueToFieldId;
        private bool _isSelected;
        private FieldMetaDataModel _fieldMetadata;
        public Guid Id { get; set; }

        public Action<BarcodeConfigurationModel> ErrorChecked { get; set; }

        public Guid InternalId { get; set; }

        public string CheckErrorProperty { get; set; }

        public BarcodeConfigurationModel()
        {
        }

        public BarcodeConfigurationModel(Action<BarcodeConfigurationModel> action)
        {
            ErrorChecked = action;
        }

        public Guid DocumentTypeId
        {
            get;
            set;
        }

        public BarcodeTypeModel? BarcodeType
        {
            get { return _barcodeType; }
            set
            {
                _barcodeType = value;
                OnPropertyChanged("BarcodeType");
            }
        }

        public int BarcodePosition
        {
            get { return _barcodePosition; }
            set
            {
                _barcodePosition = value;
                OnPropertyChanged("BarcodePosition");
            }
        }

        public bool IsDocumentSeparator
        {
            get { return _isDocumentSeparator; }
            set
            {
                _isDocumentSeparator = value;
                OnPropertyChanged("IsDocumentSeparator");
            }
        }

        public bool RemoveSeparatorPage
        {
            get { return _removeSeparatorPage; }
            set
            {
                _removeSeparatorPage = value;
                OnPropertyChanged("RemoveSeparatorPage");
            }
        }

        public bool HasDoLookup
        {
            get { return _hasDoLookup; }
            set
            {
                _hasDoLookup = value;
                OnPropertyChanged("HasDoLookup");
            }
        }

        public Guid? MapValueToFieldId
        {
            get { return _mapValueToFieldId; }
            set
            {
                _mapValueToFieldId = value;
                OnPropertyChanged("MapValueToFieldId");
            }
        }

        public FieldMetaDataModel FieldMetaData
        {
            get { return _fieldMetadata; }
            set
            {
                _fieldMetadata = value;
                OnPropertyChanged("FieldMetaData");
            }
        }

        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                _isSelected = value;
                OnPropertyChanged("IsSelected");
            }
        }

        public bool HasError { get; set; }

        public Dictionary<string, string> ErrorMessages { get; set; }

        #region IDataErrorInfo Members

        public string Error
        {
            get { return null; }
        }

        public string this[string columnName]
        {
            get 
            {
                
                HasError = false;
                string msg = string.Empty;
                CheckErrorProperty = columnName;
                if (columnName == "BarcodeType")
                {
                    if (ErrorChecked != null)
                    {
                        ErrorChecked(this);
                    }

                    if (HasError)
                    {
                        msg = ErrorMessages["uiDupplicateBarcodeConfig"];
                    }
                }

                if (columnName == "BarcodePosition")
                {
                    if (ErrorChecked != null)
                    {
                        ErrorChecked(this);
                    }
                    if (HasError)
                    {
                        msg = ErrorMessages["uiDupplicateBarcodeConfig"];
                    }
                }

                if (columnName == "MapValueToFieldId")
                {
                    if (ErrorChecked != null)
                    {
                        ErrorChecked(this);
                    }
                    if (HasError)
                    {
                        msg = ErrorMessages["uiFieldAlreadyMap"];
                    }

                }

                HasError = !string.IsNullOrEmpty(msg);
                return msg;
            }
        }

        #endregion
    }
}
