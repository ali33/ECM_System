using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ecm.Mvvm;
using Ecm.Model;
using System.Collections.ObjectModel;
using Ecm.Model.DataProvider;
using System.Windows.Input;
using System.ComponentModel;
using System.Data;
using Ecm.Domain;

namespace Ecm.Audit.ViewModel
{
    public class DeletedDocumentHistoryViewModel : ComponentViewModel
    {
        #region Private members

        private DocumentTypeModel _selectedDocumentType;
        private RelayCommand _openDocumentCommand;
        private ObservableCollection<SearchResultModel> _searchResults = new ObservableCollection<SearchResultModel>();
        private readonly DocumentTypeProvider _documentTypeProvider = new DocumentTypeProvider();
        private readonly DocumentProvider _documentProvider = new DocumentProvider();
        private readonly SearchProvider _searchProvider = new SearchProvider();
        private bool _enableDelete;

        #endregion

        #region Public properties

        public ObservableCollection<DocumentTypeModel> DocumentTypes { get; private set; }

        public ObservableCollection<DocumentTypeModel> ExistingDocumentTypes { get; private set; }

        public DocumentTypeModel SelectedDocumentType
        {
            get { return _selectedDocumentType; }
            set
            {
                _selectedDocumentType = value;
                OnPropertyChanged("SelectedDocumentType");
            }
        }

        public ObservableCollection<SearchResultModel> SearchResults
        {
            get { return _searchResults; }
            set
            {
                _searchResults = value;
                OnPropertyChanged("SearchResults");
                //if (value != null)
                //{
                //    foreach(var searchResult in value)
                //    {
                //        searchResult.PropertyChanged += SearchResultPropertyChanged;
                //    }
                //}
            }
        }

        public MainViewModel MainViewModel { get; private set; }

        public bool EnableDelete
        {
            get { return _enableDelete; }
            set 
            { 
                _enableDelete = value;
                OnPropertyChanged("EnableDelete");
            }
        }

        public ICommand OpenDocumentCommand
        {
            get { return _openDocumentCommand ?? (_openDocumentCommand = new RelayCommand(p => OpenDocument())); }
        }

        #endregion

        #region Public methods

        public DeletedDocumentHistoryViewModel(MainViewModel mainViewModel)
        {
            MainViewModel = mainViewModel;
            Initialize();
        }

        public void SearchDocForDeletedDocType()
        {
            IsProcessing = true;
            var worker = new BackgroundWorker();
            worker.DoWork += new DoWorkEventHandler(DoSearchDocForDeletedDocType);
            worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(DoSearchDocForDeletedDocTypeCompleted);
            worker.RunWorkerAsync();
        }

        public void SearchDeletedDocumentFromExistingDocType()
        {
            IsProcessing = true;
            var worker = new BackgroundWorker();
            worker.DoWork += new DoWorkEventHandler(DoSearchDeletedDocumentFromExistingDocType);
            worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(DoSearchDeletedDocumentFromExistingDocTypeCompleted);
            worker.RunWorkerAsync();
        }

        public void OpenDocument(DataRow searchRow)
        {
            try
            {
                Guid documentId = Guid.Empty;
                if (!string.IsNullOrEmpty(searchRow[Common.COLUMN_DOCUMENT_VERSION_ID].ToString()))
                {
                    documentId = (Guid)searchRow[Common.COLUMN_DOCUMENT_VERSION_ID];
                }
                else
                {
                    documentId = (Guid)searchRow[Common.COLUMN_DOCUMENT_ID];
                }

                var existedHistoryDetailViewModel = MainViewModel.HistoryDetailViewModels.FirstOrDefault(p => p.Document.Id == documentId);
                if (existedHistoryDetailViewModel != null)
                {
                    existedHistoryDetailViewModel.IsActivated = true;
                }
                else
                {
                    new HistoryDetailViewModel(searchRow, MainViewModel, Status.Deleted);
                }
            }
            catch (Exception ex)
            {
                ProcessHelper.ProcessException(ex);
            }
        }
        public List<DocumentModel> GetSelectedDocuments()
        {
            try
            {
                var docIds = GetSelectedDocumentIds();
                return _documentProvider.GetDocuments(docIds);
            }
            catch (Exception ex)
            {
                ProcessHelper.ProcessException(ex);
            }

            return null;
        }

