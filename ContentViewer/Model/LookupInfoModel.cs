using System;
using Ecm.Mvvm;
using System.Collections.ObjectModel;

namespace Ecm.ContentViewer.Model
{
    [Serializable]
    public class LookupInfoModel : BaseDependencyProperty
    {
        private LookupConnectionModel _connection;
        private string _sqlCommand;
        private int _lookupType;
        private string _sourceName;
        private int _minPrefixLength;
        private bool _lookupAtLostFocus;
        private int _maxLookupRow;
        private bool _applyLookupClient;


        public LookupInfoModel()
        {
            Connection = new LookupConnectionModel();
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

        public Guid FieldId { get; set; }

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

    }
}
