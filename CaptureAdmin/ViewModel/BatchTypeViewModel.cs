using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Windows.Input;
using Ecm.CaptureAdmin.View;
using Ecm.CaptureDomain;
using Ecm.CaptureModel;
using Ecm.CaptureModel.DataProvider;
using Ecm.Mvvm;
using Ecm.WorkflowDesigner.ViewModel;
using Ecm.WorkflowDesigner;

namespace Ecm.CaptureAdmin.ViewModel
{
    public class BatchTypeViewModel : ComponentViewModel
    {
        private readonly Dictionary<string, string> _errorMessage = new Dictionary<string, string>();
        private ObservableCollection<BatchTypeModel> _batchTypes = new ObservableCollection<BatchTypeModel>();
        private ObservableCollection<FieldModel> _batchFields = new ObservableCollection<FieldModel>();
        private ObservableCollection<DocTypeModel> _docTypes = new ObservableCollection<DocTypeModel>();
        private BatchTypeModel _batchType;
        private BatchTypeModel _editBatchType;
        private DocTypeModel _selectedDocType;
        private string _iconFilePath;
        private bool _isDataModified;
        private bool _canEditBatchTypeField = false;
        private bool _canEditDocTypeField = false;

        private RelayCommand _browseCommand;
        private RelayCommand _addBatchTypeCommand;
        private RelayCommand _deleteBatchTypeCommand;
        private RelayCommand _saveCommand;
        private RelayCommand _cancelCommand;
        private RelayCommand _addBatchFieldCommand;
        private RelayCommand _editBatchFieldCommand;
        private RelayCommand _deleteBatchFieldCommand;
        private RelayCommand _addDocTypeCommand;
        private RelayCommand _editDocTypeCommand;
        private RelayCommand _viewDocTypeCommand;
        private RelayCommand _deleteDocTypeCommand;
        private RelayCommand _configWorkflowCommand;
        private RelayCommand _configBarcodeCommand;
        private RelayCommand _configOcrCommand;
        private RelayCommand _deleteOcrCommand;
        private DialogBaseView _dialog;
        private readonly LanguageProvider _languageProvider = new LanguageProvider();
        private readonly BatchTypeProvider _batchTypeProvider = new BatchTypeProvider();
        private readonly DocTypeProvider _docTypeProvider = new DocTypeProvider();
        private readonly WorkflowProvider _workflowProvider = new WorkflowProvider();
        private readonly ResourceManager _resouceManager = new ResourceManager("Ecm.CaptureAdmin.Resources", Assembly.GetExecutingAssembly());

        public BatchTypeViewModel(MainViewModel mainViewModel)
        {
            MainViewModel = mainViewModel;

            _errorMessage.Add("uiInvalidIntegerValue", _resouceManager.GetString("uiInvalidIntegerValue"));
            _errorMessage.Add("uiInvalidDecimalValue", _resouceManager.GetString("uiInvalidDecimalValue"));
            _errorMessage.Add("uiFieldNameExisted", _resouceManager.GetString("uiFieldNameExisted"));
        }

        public MainViewModel MainViewModel { get; private set; }

        public ObservableCollection<BatchTypeModel> BatchTypes
        {
            get { return _batchTypes; }
            set
            {
                _batchTypes = value;
                OnPropertyChanged("BatchTypes");
            }
        }

        public BatchTypeModel BatchType
        {
            get { return _batchType; }
            set
            {
                _batchType = value;
                OnPropertyChanged("BatchType");
                _isDataModified = false;
            }
        }

        public BatchTypeModel EditBatchType
        {
            get { return _editBatchType; }
            set
            {
                _editBatchType = value;
                if (value != null)
                {
                    EditPanelVisibled = true;

                    List<FieldModel> batchFieldListWithoutSystemFields = _editBatchType.Fields.Where(f => !f.IsSystemField).ToList();
                    _editBatchType.Fields.Clear();

                    foreach (var field in batchFieldListWithoutSystemFields)
                    {
                        _editBatchType.Fields.Add(field);
                    }

                    BatchFields = _editBatchType.Fields;

                    foreach (var docType in _editBatchType.DocTypes)
                    {
                        List<FieldModel> docFieldListWithoutSystemFields = docType.Fields.Where(f => !f.IsSystemField).ToList();
                        docType.Fields.Clear();

                        foreach (var field in docFieldListWithoutSystemFields)
                        {
                            docType.Fields.Add(field);
                        }
                    }

                    DocTypes = _editBatchType.DocTypes;

                    _editBatchType.PropertyChanged += BatchTypePropertyChanged;
                }
                else
                {
                    EditPanelVisibled = false;
                    BatchFields = null;
                    DocTypes = null;
                }

                SelectedDocType = null;
                IconFilePath = null;
                CommandManager.InvalidateRequerySuggested();
                OnPropertyChanged("EditBatchType");
            }
        }

        public string IconFilePath
        {
            get { return _iconFilePath; }
            set
            {
                _iconFilePath = value;
                OnPropertyChanged("IconFilePath");
            }
        }

