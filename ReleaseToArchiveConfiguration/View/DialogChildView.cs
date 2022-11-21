using System.Windows.Controls;

namespace Ecm.Workflow.Activities.ReleaseToArchiveConfiguration.View
{
    /// <summary>
    /// Interaction logic for DialogChildView.xaml
    /// </summary>
    public class DialogChildView : UserControl
    {
        public DialogViewer VirtualParent
        {
            get;
            set;
        }
    }
}
