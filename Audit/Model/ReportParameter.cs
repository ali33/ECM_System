using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ecm.Mvvm;

namespace Ecm.Audit.Model
{
    public class ReportParameter : BaseDependencyProperty
    {
        private string _parameterName;
        private string _dataType;
        private string _value;

        public string ParameterName
        {
            get { return _parameterName; }
            set
            {
                _parameterName = value;
                OnPropertyChanged("ParameterName");
            }
        }

        public string DataType
        {
            get { return _dataType; }
            set
            {
                _dataType = value;
                OnPropertyChanged("DataType");
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
    }
}
