using System;
using System.Linq;
using Ecm.Domain;
using Ecm.Mvvm;
using System.Windows.Input;
using Ecm.Model;
using System.Collections.ObjectModel;
using Ecm.Model.DataProvider;
using Ecm.Admin.View;
using System.Collections.Generic;
using System.ComponentModel;
using System.Resources;
using System.Reflection;
using System.IO;
using System.Drawing;
using System.Windows.Forms;

namespace Ecm.Admin.ViewModel
{
    public class DocumentTypeViewModel : ComponentViewModel
    {
        private RelayCommand _addDocTypeCommand;
        private RelayCommand _deleteDocTypeCommand;
        private RelayCommand _saveCommand;
        private RelayCommand _cancelCommand;
        private RelayCommand _deleteFieldCommand;
        private RelayCommand _addFieldCommand;
        private RelayCommand _editFieldCommand;
        private RelayCommand _lookupCommand;
        private RelayCommand _deleteLookupCommand;
        private RelayCommand _browseCommand;

        private DocumentTypeModel _documentType;
        private DocumentTypeModel _editDocumentType;
        private FieldMetaDataModel _selectedField;
        private ObservableCollection<FieldMetaDataModel> _fields = new ObservableCollection<FieldMetaDataModel>();
        private ObservableCollection<DocumentTypeModel> _documentTypes = new ObservableCollection<DocumentTypeModel>();
        private readonly DocumentTypeProvider _documentTypeProvier = new DocumentTypeProvider();
        private DialogBaseView _dialog;
        private readonly ResourceManager _resource = new ResourceManager("Ecm.Admin.CreateFieldView", Assembly.GetExecutingAssembly());
        private readonly ResourceManager _res = new ResourceManager("Ecm.Admin.Resources", Assembly.GetExecutingAssembly());
        private readonly Dictionary<string, string> _errorMessage = new Dictionary<string, string>();
        private string _iconFilePath;
        private bool _isDataModified;

        public DocumentTypeViewModel(MainViewModel mainViewModel)
        {
            MainViewModel = mainViewModel;

            _errorMessage.Add("uiInvalidIntegerValue", _res.GetString("uiInvalidIntegerValue"));
            _errorMessage.Add("uiInvalidDecimalValue", _res.GetString("uiInvalidDecimalValue"));
            _errorMessage.Add("uiFieldNameExisted", _res.GetString("uiFieldNameExisted"));
        }

        public sealed override void Initialize()
        {
            LoadData();
        }

        public FieldViewModel FieldViewModel { get; set; }

        public LookupViewModel LookupViewModel { get; set; }

        public ObservableCollection<FieldMetaDataModel> Fields
        {
            get { return _fields; }
            set
            {
                _fields = value;
                OnPropertyChanged("Fields");
            }
        }

        public FieldMetaDataModel SelectedField
        {
            get { return _selectedField; }
            set
            {
                _selectedField = value;
                OnPropertyChanged("SelectedField");
            }
        }

        public DocumentTypeModel DocType
        {
            get { return _documentType; }
            set
            {
                _documentType = value;
                OnPropertyChanged("DocType");
                _isDataModified = false;
            }
        }

        public DocumentTypeModel EditDocType
        {
            get { return _editDocumentType; }
            set
            {
                _editDocumentType = value;
                if (value != null)
                {
                    _iconFilePath = string.Empty;
                    EditPanelVisibled = true;

                    List<FieldMetaDataModel> fieldListWithoutSystemFields = _editDocumentType.Fields.Where(f => !f.IsSystemField).ToList();
                    _editDocumentType.Fields.Clear();

                    foreach (var field in fieldListWithoutSystemFields)
                    {
                        _editDocumentType.Fields.Add(field);
                    }

                    _editDocumentType.PropertyChanged += DocumentTypePropertyChanged;
                }
                else
                {
                    EditPanelVisibled = false;
                }

                IconFilePath = null;
                CommandManager.InvalidateRequerySuggested();
                OnPropertyChanged("EditDocType");
            }
        }

