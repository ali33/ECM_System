using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using Ecm.Model;
using Ecm.Model.DataProvider;
using Ecm.Mvvm;
using System.Collections.Generic;
using System.Linq;
using System.Resources;
using System.Reflection;

namespace Ecm.Admin.ViewModel
{
    public class ConfigBarcodeViewModel : ComponentViewModel
    {
        private RelayCommand _saveCommand;
        private RelayCommand _cancelCommand;
        private RelayCommand _addCommand;
        private RelayCommand _deleteCommand;
        private RelayCommand _finishCommand;
        private BarcodeConfigurationModel _selectedBarcodeConfiguration;
        private BarcodeConfigurationModel _editedBarcodeConfiguration;
        private readonly Dictionary<string, string> _errorMsg = new Dictionary<string, string>();
        private ObservableCollection<BarcodeConfigurationModel> _barcodeConfigurations;
        private readonly DocumentTypeProvider _documentTypeProvier = new DocumentTypeProvider();
        private readonly ResourceManager _res = new ResourceManager("Ecm.Admin.Resources", Assembly.GetExecutingAssembly());

        public MainViewModel MainViewModel { get; private set; }
        public DocumentTypeModel DocType { get; private set; }

        public ICommand SaveCommand
        {
            get { return _saveCommand ?? (_saveCommand = new RelayCommand(p => Save(), p => CanSave())); }
        }

        public ICommand CancelCommand
        {
            get { return _cancelCommand ?? (_cancelCommand = new RelayCommand(p => Cancel())); }
        }

        public ICommand AddCommand
        {
            get { return _addCommand ?? (_addCommand = new RelayCommand(p => AddConfig())); }
        }

        public ICommand DeleteCommand
        {
            get { return _deleteCommand ?? (_deleteCommand = new RelayCommand(p => DeleteConfig(), p => CanDelete())); }
        }

        public ICommand FinishCommand
        {
            get { return _finishCommand ?? (_finishCommand = new RelayCommand(p => Finish())); }
        }

        public ObservableCollection<FieldMetaDataModel> Fields { get; set; }

        public Dictionary<string, BarcodeTypeModel> BarcodeTypes { get; set; }

        public ObservableCollection<BarcodeConfigurationModel> BarcodeConfigurations
        {
            get { return _barcodeConfigurations; }
            set
            {
                _barcodeConfigurations = value;
                OnPropertyChanged("BarcodeConfigurations");
            }
        }

        public BarcodeConfigurationModel SelectedBarcodeConfiguration
        {
            get { return _selectedBarcodeConfiguration; }
            set
            {
                _selectedBarcodeConfiguration = value;
                OnPropertyChanged("SelectedBarcodeConfiguration");

                if (value != null)
                {
                    EditPanelVisibled = true;
                    EditedBarcodeConfiguration = new BarcodeConfigurationModel(CheckError)
                    {
                        BarcodePosition = value.BarcodePosition,
                        BarcodeType = value.BarcodeType,
                        DocumentTypeId = DocType.Id,
                        FieldMetaData = SelectedBarcodeConfiguration.FieldMetaData,
                        HasDoLookup = SelectedBarcodeConfiguration.HasDoLookup,
                        Id = SelectedBarcodeConfiguration.Id,
                        IsDocumentSeparator = SelectedBarcodeConfiguration.IsDocumentSeparator,
                        MapValueToFieldId = SelectedBarcodeConfiguration.MapValueToFieldId,
                        RemoveSeparatorPage = SelectedBarcodeConfiguration.RemoveSeparatorPage,
                        ErrorMessages = _errorMsg
                    };
                }
            }
        }

        public BarcodeConfigurationModel EditedBarcodeConfiguration
        {
            get { return _editedBarcodeConfiguration; }
            set
            {
                _editedBarcodeConfiguration = value;
                OnPropertyChanged("EditedBarcodeConfiguration");
            }
        }

        public ConfigBarcodeViewModel(DocumentTypeModel docType, MainViewModel mainViewModel)
        {
            DocType = docType;
            MainViewModel = mainViewModel;
            if (DocType.BarcodeConfigurations == null)
            {
                DocType.BarcodeConfigurations = new ObservableCollection<BarcodeConfigurationModel>();
            }

            Initialize();
        }
        // Private methods
        public override sealed void Initialize()
        {
            base.Initialize();
            var resource = new ResourceManager("Ecm.Admin.Resources", Assembly.GetExecutingAssembly());
            _errorMsg.Add("uiFieldAlreadyMap", resource.GetString("uiFieldAlreadyMap"));
            _errorMsg.Add("uiDupplicateBarcodeConfig", resource.GetString("uiDupplicateBarcodeConfig"));
            GetBarcodeTypes();
            GetFields();
            LoadData();
        }

