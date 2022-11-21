using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using Ecm.CaptureDomain;
using System.Xml.Serialization;
using Ecm.Mvvm;
using System.ComponentModel;

namespace Ecm.Workflow.Activities.CustomActivityModel
{
    public class FieldModel : BaseDependencyProperty
    {
        private string _name;
        private string _displayName;
        private FieldDataType _dataType;
        private string _defaultValue;
        private bool _isLookup;
        private bool _isRequired;
        private bool _isRestricted;
        private int _displayOrder;
        private int _maxLength;
        private bool _useCurrentDate;
        private bool _isSelected;
        //private ObservableCollection<LookupMapModel> _maps = new ObservableCollection<LookupMapModel>();


        public FieldModel()
        {
        }

        public Guid Id { get; set; }

        [XmlIgnore]
        public Guid DocTypeId { get; set; }

        [XmlIgnore]
        public Guid BatchTypeId { get; set; }

        [XmlIgnore]
        public string UniqueId { get; set; }

        [XmlIgnore]
        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
                OnPropertyChanged("Name");
            }
        }

        [XmlIgnore]
        public string DisplayName
        {
            get { return _displayName; }
            set
            {
                _displayName = value;
                OnPropertyChanged("DisplayName");
            }
        }

        [XmlIgnore]
        public FieldDataType DataType
        {
            get { return _dataType; }
            set
            {
                _dataType = value;

                MaxLength = 0;
                DefaultValue = string.Empty;
                OnPropertyChanged("DataType");
            }
        }

        [XmlIgnore]
        public bool IsLookup
        {
            get { return _isLookup; }
            set
            {
                _isLookup = value;
                OnPropertyChanged("IsLookup");
            }
        }

        [XmlIgnore]
        public bool IsRequired
        {
            get { return _isRequired; }
            set
            {
                _isRequired = value;
                OnPropertyChanged("IsRequired");
            }
        }

        [XmlIgnore]
        public bool IsSystemField { get; set; }

        [XmlIgnore]
        public bool IsRestricted
        {
            get { return _isRestricted; }
            set
            {
                _isRestricted = value;
                OnPropertyChanged("IsRestricted");
            }
        }

        [XmlIgnore]
        public int DisplayOrder
        {
            get { return _displayOrder; }
            set
            {
                _displayOrder = value;
                OnPropertyChanged("DisplayOrder");
            }
        }

        [XmlIgnore]
        public int MaxLength
        {
            get { return _maxLength; }
            set
            {
                _maxLength = value;
                OnPropertyChanged("MaxLength");
                DefaultValue = string.Empty;
            }
        }

        [XmlIgnore]
        public string DefaultValue
        {
            get { return _defaultValue; }
            set
            {
                _defaultValue = value;
                OnPropertyChanged("DefaultValue");
            }
        }

        [XmlIgnore]
        public bool UseCurrentDate
        {
            get { return _useCurrentDate; }
            set
            {
                _useCurrentDate = value;
                OnPropertyChanged("UseCurrentDate");
            }
        }

        [XmlIgnore]
        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                _isSelected = value;
                OnPropertyChanged("IsSelected");
            }
        }

    }
}