        public ObservableCollection<DocumentTypeModel> DocumentTypes
        {
            get { return _documentTypes; }
            set
            {
                _documentTypes = value;
                OnPropertyChanged("DocumentTypes");
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

        public MainViewModel MainViewModel { get; private set; }

        public ICommand AddDocTypeCommand
        {
            get { return _addDocTypeCommand ?? (_addDocTypeCommand = new RelayCommand(p => AddDocType())); }
        }

        public ICommand DeleteDocTypeCommand
        {
            get { return _deleteDocTypeCommand ?? (_deleteDocTypeCommand = new RelayCommand(p => DeleteDocType(), p => CanDeleteDocType())); }
        }

        public ICommand SaveCommand
        {
            get { return _saveCommand ?? (_saveCommand = new RelayCommand(p => SaveDocType(), p => CanSaveDocType())); }
        }

        public ICommand CancelCommand
        {
            get { return _cancelCommand ?? (_cancelCommand = new RelayCommand(p => Cancel())); }
        }

        public ICommand DeleteFieldCommand
        {
            get { return _deleteFieldCommand ?? (_deleteFieldCommand = new RelayCommand(p => DeleteField(), p => CanDeleteField())); }
        }

        public ICommand AddFieldCommand
        {
            get { return _addFieldCommand ?? (_addFieldCommand = new RelayCommand(p => AddField())); }
        }

        public ICommand EditFieldCommand
        {
            get { return _editFieldCommand ?? (_editFieldCommand = new RelayCommand(p => EditField(), p => CanEditField())); }
        }

        public ICommand LookupCommand
        {
            get { return _lookupCommand ?? (_lookupCommand = new RelayCommand(Lookup, CanLookup)); }
        }

        public ICommand DeleteLookupCommand
        {
            get { return _deleteLookupCommand ?? (_deleteLookupCommand = new RelayCommand(DeleteLookup)); }
        }

        public ICommand BrowseCommand
        {
            get { return _browseCommand ?? (_browseCommand = new RelayCommand(p => Browse())); }
        }

        public void LoadFieldViewModel(FieldMetaDataModel field)
        {
            FieldViewModel = new FieldViewModel(OnSaveFieldComplete, this) { Field = field };
        }

        public void CreateEditedDocType()
        {
            if (DocType != null)
            {
                EditDocType = new DocumentTypeModel
                {
                    DocumentTypePermission = DocType.DocumentTypePermission,
                    AnnotationPermission = DocType.AnnotationPermission,
                    CreateBy = DocType.CreateBy,
                    CreatedDate = DocType.CreatedDate,
                    Fields = DocType.Fields,
                    Id = DocType.Id,
                    IsOutlook = DocType.IsOutlook,
                    ModifiedBy = DocType.ModifiedBy,
                    ModifiedDate = DocType.ModifiedDate,
                    Name = DocType.Name,
                    OCRTemplate = DocType.OCRTemplate,
                    //UniqueId = DocType.UniqueId,
                    Icon = DocType.Icon,
                    ErrorChecked = CheckDocumentTypeExisted
                };

                Fields = new ObservableCollection<FieldMetaDataModel>(DocType.Fields.Where(p => p.ParentFieldId == null).ToList());
            }
        }

        public void ConfigOcrTemplate()
        {
            try
            {
                var editDocType = new DocumentTypeModel
                {
                    AnnotationPermission = DocType.AnnotationPermission,
                    BarcodeConfigurations = DocType.BarcodeConfigurations,
                    CreateBy = DocType.CreateBy,
                    CreatedDate = DocType.CreatedDate,
                    DocumentCount = DocType.DocumentCount,
                    DocumentTypePermission = DocType.DocumentTypePermission,
                    Fields = DocType.Fields,
                    HasOCRTemplateDefined = DocType.HasOCRTemplateDefined,
                    Id = DocType.Id,
                    IsOutlook = DocType.IsOutlook,
                    IsSelected = DocType.IsSelected,
                    OCRTemplate = DocType.OCRTemplate
                };

                ConfigOCRTemplateViewModel viewModel = new ConfigOCRTemplateViewModel(editDocType);
                ConfigOCRTemplateView view = new ConfigOCRTemplateView(viewModel);
                DialogBaseView ocrTemplateDialog = new DialogBaseView(view);
                ocrTemplateDialog.Text = _res.GetString("uiOcrTemplate");
                ocrTemplateDialog.Size = new System.Drawing.Size(900, 700);
                ocrTemplateDialog.EnableToResize = true;
                ocrTemplateDialog.WindowState = System.Windows.Forms.FormWindowState.Maximized;
                view.Dialog = ocrTemplateDialog;

                if (ocrTemplateDialog.ShowDialog() == DialogResult.OK)
                {
                    DocType.OCRTemplate = editDocType.OCRTemplate;
                }
            }
            catch (Exception ex)
            {
                ProcessHelper.ProcessException(ex);
            }
        }

        public void DeleteOCRTemplate()
        {
            try
            {
                if (DialogService.ShowTwoStateConfirmDialog(_res.GetString("uiConfirmDeleteOCRTemplate")) == DialogServiceResult.Yes)
                {
                    _documentTypeProvier.DeleteOCRTemplate(DocType.Id);
                    DocType.OCRTemplate = null;
                }
            }
            catch (Exception ex)
            {
                ProcessHelper.ProcessException(ex);
            }
        }

        public void ConfigBarcode()
        {
            try
            {
                MainViewModel.ViewModel = new ConfigBarcodeViewModel(DocType, MainViewModel);
            }
            catch (Exception ex)
            {
                ProcessHelper.ProcessException(ex);
            }
        }

        public void DeleteBarcode()
        {
            try
            {
                if (DialogService.ShowTwoStateConfirmDialog(_res.GetString("uiConfirmDeleteBarcode")) == DialogServiceResult.Yes)
                {
                    _documentTypeProvier.ClearBarcodeConfigurations(DocType.Id);
                    DocType.BarcodeConfigurations.Clear();
                }
            }
            catch (Exception ex)
            {
                ProcessHelper.ProcessException(ex);
            }
        }

        private bool CanEditField()
        {
            return EditDocType != null && Fields != null && Fields.Any(f => f.IsSelected);
        }

        private void Browse()
        {
            string filePath = DialogService.ShowFileBrowseDialog(string.Empty);
            if (!string.IsNullOrEmpty(filePath))
            {
                IconFilePath = filePath;
                string extension = Path.GetExtension(filePath) + string.Empty;
                var allowedExtensions = new[] { ".png" };
                if (allowedExtensions.Contains(extension.ToLower()))
                {
                    EditDocType.Icon = Utility.UtilsStream.ReadAllBytes(new FileStream(filePath, FileMode.Open, FileAccess.Read));
                }
                else
                {
                    DialogService.ShowErrorDialog("Only image is supported. (PNG)");
                }
            }
        }

        private void EditField()
        {
            FieldViewModel.IsEditMode = true;
            FieldViewModel.Field = new FieldMetaDataModel
            {
                DataType = SelectedField.DataType,
                DisplayOrder = SelectedField.DisplayOrder,
                Id = SelectedField.Id,
                DocTypeId = SelectedField.DocTypeId,
                IsLookup = SelectedField.IsLookup,
                IsRequired = SelectedField.IsRequired,
                IsRestricted = SelectedField.IsRestricted,
                IsSelected = SelectedField.IsSelected,
                IsSystemField = SelectedField.IsSystemField,
                Name = SelectedField.Name,
                MaxLength = SelectedField.MaxLength,
                DefaultValue = SelectedField.DefaultValue,
                UseCurrentDate = SelectedField.UseCurrentDate,
                Picklists = SelectedField.Picklists ?? new ObservableCollection<PicklistModel>(),
                ErrorMessages = _errorMessage,
                ErrorChecked = CheckField
            };

            foreach (var selected in SelectedField.Children)
            {
                FieldViewModel.Field.Children.Add(selected);
            }

            LoadFieldView(FieldViewModel, true);
        }

        private void AddField()
        {
            FieldViewModel = new FieldViewModel(OnSaveFieldComplete, this)
            {
                IsEditMode = false,
                Field = new FieldMetaDataModel(CheckField)
                {
                    DocTypeId = EditDocType.Id,
                    Picklists = new ObservableCollection<PicklistModel>(),
                    ErrorMessages = _errorMessage
                }
            };

            LoadFieldView(FieldViewModel, false);
        }

        private bool CanDeleteField()
        {
            return EditDocType != null && Fields != null && Fields.Any(f => f.IsSelected);
        }

        private void DeleteField()
        {
            FieldMetaDataModel deleteField = Fields.FirstOrDefault(f => f.IsSelected);

            if (deleteField != null)
            {
                Fields.Remove(deleteField);
                if (DocType != null && DocType.DeletedFields == null)
                {
                    DocType.DeletedFields = new List<FieldMetaDataModel>();
                }

                if (EditDocType.Id != Guid.Empty)
                {
                    EditDocType.DeletedFields.Add(deleteField);
                }

                OnPropertyChanged("Fields");
                _isDataModified = true;
            }
        }

        private void Cancel()
        {
            _iconFilePath = string.Empty;
            EditDocType = null;
            EditPanelVisibled = false;
            DocType = null;
        }

        private bool CanSaveDocType()
        {
            return EditDocType != null && !string.IsNullOrEmpty(EditDocType.Name) && Fields != null && Fields.Count > 0 && _isDataModified && !EditDocType.HasError;
        }

        private void SaveDocType()
        {
            IsProcessing = true;

            EditDocType.Fields.Clear();
            var worker = new BackgroundWorker();
            worker.DoWork += DoSave;
            worker.RunWorkerCompleted += DoSaveCompleted;
            worker.RunWorkerAsync();
        }

        private void DoSaveCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            IsProcessing = false;
            if (e.Result == null)
            {
                DocType = null;
                LoadData();
            }
            else
            {
                ProcessHelper.ProcessException((Exception)e.Result);
            }
        }

        private void DoSave(object sender, DoWorkEventArgs e)
        {
            int index = 0;
            try
            {
                foreach (var field in Fields)
                {
                    index++;
                    field.DisplayOrder = index;
                    
                    if (field.Children != null && field.Children.Count() > 0)
                    {
                        int columnIndex = 0;

                        foreach (TableColumnModel tableField in field.Children)
                        {
                            columnIndex++;
                            tableField.DisplayOrder = columnIndex;
                        }
                    }
                 
                    EditDocType.Fields.Add(field);
                }

                if (EditDocType.IsOutlook && EditDocType.Id == Guid.Empty)
                {
                    var outlookField = new FieldMetaDataModel
                                           {
                                               DataType = FieldDataType.String,
                                               DisplayOrder = ++index,
                                               IsRequired = true,
                                               Name = Common.OUTLOOK_MAIL_BOBY_FIELD_NAME
                                           };

                    EditDocType.Fields.Add(outlookField);

                    outlookField = new FieldMetaDataModel
                                       {
                                           DataType = FieldDataType.String,
                                           DisplayOrder = ++index,
                                           IsRequired = true,
                                           Name = Common.OUTLOOK_MAIL_FROM_FIELD_NAME
                                       };

                    EditDocType.Fields.Add(outlookField);

                    outlookField = new FieldMetaDataModel
                                       {
                                           DataType = FieldDataType.String,
                                           DisplayOrder = ++index,
                                           IsRequired = true,
                                           Name = Common.OUTLOOK_MAIL_TO_FIELD_NAME
                                       };

                    EditDocType.Fields.Add(outlookField);

                    outlookField = new FieldMetaDataModel
                                       {
                                           DataType = FieldDataType.Date,
                                           DisplayOrder = ++index,
                                           IsRequired = true,
                                           Name = Common.OUTLOOK_MAIL_RECEIVED_DATE_FIELD_NAME
                                       };

                    EditDocType.Fields.Add(outlookField);

                    outlookField = new FieldMetaDataModel
                                       {
                                           DataType = FieldDataType.String,
                                           DisplayOrder = ++index,
                                           IsRequired = false,
                                           Name = Common.OUTLOOK_MAIL_SUBJECT_FIELD_NAME
                                       };

                    EditDocType.Fields.Add(outlookField);

                }

                if (EditDocType.Id == Guid.Empty && (EditDocType.Icon == null || EditDocType.Icon.Length == 0))
                {
                    var image = (Bitmap)_res.GetObject("DefaultIcon");
                    EditDocType.Icon = (byte[])(new ImageConverter()).ConvertTo(image, typeof(byte[]));
                }

                _documentTypeProvier.SaveDocumentType(EditDocType);
            }
            catch (Exception ex)
            {
                e.Result = ex;
            }
        }

        private bool CanDeleteDocType()
        {
            return DocType != null;
        }

        private void DeleteDocType()
        {
            if (DialogService.ShowTwoStateConfirmDialog(_res.GetString("uiConfirmDelete")) == DialogServiceResult.Yes)
            {
                IsProcessing = true;
                var worker = new BackgroundWorker();
                worker.DoWork += DoDeleteDocType;
                worker.RunWorkerCompleted += DoDeleteDocTypeComplete;
                worker.RunWorkerAsync();
            }
        }

        private void DoDeleteDocTypeComplete(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Result is Exception)
            {
                ProcessHelper.ProcessException(e.Result as Exception);
            }
            else
            {
                DocType = null;
                LoadData();
            }
            IsProcessing = false;
        }

