using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ecm.Mvvm;

namespace Ecm.CaptureModel
{
    public class BatchFieldValueModel : BaseDependencyProperty
    {
        private string _value;
        public Guid Id { get; set; }

        public Guid BackupBatchId { get; set; }

        public Guid FieldId { get; set; }

        public BatchFieldMetaDataModel Field { get; set; }

        public string Value
        {
            get { return _value; }
            set
            {
                _value = value;
                OnPropertyChanged("Value");
            }
        }
    }
}
