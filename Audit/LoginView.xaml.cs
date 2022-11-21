using System.Windows;
using Ecm.Audit.ViewModel;

namespace Ecm.Audit
{
    public partial class LoginView
    {
        public LoginView()
        {
            InitializeComponent();
            _viewModel = new LoginViewModel();
            DataContext = _viewModel;
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
