using System.Windows.Forms;

namespace Ecm.Audit.View
{
    public partial class DialogBaseView : Form
    {
        public DialogBaseView(System.Windows.Controls.UserControl view)
        {
            InitializeComponent();
            elementHost.Child = view;
            if (view is DialogChildView)
            {
                ((DialogChildView)view).VirtualParent = this;
            }
        }
    }
}
