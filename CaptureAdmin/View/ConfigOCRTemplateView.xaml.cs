using Ecm.CaptureAdmin.ViewModel;

namespace Ecm.CaptureAdmin.View
{
    /// <summary>
    /// Interaction logic for ConfigOCRTemplate.xaml
    /// </summary>
    public partial class ConfigOCRTemplateView
    {
        //private ConfigOCRTemplateViewModel _viewModel;

        public ConfigOCRTemplateView()
        {
            InitializeComponent();
            Loaded += ConfigOCRTemplateViewLoaded;
            ViewerContainer.HandleException = ProcessHelper.ProcessException;
        }

        private void ConfigOCRTemplateViewLoaded(object sender, System.Windows.RoutedEventArgs e)
        {
            //_viewModel = DataContext as ConfigOCRTemplateViewModel;
            //if (_viewModel != null)
            //{
            //    //_viewModel.PropertyChanged += ConfigOCRTemplateViewPropertyChanged;
            //    if (_viewModel.Languages != null)
            //    {
            //        if (_viewModel.Languages.Count > 0)
            //        {
            //            cboLanguage.SelectedIndex = 0;
            //        }
            //    }
            //}
        }

        //private void ConfigOCRTemplateViewPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        //{
        //    if (e.PropertyName == "TemplateFilePath")
        //    {
        //        ViewerContainer.ImportOCRTemplate(_viewModel.TemplateFilePath);
        //    }
        //}
    }
}
