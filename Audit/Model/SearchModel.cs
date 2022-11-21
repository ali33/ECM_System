using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ecm.Mvvm;

namespace Ecm.Audit.Model
{
    public class SearchModel : BaseDependencyProperty
    {
        private string _condition;
        private string _operator;
        private string _value;
        private string _value1;
        private string _name;
        private string _dataType;

        public string Condition
        {
            get { return _condition; }
            set
            {
                _condition = value;
                OnPropertyChanged("Condition");
            }
        }

        public string Operator
        {
            get { return _operator; }
            set
            {
                _operator = value;
                OnPropertyChanged("Operator");
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

        public string Value1
        {
            get { return _value1; }
            set
            {
                _value1 = value;
                OnPropertyChanged("Value1");
            }
        }

        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
                OnPropertyChanged("Name");
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
    }
}
