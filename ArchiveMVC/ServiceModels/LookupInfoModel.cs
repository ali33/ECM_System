using System;
using Ecm.Mvvm;
using Ecm.Domain;
using System.Collections.Generic;

namespace ArchiveMVC.Models
{
    [Serializable]
    public class LookupInfoModel
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
        private List<LookupMapModel> _fieldMapping = new List<LookupMapModel>();
        private List<ParameterModel> _parameters;

        public LookupInfoModel()
        {
            ConnectionInfo = new LookupConnectionModel();
            FieldMappings = new List<LookupMapModel>();
            _parameters = new List<ParameterModel>();
        }

        public Guid FieldId
        {
            get { return _fieldId; }
            set
            {
                _fieldId = value;
            }
        }

        public LookupConnectionModel ConnectionInfo
        {
            get { return _connectionInfo; }
            set
            {
                _connectionInfo = value;
            }
        }

        public int LookupType
        {
            get { return _lookupType; }
            set
            {
                _lookupType = value;
            }
        }

        public string SqlCommand
        {
            get { return _sqlCommand; }
            set
            {
                _sqlCommand = value;
            }
        }

        public int MaxLookupRow
        {
            get { return _maxLookupRow; }
            set
            {
                _maxLookupRow = value;
            }
        }

        public int MinPrefixLength
        {
            get { return _minPrefixLength; }
            set
            {
                _minPrefixLength = value;
            }
        }

        public string SourceName
        {
            get { return _sourceName; }
            set
            {
                _sourceName = value;
            }
        }

        public string LookupColumn
        {
            get { return _lookupColumn; }
            set
            {
                _lookupColumn = value;
            }
        }

        public string LookupOperator
        {
            get { return _lookupOperator; }
            set
            {
                _lookupOperator = value;
            }
        }

        public string ConnectionString { get; set; }

        public List<ParameterModel> Parameters
        {
            get { return _parameters; }
            set
            {
                _parameters = value;
            }
        }

        public List<LookupMapModel> FieldMappings
        {
            get { return _fieldMapping; }
            set
            {
                _fieldMapping = value;
            }
        }
    }
}
