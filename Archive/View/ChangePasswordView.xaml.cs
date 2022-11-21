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
using Ecm.Archive.ViewModel;

namespace Ecm.Archive.View
{
    /// <summary>
    /// Interaction logic for ChangePasswordView.xaml
    /// </summary>
    public partial class ChangePasswordView
    {
        public ChangePasswordView(ChangePasswordViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }

        private void CanOk(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = (DataContext as ChangePasswordViewModel).CanChangePassword();
        }

        private void Ok(object sender, ExecutedRoutedEventArgs e)
        {
            (DataContext as ChangePasswordViewModel).ChangePassword();
        }

        private void Cancel(object sender, ExecutedRoutedEventArgs e)
        {
            (DataContext as ChangePasswordViewModel).Cancel();
        }

        private void SetFocusToOldPassword(object sender, EventArgs e)
        {
            txtOldPassword.SelectAll();
        }
    }
}
