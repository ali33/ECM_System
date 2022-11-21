using Ecm.AppHelper;
using Ecm.Archive.View;
using Ecm.DocViewer.Controls;
using Ecm.DocViewer.Helper;
using Ecm.DocViewer.Model;
using Ecm.Domain;
using Ecm.Model;
using Ecm.Model.DataProvider;
using Ecm.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Text;
using System.Windows.Input;

namespace Ecm.Archive.ViewModel
{
    public class SearchLinkDocumentViewModel : ComponentViewModel
    {
        #region Private members
        public const string _fieldContentId = "Content_CFCCCDF6_84CC_43ED_BE4A_617974473143";

        private SearchQueryModel _selectedSearchQuery;
        private DocumentTypeModel _selectedDocumentType;
        private RelayCommand _addSearchConditionCommand;
        private RelayCommand _resetSearchConditionCommand;
        private RelayCommand _advanceSearchCommand;
        private RelayCommand _removeAdditionalField;

        private readonly SearchProvider _searchProvider = new SearchProvider();
        private readonly SearchQueryProvider _searchQueryProvider = new SearchQueryProvider();
        private readonly DocumentTypeProvider _documentTypeProvider = new DocumentTypeProvider();
        private readonly DocumentProvider _documentProvider = new DocumentProvider();
        private readonly ActionLogProvider _actionLogProvider = new ActionLogProvider();
        private ObservableCollection<SearchResultModel> _searchResults = new ObservableCollection<SearchResultModel>();
        private bool _isSearchEnabled = true;
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
                    foreach (var searchResult in value)
                    {
                        searchResult.PropertyChanged += SearchResultPropertyChanged;
                    }
                }
            }
        }

        public MainViewModel MainViewModel { get; private set; }

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
        #endregion

        #region Public methods

        public SearchLinkDocumentViewModel(MainViewModel mainViewModel)
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
                    FieldUniqueId = item.SearchQueryExpression.Field.Id != Guid.Empty ?
                                            item.SearchQueryExpression.Field.Id.ToString() :
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
                catch (Exception ex)
                {
                    ProcessHelper.ProcessException(ex);
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
                foreach (var result in SearchResults)
                {
                    var selectedRows = result.DataResult.Rows.OfType<DataRow>().Where(p => (bool)p[Common.COLUMN_SELECTED]).ToList();
                    if (selectedRows.Count > 0)
                    {
                        hasCompoundDoc = hasCompoundDoc || selectedRows.Any(p => (FileTypeModel)p[Common.COLUMN_BINARY_TYPE] == FileTypeModel.Compound);
                        hasNativeDoc = hasNativeDoc || selectedRows.Any(p => (FileTypeModel)p[Common.COLUMN_BINARY_TYPE] == FileTypeModel.Native);
                        hasMediaDoc = hasMediaDoc || selectedRows.Any(p => (FileTypeModel)p[Common.COLUMN_BINARY_TYPE] == FileTypeModel.Media);

                    }
                }

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
