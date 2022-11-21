using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ecm.Mvvm;
using System.Collections.ObjectModel;

namespace Ecm.Workflow.Activities.CustomActivityModel
{
    public class LookupInfoModel : BaseDependencyProperty
    {
        private LookupConnectionModel _connection;
        private Guid _fieldId;
        private string _sqlCommand;
        private int _lookupType;
        private string _sourceName;
        private int _minPrefixLength;
        private bool _lookupAtLostFocus;
        private int _maxLookupRow;
        private bool _applyLookupClient;
        private string _lookupColumn;
        private string _lookupOperator;
        private ObservableCollection<LookupMappingModel> _fieldMappings;
        private ObservableCollection<ParameterModel> _parameters;
        public LookupInfoModel()
        {
            Connection = new LookupConnectionModel();
            FieldMappings = new ObservableCollection<LookupMappingModel>();
            _parameters = new ObservableCollection<ParameterModel>();
        }

        public LookupConnectionModel Connection
        {
            get { return _connection; }
            set
            {
                _connection = value;
                OnPropertyChanged("Connection");
            }
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

        public string SqlCommand
        {
            get { return _sqlCommand; }
            set
            {
                _sqlCommand = value;
                OnPropertyChanged("SqlCommand");
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

        public string SourceName
        {
            get { return _sourceName; }
            set
            {
                _sourceName = value;
                OnPropertyChanged("SourceName");
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

        public bool LookupAtLostFocus
        {
            get { return _lookupAtLostFocus; }
            set
            {
                _lookupAtLostFocus = value;
                OnPropertyChanged("LookupAtLostFocus");
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

        public bool ApplyLookupClient
        {
            get { return _applyLookupClient; }
            set
            {
                _applyLookupClient = value;
                OnPropertyChanged("ApplyLookupClient");
            }
        }

        public ObservableCollection<LookupMappingModel> FieldMappings
        {
            get { return _fieldMappings; }
            set
            {
                _fieldMappings = value;
                OnPropertyChanged("FieldMappings");
            }
        }

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

        public ObservableCollection<ParameterModel> Parameters
        {
            get { return _parameters; }
            set
            {
                _parameters = value;
                OnPropertyChanged("Parameters");
            }
        }

        protected override object OnClone()
        {
            var newObj = (LookupInfoModel)base.OnClone();

            newObj._connection = (LookupConnectionModel)_connection.Clone();

            var newFieldMappings = new List<LookupMappingModel>(_fieldMappings.Count);
            foreach (var item in _fieldMappings)
            {
                newFieldMappings.Add((LookupMappingModel)item.Clone());
            }
            newObj._fieldMappings = new ObservableCollection<LookupMappingModel>(newFieldMappings);

            var newParameters = new List<ParameterModel>(_parameters.Count);
            foreach (var item in _parameters)
            {
                newParameters.Add((ParameterModel)item.Clone());
            }
            newObj._parameters = new ObservableCollection<ParameterModel>(newParameters);

            return newObj;
        }
    }
}
