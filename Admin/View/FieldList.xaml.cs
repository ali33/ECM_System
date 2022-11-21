using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Ecm.Model;
using Ecm.Admin.ViewModel;

namespace Ecm.Admin.View
{
    /// <summary>
    /// Interaction logic for FieldList.xaml
    /// </summary>
    public partial class FieldList
    {
        public FieldList()
        {
            InitializeComponent();
        }

        private void lvField_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void lvField_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {

        }

        //private void btnConfigureLookup_Click(object sender, RoutedEventArgs e)
        //{
        //    FieldMetaDataModel field = ((FrameworkElement)sender).DataContext as FieldMetaDataModel;
        //    if (field != null)
        //    {
        //        FieldListViewModel viewModel = DataContext as FieldListViewModel;
        //        if (viewModel != null)
        //        {
        //            viewModel.LookupCommand.Execute(field);
        //        }
        //    }
        //}

        private void btnDeleteLookup_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
