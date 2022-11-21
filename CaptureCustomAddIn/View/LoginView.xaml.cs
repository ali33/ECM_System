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
using Ecm.CaptureCustomAddIn.ViewModel;

namespace Ecm.CaptureCustomAddIn.View
{
    /// <summary>
    /// Interaction logic for LoginView.xaml
    /// </summary>
    public partial class LoginView : Window
    {
        public LoginView(string title, AddinType addinType)
        {
            InitializeComponent();
            this.Title = title;
            LoginViewModel viewModel = new LoginViewModel(LoginSuccess);
            viewModel.AddinType = addinType;
            DataContext = viewModel;
            txtUsername.Focus();
            this.Closed += LoginView_Closed;
        }

        void LoginView_Closed(object sender, EventArgs e)
        {
        }

        private void LoginSuccess()
        {
            this.Close();
        }
    }
}
