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

namespace Ecm.Workflow.Activities.LookupConfiguration.View
{
    /// <summary>
    /// Interaction logic for TestLookupValue.xaml
    /// </summary>
    public partial class TestLookupView : UserControl
    {

        public TestLookupView(TestLookupViewModel viewModel)
        {
            InitializeComponent();
            this.DataContext = viewModel;
        }
    }
}
