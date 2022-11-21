using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ecm.Mvvm;
using Ecm.Workflow.Activities.CustomActivityModel;
using System.Collections.ObjectModel;
using Ecm.CaptureDomain;
using Ecm.Workflow.Activities.CustomActivityModel.DataProvider;
using Ecm.LookupDomain;
using System.Data;
using System.Resources;
using System.Reflection;
using System.Windows.Input;
using Ecm.Workflow.Activities.LookupConfiguration.View;

namespace Ecm.Workflow.Activities.LookupConfiguration.ViewModel
{
    public class LookupConfigurationViewModel : ComponentViewModel
    {
        private Dictionary<ProviderType, string> _dataProviders;
        private Dictionary<LookupType, string> _lookupTypes;
        private Dictionary<DatabaseType, string> _databaseTypes = new Dictionary<DatabaseType, string>();
        private bool _isTable = true;
        private bool _isView;
        private bool _isStored;
        private ObservableCollection<string> _sources = new ObservableCollection<string>();
        private string _selectedSource;
        private KeyValuePair<string, string>? _lookupColumn;
        private ObservableCollection<ParameterModel> _parameters;
        private KeyValuePair<string, SearchOperator>? _selectedOperator;
        private Dictionary<string, SearchOperator> _operators = new Dictionary<string, SearchOperator>();
        private KeyValuePair<string, SearchOperator>? _selectedOperatorForStored;
        private Dictionary<string, SearchOperator> _operatorsForStored = new Dictionary<string, SearchOperator>();
        private bool _isSqlCommandView = true;
        private bool _isStoredProcedureView;
        private bool _isCommandAvailable;
        private bool _isMapAvailable;
        private readonly LookupProvider _lookupProvider = new LookupProvider();
        private ObservableCollection<string> _databaseNames = new ObservableCollection<string>();
        private ObservableCollection<string> _schemas = new ObservableCollection<string>();
        //private FieldModel _field = new FieldModel();
        private Dictionary<string, string> _lookupColumns = new Dictionary<string, string>();
        private ObservableCollection<FieldModel> _fields;
        private DatabaseType _selectedDatabaseType;
        private string _selectedDatabaseName;
        private DataTable _testData = new DataTable();
        private ProviderType _selectedProvider;
        private bool _isTestConnectionSuccess;
        private bool _isTestCommandSuccess;
        private LookupInfoModel _lookupInfo;
        private string _sqlCommand;
        private string _whereClause;
        private string _openChar;
        private string _closeChar;
        private string _valueChar;

        private RelayCommand _saveCommand;
        private RelayCommand _testConnectionCommand;
        private RelayCommand _buildSqlCommand;
        private RelayCommand _addConditionCommand;
        private RelayCommand _clearConditionCommand;
        private RelayCommand _testSqlCommand;
        private readonly ResourceManager _resource = new ResourceManager("Ecm.Workflow.Activities.LookupConfiguration.Resource", Assembly.GetExecutingAssembly());



        public LookupConfigurationViewModel(LookupInfoModel lookupInfo, ObservableCollection<FieldModel> fields, bool isEdit)
        {
            _fields = fields;
            if (lookupInfo == null)
            {
                LookupInfo = new LookupInfoModel();
            }
            else
            {
                LookupInfo = lookupInfo;
            }

            IsStored = false;
            IsTable = false;
            IsView = false;
            switch ((LookupDataSourceType)LookupInfo.LookupType)
            {
                case LookupDataSourceType.Table:
                    IsTable = true;
                    break;
                case LookupDataSourceType.View: 
                    IsView = true;
                    break;
                case LookupDataSourceType.StoredProcedure:
                    IsStored = true;
                    break;
                default:
                    IsTable = true;
                    break;
            }

            LoadDataProvider();
            LoadDatabaseType();
            IsEditMode = isEdit;

            if (IsEditMode)
            {
                if (LookupInfo.FieldMappings != null && LookupInfo.FieldMappings.Count > 0)
                {
                    TestConnection(isEdit);
                    LoadLookupColumn();
                    LoadIndexMapping();
                    if (IsStored)
                    {
                        LoadOperatorSourceForStored();
                    }
                    else
                    {
                        LoadOperatorSource();
                    }
                }

                if (LookupInfo != null && LookupInfo.LookupType == (int)LookupType.Stored)
                    LoadParameters(LookupInfo.SourceName);

                switch (LookupInfo.LookupType)
                {
                    case (int)LookupType.Stored:
                        IsStored = true;
                        IsTable = false;
                        IsView = false;
                        break;
                    case (int)LookupType.Table:
                        IsStored = false;
                        IsView = false;
                        IsTable = true;
                        break;
                    case (int)LookupType.View:
                        IsStored = false;
                        IsView = true;
                        IsTable = false;
                        break;

                }
                LoadDatabase();
            }
        }

