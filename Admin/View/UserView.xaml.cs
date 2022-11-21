using System.ComponentModel;
using Ecm.Model;
using Ecm.Admin.ViewModel;
using System.Threading;
using System.Linq;

namespace Ecm.Admin.View
{
    /// <summary>
    /// Interaction logic for UserView.xaml
    /// </summary>
    public partial class UserView
    {
        private UserViewModel _viewModel;
        private bool _isDesc;

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
                    _viewModel.IsEditMode = false;
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
                                                  ApplyForCapture = user.ApplyForCapture
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

        private void Sort_Event(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (_isDesc)
            {
                _viewModel.SortCommand.Execute(true);
                _isDesc = false;
            }
            else
            {
                _viewModel.SortCommand.Execute(false);
                _isDesc = true;
            }
            //if (_isDesc)
            //{
            //    var desc = from u in _viewModel.Users orderby u.Username descending select u;
            //    _viewModel.Users.Clear();
            //    _viewModel.Users = new System.Collections.ObjectModel.ObservableCollection<UserModel>(desc);
            //    _isDesc = false;
            //}
            //else
            //{
            //    var asc = from u in _viewModel.Users orderby u.Username ascending select u;
            //    _viewModel.Users.Clear();
            //    _viewModel.Users = new System.Collections.ObjectModel.ObservableCollection<UserModel>(asc);
            //    _isDesc = true;
            //}
        }
    }
}
