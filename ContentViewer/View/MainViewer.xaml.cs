using Ecm.ContentViewer.Model;
using Ecm.ContentViewer.ViewModel;
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

namespace Ecm.ContentViewer.View
{
    /// <summary>
    /// Interaction logic for MainViewer.xaml
    /// </summary>
    public partial class MainViewer : UserControl
    {
        private MainViewerViewModel _viewModel;

        public MainViewer()
        {
            InitializeComponent();
            Initialize();
            DataContext = _viewModel = new MainViewerViewModel();
        }

        public RoutedCommand ScanCommand;

        public RoutedCommand ImportCommand;

        public RoutedCommand CameraCommand;

        public RoutedCommand SelectDefaultScannerCommand;

        public RoutedCommand ShowHideScannerDialogCommand;

        public RoutedCommand ScanToDocumentTypeCommand;

        public RoutedCommand ImportFileSystemToDocumentTypeCommand;

        public RoutedCommand SelectDefaultCameraCommand;

        public RoutedCommand SelectDefaultMicCommand;

        public RoutedCommand CaptureContentToDocumentTypeCommand;

        private void Initialize()
        {
            var gesture = new InputGestureCollection { new KeyGesture(Key.S, ModifierKeys.Control) };
            ScanCommand = new RoutedCommand("Scan", typeof(MainViewer), gesture);
            var commandBinding = new CommandBinding(ScanCommand, ScanContent, CanScanContent);
            this.CommandBindings.Add(commandBinding);

            gesture = new InputGestureCollection { new KeyGesture(Key.Q, ModifierKeys.Control) };
            SelectDefaultScannerCommand = new RoutedCommand("SelectDefaultScanner", typeof(MainViewer), gesture);
            commandBinding = new CommandBinding(SelectDefaultScannerCommand, SelectDefaultScanner);
            this.CommandBindings.Add(commandBinding);

            gesture = new InputGestureCollection { new KeyGesture(Key.T, ModifierKeys.Control) };
            ShowHideScannerDialogCommand = new RoutedCommand("ShowHideScannerDialog", typeof(MainViewer), gesture);
            commandBinding = new CommandBinding(ShowHideScannerDialogCommand, ShowHideScannerDialog);
            this.CommandBindings.Add(commandBinding);

            ScanToDocumentTypeCommand = new RoutedCommand("ScanToDocumentType", typeof(MainViewer));
            commandBinding = new CommandBinding(ScanToDocumentTypeCommand, ScanToDocumentType, CanScanToDocumentType);
            this.CommandBindings.Add(commandBinding);

            gesture = new InputGestureCollection { new KeyGesture(Key.I, ModifierKeys.Control) };
            ImportCommand = new RoutedCommand("ImportFile", typeof(MainViewer), gesture);
            commandBinding = new CommandBinding(ImportCommand, ImportFile, CanImportFile);
            this.CommandBindings.Add(commandBinding);

            ImportFileSystemToDocumentTypeCommand = new RoutedCommand("ImportFileToDocumentType", typeof(MainViewer));
            commandBinding = new CommandBinding(ImportFileSystemToDocumentTypeCommand, ImportFileToDocumentType, CanImportFileToDocumentType);
            this.CommandBindings.Add(commandBinding);

            gesture = new InputGestureCollection { new KeyGesture(Key.I, ModifierKeys.Control) };
            CameraCommand = new RoutedCommand("CameraCapture", typeof(MainViewer), gesture);
            commandBinding = new CommandBinding(CameraCommand, CameraCapture, CanCameraCapture);
            this.CommandBindings.Add(commandBinding);

            gesture = new InputGestureCollection { new KeyGesture(Key.W, ModifierKeys.Control) };
            SelectDefaultCameraCommand = new RoutedCommand("SelectDefaultCamera", typeof(MainViewer), gesture);
            commandBinding = new CommandBinding(SelectDefaultCameraCommand, SelectDefaultCamera, CanSelectDefaultMediaDevice);
            this.CommandBindings.Add(commandBinding);

            gesture = new InputGestureCollection { new KeyGesture(Key.E, ModifierKeys.Control) };
            SelectDefaultMicCommand = new RoutedCommand("SelectDefaultMic", typeof(MainViewer), gesture);
            commandBinding = new CommandBinding(SelectDefaultMicCommand, SelectDefaultMicroPhone, CanSelectDefaultMediaDevice);
            this.CommandBindings.Add(commandBinding);

            CaptureContentToDocumentTypeCommand = new RoutedCommand("CaptureContentToDocumentType", typeof(MainViewer));
            commandBinding = new CommandBinding(CaptureContentToDocumentTypeCommand, CaptureContentToDocumentType, CanCaptureContentToDocumentType);
            this.CommandBindings.Add(commandBinding);
        }

