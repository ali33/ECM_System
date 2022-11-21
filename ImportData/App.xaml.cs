using Ecm.Mvvm;

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Ecm.ImportData
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
      //  private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);        
        protected override void OnStartup(StartupEventArgs e)
        {
            
            base.OnStartup(e);
            DialogService.InitializeDefault();
            log4net.Config.XmlConfigurator.Configure();
            
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
