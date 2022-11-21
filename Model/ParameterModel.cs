using Ecm.Mvvm;
using System;
namespace Ecm.Model
{
    [Serializable()]
    public class ParameterModel : BaseDependencyProperty
    {
        private string _parameterValue;

        public string ParameterName { get; set; }

        public string ParameterType { get; set; }

        public string OrderIndex { get; set; }

        public string Mode { get; set; }

        public string ParameterValue
        {
            get { return _parameterValue; }
            set
            {
                _parameterValue = value;
                OnPropertyChanged("ParameterValue");
            }
        }

        public bool IsRequired { get; set; }
    }
}
