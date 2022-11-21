using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ecm.Mvvm;
using System.Windows.Input;
using Ecm.Model;
using System.Collections.ObjectModel;
using System.Resources;
using System.Reflection;
using Ecm.Domain;

namespace Ecm.Archive.ViewModel
{
    public class ContentSearchViewModel : ComponentViewModel
    {
        private string _content;
        private DocumentTypeModel _docType;
        private RelayCommand _okCommand;
        private RelayCommand _cancelCommand;
        private ResourceManager _resource = new ResourceManager("Ecm.Archive.Resources", Assembly.GetExecutingAssembly());
        private string _selectedOperator;
        private Action<string> DoSearch { get; set; }
        public event CloseDialog CloseDialog;

        public ContentSearchViewModel(Action<string> search, DocumentTypeModel docType)
        {
            _docType = docType;
            DoSearch = search;
        }

        public string ContentSearch
        {
            get { return _content; }
            set
            {
                _content = value;
                OnPropertyChanged("ContentSearch");
            }
        }

        public ICommand OkCommand
        {
            get
            {
                if (_okCommand == null)
                {
                    _okCommand = new RelayCommand(p => RunSearch(), p => CanSearch());
                }

                return _okCommand;
            }
        }

        public ICommand CancelCommand
        {
            get
            {
                if (_cancelCommand == null)
                {
                    _cancelCommand = new RelayCommand(p => CancelSearch());
                }

                return _cancelCommand;
            }
        }

        //Private methods
        private void CancelSearch()
        {
            if (CloseDialog != null)
            {
                CloseDialog();
            }
        }

        private bool CanSearch()
        {
            return !string.IsNullOrEmpty(ContentSearch);
        }

        private void RunSearch()
        {
            if (DoSearch != null)
            {
                DoSearch(ContentSearch);
                if (CloseDialog != null)
                {
                    CloseDialog();
                }
            }
        }
    }
}
