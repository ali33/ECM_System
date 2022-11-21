using Ecm.AppHelper;
using Ecm.CaptureCustomAddIn.ViewModel;
using Ecm.CaptureModel;
using System;
using System.Collections.Generic;
using System.IO;
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

namespace Ecm.CaptureCustomAddIn.View
{
    /// <summary>
    /// Interaction logic for MainView.xaml
    /// </summary>
    public partial class MainView : Window
    {
        MainViewModel _viewModel;
        private List<MailItemInfo> _mailInfos;
        private string _filePath;

        public MainView(BatchTypeModel batchType, DocTypeModel docType, string filePath, string extension, FileFormatModel fileFormat)
        {
            InitializeComponent();
            _filePath = filePath;
            DataContext = _viewModel = new MainViewModel(batchType, docType, filePath, extension, fileFormat);
            Loaded += CaptureViewLoaded;
            Unloaded += CaptureViewUnloaded;
            Closed += MainView_Closed;
        }

        public MainView(BatchTypeModel batchType, DocTypeModel docType, List<MailItemInfo> mailInfos)
        {
            InitializeComponent();
            _mailInfos = mailInfos;
            DataContext = _viewModel = new MainViewModel(batchType, docType, mailInfos);
            Loaded += CaptureViewLoaded;
            Unloaded += CaptureViewUnloaded;
            Closed += MainView_Closed;
        }

        private void CaptureViewLoaded(object sender, System.Windows.RoutedEventArgs e)
        {
            ViewerContainer.HandleException = ProcessHelper.ProcessException;
            ViewerContainer.LogException = ProcessHelper.LogException;
            ViewerContainer.AddActionLog = ProcessHelper.AddActionLog;

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

        void MainView_Closed(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(_filePath))
            {
                if (File.Exists(_filePath))
                {
                    File.Delete(_filePath);
                }
            }

            if (_mailInfos != null)
            {
                foreach (MailItemInfo item in _mailInfos)
                {
                    if (Directory.Exists(item.TempFolderName))
                    {
                        Directory.Delete(item.TempFolderName, true);
                    }
                }
            }
        }

    }
}
