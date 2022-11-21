using System;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Resources;
using Ecm.CaptureModel;
using Ecm.Mvvm;
using System.Windows.Input;
using Ecm.CaptureAdmin.View;

namespace Ecm.CaptureAdmin.ViewModel
{
    public class MainViewModel : BaseDependencyProperty
    {
        private ComponentViewModel _viewModel;
        private ObservableCollection<MenuModel> _menuItems = new ObservableCollection<MenuModel>();
        private string _welcomeText;
        private bool _isProcessing;
        private RelayCommand _showHelpCommand;
        private RelayCommand _showAboutCommand;
        private RelayCommand _exitCommand;

        public MainViewModel()
        {
            try
            {
                WelcomeText = LoginViewModel.LoginUser.Fullname;
                LoadMenuData();
            }
            catch (Exception ex)
            {
                ProcessHelper.ProcessException(ex);
            }
        }

        public ComponentViewModel ViewModel
        {
            get { return _viewModel; }
            set
            {
                _viewModel = value;
                OnPropertyChanged("ViewModel");
            }
        }

        public string WelcomeText
        {
            get { return _welcomeText; }
            set
            {
                _welcomeText = value;
                OnPropertyChanged("WelcomeText");
            }
        }

        public bool IsProcessing
        {
            get
            {
                return _isProcessing;
            }
            set
            {
                _isProcessing = value;
                OnPropertyChanged("IsProcessing");
            }
        }

        public ObservableCollection<MenuModel> MenuItems
        {
            get { return _menuItems; }
            set
            {
                _menuItems = value;
                OnPropertyChanged("MenuItems");
            }
        }

        public ICommand ShowHelpCommand
        {
            get
            {
                if (_showHelpCommand == null)
                {
                    _showHelpCommand = new RelayCommand(p => ViewHelp());
                }

                return _showHelpCommand;
            }
        }

        public ICommand ShowAboutCommand
        {
            get
            {
                if (_showAboutCommand == null)
                {
                    _showAboutCommand = new RelayCommand(p => ViewAbout());
                }

                return _showAboutCommand;
            }
        }

        public ICommand ExitCommand
        {
            get
            {
                if (_exitCommand == null)
                {
                    _exitCommand = new RelayCommand(p => Logout());
                }

                return _exitCommand;
            }
        }

        private void Logout()
        {
            LoginViewModel.LoginUser = new UserModel();
            NavigationHelper.Navigate(new Uri("LoginView.xaml", UriKind.RelativeOrAbsolute));
        }

        private void ViewAbout()
        {
            ViewModel = new AboutViewModel();
        }

        private void ViewHelp()
        {
            var res = new ResourceManager("Ecm.CaptureAdmin.MainView", Assembly.GetExecutingAssembly());
            DialogBaseView dialog = new DialogBaseView(new HelpView());
            dialog.Text = res.GetString("uiHelpTitle");
            dialog.MaximizeBox = true;
            dialog.MinimizeBox = true;
            dialog.Size = new System.Drawing.Size(800, 600);
            dialog.Show();
        }

        private void LoadMenuData()
        {
            var res = new ResourceManager("Ecm.CaptureAdmin.MainView", Assembly.GetExecutingAssembly());

            var menuItem = new MenuModel(res.GetString("BatchType"), "/Resources/Images/folder.png", true, res.GetString("BatchTypeTitle"), Common.MENU_BATCH_TYPE);
            MenuItems.Add(menuItem);

            menuItem = new MenuModel(res.GetString("User"), "/Resources/Images/user.png", false, res.GetString("UserTitle"), Common.MENU_USER);
            MenuItems.Add(menuItem);

            menuItem = new MenuModel(res.GetString("UserGroup"), "/Resources/Images/usergroup.png", false, res.GetString("UserGroupTitle"), Common.MENU_USER_GROUP);
            MenuItems.Add(menuItem);

            menuItem = new MenuModel(res.GetString("Permission"), "/Resources/Images/security.png", false, res.GetString("PermissionTitle"), Common.MENU_PERMISSION);
            MenuItems.Add(menuItem);

            menuItem = new MenuModel(res.GetString("ActionLog"), "/Resources/Images/systemfolder48.png", false, res.GetString("ActionLogTitle"), Common.MENU_ACTION_LOG);
            MenuItems.Add(menuItem);

            //menuItem = new MenuModel(res.GetString("WorkflowTracking"), "/Resources/Images/flowchart.png", new ActionLogViewModel(), false, res.GetString("WorkflowTrackingTitle"), Common.MENU_ACTION_LOG);
            //MenuItems.Add(menuItem);

            menuItem = new MenuModel(res.GetString("Settings"), "/Resources/Images/option.png", false, res.GetString("SettingTitle"), Common.MENU_SETTING);
            MenuItems.Add(menuItem);
        }
    }
}
