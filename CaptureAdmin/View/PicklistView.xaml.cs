using System.Windows.Controls;
using Ecm.CaptureAdmin.ViewModel;

namespace Ecm.CaptureAdmin.View
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
