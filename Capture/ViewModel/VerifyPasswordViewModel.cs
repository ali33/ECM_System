using Ecm.Capture.View;
using Ecm.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace Ecm.Capture.ViewModel
{
    public class VerifyPasswordViewModel : ComponentViewModel
    {
        private string _password;
        private RelayCommand _okCommand;
        private RelayCommand _cancelCommand;

        public DialogBaseView Dialog { get; set; }

        public string Password
        {
            get { return _password; }
            set
            {
                _password = value;
                OnPropertyChanged("Password");
            }
        }

        public ICommand OkCommand
        {
            get
            {
                if (_okCommand == null)
                {
                    _okCommand = new RelayCommand(p => LoginView(), p => CanOk());
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

        private void Cancel()
        {
            if (Dialog != null)
            {
                Dialog.DialogResult = System.Windows.Forms.DialogResult.Cancel;
                Dialog.Close();
            }
        }


        private bool CanOk()
        {
            return !string.IsNullOrEmpty(Password) && !string.IsNullOrWhiteSpace(Password);
        }

        private void LoginView()
        {
            if (Dialog != null)
            {
                Dialog.DialogResult = System.Windows.Forms.DialogResult.OK;
                Dialog.Close();
            }
        }


    }
}
