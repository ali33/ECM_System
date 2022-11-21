using System;
using Ecm.AppHelper;
using Ecm.Model.DataProvider;
using Ecm.Mvvm;
using Ecm.Model;
using System.Windows.Input;
using System.ComponentModel;
using Ecm.Localization;
using System.Globalization;
using System.Resources;
using System.Reflection;
using Ecm.Archive.View;

namespace Ecm.Archive.ViewModel
{
    public class LoginViewModel : ComponentViewModel
    {
        private RelayCommand _loginCommand;
        private RelayCommand _sendPasswordCommand;
        private RelayCommand _cancelSendPasswordCommand;
        private RelayCommand _forgotPasswordCommand;

        private bool _hasError;
        private string _error;
        private readonly SecurityProvider _securityProvider = new SecurityProvider();
        private UserModel _forgotPasswordUser;
        private UserModel _user = new UserModel();
        private bool _isLoginFormEnable = true;
        private string _companyName = "MIA Solutions {0}";
        public event CloseDialog CloseDialog;
        public event EventHandler<MoveFocusEventArgs> MoveFocus;
        public static UserModel LoginUser { get; set; }
        private readonly ResourceManager _resource = new ResourceManager("Ecm.Archive.Resources", Assembly.GetExecutingAssembly());

        public UserModel User
        {
            get { return _user; }
            set
            {
                _user = value;
                OnPropertyChanged("User");
            }
        }

        public bool IsLoginFormProcess
        {
            get { return _isLoginFormEnable; }
            set
            {
                _isLoginFormEnable = value;
                OnPropertyChanged("IsLoginFormProcess");
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

        public string CompanyName
        {
            get { return string.Format(_companyName, DateTime.Now.Year.ToString() + "®"); }
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

        public ICommand ForgotPasswordCommand
        {
            get
            {
                if (_forgotPasswordCommand == null)
                {
                    _forgotPasswordCommand = new RelayCommand(p => ResetPassword());
                }

                return _forgotPasswordCommand;
            }
        }

        //Private methods

        private void ResetPassword()
        {
            ForgotPasswordViewModel viewModel = new ForgotPasswordViewModel();
            ForgotPasswordView view = new ForgotPasswordView(viewModel);
            DialogBaseView dialog = new DialogBaseView(view);

            viewModel.Dialog = dialog;
            dialog.Text = _resource.GetString("ResetPasswordTitle");
            dialog.MinimizeBox = false;
            dialog.MaximizeBox = false;
            dialog.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            dialog.Width = 370;
            dialog.Height = 150;
            dialog.ShowInTaskbar = false;
            dialog.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;

            dialog.ShowDialog();
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
            IsLoginFormProcess = false;
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
                    SetError(_resource.GetString("uiLoginFail"));
                    HasError = true;
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
                    //DialogService.ShowMessageDialog((e.Result as Exception).Message);
                    //DialogService.ShowMessageDialog((e.Result as Exception).StackTrace);
                    ProcessHelper.ProcessException(e.Result as Exception);
                }

                RaiseMoveFocus("Password");
                IsLoginFormProcess = true;
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