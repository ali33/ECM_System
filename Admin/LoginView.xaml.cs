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
using System.ComponentModel;
using System.Threading;
using Ecm.Admin.ViewModel;
using Ecm.Admin.View;
using Ecm.Model;
using System.Globalization;
using Ecm.Localization;

namespace Ecm.Admin
{
    /// <summary>
    /// Interaction logic for LoginView.xaml
    /// </summary>
    public partial class LoginView : System.Windows.Controls.Page
    {
        private DialogBaseView _dialog;

        public LoginView()
        {
            InitializeComponent();

            DataContext = new LoginViewModel();
            txtUserName.Focus();
        }

        private void btnForgotPasswrod_Click(object sender, RoutedEventArgs e)
        {
            LoginViewModel viewModel = DataContext as LoginViewModel;
            viewModel.CloseDialog += new CloseDialog(viewModel_CloseDialog);
            viewModel.ForgotPasswordUser = new UserModel();
            _dialog = new DialogBaseView(new ForgotPasswordView(viewModel));
            _dialog.Text = "Forgot password";
            _dialog.Size = new System.Drawing.Size(400, 250);
            _dialog.ShowDialog();
        }

        void viewModel_CloseDialog()
        {
            if (_dialog != null)
            {
                _dialog.Close();
            }
        }

        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            LoginViewModel viewModel = DataContext as LoginViewModel;
            viewModel.HasError = false;
            viewModel.Error = string.Empty;
        }
    }
}
