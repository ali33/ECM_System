using System.Windows.Controls;
using Ecm.CaptureViewer.Model;

namespace Ecm.CaptureViewer.Extension
{
    public static class TreeViewItemExtensions
    {
        public static int GetDepth(this TreeViewItem item)
        {
            var data = item.Header as ContentItem;
            if (data != null)
            {
                return data.GetDepth();
            }

            return 0;
        }

        public static int GetDepth(this ContentItem item)
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
