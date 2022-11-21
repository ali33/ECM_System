using System.Windows;
using System.Windows.Controls;
using Ecm.CaptureAdmin.ViewModel;

namespace Ecm.CaptureAdmin.View
{
    /// <summary>
    /// Interaction logic for CreateFieldView.xaml
    /// </summary>
    public partial class CreateDocFieldView : DialogChildView
    {
        public CreateDocFieldView(DocFieldViewModel viewModel)
        {
            InitializeComponent();
            Loaded += CreateDocFieldViewLoaded;
            DataContext = viewModel;
        }

        private void CreateDocFieldViewLoaded(object sender, RoutedEventArgs e)
        {
            txtName.Focus();
        }

        private void cboDataTypeChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBoxItem cbi = (ComboBoxItem)cboDataType.SelectedItem; 
            string selectedText = cbi.Content.ToString();
            txtDefaultValue.Visibility = Visibility.Visible;
            tbDefaultValue.Visibility = Visibility.Visible;
            tbMaxLength.Visibility = Visibility.Collapsed;
            txtMaxLength.Visibility = Visibility.Collapsed;
            if ((selectedText + "").ToLower() == "boolean")
            {
                tbDefaultValue.Visibility = Visibility.Visible;
                cbxUseCurrentDate.Visibility = Visibility.Collapsed;
                txtDefaultValue.Visibility = Visibility.Collapsed;
                cbBoolean.Visibility = Visibility.Visible;
                cbBoolean.SelectedIndex = 1;

                lbValidationScript.Visibility = Visibility.Visible;
                txtValidationScript.Visibility = Visibility.Visible;
                btnValidationScript.Visibility = Visibility.Visible;

                lbValidationPattern.Visibility = Visibility.Collapsed;
                txtValidationPattern.Visibility = Visibility.Collapsed;
                btnValidationPattern.Visibility = Visibility.Collapsed;
               
            }
            else if ((selectedText + "").ToLower() == "date")
            {
                tbDefaultValue.Visibility = Visibility.Visible;
                cbxUseCurrentDate.Visibility = Visibility.Visible;
                txtDefaultValue.Visibility = Visibility.Collapsed;
                cbBoolean.Visibility = Visibility.Collapsed;

                lbValidationScript.Visibility = Visibility.Visible;
                txtValidationScript.Visibility = Visibility.Visible;
                btnValidationScript.Visibility = Visibility.Visible;

                lbValidationPattern.Visibility = Visibility.Collapsed;
                txtValidationPattern.Visibility = Visibility.Collapsed;
                btnValidationPattern.Visibility = Visibility.Collapsed;
            }
            else if ((selectedText + "").ToLower() == "picklist")
            {
                tbDefaultValue.Visibility = Visibility.Visible;
                cbxUseCurrentDate.Visibility = Visibility.Collapsed;
                txtDefaultValue.Visibility = Visibility.Visible;
                cbBoolean.Visibility = Visibility.Collapsed;
                lbValidationPattern.Visibility = Visibility.Collapsed;
                lbValidationScript.Visibility = Visibility.Collapsed;
                txtValidationScript.Visibility = Visibility.Collapsed;
                txtValidationPattern.Visibility = Visibility.Collapsed;
                btnValidationPattern.Visibility = Visibility.Collapsed;
                btnValidationScript.Visibility = Visibility.Collapsed;
            }
            else if ((selectedText + "").ToLower() == "table")
            {
                tbDefaultValue.Visibility = Visibility.Visible;
                cbxUseCurrentDate.Visibility = Visibility.Collapsed;
                txtDefaultValue.Visibility = Visibility.Visible;
                cbBoolean.Visibility = Visibility.Collapsed;
                lbValidationPattern.Visibility = Visibility.Collapsed;
                lbValidationScript.Visibility = Visibility.Collapsed;
                txtValidationScript.Visibility = Visibility.Collapsed;
                txtValidationPattern.Visibility = Visibility.Collapsed;
                btnValidationPattern.Visibility = Visibility.Collapsed;
                btnValidationScript.Visibility = Visibility.Collapsed;
                txtDefaultValue.Visibility = Visibility.Collapsed;
                tbDefaultValue.Visibility = Visibility.Collapsed;
            }
            else if ((selectedText + "").ToLower() == "integer")
            {
                tbDefaultValue.Visibility = Visibility.Collapsed;
                cbxUseCurrentDate.Visibility = Visibility.Collapsed;
                txtDefaultValue.Visibility = Visibility.Collapsed;
                cbBoolean.Visibility = Visibility.Collapsed;
                tbDefaultValue.Visibility = Visibility.Visible;
                cbxUseCurrentDate.Visibility = Visibility.Collapsed;
                txtDefaultValue.Visibility = Visibility.Visible;
                cbBoolean.Visibility = Visibility.Collapsed;

                lbValidationScript.Visibility = Visibility.Visible;
                txtValidationScript.Visibility = Visibility.Visible;
                btnValidationScript.Visibility = Visibility.Visible;

                lbValidationPattern.Visibility = Visibility.Collapsed;
                txtValidationPattern.Visibility = Visibility.Collapsed;
                btnValidationPattern.Visibility = Visibility.Collapsed;
            }
            else
            {
                tbDefaultValue.Visibility = Visibility.Collapsed;
                cbxUseCurrentDate.Visibility = Visibility.Collapsed;
                txtDefaultValue.Visibility = Visibility.Collapsed;
                cbBoolean.Visibility = Visibility.Collapsed;
                tbDefaultValue.Visibility = Visibility.Visible;
                cbxUseCurrentDate.Visibility = Visibility.Collapsed;
                txtDefaultValue.Visibility = Visibility.Visible;
                cbBoolean.Visibility = Visibility.Collapsed;
                lbValidationPattern.Visibility = Visibility.Visible;
                lbValidationScript.Visibility = Visibility.Visible;
                txtValidationScript.Visibility = Visibility.Visible;
                txtValidationPattern.Visibility = Visibility.Visible;
                btnValidationPattern.Visibility = Visibility.Visible;
                btnValidationScript.Visibility = Visibility.Visible;
                tbMaxLength.Visibility = Visibility.Visible;
                txtMaxLength.Visibility = Visibility.Visible;
            }
        }

    }
}
