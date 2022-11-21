using Ecm.Admin.ViewModel;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Ecm.Admin.View
{
    public partial class TableConfigurationView : UserControl
    {
        private TableConfigurationViewModel _viewModel;

        public TableConfigurationView(TableConfigurationViewModel viewModel)
        {
            InitializeComponent();
            this.DataContext = _viewModel = viewModel;
            Loaded += TableConfigurationViewLoaded;
        }

        private void TableConfigurationViewLoaded(object sender, RoutedEventArgs e)
        {
            if (DataContext != null)
            {
                ListView_TargetUpdated(lvwTableDataTypeView, null);
            }
        }

        private void TableConfigurationView_ColumnChanged(object sender, EventArgs e)
        {
            ListView_TargetUpdated(lvwTableDataTypeView, null);
        }

        private void ListView_TargetUpdated(object sender, DataTransferEventArgs e)
        {
            // Get a reference to the ListView's GridView...
            ListView listview = sender as ListView;

            if (listview != null)
            {
                listview.UpdateLayout();

                GridView gridView = listview.View as GridView;

                if (gridView != null)
                {
                    UpdateColumnWidths(gridView);
                }
            }
        }

        // Technique for updating column widths of a ListView's GridView manually
        private void UpdateColumnWidths(GridView gridView)
        {
            // For each column...
            foreach (GridViewColumn column in gridView.Columns)
            {
                column.Width = 0;
                column.Width = double.NaN;
            }
        }
    }
}
