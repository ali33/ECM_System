using System;
using Ecm.Mvvm;

namespace Ecm.Workflow.Activities.CustomActivityModel
{
    public class LookupMappingModel : BaseDependencyProperty
    {
        private Guid _fieldId;
        private int _lookupType;
        private string _fieldName;
        private string _dataColumn;
        private bool _isSelected;
        private bool _isChecked;

        public Guid FieldId
        {
            get { return _fieldId; }
            set
            {
                _fieldId = value;
                OnPropertyChanged("FieldId");
            }
        }

        /// <summary>
        /// Get or Set type of lookup: 0 for Batch and 1 for Document
        /// </summary>
        public int LookupType
        {
            get { return _lookupType; }
            set
            {
                _lookupType = value;
                OnPropertyChanged("LookupType");
            }
        }

        public string FieldName
        {
            get { return _fieldName; }
            set
            {
                _fieldName = value;
                OnPropertyChanged("FieldName");
            }
        }

        public string DataColumn
        {
            get { return _dataColumn; }
            set
            {
                _dataColumn = value;
                OnPropertyChanged("DataColumn");
            }
        }

        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                _isSelected = value;
                OnPropertyChanged("IsSelected");
            }
        }

        public bool IsChecked
        {
            get { return _isChecked; }
            set
            {
                _isChecked = value;
                OnPropertyChanged("IsChecked");
            }
        }
    }
}
