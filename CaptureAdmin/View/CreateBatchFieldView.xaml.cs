using System.Windows;
using System.Windows.Controls;
using Ecm.CaptureAdmin.ViewModel;

namespace Ecm.CaptureAdmin.View
{
    /// <summary>
    /// Interaction logic for CreateBatchFieldView.xaml
    /// </summary>
    public partial class CreateBatchFieldView : DialogChildView
    {
        public CreateBatchFieldView(BatchFieldViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
            txtName.Focus();
        }

        private void cboDataTypeChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBoxItem cbi = (ComboBoxItem)cboDataType.SelectedItem; 
            string selectedText = cbi.Content.ToString();
            if ((selectedText + "").ToLower() == "boolean")
            {
                tbDefaultValue.Visibility = Visibility.Visible;
                cbxUseCurrentDate.Visibility = Visibility.Collapsed;
                txtDefaultValue.Visibility = Visibility.Collapsed;
                cbBoolean.Visibility = Visibility.Visible;
                cbBoolean.SelectedIndex = 1;
            }
            else if ((selectedText + "").ToLower() == "date")
            {
                tbDefaultValue.Visibility = Visibility.Visible;
                cbxUseCurrentDate.Visibility = Visibility.Visible;
                txtDefaultValue.Visibility = Visibility.Collapsed;
                cbBoolean.Visibility = Visibility.Collapsed;
            }
            else if ((selectedText + "").ToLower() != "picklist")
            {
                tbDefaultValue.Visibility = Visibility.Visible;
                cbxUseCurrentDate.Visibility = Visibility.Collapsed;
                txtDefaultValue.Visibility = Visibility.Visible;
                cbBoolean.Visibility = Visibility.Collapsed;
            }
            else
            {
                tbDefaultValue.Visibility = Visibility.Collapsed;
                cbxUseCurrentDate.Visibility = Visibility.Collapsed;
                txtDefaultValue.Visibility = Visibility.Collapsed;
                cbBoolean.Visibility = Visibility.Collapsed;
            }
        }
    }
}
