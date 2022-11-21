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
    /// Interaction logic for BatchTypeView.xaml
    /// </summary>
    public partial class BatchTypeView : UserControl
    {
        private BatchTypeViewModel _viewModel;
        public BatchTypeView(BatchTypeViewModel viewModel)
        {
            InitializeComponent();
            DataContext = _viewModel = viewModel;
            viewModel.SaveCompleted = SaveCompleted;
        }

        public DialogViewer Dialog{get;set;}

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            if (Dialog != null)
            {
                Dialog.DialogResult = System.Windows.Forms.DialogResult.Cancel;
                Dialog.Close();
            }

        }

        private void btnConfigureLookup_Click(object sender, RoutedEventArgs e)
        {
            var field = ((FrameworkElement)sender).DataContext as FieldModel;
            if (field != null)
            {
                _viewModel.ConfigureLookupCommand.Execute(field);
            }
        }

        private void btnDeleteLookup_Click(object sender, RoutedEventArgs e)
        {
            var field = ((FrameworkElement)sender).DataContext as FieldModel;
            if (field != null)
            {
                _viewModel.DeleteLookupCommand.Execute(field);
            }
        }

        private void SaveCompleted()
        {
            if (Dialog != null)
            {
                Dialog.DialogResult = System.Windows.Forms.DialogResult.OK;
                Dialog.Close();
            }
        }
    }
}