        private List<Guid> GetSelectedDocumentIds()
        {
            try
            {
                var selectedRows = new List<DataRow>();
                foreach (var searchResult in SearchResults)
                {
                    selectedRows.AddRange(searchResult.DataResult.Rows.Cast<DataRow>().Where(selectedRow => (bool)selectedRow[Common.COLUMN_SELECTED]));
                }

                var selectedDocIds = new List<Guid>();
                if (selectedRows.Count > 0)
                {
                    selectedDocIds.AddRange(selectedRows.Select(searchRow => (Guid)searchRow[Common.COLUMN_DOCUMENT_VERSION_ID]));
                }

                return selectedDocIds;
            }
            catch (Exception ex)
            {
                ProcessHelper.ProcessException(ex);
            }

            return null;
        }

        #endregion

        #region Private methods

        private void LoadDocumentType()
        {
            try
            {
                DocumentTypes = new ObservableCollection<DocumentTypeModel>(_documentTypeProvider.GetDeletedDocumentTypes());
                ExistingDocumentTypes = new ObservableCollection<DocumentTypeModel>(_documentTypeProvider.GetDocumentTypes());

                if (DocumentTypes.Count > 0)
                {
                    SelectedDocumentType = DocumentTypes[0];
                }
            }
            catch (Exception ex)
            {
                ProcessHelper.ProcessException(ex);
            }
        }

        private new void Initialize()
        {
            LoadDocumentType();
        }

        private void SearchDeletedDocuments()
        {
            SearchResultModel searchResult = _searchProvider.SearchDeletedDocument(SelectedDocumentType.Id);
            if (searchResult != null)
            {
                SearchResults = new ObservableCollection<SearchResultModel> { searchResult };
            }
            else
            {
                SearchResults = new ObservableCollection<SearchResultModel>();
            }
        }

        private void OpenDeletedDocument()
        {
            var selectedRows = new List<DataRow>();
            foreach (var searchResult in SearchResults)
            {
                selectedRows.AddRange(searchResult.DataResult.Rows.Cast<DataRow>().Where(selectedRow => (bool)selectedRow[Common.COLUMN_SELECTED]));
            }

            if (selectedRows.Count > 0)
            {
                foreach (var selectedRow in selectedRows)
                {
                    OpenDocument(selectedRow);
                }
            }
        }

        private void OpenDocument()
        {
            var selectedRows = new List<DataRow>();
            foreach (var searchResult in SearchResults)
            {
                selectedRows.AddRange(searchResult.DataResult.Rows.Cast<DataRow>().Where(selectedRow => (bool)selectedRow[Common.COLUMN_SELECTED]));
            }

            if (selectedRows.Count > 0)
            {
                foreach (var selectedRow in selectedRows)
                {
                    OpenDocument(selectedRow);
                }
            }
        }

        void DoSearchDocForDeletedDocTypeCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            IsProcessing = false;
            if (e.Result is Exception)
            {
                ProcessHelper.ProcessException(e.Result as Exception);
            }
            else
            {
                try
                {
                    SearchResults = e.Result as ObservableCollection<SearchResultModel>;
                }
                catch (Exception ex)
                {
                    ProcessHelper.ProcessException(ex);
                }
            }
        }

        void DoSearchDocForDeletedDocType(object sender, DoWorkEventArgs e)
        {
            try
            {
                SearchResultModel searchResult = _searchProvider.SearchDocForDeletedDocType(1, SelectedDocumentType.Id);
                var searchResults = new ObservableCollection<SearchResultModel> { searchResult };
                e.Result = searchResults;

            }
            catch (Exception ex)
            {
                e.Result = ex;
            }
        }

        void DoSearchDeletedDocumentFromExistingDocTypeCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            IsProcessing = false;
            if (e.Result is Exception)
            {
                ProcessHelper.ProcessException(e.Result as Exception);
            }
            else
            {
                try
                {
                    SearchResults = e.Result as ObservableCollection<SearchResultModel>;
                }
                catch (Exception ex)
                {
                    ProcessHelper.ProcessException(ex);
                }
            }
        }

        void DoSearchDeletedDocumentFromExistingDocType(object sender, DoWorkEventArgs e)
        {
            try
            {
                IsProcessing = true;
                SearchResultModel searchResult = _searchProvider.SearchDeletedDocument(SelectedDocumentType.Id);
                var searchResults = new ObservableCollection<SearchResultModel> { searchResult };
                e.Result = searchResults;
            }
            catch (Exception ex)
            {
                e.Result = ex;
            }
        }

        #endregion
    }
}
