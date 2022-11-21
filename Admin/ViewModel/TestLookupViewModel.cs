using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ecm.Mvvm;
using System.Windows.Input;
using Ecm.Admin.View;

namespace Ecm.Admin.ViewModel
{
    public class TestLookupViewModel : ComponentViewModel
    {
        private RelayCommand _OkCommand;
        private RelayCommand _cancelCommand;
        private string _value;

        public DialogBaseView Dialog { get; set; }

        public string Value
        {
            get { return _value; }
            set
            {
                _value = value;
                OnPropertyChanged("Value");
            }
        }

        public ICommand OkCommand
        {
            get
            {
                if (_OkCommand == null)
                {
                    _OkCommand = new RelayCommand(p => Test(), p => CanTest());
                }
                return _OkCommand;
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

        private void Cancel()
        {
            if (Dialog != null)
            {
                Dialog.DialogResult = System.Windows.Forms.DialogResult.Cancel;
                Dialog.Close();
            }
        }

        public TestLookupViewModel()
        {
        }

        private void Test()
        {
            if (Dialog != null)
            {
                Dialog.DialogResult = System.Windows.Forms.DialogResult.OK;
                Dialog.Close();
            }
        }

        private bool CanTest()
        {
            return !string.IsNullOrEmpty(Value);
        }

    }
}