        public ObservableCollection<FieldModel> BatchFields
        {
            get { return _batchFields; }
            set
            {
                _batchFields = value;
                OnPropertyChanged("BatchFields");
            }
        }

        public ObservableCollection<DocTypeModel> DocTypes
        {
            get { return _docTypes; }
            set
            {
                _docTypes = value;
                OnPropertyChanged("DocTypes");
            }
        }

        public DocTypeModel SelectedDocType
        {
            get { return _selectedDocType; }
            set
            {
                _selectedDocType = value;
                OnPropertyChanged("SelectedDocType");
            }
        }

        public BatchFieldViewModel BatchFieldViewModel { get; set; }

        public DocTypeViewModel DocTypeViewModel { get; set; }

        /// <summary>
        /// Enable or disable the modification of batch type field
        /// </summary>
        public bool CanEditBatchTypeField
        {
            get { return _canEditBatchTypeField; }
            set
            {
                if (_canEditBatchTypeField != value)
                {
                    _canEditBatchTypeField = value;
                    OnPropertyChanged("CanEditBatchTypeField");
                }
            }
        }

        /// <summary>
        /// Enable or disable the modification of doc type field
        /// </summary>
        public bool CanEditDocTypeField
        {
            get { return _canEditDocTypeField; }
            set
            {
                if (_canEditDocTypeField != value)
                {
                    _canEditDocTypeField = value;
                    OnPropertyChanged("CanEditDocTypeField");
                }
            }
        }

        public ICommand BrowseCommand
        {
            get { return _browseCommand ?? (_browseCommand = new RelayCommand(p => Browse())); }
        }

        public ICommand AddBatchTypeCommand
        {
            get
            {
                if (_addBatchTypeCommand == null)
                {
                    _addBatchTypeCommand = new RelayCommand(p => AddBatchType());
                }

                return _addBatchTypeCommand;
            }
        }

        public ICommand DeleteBatchTypeCommand
        {
            get
            {
                if (_deleteBatchTypeCommand == null)
                {
                    _deleteBatchTypeCommand = new RelayCommand(p => DeleteBatchType(), p => CanDeleteBatchType());
                }

                return _deleteBatchTypeCommand;
            }
        }

        public ICommand SaveCommand
        {
            get { return _saveCommand ?? (_saveCommand = new RelayCommand(p => SaveBatchType(), p => CanSaveBatchType())); }
        }

        public ICommand CancelCommand
        {
            get { return _cancelCommand ?? (_cancelCommand = new RelayCommand(p => Cancel())); }
        }

        public ICommand AddBatchFieldCommand
        {
            get { return _addBatchFieldCommand ?? (_addBatchFieldCommand = new RelayCommand(p => AddBatchField(), p => CanAddBatchField())); }
        }

        public ICommand EditBatchFieldCommand
        {
            get { return _editBatchFieldCommand ?? (_editBatchFieldCommand = new RelayCommand(p => EditBatchField(), p => CanEditBatchField())); }
        }

        public ICommand DeleteBatchFieldCommand
        {
            get
            {
                if (_deleteBatchFieldCommand == null)
                {
                    _deleteBatchFieldCommand = new RelayCommand(p => DeleteBatchField(), p => CanEditBatchField());
                }

                return _deleteBatchFieldCommand;
            }
        }

        public ICommand AddDocTypeCommand
        {
            get { return _addDocTypeCommand ?? (_addDocTypeCommand = new RelayCommand(p => AddDocType(), p => CanAddDocType())); }
        }

        public ICommand EditDocTypeCommand
        {
            get { return _editDocTypeCommand ?? (_editDocTypeCommand = new RelayCommand(p => EditDocType(), p => CanEditDocType())); }
        }

        public ICommand ViewDocTypeCommand
        {
            get { return _viewDocTypeCommand ?? (_viewDocTypeCommand = new RelayCommand(p => EditDocType())); }
        }

        public ICommand DeleteDocTypeCommand
        {
            get
            {
                if (_deleteDocTypeCommand == null)
                {
                    _deleteDocTypeCommand = new RelayCommand(p => DeleteDocType(), p => CanEditDocType());
                }

                return _deleteDocTypeCommand;
            }
        }

        public ICommand ConfigWorkflowCommand
        {
            get
            {
                if (_configWorkflowCommand == null)
                {
                    _configWorkflowCommand = new RelayCommand(p => ConfigWorkflow(p));
                }

                return _configWorkflowCommand;
            }
        }

        public ICommand ConfigBarcodeCommand
        {
            get
            {
                if (_configBarcodeCommand == null)
                {
                    _configBarcodeCommand = new RelayCommand(p => ConfigBarcode(p));
                }

                return _configBarcodeCommand;
            }
        }

        public ICommand ConfigOcrCommand
        {
            get
            {
                if (_configOcrCommand == null)
                {
                    _configOcrCommand = new RelayCommand(p => ConfigOcr(p), p => CanConfigOcr(p));
                }

                return _configOcrCommand;
            }
        }

        private bool CanConfigOcr(object p)
        {
            var docType = p as DocTypeModel;
            return docType != null && !docType.HaveDoc;
        }

