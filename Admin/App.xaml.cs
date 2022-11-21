using System.ComponentModel;
using System.Windows;
using Ecm.AppHelper;
using Ecm.Model;
using Ecm.Admin.ViewModel;
using System;
using Ecm.Model.DataProvider;
using Ecm.Mvvm;
using Ecm.Localization;
using System.Globalization;
using System.Configuration;

namespace Ecm.Admin
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            var pingWorker = new BackgroundWorker();
            pingWorker.DoWork += PingWorkerDoWork;
            pingWorker.RunWorkerAsync();
            LoginViewModel.LoginUser = new UserModel();
            StartupUri = new Uri("pack://application:,,,/LoginView.xaml");
            DialogService.InitializeDefault();
            string defaultLanguage = ConfigurationManager.AppSettings["DefaultLanguage"];

            if (!string.IsNullOrEmpty(defaultLanguage))
            {
                CultureManager.UICulture = new CultureInfo(defaultLanguage);
                RuntimeInfo.Culture = new CultureInfo(defaultLanguage); 
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

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);
            ProviderBase.Close();
        }
    }
}
