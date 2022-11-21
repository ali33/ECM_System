using System.Windows.Controls;
using Ecm.Admin.ViewModel;

namespace Ecm.Admin.View
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