        public ObservableCollection<FieldModel> Fields
        {
            get { return _fields; }
            set
            {
                _fields = value;
                OnPropertyChanged("Fields");
            }
        }

        public Action<LookupInfoModel> SaveLookupComplete { get; set; }

        public event CloseDialog CloseDialog;

        public ObservableCollection<ParameterModel> Parameters
        {
            get { return _parameters; }
            set
            {
                _parameters = value;
                OnPropertyChanged("Parameters");
            }
        }

        public Dictionary<string, SearchOperator> Operators
        {
            get { return _operators; }
            set
            {
                _operators = value;
                OnPropertyChanged("Operators");
            }
        }

        public KeyValuePair<string, SearchOperator>? SelectedOperator
        {
            get { return _selectedOperator; }
            set
            {
                _selectedOperator = value;
                OnPropertyChanged("SelectedOperator");

                if (value != null)
                {
                    LookupInfo.LookupOperator = value.Value.Value.ToString();
                }
            }
        }

        public Dictionary<string, SearchOperator> OperatorsForStored
        {
            get { return _operatorsForStored; }
            set
            {
                _operatorsForStored = value;
                OnPropertyChanged("OperatorsForStored");
            }
        }

        public KeyValuePair<string, SearchOperator>? SelectedOperatorForStored
        {
            get { return _selectedOperatorForStored; }
            set
            {
                _selectedOperatorForStored = value;
                OnPropertyChanged("SelectedOperatorForStored");

                if (value != null)
                {
                    LookupInfo.LookupOperator = value.Value.Value.ToString();
                }
            }
        }

        public ObservableCollection<string> DatabaseNames
        {
            get { return _databaseNames; }
            set
            {
                _databaseNames = value;
                OnPropertyChanged("DatabaseNames");
            }
        }

        public bool IsStoredProcedureView
        {
            get { return _isStoredProcedureView; }
            set
            {
                _isStoredProcedureView = value;
                OnPropertyChanged("IsStoredProcedureView");
            }
        }

        public bool IsSqlCommandView
        {
            get { return _isSqlCommandView; }
            set
            {
                _isSqlCommandView = value;
                OnPropertyChanged("IsSqlCommandView");
            }
        }

        public ObservableCollection<string> SourceNames
        {
            get { return _sources; }
            set
            {
                _sources = value;
                OnPropertyChanged("SourceNames");
            }
        }

        public string SelectedSourceName
        {
            get { return _selectedSource; }
            set
            {
                _selectedSource = value;
                OnPropertyChanged("SelectedSourceName");
            }
        }

        public Dictionary<string, string> LookupColumns
        {
            get { return _lookupColumns; }
            set
            {
                _lookupColumns = value;
                OnPropertyChanged("LookupColumns");
            }
        }

        public KeyValuePair<string, string>? LookupColumn
        {
            get { return _lookupColumn; }
            set
            {
                _lookupColumn = value;

                OnPropertyChanged("LookupColumn");

                if (value != null)
                {
                    LookupInfo.LookupColumn = value.Value.Key;

                    if (IsStored)
                    {
                        LoadOperatorSourceForStored();
                    }
                    else
                    {
                        LoadOperatorSource();
                    }
                }
            }
        }

        public Dictionary<ProviderType, string> DataProviders
        {
            get { return _dataProviders; }
            set
            {
                _dataProviders = value;
                OnPropertyChanged("DataProviders");
            }
        }

        public Dictionary<LookupType, string> LookupTypes
        {
            get { return _lookupTypes; }
            set
            {
                _lookupTypes = value;
                OnPropertyChanged("LookupTypes");

            }
        }

        public Dictionary<DatabaseType, string> DatabaseTypes
        {
            get { return _databaseTypes; }
            set
            {
                _databaseTypes = value;
                OnPropertyChanged("DatabaseTypes");
            }
        }

        public ObservableCollection<string> Schemas
        {
            get { return _schemas; }
            set
            {
                _schemas = value;
                OnPropertyChanged("Schemas");
            }
        }

        public DatabaseType SelectedDatabaseType
        {
            get { return _selectedDatabaseType; }
            set
            {
                _selectedDatabaseType = value;
                OnPropertyChanged("SelectedDatabaseType");
                switch (value)
                {
                    case DatabaseType.DB2:
                        LookupInfo.Connection.Port = 50000;
                        break;
                    case DatabaseType.MsSql:
                        LookupInfo.Connection.Port = 1433;
                        break;
                    case DatabaseType.MySql:
                        LookupInfo.Connection.Port = 3306;
                        break;
                    case DatabaseType.Oracle:
                        LookupInfo.Connection.Port = 1521;
                        break;
                    case DatabaseType.PostgreSql:
                        LookupInfo.Connection.Port = 5432;
                        break;
                }

                LookupInfo.Connection.DatabaseType = (int)value;
            }
        }

