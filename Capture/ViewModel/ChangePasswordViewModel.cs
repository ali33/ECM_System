using System;
using Ecm.CaptureModel.DataProvider;
using Ecm.Mvvm;
using System.Windows.Input;
using Ecm.CaptureModel;
using Ecm.Utility;
using System.Resources;
using System.Reflection;

namespace Ecm.Capture.ViewModel
{
    public class ChangePasswordViewModel : ComponentViewModel
    {
        private RelayCommand _changePasswordCommand;
        private RelayCommand _cancelCommand;
        private string _newPassword;
        private string _confirmedPassword;
        private string _userName;
        private string _oldPassword;
        private string _error;
        private bool _hasError;
        private readonly SecurityProvider _securityProvider = new SecurityProvider();
        private readonly ResourceManager _resource = new ResourceManager("Ecm.Capture.Resources", Assembly.GetExecutingAssembly());

        public event EventHandler SetFocusToOldPassword;
 
        public ChangePasswordViewModel(Action closeDialog)
        {
            CloseDialog = closeDialog;
        }

        public string Error
        {
            get
            {
                return _error;
            }
            set
            {
                _error = value;
                OnPropertyChanged("Error");
            }
        }

        public bool HasError
        {
            get
            {
                return _hasError;
            }
            set
            {
                _hasError = value;
                OnPropertyChanged("HasError");
            }
        }

        public Action CloseDialog { get; set; }

        public string UserName
        {
            get { return _userName; }
            set
            {
                _userName = value;
                OnPropertyChanged("UserName");
            }
        }

        public string OldPassword
        {
            get { return _oldPassword; }
            set
            {
                _oldPassword = value;
                OnPropertyChanged("OldPassword");
            }
        }

        public string NewPassword
        {
            get { return _newPassword; }
            set
            {
                _newPassword = value;
                OnPropertyChanged("NewPassword");
            }
        }

        public string ConfirmedPassword
        {
            get { return _confirmedPassword; }
            set
            {
                _confirmedPassword = value;
                OnPropertyChanged("ConfirmedPassword");
            }
        }

        public ICommand ChangePasswordCommand
        {
            get
            {
                if (_changePasswordCommand == null)
                {
                    _changePasswordCommand = new RelayCommand(p => ChangePassword(), p => CanChangePassword());
                }

                return _changePasswordCommand;
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

        //Private methods
        public bool CanChangePassword()
        {
            return !string.IsNullOrEmpty(NewPassword) && !string.IsNullOrEmpty(ConfirmedPassword) && NewPassword.Equals(ConfirmedPassword);
        }

        public void ChangePassword()
        {
            try
            {
                string newEncryptedPassword = CryptographyHelper.EncryptUsingSymmetricAlgorithm(NewPassword);
                string oldEncryptedPassword = CryptographyHelper.EncryptUsingSymmetricAlgorithm(OldPassword);

                if (_securityProvider.ChangePassword(UserName, oldEncryptedPassword, newEncryptedPassword) != null)
                {
                    DialogService.ShowMessageDialog(_resource.GetString("uiChangePasswordSuccessfully"));
                    if (CloseDialog != null)
                        CloseDialog();
                    LoginViewModel.LoginUser = new UserModel();
                    NavigationHelper.Navigate(new Uri("LoginView.xaml", UriKind.RelativeOrAbsolute));
                }
                else
                {
                    DialogService.ShowErrorDialog(_resource.GetString("uiChangePasswordFailed"));
                    if (SetFocusToOldPassword != null)
                    {
                        SetFocusToOldPassword(this, EventArgs.Empty);
                    }
                }
            }
            catch (Exception ex)
            {
                ProcessHelper.ProcessException(ex);
            }
        }

        public void Cancel()
        {
            if (CloseDialog != null)
                CloseDialog();
        }

    }
}
