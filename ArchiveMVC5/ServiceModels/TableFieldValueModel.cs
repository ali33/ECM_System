
using System;

namespace ArchiveMVC5.Models
{
    public class TableFieldValueModel
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
            }
        }

        public string Value
        {
            get { return _value; }
            set
            {
                _value = value;
            }
        }

        public FieldMetaDataModel Field
        {
            get { return _field; }
            set
            {
                _field = value;
            }
        }

    }
}
