using System.Windows;
using System.Windows.Controls;
using Ecm.Admin.ViewModel;

namespace Ecm.Admin.View
{
    /// <summary>
    /// Interaction logic for PermissionView.xaml
    /// </summary>
    public partial class PermissionView
    {
        public PermissionView()
        {
            InitializeComponent();
        }

        private void PermissionViewLoaded(object sender, RoutedEventArgs e)
        {
            _viewModel = DataContext as PermissionViewModel;

            if (_viewModel != null)
            {
                _viewModel.SaveCompleted = SaveCompleted;
            }
        }

        private void TvwPermissionSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            var tree = sender as TreeView;

            if (tree != null)
            {
                var selectedMenu = tree.SelectedItem as Model.MenuModel;

                if (selectedMenu != null)
                {
                    _viewModel.PermissionSelectedCommand.Execute(selectedMenu);
                }
            }
        }

        private void SaveCompleted()
        {
            var item = tvwPermission.SelectedItem as Model.MenuModel;

            if (item != null)
            {
                item.IsSelected = false;
                tvwPermission.Focus();
            }
        }

        private PermissionViewModel _viewModel;
    }
}