        private void CanScanContent(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = _viewModel.Items != null && _viewModel.Items.Count > 0;
        }

        private void ScanContent(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                _viewModel.ScanningManager.ScanContent();
            }
            catch (Exception ex)
            {
                _viewModel.HandleException(ex);
            }
        }

        private void SelectDefaultScanner(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                _viewModel.ScanningManager.SelectDefaultScanner();
            }
            catch (Exception ex)
            {
                _viewModel.HandleException(ex);
            }
        }

        private void ShowHideScannerDialog(object sender, ExecutedRoutedEventArgs e)
        {
            _viewModel.ShowHideScannerDialog(); 
        }

        private void CanScanToDocumentType(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = _viewModel.Items != null && _viewModel.Items.Count > 0;
        }

        private void ScanToDocumentType(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                _viewModel.ScanningManager.ScanToDocumentType();

            }
            catch (Exception ex)
            {
                _viewModel.HandleException(ex);
            }
        }

        private void CanImportFile(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = _viewModel.Items != null && _viewModel.Items.Count > 0;
        }

        private void ImportFile(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                _viewModel.ImportManager.ImportFile();
            }
            catch (Exception ex)
            {
                _viewModel.HandleException(ex);
            }
        }

        private void CanImportFileToDocumentType(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = _viewModel.Items != null && _viewModel.Items.Count > 0;
        }

        private void ImportFileToDocumentType(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                _viewModel.ImportManager.ImportFileToDocumentType(e.Parameter as ContentTypeModel);
            }
            catch (Exception ex)
            {
                _viewModel.HandleException(ex);
            }
        }

        private void CanCaptureContent(object sender, CanExecuteRoutedEventArgs e)
        {
            
        }

        private void CameraCapture(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
            }
            catch (Exception ex)
            {
                _viewModel.HandleException(ex);
            }
        }

        private void CanCaptureContentToDocumentType(object sender, CanExecuteRoutedEventArgs e)
        {
        }

        private void CaptureContentToDocumentType(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {

            }
            catch (Exception ex)
            {
                _viewModel.HandleException(ex);
            }
        }

        private void CanSelectDefaultMediaDevice(object sender, CanExecuteRoutedEventArgs e)
        {
        }

        private void SelectDefaultCamera(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
            }
            catch (Exception ex)
            {
                _viewModel.HandleException(ex);
            }
        }

        private void SelectDefaultMicroPhone(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
            }
            catch (Exception ex)
            {
                _viewModel.HandleException(ex);
            }
        }

        #region Event

        private void LeftPanelExpandCollapseClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (LeftPanelExpandCollapse.IsChecked != null && LeftPanelExpandCollapse.IsChecked.Value)
                {
                    CollaspeLeftPanel();
                }
                else
                {
                    ExpandLeftPanel();
                }
            }
            catch (Exception ex)
            {
                _viewModel.HandleException(ex);
            }
        }

        private void LeftPanelContainerSizeChanged(object sender, SizeChangedEventArgs e)
        {
            try
            {
                if (LeftPanelContainer.Width < 50)
                {
                    CollaspeLeftPanel();
                    LeftPanelExpandCollapse.IsChecked = true;
                }
                else
                {
                    LeftPanelExpandCollapse.IsChecked = false;
                    LeftPanelContent.Visibility = Visibility.Visible;
                }
            }
            catch (Exception ex)
            {
                _viewModel.HandleException(ex);
            }
        }


        private void ExpandLeftPanel()
        {
            LeftPanelContent.Visibility = Visibility.Visible;
            LeftPanelContainer.Width = 350;
        }

        private void CollaspeLeftPanel()
        {
            LeftPanelContent.Visibility = Visibility.Collapsed;
            LeftPanelContainer.Width = 25;
        }


        #endregion
    }
}
