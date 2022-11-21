using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

using Ecm.Workflow.Activities.CustomActivityModel;

namespace Ecm.Workflow.Activities.HumanStepPermissionDesigner.View
{
    /// <summary>
    /// Interaction logic for UserGroupSelectionView.xaml
    /// </summary>
    public partial class UserGroupSelectionView
    {
        public static readonly DependencyProperty UserGroupsProperty =
            DependencyProperty.Register("UserGroups", typeof(ObservableCollection<UserGroupModel>), typeof(UserGroupSelectionView));

        public UserGroupSelectionView()
        {
            InitializeComponent();
        }

        public DialogViewer Dialog { get; set; }

        public ObservableCollection<UserGroupModel> UserGroups
        {
            get { return GetValue(UserGroupsProperty) as ObservableCollection<UserGroupModel>; }
            set { SetValue(UserGroupsProperty, value); }
        }


        private void OkButtonClick(object sender, RoutedEventArgs e)
        {
            Dialog.DialogResult = System.Windows.Forms.DialogResult.OK;
            Dialog.Close();
        }

        private void CancelButtonClick(object sender, RoutedEventArgs e)
        {
            Dialog.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            Dialog.Close();
        }

        private void LvwUserGroupTargetUpdated(object sender, DataTransferEventArgs e)
        {
            ListView listview = sender as ListView;

            if (listview != null)
            {
                GridView gridView = listview.View as GridView;

                if (gridView != null)
                {
                    UpdateColumnWidths(gridView);
                }
            }
        }

        private void UpdateColumnWidths(GridView gridView)
        {
            int count = gridView.Columns.Count;
            for (int i = 1; i < count; i++)
            {
                GridViewColumn column = gridView.Columns[i];
                column.Width = 0;
                column.Width = double.NaN;
            }
        }
    }
}
