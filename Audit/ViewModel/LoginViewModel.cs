using System;
using Ecm.AppHelper;
using Ecm.Model.DataProvider;
using Ecm.Mvvm;
using Ecm.Model;
using System.Windows.Input;
using System.ComponentModel;
using Ecm.Localization;
using System.Globalization;

namespace Ecm.Audit.ViewModel
{
    public class LoginViewModel : ComponentViewModel
    {
        private RelayCommand _loginCommand;
        private RelayCommand _sendPasswordCommand;
        private RelayCommand _cancelSendPasswordCommand;

        private bool _enableEditText = true;
        private bool _hasError;
        private string _error;
        private readonly SecurityProvider _securityProvider = new SecurityProvider();
        private UserModel _forgotPasswordUser;
        private UserModel _user = new UserModel();
        
        public event CloseDialog CloseDialog;
        public event EventHandler<MoveFocusEventArgs> MoveFocus;

        public static UserModel LoginUser { get; set; }

        public UserModel User
        {
            get { return _user; }
            set
            {
                _user = value;
                OnPropertyChanged("User");
            }
        }

        public UserModel ForgotPasswordUser
        {
            get { return _forgotPasswordUser; }
            set
            {
                _forgotPasswordUser = value;
                OnPropertyChanged("ForgotPasswordUser");
            }
        }
        
        public bool HasError
        {
            get { return _hasError; }
            set
            {
                _hasError = value;
                OnPropertyChanged("HasError");
            }
        }

        public string Error
        {
            get { return _error; }
            set
            {
                _error = value;
                OnPropertyChanged("Error");
            }
        }

        public bool EnableEditText
        {
            get
            {
                return _enableEditText;
            }
            set
            {
                _enableEditText = value;
                OnPropertyChanged("EnableEditText");
            }
        }

        public ICommand LoginCommand
        {
            get { return _loginCommand ?? (_loginCommand = new RelayCommand(p => Login(), p => CanLogin())); }
        }

        public ICommand SendPasswordCommand
        {
            get { return _sendPasswordCommand ?? (_sendPasswordCommand = new RelayCommand(p => SendNewPassword(), p => CanSendNewPassword())); }
        }

        public ICommand CancelSendPasswordCommand
        {
            get { return _cancelSendPasswordCommand ?? (_cancelSendPasswordCommand = new RelayCommand(p => CancelSendNewPassword())); }
        }

        private void CancelSendNewPassword()
        {
            if (CloseDialog != null)
                CloseDialog();
        } 
        
        private bool CanSendNewPassword()
        {
            return ForgotPasswordUser != null && !string.IsNullOrEmpty(ForgotPasswordUser.Username) && !string.IsNullOrEmpty(ForgotPasswordUser.EmailAddress);
        } 
        
        private void SendNewPassword()
        {
            if (CloseDialog != null)
                CloseDialog();
        } 
 
        private bool CanLogin()
        {
            return User != null && !string.IsNullOrEmpty(User.Username) && !string.IsNullOrEmpty(User.Password);
        }

        private void Login()
        {
            IsProcessing = true;
            EnableEditText = false;
            var worker = new BackgroundWorker();
            worker.DoWork += WorkerDoWork;
            worker.RunWorkerCompleted += WorkerRunWorkerCompleted;
            worker.RunWorkerAsync();
        }

        private void WorkerDoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                HasError = false;
                LoginUser = _securityProvider.Login(User.Username, User.Password);

                if (LoginUser == null)
                {
                    SetError(Resources.uiLoginFail);
                }
                else
                {
                    WorkingFolder.Configure(LoginUser.Username);
                }
            }
            catch (Exception ex)
            {
                HasError = true;
                e.Result = ex;
            }
        }

        private void WorkerRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            IsProcessing = false;

            if (!HasError)
            {
                string strFormat = "en";

                if (LoginUser.Language != null)
                {
                    strFormat = LoginUser.Language.Format;
                    CultureManager.UICulture = new CultureInfo(strFormat);
                    CultureManager.UICulture.DateTimeFormat.ShortDatePattern = LoginUser.Language.DateFormat;
                    CultureManager.UICulture.DateTimeFormat.ShortTimePattern = LoginUser.Language.TimeFormat;

                    RuntimeInfo.Culture = new CultureInfo(strFormat);
                    RuntimeInfo.Culture.DateTimeFormat.ShortDatePattern = LoginUser.Language.DateFormat;
                    RuntimeInfo.Culture.DateTimeFormat.ShortTimePattern = LoginUser.Language.TimeFormat;
                }
                
                NavigationHelper.Navigate(new Uri("MainView.xaml", UriKind.RelativeOrAbsolute));
            }
            else
            {
                if (e.Result is Exception)
                {
                    ProcessHelper.ProcessException(e.Result as Exception);
                }
                EnableEditText = true;
                RaiseMoveFocus("Password");
            }
        }

        private void RaiseMoveFocus(string focusedProperty)
        {
            EventHandler<MoveFocusEventArgs> handler = MoveFocus;
            if (handler != null)
            {
                var args = new MoveFocusEventArgs(focusedProperty);
                handler(this, args);
            }
        }

        private void SetError(string msg)
        {
            HasError = !string.IsNullOrEmpty(msg);
            Error = msg;
        }

    }
}