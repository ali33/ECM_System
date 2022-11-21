using System;
using Ecm.Mvvm;
using System.Collections.ObjectModel;
using Ecm.Model;
using System.Resources;
using System.Reflection;
using System.Windows.Input;
using Ecm.Admin.View;

namespace Ecm.Admin.ViewModel
{
    public class MainViewModel : BaseDependencyProperty
    {
        private ComponentViewModel _viewModel;
        private ObservableCollection<MenuModel> _menuItems = new ObservableCollection<MenuModel>();
        private string _welcomeText;
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
            ViewModel = new AboutViewModel(this);
        }

        private void ViewHelp()
        {
            var res = new ResourceManager("Ecm.Admin.MainView", Assembly.GetExecutingAssembly());
            DialogBaseView dialog = new DialogBaseView(new HelpView());
            dialog.Text = res.GetString("uiHelpTitle");
            dialog.MaximizeBox = true;
            dialog.MinimizeBox = true;
            dialog.Size = new System.Drawing.Size(800, 600);
            dialog.Show();
        }

        private void LoadMenuData()
        {
            var res = new ResourceManager("Ecm.Admin.MainView", Assembly.GetExecutingAssembly());

            var menuItem = new MenuModel(res.GetString("DocumentType"), "/Resources/Images/folder.png", new DocumentTypeViewModel(this), true, res.GetString("DocumentTypeTitle"));
            MenuItems.Add(menuItem);

            menuItem = new MenuModel(res.GetString("Permission"), "/Resources/Images/security.png", new PermissionViewModel(), false, res.GetString("PermissionTitle"));
            MenuItems.Add(menuItem);

            menuItem = new MenuModel(res.GetString("User"), "/Resources/Images/user.png", new UserViewModel(), false, res.GetString("UserTitle"));
            MenuItems.Add(menuItem);

            menuItem = new MenuModel(res.GetString("UserGroup"), "/Resources/Images/usergroup.png", new UserGroupViewModel(), false, res.GetString("UserGroupTitle"));
            MenuItems.Add(menuItem);

            //menuItem = new MenuModel(res.GetString("AmbiguousDefinition"), "/Resources/Images/autocorrect.png", new AmbiguousDefinitionViewModel(), false, res.GetString("AutoCorrectTitle"));
            //MenuItems.Add(menuItem);

            menuItem = new MenuModel(res.GetString("Settings"), "/Resources/Images/option.png", new SettingViewModel(this), false, res.GetString("SettingTitle"));
            MenuItems.Add(menuItem);

            //menuItem = new MenuModel(res.GetString("Help"), "/Resources/Images/help32.png", new HelpViewModel(), false, string.Empty);
            //MenuItems.Add(menuItem);

            //menuItem = new MenuModel(res.GetString("AboutCloudECM"), "/Resources/Images/Information32.png", new AboutViewModel(), false, string.Empty);
            //MenuItems.Add(menuItem);

            //menuItem = new MenuModel(res.GetString("Logout"), "/Resources/Images/exit.png", null, false, string.Empty);
            //MenuItems.Add(menuItem);
        }
    }
}
