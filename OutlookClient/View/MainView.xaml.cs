using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Ecm.OutlookClient.Model;
using Ecm.OutlookClient.ViewModel;

namespace Ecm.OutlookClient.View
{
    /// <summary>
    /// Interaction logic for MainView.xaml
    /// </summary>
    public partial class MainView : Window
    {
        private List<MailItemInfo> _mailInfos;
        public MainView(List<MailItemInfo> mailInfos)
        {
            InitializeComponent();
            _mailInfos = mailInfos;
            MainViewModel viewModel = new MainViewModel(_mailInfos, CloseView);
            DataContext = viewModel;
            Loaded += ViewLoaded;
            Unloaded += ViewUnloaded;
            ViewerContainer.SaveAll += ViewerContainerSaveAll;

        }

        private void CloseView()
        {
            this.Close();
        }
        private void ViewLoaded(object sender, System.Windows.RoutedEventArgs e)
        {
            ViewerContainer.HandleException = ProcessHelper.ProcessException;
            ViewerContainer.GetLookupData = (DataContext as MainViewModel).GetLookupData;
        }

        private void ViewUnloaded(object sender, System.Windows.RoutedEventArgs e)
        {

        }

        private void ViewerContainerSaveAll()
        {
            (DataContext as MainViewModel).Save();
        }


    }
}
