using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Reflection;
using System.Resources;
using System.Threading;
using System.Windows;

using Ecm.CaptureAdmin.ViewModel;
using Ecm.CaptureModel;

namespace Ecm.CaptureAdmin.View
{
    /// <summary>
    /// Interaction logic for UserGroupView.xaml
    /// </summary>
    public partial class UserGroupView
    {
        private DialogBaseView _dialog;
        private UserGroupViewModel _viewModel;

        public UserGroupView()
        {
            InitializeComponent();
            Loaded += UserGroupViewLoaded;
        }

        private void UserGroupViewLoaded(object sender, RoutedEventArgs e)
        {
            _viewModel = DataContext as UserGroupViewModel;
            if (_viewModel != null)
            {
                _viewModel.PropertyChanged += ViewModelPropertyChanged;
            }
        }

        private void ViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "SelectedUserGroup")
            {
                UserGroupModel userGroup = _viewModel.SelectedUserGroup;
                if (userGroup != null)
                {
                    userGroup.ErrorChecked = _viewModel.CheckUserGroup;
                    _viewModel.EditUserGroup = new UserGroupModel
                    {
                        Description = userGroup.Description,
                        Id = userGroup.Id,
                        Name = userGroup.Name,
                        Type = userGroup.Type,
                        Users = userGroup.Users,
                        ErrorChecked = userGroup.ErrorChecked
                    };

                    _viewModel.ResetListView = ResetList;
                }
            }
            Dispatcher.BeginInvoke((ThreadStart)(() => txtName.Focus()));
        }

        private void BtnAddUserClick(object sender, RoutedEventArgs e)
        {
            var resource = new ResourceManager("Ecm.CaptureAdmin.SelectionUserView", Assembly.GetExecutingAssembly());

            _viewModel.SearchedUsers = new ObservableCollection<UserModel>();
            _viewModel.SearchValue = string.Empty;
            _viewModel.CloseDialog += ViewModelCloseDialog;

            var view = new SelectionUserView(_viewModel);
            _dialog = new DialogBaseView(view)
                          {
                              Text = resource.GetString("dgDialogTitle"),
                              Size = new System.Drawing.Size(600, 330)
                          };

            _dialog.ShowDialog();
        }

        void ViewModelCloseDialog()
        {
            if (_dialog != null)
            {
                _dialog.Close();
            }
        }

        private void ResetList()
        {
            lvUserGroups.SelectedIndex = -1;
        }

        private void Grid_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            Dispatcher.BeginInvoke((ThreadStart)(() => txtName.Focus()));

        }
    }
}
