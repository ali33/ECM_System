using System;
using System.ComponentModel;
using System.Configuration;
using System.Globalization;
using System.Windows;

using Ecm.AppHelper;
using Ecm.Capture.ViewModel;
using Ecm.Localization;
using Ecm.CaptureModel;
using Ecm.CaptureModel.DataProvider;
using Ecm.Mvvm;
using Ecm.CustomControl;
using Ecm.Capture.View;

namespace Ecm.Capture
{
    public partial class App
    {
        string _userName = string.Empty;
        string _password = string.Empty;
        string _workItemId = string.Empty;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            var pingWorker = new BackgroundWorker();
            pingWorker.DoWork += PingWorkerDoWork;
            pingWorker.RunWorkerAsync();

            DialogService.InitializeDefault();
            XbapHelper.Configurate();
            string defaultLanguage = ConfigurationManager.AppSettings["DefaultLanguage"];
            if (!string.IsNullOrEmpty(defaultLanguage))
            {
                CultureManager.UICulture = new CultureInfo(defaultLanguage);
                RuntimeInfo.Culture = new CultureInfo(defaultLanguage);
            }
            
            ApplicationMode mode =  ApplicationMode.None;

            UserModel user = GetUserInfoFromUrl(out mode);

            if (user == null)
            {
                StartupUri = new Uri("pack://application:,,,/LoginView.xaml");
            }
            else
            {
                string password = string.Empty;

                if (string.IsNullOrEmpty(user.Password))
                {
                    VerifyPasswordViewModel viewModel = new VerifyPasswordViewModel();
                    VerifyPasswordView view = new VerifyPasswordView(viewModel);
                    DialogBaseView dialog = new DialogBaseView(view);
                    viewModel.Dialog = dialog;
                    dialog.Size = new System.Drawing.Size(250, 150);
                    dialog.MaximizeBox = false;
                    dialog.MinimizeBox = false;

                    //// TODO : Temp test
                    //viewModel.Password = "sa";
                    //user.Password = viewModel.Password;

                    if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        user.Password = viewModel.Password;
                    }
                    else
                    {
                        StartupUri = new Uri("pack://application:,,,/LoginView.xaml");
                    }
                }

                LoginViewModel.LoginUser = Login(user.Username, user.Password);

                if (LoginViewModel.LoginUser == null)
                {
                    StartupUri = new Uri("pack://application:,,,/LoginView.xaml");
                }
                else
                {

                    if (mode != ApplicationMode.None)
                    {
                        WorkingFolder.Configure(LoginViewModel.LoginUser.Username);
                        StartupUri = new Uri("pack://application:,,,/AssignedWorkitemView.xaml?mode=" + mode + "&workitemid=" + _workItemId);
                        //StartupUri = new Uri("pack://application:,,,/MainView.xaml?mode=" + mode + "&workitemid=" + _workItemId);
                    }
                }
            }
        }

        private void PingWorkerDoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                ProviderBase.PingServer();
            }
            catch { }
        }

        protected override void OnNavigated(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigated(e);
            var page = e.Content as System.Windows.Controls.Page;
            if (page != null)
            {
                NavigationHelper.Initialize(page.NavigationService);
            }
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            WorkingFolder.Delete(WorkingFolder.UndeletedFiles);
        }

        private UserModel GetUserInfoFromUrl(out ApplicationMode mode)
        {
            mode = ApplicationMode.None;
            try
            {
                string urlMode = XbapHelper.Params["mode"];

                if (!string.IsNullOrEmpty(urlMode))
                {
                    if (urlMode.ToLower() == "assignedwork")
                    {
                        mode = ApplicationMode.AssignedWork;
                    }
                    else if (urlMode.ToLower() == "capture")
                    {
                        mode = ApplicationMode.Capture;
                    }
                    else if (urlMode.ToLower() == "workitem")
                    {
                        mode = ApplicationMode.WorkItem;
                    }
                }
                _workItemId = XbapHelper.Params["workitemid"] + string.Empty;
                _userName = XbapHelper.Params["username"] + string.Empty;
                //_password = XbapHelper.Params["password"] + string.Empty;

                if (!string.IsNullOrEmpty(_userName))
                {
                    UserModel userModel = new UserModel { Username = _userName };
                    //_password = Utility.UrlKeywordReplace.GenerateText(_password);
                    //userModel.Password = Utility.CryptographyHelper.DecryptUsingSymmetricAlgorithm(_password);

                    return userModel;
                }
            }
            catch (Exception ex)
            {
                DialogService.ShowErrorDialog(ex.Message);
            }

            return null;
        }

        private UserModel Login(string username, string password)
        {
            try
            {
                SecurityProvider provider = new SecurityProvider();
                return provider.Login(username, password);
            }
            catch (Exception ex)
            {
                DialogService.ShowMessageDialog(ex.Message);
            }

            return null;
        }

    }
}
