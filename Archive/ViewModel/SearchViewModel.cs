using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Forms;
using Ecm.Archive.View;
using Ecm.Model.DataProvider;
using Ecm.Mvvm;
using Ecm.Model;
using System.Windows.Input;
using System.Collections.ObjectModel;
using Ecm.Domain;
using System.ComponentModel;
using System.Resources;
using System.Reflection;
using Ecm.DocViewer.Model;
using Ecm.DocViewer.Helper;
using Ecm.AppHelper;
using Ecm.DocViewer.Controls;

namespace Ecm.Archive.ViewModel
{
    public class SearchViewModel : ComponentViewModel
    {
        #region Private members
        public const string _fieldContentId = "Content_CFCCCDF6_84CC_43ED_BE4A_617974473143";

        private SearchQueryModel _selectedSearchQuery;
        private DocumentTypeModel _selectedDocumentType;
        private RelayCommand _addSearchConditionCommand;
        private RelayCommand _resetSearchConditionCommand;
        private RelayCommand _deleteQueryCommand;
        private RelayCommand _saveQueryCommand;
        private RelayCommand _advanceSearchCommand;
        private RelayCommand _openDocumentCommand;
        private RelayCommand _deleteDocumentCommand;
        private RelayCommand _contentSearchCommand;
        private RelayCommand _removeAdditionalField;
        private RelayCommand _downloadCommand;
        private RelayCommand _sendMailCommand;
        private RelayCommand _printCommand;

        private readonly SearchProvider _searchProvider = new SearchProvider();
        private readonly SearchQueryProvider _searchQueryProvider = new SearchQueryProvider();
        private readonly DocumentTypeProvider _documentTypeProvider = new DocumentTypeProvider();
        private readonly DocumentProvider _documentProvider = new DocumentProvider();
        private readonly ActionLogProvider _actionLogProvider = new ActionLogProvider();
        private ObservableCollection<SearchResultModel> _searchResults = new ObservableCollection<SearchResultModel>();
        private bool _isSearchEnabled = true;
        private bool _enableDelete;
        private bool _enablePrint;
        private bool _enableEmail;
        private ResourceManager _resource = new ResourceManager("Ecm.Archive.Resources", Assembly.GetExecutingAssembly());
        #endregion

        #region Public properties

