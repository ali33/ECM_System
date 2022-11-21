using System.Windows.Controls;
using Ecm.CaptureAdmin.ViewModel;

namespace Ecm.CaptureAdmin.View
{
    /// <summary>
    /// Interaction logic for SelectionUserView.xaml
    /// </summary>
    public partial class SelectionUserGroupView : UserControl
    {

        public SelectionUserGroupView(UserViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}
