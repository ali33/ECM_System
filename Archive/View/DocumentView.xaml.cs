using Ecm.Archive.ViewModel;
using System.Windows.Controls;

namespace Ecm.Archive.View
{
    /// <summary>
    /// Interaction logic for DocumentView.xaml
    /// </summary>
    public partial class DocumentView : UserControl
    {
        public DocumentView()
        {
            InitializeComponent();
            Loaded += DocumentViewLoaded;
            Unloaded += DocumentViewUnloaded;
            ViewerContainer.DeleteDocumentItem += ViewerContainerDeleteDocumentItem;
        }

        private void ViewerContainerDeleteDocumentItem(DocViewer.Model.ContentItem documentItem)
        {
            _viewModel.DeleteDocument();
        }

        private void DocumentViewLoaded(object sender, System.Windows.RoutedEventArgs e)
        {
            if (!_isLoaded)
            {
                _isLoaded = true;

                ViewerContainer.HandleException = ProcessHelper.ProcessException;
                ViewerContainer.LogException = ProcessHelper.LogException;
                ViewerContainer.AddActionLog = ProcessHelper.AddActionLog;
                _viewModel = DataContext as DocumentViewModel;

                if (_viewModel != null)
                {
                    ViewerContainer.SaveAll += ViewerContainerSaveAll;
                    ViewerContainer.GetLookupData = _viewModel.GetLookupData;
                }
            }
        }

        private void DocumentViewUnloaded(object sender, System.Windows.RoutedEventArgs e)
        {

        }

        private void ViewerContainerSaveAll()
        {
            _viewModel.Save();
        }

        private DocumentViewModel _viewModel;
        private bool _isLoaded;
    }
}
