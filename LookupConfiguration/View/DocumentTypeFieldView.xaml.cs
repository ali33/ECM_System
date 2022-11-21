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
using Ecm.Workflow.Activities.LookupConfiguration.ViewModel;
using Ecm.Workflow.Activities.CustomActivityModel;

namespace Ecm.Workflow.Activities.LookupConfiguration.View
{
    /// <summary>
    /// Interaction logic for DocumentTypeFieldView.xaml
    /// </summary>
    public partial class DocumentTypeFieldView : UserControl
    {
        public DocumentTypeFieldView()
        {
            InitializeComponent();
            //DataContext = _viewModel = viewModel;
        }

        private void btnConfigure_Click(object sender, RoutedEventArgs e)
        {
            DocumentFieldViewModel viewModel = DataContext as DocumentFieldViewModel;
            var field = ((FrameworkElement)sender).DataContext as FieldModel;
            if (field != null)
            {
                viewModel.ConfigureLookupCommand.Execute(field);
            }
        }

        private void btnDeleteConfigure_Click(object sender, RoutedEventArgs e)
        {
            DocumentFieldViewModel viewModel = DataContext as DocumentFieldViewModel;
            var field = ((FrameworkElement)sender).DataContext as FieldModel;
            if (field != null)
            {
                viewModel.DeleteLookupCommand.Execute(field);
            }
        }
    }
}
