using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ecm.Mvvm;
using Ecm.Model;
using Ecm.Model.DataProvider;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.ComponentModel;
using System.Data;
using Ecm.Domain;
using Ecm.Audit.View;
using System.Windows.Forms;

namespace Ecm.Audit.ViewModel
{
    public class HistoryViewModel : ComponentViewModel
    {
        #region Private members

        private DocumentTypeModel _selectedDocumentType;
        private DocumentTypeModel _deletedDocumentType;
        private RelayCommand _addSearchConditionCommand;
        private RelayCommand _resetSearchConditionCommand;
        private RelayCommand _advanceSearchCommand;
        private RelayCommand _openDocumentCommand;
        private RelayCommand _openDeletedDocumentCommand;
        private RelayCommand _saveQueryCommand;
        private RelayCommand _deleteQueryCommand;

        private readonly SearchProvider _searchProvider = new SearchProvider();
        private readonly SearchQueryProvider _searchQueryProvider = new SearchQueryProvider();
        private readonly DocumentTypeProvider _documentTypeProvider = new DocumentTypeProvider();
        private readonly DocumentProvider _documentProvider = new DocumentProvider();
        private ObservableCollection<SearchResultModel> _searchResults = new ObservableCollection<SearchResultModel>();
        private ObservableCollection<SearchResultModel> _deletedSearchResults = new ObservableCollection<SearchResultModel>();
        private SearchQueryModel _selectedSearchQuery;


        private bool _enableDelete;

        #endregion

        #region Public properties

        public ObservableCollection<DocumentTypeModel> DocumentTypes { get; private set; }

        public DocumentTypeModel SelectedDocumentType
        {
            get { return _selectedDocumentType; }
            set
            {
                _selectedDocumentType = value;
                OnPropertyChanged("SelectedDocumentType");
                if (value != null)
                {
                    SelectedSearchQuery = null;
                    LoadSavedQueries(value.Id);
                    LoadDefaultSearchExpression();
                    LoadAvailableFields();
                }
            }
        }

        public ObservableCollection<DocumentTypeModel> DeletedDocumentTypes { get; private set; }

        public DocumentTypeModel DeletedDocumentType
        {
            get { return _deletedDocumentType; }
            set
            {
                _deletedDocumentType = value;
                OnPropertyChanged("DeletedDocumentType");
            }
        }

        public SearchQueryModel SelectedSearchQuery
        {
            get { return _selectedSearchQuery; }
            set
            {
                _selectedSearchQuery = value;
                OnPropertyChanged("SelectedSearchQuery");

                if (value != null)
                {
                    LoadSearchExpressionByQuery(value);
                }
            }
        }

        public ObservableCollection<SearchQueryModel> SavedQueries { get; private set; }

        public ObservableCollection<SearchHistoryExpressionViewModel> SearchQueryExpressions { get; private set; }

        public ObservableCollection<FieldMetaDataModel> AvailableFields { get; private set; }

        public ObservableCollection<SearchResultModel> SearchResults
        {
            get { return _searchResults; }
            set
            {
                _searchResults = value;
                OnPropertyChanged("SearchResults");
                if (value != null)
                {
                    foreach(var searchResult in value)
                    {
                        searchResult.PropertyChanged += SearchResultPropertyChanged;
                    }
                }
            }
        }

