using System.Windows.Controls;
using Ecm.CaptureAdmin.ViewModel;

namespace Ecm.CaptureAdmin.View
{
    /// <summary>
    /// Interaction logic for SelectionUserView.xaml
    /// </summary>
    public partial class SelectionUserView : UserControl
    {
        public SelectionUserView(UserGroupViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}
