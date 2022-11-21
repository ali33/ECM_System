using System;
using Ecm.Archive.ViewModel;
using System.Windows.Controls;

namespace Ecm.Archive.View
{
    public partial class CaptureView : UserControl
    {
        public CaptureView()
        {
            InitializeComponent();
            Loaded += CaptureViewLoaded;
            Unloaded += CaptureViewUnloaded;
        }

        public void Backup()
        {
            ViewerContainer.Backup();
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
                ViewerContainer.GetLookupData = _viewModel.GetLookupData;
                _viewModel.SaveComplete += ViewModelSaveComplete;
            }
        }

        private void ViewModelSaveComplete(object sender, EventArgs e)
        {
            try
            {
                ViewerContainer.Clean();
            }
            catch(Exception ex)
            {
                ProcessHelper.ProcessException(ex);
            }
        }

        private void CaptureViewUnloaded(object sender, System.Windows.RoutedEventArgs e)
        {
            ViewerContainer.SaveAll -= ViewerContainerSaveAll;
            _viewModel.SaveComplete -= ViewModelSaveComplete;
        }

        private void ViewerContainerSaveAll()
        {
            _viewModel.Save();
        }

        private CaptureViewModel _viewModel;
    }
}
