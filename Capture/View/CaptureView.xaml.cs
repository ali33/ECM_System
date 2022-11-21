using Ecm.Capture.ViewModel;
using System;
namespace Ecm.Capture.View
{
    public partial class CaptureView
    {
        private CaptureViewModel _viewModel;

        public CaptureView()
        {
            InitializeComponent();
            Loaded += CaptureViewLoaded;
            Unloaded += CaptureViewUnloaded;
        }

        public void Backup()
        {
            //ViewerContainer.Backup();
        }

        private void CaptureViewLoaded(object sender, System.Windows.RoutedEventArgs e)
        {
            ViewerContainer.HandleException = ProcessHelper.ProcessException;
            ViewerContainer.LogException = ProcessHelper.LogException;
            ViewerContainer.AddActionLog = ProcessHelper.AddActionLog;
            _viewModel = DataContext as CaptureViewModel;

            if (_viewModel != null)
            {
                ViewerContainer.SaveAll += ViewerContainerSaveAll;
                ViewerContainer.Save += Save;
                ViewerContainer.GetLookupData = _viewModel.GetLookupData;
                _viewModel.SaveComplete += ViewModelSaveComplete;
            }
        }

        private void CaptureViewUnloaded(object sender, System.Windows.RoutedEventArgs e)
        {
            ViewerContainer.SaveAll -= ViewerContainerSaveAll;
            ViewerContainer.Save -= Save;
            _viewModel.SaveComplete -= ViewModelSaveComplete;
        }

        void Save(CaptureViewer.Model.ContentItem batchItem)
        {
            _viewModel.Save(batchItem);
        }

        private void ViewerContainerSaveAll()
        {
            _viewModel.SaveAll();
        }

        private void ViewModelSaveComplete(object sender, EventArgs e)
        {
            try
            {
                ViewerContainer.Clean();
                ViewerContainer.SaveAllComplete();
                ViewerContainer.Comments = null;
            }
            catch (Exception ex)
            {
                ProcessHelper.ProcessException(ex);
            }
        }
    }
}
