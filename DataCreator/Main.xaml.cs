using System.Windows.Input;
using Ecm.DataCreator.ViewModel;

namespace Ecm.DataCreator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private readonly MainViewModel _viewModel;

        public MainWindow()
        {
            InitializeComponent();
            _viewModel = new MainViewModel(CloseView);
            DataContext = _viewModel;

            Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            cboOption.SelectedIndex = 0;
        }

        private void cboDatabase_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            _viewModel.LoadDatabaseName();
        }

        private void cboOption_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (_viewModel != null)
            {
                switch (cboOption.SelectedIndex)
                {
                    case 0:
                        _viewModel.DatabaseMode = DbMode.CreateNewArchive;
                        break;
                    case 1:
                        _viewModel.DatabaseMode = DbMode.CreateNewCapture;
                        break;
                    case 2:
                        _viewModel.DatabaseMode = DbMode.CreateNewArchiveAndCapture;
                        break;
                    case 3:
                        _viewModel.DatabaseMode = DbMode.IncludeNewCapture;
                        break;
                    case 4:
                        _viewModel.DatabaseMode = DbMode.IncludeNewArchive;
                        break;
                }
            }
        }

        private void CloseView()
        {
            Close();
        }
    }
}
