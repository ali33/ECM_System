using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Linq;
using Ecm.DocViewer.Model;
using Ecm.Domain;
using Ecm.Model;
using Ecm.Model.DataProvider;
using Ecm.Mvvm;
using Ecm.Utility.Exceptions;
using Ecm.AppHelper;
using System.IO;
using System.Configuration;
using System.Resources;
using System.Reflection;

namespace Ecm.Archive.ViewModel
{
    public class DocumentViewModel : ComponentViewModel
    {
        #region Private members
        private readonly ResourceManager _resource = new ResourceManager("Ecm.Archive.Resources", Assembly.GetExecutingAssembly());

        private readonly DocumentProvider _documentProvider;
        private readonly DocumentTypeProvider _documentTypeProvider;
        private readonly LookupProvider _lookupProvider;
        private bool _isActivated;
        private string _documentName;
        private RelayCommand _closeCommand;
        private readonly Dictionary<PageModel, byte[]> _backupBinaries = new Dictionary<PageModel, byte[]>();
        private bool _isSaveEnabled = true;

        #endregion

        #region Public properties

        public DataRow SearchDataRow { get; private set; }

        public DocumentModel Document { get; private set; }

        public bool IsActivated
        {
            get { return _isActivated; }
            set
            {
                if (MainViewModel.ContentViewModel != this)
                {
                    _isActivated = value;
                }

                OnPropertyChanged("IsActivated");
                if (IsActivated && MainViewModel.ContentViewModel != this)
                {
                    MainViewModel.ContentViewModel = this;
                }
            }
        }

        public string DocumentName
        {
            get { return _documentName; }
            set
            {
                _documentName = value;
                OnPropertyChanged("DocumentName");
            }
        }

        public MainViewModel MainViewModel { get; private set; }

        public RelayCommand CloseCommand
        {
            get { return _closeCommand ?? (_closeCommand = new RelayCommand(p => Close())); }
        }

        public string UserName
        {
            get { return LoginViewModel.LoginUser.Username; }
        }

        public ObservableCollection<BatchTypeModel> BatchTypes { get; private set; }

        public ObservableCollection<DocumentTypeModel> DocumentTypes { get; private set; }

        public ObservableCollection<ContentItem> Items { get; private set; }

        public bool IsSaveEnabled
        {
            get { return _isSaveEnabled; }
            set
            {
                _isSaveEnabled = value;
                OnPropertyChanged("IsSaveEnabled");
            }
        }

        #endregion

        #region Public methods

        public DocumentViewModel(DataRow searchDataRow, MainViewModel mainViewModel)
        {
            _documentTypeProvider = new DocumentTypeProvider();
            _documentProvider = new DocumentProvider();
            _lookupProvider = new LookupProvider();
            DocViewer.ViewerContainer.ViewLink += ViewerContainer_ViewLink;

            SearchDataRow = searchDataRow;
            MainViewModel = mainViewModel;
            Initialize();
            GetDocumentData();
        }

        public DocumentViewModel(Guid docId, MainViewModel mainViewModel)
        {
            _documentTypeProvider = new DocumentTypeProvider();
            _documentProvider = new DocumentProvider();
            _lookupProvider = new LookupProvider();

            MainViewModel = mainViewModel;
            Initialize();
            GetDocumentData(docId);

        }
        private void ViewerContainer_ViewLink(Guid docId)
        {
            //GetDocumentData(docId);
            MainViewModel.OpenLinkDocument(docId);
        }

        public void Close()
        {
            if (SearchDataRow != null)
            {
                SearchDataRow[Common.COLUMN_DOCUMENT] = null;
            }

            var currentIndex = MainViewModel.DocumentViewModels.IndexOf(this);
            if (MainViewModel.DocumentViewModels.Count > 1)
            {
                if (currentIndex == MainViewModel.DocumentViewModels.Count - 1)
                {
                    currentIndex--;
                }
                else
                {
                    currentIndex++;
                }

                MainViewModel.DocumentViewModels[currentIndex].IsActivated = true;
            }

            MainViewModel.DocumentViewModels.Remove(this);
        }

