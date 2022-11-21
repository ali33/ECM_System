using System.ComponentModel;
using System.Threading;
using Ecm.CaptureAdmin.ViewModel;
using Ecm.CaptureModel;

namespace Ecm.CaptureAdmin.View
{
    /// <summary>
    /// Interaction logic for UserView.xaml
    /// </summary>
    public partial class UserView
    {
        private UserViewModel _viewModel;

        public UserView()
        {
            InitializeComponent();
            Loaded += UserViewLoaded;
        }

        private void UserViewLoaded(object sender, System.Windows.RoutedEventArgs e)
        {
            _viewModel = DataContext as UserViewModel;
            if (_viewModel != null)
            {
                _viewModel.PropertyChanged += ViewModel_PropertyChanged;
            }
        }

        private void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "SelectedUser")
            {
                UserModel user = _viewModel.SelectedUser;
                if (user != null)
                {
                    user.ErrorChecked = _viewModel.CheckUserExist;
                    _viewModel.EditUser = new UserModel
                                              {
                                                  Description = user.Description,
                                                  EmailAddress = user.EmailAddress,
                                                  Fullname = user.Fullname,
                                                  Id = user.Id,
                                                  IsAdmin = user.IsAdmin,
                                                  Language = user.Language,
                                                  LanguageId = user.LanguageId,
                                                  Password = user.Password,
                                                  Type = user.Type,
                                                  UserGroups = user.UserGroups,
                                                  Username = user.Username,
                                                  ErrorChecked = user.ErrorChecked,
                                                  Picture = user.Picture,
                                                  ApplyForArchive = user.ApplyForArchive
                                              };

                    _viewModel.ResetListView = ResetList;
                }
            }
            if (e.PropertyName == "EditUser" || e.PropertyName == "SelectedUser")
            {
                Dispatcher.BeginInvoke((ThreadStart)(() => txtUsername.Focus()));
            }
        }

        private void ResetList()
        {
            lvUsers.SelectedIndex = -1;
        }

        private void Grid_IsVisibleChanged(object sender, System.Windows.DependencyPropertyChangedEventArgs e)
        {
            Dispatcher.BeginInvoke((ThreadStart)(() => txtUsername.Focus()));
        }
    }
}
