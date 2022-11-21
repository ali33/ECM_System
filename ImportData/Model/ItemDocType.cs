using Ecm.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.ComponentModel;
using Ecm.Mvvm;

namespace Ecm.ImportData.ViewModel
{
    public class ItemDocType: BaseDependencyProperty, IDisposable
    {
        public ItemDocType()
        {
            backgroundWorker = new BackgroundWorker();
                    }
        public DocumentType documentType { get; set; }
        public System.IO.DirectoryInfo directoryInfo { get; set; }
        public BackgroundWorker backgroundWorker { get; set; }
        private Status _Status;
        public Status Status
        {
            get
            {
                return _Status;
            }
            set
            {
                _Status = value;
                OnPropertyChanged("Status");
                if (_Status == ViewModel.Status.Running)
                {
                    TitleButton = "Stop";
                }
                else
                {
                    TitleButton = "Start";
                }
            }
        }

        public string _TitleButton;
        public string TitleButton
        {
            get
            {
                return _TitleButton;
            }
            set
            {
                _TitleButton = value;
                OnPropertyChanged("TitleButton");
            }

        }

        public String DirName
        {
            get
            {
                return directoryInfo.Name;
            }
        }

        public String DocumentTypeName
        {
            get
            {
                return documentType.Name;
            }
        }
        private int _Progress;
        public int Progress
        {
            get
            {
                return _Progress;
            }
            set
            {
                _Progress = value; OnPropertyChanged("Progress");
            }
        }
    }
    public enum Status
    {
        Stop=0,
        Running =1

    
    }
    
}

