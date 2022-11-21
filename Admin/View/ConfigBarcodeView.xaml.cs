using System.Windows;
using Ecm.Admin.ViewModel;
using System.Threading;

namespace Ecm.Admin.View
{
    /// <summary>
    /// Interaction logic for ConfigBarcodeView.xaml
    /// </summary>
    public partial class ConfigBarcodeView
    {
        public ConfigBarcodeView()
        {
            InitializeComponent();
            Loaded += ConfigBarcodeViewLoaded;
        }

        private void ConfigBarcodeViewLoaded(object sender, RoutedEventArgs e)
        {
            ConfigBarcodeViewModel viewModel = DataContext as ConfigBarcodeViewModel;
            
            if (viewModel != null)
            {
                viewModel.ResetListView = ResetListView;
                viewModel.PropertyChanged += ViewModelPropertyChanged;
            }
        }

        private void ViewModelPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "SelectedBarcodeConfiguration" || e.PropertyName == "EditedBarcodeConfiguration")
            {
                Dispatcher.BeginInvoke((ThreadStart)(() => cboBarcodeType.Focus()));
            }
        }

        private void ResetListView()
        {
            lvBarcode.SelectedIndex = -1;
        }

        private void EditPanel_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            Dispatcher.BeginInvoke((ThreadStart)(() => cboBarcodeType.Focus()));
        }
    }
}
