using System;
using System.Collections.Generic;
using System.Linq;
using Ecm.Mvvm;
using Ecm.Model;
using System.Windows.Input;

namespace Ecm.Admin.ViewModel
{
    public class PicklistViewModel : BaseDependencyProperty
    {
        private RelayCommand _okCommand;
        private RelayCommand _cancelCommand;
        private string _values = string.Empty;
        private List<PicklistModel> _picklists = new List<PicklistModel>();

        public PicklistViewModel(Action<PicklistViewModel> saveAction)
        {
            SaveAction = saveAction;
        }

        public bool IsEditMode { get; set; }

        public string Values
        {
            get { return _values; }
            set
            {
                _values = value;
                OnPropertyChanged("Values");
            }
        }

        private Action<PicklistViewModel> SaveAction { get; set; }

        public List<PicklistModel> Picklists
        {
            get { return _picklists; }
            set
            {
                _picklists = value;
                if (value != null)
                {
                    foreach (var picklist in value)
                    {
                        _values += picklist.Value + Environment.NewLine;
                    }

                    if (_values.EndsWith(Environment.NewLine))
                    {
                        _values = _values.Substring(0, _values.Length - Environment.NewLine.Length);
                    }
                }
            }
        }

        public ICommand OkCommand
        {
            get { return _okCommand ?? (_okCommand = new RelayCommand(p => SavePicklist(), p => CanSave())); }
        }

        public ICommand CancelCommand
        {
            get { return _cancelCommand ?? (_cancelCommand = new RelayCommand(p => Cancel())); }
        }

        //Private methods

        private bool CanSave()
        {
            return !string.IsNullOrEmpty(Values) && !string.IsNullOrWhiteSpace(Values);
        }

        private void SavePicklist()
        {
            if (SaveAction != null)
            {
                Picklists.Clear();
                string[] valueArray = Values.Split(new [] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries).Distinct().ToArray();

                foreach (var value in valueArray)
                {
                    Picklists.Add(new PicklistModel { Value = value });
                }
                _values = string.Empty;
                SaveAction(this);
            }
        }
        private void Cancel()
        {
            if (SaveAction != null)
            {
                _values = string.Empty;
                SaveAction(null);
            }
        }
    }
}
