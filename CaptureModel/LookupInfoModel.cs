using System;
using Ecm.Mvvm;
using Ecm.CaptureDomain;
using System.Collections.ObjectModel;
using System.Xml.Serialization;
using System.Collections.Generic;
using Ecm.LookupDomain;
using System.Linq;

namespace Ecm.CaptureModel
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

        public string LookupColumn { get; set; }

        public string LookupOperator { get; set; }

        [XmlElement]
        public List<LookupMap> LookupMaps { get; set; }

        protected override object OnClone()
        {
            var newObj = (LookupInfoModel)base.OnClone();

            if (Connection != null)
            {
                newObj.Connection = (LookupConnectionModel)Connection.Clone(); 
            }
            if (LookupMaps != null)
            {
                newObj.LookupMaps = LookupMaps.Select(h => (LookupMap)h.Clone()).ToList();
            }

            return newObj;
        }
    }
}
