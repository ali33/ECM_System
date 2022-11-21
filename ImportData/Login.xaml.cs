using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Ecm.ImportData
{
    /// <summary>
    /// Interaction logic for Login.xaml
    /// </summary>
    public partial class Login : Window
    {
        bool isGetValue=false;
        bool isCancel = false;
        public Login()
        {
            InitializeComponent();
        }

        private void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            if(!String.IsNullOrWhiteSpace(txtUserName.Text) &&
                !String.IsNullOrWhiteSpace(txtPassword.Password)){
                isGetValue = true;
                this.Close();
            }
        }      

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            isCancel = true;
            this.Close();
        }

        public bool IsGetValue
        {
            get
            {
                return isGetValue;
            }
        }
        public bool IsCancel
        {
            get
            {
                return isCancel;
            }
            set
            {
                isCancel = value;
            }
        }
        
        public string UserName
        {
            get
            {
                return txtUserName.Text;
            }
        }

        public string Password
        {
            get
            {
                return txtPassword.Password;
            }
        }

        
    }
}
