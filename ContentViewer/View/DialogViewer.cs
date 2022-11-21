using System.Windows;
using System.Windows.Forms;

namespace Ecm.ContentViewer
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
