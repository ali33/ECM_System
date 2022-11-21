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
using System.Windows.Xps.Packaging;
using Ecm.Audit.ViewModel;
using System.Data;
using Microsoft.Reporting.WinForms;
using System.Reflection;
using System.IO;
using Ecm.Model;

namespace Ecm.Audit.View
{
    /// <summary>
    /// Interaction logic for AuditView.xaml
    /// </summary>
    public partial class AuditView : UserControl
    {
        private ReportViewModel _viewModel;
        private string _reportName;

        public AuditView()
        {
            InitializeComponent();
            _viewModel = new ReportViewModel();
            DataContext = _viewModel;
        }

        private void DisplayReport(string reportName, ReportDataSource dataSource)
        {
            try
            {
                rpViewer.LocalReport.DataSources.Clear();
                rpViewer.LocalReport.DataSources.Add(dataSource);

                // Set A Report Embedded Resource  
                rpViewer.LocalReport.ReportPath = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), reportName);
                rpViewer.RefreshReport();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        private void DisplayReport(string reportName, ReportDataSource dataSource, ReportParameter[] paras)
        {
            try
            {
                rpViewer.LocalReport.DataSources.Clear();
                rpViewer.LocalReport.DataSources.Add(dataSource);

                // Set A Report Embedded Resource  
                rpViewer.LocalReport.ReportPath = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), reportName);
                rpViewer.LocalReport.SetParameters(paras);
                rpViewer.RefreshReport();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        private void AdminMenu_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ReportMenu menuItem = (sender as ListBox).SelectedItem as ReportMenu;
            ReportDataSource resource = null;
            //ReportParameter[] para;
            _viewModel.BuildParameterPane(menuItem.MenuName);
            switch(menuItem.MenuName)
            {
                case Common.REPORT_ACTION_LOG:
                    DataTable dtActionLogSources = _viewModel.GetActionLogDataSource();
                    resource = new ReportDataSource("ActionLogData", dtActionLogSources);
                    _reportName = menuItem.MenuName;
                    DisplayReport("Report/ActionLogReport.rdlc", resource);
                    break;
                case Common.REPORT_DOCUMENT:
                case Common.REPORT_PAGE:
                default:
                    break;
            }
            
        }

        private void pnMainSearch_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (pnMainSearch.ActualHeight <= 40)
            {
                SearchPanel.Visibility = Visibility.Collapsed;
                SearchButtonPanel.Visibility = Visibility.Collapsed;
                //tbReportParameter.Visibility = System.Windows.Visibility.Hidden;
                txtSearchTitle1.Visibility = System.Windows.Visibility.Visible;

                btnExpandSearch.IsChecked = true;
            }
            else
            {
                SearchPanel.Visibility = Visibility.Visible;
                SearchButtonPanel.Visibility = Visibility.Visible;
                //tbReportParameter.Visibility = System.Windows.Visibility.Visible;
                txtSearchTitle1.Visibility = System.Windows.Visibility.Hidden;

                btnExpandSearch.IsChecked = false;
            }


        }

        private void btnExpandSearch_Click(object sender, RoutedEventArgs e)
        {
            if (SearchButtonPanel.Visibility == Visibility.Collapsed && SearchPanel.Visibility == Visibility.Collapsed)
            {
                SearchPanel.Visibility = Visibility.Visible;
                SearchButtonPanel.Visibility = Visibility.Visible;
                //tbReportParameter.Visibility = System.Windows.Visibility.Visible;
                txtSearchTitle1.Visibility = System.Windows.Visibility.Hidden;

                pnMainSearch.Height = 150;
            }
            else
            {
                SearchPanel.Visibility = Visibility.Collapsed;
                SearchButtonPanel.Visibility = Visibility.Collapsed;
                //tbReportParameter.Visibility = System.Windows.Visibility.Hidden;
                txtSearchTitle1.Visibility = System.Windows.Visibility.Visible;

                pnMainSearch.Height = 23;
            }

        }

        private void btnExpandLeft_Click(object sender, RoutedEventArgs e)
        {
            if (pnLeftExpand.Visibility == System.Windows.Visibility.Visible && pnLeftPanel.Visibility == System.Windows.Visibility.Visible)
            {
                pnLeftExpand.Visibility = System.Windows.Visibility.Collapsed;
                pnLeftSearch.Visibility = System.Windows.Visibility.Collapsed;
                pnMainSearch.Visibility = System.Windows.Visibility.Collapsed;
                pnLeftPanel.Width = 30;
            }
            else
            {
                pnLeftExpand.Visibility = System.Windows.Visibility.Visible;
                pnLeftSearch.Visibility = System.Windows.Visibility.Visible;
                pnMainSearch.Visibility = System.Windows.Visibility.Visible;
                pnLeftPanel.Width = 440;
            }
        }

        private void pnReport_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (pnLeftPanel.ActualWidth <= 35)
            {
                pnLeftExpand.Visibility = System.Windows.Visibility.Collapsed;
                pnLeftSearch.Visibility = System.Windows.Visibility.Collapsed;
                btnExpandLeft.IsChecked = true;
            }
            else
            {
                pnLeftExpand.Visibility = System.Windows.Visibility.Visible;
                pnLeftSearch.Visibility = System.Windows.Visibility.Visible;
                btnExpandLeft.IsChecked = false;
            }
        }

        private void btnDisplayReport_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.DisplayReportCommand.Execute(_reportName);

            if (string.IsNullOrEmpty(_reportName))
            {
                return;
            }

            ReportDataSource resource = null;
            switch (_reportName)
            {
                case Common.REPORT_ACTION_LOG:
                    resource = new ReportDataSource("ActionLogData", _viewModel.ReportDataSource);
                    DisplayReport("Report/ActionLogReport.rdlc", resource);
                    break;
            }
        }
    }
}
