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
using System.Windows.Shapes;
using Ecm.Audit.ViewModel;
using Ecm.Audit.View;

namespace Ecm.Audit
{
    /// <summary>
    /// Interaction logic for MainView.xaml
    /// </summary>
    public partial class MainView : Page
    {
        private MainViewModel _viewModel = new MainViewModel();
        //private ActionLogView _actionLogView;
        //private AuditView _auditView;
        //private HistoryView _historyView;
        //private HistoryDetailView _historyDetailView;
        //private AboutView _aboutView;
        private List<HistoryDetailView> _historyDetailViews = new List<HistoryDetailView>();
        public MainView()
        {
            InitializeComponent();
            DataContext = _viewModel;
            _viewModel.HistoryDetailViewModels.CollectionChanged += HistoryDetailViewModelsCollectionChanged;
            _viewModel.PropertyChanged += ViewModelPropertyChanged;
            Loaded += MainViewLoaded;
            ButtonDocuments.ContextMenu.MinWidth = 250;

        }

        void MainViewLoaded(object sender, RoutedEventArgs e)
        {
            if (_viewModel.ViewModel != null)
            {
                LoadView();
            }
        }

        void ViewModelPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "ViewModel")
            {
                LoadView();
            }
        }

        void HistoryDetailViewModelsCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            var documentItems = new List<MenuItem>();
            foreach (var item in ButtonDocuments.ContextMenu.Items)
            {
                if (item is MenuItem && (item as MenuItem).DataContext is HistoryDetailViewModel)
                {
                    documentItems.Add(item as MenuItem);
                }
            }

            foreach (var item in documentItems)
            {
                ButtonDocuments.ContextMenu.Items.Remove(item);
            }

            foreach (var historyDetailViewModel in _viewModel.HistoryDetailViewModels)
            {
                var menuItem = new MenuItem
                {
                    DataContext = historyDetailViewModel,
                    Style = TryFindResource("DocumentMenuItemStyle") as Style,
                    IsCheckable = true
                };
                var binding = new Binding("IsActivated") { Mode = BindingMode.TwoWay };
                BindingOperations.SetBinding(menuItem, MenuItem.IsCheckedProperty, binding);

                binding = new Binding("DocumentName") { NotifyOnTargetUpdated = true };
                BindingOperations.SetBinding(menuItem, HeaderedItemsControl.HeaderProperty, binding);
                ButtonDocuments.ContextMenu.Items.Add(menuItem);
            }

            var orphanDocumentViews = _historyDetailViews.Where(p => !_viewModel.HistoryDetailViewModels.Any(q => q == p.DataContext)).ToList();
            foreach (var orphanDocumentView in orphanDocumentViews)
            {
                _historyDetailViews.Remove(orphanDocumentView);
            }
        }

        private void GlobalSearchTextbox_Search(object sender, RoutedEventArgs e)
        {
            if (GlobalSearchTextbox.HasText)
            {
                (DataContext as MainViewModel).RunGlobalSearch(GlobalSearchTextbox.Text);
            }
        }

        private void LoadView()
        {
            if (_viewModel.ViewModel is HistoryDetailViewModel)
            {
                var existedView = _historyDetailViews.FirstOrDefault(p => p.DataContext == _viewModel.ViewModel);

                if (existedView == null)
                {
                    existedView = new HistoryDetailView { DataContext = _viewModel.ViewModel };
                    _historyDetailViews.Add(existedView);
                }

                ViewContainer.Content = existedView;
            }
        }

    }
}
