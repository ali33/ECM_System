using Ecm.Capture.ViewModel;
using Ecm.CaptureModel;
using Ecm.CustomControl;
using Ecm.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Ecm.Capture
{
    /// <summary>
    /// Interaction logic for AssignedWorkitemView.xaml
    /// </summary>
    public partial class AssignedWorkitemView : Page
    {
        [DllImport("user32", ExactSpelling = true, CharSet = CharSet.Auto)]
        private static extern IntPtr GetAncestor(IntPtr hwnd, int flags);
        [DllImport("user32", CharSet = CharSet.Auto)]
        private static extern bool PostMessage(IntPtr hwnd, int msg, IntPtr wparam, IntPtr lparam); 

        private ApplicationMode _mode = ApplicationMode.None;
        private string _exterWorkitem;
        private AssignedWorkitemViewModel _viewModel;

        public AssignedWorkitemView()
        {
            InitializeComponent();
            
            XbapHelper.Configurate();
            GetUserInfoFromUrl();

            _viewModel = new AssignedWorkitemViewModel(_exterWorkitem, SaveCompleted);
            DataContext = _viewModel;
            Loaded += AssignedWorkitemViewLoaded;
        }

        private void AssignedWorkitemViewLoaded(object sender, RoutedEventArgs e)
        {

            if (_mode == ApplicationMode.WorkItem)
            {
                ViewerContainer.HandleException = ProcessHelper.ProcessException;
                ViewerContainer.LogException = ProcessHelper.LogException;
                ViewerContainer.AddActionLog = ProcessHelper.AddActionLog;

                ViewerContainer.SaveAll += ViewerContainerSaveAll;
                ViewerContainer.ApproveAll += ViewerContainerApprove;
                ViewerContainer.GetLookupData = _viewModel.GetLookupData;
            }
            else
            {
                DialogService.ShowMessageDialog("Invalid workitem");
                CloseWindow();
            }   
        }

        private void ViewerContainerApprove()
        {
            _viewModel.Approve();
        }

        private void WorkItemViewUnloaded(object sender, RoutedEventArgs e)
        {

        }

        private void ViewerContainerSaveAll()
        {
            _viewModel.Save();
        }

        private void SaveCompleted()
        {
            CloseWindow();
        }

        private void GetUserInfoFromUrl()
        {
            string urlMode = XbapHelper.Params["mode"];

            if (!string.IsNullOrEmpty(urlMode))
            {
                if (urlMode.ToLower() == "assignedwork")
                {
                    _mode = ApplicationMode.AssignedWork;
                }
                else if (urlMode.ToLower() == "capture")
                {
                    _mode = ApplicationMode.Capture;
                }
                else if (urlMode.ToLower() == "workitem")
                {
                    _mode = ApplicationMode.WorkItem;
                }
            }

            _exterWorkitem = XbapHelper.Params["workitemid"] + string.Empty;
        }

        private void CloseWindow()
        {
            WindowInteropHelper wih = new WindowInteropHelper(Application.Current.MainWindow);
            IntPtr ieHwnd = GetAncestor(wih.Handle, 2);
            PostMessage(ieHwnd, 0x10, IntPtr.Zero, IntPtr.Zero);

        }
    }
}
