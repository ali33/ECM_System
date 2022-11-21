using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Navigation;

using Ecm.Capture.View;
using Ecm.Capture.ViewModel;
using System.Windows.Controls;
using Ecm.CustomControl;
using Ecm.CaptureModel;
using Ecm.Mvvm;

namespace Ecm.Capture
{
    public partial class MainView : Page
    {
        private readonly MainViewModel _viewModel;// = new MainViewModel();
        private CaptureView _captureView;
        private AssignedTaskView _assignedTaskView;
        private AboutView _aboutView;
        private readonly List<WorkItemView> _workItemViews = new List<WorkItemView>();
        private ApplicationMode _mode = ApplicationMode.None;
        private string _exterWorkitem;

        public MainView()
        {
            InitializeComponent();
            XbapHelper.Configurate();

            GetUserInfoFromUrl();

            if (_mode == ApplicationMode.WorkItem)
            {
                _viewModel = new MainViewModel(App.Current.Dispatcher, _exterWorkitem);
            }
            else
            {
                _viewModel = new MainViewModel(App.Current.Dispatcher);
            }

            DataContext = _viewModel;
            _viewModel.WorkItemViewModels.CollectionChanged += WorkItemViewModelsCollectionChanged;
            _viewModel.PropertyChanged += ViewModelPropertyChanged;
            ButtonWorkItems.ContextMenu.MinWidth = 250;

            Loaded += MainViewLoaded;
            Unloaded += MainViewUnloaded;
        }

        private void MainViewLoaded(object sender, RoutedEventArgs e)
        {
            if (_viewModel.ContentViewModel != null)
            {
                LoadView();
            }
        }

        private void MainViewUnloaded(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_captureView != null)
                {
                    _captureView.Backup();
                }

                if (_assignedTaskView != null)
                {
                    (_assignedTaskView.DataContext as AssignedTaskViewModel).Dispose();
                }
            }
            catch (Exception ex)
            {
                ProcessHelper.ProcessException(ex);
            }
        }

        private void ViewModelPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "ContentViewModel")
            {
                LoadView();
            }
        }

        private void LoadView()
        {
            if (_viewModel.ContentViewModel is CaptureViewModel)
            {
                if (_captureView == null)
                {
                    _captureView = new CaptureView { DataContext = _viewModel.ContentViewModel };
                }

                ViewContainer.Content = _captureView;
            }
            else if (_viewModel.ContentViewModel is AssignedTaskViewModel)
            {
                if (_assignedTaskView == null)
                {
                    _assignedTaskView = new AssignedTaskView { DataContext = _viewModel.ContentViewModel };
                }

                ViewContainer.Content = _assignedTaskView;
            }
            else if (_viewModel.ContentViewModel is AboutViewModel)
            {
                if (_aboutView == null)
                {
                    _aboutView = new AboutView { DataContext = _viewModel.ContentViewModel };
                }

                ViewContainer.Content = _aboutView;
            }
            else if (_viewModel.ContentViewModel is WorkItemViewModel)
            {
                var existedView = _workItemViews.FirstOrDefault(p => p.DataContext == _viewModel.ContentViewModel);
                if (existedView == null)
                {
                    existedView = new WorkItemView { DataContext = _viewModel.ContentViewModel };
                    _workItemViews.Add(existedView);
                }

                ViewContainer.Content = existedView;
            }
        }

        private void WorkItemViewModelsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            var documentItems = new List<MenuItem>();
            foreach (var item in ButtonWorkItems.ContextMenu.Items)
            {
                if (item is MenuItem && (item as MenuItem).DataContext is WorkItemViewModel)
                {
                    documentItems.Add(item as MenuItem);
                }
            }

            foreach (var item in documentItems)
            {
                ButtonWorkItems.ContextMenu.Items.Remove(item);
            }

            foreach (var workItemViewModel in _viewModel.WorkItemViewModels)
            {
                var menuItem = new MenuItem
                {
                    DataContext = workItemViewModel,
                    Style = TryFindResource("WorkItemMenuItemStyle") as Style,
                    IsCheckable = true
                };
                var binding = new Binding("IsActivated") { Mode = BindingMode.TwoWay };
                BindingOperations.SetBinding(menuItem, MenuItem.IsCheckedProperty, binding);

                binding = new Binding("WorkItemName") { NotifyOnTargetUpdated = true };
                BindingOperations.SetBinding(menuItem, HeaderedItemsControl.HeaderProperty, binding);
                ButtonWorkItems.ContextMenu.Items.Add(menuItem);
            }

            var orphanTaskViews = _workItemViews.Where(p => !_viewModel.WorkItemViewModels.Any(q => q == p.DataContext)).ToList();
            foreach (var orphanTaskView in orphanTaskViews)
            {
                _workItemViews.Remove(orphanTaskView);
            }
        }

        private void DropDownOpenning(object sender, RoutedEventArgs e)
        {

            if (_viewModel.ContentViewModel is CaptureViewModel)
            {
                 var view = ViewContainer.Content as CaptureView;
                if (view.ViewerContainer != null && view.ViewerContainer._isNativeMode)
                {
                    view.ViewerContainer.ShowHideNativeView(true);
                }
            }
            else if (_viewModel.ContentViewModel is WorkItemViewModel)
            {
                var view = ViewContainer.Content as WorkItemView;
                if (view.ViewerContainer != null && view.ViewerContainer._isNativeMode)
                {
                    view.ViewerContainer.ShowHideNativeView(true);
                }
            }

        }

        private void DropDownClosing(object sender, RoutedEventArgs e)
        {
            if (_viewModel.ContentViewModel is CaptureViewModel)
            {
                var view = ViewContainer.Content as CaptureView;

                if (view.ViewerContainer != null && view.ViewerContainer._isNativeMode)
                {
                    view.ViewerContainer.ShowHideNativeView(false);
                }
            }
            else if (_viewModel.ContentViewModel is WorkItemViewModel)
            {
                var view = ViewContainer.Content as WorkItemView;
                if (view.ViewerContainer != null && view.ViewerContainer._isNativeMode)
                {
                    view.ViewerContainer.ShowHideNativeView(false);
                }
            }
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

    }
}
