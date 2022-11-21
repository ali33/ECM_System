using System.Windows;
using Ecm.Archive.ViewModel;
using System.Windows.Controls;

namespace Ecm.Archive
{
    public partial class LoginView : Page
    {
        public LoginView()
        {
            InitializeComponent();
            _viewModel = new LoginViewModel();
            DataContext = _viewModel;
            txtUserName.Focus();
        }

        //private void btnForgotPasswrod_Click(object sender, RoutedEventArgs e)
        //{
        //    LoginViewModel viewModel = DataContext as LoginViewModel;
        //    viewModel.CloseDialog += new CloseDialog(viewModel_CloseDialog);
        //    viewModel.ForgotPasswordUser = new User();
        //    _dialog = new DialogBaseView(new ForgotPasswordView(viewModel));
        //    _dialog.Text = "Forgot password";
        //    _dialog.Size = new System.Drawing.Size(400, 150);
        //    _dialog.ShowDialog();
        //}

        //void viewModel_CloseDialog()
        //{
        //    if (_dialog != null)
        //        _dialog.Close();
        //}

        private void TextBoxGotFocus(object sender, RoutedEventArgs e)
        {
            _viewModel.HasError = false;
            _viewModel.Error = string.Empty;
        }

        private readonly LoginViewModel _viewModel;
    }
}