        public ProviderType SelectedProvider
        {
            get { return _selectedProvider; }
            set
            {
                _selectedProvider = value;
                OnPropertyChanged("SelectedProvider");

                if (value != null)
                {
                    LookupInfo.Connection.ProviderType = (int)value;
                }
            }
        }

        public string SelectedDatabaseName
        {
            get { return _selectedDatabaseName; }
            set
            {
                _selectedDatabaseName = value;
                OnPropertyChanged("SelectedDatabaseName");
            }
        }

        public DataTable TestData
        {
            get { return _testData; }
            set
            {
                _testData = value;
                OnPropertyChanged("TestData");
            }
        }

        public LookupInfoModel LookupInfo
        {
            get { return _lookupInfo; }
            set
            {
                _lookupInfo = value;
                OnPropertyChanged("LookupInfo");
            }
        }

        public bool IsCommandAvailable
        {
            get { return _isCommandAvailable; /*return IsView || IsStored || IsTable;*/ }
            set
            {
                _isCommandAvailable = value;
                OnPropertyChanged("IsCommandAvailable");
            }
        }

        public bool IsMapAvailable
        {
            get { return _isMapAvailable; }
            set
            {
                _isMapAvailable = value;
                OnPropertyChanged("IsMapAvailable");
            }
        }

        public LookupDataSourceType LookupSource
        {
            get
            {
                if (IsStored)
                {
                    return LookupDataSourceType.StoredProcedure;
                }
                else if (IsTable)
                {
                    return LookupDataSourceType.Table;
                }
                else
                {
                    return LookupDataSourceType.View;
                }
            }
        }

        public bool IsTable
        {
            get { return _isTable; }
            set
            {
                _isTable = value;
                OnPropertyChanged("IsTable");
                if (value)
                {
                    LookupInfo.LookupType = (int)LookupType.Table;
                    IsSqlCommandView = value;
                    IsStoredProcedureView = !value;
                    //IsCommandAvailable = value;
                    LoadDataSource(LookupDataSourceType.Table);
                }
            }
        }

        public bool IsView
        {
            get { return _isView; }
            set
            {
                _isView = value;
                OnPropertyChanged("IsView");
                if (value)
                {
                    LookupInfo.LookupType = (int)LookupType.View;
                    IsSqlCommandView = value;
                    IsStoredProcedureView = !value;
                    //IsCommandAvailable = true;
                    LoadDataSource(LookupDataSourceType.View);
                }

            }
        }

        public bool IsStored
        {
            get { return _isStored; }
            set
            {
                _isStored = value;
                OnPropertyChanged("IsStored");
                if (value)
                {
                    LookupInfo.LookupType = (int)LookupType.Stored;
                    IsSqlCommandView = !value;
                    IsStoredProcedureView = value;
                    //IsCommandAvailable = true;
                    LoadDataSource(LookupDataSourceType.StoredProcedure);
                }

            }
        }

        public bool IsEditMode { get; set; }

        public bool IsTestConnectionSuccess
        {
            get { return _isTestConnectionSuccess; }
            set
            {
                _isTestConnectionSuccess = value;
                OnPropertyChanged("IsTestConnectionSuccess");
            }
        }

        public ICommand SaveCommand
        {
            get
            {
                if (_saveCommand == null)
                {
                    _saveCommand = new RelayCommand(p => Save(), p => CanSave());
                }

                return _saveCommand;
            }
        }

        public ICommand TestConnectionCommand
        {
            get
            {
                if (_testConnectionCommand == null)
                {
                    _testConnectionCommand = new RelayCommand(p => TestConnection(), p => CanTestConnection());
                }

                return _testConnectionCommand;
            }
        }

        public ICommand BuildSqlCommand
        {
            get
            {
                if (_buildSqlCommand == null)
                {
                    _buildSqlCommand = new RelayCommand(p => BuildExecuteCommand(), p => CanBuildSql());
                }

                return _buildSqlCommand;
            }
        }

        public ICommand AddConditionCommand
        {
            get
            {
                if (_addConditionCommand == null)
                {
                    _addConditionCommand = new RelayCommand(p => AddWhereClause(), p => CanAddCondition());
                }

                return _addConditionCommand;
            }
        }

        public ICommand ClearConditionCommand
        {
            get
            {
                if (_clearConditionCommand == null)
                {
                    _clearConditionCommand = new RelayCommand(p => ClearWhereClause(), p => CanClearCondition());
                }

                return _clearConditionCommand;
            }
        }

        public ICommand TestSqlCommand
        {
            get
            {
                if (_testSqlCommand == null)
                {
                    _testSqlCommand = new RelayCommand(p => TestSqlData(), p => CanTestSql());
                }

                return _testSqlCommand;
            }
        }

