using System;
using System.Configuration;
using System.Globalization;
using System.Windows;

using Ecm.AppHelper;
using Ecm.CaptureAdmin.ViewModel;
using Ecm.CaptureModel;
using Ecm.CaptureModel.DataProvider;
using Ecm.Localization;
using Ecm.Mvvm;

namespace Ecm.CaptureAdmin
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

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
