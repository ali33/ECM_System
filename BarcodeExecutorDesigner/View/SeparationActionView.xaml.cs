using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Forms;
using TextBox = System.Windows.Controls.TextBox;
using Ecm.Workflow.Activities.BarcodeExecutorDesigner.ViewModel;
using Ecm.Workflow.Activities.CustomActivityModel;
using Ecm.CaptureDomain;
using System.Windows.Data;

namespace Ecm.Workflow.Activities.BarcodeExecutorDesigner.View
{
    public partial class SeparationActionView
    {
        private SeparationActionViewModel _viewModel;

        public SeparationActionView()
        {
            InitializeComponent();
            Unloaded += SeparationActionViewUnloaded;
        }

        public void Initialize(SeparationActionModel separationActionModel, List<DocumentType> docTypes)
        {
            _viewModel = new SeparationActionViewModel(separationActionModel, docTypes);
            _viewModel.CloseWindow += CloseWindow;
            _viewModel.SaveAndCloseWindow += SaveAndCloseWindow;
            DataContext = _viewModel;
        }

        public DialogViewer Dialog;

        private void SaveAndCloseWindow(object sender, System.EventArgs e)
        {
            Dialog.DialogResult = DialogResult.Yes;
            Dialog.Close();
        }

        private void CloseWindow(object sender, System.EventArgs e)
        {
            Dialog.DialogResult = DialogResult.Cancel;
            Dialog.Close();
        }

        private void SeparationActionViewUnloaded(object sender, System.Windows.RoutedEventArgs e)
        {
            if (_viewModel != null)
            {
                _viewModel.CloseWindow -= CloseWindow;
                _viewModel.SaveAndCloseWindow -= SaveAndCloseWindow;
            }
        }

        private void BarcodePositionInDocTextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox positionInDoc = sender as TextBox;
            if (positionInDoc != null && _viewModel != null)
            {
                _viewModel.HasErrorOnPositionInDoc = string.IsNullOrWhiteSpace(positionInDoc.Text);
            }
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            BindingExpression be = txtBarcodePosition.GetBindingExpression(TextBox.TextProperty);
            be.UpdateSource();

        }
    }
}
