using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ecm.Mvvm;
using System.Data;
using Ecm.Model.DataProvider;
using Ecm.Model;
using System.Collections.ObjectModel;
using Ecm.Audit.Model;
using System.Windows.Input;

namespace Ecm.Audit.ViewModel
{
    public class ReportViewModel : ComponentViewModel
    {
        private ActionLogProvider _actionLogProvider = new ActionLogProvider();
        private ObservableCollection<SearchExpressionViewModel> _searchModels = new ObservableCollection<SearchExpressionViewModel>();
        private RelayCommand _displayReportCommand;

        public ReportViewModel()
        {
            LoadMenu();
        }

        public ObservableCollection<string> MenuItems { get; set; }
        
        public ObservableCollection<ReportMenu> MenuModels { get; set; }

        public DataTable ReportDataSource { get; set; }
        
        public ObservableCollection<SearchExpressionViewModel> Searchs
        {
            get { return _searchModels; }
            set
            {
                _searchModels = value;
                OnPropertyChanged("Searchs");
            }
        }

        public ICommand DisplayReportCommand
        {
            get
            {
                if (_displayReportCommand == null)
                {
                    _displayReportCommand = new RelayCommand(p => DisplayReport(p), p => CanDisplayReport());
                }

                return _displayReportCommand;
            }
        }

        //Public methods
        public DataTable GetActionLogDataSource()
        {
            List<ActionLogModel> actionLogs = new List<ActionLogModel>();
            string format = "yyyy-MM-dd HH:mm:ss";
            string date1 = Convert.ToDateTime(DateTime.Today.ToShortDateString() + " 00:00:00").ToString(format);
            string date2 = Convert.ToDateTime(DateTime.Today.ToShortDateString() + " 23:59:59").ToString(format);
            string expression = string.Empty;
            expression = "LoggedDate >= '" + date1 + "' AND LoggedDate <= '" + date2 + "' ";

            actionLogs = _actionLogProvider.SearchActionLog(expression);
            
            return BuildActionLogTable(actionLogs);
        }

        private DataTable SearchActionLog(string expression)
        {
            List<ActionLogModel> actionLogs = new List<ActionLogModel>();
            actionLogs = _actionLogProvider.SearchActionLog(expression);

            return BuildActionLogTable(actionLogs);
        }

        public void BuildParameterPane(string reportName)
        {
            Searchs.Clear();
            SearchExpressionViewModel searchViewModel = new SearchExpressionViewModel();

            switch (reportName)
            {
                case Common.REPORT_ACTION_LOG:
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

                    break;

            }
        }
        
        //private methods

        private DataTable BuildActionLogTable(List<ActionLogModel> actionLogs)
        {
            ReportData.ActionLogDataTable data = new ReportData.ActionLogDataTable();
            if (actionLogs == null)
            {
                return data;
            }

            foreach (var actionLog in actionLogs)
            {
                ReportData.ActionLogRow row = data.NewActionLogRow();

                row[data.IdColumn] = actionLog.Id;
                row[data.ActionNameColumn] = actionLog.ActionName;
                row[data.LoggedDateColumn] = actionLog.LoggedDate;
                row[data.IpAddressColumn] = actionLog.IpAddress;
                row[data.MessageColumn] = actionLog.Message;
                row[data.ObjectIdColumn] = actionLog.ObjectId;
                row[data.ObjectTypeColumn] = actionLog.ObjectType;
                row[data.UsernameColumn] = actionLog.Username;

                data.Rows.Add(row);
            }

            return data;
        }

        private void LoadMenu()
        {
            MenuModels = new ObservableCollection<ReportMenu>();
            MenuModels.Add(new ReportMenu { ImagePath = @"../Resources/Images/report22.png", MenuName = Common.REPORT_ACTION_LOG, MenuText = "Action log" });
            //MenuModels.Add(new ReportMenu { ImagePath = @"../Resources/Images/document.png", MenuName = Common.REPORT_DOCUMENT, MenuText = "Document version" });
            //MenuItems = new ObservableCollection<string>();
            //MenuItems.Add("Action log");
            //MenuItems.Add("Document");
            //MenuItems.Add("Page");
            //MenuItems.Add("Other");
        }

        private bool CanDisplayReport()
        {
            return Searchs != null && Searchs.Count > 0;
        }

        private void DisplayReport(object reportName)
        {
            string expression = ProcessHelper.BuildSearchExpression2(Searchs);

            if (reportName == null)
            {
                return;
            }

            switch (reportName.ToString())
            {
                case Common.REPORT_ACTION_LOG:
                    ReportDataSource = SearchActionLog(expression);
                    break;
            }
        }
    }
}
