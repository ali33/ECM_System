using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using Ecm.Mvvm;
using Ecm.Workflow.Activities.CustomActivityModel;
using Ecm.CaptureDomain;
using Ecm.Workflow.Activities.CustomActivityDomain;
using System.Resources;
using System.Reflection;

namespace Ecm.Workflow.Activities.BarcodeExecutorDesigner.ViewModel
{
    public class ReadActionViewModel : ComponentViewModel, IDataErrorInfo
    {
        private readonly ReadActionModel _readActionModel;
        private readonly List<DocumentType> _docTypes;
        private readonly BatchType _batchType;

        private RelayCommand _saveCommand;
        private RelayCommand _cancelCommand;
        private bool _hasError;
        private bool _hasErrorWithPositionInDoc;
        private bool _hasErrorWithDocTypeId;
        private bool _hasErrorWithCopyToIndex;
        private string _copyToFieldName;
        private string _copyToFieldGuid = string.Empty;
        private bool _isBarcode2D;
        private string _sampleTemplate = "";
        private string _sampleText;
        private Dictionary<string, string> _docIndexes;
        private readonly ResourceManager _resource = new ResourceManager("Ecm.Workflow.Activities.BarcodeExecutorDesigner.Resource", Assembly.GetExecutingAssembly());

        public ReadActionViewModel(ReadActionModel readActionModel, List<DocumentType> docTypes, BatchType batchType)
        {
            _readActionModel = readActionModel;
            _docTypes = docTypes;
            _batchType = batchType;
            _sampleTemplate = _resource.GetString("uiSampleText");

            Initialize();
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
                    if (IsDocIndex && DocTypeId == Guid.Empty)
                    {
                        errorMsg = _resource.GetString("uiSelectDocType");
                        _hasErrorWithDocTypeId = true;
                    }
                    else
                    {
                        _hasErrorWithDocTypeId = false;
                    }
                }

                if (columnName == "BarcodePositionInDoc")
                {
                    if (BarcodePositionInDoc < 1 || BarcodePositionInDoc > 100)
                    {
                        errorMsg = _resource.GetString("uiInvalidPosition");
                        _hasErrorWithPositionInDoc = true;
                    }
                    else
                    {
                        _hasErrorWithPositionInDoc = false;
                    }
                }

                if (columnName == "CopyToFieldName" && IsBarcode2D && !string.IsNullOrWhiteSpace(Separator))
                {
                    _hasErrorWithCopyToIndex = !ValidateIndexNames(out errorMsg);
                }

                if (columnName == "CopyToFieldGuid")
                {
                    if (CopyToFieldGuid == string.Empty)
                    {
                        errorMsg = _resource.GetString("uiSelectField");
                        _hasErrorWithCopyToIndex = true;
                    }
                    else
                    {
                        _hasErrorWithCopyToIndex = false;
                    }
                }