        //Public methods

        public void LoadDataSource(LookupDataSourceType type)
        {
            try
            {
                if (LookupInfo.Connection == null
                    || string.IsNullOrWhiteSpace(LookupInfo.Connection.Host))
                {
                    return;
                }

                //SourceNames.Clear();
                SourceNames = new ObservableCollection<string>(_lookupProvider.GetDataSources(LookupInfo.Connection, type));
            }
            catch (Exception ex)
            {
                ProcessHelper.ProcessException(ex);
            }
        }

        //public void LoadOperatorSource()
        //{
        //    Dictionary<string, SearchOperator> operators = new Dictionary<string, SearchOperator>();
        //    operators.Add("Equal", SearchOperator.Equal);
        //    operators.Add("Not equal", SearchOperator.NotEqual);
        //    operators.Add("Greater than", SearchOperator.GreaterThan);
        //    operators.Add("Less than", SearchOperator.LessThan);
        //    operators.Add("Greater than or equal", SearchOperator.GreaterThanOrEqualTo);
        //    operators.Add("Less than or equal", SearchOperator.LessThanOrEqualTo);
        //    operators.Add("Like", SearchOperator.Contains);

        //    Operators = operators;
        //}
        public void LoadOperatorSource()
        {
            Dictionary<string, SearchOperator> operators = new Dictionary<string, SearchOperator>();

            if (LookupColumn != null)
            {
                switch (LookupColumn.Value.Value)
                {
                    case "bit":
                    case "bool":
                    case "boolean":
                        operators.Add(Common.EQUAL, SearchOperator.Equal);
                        operators.Add(Common.NOT_EQUAL, SearchOperator.NotEqual);
                        break;
                    case "int":
                    case "decimal":
                    case "tinyint":
                    case "bigint":
                    case "float":
                    case "date":
                    case "datetime":
                    case "real":
                    case "double precision":
                    case "numeric":
                    case "number":
                    case "integer":
                    case "double":
                    case "smallint":
                        operators.Add(Common.EQUAL, SearchOperator.Equal);
                        operators.Add(Common.NOT_EQUAL, SearchOperator.NotEqual);
                        operators.Add(Common.GREATER_THAN, SearchOperator.GreaterThan);
                        operators.Add(Common.LESS_THAN, SearchOperator.LessThan);
                        operators.Add(Common.GREATER_THAN_OR_EQUAL_TO, SearchOperator.GreaterThanOrEqualTo);
                        operators.Add(Common.LESS_THAN_OR_EQUAL_TO, SearchOperator.LessThanOrEqualTo);
                        break;
                    case "char":
                    case "varchar":
                    case "nvarchar":
                    case "text":
                    case "ntext":
                    case "uniqueidentifier":
                    case "character varying":
                    case "varchar2":
                    case "nvarchar2":
                    case "longtext":
                    case "mediumtext":
                    case "tinytext":
                        operators.Add(Common.EQUAL, SearchOperator.Equal);
                        operators.Add(Common.NOT_EQUAL, SearchOperator.NotEqual);
                        operators.Add(Common.STARTS_WITH, SearchOperator.StartsWith);
                        operators.Add(Common.ENDS_WITH, SearchOperator.EndsWith);
                        operators.Add(Common.NOT_CONTAINS, SearchOperator.NotContains);
                        operators.Add(Common.CONTAINS, SearchOperator.Contains);
                        break;
                }
                Operators = operators;

                SelectedOperator = Operators.SingleOrDefault(p => p.Value.ToString() == LookupInfo.LookupOperator);
            }
        }

        public void LoadOperatorSourceForStored()
        {
            Dictionary<string, SearchOperator> operators = new Dictionary<string, SearchOperator>();

            if (LookupColumn != null && LookupColumn.Value.Value != null)
            {
                switch (LookupColumn.Value.Value.ToLower())
                {
                    case "bool":
                    case "boolean":
                        operators.Add(Common.EQUAL, SearchOperator.Equal);
                        operators.Add(Common.NOT_EQUAL, SearchOperator.NotEqual);
                        break;
                    case "int16":
                    case "int32":
                    case "int64":
                    case "decimal":
                    case "double":
                    case "date":
                    case "datetime":
                        operators.Add(Common.EQUAL, SearchOperator.Equal);
                        operators.Add(Common.NOT_EQUAL, SearchOperator.NotEqual);
                        operators.Add(Common.GREATER_THAN, SearchOperator.GreaterThan);
                        operators.Add(Common.LESS_THAN, SearchOperator.LessThan);
                        operators.Add(Common.GREATER_THAN_OR_EQUAL_TO, SearchOperator.GreaterThanOrEqualTo);
                        operators.Add(Common.LESS_THAN_OR_EQUAL_TO, SearchOperator.LessThanOrEqualTo);
                        break;
                    case "string":
                        operators.Add(Common.EQUAL, SearchOperator.Equal);
                        operators.Add(Common.NOT_EQUAL, SearchOperator.NotEqual);
                        operators.Add(Common.STARTS_WITH, SearchOperator.StartsWith);
                        operators.Add(Common.ENDS_WITH, SearchOperator.EndsWith);
                        operators.Add(Common.NOT_CONTAINS, SearchOperator.NotContains);
                        operators.Add(Common.CONTAINS, SearchOperator.Contains);
                        break;
                }
                OperatorsForStored = operators;

                SelectedOperatorForStored = OperatorsForStored.SingleOrDefault(p => p.Value.ToString() == LookupInfo.LookupOperator);
            }
        }

