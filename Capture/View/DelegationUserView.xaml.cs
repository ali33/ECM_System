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
using Ecm.Capture.ViewModel;
using Ecm.CaptureModel;

namespace Ecm.Capture.View
{
    /// <summary>
    /// Interaction logic for DelegationUserView.xaml
    /// </summary>
    public partial class DelegationUserView : UserControl
    {
        DelegationUserViewModel _viewModel;
        public DelegationUserView()
        {
            InitializeComponent();
            _viewModel = new DelegationUserViewModel(SaveCompleted);
            DataContext = _viewModel;
        }

        public DialogBaseView Dialog { get; set; }

        public string DelegationUser { get; set; }

        public string DelegatedComment { get; set; }

        public void SaveCompleted(bool isCompleted)
        {
            if (isCompleted)
            {
                DelegationUser = _viewModel.DelegationUser.Username;
                DelegatedComment = _viewModel.DelegatedComment;
                Dialog.DialogResult = System.Windows.Forms.DialogResult.OK;
            }
            else
            {
                Dialog.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            }

            Dialog.Close();
        }
    }
}
