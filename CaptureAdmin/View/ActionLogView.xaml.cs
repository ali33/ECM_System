using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Ecm.CaptureAdmin.View
{
    /// <summary>
    /// Interaction logic for ActionLogView.xaml
    /// </summary>
    public partial class ActionLogView : UserControl
    {
        public ActionLogView()
        {
            InitializeComponent();
        }

        private void lvActionLog_TargetUpdated(object sender, DataTransferEventArgs e)
        {
            var listView = sender as ListView;
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

        private void lvActionLog_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void lvActionLog_EndSortingProcess(object sender, EventArgs e)
        {
            lvActionLog_TargetUpdated(sender, null);
        }

        private void pnMainSearch_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (pnMainSearch.ActualHeight <= 35)
            {
                SearchPanel.Visibility = Visibility.Collapsed;
                SearchButtonPanel.Visibility = Visibility.Collapsed;
                btnExpandSearch.IsChecked = true;
            }
            else
            {
                SearchPanel.Visibility = Visibility.Visible;
                SearchButtonPanel.Visibility = Visibility.Visible;
                btnExpandSearch.IsChecked = false;
            }


        }

        private void btnExpandSearch_Click(object sender, RoutedEventArgs e)
        {
            if (SearchButtonPanel.Visibility == Visibility.Collapsed && SearchPanel.Visibility == Visibility.Collapsed)
            {
                SearchPanel.Visibility = Visibility.Visible;
                SearchButtonPanel.Visibility = Visibility.Visible;
                pnMainSearch.Height = 150;
            }
            else
            {
                SearchPanel.Visibility = Visibility.Collapsed;
                SearchButtonPanel.Visibility = Visibility.Collapsed;
                pnMainSearch.Height = 20;
            }

        }

    }
}