        public ICommand DeleteOcrCommand
        {
            get
            {
                if (_deleteOcrCommand == null)
                {
                    _deleteOcrCommand = new RelayCommand(p => DeleteOcr(p), p => CanConfigOcr(p));
                }

                return _deleteOcrCommand;
            }
        }

        #region Public methods

        public sealed override void Initialize()
        {
            LoadData();
        }

        public void CreateEditedBatchType()
        {
            if (BatchType != null)
            {
                EditBatchType = new BatchTypeModel
                {
                    CreatedBy = BatchType.CreatedBy,
                    CreatedDate = BatchType.CreatedDate,
                    Fields = BatchType.Fields,
                    DocTypes = BatchType.DocTypes,
                    Id = BatchType.Id,
                    ModifiedBy = BatchType.ModifiedBy,
                    ModifiedDate = BatchType.ModifiedDate,
                    Name = BatchType.Name,
                    UniqueId = BatchType.UniqueId,
                    Icon = BatchType.Icon,
                    ErrorChecked = CheckBatchTypeExisted,
                    Description = BatchType.Description,
                    IsApplyForOutlook = BatchType.IsApplyForOutlook
                };

                CanEditBatchTypeField = _batchTypeProvider.CanEditBatchTypeField(BatchType.Id);

                if (EditBatchType.DocTypes != null)
                {
                    var docTypeHaveDoc = _docTypeProvider.CheckDocTypeHaveDocument(EditBatchType.DocTypes.Select(h => h.Id).ToList());
                    foreach (var docType in EditBatchType.DocTypes)
                    {
                        docType.HaveDoc = docTypeHaveDoc.Contains(docType.Id);
                    }
                }
            }
        }

        public List<UserGroup> GetUserGroups()
        {
            return _workflowProvider.GetUserGroups();
        }

        public WorkflowDefinition GetWorkflowDefinition(Guid batchTypeId)
        {
            return _workflowProvider.GetWorkflowDefinition(batchTypeId);
        }

        //public List<HumanStepPermission> GetHumanStepPermissions(Guid workflowDefinitionId)
        //{
        //    return _workflowProvider.GetWorkflowHumanStepPermissions(workflowDefinitionId);
        //}

        public List<CustomActivitySetting> GetCustomActivitySettings(Guid workflowDefinitionId)
        {
            return _workflowProvider.GetCustomActivitySettings(workflowDefinitionId);
        }

        public Guid SaveWorkflow(Guid batchTypeId, WorkflowDefinition workflowDefinition, List<CustomActivitySetting> customActivitySetting)
        {
            return _workflowProvider.SaveWorkflow(batchTypeId, workflowDefinition, customActivitySetting);
        }
        //public Guid SaveWorkflow(Guid batchTypeId, WorkflowDefinition workflowDefinition, List<HumanStepPermission> humanStepPermissions, List<CustomActivitySetting> customActivitySetting)
        //{
        //    return _workflowProvider.SaveWorkflow(batchTypeId, workflowDefinition, humanStepPermissions, customActivitySetting);
        //}

        public void DeleteOcrTemplate(DocTypeModel docType)
        {
            try
            {
                if (DialogService.ShowTwoStateConfirmDialog(_resouceManager.GetString("uiConfirmDeleteOCRTemplate")) == DialogServiceResult.Yes)
                {
                    _docTypeProvider.DeleteOcrTemplate(docType.Id);
                    docType.OCRTemplate = null;
                }
            }
            catch (Exception ex)
            {
                ProcessHelper.ProcessException(ex);
            }
        }

        public void SaveOcrTemplate(DocTypeModel docType, OCRTemplateModel ocrTemplate)
        {
            ocrTemplate.DocTypeId = docType.Id;
            _docTypeProvider.SaveOcrTemplate(ocrTemplate);
            docType.OCRTemplate = ocrTemplate;
        }

        public void DeleteBarcode(DocTypeModel docType)
        {
            try
            {
                if (DialogService.ShowTwoStateConfirmDialog(_resouceManager.GetString("uiConfirmDeleteBarcode")) == DialogServiceResult.Yes)
                {
                    //_docTypeProvider.ClearBarcodeConfigurations(docType.Id);
                    //docType.BarcodeConfigurations.Clear();
                }
            }
            catch (Exception ex)
            {
                ProcessHelper.ProcessException(ex);
            }
        }

        public List<LanguageModel> GetLanguages()
        {
            return _languageProvider.GetLanguages();
        }

        #endregion

        #region Private methods

        private void LoadData()
        {
            try
            {
                BatchType = null;
                EditBatchType = null;
                BatchTypes = _batchTypeProvider.GetBatchTypes();
            }
            catch (Exception ex)
            {
                ProcessHelper.ProcessException(ex);
            }
        }

        private void BatchTypePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Name" || e.PropertyName == "Description" || e.PropertyName == "Icon")
            {
                _isDataModified = true;
            }
        }

