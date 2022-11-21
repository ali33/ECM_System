using System.Windows.Controls;
using System.Windows.Input;
using Ecm.CaptureAdmin.ViewModel;
using Ecm.CaptureModel;

namespace Ecm.CaptureAdmin.View
{
    /// <summary>
    /// Interaction logic for CreateDocTypeView.xaml
    /// </summary>
    public partial class CreateDocTypeView : DialogChildView
    {
        public CreateDocTypeView(DocTypeViewModel viewModel)
        {
            InitializeComponent();

            DataContext = viewModel;
            _viewModel = viewModel;
            txtDocTypeName.Focus();
        }

        private void LvlFieldMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var field = ((ListView)sender).SelectedItem as FieldModel;

            if (field != null)
            {
                _viewModel.EditFieldCommand.Execute(null);
            }
        }

        private readonly DocTypeViewModel _viewModel;
    }
}
