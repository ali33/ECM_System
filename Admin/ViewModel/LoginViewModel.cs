using System;
using Ecm.AppHelper;
using Ecm.Mvvm;
using Ecm.Model;
using System.Windows.Input;
using System.ComponentModel;
using Ecm.Model.DataProvider;
using System.Resources;
using System.Reflection;
using System.Globalization;
using Ecm.Localization;

namespace Ecm.Admin.ViewModel
{
    public class LoginViewModel : BaseDependencyProperty
    {
        private RelayCommand _loginCommand;
        private RelayCommand _sendPasswordCommand;
        private RelayCommand _cancelSendPasswordCommand;
        private bool _isProcessing;
        public event EventHandler<MoveFocusEventArgs> MoveFocus;
        private bool _hasError;
        private string _error;
        private SecurityProvider _securityProvider = new SecurityProvider();
        private UserModel _forgotPasswordUser;
        private UserModel _user = new UserModel();
        private string _loginFailMessage;
        private ResourceManager _resource = new ResourceManager("Ecm.Admin.Resources", Assembly.GetExecutingAssembly());
        private string _companyName = "MIA Solutions {0}";

        //Events
        public event CloseDialog CloseDialog;

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

        public bool IsProcessing
        {
            get
            {
                return _isProcessing;
            }
            set
            {
                _isProcessing = value;
                OnPropertyChanged("IsProcessing");
            }
        }

        public bool EnableEditText
        {
            get
            {
                return !IsProcessing;
            }
        }

        public string CompanyName
        {
            get { return string.Format(_companyName, DateTime.Now.Year.ToString() + "®"); }
        }

        public ICommand LoginCommand
        {
            get
            {
                if (_loginCommand == null)
                {
                    _loginCommand = new RelayCommand(p => Login(), p => CanLogin());
                }
                return _loginCommand;
            }
        }

        public ICommand SendPasswordCommand
        {
            get
            {
                if (_sendPasswordCommand == null)
                {
                    _sendPasswordCommand = new RelayCommand(p => SendNewPassword(), p => CanSendNewPassword());
                }
                return _sendPasswordCommand;
            }
        }

        public ICommand CancelSendPasswordCommand
        {
            get
            {
                if (_cancelSendPasswordCommand == null)
                {
                    _cancelSendPasswordCommand = new RelayCommand(p => CancelSendNewPassword());
                }
                return _cancelSendPasswordCommand;
            }
        }

        private void CancelSendNewPassword()
        {
            if (CloseDialog != null)
            {
                CloseDialog();
            }
        } 
        
        private bool CanSendNewPassword()
        {
            return ForgotPasswordUser != null && !string.IsNullOrEmpty(ForgotPasswordUser.Username) && !string.IsNullOrEmpty(ForgotPasswordUser.EmailAddress);
        } 
        
        private void SendNewPassword()
        {
            if (CloseDialog != null)
            {
                CloseDialog();
            }
        } 
 
        private bool CanLogin()
        {
            return User != null && !string.IsNullOrEmpty(User.Username) && !string.IsNullOrEmpty(User.Password) &&!IsProcessing;
        }

        private void Login()
        {
            IsProcessing = true;
            OnPropertyChanged("EnableEditText");
            _loginFailMessage = _resource.GetString("uiLoginFail");

            var worker = new BackgroundWorker();
            worker.DoWork += worker_DoWork;
            worker.RunWorkerCompleted += worker_RunWorkerCompleted;
            worker.RunWorkerAsync();
        }

        private void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                HasError = false;
                LoginUser = _securityProvider.Login(User.Username, User.Password);

                if (LoginUser == null || !LoginUser.IsAdmin)
                {
                    SetError(_loginFailMessage);
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

        private void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
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
                OnPropertyChanged("EnableEditText");

                RaiseMoveFocus("Password");
            }
        }

        private void RaiseMoveFocus(string focusedProperty)
        {
            EventHandler<MoveFocusEventArgs> handler = MoveFocus;
            if (handler != null)
            {
                MoveFocusEventArgs args = new MoveFocusEventArgs(focusedProperty);
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