        public void Save()
        {
            IsProcessing = true;
            IsSaveEnabled = false;

            var documents = new List<DocumentModel>();
            foreach (var docItem in Items[0].Children)
            {
                documents.Add(docItem.DocumentData);
                docItem.DocumentData.Pages = new List<PageModel>();
                docItem.DocumentData.PageCount = docItem.Children.Count;
                docItem.DocumentData.DeletedPages = new List<Guid>();
                foreach (var deletedPageItem in docItem.DeletedPages)
                {
                    docItem.DocumentData.DeletedPages.Add(deletedPageItem.PageData.Id);
                }

                foreach (var pageItem in docItem.Children)
                {
                    if (pageItem.PageData.Id == Guid.Empty ||
                        (pageItem.ChangeType & ChangeType.ReplacePage) == ChangeType.ReplacePage)
                    {
                        pageItem.PageData.FileBinaries = pageItem.GetBinary(DocViewerMode.Document);
                    }
                    else
                    {
                        _backupBinaries.Add(pageItem.PageData, pageItem.PageData.FileBinaries);
                        pageItem.PageData.FileBinaries = null;
                    }

                    pageItem.PageData.PageNumber = docItem.Children.IndexOf(pageItem);
                    docItem.DocumentData.Pages.Add(pageItem.PageData);
                }
            }

            var saveWorker = new BackgroundWorker();
            saveWorker.DoWork += SaveWorkerDoWork;
            saveWorker.RunWorkerCompleted += SaveWorkerRunWorkerCompleted;
            saveWorker.RunWorkerAsync(documents);
        }

        public DataTable GetLookupData(FieldMetaDataModel fieldMetaData, string fieldValue)
        {
            return _lookupProvider.GetLookupData(fieldMetaData.LookupInfo, fieldValue);
        }

        public void DeleteDocument()
        {
            try
            {
                //if (DialogService.ShowTwoStateConfirmDialog("Are you sure you want to delete the opening document?") == DialogServiceResult.Yes)
                //{
                    IsProcessing = true;
                    IsSaveEnabled = false;

                    var deleteWorker = new BackgroundWorker();
                    deleteWorker.DoWork += DeleteWorkerDoWork;
                    deleteWorker.RunWorkerCompleted += DeleteWorkerRunWorkerCompleted;
                    deleteWorker.RunWorkerAsync();
                //}
            }
            catch (Exception ex)
            {
                ProcessHelper.ProcessException(ex);
            }
        }

        #endregion

        #region Private methods

        private new void Initialize()
        {
            DocumentTypes = new ObservableCollection<DocumentTypeModel>(_documentTypeProvider.GetDocumentTypes());
            BatchTypes = new ObservableCollection<BatchTypeModel>
                             {
                                 new BatchTypeModel
                                     {
                                         Id = Guid.Empty, 
                                         Name = _resource.GetString("uiDefaultBatch"), 
                                         DocumentTypes = DocumentTypes
                                     }
                             };
            Items = new ObservableCollection<ContentItem>
                        {
                            new ContentItem(new BatchModel(Guid.Empty, DateTime.Now, UserName, BatchTypes[0]))
                        };
        }

        private void GetDocumentData()
        {
            Document = SearchDataRow[Common.COLUMN_DOCUMENT] as DocumentModel;
            if (Document == null)
            {
                IsProcessing = true;
                IsSaveEnabled = false;

                var documentId = (Guid)SearchDataRow[Common.COLUMN_DOCUMENT_ID];
                var dataRetrieveWorker = new BackgroundWorker();
                dataRetrieveWorker.DoWork += DataRetrieveWorkerDoWork;
                dataRetrieveWorker.RunWorkerCompleted += DataRetrieveWorkerRunWorkerCompleted;
                dataRetrieveWorker.RunWorkerAsync(documentId);
            }
            else
            {
                LoadDocument();
            }
        }

        private void GetDocumentData(Guid docId)
        {
            IsProcessing = true;
            IsSaveEnabled = false;

            var openLink = new BackgroundWorker();
            openLink.DoWork += OpenLinkWorkerDoWork;
            openLink.RunWorkerCompleted += OpenLinkWorkerRunWorkerCompleted;
            openLink.RunWorkerAsync(docId);
        }

        private void OpenLinkWorkerRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            IsProcessing = false;
            IsSaveEnabled = true;

            if (e.Result is Exception)
            {
                ProcessHelper.ProcessException(e.Result as Exception);
            }
            else
            {
                LoadDocument();
            }
        }

        private void OpenLinkWorkerDoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                Document = _documentProvider.GetDocument((Guid)e.Argument);

