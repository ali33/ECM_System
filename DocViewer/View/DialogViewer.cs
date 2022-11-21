using System.Windows;
using System.Windows.Forms;

namespace Ecm.DocViewer
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
    }
}