        public ObservableCollection<SearchResultModel> DeletedSearchResults
        {
            get { return _deletedSearchResults; }
            set
            {
                _deletedSearchResults = value;
                OnPropertyChanged("DeletedSearchResults");
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

        public ICommand AddSearchConditionCommand
        {
            get
            {
                return _addSearchConditionCommand ?? (_addSearchConditionCommand = new RelayCommand(p => AddSearchCondition(), p => CanAddCondition()));
            }
        }

        public ICommand ResetSearchConditionCommand
        {
            get
            {
                return _resetSearchConditionCommand ?? (_resetSearchConditionCommand = new RelayCommand(p => ResetSearchCondition(), p => CanResetSearchCondition()));
            }
        }

        public ICommand AdvanceSearchCommand
        {
            get { return _advanceSearchCommand ?? (_advanceSearchCommand = new RelayCommand(p => RunAdvanceSearch(), p => CanRunAdvanceSearch())); }
        }

        public ICommand OpenDocumentCommand
        {
            get { return _openDocumentCommand ?? (_openDocumentCommand = new RelayCommand(p => OpenDocument())); }
        }

        public ICommand OpenDeletedDocumentCommand
        {
            get { return _openDeletedDocumentCommand ?? (_openDeletedDocumentCommand = new RelayCommand(p => OpenDeletedDocument())); }
        }

        public ICommand SaveQueryCommand
        {
            get
            {
                return _saveQueryCommand ?? (_saveQueryCommand = new RelayCommand(p => SaveQuery(), p => CanSaveQuery()));
            }
        }

        public ICommand DeleteQueryCommand
        {
            get { return _deleteQueryCommand ?? (_deleteQueryCommand = new RelayCommand(p => DeleteQuery(), p => CanDeleteQuery())); }
        }


        #endregion

        #region Public methods

        public HistoryViewModel(MainViewModel mainViewModel)
        {
            SavedQueries = new ObservableCollection<SearchQueryModel>();
            SearchQueryExpressions = new ObservableCollection<SearchHistoryExpressionViewModel>();
            AvailableFields = new ObservableCollection<FieldMetaDataModel>();
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

        public void RunAdvanceSearch()
        {
            IsProcessing = true;
            var worker = new BackgroundWorker();
            worker.DoWork += RunAdvanceSearchDoWork;
            worker.RunWorkerCompleted += RunAdvanceSearchRunWorkerCompleted;
            worker.RunWorkerAsync();
        }

        public void OpenDocument(DataRow searchRow)
        {
            try
            {
                var documentId = (Guid)searchRow[Common.COLUMN_DOCUMENT_ID];
                var existedHistoryDetailViewModel = MainViewModel.HistoryDetailViewModels.FirstOrDefault(p => p.CurrentDocument.Id == documentId);
                if (existedHistoryDetailViewModel != null)
                {
                    existedHistoryDetailViewModel.IsActivated = true;
                }
                else
                {
                    new HistoryDetailViewModel(searchRow, MainViewModel);
                }
            }
            catch (Exception ex)
            {
                ProcessHelper.ProcessException(ex);
            }
        }

        #endregion

        #region Private methods

        private void SaveQuery()
        {
            try
            {
                if (SelectedSearchQuery == null)
                {
                    var selectQueryName = new SearchQueryName();
                    var dialog = new DialogBaseView(selectQueryName) { Text = "Query name", Width = 300, Height = 150 };
                    selectQueryName.Dialog = dialog;
                    selectQueryName.DocumentTypeId = SelectedDocumentType.Id;
                    selectQueryName.QueryNameExisted = _searchQueryProvider.QueryExisted;

                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
                        SelectedSearchQuery = new SearchQueryModel
                        {
                            DocTypeId = SelectedDocumentType.Id,
                            UserId = LoginViewModel.LoginUser.Id,
                            SearchQueryExpressions = GetSearchExpressions(),
                            Name = selectQueryName.QueryName
                        };
                        SavedQueries.Add(SelectedSearchQuery);
                    }
                    else
                    {
                        return;
                    }
                }
                else
                {
                    SelectedSearchQuery.SearchQueryExpressions = GetSearchExpressions();
                    var deletedExpressions = SearchQueryExpressions.Where(p => p.SearchQueryExpression.Id != Guid.Empty &&
                                                                               string.IsNullOrEmpty(p.SearchQueryExpression.Value1) ||
                                                                               (p.SearchQueryExpression.Operator == SearchOperator.InBetween &&
                                                                                string.IsNullOrEmpty(p.SearchQueryExpression.Value2)));
                    SelectedSearchQuery.DeletedExpressions.AddRange(deletedExpressions.Select(p => p.SearchQueryExpression.Id));
                }

                var queryId = _searchQueryProvider.SaveQuery(SelectedSearchQuery);
                LoadSavedQueries(SelectedDocumentType.Id);
                SelectedSearchQuery = SavedQueries.FirstOrDefault(p => p.Id == queryId);
            }
            catch (Exception ex)
            {
                ProcessHelper.ProcessException(ex);
            }
        }

        private void LoadSearchExpressionByQuery(SearchQueryModel value)
        {
            if (value != null)
            {
                SearchQueryExpressions.Clear();
                foreach (var item in value.SearchQueryExpressions)
                {
                    var viewModel = new SearchHistoryExpressionViewModel();
                    var expression = new SearchQueryExpressionModel
                    {
                        Condition = item.Condition,
                        Field = item.Field,
                        Id = item.Id,
                        Operator = item.Operator,
                        SearchQueryId = item.SearchQueryId,
                        Value1 = item.Value1,
                        Value2 = item.Value2
                    };

                    viewModel.SearchQueryExpression = expression;
                    SearchQueryExpressions.Add(viewModel);
                }
            }
        }

        private void LoadSavedQueries(Guid documentTypeId)
        {
            try
            {
                SavedQueries.Clear();
                var queries = _searchQueryProvider.GetSavedQueries(documentTypeId);
                foreach (var query in queries)
                {
                    SavedQueries.Add(query);
                }
            }
            catch (Exception ex)
            {
                ProcessHelper.ProcessException(ex);
            }
        }

        private void LoadDocumentType()
        {
            try
            {
                DocumentTypes = new ObservableCollection<DocumentTypeModel>(_documentTypeProvider.GetDocumentTypes());

                if (DocumentTypes.Count > 0)
                {
                    SelectedDocumentType = DocumentTypes.FirstOrDefault();
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

        private void LoadDefaultSearchExpression()
        {
            SearchQueryExpressions.Clear();
            for (int i = 0; i < SelectedDocumentType.Fields.Count; i++)
            {
                if (SelectedDocumentType.Fields[i].IsSystemField)
                {
                    continue;
                }

                var searchExpressionViewModel = new SearchHistoryExpressionViewModel();
                var expression = new SearchQueryExpressionModel
                {
                    Condition = i == 0 ? SearchConjunction.None : SearchConjunction.And,
                    Field = SelectedDocumentType.Fields[i]
                };

                searchExpressionViewModel.SearchQueryExpression = expression;
                searchExpressionViewModel.SearchQueryExpression.Operator = SearchOperator.Equal;
                SearchQueryExpressions.Add(searchExpressionViewModel);
            }
        }

        private void LoadAvailableFields()
        {
            AvailableFields.Clear();
            foreach (var field in SelectedDocumentType.Fields)
            {
                AvailableFields.Add(field);
            }
        }

        private bool CanAddCondition()
        {
            return SelectedDocumentType != null;
        }

        private void AddSearchCondition()
        {
            var viewModel = new SearchHistoryExpressionViewModel();
            var expression = new SearchQueryExpressionModel { Condition = SearchConjunction.And, Operator = SearchOperator.Equal };

            viewModel.IsAdditionCondition = true;
            viewModel.SearchQueryExpression = expression;
            SearchQueryExpressions.Add(viewModel);
        }

        private bool CanResetSearchCondition()
        {
            return SearchQueryExpressions.Any(p => p.IsAdditionCondition ||
                                                   string.IsNullOrEmpty(p.SearchQueryExpression.Value1) ||
                                                   string.IsNullOrEmpty(p.SearchQueryExpression.Value2));
        }

        private void ResetSearchCondition()
        {
            LoadDefaultSearchExpression();
            SelectedSearchQuery = null;
        }

        private bool CanSaveQuery()
        {
            return SearchQueryExpressions != null && SearchQueryExpressions.Count > 0 &&
                   SearchQueryExpressions.Any(p => !string.IsNullOrEmpty(p.SearchQueryExpression.Value1) || !string.IsNullOrEmpty(p.SearchQueryExpression.Value2));
        }

        private ObservableCollection<SearchQueryExpressionModel> GetSearchExpressions()
        {
            var expressions = new ObservableCollection<SearchQueryExpressionModel>();
            var valuedExpressions = SearchQueryExpressions.Where(p => !string.IsNullOrEmpty(p.SearchQueryExpression.Value1) &&
                                                                       (p.SearchQueryExpression.Operator != SearchOperator.InBetween ||
                                                                        !string.IsNullOrEmpty(p.SearchQueryExpression.Value2)));
            foreach (var item in valuedExpressions)
            {
                SearchQueryExpressionModel searchQueryExpression = new SearchQueryExpressionModel
                {
                    Condition = item.SearchQueryExpression.Condition,
                    Field = item.SearchQueryExpression.Field,
                    Id = item.SearchQueryExpression.Id,
                    Operator = item.SearchQueryExpression.Operator,
                    OperatorText = item.SearchQueryExpression.OperatorText,
                    SearchQueryId = item.SearchQueryExpression.SearchQueryId,
                    Value1 = item.SearchQueryExpression.Value1,
                    Value2 = item.SearchQueryExpression.Value2
                };

                expressions.Add(searchQueryExpression);
            }

            if (expressions.Count > 0)
            {
                expressions[0].Condition = SearchConjunction.None;
            }

            return expressions;
        }

        private bool CanRunAdvanceSearch()
        {
            return SelectedDocumentType != null;
        }

        private void RunAdvanceSearchDoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                SearchResultModel searchResult = _searchProvider.RunAdvanceSearch(0, SelectedDocumentType.Id, new SearchQueryModel { SearchQueryExpressions = GetSearchExpressions() });
                if (searchResult != null)
                {
                    var searchResults = new ObservableCollection<SearchResultModel> { searchResult };
                    e.Result = searchResults;
                }
                else
                {
                    e.Result = null;
                }
            }
            catch (Exception ex)
            {
                e.Result = ex;
            }
        }

        private void RunAdvanceSearchRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
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
                    //SearchDeletedDocuments();
                }
                catch(Exception ex)
                {
                    ProcessHelper.ProcessException(ex);
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
                SearchResultModel searchResult = _searchProvider.SearchDocForDeletedDocType(1, DeletedDocumentType.Id);
                var searchResults = new ObservableCollection<SearchResultModel> { searchResult };
                e.Result = searchResults;

            }
            catch (Exception ex)
            {
                e.Result = ex;
            }
        }

        private void SearchDeletedDocuments()
        {
            SearchResultModel searchResult = _searchProvider.SearchDeletedDocument(SelectedDocumentType.Id);
            if (searchResult != null)
            {
                DeletedSearchResults = new ObservableCollection<SearchResultModel> { searchResult };
            }
            else
            {
                DeletedSearchResults = new ObservableCollection<SearchResultModel>();
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

        private void SearchResultPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            //EnableDelete = true;
            //var selectedDocTypes = (from p in SearchResults where p.IsSelected select p.DocumentType).ToList();
            //foreach(var docType in selectedDocTypes)
            //{
            //    EnableDelete = EnableDelete && docType.DocumentTypePermissions.Any(p => p.AllowedDeletePage);
            //}
        }

        private void DeleteQuery()
        {
            try
            {
                if (DialogService.ShowTwoStateConfirmDialog(@"Are you sure you want to delete the selected query?") == DialogServiceResult.Yes)
                {
                    _searchQueryProvider.DeleteQuery(SelectedSearchQuery.Id);
                    SavedQueries.Remove(SelectedSearchQuery);
                    SelectedSearchQuery = null;

                    LoadDefaultSearchExpression();
                }
            }
            catch (Exception ex)
            {
                ProcessHelper.ProcessException(ex);
            }
        }

        private bool CanDeleteQuery()
        {
            return SelectedSearchQuery != null;
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
                    selectedDocIds.AddRange(selectedRows.Select(searchRow => (Guid)searchRow[Common.COLUMN_DOCUMENT_ID]));
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

    }
}
