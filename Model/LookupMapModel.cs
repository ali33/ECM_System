using Ecm.Mvvm;
using System;

namespace Ecm.Model
{
    public class LookupMapModel : BaseDependencyProperty
    {
        private string _name;
        private string _dataColumn;
        private bool _isChecked;

        public Guid FieldId { get; set; }
        
        public Guid ArchiveFieldId { get; set; }

        public string Name 
        {
            get { return _name; }
            set
            {
                _name = value;
                OnPropertyChanged("Name");
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
