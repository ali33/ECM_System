using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Ecm.Workflow.Activities.BarcodeExecutorDesigner.ViewModel;

namespace Ecm.Workflow.Activities.BarcodeExecutorDesigner.View
{
    public partial class BarcodeConfigurationView
    {
        private BarcodeConfigurationViewModel _viewModel;

        public DialogViewer Dialog { get; set; }

        public BarcodeConfigurationView(BarcodeConfigurationViewModel viewModel)
        {
            InitializeComponent();
            DataContext = _viewModel = viewModel;
            Loaded += BarcodeConfigurationViewLoaded;
        }

        //public void Initialize(BarcodeConfigurationActivity barcodeActivity, BatchProfile batchProfile, List<DocType> docTypes, bool isCopiedFromClientProcessing)
        //{
        //    _viewModel = new BarcodeConfigurationViewModel(batchProfile, docTypes, barcodeActivity);
        //    _viewModel.IsModified = isCopiedFromClientProcessing;
        //    DataContext = _viewModel;
        //}

        //public BarcodeConfigurationActivity CurrentBarcodeActivity
        //{
        //    get { return _viewModel.CurrentBarcodeActivity; }
        //}

        public bool TransferBarcodeToClientProcessing
        {
            get { return _viewModel.TransferBarcodeToClientProcessing; }
        }

        //public event EventHandler SaveAndCloseWindow;
        //public event EventHandler CloseWindow;

        //private void CanExecuteSave(object sender, CanExecuteRoutedEventArgs e)
        //{
        //    e.CanExecute = (_viewModel != null && _viewModel.SaveBarcodeConfigurationCommand.CanExecute(null));
        //}

        //private void ExecuteSave(object sender, ExecutedRoutedEventArgs e)
        //{
        //    if (_viewModel != null)
        //    {
        //        _viewModel.SaveBarcodeConfigurationCommand.Execute(null);
        //    }

        //    if (SaveAndCloseWindow != null)
        //    {
        //        SaveAndCloseWindow(this, EventArgs.Empty);
        //    }
        //}

        //private void CanExecuteCancel(object sender, CanExecuteRoutedEventArgs e)
        //{
        //    e.CanExecute = true;
        //}

        //private void ExecuteCancel(object sender, ExecutedRoutedEventArgs e)
        //{
        //    if (CloseWindow != null)
        //    {
        //        CloseWindow(this, EventArgs.Empty);
        //    }
        //}

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
