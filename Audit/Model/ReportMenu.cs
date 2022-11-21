using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ecm.Mvvm;

namespace Ecm.Audit
{
    public class ReportMenu : BaseDependencyProperty
    {
        private string _menuText;
        private string _imagePath;
        private string _menuName;

        public string MenuText
        {
            get { return _menuText; }
            set
            {
                _menuText = value;
                OnPropertyChanged("MenuText");
            }
        }

        public string MenuName
        {
            get { return _menuName; }
            set
            {
                _menuName = value;
                OnPropertyChanged("MenuName");
            }
        }

        public string ImagePath
        {
            get { return _imagePath; }
            set
            {
                _imagePath = value;
                OnPropertyChanged("ImagePath");
            }
        }

    }
}
