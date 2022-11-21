using System.Windows.Forms;

namespace Ecm.Capture.View
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

        public void SizeToContent()
        {
            elementHost.Dock = DockStyle.None;
            elementHost.Margin = new Padding(1);
            elementHost.AutoSize = true;

            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
        }
    }
}