                _hasError = !string.IsNullOrEmpty(errorMsg);
                return errorMsg;
            }
        }

        public event EventHandler SaveAndCloseWindow;
        public event EventHandler CloseWindow;

        public Dictionary<string, BarcodeTypeModel> BarcodeTypes { get; private set; }

        public Dictionary<string, Guid> DocTypes { get; private set; }

        public Dictionary<string, string> BatchFields { get; private set; }

        public Dictionary<string, string> DocFields
        {
            get { return _docIndexes; }
            set
            {
                _docIndexes = value;
                OnPropertyChanged("DocFields");
            }
        }

        public BarcodeTypeModel BarcodeType
        {
            get { return _readActionModel.BarcodeType; }
            set
            {
                _readActionModel.BarcodeType = value;
                OnPropertyChanged("BarcodeType");

                IsBarcode2D = IsBarcodeType2D(value);
            }
        }

        public bool IsBarcode2D
        {
            get { return _isBarcode2D; }
            set
            {
                _isBarcode2D = value;
                OnPropertyChanged("IsBarcode2D");

                if (!value)
                {
                    Separator = string.Empty;
                }
            }
        }

        public int BarcodePositionInDoc
        {
            get { return _readActionModel.BarcodePositionInDoc; }
            set
            {
                _readActionModel.BarcodePositionInDoc = value;
                OnPropertyChanged("BarcodePositionInDoc");
            }
        }

        public string StartsWith
        {
            get { return _readActionModel.StartsWith; }
            set
            {
                _readActionModel.StartsWith = value;
                OnPropertyChanged("StartsWith");
            }
        }

        public bool IsDocIndex
        {
            get { return _readActionModel.IsDocIndex; }
            set
            {
                _readActionModel.IsDocIndex = value;
                OnPropertyChanged("IsDocIndex");

                DocTypeId = Guid.Empty;
                CopyToFieldGuid = string.Empty;
                CopyToFieldName = string.Empty;
            }
        }

        public Guid DocTypeId
        {
            get { return _readActionModel.DocTypeId; }
            set
            {
                _readActionModel.DocTypeId = value;
                OnPropertyChanged("DocTypeId");

                InitializeDocIndexes();
                CopyToFieldGuid = string.Empty;
                CopyToFieldName = string.Empty;
            }
        }

        public string Separator
        {
            get { return _readActionModel.Separator; }
            set
            {
                _readActionModel.Separator = value;
                OnPropertyChanged("Separator");

                if (!string.IsNullOrWhiteSpace(value))
                {
                    SampleText = string.Format(_sampleTemplate, value);
                    OnPropertyChanged("CopyToFieldName");
                }
            }
        }

        public bool OverwriteFieldValue
        {
            get { return _readActionModel.OverwriteFieldValue; }
            set
            {
                _readActionModel.OverwriteFieldValue = value;
                OnPropertyChanged("OverwriteFieldValue");
            }
        }

        public string CopyToFieldName
        {
            get { return _copyToFieldName; }
            set
            {
                _copyToFieldName = value;
                OnPropertyChanged("CopyToFieldName");
            }
        }

        public string SampleText
        {
            get { return _sampleText; }
            set
            {
                _sampleText = value;
                OnPropertyChanged("SampleText");
            }
        }

        public string CopyToFieldGuid
        {
            get { return _copyToFieldGuid; }
            set
            {
                _copyToFieldGuid = value;
                OnPropertyChanged("CopyToFieldGuid");
            }
        }

        public bool HasErrorWithPositionInDoc
        {
            get { return _hasErrorWithPositionInDoc; }
            set
            {
                _hasErrorWithPositionInDoc = value;
                OnPropertyChanged("HasErrorWithPositionInDoc");
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

        private void Initialize()
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
                                   {"PDF417", BarcodeTypeModel.PDF417},
                                   {"QRCODE", BarcodeTypeModel.QRCODE},
                                   {"SHORTCODE128", BarcodeTypeModel.SHORTCODE128},
                                   {"UPCA", BarcodeTypeModel.UPCA},
                                   {"UPCE", BarcodeTypeModel.UPCE}
                               };

            BatchFields = new Dictionary<string, string>();
            List<BatchFieldMetaData> normalBatchIndexes = _batchType.Fields.Where(p => !p.IsSystemField).ToList();
            foreach (BatchFieldMetaData field in normalBatchIndexes)
            {
                BatchFields.Add(field.Name, field.UniqueId);
            }

            DocTypes = new Dictionary<string, Guid>();
            foreach (DocumentType docType in _docTypes)
            {
                DocTypes.Add(docType.Name, docType.Id);
            }
        }

        private bool IsBarcodeType2D(BarcodeTypeModel barcodeType)
        {
            return barcodeType == BarcodeTypeModel.DATAMATRIX || barcodeType == BarcodeTypeModel.QRCODE || barcodeType == BarcodeTypeModel.PDF417 || barcodeType == BarcodeTypeModel.MICROPDF417;
        }

        private void ValidateGui()
        {
            IsBarcode2D = IsBarcodeType2D(_readActionModel.BarcodeType);

            if (IsDocIndex && DocTypeId != Guid.Empty)
            {
                InitializeDocIndexes();
            }

            if (IsBarcode2D && !string.IsNullOrWhiteSpace(Separator))
            {
                CopyToFieldName = _readActionModel.CopyValueToFieldName;
            }
            else if (_readActionModel.CopyValueToFields.Count > 0)
            {
                CopyToFieldGuid = _readActionModel.CopyValueToFields[0].FieldGuid;
            }
        }

        private void InitializeDocIndexes()
        {
            Dictionary<string, string> docIndexes = new Dictionary<string, string>();
            if (IsDocIndex && DocTypeId != Guid.Empty)
            {
                DocumentType docType = _docTypes.FirstOrDefault(p => p.Id == DocTypeId);
                if (docType != null)
                {
                    foreach (DocumentFieldMetaData field in docType.Fields)
                    {
                        docIndexes.Add(field.Name, field.UniqueId);
                    }
                }
            }

            DocFields = docIndexes;
        }

        private bool CanSave()
        {
            return !_hasError && !HasErrorWithPositionInDoc && !_hasErrorWithDocTypeId && !_hasErrorWithCopyToIndex &&
                   BarcodePositionInDoc >= 0 && BarcodePositionInDoc < 101 &&
                   (!IsDocIndex || DocTypeId != Guid.Empty);
        }

        private void Save()
        {
            _readActionModel.TargetTypeName = IsDocIndex ? _docTypes.First(p => p.Id == DocTypeId).Name : _batchType.Name;

            if (IsBarcode2D && !string.IsNullOrWhiteSpace(Separator))
            {
                _readActionModel.CopyValueToFields = ParseIndexNames();
            }
            else
            {
                _readActionModel.CopyValueToFields.Clear();
                string fieldName = IsDocIndex ? _docTypes.First(p => p.Id == DocTypeId).Fields.First(p => p.UniqueId == CopyToFieldGuid).Name : _batchType.Fields.First(p => p.UniqueId == CopyToFieldGuid).Name;

                _readActionModel.CopyValueToFields.Add(new CopyValueToFieldModel
                                                            {
                                                                FieldGuid = CopyToFieldGuid,
                                                                FieldName = fieldName,
                                                                Position = 0
                                                            });
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

        private bool ValidateIndexNames(out string errorMessage)
        {
            errorMessage = string.Empty;

            if (string.IsNullOrWhiteSpace(_copyToFieldName))
            {
                errorMessage = _resource.GetString("uiFieldRequired");
                return false;
            }

            string[] fieldNames = _copyToFieldName.Split(new[] { _readActionModel.Separator }, StringSplitOptions.RemoveEmptyEntries);
            if (fieldNames.Length == 0)
            {
                errorMessage = _resource.GetString("uiFieldRequired");
                return false;
            }

            fieldNames = _copyToFieldName.Split(new [] { _readActionModel.Separator }, StringSplitOptions.None);
            int count = fieldNames.Length;

            if (IsDocIndex)
            {
                DocumentType docType = _docTypes.FirstOrDefault(p => p.Id == DocTypeId);
                if (docType == null)
                {
                    errorMessage = _resource.GetString("uiDocTypeNotFound");
                    return false;
                }

                for (int i = 0; i < count; i++)
                {
                    string fieldName = fieldNames[i].Trim();
                    if (!string.IsNullOrEmpty(fieldName))
                    {
                        DocumentFieldMetaData indexField = docType.Fields.FirstOrDefault(p => p.Name.Equals(fieldName, StringComparison.OrdinalIgnoreCase));
                        if (indexField == null)
                        {
                            errorMessage = string.Format(_resource.GetString("uiInvalidFieldName"), fieldName);
                            return false;
                        }
                    }
                }
            }
            else
            {
                for (int i = 0; i < count; i++)
                {
                    string fieldName = fieldNames[i].Trim();
                    if (!string.IsNullOrEmpty(fieldName))
                    {
                        BatchFieldMetaData field = _batchType.Fields.FirstOrDefault(p => p.Name.Equals(fieldName, StringComparison.OrdinalIgnoreCase));
                        if (field == null)
                        {
                            errorMessage = string.Format(_resource.GetString("uiInvalidFieldName"), fieldName);
                            return false;
                        }
                    }
                }
            }

            return true;
        }

        private List<CopyValueToFieldModel> ParseIndexNames()
        {
            List<CopyValueToFieldModel> result = new List<CopyValueToFieldModel>();

            string[] fieldNames = _copyToFieldName.Split(new [] { _readActionModel.Separator }, StringSplitOptions.None);
            int count = fieldNames.Length;

            if (IsDocIndex)
            {
                DocumentType docType = _docTypes.FirstOrDefault(p => p.Id == DocTypeId);

                for (int i = 0; i < count; i++)
                {
                    string fieldName = fieldNames[i].Trim();
                    if (string.IsNullOrEmpty(fieldName))
                    {
                        result.Add(new CopyValueToFieldModel
                                       {
                                           FieldGuid = string.Empty,
                                           FieldName = string.Empty,
                                           Position = i
                                       });
                    }
                    else
                    {
                        DocumentFieldMetaData field = docType.Fields.FirstOrDefault(p => p.Name.Equals(fieldName, StringComparison.OrdinalIgnoreCase));
                        result.Add(new CopyValueToFieldModel
                                       {
                                           FieldGuid = field.UniqueId,
                                           FieldName = field.Name,
                                           Position = i
                                       });
                    }
                }
            }
            else
            {
                for (int i = 0; i < count; i++)
                {
                    string fieldName = fieldNames[i].Trim();
                    if (string.IsNullOrEmpty(fieldName))
                    {
                        result.Add(new CopyValueToFieldModel
                                       {
                                           FieldGuid = string.Empty,
                                           FieldName = string.Empty,
                                           Position = i
                                       });
                    }
                    else
                    {
                        BatchFieldMetaData field = _batchType.Fields.FirstOrDefault(p => p.Name.Equals(fieldName, StringComparison.OrdinalIgnoreCase));
                        result.Add(new CopyValueToFieldModel
                                       {
                                           FieldGuid = field.UniqueId,
                                           FieldName = field.Name,
                                           Position = i
                                       });
                    }
                }
            }

            return result;
        }
    }
}
