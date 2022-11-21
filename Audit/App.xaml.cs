using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Navigation;
using Ecm.Audit.ViewModel;
using Ecm.Mvvm;
using Ecm.AppHelper;
using Ecm.Model;
using System.Globalization;
using Ecm.Localization;

namespace Ecm.Audit
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
            WorkingFolder.Configure("ECM");
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

    }
}
