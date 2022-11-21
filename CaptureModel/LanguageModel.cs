using Ecm.Mvvm;
using System;

namespace Ecm.CaptureModel
{
    public class LanguageModel : BaseDependencyProperty
    {
        private Guid _id;
        private string _name;
        private string _format;
        private string _dateFormat;
        private string _timeFormat;
        private string _thousandChar;
        private string _decimalChar;

        public Guid Id
        {
            get { return _id; } 
            set
            {
                _id = value;
                OnPropertyChanged("Id");
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

        public string Format
        {
            get { return _format; }
            set
            {
                _format = value;
                OnPropertyChanged("Format");
            }
        }
        public string DateFormat
        {
            get { return _dateFormat; }
            set
            {
                _dateFormat = value;
                OnPropertyChanged("DateFormat");
            }
        }

        public string TimeFormat
        {
            get { return _timeFormat; }
            set
            {
                _timeFormat = value;
                OnPropertyChanged("TimeFormat");
            }
        }

        public string ThousandChar
        {
            get { return _thousandChar; }
            set
            {
                _thousandChar = value;
                OnPropertyChanged("ThousandChar");
            }
        }

        public string DecimalChar
        {
            get { return _decimalChar; }
            set
            {
                _decimalChar = value;
                OnPropertyChanged("DecimalChar");
            }
        }
    }
}