        private void DoDeleteDocType(object sender, DoWorkEventArgs e)
        {
            try
            {
                //long docTypeVersionId = _documentTypeProvier.AddDocTypeHistory(DocType);
                //_documentTypeProvier.AddFieldMetadataHistory(DocType);
                //_documentTypeProvier.AddDocumentHistory(DocType, docTypeVersionId);
                _documentTypeProvier.DeleteDocumentType(DocType);
            }
            catch (Exception ex)
            {
                e.Result = ex;
            }
        }

        private void AddDocType()
        {
            _iconFilePath = string.Empty;
            EditDocType = new DocumentTypeModel(CheckDocumentTypeExisted);
            Fields = new ObservableCollection<FieldMetaDataModel>();
            EditPanelVisibled = true;
            DocType = null;
            IconFilePath = null;
        }

        private void LoadFieldView(FieldViewModel viewModel, bool isEdit)
        {
            string editTitle = _resource.GetString("dgEditDialogTitle.Text");
            string addTitle = _resource.GetString("dgAddDialogTitle.Text");
            viewModel.CloseDialog += ViewModelCloseDialog;
            var view = new CreateFieldView(viewModel);

            try
            {
                _dialog = new DialogBaseView(view) { Size = new Size(600, 400), Text = isEdit ? editTitle : addTitle };

                _dialog.ShowDialog();
            }
            catch (Exception ex)
            {
                ProcessHelper.ProcessException(ex);
            }
        }

