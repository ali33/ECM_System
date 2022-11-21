using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Windows.Input;

using Ecm.CaptureDomain;
using Ecm.CaptureModel;
using Ecm.CaptureModel.DataProvider;
using Ecm.Mvvm;
using Ecm.Capture.View;
using System.Resources;
using System.Reflection;
using System.Windows.Forms;
using System.Windows.Threading;
using System.Windows.Interop;
using Ecm.Utility;

namespace Ecm.Capture.ViewModel
{
    public class AssignedTaskViewModel : ComponentViewModel, IDisposable
    {
        #region Private members
        private const string ADVANCE_SEARCH = "AdvanceSearch";
        private const string DELEGATE = "DDELEGATE";

        private string _currentSearch = string.Empty;
        private readonly ResourceManager _resource = new ResourceManager("Ecm.Capture.Resources", Assembly.GetExecutingAssembly());
        private BatchTypeModel _selectedTypeBatch;
        private DocTypeModel _selectedDocumentType;
        private SearchQueryModel _selectedSearchQuery;
        private ObservableCollection<TaskMenu> _menus;
        private TaskType _selectedTaskType;
        private ObservableCollection<BatchTypeModel> _batchTypes;

        private readonly DocTypeProvider _documentTypeProvider = new DocTypeProvider();
        private readonly WorkItemProvider _workItemProvider = new WorkItemProvider();
        private readonly BatchTypeProvider _batchTypeProvider = new BatchTypeProvider();
        private SearchQueryProvider _searchQueryProvider = new SearchQueryProvider();
        //private readonly ActionLogProvider _actionLogProvider = new ActionLogProvider();
        private RelayCommand _openCommand;
        private RelayCommand _approveCommand;
        private RelayCommand _rejectCommand;
        private RelayCommand _delegateCommand;
        private RelayCommand _resumeCommand;
        private RelayCommand _unLockCommand;
        private RelayCommand _emailAsLinkCommand;
        private RelayCommand _deleteCommand;
        private RelayCommand _searchCommand;
        private RelayCommand _addSearchConditionCommand;
        private RelayCommand _resetSearchConditionCommand;
        private RelayCommand _deleteQueryCommand;
        private RelayCommand _saveQueryCommand;
        private RelayCommand _advanceSearchCommand;
        private RelayCommand _reloadCommand;

        private System.Threading.Timer _refreshTimer;
        private BackgroundWorker _searchWorker;
        private BackgroundWorker _refreshWorker;
        private BackgroundWorker _doActionWorker;
        private Dispatcher _dispatcher;
        #endregion

        #region Public properties

        public MainViewModel MainViewModel { get; private set; }

        public event EventHandler EmailAsLinkHandler;

        public event EventHandler RejectWorkItemHandler;

        public event EventHandler DelegateUserHandler;

        public WorkItemListViewModel WorkItems { get; set; }

        public ObservableCollection<DocTypeModel> DocumentTypes { get; private set; }

        public DocTypeModel SelectedDocumentType
        {
            get { return _selectedDocumentType; }
            set
            {
                _selectedDocumentType = value;
                OnPropertyChanged("SelectedDocumentType");
            }
        }

        public BatchTypeModel SelectedBatchType
        {
            get { return _selectedTypeBatch; }
            set
            {
                _selectedTypeBatch = value;
                OnPropertyChanged("SelectedBatchType");
                LoadAvailableFields();
                LoadDefaultSearchExpression();
                LoadSavedQueries(value.Id);
            }
        }

        public TaskType SelectedTaskType { get; set; }

        public ObservableCollection<BatchTypeModel> BatchTypes
        {
            get { return _batchTypes; }
            set
            {
                _batchTypes = value;
                OnPropertyChanged("BatchTypes");
            }
        }

        public ObservableCollection<TaskMenu> Menus
        {
            get { return _menus; }
            set
            {
                _menus = value;
                OnPropertyChanged("Menus");
            }
        }

