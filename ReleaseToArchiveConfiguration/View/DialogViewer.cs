using System.Windows;
using System.Windows.Forms;

namespace Ecm.Workflow.Activities.ReleaseToArchiveConfiguration.View
{
    public partial class DialogViewer : Form
    {
        public DialogViewer()
        {
            InitializeComponent();
            DialogResult = System.Windows.Forms.DialogResult.None;
        }

        public DialogViewer(System.Windows.Controls.UserControl view)
        {
            InitializeComponent();
            ChangeView(view);
        }

        public void ChangeView(System.Windows.Controls.UserControl view)
        {
            elementHost.Child = view;
            if (view is DialogChildView)
            {
                ((DialogChildView)view).VirtualParent = this;
            }
        }

        public bool EnableToResize
        {
            get { return FormBorderStyle == FormBorderStyle.Sizable || FormBorderStyle == FormBorderStyle.SizableToolWindow; }
            set
            {
                FormBorderStyle = value ? FormBorderStyle.Sizable : FormBorderStyle.FixedDialog;
                MaximizeBox = value;
            }
        }

        public UIElement WpfContent
        {
            get { return elementHost.Child; }
            set { elementHost.Child = value; }
        }

    }
}
