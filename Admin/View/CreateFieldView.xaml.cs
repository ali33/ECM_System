using System.Windows;
using System.Windows.Controls;
using Ecm.Admin.ViewModel;

namespace Ecm.Admin.View
{
    /// <summary>
    /// Interaction logic for CreateFieldView.xaml
    /// </summary>
    public partial class CreateFieldView : DialogChildView
    {
        public CreateFieldView(FieldViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
            txtName.Focus();

            if (!viewModel.HasParent)
            {
                //cboDataType.Items.Add(new ComboBoxItem { Content = "Table" });
            }
            else
            {
                //btnAdd.Visibility = System.Windows.Visibility.Hidden;
            }
        }

        //private void btnConfigLookup_Click(object sender, RoutedEventArgs e)
        //{
        //    FieldViewModel columnViewModel = new FieldViewModel(SaveColumn, (DataContext as )
        //                                         {
        //                                             HasParent = true,
        //                                             IsEditMode = false,
        //                                             Field = new Model.FieldMetaDataModel()
        //                                         };
        //    CreateFieldView newView = new CreateFieldView(columnViewModel);
        //    VirtualParent.ChangeView(newView);
        //}

        private void SaveColumn(Model.TableColumnModel column)
        {
            VirtualParent.ChangeView(this);

            FieldViewModel viewModel = DataContext as FieldViewModel;
            if (viewModel != null)
            {
                viewModel.Field.Children.Add(column);
            }
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
            else if ((selectedText + "").ToLower() != "picklist" && (selectedText + "").ToLower() != "table")
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
