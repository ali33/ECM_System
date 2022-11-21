using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ecm.Mvvm;
using Ecm.Model;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Ecm.Model.DataProvider;
using System.IO;

namespace Ecm.CustomAddin.ViewModel
{
    public class MainViewModel : BaseDependencyProperty
    {
        private DocumentTypeModel _documentType;
        private ObservableCollection<DocumentTypeModel> _documentTypes;
        private ObservableCollection<FieldValueModel> _fieldValues = new ObservableCollection<FieldValueModel>();
        private DocumentModel _docModel;
        private PageModel _pageModel; 
        private RelayCommand _saveCommand;
        private RelayCommand _cancelCommand;
        private DocumentTypeProvider _documentTypeProvider = new DocumentTypeProvider();
        private DocumentProvider _documentProvider = new DocumentProvider();

        private string _fileExtension;
        private byte[] _file;
        private string _fileName;
        private bool _allowToEditIndex;

        public MainViewModel(Action closeAction, string fileName, byte[] file, string fileExtension)
        {
            CloseAction = closeAction;
            _file = file;
            _fileName = fileName;
            _fileExtension = fileExtension;
            GetLookupData = LookupData;
            LoadData();
        }

        public MainViewModel(Action closeAction, DocumentModel docModel, PageModel pageModel)
        {
            CloseAction = closeAction;
            _pageModel = pageModel;
            _docModel = docModel;
            GetLookupData = LookupData;
            LoadData();
        }

        private Action CloseAction { get; set; }

        public ObservableCollection<DocumentTypeModel> DocumentTypes
        {
            get { return _documentTypes; }
            set
            {
                _documentTypes = value;
                OnPropertyChanged("DocumentTypes");
            }
        }

        public DocumentTypeModel DocumentType
        {
            get { return _documentType; }
            set
            {
                _documentType = value;
                OnPropertyChanged("DocumentType");
                FieldValues.Clear();
                if (value != null)
                {
                    AllowToEditIndex = value.DocumentTypePermission.AllowedUpdateFieldValue;

                    if (_docModel != null)
                    {
                        if (value.Id == _docModel.DocumentType.Id)
                        {
                            FieldValues = new ObservableCollection<FieldValueModel>(_docModel.FieldValues);
                        }
                        else
                        {
                            AddFieldValues(value.Fields);
                        }
                    }
                    else
                    {
                        AddFieldValues(value.Fields);
                    }
                }
            }
        }

        private void AddFieldValues(ObservableCollection<FieldMetaDataModel> fields)
        {
            foreach (var field in fields)
            {
                if (!field.IsSystemField)
                {
                    FieldValueModel fieldValue = new FieldValueModel();
                    fieldValue.Field = field;
                    FieldValues.Add(fieldValue);
                }
            }
        }

        public bool AllowToEditIndex
        {
            get { return _allowToEditIndex; }
            set
            {
                _allowToEditIndex = value;
                OnPropertyChanged("AllowToEditIndex");
            }
        }

        public ObservableCollection<FieldValueModel> FieldValues
        {
            get { return _fieldValues; }
            set
            {
                _fieldValues = value;
                OnPropertyChanged("FieldValues");
            }
        }

        public ICommand SaveCommand
        {
            get
            {
                if (_saveCommand == null)
                {
                    _saveCommand = new RelayCommand(p => SaveDocument(), p => CanSave());
                }
                return _saveCommand;
            }
        }

        public ICommand CancelCommand
        {
            get
            {
                if (_cancelCommand == null)
                {
                    _cancelCommand = new RelayCommand(p => Cancel());
                }
                return _cancelCommand;
            }
        }

        private void Cancel()
        {
            if (CloseAction != null)
            {
                CloseAction();
            }
        }

        private bool CanSave()
        {
            return DocumentType != null && !FieldValues.Any(f => f.Field.IsRequired && string.IsNullOrEmpty(f.Value) && !f.Field.IsSystemField);
        }

        private void SaveDocument()
        {
            try
            {
                DocumentModel document = new DocumentModel
                {
                    CreatedBy = LoginViewModel.LoginUser.Username,
                    CreatedDate = DateTime.Now,
                    DocumentType = DocumentType,
                    FieldValues = FieldValues.ToList(),
                    PageCount = 1
                };

                PageModel page = new PageModel
                {
                    
                    FileBinaries = _pageModel != null ? _pageModel.FileBinaries : _file,
                    FileExtension = _pageModel != null ? _pageModel.FileExtension : _fileExtension,
                    FileFormat = FileFormatModel.Xls,
                    FileType = FileTypeModel.Native,
                    OriginalFileName = _pageModel != null ? _pageModel.OriginalFileName : _fileName,
                    Content = _pageModel != null ? _pageModel.Content : null,
                    ContentLanguageCode = _pageModel != null ? _pageModel.ContentLanguageCode : null,
                    PageNumber = 1
                };

                page.FileHash = Utility.CryptographyHelper.GenerateFileHash(page.FileBinaries);

                document.Pages = new List<PageModel>();
                document.Pages.Add(page);

                _documentProvider.InsertDocuments(new List<DocumentModel> { document });

                if (CloseAction != null)
                {
                    CloseAction();
                }
            }
            catch (Exception ex)
            {
                ProcessHelper.ProcessException(ex);
            }
        }

        public Func<FieldMetaDataModel, string, System.Data.DataTable> GetLookupData;
        //Private methods

        private void LoadData()
        {
            DocumentTypes = new ObservableCollection<DocumentTypeModel>(_documentTypeProvider.GetDocumentTypes().Where(d => !d.IsOutlook));
        }

        private System.Data.DataTable LookupData(FieldMetaDataModel fieldMetaData, string fieldValue)
        {
            try
            {
                return new LookupProvider().GetLookupData(fieldMetaData.LookupInfo, fieldValue);
            }
            catch (Exception ex)
            {
                ProcessHelper.ProcessException(ex);
            }

            return null;
        }


    }
}