        public void LoadIndexMapping()
        {
            if (LookupInfo.FieldMappings.Count == 0)
            {
                LookupInfo.FieldMappings = new ObservableCollection<LookupMappingModel>();
                ObservableCollection<LookupMappingModel> maps = new ObservableCollection<LookupMappingModel>();

                foreach (FieldModel field in _fields)
                {
                    LookupMappingModel map = new LookupMappingModel();

                    if (LookupInfo.FieldMappings != null)
                    {
                        map.DataColumn = LookupInfo.FieldMappings.FirstOrDefault(f => f.FieldId == field.Id) == null ? "" : LookupInfo.FieldMappings.FirstOrDefault(f => f.FieldId == field.Id).DataColumn;
                        map.IsChecked = false;
                        map.FieldName = field.Name;
                        map.FieldId = field.Id;
                        maps.Add(map);
                    }
                }

                LookupInfo.FieldMappings = maps;
            }
        }

        public void LoadLookupColumn()
        {
            try
            {
                //Dictionary<string, string> lookupColumnData = _lookupProvider.GetColumns(LookupInfo.Connection, LookupInfo.SourceName, LookupSource);
                //LookupColumns = new ObservableCollection<string>(lookupColumnData.Select(p => p.Key));
                var dic = new Dictionary<string, string>();
                dic.Add(string.Empty, string.Empty);
                var colums = _lookupProvider.GetColumns(LookupInfo.Connection, LookupInfo.SourceName, LookupSource);
                foreach (var item in colums)
                {
                    dic.Add(item.Key, item.Value);
                }

                LookupColumns = dic;
                LookupColumn = LookupColumns.SingleOrDefault(p => p.Key == LookupInfo.LookupColumn);
            }
            catch (Exception ex)
            {
                ProcessHelper.ProcessException(ex);
            }
        }

        public void LoadParameters(string storedName)
        {
            if (LookupInfo.Parameters == null || LookupInfo.Parameters.Count == 0)
            {
                DataTable dt = new DataTable();
                try
                {
                    dt = _lookupProvider.GetParameters(LookupInfo.Connection, storedName);

                    foreach (DataRow para in dt.Rows)
                    {
                        ParameterModel paraModel = new ParameterModel
                        {
                            Mode = para["Mode"].ToString(),
                            OrderIndex = para["OrderIndex"].ToString(),
                            ParameterName = para["Name"].ToString(),
                            ParameterType = para["DataType"].ToString(),
                        };

                        LookupInfo.Parameters.Add(paraModel);
                    }
                }

                catch (Exception ex)
                {
                    ProcessHelper.ProcessException(ex);
                }
            }
        }

        //Private methods
        private bool CanTestConnection()
        {
            return LookupInfo.Connection != null && !string.IsNullOrWhiteSpace(LookupInfo.Connection.Host) &&
                !string.IsNullOrWhiteSpace(LookupInfo.Connection.Username) && !string.IsNullOrWhiteSpace(LookupInfo.Connection.Password);
        }

        private void TestConnection()
        {
            TestConnection(false);
        }

        private bool CanBuildSql()
        {
            return LookupInfo.Parameters != null;
        }

        private bool CanAddCondition()
        {
            return LookupColumn != null && LookupColumn.Value.Key != null && SelectedOperator != null && SelectedOperator.Value.Key != null;
        }

        private bool CanClearCondition()
        {
            return !string.IsNullOrEmpty(_whereClause);
        }

        private bool CanTestSql()
        {
            return LookupInfo != null && LookupInfo.Connection != null && LookupInfo.SqlCommand != null && _isTestConnectionSuccess;
        }

