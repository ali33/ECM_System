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
using Ecm.OutlookClient.ViewModel;
using Ecm.OutlookClient.Model;

namespace Ecm.OutlookClient.View
{
    /// <summary>
    /// Interaction logic for LoginView.xaml
    /// </summary>
    public partial class LoginView : Window
    {
        public LoginView()
        {
            InitializeComponent();
            LoginViewModel viewModel = new LoginViewModel(LoginSuccess);
            DataContext = viewModel;
            txtUsername.Focus();
        }

        private void LoginSuccess()
        {
            this.Close();
        }


    }
}
