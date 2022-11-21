using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Ecm.CustomControl
{
    public class AutoResizeListView
    {
        public static bool GetActive(SortableListView listView)
        {
            return (bool)listView.GetValue(ActiveProperty);
        }

        public static void SetActive(SortableListView listView, bool value)
        {
            listView.SetValue(ActiveProperty, value);
        }

        public static readonly DependencyProperty ActiveProperty =
            DependencyProperty.RegisterAttached("Active", typeof(bool),
                typeof(AutoResizeListView), new UIPropertyMetadata(false, ActivePropertyChanged));

        public static void ActivePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            SortableListView listView = sender as SortableListView;
            if (listView != null)
            {
                listView.TargetUpdated += ListViewTargetUpdated;
                listView.EndSortingProcess += ListViewEndSortingProcess;
            }
        }

        static void ListViewEndSortingProcess(object sender, System.EventArgs e)
        {
            UpdateGridView(sender as ListView);
        }

        static void ListViewTargetUpdated(object sender, DataTransferEventArgs e)
        {
            UpdateGridView(sender as ListView);
        }

        private static void UpdateGridView(ListView listView)
        {
            if (listView != null)
            {
                var gridView = listView.View as GridView;

                if (gridView != null)
                {
                    foreach (GridViewColumn column in gridView.Columns)
                    {
                        column.Width = 0;
                        column.Width = double.NaN;
                    }
                }
            }
        }
    }
}