        public WorkItemSearchResultModel Batchs { get; set; }

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
                    LoadSearchExpressionByQuery(value);
                }
            }
        }

        public ObservableCollection<SearchExpressionViewModel> SearchQueryExpressions { get; private set; }

        public ObservableCollection<FieldModel> AvailableFields { get; private set; }

        public ICommand OpenCommand
        {
            get
            {
                return _openCommand ?? (_openCommand = new RelayCommand(p => OpenWorkItem()));
            }
        }

        public ICommand ApproveCommand
        {
            get
            {
                return _approveCommand ?? (_approveCommand = new RelayCommand(p => ApproveWorkItems()));
            }
        }

        public ICommand RejectCommand
        {
            get
            {
                return _rejectCommand ?? (_rejectCommand = new RelayCommand(p => RejectWorkItems()));
            }
        }

        public ICommand DelegateCommand
        {
            get
            {
                return _delegateCommand ?? (_delegateCommand = new RelayCommand(p => DelegateWorkItems()));
            }
        }

        public ICommand ResumeCommand
        {
            get
            {
                return _resumeCommand ?? (_resumeCommand = new RelayCommand(p => ResumeWorkItems()));
            }
        }

        public ICommand UnLockCommand
        {
            get
            {
                return _unLockCommand ?? (_unLockCommand = new RelayCommand(p => UnLockWorkItems()));
            }
        }

        public ICommand EmailAsLinkCommand
        {
            get
            {
                return _emailAsLinkCommand ?? (_emailAsLinkCommand = new RelayCommand(p => EmailAsLinkWorkItems()));
            }
        }

        public ICommand DeleteCommand
        {
            get
            {
                return _deleteCommand ?? (_deleteCommand = new RelayCommand(p => DeleteWorkItems()));
            }
        }

        public ICommand SearchCommand
        {
            get
            {
                if (_searchCommand == null)
                {
                    _searchCommand = new RelayCommand(p => SearchByBatch(p as TaskMenuItem));
                }

                return _searchCommand;
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

        public ICommand ReloadCommand
        {
            get
            {
                if (_reloadCommand == null)
                {
                    _reloadCommand = new RelayCommand(p => ReloadAssignedTask());
                }

                return _reloadCommand;
            }
        }

        #endregion

        #region Public methods

        public AssignedTaskViewModel(MainViewModel mainViewModel, Dispatcher dispatcher)
        {
            MainViewModel = mainViewModel;
            _dispatcher = dispatcher;
            SearchQueryExpressions = new ObservableCollection<SearchExpressionViewModel>();
            SavedQueries = new ObservableCollection<SearchQueryModel>();
            AvailableFields = new ObservableCollection<FieldModel>();
            Initialize();
        }

        private bool CanRunAdvanceSearch()
        {
            return SelectedBatchType != null;
        }

        public void RunAdvanceSearch()
        {
            IsProcessing = true;

            var worker = new BackgroundWorker();
            worker.DoWork += RunAdvanceSearchDoWork;
            worker.RunWorkerCompleted += RunAdvanceSearchRunWorkerCompleted;
            worker.RunWorkerAsync();
        }

        public void RejectWorkItems()
        {

            var rejectNotes = new RejectNotesView();
            ResourceManager resource = new ResourceManager("Ecm.Capture.AssignedTaskView", Assembly.GetExecutingAssembly());

            var dialog = new DialogBaseView(rejectNotes) { Width = 440, Height = 280, Text = resource.GetString("RejectionNote") };
            rejectNotes.Dialog = dialog;

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                IsProcessing = true;
                var worker = new BackgroundWorker();
                worker.DoWork += DoRejectWorkItem;
                worker.RunWorkerCompleted += DoActionOnWorkItemCompleted;
                worker.RunWorkerAsync(rejectNotes.Notes);
            }
        }

        public void DelegateWorkItem(string toUser, string delegatedComment)
        {
            IsProcessing = true;

            var worker = new BackgroundWorker();
            worker.DoWork += DoDelegateWorkItem;
            worker.RunWorkerCompleted += DoActionOnWorkItemCompleted;
            worker.RunWorkerAsync(new string[2] { toUser, delegatedComment });
        }

        //public void RefreshSearchResult()
        //{
        //    DoAutoRefresh();
        //}

        protected override void OnDispose()
        {
            base.OnDispose();

            if (_refreshWorker != null)
            {
                _refreshWorker.CancelAsync();
                _refreshWorker.Dispose();
                _refreshWorker = null;
            }

            if (_searchWorker != null)
            {
                _searchWorker.CancelAsync();
                _searchWorker.Dispose();
                _searchWorker = null;
            }

            if (_refreshWorker != null)
            {
                _refreshWorker.CancelAsync();
                _refreshWorker.Dispose();
                _refreshWorker = null;
            }
        }

        #endregion

        #region Private methods

        private new void Initialize()
        {
            try
            {
                BatchTypes = new ObservableCollection<BatchTypeModel>(_batchTypeProvider.GetAssignWorkBatchTypes().OrderBy(h => h.Name));
                if (BatchTypes != null && BatchTypes.Count > 0)
                {
                    SelectedBatchType = BatchTypes[0];
                }

                PopulateMenu(BatchTypes);

                WorkItems = new WorkItemListViewModel(OpenWorkItem)
                {
                    ApproveCommand = ApproveCommand,
                    OpenCommand = OpenCommand,
                    UnLockCommand = UnLockCommand,
                    EmailAsLinkCommand = EmailAsLinkCommand,
                    ResumeCommand = ResumeCommand,
                    RejectCommand = RejectCommand,
                    DeleteCommand = DeleteCommand,
                    DelegateCommand = DelegateCommand
                };

                //RegisterAutoRefreshTimer();
            }
            catch (Exception ex)
            {
                ProcessHelper.ProcessException(ex);
            }
        }

        private void ReloadData()
        {
            BatchTypes = new ObservableCollection<BatchTypeModel>(_batchTypeProvider.GetAssignWorkBatchTypes());
            //_selectedTypeBatch = BatchTypes[0];
            PopulateMenu(BatchTypes);
        }

        private void PopulateMenu(ObservableCollection<BatchTypeModel> batchTypes)
        {
            ResourceManager resource = new ResourceManager("Ecm.Capture.AssignedTaskView", Assembly.GetExecutingAssembly());
            //_dispatcher.Invoke(new Action(delegate
            //    {
            Menus = new ObservableCollection<TaskMenu>();

            foreach (BatchTypeModel batchType in BatchTypes)
            {
                TaskMenu menu = new TaskMenu()
                {
                    BatchType = batchType,
                    MenuItems = new ObservableCollection<TaskMenuItem>(),
                    IsExpand = batchType.Id == SelectedBatchType.Id
                };

                int errorBatch = 0;
                int inprocessingBatch = 0;
                int lockedBatch = 0;
                int availableBatch = 0;
                int rejectedBatch = 0;

                _workItemProvider.CountBatchs(batchType.Id, out errorBatch, out inprocessingBatch, out lockedBatch, out availableBatch, out rejectedBatch);

                TaskMenuItem errorItem = new TaskMenuItem()
                {
                    BatchType = batchType,
                    Counts = errorBatch,
                    Name = resource.GetString(Common.BATCH_ERROR),
                    Type = TaskType.Error,
                    IsSelected = batchType.Id == SelectedBatchType.Id && SelectedTaskType == TaskType.Error
                };

                menu.MenuItems.Add(errorItem);

                TaskMenuItem inProcessingItem = new TaskMenuItem()
                {
                    BatchType = batchType,
                    Counts = inprocessingBatch,
                    Name = resource.GetString(Common.BATCH_IN_PROCESSING),
                    Type = TaskType.InProcessing,
                    IsSelected = batchType.Id == SelectedBatchType.Id && SelectedTaskType == TaskType.InProcessing
                };

                menu.MenuItems.Add(inProcessingItem);

                TaskMenuItem LockedItem = new TaskMenuItem()
                {
                    BatchType = batchType,
                    Counts = lockedBatch,
                    Name = resource.GetString(Common.BATCH_LOCKED),
                    Type = TaskType.Locked,
                    IsSelected = batchType.Id == SelectedBatchType.Id && SelectedTaskType == TaskType.Locked
                };

                menu.MenuItems.Add(LockedItem);

                TaskMenuItem rejectedItem = new TaskMenuItem()
                {
                    BatchType = batchType,
                    Counts = rejectedBatch,
                    Name = resource.GetString(Common.BATCH_REJECTED),
                    Type = TaskType.Rejected,
                    IsSelected = batchType.Id == SelectedBatchType.Id && SelectedTaskType == TaskType.Rejected
                };

                menu.MenuItems.Add(rejectedItem);

                TaskMenuItem waitingItem = new TaskMenuItem()
                {
                    BatchType = batchType,
                    Counts = availableBatch,
                    Name = resource.GetString(Common.BATCH_WAITING),
                    Type = TaskType.Available,
                    IsSelected = batchType.Id == SelectedBatchType.Id && SelectedTaskType == TaskType.Available
                };

                menu.MenuItems.Add(waitingItem);

                Menus.Add(menu);
            }

            //}));
        }

        private void ReloadAssignedTask()
        {
            ReloadData();
        }

        //Search Command
        private void LoadSavedQueries(Guid batchTypeId)
        {
            try
            {
                SavedQueries.Clear();
                var queries = _searchQueryProvider.GetSavedQueries(batchTypeId);
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

            int count = SelectedBatchType.Fields.Count;
            for (int i = 0; i < count; i++)
            {
                if (!SelectedBatchType.Fields[i].IsSystemField)
                {
                    var searchExpressionViewModel = new SearchExpressionViewModel();
                    var expression = new SearchQueryExpressionModel
                    {
                        Condition = i == 0 ? SearchConjunction.None : SearchConjunction.And,
                        Field = SelectedBatchType.Fields[i]
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
                SearchQueryExpressions.Clear();
                foreach (var item in value.SearchQueryExpressions)
                {
                    var viewModel = new SearchExpressionViewModel();
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

        private void LoadAvailableFields()
        {
            AvailableFields.Clear();

            var normalField = SelectedBatchType.Fields.Where(h => !h.IsSystemField).OrderBy(h => h.Name).ToList();
            var lastOrder = 0;
            for (; lastOrder < normalField.Count; lastOrder++)
            {
                AvailableFields.Add(normalField[lastOrder]);
            }

            LoadBatchProperties(lastOrder);
        }

        private void LoadBatchProperties(int lastOrder)
        {
            var systemField = new List<FieldModel>(16);

            FieldModel field = new FieldModel();

            field.UniqueId = Common.FieldBatchName;
            field.DisplayName = Common.BATCH_DISPLAY_NAME;
            field.Name = Common.BATCH_NAME;
            field.IsBatchProperty = true;
            field.DataType = FieldDataType.String;
            field.DisplayOrder = lastOrder++;
            field.BatchTypeId = SelectedBatchType.Id;

            systemField.Add(field);

            field = new FieldModel();

            field.UniqueId = Common.FieldBlockingDate;
            field.Name = Common.BATCH_BLOCKING_DATE;
            field.DisplayName = Common.BATCH_DISPLAY_BLOCKING_DATE;
            field.IsBatchProperty = true;
            field.DataType = FieldDataType.Date;
            field.DisplayOrder = lastOrder++;
            field.BatchTypeId = SelectedBatchType.Id;

            systemField.Add(field);

            field = new FieldModel();

            field.UniqueId = Common.FieldCreatedBy;
            field.Name = Common.BATCH_CREATED_BY;
            field.DisplayName = Common.BATCH_DISPLAY_CREATED_BY;
            field.IsBatchProperty = true;
            field.DataType = FieldDataType.String;
            field.DisplayOrder = lastOrder++;
            field.BatchTypeId = SelectedBatchType.Id;

            systemField.Add(field);

            field = new FieldModel();

            field.UniqueId = Common.FieldCreatedDate;
            field.Name = Common.BATCH_CREATED_DATE;
            field.DisplayName = Common.BATCH_DISPLAY_CREATED_DATE;
            field.IsBatchProperty = true;
            field.DataType = FieldDataType.Date;
            field.DisplayOrder = lastOrder++;
            field.BatchTypeId = SelectedBatchType.Id;

            systemField.Add(field);

            field = new FieldModel();

            field.UniqueId = Common.FieldModifiedBy;
            field.Name = Common.BATCH_MODIFIED_BY;
            field.DisplayName = Common.BATCH_DISPLAY_MODIFIED_BY;
            field.IsBatchProperty = true;
            field.DataType = FieldDataType.String;
            field.DisplayOrder = lastOrder++;
            field.BatchTypeId = SelectedBatchType.Id;

            systemField.Add(field);

            field = new FieldModel();

            field.UniqueId = Common.FieldModifiedDate;
            field.Name = Common.BATCH_MODIFIED_DATE;
            field.DisplayName = Common.BATCH_DISPLAY_MODIFIED_DATE;
            field.IsBatchProperty = true;
            field.DataType = FieldDataType.Date;
            field.DisplayOrder = lastOrder++;
            field.BatchTypeId = SelectedBatchType.Id;

            systemField.Add(field);

            field = new FieldModel();

            field.UniqueId = Common.FieldLockedBy;
            field.Name = Common.BATCH_LOCKED_BY;
            field.DisplayName = Common.BATCH_DISPLAY_LOCKED_BY;
            field.IsBatchProperty = true;
            field.DataType = FieldDataType.String;
            field.DisplayOrder = lastOrder++;
            field.BatchTypeId = SelectedBatchType.Id;

            systemField.Add(field);

            field = new FieldModel();

            field.UniqueId = Common.FieldLastAccessedBy;
            field.Name = Common.BATCH_LASTACCESSED_BY;
            field.DisplayName = Common.BATCH_DISPLAY_LASTACCESSED_BY;
            field.IsBatchProperty = true;
            field.DataType = FieldDataType.String;
            field.DisplayOrder = lastOrder++;
            field.BatchTypeId = SelectedBatchType.Id;

            systemField.Add(field);

            field = new FieldModel();

            field.UniqueId = Common.FieldLastAccessedDate;
            field.Name = Common.BATCH_LASTACCESSED_DATE;
            field.DisplayName = Common.BATCH_DISPLAY_LASTACCESSED_DATE;
            field.IsBatchProperty = true;
            field.DataType = FieldDataType.Date;
            field.DisplayOrder = lastOrder++;
            field.BatchTypeId = SelectedBatchType.Id;

            systemField.Add(field);

            field = new FieldModel();

            field.UniqueId = Common.FieldBlockingActivityName;
            field.Name = Common.BATCH_BLOCKING_ACTIVITY_NAME;
            field.DisplayName = Common.BATCH_DISPLAY_BLOCKING_ACTIVITY_NAME;
            field.IsBatchProperty = true;
            field.DataType = FieldDataType.String;
            field.DisplayOrder = lastOrder++;
            field.BatchTypeId = SelectedBatchType.Id;

            systemField.Add(field);

            field = new FieldModel();

            field.UniqueId = Common.FieldBlockingActivityDescription;
            field.Name = Common.BATCH_BLOCKING_ACTIVITY_DESCRIPTION;
            field.DisplayName = Common.BATCH_DISPLAY_BLOCKING_ACTIVITY_DESCRIPTION;
            field.IsBatchProperty = true;
            field.DataType = FieldDataType.String;
            field.DisplayOrder = lastOrder++;
            field.BatchTypeId = SelectedBatchType.Id;

            systemField.Add(field);

            field = new FieldModel();

            field.UniqueId = Common.FieldStatusMsg;
            field.Name = Common.BATCH_STATUS_MSG;
            field.DisplayName = Common.BATCH_DISPLAY_STATUS_MSG;
            field.IsBatchProperty = true;
            field.DataType = FieldDataType.String;
            field.DisplayOrder = lastOrder++;
            field.BatchTypeId = SelectedBatchType.Id;

            systemField.Add(field);

            field = new FieldModel();

            field.UniqueId = Common.FieldDocumentCount;
            field.Name = Common.BATCH_DOCUMENT_COUNT;
            field.DisplayName = Common.BATCH_DISPLAY_DOCUMENT_COUNT;
            field.IsBatchProperty = true;
            field.DataType = FieldDataType.Integer;
            field.DisplayOrder = lastOrder++;
            field.BatchTypeId = SelectedBatchType.Id;

            systemField.Add(field);

            field = new FieldModel();

            field.UniqueId = Common.FieldPageCount;
            field.Name = Common.BATCH_PAGE_COUNT;
            field.DisplayName = Common.BATCH_DISPLAY_PAGE_COUNT;
            field.IsBatchProperty = true;
            field.DataType = FieldDataType.Integer;
            field.DisplayOrder = lastOrder++;
            field.BatchTypeId = SelectedBatchType.Id;

            systemField.Add(field);

            field = new FieldModel();

            field.UniqueId = Common.FieldDelegatedBy;
            field.Name = Common.BATCH_DELEGATED_BY;
            field.DisplayName = Common.BATCH_DISPLAY_DELEGATED_BY;
            field.IsBatchProperty = true;
            field.DataType = FieldDataType.String;
            field.DisplayOrder = lastOrder++;
            field.BatchTypeId = SelectedBatchType.Id;

            systemField.Add(field);

            field = new FieldModel();

            field.UniqueId = Common.FieldDelegatedTo;
            field.Name = Common.BATCH_DELEGATED_TO;
            field.DisplayName = Common.BATCH_DISPLAY_DELEGATED_TO;
            field.IsBatchProperty = true;
            field.DataType = FieldDataType.String;
            field.DisplayOrder = lastOrder++;
            field.BatchTypeId = SelectedBatchType.Id;

            systemField.Add(field);

            foreach (var item in systemField.OrderBy(h => h.Name))
            {
                AvailableFields.Add(item);
            }

        }

        private bool CanAddCondition()
        {
            return SelectedBatchType != null;
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
                if (SelectedSearchQuery == null)
                {
                    var selectQueryName = new SearchQueryName();
                    var dialog = new DialogBaseView(selectQueryName) { Text = _resource.GetString("tbQueryName"), Width = 300, Height = 150 };
                    selectQueryName.Dialog = dialog;
                    if (SelectedDocumentType != null)
                    {
                        selectQueryName.DocumentTypeId = SelectedDocumentType.Id;

                    }
                    else
                    {
                        selectQueryName.DocumentTypeId = SelectedBatchType.Id;
                    }

                    selectQueryName.QueryNameExisted = _searchQueryProvider.QueryExisted;

                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
                        SelectedSearchQuery = new SearchQueryModel
                        {
                            BatchTypeId = SelectedBatchType.Id,
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
                LoadSavedQueries(SelectedBatchType.Id);
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
                    Value2 = item.SearchQueryExpression.Value2
                };

                if (item.SearchQueryExpression.Field.DataType == FieldDataType.Date)
                {
                    searchQueryExpression.Value1 = item.SearchQueryExpression.DateValue1.Value.ToString("yyyy-MM-dd");
                    searchQueryExpression.Value2 = item.SearchQueryExpression.DateValue2.HasValue
                                                    ? item.SearchQueryExpression.DateValue2.Value.ToString("yyyy-MM-dd")
                                                    : null;
                }

                expressions.Add(searchQueryExpression);
            }

            if (expressions.Count > 0)
            {
                expressions[0].Condition = SearchConjunction.None;
            }

            return expressions;
        }

        //End search command
        private void RunAdvanceSearchDoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                _currentSearch = ADVANCE_SEARCH;
                ObservableCollection<SearchQueryExpressionModel> searchExpression = GetSearchExpressions();
                e.Result = _workItemProvider.RunAdvanceSearch(0, SelectedBatchType.Id, new SearchQueryModel { SearchQueryExpressions = searchExpression, SearchQueryString = BuildSearchExpression(searchExpression) });
            }
            catch (Exception ex)
            {
                e.Result = ex;
            }
        }

        private string BuildSearchExpression(ObservableCollection<SearchQueryExpressionModel> searchs)
        {
            string expression = string.Empty;
            foreach (SearchQueryExpressionModel searchExpressionViewModel in searchs)
            {
                if (!string.IsNullOrEmpty(searchExpressionViewModel.Value1))
                {
                    string itemName = "[" + searchExpressionViewModel.Field.Name.Replace(" ", string.Empty) + "]";
                    if (searchExpressionViewModel.Condition != SearchConjunction.None)
                    {
                        if (!string.IsNullOrEmpty(expression))
                        {
                            expression += " " + searchExpressionViewModel.Condition.ToString() + " ";
                        }
                    }

                    switch (searchExpressionViewModel.Field.DataType)
                    {
                        case FieldDataType.Picklist:
                        case FieldDataType.String:

                            var tempValue = string.Format("{0}", searchExpressionViewModel.Value1).Replace("'", "''");
                            if (searchExpressionViewModel.Operator != SearchOperator.Equal
                                && searchExpressionViewModel.Operator != SearchOperator.NotEqual)
                            {
                                tempValue = tempValue.Replace("[", "[[]");
                                tempValue = tempValue.Replace("%", "[%]");
                                tempValue = tempValue.Replace("_", "[_]");
                            }


                            switch (searchExpressionViewModel.Operator)
                            {
                                case SearchOperator.Contains:
                                    expression += itemName + " like '%" + tempValue + "%' ";
                                    break;
                                case SearchOperator.NotContains:
                                    expression += itemName + " not like '%" + tempValue + "%' ";
                                    break;
                                case SearchOperator.EndsWith:
                                    expression += itemName + " like '%" + tempValue + "' ";
                                    break;
                                case SearchOperator.StartsWith:
                                    expression += itemName + " like '" + tempValue + "%' ";
                                    break;
                                case SearchOperator.Equal:
                                    expression += itemName + " = '" + tempValue + "' ";
                                    break;
                                case SearchOperator.NotEqual:
                                    expression += itemName + " <> '" + tempValue + "' ";
                                    break;
                                default:
                                    expression += itemName + " like '%" + tempValue + "%' ";
                                    break;
                            }
                            break;
                        case FieldDataType.Integer:
                        case FieldDataType.Decimal:
                            switch (searchExpressionViewModel.Operator)
                            {
                                case SearchOperator.Equal:
                                    expression += itemName + " = " + searchExpressionViewModel.Value1;
                                    break;
                                case SearchOperator.NotEqual:
                                    expression += itemName + " <> " + searchExpressionViewModel.Value1;
                                    break;
                                case SearchOperator.GreaterThan:
                                    expression += itemName + " > " + searchExpressionViewModel.Value1;
                                    break;
                                case SearchOperator.GreaterThanOrEqualTo:
                                    expression += itemName + " >= " + searchExpressionViewModel.Value1;
                                    break;
                                case SearchOperator.LessThan:
                                    expression += itemName + " < " + searchExpressionViewModel.Value1;
                                    break;
                                case SearchOperator.LessThanOrEqualTo:
                                    expression += itemName + " <= " + searchExpressionViewModel.Value1;
                                    break;
                                case SearchOperator.InBetween:
                                    if (!string.IsNullOrEmpty(searchExpressionViewModel.Value1))
                                    {
                                        expression += "(";
                                        expression += itemName + " >= " + searchExpressionViewModel.Value1;
                                        expression += "AND ";
                                        expression += itemName + " <= " + searchExpressionViewModel.Value2;
                                        expression += ") ";
                                    }
                                    break;
                                default:
                                    expression += itemName + " = " + searchExpressionViewModel.Value1;
                                    break;
                            }
                            break;
                        case FieldDataType.Boolean:
                            expression += itemName + " = " + searchExpressionViewModel.Value1;
                            break;
                        case FieldDataType.Date:
                            switch (searchExpressionViewModel.Operator)
                            {
                                case SearchOperator.Equal:
                                    expression += "(";
                                    expression += itemName + " >= CONVERT(DATETIME,'" + Convert.ToDateTime(searchExpressionViewModel.Value1 + " 00:00:00").ToString("yyyy-MM-dd HH:mm:ss") + "') ";
                                    expression += "AND ";
                                    expression += itemName + " <= CONVERT(DATETIME,'" + Convert.ToDateTime(searchExpressionViewModel.Value1 + " 23:59:59").ToString("yyyy-MM-dd HH:mm:ss") + "') ";
                                    expression += ") ";
                                    break;
                                case SearchOperator.NotEqual:
                                    expression += "(";
                                    expression += itemName + " < CONVERT(DATETIME,'" + Convert.ToDateTime(searchExpressionViewModel.Value1 + " 00:00:00").ToString("yyyy-MM-dd HH:mm:ss") + "') ";
                                    expression += "OR ";
                                    expression += itemName + " > CONVERT(DATETIME,'" + Convert.ToDateTime(searchExpressionViewModel.Value1 + " 23:59:59").ToString("yyyy-MM-dd HH:mm:ss") + "') ";
                                    expression += ")";
                                    break;
                                case SearchOperator.GreaterThan:
                                    expression += itemName + " > CONVERT(DATETIME,'" + Convert.ToDateTime(searchExpressionViewModel.Value1 + " 23:59:59").ToString("yyyy-MM-dd HH:mm:ss") + "') ";
                                    break;
                                case SearchOperator.GreaterThanOrEqualTo:
                                    expression += itemName + " >= CONVERT(DATETIME,'" + Convert.ToDateTime(searchExpressionViewModel.Value1 + " 00:00:00").ToString("yyyy-MM-dd HH:mm:ss") + "') ";
                                    break;
                                case SearchOperator.LessThan:
                                    expression += itemName + " < CONVERT(DATETIME,'" + Convert.ToDateTime(searchExpressionViewModel.Value1 + " 00:00:00").ToString("yyyy-MM-dd HH:mm:ss") + "') ";
                                    break;
                                case SearchOperator.LessThanOrEqualTo:
                                    expression += itemName + " <= CONVERT(DATETIME,'" + Convert.ToDateTime(searchExpressionViewModel.Value1 + " 23:59:59").ToString("yyyy-MM-dd HH:mm:ss") + "') ";
                                    break;
                                case SearchOperator.InBetween:
                                    if (!string.IsNullOrEmpty(searchExpressionViewModel.Value1))
                                    {
                                        expression += "(";
                                        expression += itemName + " >= CONVERT(DATETIME,'" + Convert.ToDateTime(searchExpressionViewModel.Value1 + " 00:00:00").ToString("yyyy-MM-dd HH:mm:ss") + "') ";
                                        expression += "AND ";
                                        expression += itemName + " <= CONVERT(DATETIME,'" + Convert.ToDateTime(searchExpressionViewModel.Value2 + " 23:59:59").ToString("yyyy-MM-dd HH:mm:ss") + "') ";
                                        expression += ") ";
                                    }
                                    break;
                                default:
                                    expression += "(";
                                    expression += itemName + " >= CONVERT(DATETIME,'" + Convert.ToDateTime(searchExpressionViewModel.Value1 + " 00:00:00").ToString("yyyy-MM-dd HH:mm:ss") + "') ";
                                    expression += "AND ";
                                    expression += itemName + " <= CONVERT(DATETIME,'" + Convert.ToDateTime(searchExpressionViewModel.Value1 + " 23:59:59").ToString("yyyy-MM-dd HH:mm:ss") + "') ";
                                    expression += ") ";
                                    break;
                            }
                            break;
                        default:
                            break;
                    }
                }
            }

            return expression;
        }

        private void RunAdvanceSearchRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
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
                    WorkItemSearchResultModel searchResult = e.Result as WorkItemSearchResultModel;
                    WorkItems.SearchResult = searchResult;
                    //this..DocumentCount = searchResult == null ? 0 : searchResult.DataResult.Rows.Count;
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

        private void EmailAsLinkWorkItems()
        {
            //if (EmailAsLinkHandler != null)
            //{
            //    EmailAsLinkHandler(this, null);
            //    _workItemProvider.AddLog("Send mail work items ids: " + WorkItems.GetSelectedWorkItemIds().ToArray().ToString(), ActionName.SendEmail);
            //}
            try
            {
                string url = BrowserInteropHelper.Source.AbsoluteUri;
                if (BrowserInteropHelper.Source.Query != string.Empty)
                {
                    url = BrowserInteropHelper.Source.AbsoluteUri.Replace(BrowserInteropHelper.Source.Query, string.Empty);
                }

                const string queryTemplate = "mode=workitem&username={0}&workitemid={1}";
                string newLine = Environment.NewLine;
                string body = string.Empty;

                List<Guid> ids = WorkItems.GetSelectedWorkItemIds();
                var batchIds = new List<string>();

                foreach (Guid id in ids)
                {
                    // Use Uri to encode the query by using a dummy URL
                    var encodedUri = new Uri("http://localhost/index.html?" + string.Format(queryTemplate, LoginViewModel.LoginUser.Username, id));
                    body += newLine + url + encodedUri.Query;

                    batchIds.Add(id + string.Empty);
                }

                if (body != string.Empty)
                {
                    body = _resource.GetString("uiEmailBody") + newLine + newLine + body.Substring(newLine.Length);
                    var mapi = new UtilsMapi();
                    mapi.SendMailPopup(_resource.GetString("uiEmailSubject"), body);
                }

                _workItemProvider.AddLog("Send mail work items ids: " + string.Join(",", WorkItems.GetSelectedWorkItemIds()), ActionName.SendEmail);
            }
            catch (Exception ex)
            {
                ProcessHelper.ProcessException(ex);
            }
        }

        private void OpenWorkItem()
        {
            List<Guid> ids = WorkItems.GetSelectedWorkItemIds();

            // Just open first selected work item
            if (ids.Count > 0)
            {
                foreach (var id in ids)
                {
                    OpenWorkItem(id);
                }
            }
        }

        private void OpenWorkItem(Guid id)
        {
            IsProcessing = true;

            var worker = new BackgroundWorker();
            worker.DoWork += DoOpenWorkItem;
            worker.RunWorkerCompleted += DoOpenWorkItemCompleted;
            worker.RunWorkerAsync(id);
        }

        private void DoOpenWorkItem(object sender, DoWorkEventArgs e)
        {
            try
            {
                Guid id = (Guid)e.Argument;

                var existedWorkItemViewModel = MainViewModel.WorkItemViewModels.FirstOrDefault(p => p.WorkItem.Id == id);
                if (existedWorkItemViewModel != null)
                {
                    e.Result = new BatchModel() { Id = id };
                }
                else
                {
                    e.Result = _workItemProvider.GetWorkItem(id);
                }
            }
            catch (Exception ex)
            {
                e.Result = ex;
            }
        }

        private void DoOpenWorkItemCompleted(object sender, RunWorkerCompletedEventArgs e)
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
                    BatchModel workItem = e.Result as BatchModel;
                    if (workItem != null)
                    {
                        var existedWorkItemViewModel = MainViewModel.WorkItemViewModels.FirstOrDefault(p => p.WorkItem.Id == workItem.Id);

                        if (existedWorkItemViewModel != null)
                        {
                            existedWorkItemViewModel.IsActivated = true;
                        }
                        else
                        {
                            new WorkItemViewModel(workItem, MainViewModel);
                        }

                        //_actionLogProvider.AddLog("Open work item", ActionName.OpenDocument, ObjectType.Document, workItem.Id);
                    }
                }
                catch (Exception ex)
                {
                    ProcessHelper.ProcessException(ex);
                }
                finally
                {
                    PopulateMenu(BatchTypes);
                    if (_currentSearch != ADVANCE_SEARCH)
                    {
                        WorkItems.SearchResult = _workItemProvider.GetBatchByStatus(_currentSearch, SelectedBatchType.Id, 0);
                    }
                    else
                    {
                        ObservableCollection<SearchQueryExpressionModel> searchExpression = GetSearchExpressions();
                        WorkItems.SearchResult = _workItemProvider.RunAdvanceSearch(0, SelectedBatchType.Id, new SearchQueryModel { SearchQueryExpressions = searchExpression, SearchQueryString = BuildSearchExpression(searchExpression) });
                    }

                    IsProcessing = false;
                }
            }
        }

        //private void RejectWorkItems()
        //{
        //    RejectWorkItems();
        //    //if (RejectWorkItemHandler != null)
        //    //{
        //    //    RejectWorkItemHandler(this, null);
        //    //}

        //}

        private void DelegateWorkItems()
        {
            //if (DelegateUserHandler != null)
            //{
            //    DelegateUserHandler(this, null);
            //}

            DelegationUserView delegationView = new DelegationUserView();
            ResourceManager resource = new ResourceManager("Ecm.Capture.AssignedTaskView", Assembly.GetExecutingAssembly());
            DialogBaseView dialog = new DialogBaseView(delegationView) { Width = 500, Height = 400, Text = resource.GetString("DelegationTitle") };
            delegationView.Dialog = dialog;

            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                IsProcessing = true;
                var worker = new BackgroundWorker();
                worker.DoWork += DoDelegateWorkItem;
                worker.RunWorkerCompleted += DoActionOnWorkItemCompleted;
                worker.RunWorkerAsync(new string[] { delegationView.DelegationUser, delegationView.DelegatedComment });
            }
        }

        private void ApproveWorkItems()
        {
            DoAction("APPROVE");
        }

        private void UnLockWorkItems()
        {
            DoAction("UNLOCK");
        }

        private void ResumeWorkItems()
        {
            DoAction("RESUME");
        }

        private void DeleteWorkItems()
        {
            DoAction("DELETE");
        }

        private void DoAction(string action)
        {
            if ("unlock".Equals(action, StringComparison.OrdinalIgnoreCase))
            {
                var message = _resource.GetString("uiConfirmUnlock") + Environment.NewLine + _resource.GetString("uiConfirmContinue");

                if (DialogService.ShowTwoStateConfirmDialog(message) != DialogServiceResult.Yes)
                {
                    return;
                }
            }

            IsProcessing = true;

            if (_doActionWorker != null && _doActionWorker.IsBusy)
            {
                _doActionWorker.CancelAsync();
                _doActionWorker.Dispose();
                _doActionWorker = null;
            }

            if (_doActionWorker == null)
            {
                _doActionWorker = new BackgroundWorker() { WorkerSupportsCancellation = true };
                _doActionWorker.DoWork += DoActionOnWorkItem;
                _doActionWorker.RunWorkerCompleted += DoActionOnWorkItemCompleted;
            }

            if (_searchWorker != null && _searchWorker.IsBusy)
            {
                _searchWorker.CancelAsync();
            }

            if (_refreshWorker != null && _refreshWorker.IsBusy)
            {
                _refreshWorker.CancelAsync();
            }

            _doActionWorker.RunWorkerAsync(action);
        }

        private void DoActionOnWorkItem(object sender, DoWorkEventArgs e)
        {
            try
            {
                List<Guid> ids = WorkItems.GetSelectedWorkItemIds();

                List<BatchModel> batchs = _workItemProvider.GetBatchs(ids);

                if ("APPROVE".Equals(e.Argument))
                {
                    _workItemProvider.ApproveWorkItems(batchs.Select(h => h.Id).ToList());
                    //_workItemProvider.AddLog("Approver work items ids: " + string.Join(",", ids), ActionName.ApprovedBatch);
                }
                else if ("REJECT".Equals(e.Argument))
                {
                    //var rejectNotes = new RejectNotesView();
                    //ResourceManager resource = new ResourceManager("Ecm.Capture.AssignedTaskView", Assembly.GetExecutingAssembly());

                    //var dialog = new DialogBaseView(rejectNotes) { Width = 440, Height = 280, Text = resource.GetString("RejectionNote") };
                    //rejectNotes.Dialog = dialog;

                    //if (dialog.ShowDialog() == DialogResult.OK)
                    //{
                    //    _workItemProvider.RejectWorkItems(ids, rejectNotes.Notes);
                    //}
                }
                else if ("UNLOCK".Equals(e.Argument))
                {
                    List<Guid> canUnlockIds = batchs.Where(p => p.LockedBy == LoginViewModel.LoginUser.Username || LoginViewModel.LoginUser.IsAdmin).Select(p => p.Id).ToList();
                    List<string> cannotUnlockIds = batchs.Where(p => p.LockedBy != LoginViewModel.LoginUser.Username && !LoginViewModel.LoginUser.IsAdmin).Select(p => p.BatchName).ToList();
                    bool canUnlock = true;

                    if (cannotUnlockIds.Count > 0)
                    {
                        string message = _resource.GetString("uiUnlockBatchConfirmMsg");//"You do not have permission to unlock batchs {0}. Do you want to unlock left batchs?";
                        message = string.Format(message, string.Join(",", cannotUnlockIds));
                        DialogServiceResult result = DialogService.ShowTwoStateConfirmDialog(message);
                        if (result == DialogServiceResult.Yes)
                        {
                            canUnlock = true;
                        }
                        else if (result == DialogServiceResult.No)
                        {
                            canUnlock = false;
                        }
                    }

                    if (canUnlock)
                    {
                        _workItemProvider.UnLockWorkItems(canUnlockIds);
                        _workItemProvider.AddLog("Unlock work items ids: " + string.Join(",", canUnlockIds), ActionName.UnlockedBatch);
                    }

                }
                else if ("RESUME".Equals(e.Argument))
                {
                    _workItemProvider.ResumeWorkItems(ids);
                    _workItemProvider.AddLog("Resume work items ids: " + string.Join(",", ids), ActionName.ResumeBatch);
                }
                else if ("DELETE".Equals(e.Argument))
                {
                    List<Guid> canDeleteIds = batchs.Where(p => p.Permission.CanDelete).Select(p => p.Id).ToList();
                    List<string> cannotDeleteIds = batchs.Where(p => !p.Permission.CanDelete).Select(p => p.BatchName).ToList();
                    bool canDelete = true;

                    if (cannotDeleteIds.Count > 0)
                    {
                        string message = _resource.GetString("uiDeleteBatchConfirmMsg");//"You do not have permission to delete batchs {0}. Do you want to delete left batchs?";
                        message = string.Format(message, string.Join(",", cannotDeleteIds));

                        DialogServiceResult result = DialogService.ShowTwoStateConfirmDialog(message);
                        if (result == DialogServiceResult.Yes)
                        {
                            canDelete = true;
                        }
                        else
                        {
                            canDelete = false;

                        }
                    }

                    if (canDelete)
                    {
                        if (DialogService.ShowTwoStateConfirmDialog(_resource.GetString("uiConfirmDelete")) == DialogServiceResult.Yes)
                        {
                            _workItemProvider.DeleteWorkItems(canDeleteIds);
                            _workItemProvider.AddLog("Delete work items ids: " + string.Join(",", ids), ActionName.ResumeBatch);
                        }
                    }
                }

                if (_currentSearch != ADVANCE_SEARCH)
                {
                    e.Result = _workItemProvider.GetBatchByStatus(_currentSearch, SelectedBatchType.Id, 0);
                }
                else
                {
                    e.Result = _workItemProvider.RunAdvanceSearch(0, SelectedBatchType.Id, new SearchQueryModel { SearchQueryExpressions = GetSearchExpressions() });
                }

                _workItemProvider.AddLog("Reload search result", ActionName.ReloadSearchResult);
            }
            catch (Exception ex)
            {
                e.Result = ex;
            }
        }

        private void DoRejectWorkItem(object sender, DoWorkEventArgs e)
        {
            try
            {
                List<Guid> ids = WorkItems.GetSelectedWorkItemIds();
                _workItemProvider.RejectWorkItems(ids, e.Argument as string);
                _workItemProvider.AddLog("Reject work items ids: " + string.Join(",", ids), ActionName.RejectedBatch);

                // Refresh result
                if (_currentSearch == ADVANCE_SEARCH)
                {
                    e.Result = _workItemProvider.RunAdvanceSearch(0, SelectedBatchType.Id, new SearchQueryModel { SearchQueryExpressions = GetSearchExpressions() });
                }
                else
                {
                    e.Result = _workItemProvider.GetBatchByStatus(_currentSearch, SelectedBatchType.Id, 0);
                }
                _workItemProvider.AddLog("Reload search result", ActionName.ReloadSearchResult);
            }
            catch (Exception ex)
            {
                e.Result = ex;
            }
        }

        private void DoDelegateWorkItem(object sender, DoWorkEventArgs e)
        {
            try
            {
                List<Guid> ids = WorkItems.GetSelectedWorkItemIds();
                string[] arg = e.Argument as string[];

                _workItemProvider.DelegateWorkItems(ids, arg[0], arg[1]);
                _workItemProvider.AddLog("Delegate work items ids: " + string.Join(",", ids), ActionName.DelegatedBatch);

                // Refresh result
                if (_currentSearch == ADVANCE_SEARCH)
                {
                    e.Result = _workItemProvider.RunAdvanceSearch(0, SelectedBatchType.Id, new SearchQueryModel { SearchQueryExpressions = GetSearchExpressions() });
                }
                else
                {
                    e.Result = _workItemProvider.GetBatchByStatus(_currentSearch, SelectedBatchType.Id, 0);
                }
            }
            catch (Exception ex)
            {
                e.Result = ex;
            }
        }

        private void DoActionOnWorkItemCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Result != null && e.Result is Exception)
            {
                IsProcessing = false;
                ProcessHelper.ProcessException(e.Result as Exception);
            }
            else
            {
                try
                {
                    WorkItemSearchResultModel searchResult = null;
                    if (e.Result != null)
                    {
                        searchResult = e.Result as WorkItemSearchResultModel;
                    }

                    WorkItems.SearchResult = searchResult;
                    BatchTypes = new ObservableCollection<BatchTypeModel>(_batchTypeProvider.GetAssignWorkBatchTypes());
                    PopulateMenu(BatchTypes);
                }
                catch (Exception ex)
                {
                    ProcessHelper.ProcessException(ex);
                }
                finally
                {
                    //RefreshSearchResult();
                    IsProcessing = false;
                }
            }
        }

        // Auto refresh
        private void RegisterAutoRefreshTimer()
        {
            if (_refreshTimer == null)
            {
                _refreshTimer = new System.Threading.Timer(StartRefresh, null, 1000 * 60 * 3, Timeout.Infinite);
            }
        }

        private void StartRefresh(object sender)
        {
            DoAutoRefresh();
        }

        private void PostponeRefresh()
        {
            if (_refreshTimer == null)
            {
                _refreshTimer = new System.Threading.Timer(StartRefresh, null, 1000 * 60 * 3, Timeout.Infinite);
            }
            else
            {
                // postpone auto-refresh by 5 minutes
                _refreshTimer.Change(1000 * 60 * 3, Timeout.Infinite);
            }
        }

        private void DoAutoRefresh()
        {
            IsProcessing = true;
            if (_doActionWorker != null && _doActionWorker.IsBusy)
            {
                _refreshWorker.CancelAsync();
                _refreshWorker.Dispose();
                _refreshWorker = null;

                return;
            }

            if (_refreshWorker != null && _refreshWorker.IsBusy)
            {
                _refreshWorker.CancelAsync();
                _refreshWorker.Dispose();
                _refreshWorker = null;
            }

            _refreshWorker = new BackgroundWorker() { WorkerSupportsCancellation = true };
            _refreshWorker.DoWork += AutoRefreshDoWork;
            _refreshWorker.RunWorkerCompleted += AutoRefreshDoWorkCompleted;

            if (_searchWorker != null && _searchWorker.IsBusy)
            {
                _searchWorker.CancelAsync();
            }

            _refreshWorker.RunWorkerAsync();
        }

        private void AutoRefreshDoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                ReloadData();
            }
            catch (Exception ex)
            {
                e.Result = ex;
            }
        }

        private void AutoRefreshDoWorkCompleted(object sender, RunWorkerCompletedEventArgs e)
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
                    if (_currentSearch == ADVANCE_SEARCH)
                    {
                        RunAdvanceSearch();
                    }
                    else
                    {
                        SearchByBatch(_currentSearch);
                    }
                }
                catch (Exception ex)
                {
                    ProcessHelper.ProcessException(ex);
                }
                finally
                {
                    IsProcessing = false;
                    PostponeRefresh();
                }
            }
        }

        private void SearchByBatch(TaskMenuItem item)
        {
            IsProcessing = true;

            if (_searchWorker != null && _searchWorker.IsBusy)
            {
                _searchWorker.CancelAsync();
                _searchWorker.Dispose();
                _searchWorker = null;
            }

            if (_searchWorker == null)
            {
                _searchWorker = new BackgroundWorker() { WorkerSupportsCancellation = true };
                _searchWorker.DoWork += DoSearchByBatch;
                _searchWorker.RunWorkerCompleted += DoSearchByBatchCompleted;
            }

            _searchWorker.RunWorkerAsync(item);
        }

        private void SearchByBatch(string currentSearch)
        {
            IsProcessing = true;
            if (_searchWorker != null && _searchWorker.IsBusy)
            {
                _searchWorker.CancelAsync();
                _searchWorker.Dispose();
                _searchWorker = null;
            }

            if (_searchWorker == null)
            {
                _searchWorker = new BackgroundWorker() { WorkerSupportsCancellation = true };
                _searchWorker.DoWork += DoSearchByBatch;
                _searchWorker.RunWorkerCompleted += DoSearchByBatchCompleted;
            }

            _searchWorker.RunWorkerAsync(currentSearch);
        }

        private void DoSearchByBatch(object sender, DoWorkEventArgs e)
        {
            try
            {
                BackgroundWorker worker = sender as BackgroundWorker;
                CheckBackgroundWorkerStatus(worker, e);
                if (!e.Cancel)
                {
                    //WorkItemSearchResultModel result = _workItemProvider.RunAdvanceSearch(1, SelectedBatchType.Id, new SearchQueryModel { SearchQueryExpressions = GetSearchExpressions() });
                    if (e.Argument is TaskMenuItem)
                    {
                        TaskMenuItem item = e.Argument as TaskMenuItem;
                        switch (item.Type)
                        {
                            case TaskType.Available:
                                _currentSearch = Common.BATCH_WAITING;
                                e.Result = _workItemProvider.GetWaitingBatchs(item.BatchType.Id, 0);
                                break;
                            case TaskType.Error:
                                _currentSearch = Common.BATCH_ERROR;
                                e.Result = _workItemProvider.GetErrorBatchs(item.BatchType.Id, 0);
                                break;
                            case TaskType.InProcessing:
                                _currentSearch = Common.BATCH_IN_PROCESSING;
                                e.Result = _workItemProvider.GetInProcessingBatch(item.BatchType.Id, 0);
                                break;
                            case TaskType.Locked:
                                _currentSearch = Common.BATCH_LOCKED;
                                e.Result = _workItemProvider.GetLockedBatchs(item.BatchType.Id, 0);
                                break;
                            case TaskType.Rejected:
                                _currentSearch = Common.BATCH_REJECTED;
                                e.Result = _workItemProvider.GetRejectedBatchs(item.BatchType.Id, 0);
                                break;
                        }
                    }
                    else
                    {
                        string searchName = e.Argument.ToString();
                        switch (searchName)
                        {
                            case Common.BATCH_WAITING:
                                e.Result = _workItemProvider.GetWaitingBatchs(SelectedBatchType.Id, 0);
                                break;
                            case Common.BATCH_ERROR:
                                e.Result = _workItemProvider.GetErrorBatchs(SelectedBatchType.Id, 0);
                                break;
                            case Common.BATCH_IN_PROCESSING:
                                _currentSearch = Common.BATCH_IN_PROCESSING;
                                e.Result = _workItemProvider.GetInProcessingBatch(SelectedBatchType.Id, 0);
                                break;
                            case Common.BATCH_LOCKED:
                                _currentSearch = Common.BATCH_LOCKED;
                                e.Result = _workItemProvider.GetLockedBatchs(SelectedBatchType.Id, 0);
                                break;
                        }
                    }

                    CheckBackgroundWorkerStatus(worker, e);
                }
            }
            catch (Exception ex)
            {
                e.Result = ex;
            }

        }

        private void DoSearchByBatchCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (!e.Cancelled)
            {
                if (e.Result is Exception)
                {
                    IsProcessing = false;
                    ProcessHelper.ProcessException(e.Result as Exception);
                }
                else
                {
                    WorkItems.SearchResult = e.Result as WorkItemSearchResultModel;
                }
            }
            IsProcessing = false;
        }

        private string BuildSearchExpression(ObservableCollection<SearchExpressionViewModel> expressions)
        {
            return ProcessHelper.BuildSearchExpression(expressions);
        }

        private void CheckBackgroundWorkerStatus(BackgroundWorker worker, DoWorkEventArgs e)
        {
            if (worker.CancellationPending)
            {
                e.Cancel = true;
            }
        }

        #endregion
    }
}
