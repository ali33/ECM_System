using System.Windows;
using Ecm.CaptureAdmin.ViewModel;

namespace Ecm.CaptureAdmin
{
    /// <summary>
    /// Interaction logic for LoginView.xaml
    /// </summary>
    public partial class LoginView
    {
        public LoginView()
        {
            InitializeComponent();

            DataContext = new LoginViewModel();
            txtUserName.Focus();
        }

        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            LoginViewModel viewModel = DataContext as LoginViewModel;
            if (viewModel != null)
            {
                viewModel.HasError = false;
                viewModel.Error = string.Empty;
            }
        }
    }
}
