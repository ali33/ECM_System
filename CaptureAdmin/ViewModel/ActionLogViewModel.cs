using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ecm.Mvvm;
using System.Collections.ObjectModel;
using Ecm.CaptureModel.DataProvider;
using Ecm.CaptureModel;
using System.Windows.Input;
using System.ComponentModel;
using Ecm.CaptureAdmin.Model;
using System.Resources;
using System.Reflection;

namespace Ecm.CaptureAdmin.ViewModel
{
    public class ActionLogViewModel : ComponentViewModel
    {
        private readonly ResourceManager _resource = new ResourceManager("Ecm.CaptureAdmin.Resources", Assembly.GetExecutingAssembly());

        private ObservableCollection<SearchExpressionViewModel> _searchModels = new ObservableCollection<SearchExpressionViewModel>();
        private ActionLogModel _actionLog;
        private ActionLogProvider _actionLogProvider = new ActionLogProvider();
        private PermissionProvider _permissionProvider = new PermissionProvider();
        private int _currentIndex;
        private int _numberRow;
        private long _totalRow;
        private bool _isSelectedAll;
        private string _globalTextSearch;
        private ObservableCollection<ActionLogModel> _actionLogs = new ObservableCollection<ActionLogModel>();

        private RelayCommand _advanceSearchCommand;
        private RelayCommand _showNextCommand;
        private RelayCommand _showPreviousCommand;
        private RelayCommand _deleteLogCommand;

        public ActionLogViewModel()
        {
            _numberRow = new SettingProvider().GetSettings().SearchResultPageSize;
            BuildSearchPane();
            if (_numberRow == 0)
            {
                _numberRow = 100;
            }

            LoadData();
        }

        public string Expression { get; set; }
        
        public bool IsSelectedAll
        {
            get { return _isSelectedAll; }
            set
            {
                _isSelectedAll = value;
                OnPropertyChanged("IsSelectedAll");

                if (value)
                {
                    foreach (var action in ActionLogs)
                    {
                        action.IsSelected = true;
                    }
                }
                else
                {
                    foreach (var action in ActionLogs)
                    {
                        action.IsSelected = false;
                    }
                }
            }
        }

        public ObservableCollection<SearchExpressionViewModel> Searchs
        {
            get { return _searchModels; }
            set
            {
                _searchModels = value;
                OnPropertyChanged("Searchs");
            }
        }

        public ObservableCollection<ActionLogModel> ActionLogs
        {
            get { return _actionLogs; }
            set
            {
                _actionLogs = value;
                OnPropertyChanged("ActionLogs");
            }
        }

        public ActionLogModel ActionLog
        {
            get { return _actionLog; }
            set
            {
                _actionLog = value;
                OnPropertyChanged("ActionLog");
            }
        }

        public ICommand AdvanceSearchCommand
        {
            get
            {
                if (_advanceSearchCommand == null)
                {
                    _advanceSearchCommand = new RelayCommand(p => RunSearch(), p => CanSearch());
                }
                return _advanceSearchCommand;
            }
        }

        public ICommand ShowNextCommand
        {
            get
            {
                if (_showNextCommand == null)
                {
                    _showNextCommand = new RelayCommand(p => ShowNextPage(), p => CanNextPage());
                }

                return _showNextCommand;
            }
        }

        public ICommand ShowPreviousCommand
        {
            get
            {
                if (_showPreviousCommand == null)
                {
                    _showPreviousCommand = new RelayCommand(p => ShowPreviousPage(), p => CanPreviousPage());
                }
                return _showPreviousCommand;
            }
        }

        public ICommand DeleteLogCommand
        {
            get
            {
                if (_deleteLogCommand == null)
                {
                    _deleteLogCommand = new RelayCommand(p => DeleteActionLog(), p => CanDeleteActionLog());
                }

                return _deleteLogCommand;
            }
        }
        //Public methods

        public void RunGlobalSearch(string text)
        {
            _globalTextSearch = text;
            _currentIndex = 1;
            IsProcessing = true;
            BackgroundWorker worker = new BackgroundWorker();
            worker.DoWork += new DoWorkEventHandler(RunGlobalSearch);
            worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(RunGlobalSearchCompleted);
            worker.RunWorkerAsync();
        }

        void RunGlobalSearchCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            IsProcessing = false;
            if (e.Result is Exception)
            {
                ProcessHelper.ProcessException(e.Result as Exception);
            }

            _globalTextSearch = string.Empty;
        }