        private void ViewModelCloseDialog()
        {
            if (_dialog != null)
            {
                _dialog.Close();
            }
        }

        private void OnSaveFieldComplete(FieldMetaDataModel field)
        {
            FieldMetaDataModel editField = null;
            
            if (field.DataType != FieldDataType.Date)
            {
                field.UseCurrentDate = false;
            }

            if (field.Id == Guid.Empty && !FieldViewModel.IsEditMode)
            {
                Fields.Add(field);
            }
            else if (field.Id == Guid.Empty && FieldViewModel.IsEditMode)
            {
                FieldMetaDataModel deleteField = Fields.FirstOrDefault(f => f.IsSelected);
                int index = -1;
                if (deleteField != null)
                {
                    index = Fields.IndexOf(deleteField);
                    Fields.Remove(deleteField);
                }

                if (index == -1)
                {
                    Fields.Add(field);
                }
                else
                {
                    Fields.Insert(index, field);
                }
            }
            else
            {
                editField = Fields.SingleOrDefault(f => f.Id == field.Id);

                if (editField != null)
                {
                    editField.DataType = field.DataType;
                    editField.MaxLength = field.MaxLength;
                    editField.DefaultValue = field.DefaultValue;
                    editField.DeletedMaps = field.DeletedMaps;
                    editField.DisplayOrder = field.DisplayOrder;
                    editField.DocTypeId = field.DocTypeId;
                    editField.Id = field.Id;
                    editField.IsLookup = field.IsLookup;
                    editField.IsRequired = field.IsRequired;
                    editField.IsRestricted = field.IsRestricted;
                    editField.IsSelected = field.IsSelected;
                    editField.IsSystemField = field.IsSystemField;
                    //editField.LookupInfo = field.LookupInfo;
                    editField.Name = field.Name;
                    editField.Picklists = field.Picklists;
                    editField.UseCurrentDate = field.UseCurrentDate;
                    editField.DeletedChildrenIds = field.DeletedChildrenIds;

                    if (field.Children != null)
                    {
                        editField.Children.Clear();

                        foreach (var editChildren in field.Children)
                        {
                            editField.Children.Add(editChildren);
                        }
                    }
                }
            }

            if (_dialog != null)
            {
                _dialog.Close();
            }

            _isDataModified = true;
            OnPropertyChanged("Fields");
        }

