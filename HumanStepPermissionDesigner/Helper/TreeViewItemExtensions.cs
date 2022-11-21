using System.Windows.Controls;
using Ecm.Workflow.Activities.CustomActivityModel;

namespace Ecm.Workflow.Activities.HumanStepPermissionDesigner.Helper
{
    public static class TreeViewItemExtensions
    {
        public static int GetDepth(this TreeViewItem item)
        {
            MenuItemModel data = item.Header as MenuItemModel;

            if (data != null)
            {
                return data.GetDepth();
            }

            return 0;
        }

        public static int GetDepth(this MenuItemModel item)
        {
            while (item.Parent != null)
            {
                item = item.Parent;
                return item.GetDepth() + 1;
            }

            return 0;
        }
    }
}