        void RunGlobalSearch(object sender, DoWorkEventArgs e)
        {
            _currentIndex = 0;
            string expression = string.Empty;
            DateTime dateValue = DateTime.Now;
            int intValue = 0;

            bool isDate = DateTime.TryParse(_globalTextSearch, out dateValue);
            bool isNum = int.TryParse(_globalTextSearch, out intValue);

            expression = "Username like '%" + _globalTextSearch + "%' OR ActionName like '%" + _globalTextSearch + "%' OR IpAddress like '%" + _globalTextSearch + "%' OR Message like '%" + _globalTextSearch + "%' OR ObjectType like '%" + _globalTextSearch + "%'";

            if (isDate)
            {
                string format = "yyyy-MM-dd HH:mm:ss";
                string date1 = Convert.ToDateTime(dateValue.ToShortDateString() + " 00:00:00").ToString(format);
                string date2 = Convert.ToDateTime(dateValue.ToShortDateString() + " 23:59:59").ToString(format);
                expression += " OR (LoggedDate >= '" + date1 + "' AND LoggedDate < '" + date2 + "')";
            }

            if (isNum)
            {
                expression += " OR ObjectID = " + intValue.ToString();
            }

            Expression = expression;
            try
            {
                ActionLogs = new ObservableCollection<ActionLogModel>(_actionLogProvider.SearchActionLog(Expression, _currentIndex, _numberRow, out _totalRow));
            }
            catch (Exception ex)
            {
                e.Result = ex;
            }
        }

        //Private methods

        private bool CanDeleteActionLog()
        {
            return ActionLogs != null && ActionLog != null && (ActionLog.IsSelected || ActionLogs.Any(a => a.IsSelected));
        }

        private void DeleteActionLog()
        {
            if (DialogService.ShowTwoStateConfirmDialog(_resource.GetString("uiConfirmDelete")) == DialogServiceResult.No)
            {
                return;
            }

            IsProcessing = true;
            BackgroundWorker worker = new BackgroundWorker();
            worker.DoWork += new DoWorkEventHandler(DoDeleteActionLog);
            worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(RunDeletteActionLogCompleted);
            worker.RunWorkerAsync();

        }

        void RunDeletteActionLogCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Result is Exception)
            {
                ProcessHelper.ProcessException(e.Result as Exception);
            }
            else
            {
                LoadData();
            }

            IsProcessing = false;
        }

        void DoDeleteActionLog(object sender, DoWorkEventArgs e)
        {
            try
            {
                foreach (var actionLog in ActionLogs)
                {
                    if (actionLog.IsSelected)
                    {
                        _actionLogProvider.DeleteLog(actionLog.Id);
                    }
                }

            }
            catch (Exception ex)
            {
                e.Result = ex;
            }
        }

        private bool CanPreviousPage()
        {
            return _totalRow > 0 && _currentIndex > 0;
        }

        private void ShowPreviousPage()
        {
            _currentIndex--;
            GetActionLogs(_currentIndex);
        }
        private bool CanNextPage()
        {
            return _totalRow > 0 && ((_currentIndex + 1) * _numberRow) < _totalRow;
        }

        private void ShowNextPage()
        {
            _currentIndex++;
            GetActionLogs(_currentIndex);
        }
        private bool CanSearch()
        {
            return Searchs != null && Searchs.Count > 0;
        }

        private void RunSearch()
        {
            ActionLogs.Clear();
            Expression = ProcessHelper.BuildSearchExpression(Searchs);
            //Dictionary<string,object> values = GetParameterValues();
            try
            {
                ActionLogs = new ObservableCollection<ActionLogModel>(_actionLogProvider.SearchActionLog(Expression, _currentIndex, _numberRow, out _totalRow));
            }
            catch (Exception ex)
            {
                ProcessHelper.ProcessException(ex);
            }
        }

        private void LoadData()
        {
            GetActionLogs(_currentIndex);
        }

        private ObservableCollection<ActionLogModel> GetActionLogs(int index)
        {
            try
            {
                return ActionLogs = new ObservableCollection<ActionLogModel>(_actionLogProvider.SearchActionLog(Expression, _currentIndex, _numberRow, out _totalRow));
            }
            catch (Exception ex)
            {
                ProcessHelper.ProcessException(ex);
            }

            return null;
        }


        private void BuildSearchPane()
        {
            Searchs.Clear();
            SearchExpressionViewModel searchViewModel = new SearchExpressionViewModel();

            SearchModel searchModel = new SearchModel();
            searchModel.Name = "Username";
            searchModel.DataType = "String";
            searchViewModel.Search = searchModel;

            Searchs.Add(searchViewModel);

            searchViewModel = new SearchExpressionViewModel();
            searchModel = new SearchModel();
            searchModel.DataType = "Date";
            searchModel.Name = "Logged Date";
            searchModel.Condition = Common.AND;
            searchViewModel.Search = searchModel;

            Searchs.Add(searchViewModel);

            searchViewModel = new SearchExpressionViewModel();
            searchModel = new SearchModel();
            searchModel.Name = "Action Name";
            searchModel.DataType = "String";
            searchModel.Condition = Common.AND;
            searchViewModel.Search = searchModel;

            Searchs.Add(searchViewModel);
        }

    }
}
