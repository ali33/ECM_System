using System;
using System.Windows.Controls;
using Ecm.Admin.ViewModel;
using Ecm.Model;
using Ecm.Mvvm;

namespace Ecm.Admin
{
    /// <summary>
    /// Interaction logic for Page1.xaml
    /// </summary>
    public partial class MainView
    {
        public MainView()
        {
            //CultureManager.UICulture = new CultureInfo("vi-VN");
            //Thread.CurrentThread.CurrentCulture;
            InitializeComponent();
            DataContext = new MainViewModel();

        }

        private void AdminMenuSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var viewModel = DataContext as MainViewModel;
            var menuItem = (sender as ListBox).SelectedItem as Model.MenuModel;

            if (viewModel != null && menuItem != null)
            {
                if (menuItem.ViewModel == null)
                {
                    LoginViewModel.LoginUser = new UserModel();
                    NavigationHelper.Navigate(new Uri("LoginView.xaml", UriKind.RelativeOrAbsolute));
                }
                else
                {
                    menuItem.ViewModel.EditPanelVisibled = false;
                    menuItem.ViewModel.Initialize();
                    viewModel.ViewModel = menuItem.ViewModel;
                }
            }
        }
    }
}
