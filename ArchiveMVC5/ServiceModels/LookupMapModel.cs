
using System;

namespace ArchiveMVC5.Models
{
    public class LookupMapModel
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
            }
        }

        public string DataColumn
        {
            get { return _dataColumn; }
            set
            {
                _dataColumn = value;
            }
        }

        public bool IsChecked
        {
            get { return _isChecked; }
            set
            {
                _isChecked = value;
            }
        }
    }
}
