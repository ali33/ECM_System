using Ecm.CaptureCustomAddIn.ViewModel;
using Ecm.CaptureModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Ecm.CaptureCustomAddIn.View
{
    /// <summary>
    /// Interaction logic for AssignedTaskView.xaml
    /// </summary>
    public partial class AssignedTaskView : Window
    {
        private AssignedTaskViewModel _viewModel;

        public AssignedTaskView(AddinType type)
        {
            InitializeComponent();
            DataContext = _viewModel = new AssignedTaskViewModel(CloseView, type);
        }

        private void lvPage_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            CustomControl.SortableListView control = sender as CustomControl.SortableListView;
            PageModel page = control.SelectedItem as PageModel;
            AddinType type = AddinType.Excel;

            switch (page.FileExtension)
            {
                case "xlsx":
                case "xls":
                    type = AddinType.Excel;
                    break;
                case "doc":
                case "docx":
                    type = AddinType.Word;
                    break;
                case "ppt":
                case "pptx":
                    type = AddinType.PowerPoint;
                    break;
            }

            _viewModel.OpenDocumentCommand.Execute(type);
        }

        private void CloseView()
        {
            this.Close();
        }
    }
}
