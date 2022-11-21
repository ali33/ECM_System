using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows.Input;

using Ecm.AppHelper;
using Ecm.Localization;
using Ecm.CaptureModel;
using Ecm.CaptureModel.DataProvider;
using Ecm.Mvvm;
using Ecm.Capture.Properties;
using System.Resources;
using System.Reflection;
using Ecm.Capture.View;

namespace Ecm.Capture.ViewModel
{
    public class LoginViewModel : ComponentViewModel
    {
        #region Private methods
        private readonly ResourceManager _resource = new ResourceManager("Ecm.Capture.Resources", Assembly.GetExecutingAssembly());
        private UserModel _user = new UserModel();
        private bool _hasError;
        private string _error;
        private string _companyName = "MIA Solutions {0}";

        private readonly SecurityProvider _securityProvider = new SecurityProvider();
        private RelayCommand _loginCommand;
        private RelayCommand _forgotPasswordCommand;

        #endregion

        #region Public properties

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

        public string CompanyName
        {
            get { return string.Format(_companyName, DateTime.Now.Year.ToString() + "®"); }
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

        public ICommand LoginCommand
        {
            get { return _loginCommand ?? (_loginCommand = new RelayCommand(p => Login(), p => CanLogin())); }
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

        #endregion

        #region Private methods
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

        private bool CanLogin()
        {
            return User != null && !string.IsNullOrEmpty(User.Username) && !string.IsNullOrEmpty(User.Password);
        }

        private void Login()
        {
            IsProcessing = true;

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
                    var uiCulture = new CultureInfo(strFormat);
                    
                    uiCulture.DateTimeFormat.ShortDatePattern = LoginUser.Language.DateFormat;
                    uiCulture.DateTimeFormat.FullDateTimePattern = LoginUser.Language.DateFormat + " " + LoginUser.Language.TimeFormat;
                    uiCulture.NumberFormat.NumberDecimalSeparator = LoginUser.Language.DecimalChar;
                    uiCulture.NumberFormat.NumberGroupSeparator = LoginUser.Language.ThousandChar;

                    CultureManager.UICulture = uiCulture;

                    RuntimeInfo.Culture = uiCulture;
                }

                NavigationHelper.Navigate(new Uri("MainView.xaml", UriKind.RelativeOrAbsolute));
            }
            else
            {
                if (e.Result is Exception)
                {
                    ProcessHelper.ProcessException(e.Result as Exception);
                }

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

        #endregion
    }
}