        private void TestSqlData()
        {
            try
            {
                //if (LookupInfo.LookupType == (int)LookupType.Stored)
                //{
                //    TestData = _lookupProvider.GetLookupData(LookupInfo, "");
                //}
                //else
                //{
                TestLookupViewModel testViewModel = new TestLookupViewModel();
                TestLookupView testLookupView = new TestLookupView(testViewModel);
                DialogViewer dialog = new DialogViewer(testLookupView);
                testViewModel.Dialog = dialog;
                dialog.Width = 400;
                dialog.Height = 230;
                dialog.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
                dialog.MaximizeBox = false;
                dialog.MinimizeBox = false;
                dialog.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
                dialog.Text = _resource.GetString("uiDialogTitle");
                dialog.EnableToResize = false;

                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    TestData = _lookupProvider.GetLookupData(LookupInfo, testViewModel.Value);
                }
                //}
                _isTestCommandSuccess = true;
            }
            catch (Exception ex)
            {
                _isTestCommandSuccess = false;
                ProcessHelper.ProcessException(ex);
            }
        }

        private bool CanSave()
        {
            return LookupInfo != null && _isTestConnectionSuccess && LookupInfo.FieldMappings != null &&
                LookupInfo.FieldMappings.Count > 0 && !string.IsNullOrEmpty(LookupInfo.SqlCommand) &&
                !string.IsNullOrWhiteSpace(LookupInfo.SqlCommand) && (IsEditMode || _isTestCommandSuccess);
        }

        private void Save()
        {
            if (SaveLookupComplete != null)
            {
                SaveLookupComplete(LookupInfo);
            }
        }

        private void TestConnection(bool isEditMode)
        {
            var resource = new ResourceManager("Ecm.Workflow.Activities.LookupConfiguration.Resource", Assembly.GetExecutingAssembly());
            try
            {
                if (_lookupProvider.TestConnection(LookupInfo.Connection))
                {
                    IsTestConnectionSuccess = true;
                    IsMapAvailable = true;
                    IsCommandAvailable = true;

                    if (!isEditMode)
                    {
                        DialogService.ShowMessageDialog(resource.GetString("uiTestConnectionSuccessfully"));
                    }
                }
                else
                {
                    IsTestConnectionSuccess = false;
                    IsMapAvailable = false;
                    IsCommandAvailable = false;
                    if (!isEditMode)
                    {
                        DialogService.ShowErrorDialog(resource.GetString("uiTestConnectionFail"));
                    }
                    else
                    {
                        DialogService.ShowErrorDialog(resource.GetString("uiEditModeTestConnectionFail"));
                    }

                    DatabaseNames = null;
                }

                if (IsTestConnectionSuccess)
                {
                    LoadDatabase();
                    LoadSchemas();
                    LoadDataSource(LookupSource);
                }
            }
            catch (Exception ex)
            {
                ProcessHelper.ProcessException(ex);
                IsTestConnectionSuccess = false;
                IsMapAvailable = false;
                IsCommandAvailable = false;
            }
        }

        private void LoadDatabaseType()
        {
            DatabaseTypes = new Dictionary<DatabaseType, string>();
            DatabaseTypes.Add(DatabaseType.DB2, Common.IBM_DB2);
            DatabaseTypes.Add(DatabaseType.MySql, Common.MY_SQL);
            DatabaseTypes.Add(DatabaseType.Oracle, Common.ORACLE);
            DatabaseTypes.Add(DatabaseType.PostgreSql, Common.POSTGRE_SQL);
            DatabaseTypes.Add(DatabaseType.MsSql, Common.SQL_SERVER);

            if (LookupInfo == null && LookupInfo.Connection == null)
            {
                SelectedDatabaseType = DatabaseType.MsSql;
            }
            else
            {
                SelectedDatabaseType = (DatabaseType)LookupInfo.Connection.DatabaseType;
            }
        }

        private void LoadDataProvider()
        {
            DataProviders = new Dictionary<ProviderType, string>();
            DataProviders.Add(ProviderType.AdoNet, Common.ADO_NET);
            DataProviders.Add(ProviderType.OleDb, Common.OLEDB);

            if (LookupInfo == null && LookupInfo.Connection == null && LookupInfo.Connection.ProviderType != 0)
            {
                SelectedProvider = ProviderType.AdoNet;
            }
            else
            {
                SelectedProvider = (ProviderType)LookupInfo.Connection.ProviderType;
            }
        }

        private void LoadSchemas()
        {
            try
            {
                Schemas = new ObservableCollection<string>(_lookupProvider.GetSchemas(LookupInfo.Connection));

                if (string.IsNullOrEmpty(LookupInfo.Connection.Schema))
                {
                    LookupInfo.Connection.Schema = Schemas[0];
                }
            }
            catch (Exception ex)
            {
                ProcessHelper.ProcessException(ex);
            }
        }

        private void LoadDatabase()
        {
            try
            {
                DatabaseNames = new ObservableCollection<string>(_lookupProvider.GetDatabaseName(LookupInfo.Connection));

                if (string.IsNullOrEmpty(LookupInfo.Connection.DatabaseName))
                {
                    LookupInfo.Connection.DatabaseName = DatabaseNames[0];
                }
            }
            catch (Exception ex)
            {
                ProcessHelper.ProcessException(ex);
            }
        }

