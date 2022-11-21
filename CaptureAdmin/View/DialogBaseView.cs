using System.Windows.Forms;
using System.Windows;

namespace Ecm.CaptureAdmin.View
{
    public partial class DialogBaseView : Form
    {
        public DialogBaseView()
        {
            InitializeComponent();
        }

        public DialogBaseView(System.Windows.Controls.UserControl view)
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
