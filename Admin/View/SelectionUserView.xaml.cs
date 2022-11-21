using System.Windows.Controls;
using Ecm.Admin.ViewModel;

namespace Ecm.Admin.View
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
