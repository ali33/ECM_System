using System;
using System.Collections.Generic;
using System.Windows;
//using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interop;
using Ecm.Capture.ViewModel;
using Ecm.Utility;
using System.Resources;
using System.Reflection;
using System.Windows.Controls;
using Ecm.CaptureModel;

namespace Ecm.Capture.View
{
    public partial class AssignedTaskView
    {
        private AssignedTaskViewModel _viewModel;
        private readonly ResourceManager _resource = new ResourceManager("Ecm.Capture.Resources", Assembly.GetExecutingAssembly());
        private DialogBaseView _dialog;
        public AssignedTaskView()
        {
            InitializeComponent();
            Loaded += AssignedTaskViewLoaded;
            Unloaded += AssignedTaskViewUnload;
        }

        private void AssignedTaskViewLoaded(object sender, RoutedEventArgs e)
        {
            try
            {
                _viewModel = DataContext as AssignedTaskViewModel;
            }
            catch (Exception ex)
            {
                ProcessHelper.ProcessException(ex);
            }
        }

        void AssignedTaskViewUnload(object sender, RoutedEventArgs e)
        {
            _viewModel.Dispose();
        }

        private void PnlLeftSizeChanged(object sender, SizeChangedEventArgs e)
        {
            try
            {
                if (pnLeft.ActualWidth <= 22)
                {
                    pnLeftHeader.Visibility = Visibility.Collapsed;
                    pnDocTypes.Visibility = Visibility.Collapsed;
                    pnLeft.Width = 22;
                    btnExpandLeft.IsChecked = true;
                }
                else
                {
                    pnLeftHeader.Visibility = Visibility.Visible;
                    pnDocTypes.Visibility = Visibility.Visible;
                    btnExpandLeft.IsChecked = false;
                }
            }
            catch (Exception ex)
            {
                ProcessHelper.ProcessException(ex);
            }
        }

        private void BtnExpandLeftClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (btnExpandLeft.IsChecked.Value)
                {
                    pnDocTypes.Visibility = Visibility.Collapsed;
                    pnLeftHeader.Visibility = Visibility.Collapsed;
                    pnLeft.Width = 22;
                }
                else
                {
                    pnDocTypes.Visibility = Visibility.Visible;
                    pnLeftHeader.Visibility = Visibility.Visible;
                    pnLeft.Width = pnLeft.MaxWidth;
                }
            }
            catch (Exception ex)
            {
                ProcessHelper.ProcessException(ex);
            }
        }

        private void DocTypeMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                _viewModel.RunAdvanceSearch();
            }
            catch (Exception ex)
            {
                ProcessHelper.ProcessException(ex);
            }
        }

        private void SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            AssignedTaskViewModel viewModel = DataContext as AssignedTaskViewModel;
            var treeView = sender as TreeView;
            BatchTypeModel batchType = null;
            if (treeView != null)
            {
                if (treeView.SelectedItem is TaskMenu)
                {
                    TaskMenu menu = treeView.SelectedItem as TaskMenu;
                    batchType = menu.BatchType;
                }
                else if (treeView.SelectedItem is TaskMenuItem)
                {
                    TaskMenuItem menuItem = treeView.SelectedItem as TaskMenuItem;
                    batchType = menuItem.BatchType;
                    viewModel.SelectedTaskType = menuItem.Type;
                    viewModel.SearchCommand.Execute(menuItem);
                }

                if (batchType != null)
                {
                    viewModel.SelectedBatchType = batchType;
                }
            }
        }

        private void BtnExpandSearchClick(object sender, RoutedEventArgs e)
        {
            try
            {
                ToggleSearchConditionArea(SearchButtonPanel.Visibility == Visibility.Collapsed &&
                                          SearchPanel.Visibility == Visibility.Collapsed);
            }
            catch (Exception ex)
            {
                ProcessHelper.ProcessException(ex);
            }
        }

        public void ToggleSearchConditionArea(bool show)
        {
            if (show)
            {
                SearchPanel.Visibility = Visibility.Visible;
                SearchButtonPanel.Visibility = Visibility.Visible;
                pnMainSearch.Height = Double.NaN;
            }
            else
            {
                SearchPanel.Visibility = Visibility.Collapsed;
                SearchButtonPanel.Visibility = Visibility.Collapsed;
                pnMainSearch.Height = 22;
            }
        }

        private void PnlMainSearchSizeChanged(object sender, SizeChangedEventArgs e)
        {
            try
            {
                if (pnMainSearch.ActualHeight <= 35)
                {
                    SearchPanel.Visibility = Visibility.Collapsed;
                    SearchButtonPanel.Visibility = Visibility.Collapsed;
                    btnExpandSearch.IsChecked = true;
                }
                else
                {
                    SearchPanel.Visibility = Visibility.Visible;
                    SearchButtonPanel.Visibility = Visibility.Visible;
                    btnExpandSearch.IsChecked = false;
                }
            }
            catch (Exception ex)
            {
                ProcessHelper.ProcessException(ex);
            }
        }

    }
}
