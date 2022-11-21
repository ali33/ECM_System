using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ecm.Mvvm;
using Ecm.Model;
using Ecm.OutlookClient.ViewModel;
using System.Windows.Input;
using Ecm.Model.DataProvider;
using Ecm.OutlookClient.Model;
using Ecm.OutlookClient.View;
using Ecm.AppHelper;
using Microsoft.Win32;

namespace Ecm.OutlookClient.ViewModel
{
    public class LoginViewModel : BaseDependencyProperty
    {
        private RelayCommand _signingCommand;
        private RelayCommand _cancelCommand;
        private SecurityProvider _securityProvider = new SecurityProvider();
        private UserModel _user = new UserModel();
        //private List<MailItemInfo> _mailItemInfos;
        private bool _autoSignIn;

        public LoginViewModel(Action closeView)
        {
            //_mailItemInfos = mailInfos;
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
                LoginUser = new Ecm.Model.UserModel();
                LoginUser = _securityProvider.Login(User.Username, User.Password);

                if (LoginUser == null)
                {
                    DialogService.ShowMessageDialog("Login fail. Please re-enter username and password and try again!");
                    return;
                }

                WorkingFolder.Configure(LoginUser.Username);

                if (IsAutoSignIn)
                {
                    try
                    {
                        Registry.CurrentUser.DeleteSubKey(Common.OUTLOOK_AUTO_SIGNIN_KEY);
                    }
                    catch
                    {
                    }

                    RegistryKey key = Registry.CurrentUser.CreateSubKey(Common.OUTLOOK_AUTO_SIGNIN_KEY);
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
        }
    }
}
