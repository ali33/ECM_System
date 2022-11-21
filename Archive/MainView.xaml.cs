using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Ecm.Archive.View;
using Ecm.Archive.ViewModel;
using System;
using Ecm.AppHelper;

namespace Ecm.Archive
{
    public partial class MainView : Page
    {
        private readonly MainViewModel _viewModel = new MainViewModel();
        private CaptureView _captureView;
        private SearchView _searchView;
        private AboutView _aboutView;
        private readonly List<DocumentView> _documentViews = new List<DocumentView>();

        public MainView()
        {
            InitializeComponent(); 
            DataContext = _viewModel;
            _viewModel.DocumentViewModels.CollectionChanged += DocumentViewModelsCollectionChanged;
            //_viewModel.WorkItemViewModels.CollectionChanged += WorkItemViewModelsCollectionChanged;
            _viewModel.PropertyChanged += ViewModelPropertyChanged;
            ButtonDocuments.ContextMenu.MinWidth = 250;
            //ButtonWorkItems.ContextMenu.MinWidth = 250;
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
                    //_captureView.Backup();
                    WorkingFolder.Delete(WorkingFolder.UndeletedFiles);
                }
            }
            catch(Exception ex)
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
            if (_viewModel.ContentViewModel is SearchViewModel)
            {
                if (_searchView == null)
                {
                    _searchView = new SearchView { DataContext = _viewModel.ContentViewModel };
                }

                ViewContainer.Content = _searchView;
            }
            else if (_viewModel.ContentViewModel is CaptureViewModel)
            {
                if (_captureView == null)
                {
                    _captureView = new CaptureView { DataContext = _viewModel.ContentViewModel };
                }

                ViewContainer.Content = _captureView;
            }
            //else if (_viewModel.ContentViewModel is AssignedTaskViewModel)
            //{
            //    if (_assignedTaskView == null)
            //    {
            //        _assignedTaskView = new AssignedTaskView { DataContext = _viewModel.ContentViewModel };
            //    }

            //    ViewContainer.Content = _assignedTaskView;
            //}
            else if (_viewModel.ContentViewModel is AboutViewModel)
            {
                if (_aboutView == null)
                {
                    _aboutView = new AboutView { DataContext = _viewModel.ContentViewModel };
                }

                ViewContainer.Content = _aboutView;
            }
            else if (_viewModel.ContentViewModel is DocumentViewModel)
            {
                var existedView = _documentViews.FirstOrDefault(p => p.DataContext == _viewModel.ContentViewModel);
                if (existedView == null)
                {
                    existedView = new DocumentView { DataContext = _viewModel.ContentViewModel };
                    _documentViews.Add(existedView);
                }

                ViewContainer.Content = existedView;
            }
            //else if (_viewModel.ContentViewModel is WorkItemViewModel)
            //{
            //    var existedView = _workItemViews.FirstOrDefault(p => p.DataContext == _viewModel.ContentViewModel);
            //    if (existedView == null)
            //    {
            //        existedView = new WorkItemView { DataContext = _viewModel.ContentViewModel };
            //        _workItemViews.Add(existedView);
            //    }

            //    ViewContainer.Content = existedView;
            //}
        }

        private void RunGlobalSearch(object sender, RoutedEventArgs e)
        {
            if (GlobalSearchTextbox.HasText)
            {
                _viewModel.RunGlobalSearch(GlobalSearchTextbox.Text);
                _searchView.ToggleSearchConditionArea(false);
            }
        }

        private void DocumentViewModelsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            var documentItems = new List<MenuItem>();
            foreach (var item in ButtonDocuments.ContextMenu.Items)
            {
                if (item is MenuItem && (item as MenuItem).DataContext is DocumentViewModel)
                {
                    documentItems.Add(item as MenuItem);
                }
            }

            foreach (var item in documentItems)
            {
                ButtonDocuments.ContextMenu.Items.Remove(item);
                var docViewModel = item.DataContext as DocumentViewModel;
                _viewModel.DeleteOutlookTempFiles(docViewModel.Document.EmbeddedPictures.Keys.ToList());
            }

            foreach (var documentViewModel in _viewModel.DocumentViewModels)
            {
                var menuItem = new MenuItem
                                   {
                                       DataContext = documentViewModel,
                                       Style = TryFindResource("DocumentMenuItemStyle") as Style,
                                       IsCheckable = true
                                   };
                var binding = new Binding("IsActivated") { Mode = BindingMode.TwoWay };
                BindingOperations.SetBinding(menuItem, MenuItem.IsCheckedProperty, binding);

                binding = new Binding("DocumentName") { NotifyOnTargetUpdated = true };
                BindingOperations.SetBinding(menuItem, HeaderedItemsControl.HeaderProperty, binding);
                ButtonDocuments.ContextMenu.Items.Add(menuItem);
            }

            var orphanDocumentViews = _documentViews.Where(p => !_viewModel.DocumentViewModels.Any(q => q == p.DataContext)).ToList();
            foreach (var orphanDocumentView in orphanDocumentViews)
            {
                _documentViews.Remove(orphanDocumentView);
            }
        }

        private void SplitButton_DropDownClosing(object sender, RoutedEventArgs e)
        {
            if (_viewModel.ContentViewModel is CaptureViewModel)
            {
                CaptureView view = ViewContainer.Content as CaptureView;
                if (view.ViewerContainer != null && view.ViewerContainer._isNativeMode)
                {
                    view.ViewerContainer.ShowHideNativeView(false);
                }
            }
            else if (_viewModel.ContentViewModel is DocumentViewModel)
            {
                DocumentView view = ViewContainer.Content as DocumentView;
                if (view.ViewerContainer != null && view.ViewerContainer._isNativeMode)
                {
                    view.ViewerContainer.ShowHideNativeView(false);
                }
            }
                
        }

        private void SplitButton_DropDownOpenning(object sender, RoutedEventArgs e)
        {
            if (_viewModel.ContentViewModel is CaptureViewModel)
            {
                CaptureView view = ViewContainer.Content as CaptureView;
                if (view.ViewerContainer != null && view.ViewerContainer._isNativeMode)
                {
                    view.ViewerContainer.ShowHideNativeView(true);
                }
            }
            else if (_viewModel.ContentViewModel is DocumentViewModel)
            {
                DocumentView view = ViewContainer.Content as DocumentView;
                if (view.ViewerContainer != null && view.ViewerContainer._isNativeMode)
                {
                    view.ViewerContainer.ShowHideNativeView(true);
                }
            }
        }

    }
}