        private void GetBarcodeTypes()
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
                                   {"SHORTCODE128", BarcodeTypeModel.SHORTCODE128},
                                   {"UPCA", BarcodeTypeModel.UPCA},
                                   {"UPCE", BarcodeTypeModel.UPCE}
                               };
        }

        private void GetFields()
        {
            try
            {
                Fields = new ObservableCollection<FieldMetaDataModel>();
                Fields = DocType.Fields;
            }
            catch (Exception ex)
            {
                ProcessHelper.ProcessException(ex);
            }
        }

        private void DeleteConfig()
        {
            if (DialogService.ShowTwoStateConfirmDialog(_res.GetString("uiConfirmDelete")) == DialogServiceResult.Yes)
            {
                IsProcessing = true;
                var worker = new BackgroundWorker();
                worker.DoWork += DoDelete;
                worker.RunWorkerCompleted += DoDeleteComplated;
                worker.RunWorkerAsync();
            }
        }

        private void DoDeleteComplated(object sender, RunWorkerCompletedEventArgs e)
        {
            IsProcessing = false;
            EditedBarcodeConfiguration = null;
            EditPanelVisibled = false;
            if (e.Result is Exception)
            {
                ProcessHelper.ProcessException(e.Result as Exception);
            }
        }

        private void DoDelete(object sender, DoWorkEventArgs e)
        {
            try
            {
                _documentTypeProvier.DeleteBarcodeConfiguration(SelectedBarcodeConfiguration.Id);
                BarcodeConfigurations = new ObservableCollection<BarcodeConfigurationModel>(_documentTypeProvier.GetBarcodeConfigurations(DocType.Id));
            }
            catch (Exception ex)
            {
                e.Result = ex;
            }
        }

        private bool CanDelete()
        {
            return SelectedBarcodeConfiguration != null;
        }

        private void AddConfig()
        {
            EditPanelVisibled = true;
            EditedBarcodeConfiguration = new BarcodeConfigurationModel(CheckError) { ErrorMessages = _errorMsg, DocumentTypeId = DocType.Id };
        }

        private bool CanSave()
        {
            return EditedBarcodeConfiguration != null &&
                   EditedBarcodeConfiguration.BarcodePosition != 0 &&
                   EditedBarcodeConfiguration.DocumentTypeId != Guid.Empty &&
                   (EditedBarcodeConfiguration.MapValueToFieldId != Guid.Empty || EditedBarcodeConfiguration.IsDocumentSeparator) &&
                   !EditedBarcodeConfiguration.HasError &&
                   EditedBarcodeConfiguration.BarcodeType != null;
        }

        private void CheckError(BarcodeConfigurationModel barcodeConfig)
        {
            if (barcodeConfig.CheckErrorProperty == "BarcodeType" && barcodeConfig.BarcodePosition != 0)
            {
                if (barcodeConfig.Id == Guid.Empty)
                {
                    if (BarcodeConfigurations.FirstOrDefault(b => b.BarcodeType == barcodeConfig.BarcodeType && b.BarcodePosition == barcodeConfig.BarcodePosition) != null)
                    {
                        barcodeConfig.HasError = true;
                    }
                    else
                        barcodeConfig.HasError = false;
                }
                else
                {
                    if (BarcodeConfigurations.FirstOrDefault(b => b.BarcodeType == barcodeConfig.BarcodeType && b.BarcodePosition == barcodeConfig.BarcodePosition && b.Id != barcodeConfig.Id) != null)
                    {
                        barcodeConfig.HasError = true;
                    }
                    else
                    {
                        barcodeConfig.HasError = false;
                    }
                }
            }
            else if (barcodeConfig.CheckErrorProperty == "BarcodePosition" && barcodeConfig.BarcodeType != null)
            {
                if (barcodeConfig.Id == Guid.Empty)
                {
                    if (BarcodeConfigurations.FirstOrDefault(b => b.BarcodeType == barcodeConfig.BarcodeType && b.BarcodePosition == barcodeConfig.BarcodePosition) != null)
                    {
                        barcodeConfig.HasError = true;
                    }
                    else
                    {
                        barcodeConfig.HasError = false;
                    }
                }
                else
                {
                    if (BarcodeConfigurations.FirstOrDefault(b => b.BarcodeType == barcodeConfig.BarcodeType && b.BarcodePosition == barcodeConfig.BarcodePosition && b.Id != barcodeConfig.Id) != null)
                    {
                        barcodeConfig.HasError = true;
                    }
                    else
                    {
                        barcodeConfig.HasError = false;
                    }
                }
            }
            else if (barcodeConfig.CheckErrorProperty == "MapValueToFieldId" && barcodeConfig.MapValueToFieldId != null)
            {
                if (BarcodeConfigurations.FirstOrDefault(f => f.MapValueToFieldId == barcodeConfig.MapValueToFieldId && f.Id != barcodeConfig.Id && barcodeConfig.Id != Guid.Empty) != null)
                {
                    barcodeConfig.HasError = true;
                }
                else
                {
                    barcodeConfig.HasError = false;
                }
            }
        }

        private void Save()
        {
            var saveWorker = new BackgroundWorker();
            saveWorker.RunWorkerCompleted += SaveWorkerRunWorkerCompleted;
            saveWorker.DoWork += SaveWorkerDoWork;
            saveWorker.RunWorkerAsync(DocType.OCRTemplate);
        }

        private void SaveWorkerDoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                _documentTypeProvier.SaveBarcodeConfiguration(EditedBarcodeConfiguration);
                BarcodeConfigurations = new ObservableCollection<BarcodeConfigurationModel>(_documentTypeProvier.GetBarcodeConfigurations(DocType.Id));
            }
            catch (Exception ex)
            {
                e.Result = ex;
            }
        }

        private void SaveWorkerRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            IsProcessing = false;
            if (e.Result is Exception)
            {
                ProcessHelper.ProcessException(e.Result as Exception);
            }
            else
            {
                EditedBarcodeConfiguration = null;
                EditPanelVisibled = false;
            }
        }

        private void Cancel()
        {
            EditPanelVisibled = false;
            EditPanelVisibled = false;
            if (ResetListView != null)
            {
                ResetListView();
            }
        }

        private void Finish()
        {
            MainViewModel.ViewModel = new DocumentTypeViewModel(MainViewModel);
            MainViewModel.ViewModel.Initialize();
        }

        private void LoadData()
        {
            try
            {
                BarcodeConfigurations = new ObservableCollection<BarcodeConfigurationModel>(DocType.BarcodeConfigurations);
            }
            catch (Exception ex)
            {
                ProcessHelper.ProcessException(ex);
            }
        }
    }
}
