using Ecm.Mvvm;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Ecm.Admin.ViewModel
{
    public class SupportViewModel : ComponentViewModel
    {
        private string _filePath;

        public SupportViewModel()
        {
            FilePath = "file://" + Path.Combine(new FileInfo(Assembly.GetExecutingAssembly().Location).DirectoryName, "Map.html");
            //FilePath = "http://maps.google.com";
        }

        public string FilePath
        {
            get { return _filePath; }
            set
            {
                _filePath = value;
                OnPropertyChanged("FilePath");
            }
        }

    }
}
