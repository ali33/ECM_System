using Ecm.CaptureCustomAddIn.ViewModel;
using Ecm.CaptureModel;
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

namespace Ecm.CaptureCustomAddIn.View
{
    /// <summary>
    /// Interaction logic for BatchTypeSelectionView.xaml
    /// </summary>
    public partial class BatchTypeSelectionView : Window
    {
        private BatchTypeSelectionViewModel _viewModel;
        public BatchTypeSelectionView(string filePath, string extension, FileFormatModel fileFormat)
        {
            InitializeComponent();
            DataContext = _viewModel = new BatchTypeSelectionViewModel(CloseView, filePath, extension, fileFormat);
        }

        public BatchTypeSelectionView(List<MailItemInfo> mailInfos)
        {
            InitializeComponent();
            DataContext = _viewModel = new BatchTypeSelectionViewModel(CloseView, mailInfos);
        }

        private void CloseView(bool obj)
        {
            this.DialogResult = obj;
            this.Close();
        }
    }
}
