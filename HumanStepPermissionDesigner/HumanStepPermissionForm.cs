using System.Windows.Forms;
using Ecm.Workflow.Activities.HumanStepPermissionDesigner.ViewModel;

namespace Ecm.Workflow.Activities.HumanStepPermissionDesigner
{
    public partial class HumanStepPermissionForm : Form
    {
        public HumanStepPermissionForm()
        {
            InitializeComponent();
        }

        public void Initialize(HumanStepPermissionViewModel viewModel)
        {
            humanStepPermission.InitializeViewModel(viewModel);
            humanStepPermission.Form = this;
        }
    }
}