        private void LoadData()
        {
            try
            {
                DocType = null;
                EditDocType = null;
                DocumentTypes = _documentTypeProvier.GetDocumentTypes();
            }
            catch (Exception ex)
            {
                ProcessHelper.ProcessException(ex);
            }
        }
        
        private void CheckField(FieldMetaDataModel field)
        {
            if (field.Id == Guid.Empty)
            {
                field.HasErrorWithName = Fields.FirstOrDefault(f => !f.IsSelected && f.Name.Equals(field.Name, StringComparison.CurrentCultureIgnoreCase)) != null;
            }
            else
            {
                field.HasErrorWithName = Fields.FirstOrDefault(f => f.Name.Equals(field.Name, StringComparison.CurrentCultureIgnoreCase) && f.Id != field.Id) != null;
            }
        }

        private void Lookup(object sender)
        {
            var field = sender as FieldMetaDataModel;

            //if (field != null)
            //{
                SelectedField = field;
                bool isEdit = SelectedField.LookupInfo != null && SelectedField.Maps != null;

            //    var lookField = new FieldMetaDataModel
            //    {
            //        Children = SelectedField.Children,
            //        DataType = SelectedField.DataType,
            //        MaxLength = SelectedField.MaxLength,
            //        DefaultValue = SelectedField.DefaultValue,
            //        DisplayOrder = SelectedField.DisplayOrder,
            //        Id = SelectedField.Id,
            //        DocTypeId = SelectedField.DocTypeId,
            //        IsLookup = SelectedField.IsLookup,
            //        IsRequired = SelectedField.IsRequired,
            //        IsRestricted = SelectedField.IsRestricted,
            //        IsSelected = SelectedField.IsSelected,
            //        IsSystemField = SelectedField.IsSystemField,
            //        Name = SelectedField.Name
            //    };

            //    if (isEdit)
            //    {
            //        LookupInfoModel lookup = SelectedField.LookupInfo;
            //        lookField.LookupInfo = new LookupInfoModel
            //        {
            //            ConnectionInfo = lookup.ConnectionInfo,
            //            ConnectionString = lookup.ConnectionString,
            //            //DatabaseName = lookup.DatabaseName,
            //            LookupType = lookup.LookupType,
            //            //DataProvider = lookup.DataProvider,
            //            FieldId = lookup.FieldId,
            //            MaxLookupRow = lookup.MaxLookupRow,
            //            MinPrefixLength = lookup.MinPrefixLength,
            //            //Password = lookup.Password,
            //            //ServerName = lookup.ServerName,
            //            SourceName = lookup.SourceName,
            //            SqlCommand = lookup.SqlCommand,
            //            //Username = lookup.Username,
            //            LookupOperator = lookup.LookupOperator,
            //            LookupColumn = lookup.LookupColumn,
            //            ParameterValue = lookup.ParameterValue
            //        };

            //        lookField.Maps = new ObservableCollection<LookupMapModel>(SelectedField.Maps.ToList());
            //    }
            //    else
            //    {
            //        lookField.LookupInfo = new LookupInfoModel();
            //        lookField.Maps = new ObservableCollection<LookupMapModel>();
            //    }

                LookupViewModel = new LookupViewModel(field, DocType.Fields, isEdit) { IsEditMode = field.IsLookup};//(SaveLookup, DocType.Fields, lookField, isEdit) { IsEditMode = lookField.IsLookup };
                LookupViewModel.CloseDialog += ViewModelCloseDialog;
                LookupViewModel.SaveLookupComplete += SaveLookupCompleted;

                _dialog = new DialogBaseView(new LookupConfigurationView(LookupViewModel)) { Size = new Size(700, 670) };
                var lookupRes = new ResourceManager("Ecm.Admin.LookupConfigurationView", Assembly.GetExecutingAssembly());

                _dialog.Text = lookupRes.GetString("dggDialogTitle");
                _dialog.ShowDialog();
            //}
        }

