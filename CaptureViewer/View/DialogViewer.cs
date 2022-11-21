using System.Windows;
using System.Windows.Forms;

namespace Ecm.CaptureViewer
{
    public partial class DialogViewer : Form
    {
        public DialogViewer()
        {
            InitializeComponent();
        }

        public UIElement WpfContent
        {
            get { return elementHost.Child; }
            set { elementHost.Child = value; }
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
