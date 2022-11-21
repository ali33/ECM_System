using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ecm.Mvvm;
using System.ComponentModel;

namespace Ecm.Model
{
    public class PicklistModel : BaseDependencyProperty, IDataErrorInfo
    {
        private Guid _fieldId;
        private string _value;

        public PicklistModel()
        {
        }

        public PicklistModel(Action<PicklistModel> errorChecked)
        {
            errorChecked = ErrorChecked;
        }

        public Guid Id { get; set; }

        public Guid FieldId
        {
            get { return _fieldId; }
            set
            {
                _fieldId = value;
                OnPropertyChanged("FieldId");
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

        #region IDataErrorInfo Members
        public Action<PicklistModel> ErrorChecked { get; set; }

        public bool HasError { get; set; }

        public string Error
        {
            get { throw new NotImplementedException(); }
        }

        public string ErrorMessage { get; set; }

        public string this[string columnName]
        {
            get
            {
                string errorMsg = string.Empty;
                if (columnName == "Value")
                {
                    if (ErrorChecked != null)
                    {
                        ErrorChecked(this);
                    }

                    if (HasError)
                    {
                        errorMsg = ErrorMessage;
                    }

                }

                return errorMsg;
            }
        }

        #endregion
    }
}
