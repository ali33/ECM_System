using System.Windows.Controls;

namespace Ecm.Admin.View
{
    /// <summary>
    /// Interaction logic for DialogChildView.xaml
    /// </summary>
    public class DialogChildView : UserControl
    {
        public DialogBaseView VirtualParent
        {
            get;
            set;
        }
    }
}
