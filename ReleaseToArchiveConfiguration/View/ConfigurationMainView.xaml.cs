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
using Ecm.Workflow.Activities.ReleaseToArchiveConfiguration.ViewModel;

namespace Ecm.Workflow.Activities.ReleaseToArchiveConfiguration.View
{
    /// <summary>
    /// Interaction logic for ConfigurationMainView.xaml
    /// </summary>
    public partial class ConfigurationMainView : UserControl
    {
        public DialogViewer Dialog { get; set; }
        ConfigurationMainViewModel _viewModel;

        public ConfigurationMainView(ConfigurationMainViewModel viewModel)
        {
            InitializeComponent();
            DataContext = _viewModel = viewModel;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //_viewModel.OkCommand.Execute(null);
            Dialog.DialogResult = System.Windows.Forms.DialogResult.OK;
            Dialog.Close();
        }

        private void tvCaptureFields_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            var tree = sender as TreeView;

            if (tree != null)
            {
                var selectedMenu = tree.SelectedItem as Model.TreeModel;

                if (selectedMenu != null)
                {
                    _viewModel.MappingSelectedCommand.Execute(selectedMenu);
                }
            }

        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Dialog.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            Dialog.Close();
        }

    }
}
