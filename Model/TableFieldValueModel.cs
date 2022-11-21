using Ecm.Mvvm;
using System;

namespace Ecm.Model
{
    public class TableFieldValueModel : BaseDependencyProperty
    {
        private int _rowNumber;
        private string _value;
        private FieldMetaDataModel _field;

        public Guid Id { get; set; }

        public Guid FieldId { get; set; }

        public Guid DocId { get; set; } 

        public int RowNumber
        {
            get { return _rowNumber; }
            set
            {
                _rowNumber = value;
                OnPropertyChanged("RowNumber");
            }
        }

        public string Value
        {
            get { return _value; }
            set
            {
                _value = value;
                OnPropertyChanged("Value");
            }
        }

        public FieldMetaDataModel Field
        {
            get { return _field; }
            set
            {
                _field = value;
                OnPropertyChanged("Field");
            }
        }

        public bool IsNew { get; set; }

    }
}