        public bool IsSearchEnabled
        {
            get { return _isSearchEnabled; }
            set
            {
                _isSearchEnabled = value;
                OnPropertyChanged("IsSearchEnabled");
            }
        }

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
                    SearchResults = null;
                }
            }
        }

        public ObservableCollection<SearchQueryModel> SavedQueries { get; private set; }

        public SearchQueryModel SelectedSearchQuery
        {
            get { return _selectedSearchQuery; }
            set
            {
                _selectedSearchQuery = value;
                OnPropertyChanged("SelectedSearchQuery");

                if (value != null)
                {
                    if (value.Id == Guid.Empty)
                    {
                        LoadDefaultSearchExpression();
                    }
                    else
                    {
                        LoadSearchExpressionByQuery(value);
                    }
                }
            }
        }

        public ObservableCollection<SearchExpressionViewModel> SearchQueryExpressions { get; private set; }

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

        public bool EnablePrint
        {
            get { return _enablePrint; }
            set
            {
                _enablePrint = value;
                OnPropertyChanged("EnablePrint");
            }
        }

        public bool EnableEmail
        {
            get { return _enableEmail; }
            set
            {
                _enableEmail = value;
                OnPropertyChanged("EnableEmail");
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

        public ICommand DeleteQueryCommand
        {
            get { return _deleteQueryCommand ?? (_deleteQueryCommand = new RelayCommand(p => DeleteQuery(), p => CanDeleteQuery())); }
        }

        public ICommand SaveQueryCommand
        {
            get
            {
                return _saveQueryCommand ?? (_saveQueryCommand = new RelayCommand(p => SaveQuery(), p => CanSaveQuery()));
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

        public ICommand DeleteDocumentCommand
        {
            get { return _deleteDocumentCommand ?? (_deleteDocumentCommand = new RelayCommand(p => DeleteDocument())); }
        }

        public ICommand ContentSearchCommand
        {
            get
            {
                if (_contentSearchCommand == null)
                {
                    _contentSearchCommand = new RelayCommand(p => RunSearchContent());
                }

                return _contentSearchCommand;
            }
        }

        public ICommand RemoveAdditionalField
        {
            get
            {
                if (_removeAdditionalField == null)
                {
                    _removeAdditionalField = new RelayCommand(p => RemoveAdditionalCondition(p));
                }

                return _removeAdditionalField;
            }
        }

        public ICommand DownloadCommand
        {
            get
            {
                if (_downloadCommand == null)
                {
                    _downloadCommand = new RelayCommand(p => DownloadDocument());
                }

                return _downloadCommand;
            }
        }

        public ICommand SendMailCommand
        {
            get
            {
                if (_sendMailCommand == null)
                {
                    _sendMailCommand = new RelayCommand(p => SendMailDocument());
                }

                return _sendMailCommand;
            }
        }

        public ICommand PrintCommand
        {
            get
            {
                if (_printCommand == null)
                {
                    _printCommand = new RelayCommand(p => PrintDocument());
                }

                return _printCommand;
            }
        }
        #endregion

        #region Public methods

        public SearchViewModel(MainViewModel mainViewModel)
        {
            SavedQueries = new ObservableCollection<SearchQueryModel>();
            SearchQueryExpressions = new ObservableCollection<SearchExpressionViewModel>();
            AvailableFields = new ObservableCollection<FieldMetaDataModel>();
            MainViewModel = mainViewModel;

            Initialize();
        }

        public void RunAdvanceSearch()
        {
            IsProcessing = true;
            IsSearchEnabled = false;

            var worker = new BackgroundWorker();
            worker.DoWork += RunAdvanceSearchDoWork;
            worker.RunWorkerCompleted += RunAdvanceSearchRunWorkerCompleted;
            worker.RunWorkerAsync();
        }

        public void RunGlobalSearch(string text)
        {
            IsProcessing = true;
            IsSearchEnabled = false;

            var worker = new BackgroundWorker();
            worker.DoWork += RunGlobalSearchDoWork;
            worker.RunWorkerCompleted += RunGlobalSearchRunWorkerCompleted;
            worker.RunWorkerAsync(text);
        }

        public void OpenDocument(DataRow searchRow)
        {
            IsProcessing = true;
            IsSearchEnabled = false;

            var worker = new BackgroundWorker();
            worker.DoWork += DoOpenDocument;
            worker.RunWorkerCompleted += DoOpenDocumentCompleted;
            worker.RunWorkerAsync(searchRow);
        }

        public void DownloadDocument(DataRow searchRow)
        {
            IsProcessing = true;
            IsSearchEnabled = false;

            var worker = new BackgroundWorker();
            worker.DoWork += DoGetDocument;
            worker.RunWorkerCompleted += DoDownloadDocumentCompleted;
            worker.RunWorkerAsync(searchRow);
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

        public void LoadMoreGlobalSearchResults()
        {
            IsProcessing = true;
            IsSearchEnabled = false;

            var worker = new BackgroundWorker();
            worker.DoWork += LoadMoreGlobalSearchDoWork;
            worker.RunWorkerCompleted += LoadMoreGlobalSearchRunWorkerCompleted;
            worker.RunWorkerAsync(SearchResults[SearchResults.Count - 1]);
        }


        #endregion

        #region Private methods

        private new void Initialize()
        {
            DocumentTypes = new ObservableCollection<DocumentTypeModel>(_documentTypeProvider.GetDocumentTypes());
            if (DocumentTypes.Count > 0)
            {
                SelectedDocumentType = DocumentTypes[0];
            }
        }

        private void RunSearchContent()
        {
            ContentSearchViewModel searchContentViewModel = new ContentSearchViewModel(DoContentSearch, SelectedDocumentType);
            ContentSearchView contentView = new ContentSearchView(searchContentViewModel);
            DialogBaseView dialog = new DialogBaseView(contentView);
            dialog.Width = 600;
            dialog.Height = 300;
            dialog.Text = _resource.GetString("uiContentDialogTitle");
            searchContentViewModel.CloseDialog += () =>
            {
                dialog.Close();
            };

            dialog.ShowDialog();
        }

        private void DoContentSearch(string content)
        {
            IsProcessing = true;
            IsSearchEnabled = false;

            BackgroundWorker worker = new BackgroundWorker();
            worker.DoWork += new DoWorkEventHandler(ContentSearch_DoWork);
            worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(ContentSearch_RunWorkerCompleted);
            worker.RunWorkerAsync(content);
        }

        void ContentSearch_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            IsProcessing = false;
            IsSearchEnabled = true;

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

        void ContentSearch_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                string content = (string)e.Argument;
                SearchResultModel searchResult = _searchProvider.RunContentSearch(0, SelectedDocumentType.Id, content);
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

        private void LoadSavedQueries(Guid documentTypeId)
        {
            try
            {
                SavedQueries.Clear();
                var queries = _searchQueryProvider.GetSavedQueries(documentTypeId);
                SavedQueries.Add(new SearchQueryModel { Name = _resource.GetString("uiSelect"), Id= Guid.Empty });

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

        private void LoadDefaultSearchExpression()
        {
            SearchQueryExpressions.Clear();

            int count = SelectedDocumentType.Fields.Count;
            for (int i = 0; i < count; i++)
            {
                if (!SelectedDocumentType.Fields[i].IsSystemField && SelectedDocumentType.Fields[i].DataType != FieldDataType.Table)
                {
                    var searchExpressionViewModel = new SearchExpressionViewModel();
                    var expression = new SearchQueryExpressionModel
                                         {
                                             Condition = i == 0 ? SearchConjunction.None : SearchConjunction.And,
                                             Field = SelectedDocumentType.Fields[i]
                                         };

                    searchExpressionViewModel.SearchQueryExpression = expression;
                    SearchQueryExpressions.Add(searchExpressionViewModel);
                }
            }
        }

        private void LoadSearchExpressionByQuery(SearchQueryModel value)
        {
            if (value != null)
            {
                LoadDefaultSearchExpression();
                foreach (var item in value.SearchQueryExpressions)
                {
                    var viewModel = new SearchExpressionViewModel();

                    if (item.Field == null)
                    {
                        var field = AvailableFields.SingleOrDefault(p => p.FieldUniqueId == item.FieldUniqueId);                        
                        var expression = new SearchQueryExpressionModel
                        {
                            Condition = item.Condition,
                            Field = field,
                            Id = item.Id,
                            Operator = item.Operator,
                            SearchQueryId = item.SearchQueryId,
                            Value1 = item.Value1,
                            Value2 = item.Value2
                        };

                        viewModel.SearchQueryExpression = expression;
                        SearchQueryExpressions.Add(viewModel);
                    }
                    else
                    {
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

                        var existing = SearchQueryExpressions.SingleOrDefault(p => p.SearchQueryExpression.Field.Id == expression.Field.Id);

                        if (existing != null)
                        {
                            existing.SearchQueryExpression = expression;
                        }
                    }
                }
            }
        }

        private void LoadAvailableFields()
        {
            AvailableFields.Clear();
            foreach (var field in SelectedDocumentType.Fields.Where(p => !p.IsSystemField && p.DataType != FieldDataType.Table))
            {
                AvailableFields.Add(field);
            }

            FieldMetaDataModel fieldModel = new FieldMetaDataModel
            {
                Id = Guid.Empty,
                Name = "Created by",
                DataType = FieldDataType.String,
                FieldUniqueId = Common.DOCUMENT_CREATED_BY
            };

            AvailableFields.Add(fieldModel);

            fieldModel = new FieldMetaDataModel
            {
                Id = Guid.Empty,
                Name = "Created date",
                DataType = FieldDataType.Date,
                FieldUniqueId = Common.DOCUMENT_CREATED_DATE
            };

            AvailableFields.Add(fieldModel);

            fieldModel = new FieldMetaDataModel
            {
                Id = Guid.Empty,
                Name = "Document Id",
                DataType = FieldDataType.String,
                FieldUniqueId = Common.DOCUMENT_ID
            };

            AvailableFields.Add(fieldModel);

            fieldModel = new FieldMetaDataModel
            {
                Id = Guid.Empty,
                Name = "Modified by",
                DataType = FieldDataType.String,
                FieldUniqueId = Common.DOCUMENT_MODIFIED_BY
            };

            AvailableFields.Add(fieldModel);

            fieldModel = new FieldMetaDataModel
            {
                Id = Guid.Empty,
                Name = "Modified date",
                DataType = FieldDataType.Date,
                FieldUniqueId = Common.DOCUMENT_MODIFIED_DATE
            };

            AvailableFields.Add(fieldModel);

            fieldModel = new FieldMetaDataModel
            {
                Id = Guid.Empty,
                Name = "Page count",
                DataType = FieldDataType.Integer,
                FieldUniqueId = Common.DOCUMENT_PAGE_COUNT
            };

            AvailableFields.Add(fieldModel);

            fieldModel = new FieldMetaDataModel
            {
                Id = Guid.Empty,
                Name = "Binary type",
                DataType = FieldDataType.String,
                FieldUniqueId = Common.DOCUMENT_FILE_BINARY
            };

            AvailableFields.Add(fieldModel);

            fieldModel = new FieldMetaDataModel
            {
                Id = Guid.Empty,
                Name = "Version",
                DataType = FieldDataType.Integer,
                FieldUniqueId = Common.DOCUMENT_VERSION
            };

            AvailableFields.Add(fieldModel);

        }

        private bool CanAddCondition()
        {
            return SelectedDocumentType != null;
        }

        private void AddSearchCondition()
        {
            var viewModel = new SearchExpressionViewModel();
            var expression = new SearchQueryExpressionModel { Condition = SearchConjunction.And, Operator = SearchOperator.Equal };

            if (SelectedSearchQuery != null)
            {
                expression.SearchQueryId = SelectedSearchQuery.Id;
            }

            viewModel.IsAdditionCondition = true;
            viewModel.SearchQueryExpression = expression;
            SearchQueryExpressions.Add(viewModel);
        }

        private void RemoveAdditionalCondition(object para)
        {
            var searchExpressionViewModel = para as SearchExpressionViewModel;
            SearchQueryExpressions.Remove(searchExpressionViewModel);
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

        private void SaveQuery()
        {
            try
            {
                if (SelectedSearchQuery == null || SelectedSearchQuery.Id == Guid.Empty)
                {
                    var selectQueryName = new SearchQueryName();
                    var dialog = new DialogBaseView(selectQueryName) { Text = _resource.GetString("tbQueryName"), Width = 300, Height = 150 };
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

        private bool CanDeleteQuery()
        {
            return SelectedSearchQuery != null;
        }

        private void DeleteQuery()
        {
            try
            {
                if (DialogService.ShowTwoStateConfirmDialog(_resource.GetString("uiConfirmDeleteQuery")) == DialogServiceResult.Yes)
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

        private ObservableCollection<SearchQueryExpressionModel> GetSearchExpressions()
        {
            var expressions = new ObservableCollection<SearchQueryExpressionModel>();
            var valuedExpressions = SearchQueryExpressions.Where(p => !string.IsNullOrEmpty(p.SearchQueryExpression.Value1) &&
                                                                       (p.SearchQueryExpression.Operator != SearchOperator.InBetween ||
                                                                        !string.IsNullOrEmpty(p.SearchQueryExpression.Value2)));
            foreach (var item in valuedExpressions)
            {
                var searchQueryExpression = new SearchQueryExpressionModel
                {
                    Condition = item.SearchQueryExpression.Condition,
                    Field = item.SearchQueryExpression.Field,
                    Id = item.SearchQueryExpression.Id,
                    Operator = item.SearchQueryExpression.Operator,
                    OperatorText = item.SearchQueryExpression.OperatorText,
                    SearchQueryId = item.SearchQueryExpression.SearchQueryId,
                    Value1 = item.SearchQueryExpression.Value1,
                    Value2 = item.SearchQueryExpression.Value2,
                    FieldUniqueId = item.SearchQueryExpression.Field.Id != Guid.Empty       ?
                                            item.SearchQueryExpression.Field.Id.ToString()  :
                                            item.SearchQueryExpression.Field.FieldUniqueId
                    
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
            IsSearchEnabled = true;

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
                catch(Exception ex)
                {
                    ProcessHelper.ProcessException(ex);
                }
            }
        }

        private void RunGlobalSearchDoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                string query = e.Argument + string.Empty;
                var results = _searchProvider.RunGlobalSearch(query, 0);
                if (results != null)
                {
                    var searchResults = new ObservableCollection<SearchResultModel>(results);
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

        private void RunGlobalSearchRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            IsProcessing = false;
            IsSearchEnabled = true;

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

        private void LoadMoreGlobalSearchDoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                SearchResultModel searchResult = (SearchResultModel) e.Argument;
                e.Result = _searchProvider.RunGlobalSearch(searchResult.GlobalSearchText, searchResult.PageIndex + 1);
            }
            catch (Exception ex)
            {
                e.Result = ex;
            }
        }

        private void LoadMoreGlobalSearchRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            IsProcessing = false;
            IsSearchEnabled = true;

            if (e.Result is Exception)
            {
                ProcessHelper.ProcessException(e.Result as Exception);
            }
            else
            {
                try
                {
                    var results = (List<SearchResultModel>) e.Result;
                    if (results == null || results.Count == 0)
                    {
                        return;
                    }

                    foreach(SearchResultModel result in results)
                    {
                        var existedResult = SearchResults.FirstOrDefault(p => p.DocumentTypeName == result.DocumentTypeName);
                        if (existedResult != null)
                        {
                            existedResult.HasMoreResult = result.HasMoreResult;
                            foreach (DataRow row in result.DataResult.Rows)
                            {
                                existedResult.DataResult.ImportRow(row);
                            }

                            existedResult.ResultCount = existedResult.DataResult.Rows.Count;
                        }
                        else
                        {
                            SearchResults.Add(result);
                        }
                    }

                    foreach(SearchResultModel result in SearchResults)
                    {
                        result.HasMoreResult = results[0].HasMoreResult;
                        result.PageIndex = results[0].PageIndex;
                    }
                }
                catch (Exception ex)
                {
                    ProcessHelper.ProcessException(ex);
                }
            }
        }

        private void OpenDocument()
        {
            var selectedRows = new List<DataRow>();
            foreach(var searchResult in SearchResults)
            {
                selectedRows.AddRange(searchResult.DataResult.Rows.Cast<DataRow>().Where(selectedRow => (bool) selectedRow[Common.COLUMN_SELECTED]));
            }

            if (selectedRows.Count > 0)
            {
                foreach(var selectedRow in selectedRows)
                {
                    OpenDocument(selectedRow);
                }
            }

        }

        private void DeleteDocument()
        {
            var selectedRows = new List<DataRow>();
            foreach (var searchResult in SearchResults)
            {
                selectedRows.AddRange(searchResult.DataResult.Rows.Cast<DataRow>().Where(selectedRow => (bool)selectedRow[Common.COLUMN_SELECTED]));
            }

            if (selectedRows.Count > 0)
            {
                if (DialogService.ShowTwoStateConfirmDialog(_resource.GetString("uiConfirmDeleteDocument")) == DialogServiceResult.Yes)
                {
                    IsProcessing = true;
                    IsSearchEnabled = false;

                    var deleteWorker = new BackgroundWorker();
                    deleteWorker.DoWork += DeleteWorkerDoWork;
                    deleteWorker.RunWorkerCompleted += DeleteWorkerRunWorkerCompleted;
                    deleteWorker.RunWorkerAsync(selectedRows);
                }
            }
        }

        private void DownloadDocument()
        {
            IsProcessing = true;
            IsSearchEnabled = false;

            var worker = new BackgroundWorker();
            worker.DoWork += DoGetDocument;
            worker.RunWorkerCompleted += DoDownloadDocumentCompleted;
            worker.RunWorkerAsync();
        }

        private void SendMailDocument()
        {
            IsProcessing = true;
            IsSearchEnabled = false;

            var worker = new BackgroundWorker();
            worker.DoWork += DoGetDocument;
            worker.RunWorkerCompleted += DoSendMailDocumentCompleted;
            worker.RunWorkerAsync();

        }


        private void PrintDocument()
        {
            IsProcessing = true;
            IsSearchEnabled = false;

            var worker = new BackgroundWorker();
            worker.DoWork += DoGetDocument;
            worker.RunWorkerCompleted += DoPrintDocumentCompleted;
            worker.RunWorkerAsync();
        }

        private void DoPrintDocumentCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            try
            {
                var printWorker = new BackgroundWorker();
                printWorker.DoWork += DoGetDocument;
                printWorker.RunWorkerCompleted += PrintWorkerRunWorkerCompleted;
                printWorker.RunWorkerAsync(true);
            }
            catch (Exception ex)
            {
                ProcessHelper.ProcessException(ex);
            }

        }

        private void PrintWorkerRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Result is Exception)
            {
                IsProcessing = false;
                ProcessHelper.ProcessException(e.Result as Exception);
            }
            else
            {
                try
                {
                    if (e.Result != null)
                    {
                        var documents = (List<DocumentModel>)e.Result;
                        var items = new List<CanvasElement>();

                        foreach (var document in documents)
                        {
                            var permission = new ContentViewerPermission
                            {
                                CanHideAnnotation = document.DocumentType.AnnotationPermission.AllowedHideRedaction,
                                CanSeeHighlight = document.DocumentType.AnnotationPermission.AllowedSeeHighlight,
                                CanSeeText = document.DocumentType.AnnotationPermission.AllowedSeeText
                            };

                            items.AddRange(document.Pages.Select(page => new CanvasElement(page.FileBinaries, page, permission)));

                            _actionLogProvider.AddLog("Print document", ActionName.Print, ObjectType.Document, document.Id);
                        }

                        var printHelper = new PrintHelper("CloudECM", new WorkingFolder("Serach")) { HandleException = ProcessHelper.ProcessException };
                        printHelper.Print(items);
                    }
                }
                catch (Exception ex)
                {
                    ProcessHelper.ProcessException(ex);
                }
                finally
                {
                    IsProcessing = false;
                }
            }
        }

        private void DeleteWorkerDoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                var selectedRows = (List<DataRow>)e.Argument;
                foreach (var selectedRow in selectedRows)
                {
                    _documentProvider.DeleteDocument((Guid)selectedRow[Common.COLUMN_DOCUMENT_ID]);
                }

                e.Result = selectedRows;
            }
            catch(Exception ex)
            {
                e.Result = ex;
            }
        }

        private void DeleteWorkerRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            IsProcessing = false;
            IsSearchEnabled = true;

            if (e.Result is Exception)
            {
                ProcessHelper.ProcessException(e.Result as Exception);
            }
            else
            {
                var selectedRows = (List<DataRow>)e.Result;
                foreach (DataRow row in selectedRows)
                {
                    SearchResultModel result = SearchResults.First(p => p.DocumentType.Id == (Guid)row[Common.COLUMN_DOCUMENT_TYPE_ID]);
                    result.DataResult.Rows.Remove(row);
                }
            }
        }

        private void SearchResultPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IsSelected")
            {
                bool hasCompoundDoc = false;
                bool hasNativeDoc = false;
                bool hasMediaDoc = false;
                EnableDelete = true;
                EnablePrint = true;
                EnableEmail = true;

                foreach (var result in SearchResults)
                {
                    var selectedRows = result.DataResult.Rows.OfType<DataRow>().Where(p => (bool)p[Common.COLUMN_SELECTED]).ToList();
                    if (selectedRows.Count > 0)
                    {
                        hasCompoundDoc = hasCompoundDoc || selectedRows.Any(p => (FileTypeModel) p[Common.COLUMN_BINARY_TYPE] == FileTypeModel.Compound);
                        hasNativeDoc = hasNativeDoc || selectedRows.Any(p => (FileTypeModel) p[Common.COLUMN_BINARY_TYPE] == FileTypeModel.Native);
                        hasMediaDoc = hasMediaDoc || selectedRows.Any(p => (FileTypeModel) p[Common.COLUMN_BINARY_TYPE] == FileTypeModel.Media);

                        EnableDelete = EnableDelete && result.DocumentType.DocumentTypePermission.AllowedDeletePage;
                        EnablePrint = EnablePrint && result.DocumentType.DocumentTypePermission.AlowedPrintDocument;
                        EnableEmail = EnableEmail && result.DocumentType.DocumentTypePermission.AllowedEmailDocument;
                    }
                }

                if (hasCompoundDoc || hasNativeDoc || hasMediaDoc)
                {
                    EnablePrint = false;
                }

                if (hasCompoundDoc)
                {
                    EnableEmail = false;
                }
            }
        }

        private void DoOpenDocumentCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Result is Exception)
            {
                IsProcessing = false;
                IsSearchEnabled = true;
                ProcessHelper.ProcessException(e.Result as Exception);
            }
            else
            {
                try
                {
                    var searchRow = e.Result as DataRow;
                    var documentId = (Guid)searchRow[Common.COLUMN_DOCUMENT_ID];
                    var existedDocumentViewModel = MainViewModel.DocumentViewModels.FirstOrDefault(p => p.Document.Id == documentId);

                    if (existedDocumentViewModel != null)
                    {
                        existedDocumentViewModel.IsActivated = true;
                    }
                    else
                    {
                        new DocumentViewModel(searchRow, MainViewModel);
                    }

                    _actionLogProvider.AddLog("Open document", ActionName.OpenDocument, ObjectType.Document, documentId);
                }
                finally
                {
                    IsProcessing = false;
                    IsSearchEnabled = true;
                }
            }
        }

        private void DoOpenDocument(object sender, DoWorkEventArgs e)
        {
            try
            {
                DataRow searchRow = (DataRow)e.Argument;
                e.Result = searchRow;
            }
            catch (Exception ex)
            {
                e.Result = ex;
            }
        }

        private void DoGetDocument(object sender, DoWorkEventArgs e)
        {
            try
            {
                e.Result = GetSelectedDocuments();
            }
            catch (Exception ex)
            {
                e.Result = ex;
            }
        }

        private void DoDownloadDocumentCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Result is Exception)
            {
                IsProcessing = false;
                ProcessHelper.ProcessException(e.Result as Exception);
            }
            else
            {
                //var selectedRows = (List<DataRow>)e.Result;
                //List<Guid> selectedDocIds = selectedRows.Select(p => (Guid)p[Common.COLUMN_DOCUMENT_ID]).ToList();
                try
                {
                    if (e.Result != null)
                    {
                        DownloadFileHelper downloadHelper = new DownloadFileHelper(new WorkingFolder("Search"))
                        {
                            HandleException = ProcessHelper.ProcessException
                        };

                        List<DocumentModel> documents = (List<DocumentModel>)e.Result;

                        if (documents.Count == 1)
                        {
                            var document = documents[0];
                            string extension;
                            string fileName = Guid.NewGuid().ToString();

                            if (document.BinaryType == FileTypeModel.Image)
                            {
                                extension = ".xps";
                                var permission = new ContentViewerPermission
                                {
                                    CanHideAnnotation = document.DocumentType.AnnotationPermission.AllowedHideRedaction,
                                    CanSeeHighlight = document.DocumentType.AnnotationPermission.AllowedSeeHighlight,
                                    CanSeeText = document.DocumentType.AnnotationPermission.AllowedSeeText
                                };

                                var items = new List<CanvasElement>();
                                items.AddRange(document.Pages.Select(page => new CanvasElement(page.FileBinaries, page, permission)));

                                downloadHelper.FileName = DialogService.ShowSaveFileDialog(string.Format("{0} documents |*{1}", extension.Replace(".", string.Empty).ToUpper(), extension),
                                                                                           fileName + extension);

                                downloadHelper.Add(items, fileName);
                            }
                            else
                            {
                                extension = document.Pages[0].FileExtension.StartsWith(".") ? document.Pages[0].FileExtension : "." + document.Pages[0].FileExtension;
                                downloadHelper.FileName = DialogService.ShowSaveFileDialog(string.Format("{0} documents |*{1}", extension.Replace(".", string.Empty).ToUpper(), extension),
                                                                                           fileName + extension);
                                downloadHelper.Add(document.Pages[0].FileBinaries, fileName, extension);
                            }

                            if (!string.IsNullOrEmpty(downloadHelper.FileName))
                            {
                                downloadHelper.Save();
                                _actionLogProvider.AddLog("Download document", ActionName.DownloadDocument, ObjectType.Document, document.Id);
                            }
                        }
                        else
                        {
                            downloadHelper.FolderName = DialogService.ShowFolderBrowseDialog(string.Empty) + "\\";

                            if (!string.IsNullOrEmpty(downloadHelper.FolderName))
                            {
                                foreach (var document in documents)
                                {
                                    string fileName = Guid.NewGuid().ToString();

                                    if (document.BinaryType == FileTypeModel.Image)
                                    {
                                        var permission = new ContentViewerPermission
                                        {
                                            CanHideAnnotation = document.DocumentType.AnnotationPermission.AllowedHideRedaction,
                                            CanSeeHighlight = document.DocumentType.AnnotationPermission.AllowedSeeHighlight,
                                            CanSeeText = document.DocumentType.AnnotationPermission.AllowedSeeText
                                        };

                                        var items = new List<CanvasElement>();
                                        items.AddRange(document.Pages.Select(page => new CanvasElement(page.FileBinaries, page, permission)));

                                        downloadHelper.Add(items, fileName);
                                    }
                                    else
                                    {
                                        string extension = document.Pages[0].FileExtension.StartsWith(".") ? document.Pages[0].FileExtension : "." + document.Pages[0].FileExtension;
                                        downloadHelper.Add(document.Pages[0].FileBinaries, fileName, extension);
                                    }

                                    _actionLogProvider.AddLog("Download document", ActionName.DownloadDocument, ObjectType.Document, document.Id);
                                }

                                downloadHelper.Save();
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    ProcessHelper.ProcessException(ex);
                }
                finally
                {
                    IsProcessing = false;
                }
            }
        }

        private void DoSendMailDocumentCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Result is Exception)
            {
                IsProcessing = false;
                ProcessHelper.ProcessException(e.Result as Exception);
            }
            else
            {
                try
                {
                    if (e.Result != null)
                    {
                        var emailHelper = new SendMailHelper(new WorkingFolder("Search")) { HandleException = ProcessHelper.ProcessException };
                        var documents = (List<DocumentModel>)e.Result;

                        foreach (var document in documents)
                        {
                            string fileName = Guid.NewGuid().ToString();

                            if (document.BinaryType == FileTypeModel.Image)
                            {
                                var permission = new ContentViewerPermission
                                {
                                    CanHideAnnotation = document.DocumentType.AnnotationPermission.AllowedHideRedaction,
                                    CanSeeHighlight = document.DocumentType.AnnotationPermission.AllowedSeeHighlight,
                                    CanSeeText = document.DocumentType.AnnotationPermission.AllowedSeeText
                                };

                                var items = new List<CanvasElement>();
                                items.AddRange(document.Pages.Select(page => new CanvasElement(page.FileBinaries, page, permission)));

                                emailHelper.AddAttachment(items, fileName);
                            }
                            else
                            {
                                string extension = document.Pages[0].FileExtension.StartsWith(".") ? document.Pages[0].FileExtension : "." + document.Pages[0].FileExtension;
                                emailHelper.AddAttachment(document.Pages[0].FileBinaries, fileName, extension);
                            }

                            _actionLogProvider.AddLog("Email document", ActionName.SendEmail, ObjectType.Document, document.Id);
                        }

                        emailHelper.SendMail();
                    }
                }
                catch (Exception ex)
                {
                    ProcessHelper.ProcessException(ex);
                }
                finally
                {
                    IsProcessing = false;
                }
            }
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
