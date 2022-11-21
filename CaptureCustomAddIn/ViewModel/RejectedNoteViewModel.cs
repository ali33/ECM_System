using Ecm.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace Ecm.CaptureCustomAddIn.ViewModel
{
    public class RejectedNoteViewModel : BaseDependencyProperty
    {
        private RelayCommand _okCommand;
        private RelayCommand _cancelCommand;
        private string _rejectedNotes;

        public RejectedNoteViewModel(Action<bool> closeView)
        {
            CloseView = closeView;
        }

        private Action<bool> CloseView { get; set; }

        public string RejectedNotes
        {
            get { return _rejectedNotes; }
            set
            {
                _rejectedNotes = value;
                OnPropertyChanged("RejectedNotes");
            }
        }

        public ICommand OkCommand
        {
            get
            {
                if (_okCommand == null)
                {
                    _okCommand = new RelayCommand(p => Save(), p => CanSave());
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
                    _cancelCommand = new RelayCommand(p => Cancel());
                }

                return _cancelCommand;
            }
        }
        //Private method
        private void Cancel()
        {
            if (CloseView != null)
            {
                CloseView(false);
            }
        }

        private bool CanSave()
        {
            return !string.IsNullOrEmpty(RejectedNotes);
        }

        private void Save()
        {
            if (CloseView != null)
            {
                CloseView(true);
            }
        }

    }
}
