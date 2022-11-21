using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Forms;
using TextBox = System.Windows.Controls.TextBox;
using Ecm.CaptureDomain;
using Ecm.CaptureModel;
using Ecm.CaptureAdmin.ViewModel;

namespace Ecm.CaptureAdmin.View
{
    public partial class ReadActionView
    {
        private ReadActionViewModel _viewModel;
        
        public ReadActionView()
        {
            InitializeComponent();
            Unloaded += ReadActionViewUnloaded;
        }

        public void Initialize(ReadActionModel readActionModel, List<DocTypeModel> docTypes, BatchTypeModel batchType)
        {
            _viewModel = new ReadActionViewModel(readActionModel, docTypes, batchType);
            _viewModel.CloseWindow += CloseWindow;
            _viewModel.SaveAndCloseWindow += SaveAndCloseWindow;
            DataContext = _viewModel;
        }

        public DialogBaseView Dialog;

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

        private void ReadActionViewUnloaded(object sender, System.Windows.RoutedEventArgs e)
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
                _viewModel.HasErrorWithPositionInDoc = string.IsNullOrWhiteSpace(positionInDoc.Text);
            }
        }
    }
}
