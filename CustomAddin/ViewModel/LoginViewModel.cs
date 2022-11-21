using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ecm.Mvvm;
using Ecm.Model;
using Ecm.CustomAddin.ViewModel;
using System.Windows.Input;
using Ecm.Model.DataProvider;
using Ecm.CustomAddin;
using Ecm.CustomAddin.View;
using Ecm.AppHelper;
using Microsoft.Win32;

namespace Ecm.CustomAddin.ViewModel
{
    public class LoginViewModel : BaseDependencyProperty
    {
        private RelayCommand _signingCommand;
        private RelayCommand _cancelCommand;
        private SecurityProvider _securityProvider = new SecurityProvider();
        private UserModel _user = new UserModel();
        private bool _autoSignIn;
        private bool _isEnableLogin = true;

        public LoginViewModel(Action closeView, AddinType addinType)
        {
            AddinType = addinType;
            CloseView = closeView;
        }

        private AddinType AddinType { get; set; }

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
                LoginUser = new Ecm.Model.UserModel();
                ProviderBase.Configure(User.Username, Utility.CryptographyHelper.GenerateHash(User.Username, User.Password));
                LoginUser = _securityProvider.Login(User.Username, User.Password);

                WorkingFolder.Configure(LoginUser.Username);

                if (IsAutoSignIn)
                {
                    RegistryKey key = null;

                    switch (AddinType)
                    {
                        case AddinType.Excel:
                            try
                            {
                                Registry.CurrentUser.DeleteSubKey(Common.EXCEL_AUTO_SIGNIN_KEY);
                            }
                            catch { }
                            key = Registry.CurrentUser.CreateSubKey(Common.EXCEL_AUTO_SIGNIN_KEY);
                            break;
                        case AddinType.Word:
                            try
                            {
                                Registry.CurrentUser.DeleteSubKey(Common.WORD_AUTO_SIGNIN_KEY);
                            }
                            catch { }
                            key = Registry.CurrentUser.CreateSubKey(Common.WORD_AUTO_SIGNIN_KEY);
                            break;
                        case AddinType.PowerPoint:
                            try
                            {
                                Registry.CurrentUser.DeleteSubKey(Common.POWER_POINT_AUTO_SIGNIN_KEY);
                            }
                            catch { }
                            key = Registry.CurrentUser.CreateSubKey(Common.POWER_POINT_AUTO_SIGNIN_KEY);
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
