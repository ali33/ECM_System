using System.Windows;
using System.Windows.Forms;

namespace Ecm.Workflow.Activities.HumanStepPermissionDesigner.View
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
