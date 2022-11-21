using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using Ecm.CaptureDomain;
using Ecm.Workflow.Activities.HumanStepPermissionDesigner.ViewModel;
using TreeView = System.Windows.Controls.TreeView;
using Ecm.Workflow.Activities.CustomActivityModel;

namespace Ecm.Workflow.Activities.HumanStepPermissionDesigner.View
{
    /// <summary>
    /// Interaction logic for HumanStepPermissionView.xaml
    /// </summary>
    public partial class HumanStepPermissionView
    {
        public HumanStepPermissionView()
        {
            InitializeComponent();
        }

        public void InitializeViewModel(HumanStepPermissionViewModel viewModel)
        {
            _viewModel = viewModel;
            DataContext = viewModel;
        }

        public Form Form { get; set; }

        private void OkButtonClick(object sender, RoutedEventArgs e)
        {
            if (Form != null)
            {
                Form.DialogResult = DialogResult.OK;
                Form.Close();
            }
        }

        private void CancelButtonClick(object sender, RoutedEventArgs e)
        {
            if (Form != null)
            {
                Form.DialogResult = DialogResult.Cancel;
                Form.Close();
            }
        }

        private void TvwUserGroupSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            TreeView treeView = sender as TreeView;
            if (treeView != null)
            {
                MenuItemModel selectedMenuItem = treeView.SelectedItem as MenuItemModel;

                if (selectedMenuItem != null)
                {
                    //_viewModel.SetSelection(selectedMenuItem);

                    if (selectedMenuItem.Type == MenuItemType.Root)
                    {
                        _viewModel.IsViewAvailable = false;
                    }
                    else
                    {
                        _viewModel.IsViewAvailable = true;

                        if ((_viewModel.SelectedUserGroup != null && _viewModel.SelectedUserGroup.Id != selectedMenuItem.Id) ||
                            _viewModel.SelectedUserGroup == null)
                        {
                            _viewModel.SelectedUserGroup = _viewModel.UserGroups.FirstOrDefault(p => p.Id == selectedMenuItem.Id);
                            _viewModel.InitializeUserGroupPermission();
                            _viewModel.InitializeAnnotationPermission();
                            _viewModel.InitializeDocumentTypePermission();
                        }
                    }
                }
            }
        }

        private void AddUserGroupClick(object sender, RoutedEventArgs e)
        {
            DialogViewer dialog = new DialogViewer
                                      {
                                          Width = 640,
                                          Height = 480,
                                          Text = "Select user groups"
                                      };

            List<UserGroup> groups = _viewModel.GetUnassignedUserGroups();
            ObservableCollection<UserGroupModel> groupModels = new ObservableCollection<UserGroupModel>();
            groups.ForEach(p => groupModels.Add(new UserGroupModel
                                                    {
                                                        Id = p.Id,
                                                        Name = p.Name,
                                                        Description = string.Empty
                                                    }));

            var groupView = new UserGroupSelectionView
                                {
                                    Dialog = dialog,
                                    UserGroups = groupModels
                                };

            dialog.WpfContent = groupView;

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                List<UserGroupModel> selectedGroups = groupView.UserGroups.Where(p => p.IsSelected).ToList();
                if (selectedGroups.Count > 0)
                {
                    _viewModel.AddUserGroups(groups.Where(p => selectedGroups.Any(q => q.Id == p.Id)).OrderBy(p => p.Name).ToList());
                }
            }
        }

        private void RemoveUserGroupClick(object sender, RoutedEventArgs e)
        {
            MenuItemModel menuItem = tvwUserGroup.SelectedItem as MenuItemModel;
            _viewModel.RemoveUserGroup(menuItem);
        }

        private HumanStepPermissionViewModel _viewModel;
    }
}
