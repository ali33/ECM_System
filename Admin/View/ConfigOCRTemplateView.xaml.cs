using Ecm.Admin.ViewModel;

namespace Ecm.Admin.View
{
    /// <summary>
    /// Interaction logic for ConfigOCRTemplate.xaml
    /// </summary>
    public partial class ConfigOCRTemplateView
    {
        private ConfigOCRTemplateViewModel _viewModel;
        public DialogBaseView Dialog { get; set; }

        public ConfigOCRTemplateView(ConfigOCRTemplateViewModel viewModel)
        {
            InitializeComponent();
            DataContext = _viewModel = viewModel;
            //Loaded += ConfigOCRTemplateViewLoaded;
            _viewModel.SaveOcrTemplateCompleted = SaveOcrCompleted;
            ViewerContainer.HandleException = ProcessHelper.ProcessException;
        }

        //private void ConfigOCRTemplateViewLoaded(object sender, System.Windows.RoutedEventArgs e)
        //{
        //    if (_viewModel != null)
        //    {
        //        //_viewModel.PropertyChanged += ConfigOCRTemplateViewPropertyChanged;
        //        if (_viewModel.Languages != null)
        //        {
        //            if (_viewModel.Languages.Count > 0)
        //            {
        //                cboLanguage.SelectedIndex = 0;
        //            }
        //        }
        //    }
        //}

        //private void ConfigOCRTemplateViewPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        //{
        //    //if (e.PropertyName == "TemplateFilePath")
        //    //{
        //    //    ViewerContainer.ImportOCRTemplate(_viewModel.TemplateFilePath);
        //    //}
        //}

        private void SaveOcrCompleted(bool isCompleted)
        {
            if (isCompleted)
            {
                Dialog.DialogResult = System.Windows.Forms.DialogResult.OK;
            }
            else
            {
                Dialog.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            }
            Dialog.Close();
        }


    }
}
