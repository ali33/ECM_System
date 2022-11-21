using System.Windows;
using Ecm.Capture.ViewModel;

namespace Ecm.Capture.View
{
    public partial class WorkItemView
    {
        private WorkItemViewModel _viewModel;
        private bool _isLoaded;

        public WorkItemView()
        {
            InitializeComponent();

            Loaded += WorkItemViewLoaded;
            Unloaded += WorkItemViewUnloaded;
        }

        private void WorkItemViewLoaded(object sender, RoutedEventArgs e)
        {
            if (!_isLoaded)
            {
                _isLoaded = true;

                ViewerContainer.HandleException = ProcessHelper.ProcessException;
                ViewerContainer.LogException = ProcessHelper.LogException;
                ViewerContainer.AddActionLog = ProcessHelper.AddActionLog;
                _viewModel = DataContext as WorkItemViewModel;
                //_viewModel.SaveCompletedAction += SaveCompleted;

                if (_viewModel != null)
                {
                    ViewerContainer.SaveAll += ViewerContainerSaveAll;
                    ViewerContainer.ApproveAll += ViewerContainerApprove;
                    ViewerContainer.GetLookupData = _viewModel.GetLookupData;
                    ViewerContainer.Submit += SubmitBatch;
                }
            }
        }

        void SubmitBatch()
        {
            _viewModel.Submit();
        }

        private void ViewerContainerApprove()
        {
            _viewModel.Approve();
        }

        private void WorkItemViewUnloaded(object sender, RoutedEventArgs e)
        {

        }

        //private void SaveCompleted()
        //{
        //    _viewModel.Close();
        //}

        private void ViewerContainerSaveAll()
        {
            _viewModel.Save();
        }
    }
}