        private void SaveLookupCompleted(LookupInfoModel lookupInfo)
        {
            //SelectedField = field;
            FieldMetaDataModel editField = Fields.First(f => f.Id == lookupInfo.FieldId);
            editField.LookupInfo = lookupInfo;
            editField.IsLookup = true;

            //if (LookupViewModel.IsEditMode)
            //{
            //    editField.Maps = field.Maps;
            //    editField.LookupInfo = field.LookupInfo;
            //}
            //else
            //{
            //    foreach (var lookupMap in field.Maps)
            //    {
            //        if (lookupMap.Id == Guid.Empty)
            //        {
            //            if (editField.Maps == null)
            //            {
            //                editField.Maps = new ObservableCollection<LookupMapModel>();
            //            }
            //            editField.Maps.Add(lookupMap);
            //        }
            //    }

            //    editField.Children = field.Children;
            //    editField.DataType = field.DataType;
            //    editField.MaxLength = field.MaxLength;
            //    editField.DefaultValue = field.DefaultValue;
            //    editField.DisplayOrder = field.DisplayOrder;
            //    editField.DocTypeId = field.DocTypeId;
            //    editField.ErrorChecked = field.ErrorChecked;
            //    editField.HasErrorWithName = field.HasErrorWithName;
            //    editField.HasErrorWithDefaultValue = field.HasErrorWithDefaultValue;
            //    editField.Id = field.Id;
            //    editField.IsRequired = field.IsRequired;
            //    editField.IsRestricted = field.IsRestricted;
            //    editField.IsSelected = field.IsSelected;
            //    editField.IsSystemField = field.IsSystemField;
            //    editField.Name = field.Name;
                //editField.IsLookup = true;

                //LookupInfoModel info = field.LookupInfo;

                //editField.LookupInfo = new LookupInfoModel
                //{
                //    ConnectionString = info.ConnectionString,
                //    DatabaseName = info.DatabaseName,
                //    LookupType = info.LookupType,
                //    DataProvider = info.DataProvider,
                //    FieldId = info.FieldId,
                //    MaxLookupRow = info.MaxLookupRow,
                //    MinPrefixLength = info.MinPrefixLength,
                //    Password = info.Password,
                //    ServerName = info.ServerName,
                //    SourceName = info.SourceName,
                //    SqlCommand = info.SqlCommand,
                //    Username = info.Username,
                //    LookupColumn = info.LookupColumn,
                //    LookupOperator = info.LookupOperator
                //};

            //}

            if (_dialog != null)
            {
                _dialog.Close();
            }

            _isDataModified = true;
        }

        private void DeleteLookup(object sender)
        {
            var field = (FieldMetaDataModel)sender;
            SelectedField = field;
            SelectedField.LookupInfo = null;
            SelectedField.Maps = null;
            SelectedField.IsLookup = false;
            _isDataModified = true;

            //_documentTypeProvier.DeleteLookupInfo(field.Id);
        }

        private void DocumentTypePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Name" || e.PropertyName == "Icon")
            {
                _isDataModified = true;
            }
        }

        private bool CanLookup(object sender)
        {
            var field = sender as FieldMetaDataModel;

            return DocType != null && field!= null;
        }

        public void CheckDocumentTypeExisted(DocumentTypeModel documentType)
        {
            if (!string.IsNullOrWhiteSpace(documentType.Name))
            {
                documentType.HasError = DocumentTypes.Any(p => p.Name.Equals(documentType.Name, StringComparison.CurrentCultureIgnoreCase) && p.Id != documentType.Id);
            }
        }
    }
}