        private void BuildCommandText()
        {
            var mapFields = LookupInfo.FieldMappings.Where(p => !string.IsNullOrWhiteSpace(p.DataColumn)).ToList();

            string sqlSelect = string.Empty;

            switch (SelectedDatabaseType)
            {
                case DatabaseType.MsSql:
                    _sqlCommand = "SELECT TOP {0} {1} FROM [{2}] WHERE {3}";
                    _openChar = "[";
                    _closeChar = "]";
                    _valueChar = "'";
                    break;
                case DatabaseType.MySql:
                    _sqlCommand = "SELECT {1} FROM {2} WHERE {3} LIMIT 0, {0}";
                    _openChar = "`";
                    _closeChar = "`";
                    _valueChar = "\"";
                    break;
                case DatabaseType.PostgreSql:
                    _sqlCommand = "SELECT {1} FROM {2} WHERE {3} LIMIT {0}";
                    _openChar = "\"";
                    _closeChar = "\"";
                    _valueChar = "'";
                    break;
                case DatabaseType.Oracle:
                    _sqlCommand = "SELECT {1} FROM {2} WHERE {3} ROWNUM <= {0}";
                    _openChar = "\"";
                    _closeChar = "\"";
                    _valueChar = "\"";
                    break;
                case DatabaseType.DB2:
                    _sqlCommand = "SELECT {1} FROM {2} WHERE {3} FETCH FIRST {0} ROWS ONLY";
                    _openChar = "\"";
                    _closeChar = "\"";
                    _valueChar = "\"";
                    break;
                default:
                    throw new NotSupportedException();
            }

            sqlSelect = mapFields.Aggregate(string.Empty, (current, map) => current + (_openChar + map.DataColumn + _closeChar + " AS " + _openChar + map.FieldName + _closeChar + ","));

            if (sqlSelect.EndsWith(","))
            {
                sqlSelect = sqlSelect.Substring(0, sqlSelect.Length - 1);
            }


            if (LookupInfo.MaxLookupRow != 0)
            {
                _sqlCommand = string.Format(_sqlCommand, LookupInfo.MaxLookupRow, sqlSelect, LookupInfo.SourceName, "{0}");
            }
            else
            {
                _sqlCommand = string.Format(_sqlCommand, "1000", sqlSelect, LookupInfo.SourceName, "{0}");
            }

            LookupInfo.SqlCommand = _sqlCommand;
        }

        private void BuildExecuteCommand()
        {
            string sql = string.Empty;
            string openChar = string.Empty;
            string closeChar = string.Empty;
            string paraList = string.Empty;

            switch (SelectedDatabaseType)
            {
                case DatabaseType.MsSql:
                    openChar = "[";
                    closeChar = "]";
                    sql = "EXEC {3}{0}{4}.{3}{1}{4} {2}";// + LookupInfo.SourceName + " ";

                    foreach (ParameterModel para in LookupInfo.Parameters)
                    {
                        if (string.IsNullOrWhiteSpace(para.ParameterValue))
                        {
                            continue;
                        }

                        switch (para.ParameterType)
                        {
                            case "int":
                            case "decimal":
                            case "tinyint":
                            case "bigint":
                            case "float":
                            case "bit":
                                paraList += para.ParameterName + "=" + para.ParameterValue + ",";
                                break;
                            case "char":
                            case "varchar":
                            case "nvarchar":
                            case "text":
                            case "ntext":
                            case "date":
                            case "datetime":
                                paraList += para.ParameterName + "='" + para.ParameterValue + "',";
                                break;
                        }

                    }

                    break;
                case DatabaseType.MySql:
                    openChar = closeChar = "`";
                    sql = "CALL {3}{0}{4}.{3}{1}{4}({2})";
                    foreach (ParameterModel para in LookupInfo.Parameters)
                    {
                        switch (para.ParameterType)
                        {
                            case "int":
                            case "decimal":
                            case "tinyint":
                            case "bigint":
                            case "float":
                            case "bit":
                                paraList += para.ParameterValue + ",";
                                break;
                            case "char":
                            case "varchar":
                            case "text":
                            case "longtext":
                            case "mediumtext":
                            case "tinytext":
                            case "date":
                            case "datetime":
                                paraList += "'" + para.ParameterValue + "',";
                                break;
                        }
                    }
                    break;
                case DatabaseType.Oracle:
                    openChar = closeChar = "\"";
                    sql = "CALL {3}{0}{4}.{3}{1}{4}({2})";

                    foreach (ParameterModel para in LookupInfo.Parameters)
                    {
                        switch (para.ParameterType)
                        {
                            case "number":
                            case "float":
                            case "long":
                            case "bigint":
                                paraList += para.ParameterValue + ",";
                                break;
                            case "char":
                            case "nchar":
                            case "varchar2":
                            case "nvarchar2":
                            case "text":
                            case "ntext":
                            case "date":
                                paraList += "'" + para.ParameterValue + "',";
                                break;
                        }

                    }
                    break;
                case DatabaseType.PostgreSql:
                    openChar = closeChar = "\"";
                    sql = "SELECT * FROM [{0}].{3}{1}{4}({2})";

                    foreach (ParameterModel para in LookupInfo.Parameters)
                    {
                        switch (para.ParameterType)
                        {
                            case "smallint":
                            case "numeric":
                            case "integer":
                            case "bigint":
                            case "float":
                            case "boolean":
                            case "real":
                            case "double precision":
                                paraList += para.ParameterValue + ",";
                                break;
                            case "char":
                            case "character varying":
                            case "text":
                            case "date":
                            case "datetime":
                                paraList += "'" + para.ParameterValue + "',";
                                break;
                        }
                    }

                    break;
                case DatabaseType.DB2:
                    openChar = closeChar = "\"";
                    sql = "CALL {3}{0}{4}.{3}{1}{4}({2})";

                    foreach (ParameterModel para in LookupInfo.Parameters)
                    {
                        switch (para.ParameterType)
                        {
                            case "numeric":
                            case "decimal":
                            case "integer":
                            case "float":
                            case "double":
                            case "smallint":
                                paraList += para.ParameterValue + ",";
                                break;
                            case "char":
                            case "varchar":
                            case "date":
                            case "time":
                                paraList += "'" + para.ParameterValue + "',";
                                break;
                        }

                    }
                    break;
                default:
                    break;
            }

            if (paraList.EndsWith(","))
            {
                paraList = paraList.Substring(0, paraList.Length - 1);
            }

            sql = string.Format(sql, LookupInfo.Connection.Schema, LookupInfo.SourceName, paraList, openChar, closeChar);
            LookupInfo.SqlCommand = sql;
        }

