using Ecm.Mvvm;
using System;

namespace Ecm.CaptureModel
{
    public class LookupMapModel : BaseDependencyProperty
    {
        private string _name;
        private string _dataColumn;
        private bool _isChecked;

        public Guid Id { get; set; }

        public Guid LookupFieldId { get; set; }

        public Guid MapFieldId { get; set; }

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
