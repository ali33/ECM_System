using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Ecm.CaptureAdmin.ViewModel;

namespace Ecm.CaptureAdmin.View
{
    public partial class BarcodeConfigurationView
    {
        private BarcodeConfigurationViewModel _viewModel;

        public DialogBaseView Dialog { get; set; }

        public BarcodeConfigurationView(BarcodeConfigurationViewModel viewModel)
        {
            InitializeComponent();
            DataContext = _viewModel = viewModel;
            Loaded += BarcodeConfigurationViewLoaded;
        }

        private void BarcodeConfigurationViewLoaded(object sender, RoutedEventArgs e)
        {
            UpdateGridView(lvwSeparation);
            UpdateGridView(lvwDataExtraction);
        }

        private void UpdateGridView(ListView listView)
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

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Dialog.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.Dialog.Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Dialog.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            Dialog.Close();
        }
    }
}
