using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ecm.Mvvm;
using Ecm.CaptureModel;
using System.Windows.Input;
using Ecm.CaptureModel.DataProvider;
using Ecm.CaptureCustomAddin;
using Ecm.AppHelper;
using Microsoft.Win32;

namespace Ecm.CaptureCustomAddIn.ViewModel
{
    public class LoginViewModel : BaseDependencyProperty
    {
        private RelayCommand _signingCommand;
        private RelayCommand _cancelCommand;
        private SecurityProvider _securityProvider = new SecurityProvider();
        private UserModel _user = new UserModel();
        private bool _isEnableLogin = true;
        private bool _autoSignIn;
        
        public LoginViewModel(Action closeView)
        {
            CloseView = closeView;
        }

        private Action CloseView { get; set; }

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

        public bool IsEnableLogin
        {
            get { return _isEnableLogin; }
            set
            {
                _isEnableLogin = value;
                OnPropertyChanged("IsEnableLogin");
            }
        }

        public bool IsAutoSignIn
        {
            get { return _autoSignIn; }
            set
            {
                _autoSignIn = value;
                OnPropertyChanged("IsAutoSignIn");
            }
        }

        public AddinType AddinType { get; set; }

        public ICommand SigningCommand
        {
            get
            {
                if (_signingCommand == null)
                {
                    _signingCommand = new RelayCommand(p => Signing(), p => CanSigning());
                }

                return _signingCommand;
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
            if (CloseView != null)
                CloseView();
        }

        private bool CanSigning()
        {
            return User != null && !string.IsNullOrEmpty(User.Username) && !string.IsNullOrEmpty(User.Password);
        }

        private void Signing()
        {
            try
            {
                IsEnableLogin = false;
                LoginUser = new UserModel();
                ProviderBase.Configure(User.Username, Utility.CryptographyHelper.GenerateHash(User.Username, User.Password));
                LoginUser = _securityProvider.Login(User.Username, User.Password);

                WorkingFolder.Configure(LoginUser.Username);
                if (IsAutoSignIn)
                {
                    RegistryKey key = null;

                    Registry.CurrentUser.DeleteSubKey(Common.EXCEL_AUTO_SIGNIN_KEY);
                    Registry.CurrentUser.DeleteSubKey(Common.WORD_AUTO_SIGNIN_KEY);
                    Registry.CurrentUser.DeleteSubKey(Common.POWER_POINT_AUTO_SIGNIN_KEY);
                    Registry.CurrentUser.DeleteSubKey(Common.OUTLOOK_AUTO_SIGNIN_KEY);

                    switch (AddinType)
                    {
                        case AddinType.Excel:
                            key = Registry.CurrentUser.CreateSubKey(Common.EXCEL_AUTO_SIGNIN_KEY);
                            break;
                        case AddinType.Word:
                            key = Registry.CurrentUser.CreateSubKey(Common.WORD_AUTO_SIGNIN_KEY);
                            break;
                        case AddinType.PowerPoint:
                            key = Registry.CurrentUser.CreateSubKey(Common.POWER_POINT_AUTO_SIGNIN_KEY);
                            break;
                        case AddinType.Outlook:
                            key = Registry.CurrentUser.CreateSubKey(Common.OUTLOOK_AUTO_SIGNIN_KEY);
                            break;
                        default:
                            break;
                    }

                    key.SetValue("Username", LoginUser.Username);
                    key.SetValue("Password", LoginUser.EncryptedPassword);
                    key.Close();
                }

                if (CloseView != null)
                {
                    CloseView();
                }
            }
            catch (Exception ex)
            {
                ProcessHelper.ProcessException(ex);
            }
            IsEnableLogin = true;
        }        
    }
}