                if (Document == null)
                {
                    e.Result = new WcfException("Document not found.");
                }
            }
            catch (Exception ex)
            {
                e.Result = ex;
            }
        }

        private void DataRetrieveWorkerDoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                Document = _documentProvider.GetDocument((Guid)e.Argument);

                if (Document == null)
                {
                    e.Result = new WcfException("Document not found.");
                }
            }
            catch (Exception ex)
            {
                e.Result = ex;
            }
        }

        private void DataRetrieveWorkerRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            IsProcessing = false;
            IsSaveEnabled = true;

            if (e.Result is Exception)
            {
                ProcessHelper.ProcessException(e.Result as Exception);
            }
            else
            {
                SearchDataRow[Common.COLUMN_DOCUMENT] = Document;
                LoadDocument();
            }
        }

        private void LoadDocument()
        {
            DocumentName = GetDocumentName();
            IsActivated = true;

            if (Document.DocumentType.IsOutlook)
            {
                WriteOutlookPictures();
            }

            MainViewModel.DocumentViewModels.Add(this);
            Document.DocumentType = BatchTypes[0].DocumentTypes.First(p => p.Id == Document.DocumentType.Id);
            foreach (var field in Document.DocumentType.Fields)
            {
                var fieldValue = Document.FieldValues.FirstOrDefault(p => p.Field.Id == field.Id);
                if (fieldValue == null)
                {
                    Document.FieldValues.Add(new FieldValueModel { Field = field, Value = string.Empty });
                }
            }

            var docItem = new ContentItem(Document);
            Items[0].Children.Add(docItem);
            foreach (var page in Document.Pages)
            {
                var pageItem = new ContentItem(page, page.FileBinaries);
                docItem.Children.Add(pageItem);
            }
        }

        private void WriteOutlookPictures()
        {
            string outlookTemp = ConfigurationManager.AppSettings["OutlookTempDir"];
            string tempPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            tempPath = Path.Combine(tempPath, outlookTemp);

            if (!Directory.Exists(tempPath))
            {
                Directory.CreateDirectory(tempPath);
            }

            foreach (KeyValuePair<string, byte[]> file in Document.EmbeddedPictures)
            {
                string fileName = Path.Combine(tempPath, file.Key);
                File.WriteAllBytes(fileName, file.Value);
            }
        }

        private string GetDocumentName()
        {
            return Document.DocumentType.Name + string.Format(" [{0}]", Document.Id);
        }

        private void SaveWorkerDoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                IList<DocumentModel> documentInfos = _documentProvider.UpdateDocuments(e.Argument as List<DocumentModel>);
                e.Result = documentInfos;
            }
            catch (Exception ex)
            {
                e.Result = ex;
            }
        }

        private void SaveWorkerRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            try
            {
                IsProcessing = false;
                IsSaveEnabled = true;

                if (e.Result is Exception)
                {
                    ProcessHelper.ProcessException(e.Result as Exception);
                }
                else
                {
                    var documentInfos = e.Result as IList<DocumentModel>;
                    if (documentInfos != null)
                    {
                        Document.Version = documentInfos[0].Version;
                        Document.ModifiedDate = documentInfos[0].ModifiedDate;
                    }

                    UpdateSearchRow();
                    Items[0].ResetStatus();
                    Close();
                }
            }
            finally
            {
                // Restore the binary for page
                foreach (PageModel page in _backupBinaries.Keys)
                {
                    page.FileBinaries = _backupBinaries[page];
                }

                _backupBinaries.Clear();
            }
        }

        private void UpdateSearchRow()
        {
            SearchDataRow[Common.COLUMN_PAGE_COUNT] = Document.PageCount;
            SearchDataRow[Common.COLUMN_MODIFIED_BY] = Document.ModifiedBy ?? LoginViewModel.LoginUser.Username;
            SearchDataRow[Common.COLUMN_MODIFIED_ON] = Document.ModifiedDate ?? DateTime.Now;
            SearchDataRow[Common.COLUMN_VERSION] = Document.Version;

            foreach (var fieldValue in Document.FieldValues)
            {
                if (fieldValue.Field.IsSystemField || string.IsNullOrEmpty(fieldValue.Value) || fieldValue.Field.DataType == FieldDataType.Table)
                {
                    continue;
                }

                SearchDataRow[fieldValue.Field.Name] = SearchProvider.ConvertData(fieldValue.Value, fieldValue.Field.DataType);
            }
        }

        private void DeleteWorkerDoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                _documentProvider.DeleteDocument(Document.Id);
            }
            catch (Exception ex)
            {
                e.Result = ex;
            }
        }

        private void DeleteWorkerRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            IsProcessing = false;
            IsSaveEnabled = true;

            if (e.Result is Exception)
            {
                ProcessHelper.ProcessException(e.Result as Exception);
            }
            else
            {
                SearchDataRow.Table.Rows.Remove(SearchDataRow);
                MainViewModel.DocumentViewModels.Remove(this);
            }
        }

        #endregion

        public bool IsRefresh { get; set; }
    }
}