using System.Windows;
using Ecm.Capture.ViewModel;

namespace Ecm.Capture
{
    public partial class LoginView
    {
        public LoginView()
        {
            InitializeComponent();

            _viewModel = new LoginViewModel();
            DataContext = _viewModel;
            txtUserName.Focus();
        }

        private void TextBoxGotFocus(object sender, RoutedEventArgs e)
        {
            _viewModel.HasError = false;
            _viewModel.Error = string.Empty;
        }

        private readonly LoginViewModel _viewModel;
    }
}
