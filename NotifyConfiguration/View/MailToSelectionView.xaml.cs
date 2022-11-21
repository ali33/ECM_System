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
using Ecm.Workflow.Activities.NotifyConfiguration.ViewModel;
using Ecm.Workflow.Activities.CustomActivityModel;

namespace Ecm.Workflow.Activities.NotifyConfiguration.View
{
    /// <summary>
    /// Interaction logic for MailToSelection.xaml
    /// </summary>
    public partial class MailToSelectionView : UserControl
    {
        private MailToSelectionViewModel _viewModel;
        public DialogViewer Dialog { get; set; }

        public MailToSelectionView(MailToSelectionViewModel viewModel)
        {
            InitializeComponent();
            DataContext = _viewModel = viewModel;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Dialog.DialogResult = System.Windows.Forms.DialogResult.OK;
            Dialog.Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Dialog.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            Dialog.Close();
        }
    }
}
