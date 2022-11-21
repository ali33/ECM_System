using System.Windows.Controls;
using Ecm.Admin.ViewModel;

namespace Ecm.Admin.View
{
    /// <summary>
    /// Interaction logic for PicklistView.xaml
    /// </summary>
    public partial class PicklistView : UserControl
    {
        public PicklistView(PicklistViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}
