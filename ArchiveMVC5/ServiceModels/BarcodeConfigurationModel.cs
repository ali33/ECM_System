using System;

using System.ComponentModel;
using System.Collections.Generic;

namespace ArchiveMVC5.Models
{
    public class BarcodeConfigurationModel
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
           }
        }

        public int BarcodePosition
        {
            get { return _barcodePosition; }
            set
            {
                _barcodePosition = value;
            }
        }

        public bool IsDocumentSeparator
        {
            get { return _isDocumentSeparator; }
            set
            {
                _isDocumentSeparator = value;
            }
        }

        public bool RemoveSeparatorPage
        {
            get { return _removeSeparatorPage; }
            set
            {
                _removeSeparatorPage = value;
            }
        }

        public bool HasDoLookup
        {
            get { return _hasDoLookup; }
            set
            {
                _hasDoLookup = value;
            }
        }

        public Guid? MapValueToFieldId
        {
            get { return _mapValueToFieldId; }
            set
            {
                _mapValueToFieldId = value;
            }
        }

        public FieldMetaDataModel FieldMetaData
        {
            get { return _fieldMetadata; }
            set
            {
                _fieldMetadata = value;
            }
        }

        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                _isSelected = value;
            }
        }

        public bool HasError { get; set; }

        public Dictionary<string, string> ErrorMessages { get; set; }

    }
}