        private void Browse()
        {
            string filePath = DialogService.ShowFileBrowseDialog("Image files (*.png, *.jpg)|*.png;*.jpg");
            if (!string.IsNullOrEmpty(filePath))
            {
                IconFilePath = filePath;
                string extension = Path.GetExtension(filePath) + string.Empty;
                var allowedExtensions = new[] { ".png", ".jpg" };
                if (allowedExtensions.Contains(extension.ToLower()))
                {
                    EditBatchType.Icon = Utility.UtilsStream.ReadAllBytes(new FileStream(filePath, FileMode.Open, FileAccess.Read));
                }
                else
                {
                    DialogService.ShowErrorDialog("Only image is supported. (PNG | JPG)");
                }
            }
        }

        private void CheckBatchTypeExisted(BatchTypeModel batchType)
        {
            if (!string.IsNullOrWhiteSpace(batchType.Name))
            {
                batchType.HasError = BatchTypes.Any(p => p.Name.Equals(batchType.Name, StringComparison.CurrentCultureIgnoreCase) && p.Id != batchType.Id);
            }
            else
            {
                batchType.HasError = true;
            }
        }

        private void AddBatchType()
        {
            BatchType = null;
            IconFilePath = null;

            EditBatchType = new BatchTypeModel(CheckBatchTypeExisted);
            CanEditBatchTypeField = true;
            EditPanelVisibled = true;
        }

        private bool CanDeleteBatchType()
        {
            return BatchType != null && BatchType.Id != Guid.Empty;
        }

        private void DeleteBatchType()
        {
            if (DialogService.ShowTwoStateConfirmDialog(_resouceManager.GetString("uiConfirmDelete")) == DialogServiceResult.Yes)
            {
                IsProcessing = true;
                var worker = new BackgroundWorker();
                worker.DoWork += DoDeleteBatchType;
                worker.RunWorkerCompleted += DoDeleteBatchTypeCompleted;
                worker.RunWorkerAsync();
            }
        }

        private void DoDeleteBatchType(object sender, DoWorkEventArgs e)
        {
            try
            {
                _batchTypeProvider.DeleteBatchType(BatchType.Id);
            }
            catch (Exception ex)
            {
                e.Result = ex;
            }
        }

        private void DoDeleteBatchTypeCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            try
            {
                if (e.Result is Exception)
                {
                    ProcessHelper.ProcessException(e.Result as Exception);
                }
                else
                {
                    LoadData();
                }
            }
            finally
            {
                IsProcessing = false;
            }
        }

        private void Cancel()
        {
            LoadData();

            //EditBatchType.IsRefresh = true;
            //BatchType.IsRefresh = true;
            //IconFilePath = string.Empty;
            //EditBatchType = null;
            //EditPanelVisibled = false;
            //BatchType = null;
        }

        private bool CanSaveBatchType()
        {
            return EditBatchType != null && !string.IsNullOrEmpty(EditBatchType.Name) &&
                   EditBatchType.Fields != null && EditBatchType.Fields.Count > 0 &&
                   EditBatchType.DocTypes != null && EditBatchType.DocTypes.Count > 0 &&
                   !EditBatchType.HasError && _isDataModified;
        }

        private void SaveBatchType()
        {
            IsProcessing = true;

            var worker = new BackgroundWorker();
            worker.DoWork += DoSave;
            worker.RunWorkerCompleted += DoSaveCompleted;
            worker.RunWorkerAsync();
        }

        private void DoSave(object sender, DoWorkEventArgs e)
        {
            int index = 0;
            try
            {
                if (EditBatchType.IsApplyForOutlook)
                {
                    GetOutlookField();
                }

                foreach (var field in EditBatchType.Fields)
                {
                    field.DisplayOrder = ++index;
                }

                foreach (var docType in EditBatchType.DocTypes)
                {
                    index = 0;
                    foreach (var field in docType.Fields)
                    {
                        field.DisplayOrder = ++index;
                    }
                }

                if (EditBatchType.Id == Guid.Empty && (EditBatchType.Icon == null || EditBatchType.Icon.Length == 0))
                {
                    var image = (Bitmap)_resouceManager.GetObject("DefaultIcon");
                    EditBatchType.Icon = (byte[])(new ImageConverter()).ConvertTo(image, typeof(byte[]));
                }

                _batchTypeProvider.SaveBatchType(EditBatchType);
            }
            catch (Exception ex)
            {
                e.Result = ex;
            }
        }

        private void DoSaveCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            try
            {
                if (e.Result == null)
                {
                    LoadData();
                }
                else
                {
                    ProcessHelper.ProcessException((Exception)e.Result);
                }
            }
            finally
            {
                IsProcessing = false;
            }
        }

        private void OnSaveBatchField(FieldModel field)
        {
            if (field.DataType != FieldDataType.Date)
            {
                field.UseCurrentDate = false;
            }

            ObservableCollection<FieldModel> batchFields = EditBatchType.Fields;
            if (field.Id == Guid.Empty)
            {
                if (BatchFieldViewModel.IsEditMode)
                {
                    FieldModel deleteField = batchFields.FirstOrDefault(f => f.IsSelected);
                    int index = -1;
                    if (deleteField != null)
                    {
                        index = batchFields.IndexOf(deleteField);
                        batchFields.Remove(deleteField);
                    }

                    if (index == -1)
                    {
                        batchFields.Add(field);
                    }
                    else
                    {
                        batchFields.Insert(index, field);
                    }
                }
                else
                {
                    batchFields.Add(field);
                }
            }
            else
            {
                FieldModel editField = batchFields.FirstOrDefault(f => f.Id == field.Id);

                if (editField != null)
                {
                    editField.DataType = field.DataType;
                    editField.MaxLength = field.MaxLength;
                    editField.DefaultValue = field.DefaultValue;
                    editField.DisplayOrder = field.DisplayOrder;
                    editField.Id = field.Id;
                    editField.BatchTypeId = EditBatchType.Id;
                    editField.IsSelected = field.IsSelected;
                    editField.IsSystemField = field.IsSystemField;
                    editField.Name = field.Name;
                    editField.UseCurrentDate = field.UseCurrentDate;
                }
            }

            _isDataModified = true;
            BatchFields = batchFields;
        }

        private bool CanAddBatchField()
        {
            return EditBatchType != null && !string.IsNullOrWhiteSpace(EditBatchType.Name) && CanEditBatchTypeField;
        }

        private bool CanEditBatchField()
        {
            return CanAddBatchField() && EditBatchType.Fields != null && EditBatchType.Fields.Any(f => f.IsSelected);
        }

        private void EditBatchField()
        {
            FieldModel selectField = EditBatchType.Fields.FirstOrDefault(f => f.IsSelected);
            if (selectField != null)
            {
                BatchFieldViewModel = new BatchFieldViewModel(OnSaveBatchField, this)
                {
                    IsEditMode = true,
                    Field = new FieldModel
                    {
                        DataType = selectField.DataType,
                        DisplayOrder = selectField.DisplayOrder,
                        Id = selectField.Id,
                        BatchTypeId = selectField.BatchTypeId,
                        IsSelected = selectField.IsSelected,
                        IsSystemField = selectField.IsSystemField,
                        Name = selectField.Name,
                        MaxLength = selectField.MaxLength,
                        DefaultValue = selectField.DefaultValue,
                        UseCurrentDate = selectField.UseCurrentDate,
                        ErrorMessages = _errorMessage,
                        ErrorChecked = CheckBatchField,
                        WorkingId = selectField.WorkingId
                    }
                };

                LoadBatchFieldView(BatchFieldViewModel, true);
            }
        }

        private void CheckBatchField(FieldModel field)
        {
            if (field.Id == Guid.Empty)
            {
                field.HasErrorWithName = EditBatchType.Fields.FirstOrDefault(f => f.Name.Equals(field.Name, StringComparison.CurrentCultureIgnoreCase) && f.WorkingId != field.WorkingId) != null;
            }
            else
            {
                field.HasErrorWithName = EditBatchType.Fields.FirstOrDefault(f => f.Name.Equals(field.Name, StringComparison.CurrentCultureIgnoreCase) && f.Id != field.Id) != null;
            }
        }

        private void LoadBatchFieldView(BatchFieldViewModel viewModel, bool isEdit)
        {
            ResourceManager localResource = new ResourceManager("Ecm.CaptureAdmin.CreateBatchFieldView", Assembly.GetExecutingAssembly());
            string editTitle = localResource.GetString("dgEditDialogTitle.Text");
            string addTitle = localResource.GetString("dgAddDialogTitle.Text");
            viewModel.CloseDialog += ViewModelCloseDialog;

            var view = new CreateBatchFieldView(viewModel);
            _dialog = new DialogBaseView(view) { Size = new Size(600, 400), Text = isEdit ? editTitle : addTitle };
            _dialog.ShowDialog();
        }

        private void ViewModelCloseDialog()
        {
            if (_dialog != null)
            {
                _dialog.Close();
            }
        }

        private void DeleteBatchField()
        {
            if (DialogService.ShowTwoStateConfirmDialog(_resouceManager.GetString("uiConfirmDelete")) == DialogServiceResult.Yes)
            {
                FieldModel deleteField = EditBatchType.Fields.FirstOrDefault(f => f.IsSelected);

                if (deleteField != null)
                {
                    EditBatchType.Fields.Remove(deleteField);

                    if (EditBatchType.Id != Guid.Empty && deleteField.Id != Guid.Empty)
                    {
                        EditBatchType.DeletedFields.Add(deleteField.Id);
                    }

                    _isDataModified = true;
                }
            }
        }

        private void AddBatchField()
        {
            BatchFieldViewModel = new BatchFieldViewModel(OnSaveBatchField, this)
            {
                IsEditMode = false,
                Field = new FieldModel
                {
                    BatchTypeId = EditBatchType.Id,
                    ErrorMessages = _errorMessage,
                    ErrorChecked = CheckBatchField
                }
            };

            LoadBatchFieldView(BatchFieldViewModel, false);
        }

        private bool CanAddDocType()
        {
            return EditBatchType != null && !string.IsNullOrWhiteSpace(EditBatchType.Name);
        }

        private void AddDocType()
        {
            DocTypeViewModel = new DocTypeViewModel(OnSaveDocType, this)
            {
                IsEditMode = false,
                DocType = new DocTypeModel
                {
                    ErrorChecked = CheckAddDocType
                }
            };

            LoadDocTypeView(DocTypeViewModel, false);
        }

        private bool CanEditDocType()
        {
            if (!CanAddDocType() || EditBatchType.DocTypes == null)
            {
                return false;
            }

            var selectedDoctype = EditBatchType.DocTypes.FirstOrDefault(f => f.IsSelected);
            return selectedDoctype != null && !selectedDoctype.HaveDoc;
        }

        private void EditDocType()
        {
            DocTypeModel selectDocType = EditBatchType.DocTypes.FirstOrDefault(f => f.IsSelected);
            if (selectDocType != null)
            {
                DocTypeViewModel = new DocTypeViewModel(OnSaveDocType, this)
                {
                    IsEditMode = true,
                    DocType = new DocTypeModel
                    {
                        Id = selectDocType.Id,
                        BatchTypeId = selectDocType.BatchTypeId,
                        IsSelected = selectDocType.IsSelected,
                        Name = selectDocType.Name,
                        ErrorChecked = CheckEditDocType,
                        //Fields = new ObservableCollection<FieldModel>(selectDocType.Fields.ToList()),
                        Description = selectDocType.Description,
                        HaveDoc = selectDocType.HaveDoc,
                        WorkingId = selectDocType.WorkingId
                    }
                };

                // Clone field model
                if (selectDocType.Fields != null)
                {
                    DocTypeViewModel.DocType.Fields = new ObservableCollection<FieldModel>(selectDocType.Fields.Select(h => (FieldModel)h.Clone()));
                }

                LoadDocTypeView(DocTypeViewModel, true);
            }
        }

        private void OnSaveDocType(DocTypeModel docType)
        {
            ObservableCollection<DocTypeModel> docTypes = EditBatchType.DocTypes;

            if (docType.Id == Guid.Empty)
            {
                docType.Name = string.Format("{0}", docType.Name).Trim();


                if (DocTypeViewModel.IsEditMode)
                {
                    DocTypeModel editDocType = docTypes.FirstOrDefault(f => f.WorkingId == docType.WorkingId);

                    if (editDocType != null)
                    {
                        //editDocType.Id = docType.Id;
                        //editDocType.BatchTypeId = EditBatchType.Id;
                        //editDocType.IsSelected = docType.IsSelected;
                        editDocType.Name = string.Format("{0}", docType.Name).Trim();
                        editDocType.Description = docType.Description;
                        editDocType.Fields = new ObservableCollection<FieldModel>(docType.Fields);
                        editDocType.Fields = docType.Fields;
                        editDocType.DeletedFields = docType.DeletedFields;
                    }
                }
                else
                {
                    docTypes.Add(docType);
                }


                //if (DocTypeViewModel.IsEditMode)
                //{
                //    DocTypeModel deleteDoc = docTypes.FirstOrDefault(f => f.IsSelected);
                //    int index = -1;
                //    if (deleteDoc != null)
                //    {
                //        index = docTypes.IndexOf(deleteDoc);
                //        docTypes.Remove(deleteDoc);
                //    }

                //    if (index == -1)
                //    {
                //        docTypes.Add(docType);
                //    }
                //    else
                //    {
                //        docTypes.Insert(index, docType);
                //    }
                //}
                //else
                //{
                //    docTypes.Add(docType);
                //}
            }
            else
            {
                DocTypeModel editDocType = docTypes.FirstOrDefault(f => f.Id == docType.Id);

                if (editDocType != null)
                {
                    //editDocType.Id = docType.Id;
                    //editDocType.BatchTypeId = EditBatchType.Id;
                    //editDocType.IsSelected = docType.IsSelected;
                    editDocType.Name = string.Format("{0}", docType.Name).Trim();
                    editDocType.Description = docType.Description;
                    editDocType.Fields = new ObservableCollection<FieldModel>(docType.Fields);
                    editDocType.Fields = docType.Fields;
                    editDocType.DeletedFields = docType.DeletedFields;
                }
            }

            _isDataModified = true;
            DocTypes = docTypes;
        }

        private void CheckEditDocType(DocTypeModel docType)
        {
            var docTypeName = string.Format("{0}", docType.Name).Trim();
            docType.HasError =
                string.IsNullOrEmpty(docTypeName)
                || EditBatchType.DocTypes.Any(f => !f.IsSelected &&
                                                    f.Name.Equals(docTypeName,
                                                                    StringComparison.CurrentCultureIgnoreCase));
        }

        private void CheckAddDocType(DocTypeModel docType)
        {
            var docTypeName = string.Format("{0}", docType.Name).Trim();
            docType.HasError =
                string.IsNullOrEmpty(docTypeName)
                || EditBatchType.DocTypes.Any(f => f.Name.Equals(docTypeName,
                                                                    StringComparison.CurrentCultureIgnoreCase));
        }

        private void LoadDocTypeView(DocTypeViewModel viewModel, bool isEdit)
        {
            ResourceManager localResource = new ResourceManager("Ecm.CaptureAdmin.CreateDocTypeViewRes", Assembly.GetExecutingAssembly());
            string editTitle = localResource.GetString("dgEditDialogTitle.Text");
            string addTitle = localResource.GetString("dgAddDialogTitle.Text");
            viewModel.CloseDialog += ViewModelCloseDialog;

            var view = new CreateDocTypeView(viewModel);
            _dialog = new DialogBaseView(view) { Size = new Size(800, 600), Text = isEdit ? editTitle : addTitle };
            _dialog.ShowDialog();
        }

        private void DeleteDocType()
        {
            if (DialogService.ShowTwoStateConfirmDialog(_resouceManager.GetString("uiConfirmDelete")) == DialogServiceResult.Yes)
            {
                DocTypeModel deleteDocType = EditBatchType.DocTypes.FirstOrDefault(f => f.IsSelected);

                if (deleteDocType != null)
                {
                    EditBatchType.DocTypes.Remove(deleteDocType);

                    if (EditBatchType.Id != Guid.Empty && deleteDocType.Id != Guid.Empty)
                    {
                        EditBatchType.DeletedDocTypes.Add(deleteDocType.Id);
                    }

                    _isDataModified = true;
                }
            }
        }

        private void GetOutlookField()
        {
            App.Current.Dispatcher.Invoke((Action)delegate // <--- HERE
            {
                foreach (var docType in EditBatchType.DocTypes)
                {
                    FieldModel field = new FieldModel();

                    field.Name = "Mail from";
                    field.DataType = FieldDataType.String;
                    field.BatchTypeId = EditBatchType.Id;
                    field.IsRequired = true;
                    field.MaxLength = 255;

                    docType.Fields.Add(field);

                    field = new FieldModel();

                    field.Name = "Mail to";
                    field.DataType = FieldDataType.String;
                    field.BatchTypeId = EditBatchType.Id;
                    field.IsRequired = true;
                    field.MaxLength = 255;

                    docType.Fields.Add(field);

                    field = new FieldModel();

                    field.Name = "Mail subject";
                    field.DataType = FieldDataType.String;
                    field.BatchTypeId = EditBatchType.Id;
                    field.IsRequired = true;
                    field.MaxLength = 500;

                    docType.Fields.Add(field);

                    field = new FieldModel();

                    field.Name = "Mail body";
                    field.DataType = FieldDataType.String;
                    field.BatchTypeId = EditBatchType.Id;
                    field.IsRequired = true;
                    field.MaxLength = 4000;

                    docType.Fields.Add(field);

                    field = new FieldModel();

                    field.Name = "Received date";
                    field.DataType = FieldDataType.Date;
                    field.BatchTypeId = EditBatchType.Id;
                    field.IsRequired = true;

                    docType.Fields.Add(field);
                }
            });
        }

        private void ConfigWorkflow(object p)
        {
            try
            {
                BatchType = p as BatchTypeModel;
                BatchType batchType = ObjectMapper.GetBatchType(p as BatchTypeModel);
                WorkflowDefinition workflowDefinition = GetWorkflowDefinition(batchType.Id);
                //List<HumanStepPermission> permissions;
                List<CustomActivitySetting> customActivitySettings;

                if (workflowDefinition == null)
                {
                    workflowDefinition = new WorkflowDefinition
                    {
                        BatchTypeId = batchType.Id,
                        DefinitionXML = string.Empty,
                        Id = Guid.Empty
                    };

                    //permissions = new List<HumanStepPermission>();
                    customActivitySettings = new List<CustomActivitySetting>();
                }
                else
                {
                    //permissions = GetHumanStepPermissions(workflowDefinition.Id);
                    customActivitySettings = GetCustomActivitySettings(workflowDefinition.Id);

                    //if (permissions == null)
                    //{
                    //    permissions = new List<HumanStepPermission>();
                    //}

                    if (customActivitySettings == null)
                    {
                        customActivitySettings = new List<CustomActivitySetting>();
                    }
                }

                DesignerContainerViewModel viewModel = new DesignerContainerViewModel(workflowDefinition.DefinitionXML)
                {
                    HandleExceptionAction = ProcessHelper.ProcessException,
                    LoginUser = ObjectMapper.GetUser(LoginViewModel.LoginUser),
                    UserGroups = GetUserGroups(),
                    DocTypes = batchType.DocTypes,
                    BatchType = batchType,
                    //HumanStepPermissions = permissions,
                    CustomActivitySettings = customActivitySettings
                };

                viewModel.LoginUser.Photo = null;
                foreach (var userGroup in viewModel.UserGroups)
                {
                    foreach (var user in userGroup.Users)
                    {
                        user.Photo = null;
                    }
                }

                DesignerContainer workflowDesigner = new DesignerContainer(viewModel);
                viewModel.SaveWorkflow += WorkflowDesignerSaveWorkflow;
                viewModel.LoadWorkflow();

                DialogBaseView dialog = new DialogBaseView
                {
                    Text = "Workflow designer",
                    Size = new System.Drawing.Size(1200, 700),
                    EnableToResize = true,
                    WpfContent = workflowDesigner,
                    WindowState = System.Windows.Forms.FormWindowState.Maximized
                };

                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    LoadData();
                }
            }
            catch (Exception ex)
            {
                ProcessHelper.ProcessException(ex);
            }
        }

        private void WorkflowDesignerSaveWorkflow(Guid batchTypeId, string workflowDefinition, List<CustomActivitySetting> customActivitySettings)
        {
            try
            {
                WorkflowDefinition wfDefinition = new WorkflowDefinition
                {
                    BatchTypeId = batchTypeId,
                    DefinitionXML = workflowDefinition
                };

                Guid workflowDefinitionId = SaveWorkflow(batchTypeId, wfDefinition, customActivitySettings);
                BatchType.WorkflowDefinitionId = workflowDefinitionId;
            }
            catch (Exception ex)
            {
                ProcessHelper.ProcessException(ex);
            }
        }
        //private void WorkflowDesignerSaveWorkflow(Guid batchTypeId, string workflowDefinition, List<HumanStepPermission> humanStepPermissions, List<CustomActivitySetting> customActivitySettings)
        //{
        //    try
        //    {
        //        WorkflowDefinition wfDefinition = new WorkflowDefinition
        //        {
        //            BatchTypeId = batchTypeId,
        //            DefinitionXML = workflowDefinition
        //        };

        //        Guid workflowDefinitionId = SaveWorkflow(batchTypeId, wfDefinition, humanStepPermissions, customActivitySettings);
        //        BatchType.WorkflowDefinitionId = workflowDefinitionId;
        //    }
        //    catch (Exception ex)
        //    {
        //        ProcessHelper.ProcessException(ex);
        //    }
        //}

        private void ConfigBarcode(object p)
        {
            try
            {
                BatchTypeModel batchType = p as BatchTypeModel;
                var viewModel = new BarcodeConfigurationViewModel(batchType, batchType.DocTypes.ToList());
                var view = new BarcodeConfigurationView(viewModel);

                DialogBaseView dialog = new DialogBaseView(view);
                dialog.Width = 850;
                dialog.Height = 700;
                dialog.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
                dialog.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
                dialog.MaximizeBox = false;
                dialog.MinimizeBox = false;
                dialog.Text = _resouceManager.GetString("uiDialogTitle");
                view.Dialog = dialog;

                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    LoadData();
                }
            }
            catch (Exception ex)
            {
                ProcessHelper.ProcessException(ex);
            }
        }

        private void DeleteOcr(object p)
        {
            try
            {
                var docType = p as DocTypeModel;
                if (docType != null)
                {
                    DeleteOcrTemplate(docType);
                    LoadData();
                }
            }
            catch (Exception ex)
            {
                ProcessHelper.ProcessException(ex);
            }
        }

        private void ConfigOcr(object p)
        {
            try
            {
                var docType = p as DocTypeModel;
                docType.IsSelected = true;
                docType.BatchType = BatchType;

                var editDocType = new DocTypeModel
                {
                    AnnotationPermission = docType.AnnotationPermission,
                    BarcodeConfigurations = docType.BarcodeConfigurations,
                    BatchType = docType.BatchType,
                    BatchTypeId = docType.BatchTypeId,
                    CreatedBy = docType.CreatedBy,
                    CreatedDate = docType.CreatedDate,
                    DocTypePermission = docType.DocTypePermission,
                    Id = docType.Id,
                    IsSelected = docType.IsSelected,
                    HasOCRTemplateDefined = docType.HasOCRTemplateDefined,
                    Name = docType.Name,
                    OCRTemplate = docType.OCRTemplate,
                    Fields = docType.Fields
                };

                if (docType != null)
                {
                    ConfigOCRTemplateViewModel ocrTemplateViewModel = new ConfigOCRTemplateViewModel(editDocType, GetLanguages());
                    ocrTemplateViewModel.SaveOcrTemplate += OcrTemplateViewModel_SaveOcrTemplate;

                    ConfigOCRTemplateView view = new ConfigOCRTemplateView
                    {
                        DataContext = ocrTemplateViewModel
                    };

                    DialogBaseView dialog = new DialogBaseView
                    {
                        Text = "Configure OCR template",
                        Size = new System.Drawing.Size(900, 700),
                        EnableToResize = true,
                        WpfContent = view,
                        WindowState = System.Windows.Forms.FormWindowState.Maximized
                    };

                    if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        docType.OCRTemplate = editDocType.OCRTemplate;
                        LoadData();
                    }
                }
            }
            catch (Exception ex)
            {
                ProcessHelper.ProcessException(ex);
            }
        }

        private void OcrTemplateViewModel_SaveOcrTemplate(Guid docTypeId, OCRTemplateModel ocrTemplate)
        {
            DocTypeModel docType = DocTypes.FirstOrDefault(p => p.Id == docTypeId);
            if (docType != null)
            {
                SaveOcrTemplate(docType, ocrTemplate);
            }
        }

        #endregion
    }
}
