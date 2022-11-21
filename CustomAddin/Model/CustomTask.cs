using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ecm.Mvvm;

namespace ExcelImport.Model
{
    public class CustomTask : BaseDependencyProperty
    {
        private string _customTaskTitle;
        private bool _isChecked;
        private string _controlName;

        public string CustomTaskTitle
        {
            get
            {
                return _customTaskTitle;
            }
            set
            {
                _customTaskTitle = value;
                OnPropertyChanged("CustomTaskTitle");
            }
        }

        public string ControlName
        {
            get
            {
                return _controlName;
            }
            set
            {
                _controlName = value;
                OnPropertyChanged("ControlName");
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
