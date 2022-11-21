using System.Windows.Forms;

namespace Ecm.CloudECMClientConfiguration
{
    public partial class ShowError : Form
    {
        public ShowError(string error)
        {
            InitializeComponent();
            txtErrorMessage.Text = error;
        }
    }
}
