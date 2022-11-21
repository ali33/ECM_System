using System;
using Ecm.Mvvm;
using Ecm.Domain;
using System.Collections.ObjectModel;

namespace Ecm.Model
{
    [Serializable]
    public class LookupInfoModel : BaseDependencyProperty
    {
        private Guid _fieldId;
        private int _lookupType;
        private string _sqlCommand;
        private int _maxLookupRow;
        private int _minPrefixLength;
        private string _sourceName;
        private string _lookupColumn;
        private string _lookupOperator;
        private LookupConnectionModel _connectionInfo;
        private ObservableCollection<LookupMapModel> _fieldMapping = new ObservableCollection<LookupMapModel>();
        private ObservableCollection<ParameterModel> _parameters;

        public LookupInfoModel()
        {
            ConnectionInfo = new LookupConnectionModel();
            FieldMappings = new ObservableCollection<LookupMapModel>();
            _parameters = new ObservableCollection<ParameterModel>();
        }

        public Guid FieldId
        {
            get { return _fieldId; }
            set
            {
                _fieldId = value;
                OnPropertyChanged("FieldId");
            }
        }

        //public string ServerName
        //{
        //    get { return _servierName; }
        //    set
        //    {
        //        _servierName = value;
        //        OnPropertyChanged("ServerName");
        //    }
        //}

        //public string DataProvider
        //{
        //    get { return _dataProvider; }
        //    set
        //    {
        //        _dataProvider = value;
        //        OnPropertyChanged("DataProvider");
        //    }
        //}

        //public string Username
        //{
        //    get { return _username; }
        //    set
        //    {
        //        _username = value;
        //        OnPropertyChanged("Username");
        //    }
        //}

        //public string Password
        //{
        //    get { return _password; }
        //    set
        //    {
        //        _password = value;
        //        OnPropertyChanged("Password");
        //    }
        //}

        public LookupConnectionModel ConnectionInfo
        {
            get { return _connectionInfo; }
            set
            {
                _connectionInfo = value;
                OnPropertyChanged("ConnectionInfo");
            }
        }
        public int LookupType
        {
            get { return _lookupType; }
            set
            {
                _lookupType = value;
                OnPropertyChanged("LookupType");
            }
        }

        public string SqlCommand
        {
            get { return _sqlCommand; }
            set
            {
                _sqlCommand = value;
                OnPropertyChanged("SqlCommand");
            }
        }

        public int MaxLookupRow
        {
            get { return _maxLookupRow; }
            set
            {
                _maxLookupRow = value;
                OnPropertyChanged("MaxLookupRow");
            }
        }

        public int MinPrefixLength
        {
            get { return _minPrefixLength; }
            set
            {
                _minPrefixLength = value;
                OnPropertyChanged("MinPrefixLength");
            }
        }

        public string SourceName
        {
            get { return _sourceName; }
            set
            {
                _sourceName = value;
                OnPropertyChanged("SourceName");
            }
        }

        //public string DatabaseName
        //{
        //    get { return _databaseName; }
        //    set
        //    {
        //        _databaseName = value;
        //        OnPropertyChanged("DatabaseName");
        //    }
        //}

        public string LookupColumn
        {
            get { return _lookupColumn; }
            set
            {
                _lookupColumn = value;
                OnPropertyChanged("LookupColumn");
            }
        }

        public string LookupOperator
        {
            get { return _lookupOperator; }
            set
            {
                _lookupOperator = value;
                OnPropertyChanged("LookupOperator");
            }
        }

        public string ConnectionString { get; set; }

        public ObservableCollection<ParameterModel> Parameters
        {
            get { return _parameters; }
            set
            {
                _parameters = value;
                OnPropertyChanged("Parameters");
            }
        }

        public ObservableCollection<LookupMapModel> FieldMappings
        {
            get { return _fieldMapping; }
            set
            {
                _fieldMapping = value;
                OnPropertyChanged("FieldMappings");
            }
        }
    }
}