        private void AddWhereClause()
        {
            string value = string.Empty;
            _whereClause = string.Empty;
            BuildCommandText();

            switch (LookupColumn.Value.Value)
            {
                case "real":
                case "double precision":
                case "numeric":
                case "number":
                case "int":
                case "integer":
                case "decimal":
                case "tinyint":
                case "bigint":
                case "float":
                case "double":
                case "smallint":
                case "bit":
                case "bool":
                case "boolean":
                case "uniqueidentifier":
                    value = "<<value>>";
                    break;
                case "char":
                case "varchar":
                case "nvarchar":
                case "text":
                case "ntext":
                case "date":
                case "character varying":
                case "datetime":
                case "varchar2":
                case "nvarchar2":
                case "longtext":
                case "mediumtext":
                case "tinytext":
                    value = "'<<value>>'";
                    break;
            }

            string searchOperator = string.Empty;

            switch (SelectedOperator.Value.Value)
            {
                case SearchOperator.Equal:
                    searchOperator = "=";
                    break;
                case SearchOperator.GreaterThan:
                    searchOperator = ">";
                    break;
                case SearchOperator.GreaterThanOrEqualTo:
                    searchOperator = ">=";
                    break;
                case SearchOperator.LessThan:
                    searchOperator = "<";
                    break;
                case SearchOperator.LessThanOrEqualTo:
                    searchOperator = "<=";
                    break;
                case SearchOperator.Contains:
                case SearchOperator.StartsWith:
                case SearchOperator.EndsWith:
                    searchOperator = "LIKE";
                    break;
                case SearchOperator.NotContains:
                    searchOperator = "NOT LIKE";
                    break;
                case SearchOperator.NotEqual:
                    searchOperator = "<>";
                    break;
            }


            if (SelectedOperator.Value.Value == SearchOperator.Contains || SelectedOperator.Value.Value == SearchOperator.NotContains)
            {
                value = "" + _valueChar + "%<<value>>%" + _valueChar;
            }

            if (SelectedOperator.Value.Value == SearchOperator.StartsWith)
            {
                value = _valueChar + "<<value>>%" + _valueChar;
            }

            if (SelectedOperator.Value.Value == SearchOperator.EndsWith)
            {
                value = _valueChar + "%<<value>>" + _valueChar;
            }

            if (string.IsNullOrEmpty(_whereClause))
            {
                _whereClause += _openChar + LookupColumn.Value.Key + _closeChar + " " + searchOperator + " " + value;
            }
            else
            {
                _whereClause += "AND " + _openChar + LookupColumn.Value.Key + _closeChar + " " + searchOperator + " " + value;
            }

            LookupInfo.SqlCommand = string.Format(LookupInfo.SqlCommand, _whereClause);
        }

        private void ClearWhereClause()
        {
            _whereClause = string.Empty;
            _sqlCommand = string.Empty;
            BuildCommandText();
        }
    }
}
