using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using Ecm.Mvvm;
using Ecm.CaptureDomain;
using System.Resources;
using System.Reflection;
using Ecm.CaptureModel;

namespace Ecm.CaptureAdmin.ViewModel
{
    public class SeparationActionViewModel : ComponentViewModel, IDataErrorInfo
    {
        private readonly SeparationActionModel _separationActionModel;
        private readonly List<DocTypeModel> _docTypes;
        private RelayCommand _saveCommand;
        private RelayCommand _cancelCommand;
        private bool _hasError;
        private bool _hasErrorOnPositionInDoc;
        private bool _isNotPatchCode;
        private readonly ResourceManager _resource = new ResourceManager("Ecm.CaptureAdmin.BarcodeConfigurationView", Assembly.GetExecutingAssembly());

        public SeparationActionViewModel(SeparationActionModel separationActionModel, List<DocTypeModel> docTypes)
        {
            _separationActionModel = separationActionModel;
            _docTypes = docTypes;

            InitializeData();
            ValidateGui();
        }

        public string Error
        {
            get
            {
                return null;
            }
        }

        public string this[string columnName]
        {
            get
            {
                string errorMsg = string.Empty;
                _hasError = false;

                if (columnName == "DocTypeId")
                {
                    if (HasSpecifyDocumentType && DocTypeId == Guid.Empty)
                    {
                        errorMsg = _resource.GetString("uiSelectDocType");
                    }
                }

                if (columnName == "BarcodePositionInDoc" && IsNotPatchCode && (BarcodePositionInDoc < 1 || BarcodePositionInDoc > 100))
                {
                    errorMsg = _resource.GetString("uiInvalidPosition");
                }

                _hasError = !string.IsNullOrEmpty(errorMsg);
                return errorMsg;
            }
        }

        public event EventHandler SaveAndCloseWindow;
        public event EventHandler CloseWindow;

        public Dictionary<string, BarcodeTypeModel> BarcodeTypes { get; private set; }

        public Dictionary<string, Guid> DocTypes { get; private set; }

        public BarcodeTypeModel BarcodeType
        {
            get { return _separationActionModel.BarcodeType; }
            set
            {
                _separationActionModel.BarcodeType = value;
                OnPropertyChanged("BarcodeType");

                HasSpecifyDocumentType = value != BarcodeTypeModel.PATCH;
                IsNotPatchCode = value != BarcodeTypeModel.PATCH;
            }
        }

        public int BarcodePositionInDoc
        {
            get { return _separationActionModel.BarcodePositionInDoc; }
            set
            {
                _separationActionModel.BarcodePositionInDoc = value;
                OnPropertyChanged("BarcodePositionInDoc");
            }
        }

        public string StartsWith
        {
            get { return _separationActionModel.StartsWith; }
            set
            {
                _separationActionModel.StartsWith = value;
                OnPropertyChanged("StartsWith");
            }
        }

        public Guid DocTypeId
        {
            get { return _separationActionModel.DocTypeId; }
            set
            {
                _separationActionModel.DocTypeId = value;
                OnPropertyChanged("DocTypeId");
            }
        }

        public bool RemoveSeparatorPage
        {
            get { return _separationActionModel.RemoveSeparatorPage; }
            set
            {
                _separationActionModel.RemoveSeparatorPage = value;
                OnPropertyChanged("RemoveSeparatorPage");
            }
        }

        public bool HasSpecifyDocumentType
        {
            get { return _separationActionModel.HasSpecifyDocumentType; }
            set
            {
                _separationActionModel.HasSpecifyDocumentType = value;
                OnPropertyChanged("HasSpecifyDocumentType");

                DocTypeId = Guid.Empty;
            }
        }

        public bool HasErrorOnPositionInDoc
        {
            get { return _hasErrorOnPositionInDoc; }
            set
            {
                _hasErrorOnPositionInDoc = value;
                OnPropertyChanged("HasErrorOnPositionInDoc");
            }
        }

        public bool IsNotPatchCode
        {
            get { return _isNotPatchCode; }
            set
            {
                _isNotPatchCode = value;
                OnPropertyChanged("IsNotPatchCode");

                if (!value)
                {
                    BarcodePositionInDoc = 0;
                    StartsWith = string.Empty;
                }
            }
        }

        public ICommand SaveCommand
        {
            get { return _saveCommand ?? (_saveCommand = new RelayCommand(p => Save(), p => CanSave())); }
        }

        public ICommand CancelCommand
        {
            get { return _cancelCommand ?? (_cancelCommand = new RelayCommand(p => Cancel())); }
        }

        private void InitializeData()
        {
            BarcodeTypes = new Dictionary<string, BarcodeTypeModel>
                               {
                                   {"CODABAR", BarcodeTypeModel.CODABAR},
                                   {"CODE128", BarcodeTypeModel.CODE128},
                                   {"CODE25", BarcodeTypeModel.CODE25},
                                   {"CODE25NI", BarcodeTypeModel.CODE25NI},
                                   {"CODE39", BarcodeTypeModel.CODE39},
                                   {"CODE93", BarcodeTypeModel.CODE93},
                                   {"DATABAR", BarcodeTypeModel.DATABAR},
                                   {"DATAMATRIX", BarcodeTypeModel.DATAMATRIX},
                                   {"EAN13", BarcodeTypeModel.EAN13},
                                   {"EAN8", BarcodeTypeModel.EAN8},
                                   {"MICROPDF417", BarcodeTypeModel.MICROPDF417},
                                   {"PATCH", BarcodeTypeModel.PATCH},
                                   {"PDF417", BarcodeTypeModel.PDF417},
                                   {"QRCODE", BarcodeTypeModel.QRCODE},
                                   {"SHORTCODE128", BarcodeTypeModel.SHORTCODE128},
                                   {"UPCA", BarcodeTypeModel.UPCA},
                                   {"UPCE", BarcodeTypeModel.UPCE}
                               };

            DocTypes = new Dictionary<string, Guid>();
            foreach (DocTypeModel docType in _docTypes)
            {
                DocTypes.Add(docType.Name, docType.Id);
            }
        }

        private void ValidateGui()
        {
            _isNotPatchCode = _separationActionModel.BarcodeType != BarcodeTypeModel.PATCH;
        }

        private bool CanSave()
        {
            return !_hasError &&
                   (!IsNotPatchCode || (!HasErrorOnPositionInDoc && BarcodePositionInDoc > 0 && BarcodePositionInDoc < 101)) &&
                   (!HasSpecifyDocumentType || DocTypeId != Guid.Empty);
        }

        private void Save()
        {
            if (HasSpecifyDocumentType)
            {
                _separationActionModel.DocTypeName = _docTypes.First(p => p.Id == DocTypeId).Name;
            }

            if (SaveAndCloseWindow != null)
            {
                SaveAndCloseWindow(this, EventArgs.Empty);
            }
        }

        private void Cancel()
        {
            if (CloseWindow != null)
            {
                CloseWindow(this, EventArgs.Empty);
            }
        }
    }
}
