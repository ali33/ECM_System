using System;
using System.Windows.Controls;
using Ecm.CaptureAdmin.ViewModel;
using Ecm.CaptureModel;
using Ecm.Mvvm;

namespace Ecm.CaptureAdmin
{
    /// <summary>
    /// Interaction logic for Page1.xaml
    /// </summary>
    public partial class MainView
    {
        public MainView()
        {
            InitializeComponent();
            _viewModel = new MainViewModel();
            DataContext = _viewModel;
        }

        private void AdminMenuSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                var viewModel = DataContext as MainViewModel;
                var menuItem = (sender as ListBox).SelectedItem as MenuModel;

                if (viewModel != null && menuItem != null)
                {
                    switch (menuItem.MenuName)
                    {
                        case Common.MENU_BATCH_TYPE:
                            viewModel.ViewModel = new BatchTypeViewModel(viewModel);
                            break;
                        case Common.MENU_USER:
                            viewModel.ViewModel = new UserViewModel();
                            break;
                        case Common.MENU_USER_GROUP:
                            viewModel.ViewModel = new UserGroupViewModel();
                            break;
                        case Common.MENU_PERMISSION:
                            viewModel.ViewModel = new PermissionViewModel();
                            break;
                        case Common.MENU_ACTION_LOG:
                            viewModel.ViewModel = new ActionLogViewModel();
                            break;
                        case Common.MENU_SETTING:
                            viewModel.ViewModel = new SettingViewModel();
                            break;
                    }
                    viewModel.ViewModel.Initialize();
                    viewModel.ViewModel.EditPanelVisibled = false;
                    //try
                    //{
                    //    viewModel.IsProcessing = true;

                    //    if (menuItem.ViewModel == null)
                    //    {
                    //        LoginViewModel.LoginUser = new UserModel();
                    //        NavigationHelper.Navigate(new Uri("LoginView.xaml", UriKind.RelativeOrAbsolute));
                    //    }
                    //    else
                    //    {
                    //        menuItem.ViewModel.EditPanelVisibled = false;
                    //        menuItem.ViewModel.Initialize();
                    //        viewModel.ViewModel = menuItem.ViewModel;
                    //    }
                    //}
                    //finally
                    //{
                    //    viewModel.IsProcessing = false;
                    //}
                }
            }
            catch (Exception ex)
            {
                ProcessHelper.ProcessException(ex);
            }
        }

        private readonly MainViewModel _viewModel;
    }
}
