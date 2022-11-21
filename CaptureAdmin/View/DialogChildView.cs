using System.Windows.Controls;

namespace Ecm.CaptureAdmin.View
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
