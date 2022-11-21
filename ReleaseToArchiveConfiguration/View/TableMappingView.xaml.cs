using Ecm.Workflow.Activities.ReleaseToArchiveConfiguration.ViewModel;
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

namespace Ecm.Workflow.Activities.ReleaseToArchiveConfiguration.View
{
    /// <summary>
    /// Interaction logic for TableMappingView.xaml
    /// </summary>
    public partial class TableMappingView : UserControl
    {
        public TableMappingView(TableMappingViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
            viewModel.CloseDialog = Close;
        }

        public DialogViewer Dialog { get; set; }

        private void Close()
        {
            if (Dialog != null)
            {
                Dialog.Close();
            }
        }
    }
}
