using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Data;
using System.Xml;
using System.IO;
using System.ComponentModel;
using System.Linq;

using Ecm.AppHelper;
using Ecm.CustomAddin.ViewModel;
using Ecm.CustomControl;
using Ecm.Model;
using Ecm.Mvvm;
using Ecm.Model.DataProvider;
using Ecm.Domain;
using System.Resources;
using System.Reflection;
using Ecm.CustomAddin.Converter;

namespace Ecm.CustomAddin.View
{
    public partial class SearchView : Window
    {
        private SearchViewModel _viewModel;
        private const string _folderName = "Search";
        private readonly ActionLogProvider _actionLogProvider = new ActionLogProvider();
        private readonly ResourceManager _resource = new ResourceManager("Ecm.CustomAddin.Resources", Assembly.GetExecutingAssembly());
        private AddinType _addinType;

        public SearchView(AddinType type)
        {
            InitializeComponent();
            _addinType = type;
            Loaded += SearchViewLoaded;
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

        private void SearchViewLoaded(object sender, RoutedEventArgs e)
        {
            try
            {
                _viewModel = new SearchViewModel(CloseDialog, _addinType);
                DataContext = _viewModel;

                if (_viewModel != null)
                {
                    _viewModel.PropertyChanged -= OnPropertyChanged;
                    _viewModel.PropertyChanged += OnPropertyChanged;
                    if (_viewModel.SearchResults != null && _viewModel.SearchResults.Count > 0)
                    {
                        BuildResultView(_viewModel.SearchResults);
                    }
                }
            }
            catch (Exception ex)
            {
                ProcessHelper.ProcessException(ex);
            }
        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "SearchResults")
            {
                pnResult.Children.Clear();
                if (_viewModel.SearchResults != null && _viewModel.SearchResults.Count > 0)
                {
                    BuildResultView(_viewModel.SearchResults);
                    _viewModel.SearchResults.CollectionChanged -= SearchResultsCollectionChanged;
                    _viewModel.SearchResults.CollectionChanged += SearchResultsCollectionChanged;
                }
                else
                {
                    var emptyRow = new TextBlock
                    {
                        Text = _resource.GetString("uiNoDocumentMatch"),
                        Style = TryFindResource("EmptyTextRow") as Style
                    };

                    pnResult.Children.Add(emptyRow);
                }
            }
        }

        private void SearchResultsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (SearchResultModel result in e.NewItems)
                {
                    BuildViewForEachResult(result);
                }
            }
        }

        private void BuildResultView(IList<SearchResultModel> results)
        {
            pnResult.Children.Clear();
            //if (results[0].IsGlobalSearch)
            //{
            //    pnResult.Children.Add(BuildGlobalSearchHeader(results[0]));
            //}

            foreach (var result in results)
            {
                BuildViewForEachResult(result);
            }
        }

        private void BuildViewForEachResult(SearchResultModel result)
        {
            Grid gridHeader = BuildResultHeader(result);
            pnResult.Children.Add(gridHeader);
            Grid gridData = BuildResultData(result);
            pnResult.Children.Add(gridData);
            pnResult.Children.Add(new Grid { Height = 20 });

            var toggleHeaderButton = gridHeader.Children[0] as ToggleButton;
            if (toggleHeaderButton != null)
            {
                toggleHeaderButton.Tag = gridData;
                toggleHeaderButton.Click += ToogleHeaderButtonClick;
            }
        }

        private Grid BuildResultHeader(SearchResultModel result)
        {
            var grid = new Grid { Style = TryFindResource("ResultHeader") as Style };
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto), MinWidth = 250 });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            var toggleButton = new ToggleButton { Style = TryFindResource("ResultHeaderToogle") as Style };

            var header = new StackPanel { Orientation = Orientation.Horizontal, VerticalAlignment = VerticalAlignment.Center };
            var docTypeName = new TextBlock
                                  {
                                      DataContext = result,
                                      Style = TryFindResource("ResultHeaderText") as Style
                                  };

            var binding = new Binding("DocumentTypeName") { NotifyOnTargetUpdated = true };
            BindingOperations.SetBinding(docTypeName, TextBlock.TextProperty, binding);
            var spaceLine = new TextBlock { Text = " - " };
            var resultCount = new TextBlock
                                  {
                                      DataContext = result
                                  };

            binding = new Binding("ResultCount") { NotifyOnTargetUpdated = true };
            BindingOperations.SetBinding(resultCount, TextBlock.TextProperty, binding);

            grid.Children.Add(toggleButton);
            header.Children.Add(docTypeName);
            header.Children.Add(spaceLine);
            header.Children.Add(resultCount);

            if (!result.IsGlobalSearch)
            {
                var slash = new TextBlock { Text = "/" };
                var totalCount = new TextBlock
                                     {
                                         DataContext = result
                                     };
                binding = new Binding("TotalCount") { NotifyOnTargetUpdated = true };
                BindingOperations.SetBinding(totalCount, TextBlock.TextProperty, binding);

                header.Children.Add(slash);
                header.Children.Add(totalCount);
            }

            var unit = new TextBlock { Text = _resource.GetString("uiDocFound") };
            header.Children.Add(unit);
            grid.Children.Add(header);
            Grid.SetColumn(header, 1);
            if (!result.IsGlobalSearch)
            {
                var showMoreButton = new Button
                                         {
                                             DataContext = result,
                                             Content = _resource.GetString("uiShowMore"),
                                             Style = TryFindResource("ResultHeaderButton") as Style,
                                             Tag = result
                                         };
                binding = new Binding("HasMoreResult") { NotifyOnTargetUpdated = true, Converter = new BoolVisibilityConverter() };
                BindingOperations.SetBinding(showMoreButton, VisibilityProperty, binding);
                showMoreButton.Click += ShowMoreButtonClick;
                grid.Children.Add(showMoreButton);
                Grid.SetColumn(showMoreButton, 2);
            }

            return grid;
        }

        private Grid BuildResultData(SearchResultModel result)
        {
            var grid = new Grid();
            if (result.DataResult.Rows.Count == 0)
            {
                grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) });
                grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) });
                var emptyRow = new TextBlock
                                   {
                                       Text = _resource.GetString("uiNoDocumentMatch"),
                                       Style = TryFindResource("EmptyTextRow") as Style
                                   };
                grid.Children.Add(emptyRow);
                Grid.SetRow(emptyRow, 1);
            }

            var listView = new SortableListView { SelectionMode = SelectionMode.Single, DataContext = result, Name="lvSearchResult" };

            // Create a binding with auto resize column widths of listview
            var binding = new Binding("DataResult") { NotifyOnTargetUpdated = true };
            BindingOperations.SetBinding(listView, ItemsControl.ItemsSourceProperty, binding);
            Binding.AddTargetUpdatedHandler(listView, ListViewTargetUpdated);

            listView.SelectionMode = SelectionMode.Single;
            listView.ColumnHeaderSortedAscendingTemplate = "GridHeaderTemplateArrowUp";
            listView.ColumnHeaderSortedDescendingTemplate = "GridHeaderTemplateArrowDown";
            listView.ColumnHeaderNotSortedTemplate = "GridHeaderTemplateTransparent";
            listView.BorderThickness = new Thickness(0);
            listView.HorizontalAlignment = HorizontalAlignment.Stretch;
            listView.Focusable = false;
            listView.AddHandler(MouseWheelEvent, new RoutedEventHandler(UnHandleMouseWheelEvent), true);
            listView.ItemContainerStyle = TryFindResource("ResultItemStyle") as Style;
            listView.View = GenerateGridView(result);
            listView.EndSortingProcess += ListViewEndSortingProcess;
            grid.Children.Add(listView);
            return grid;
        }

        private GridView GenerateGridView(SearchResultModel searchResult)
        {
            DataTable table = searchResult.DataResult;
            if (table == null)
            {
                return null;
            }

            var view = new GridView();
            BuildColumns(searchResult, view);
            return view;
        }

        private void BuildColumns(SearchResultModel searchResult, GridView view)
        {
            DataTable table = searchResult.DataResult;
            foreach (DataColumn column in table.Columns)
            {
                if (column.ColumnName == Common.COLUMN_DOCUMENT ||
                    column.ColumnName == Common.COLUMN_DOCUMENT_ID ||
                    column.ColumnName == Common.COLUMN_DOCUMENT_TYPE_ID ||
                    column.ColumnName == Common.COLUMN_DOCUMENT_VERSION_ID ||
                    column.ColumnName == Common.COLUMN_SELECTED)
                {
                    continue;
                }

                var viewColumn = new SortableGridViewColumn
                {
                    Header = column.ColumnName,
                    DisplayMemberBinding = new Binding(column.ColumnName),
                    SortPropertyName = column.ColumnName
                };

                PopulateColumnStyle(searchResult, viewColumn, column);
                view.Columns.Add(viewColumn);
            }
        }

        private void PopulateColumnStyle(SearchResultModel searchResult, SortableGridViewColumn viewColumn, DataColumn column)
        {
            if (column.ColumnName == Common.COLUMN_CHECKED)
            {
                viewColumn.Header = searchResult;
                viewColumn.HeaderTemplate = FindResource("CheckBoxResultHeader") as DataTemplate;
                viewColumn.Width = 35;
                viewColumn.SortPropertyName = null;
                viewColumn.DisplayMemberBinding = null;
                viewColumn.CellTemplate = GetCheckBoxColumnTemplate(column.ColumnName);
                return;
            }

            if (column.DataType != typeof(string))
            {
                viewColumn.CellTemplate = GetRightAlignColumnTemplate(column.ColumnName);
                viewColumn.DisplayMemberBinding = null;

                if (column.DataType == typeof(DateTime))
                {
                    if (column.ColumnName == Common.COLUMN_MODIFIED_ON || column.ColumnName == Common.COLUMN_CREATED_ON)
                    {
                        viewColumn.CellTemplate = GetLongDateColumnTemplate(column.ColumnName);
                        //viewColumn.CellTemplate = GetShortDateColumnTemplate(column.ColumnName);
                    }
                    else
                    {
                        viewColumn.CellTemplate = GetShortDateColumnTemplate(column.ColumnName);
                    }
                }
                else if (column.DataType == typeof(bool))
                {
                    viewColumn.CellTemplate = GetBooleanColumnTemplate(column.ColumnName);
                }
            }
            else
            {
                viewColumn.CellTemplate = GetStringColumnTemplate(column.ColumnName);
                viewColumn.DisplayMemberBinding = null;
            }
        }

        private void UnHandleMouseWheelEvent(object sender, RoutedEventArgs e)
        {
            e.Handled = false;
        }

        private void ToogleHeaderButtonClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var toggleHeaderButton = sender as ToggleButton;
                if (toggleHeaderButton != null)
                {
                    var gridData = toggleHeaderButton.Tag as Grid;
                    if (gridData != null)
                    {
                        gridData.Visibility = toggleHeaderButton.IsChecked == true ? Visibility.Collapsed : Visibility.Visible;
                    }
                }
            }
            catch (Exception ex)
            {
                ProcessHelper.ProcessException(ex);
            }
        }

        private void ShowMoreButtonClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var showMoreButton = sender as Button;
                if (showMoreButton != null)
                {
                    var searchResult = showMoreButton.Tag as SearchResultModel;
                    if (searchResult != null)
                    {
                        searchResult.LoadMoreResult(LoginViewModel.LoginUser);
                    }
                }
            }
            catch (Exception ex)
            {
                ProcessHelper.ProcessException(ex);
            }
        }

        private DataTemplate GetStringColumnTemplate(string columnName)
        {
            var builder = new StringBuilder();
            builder.Append(@"<DataTemplate xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation'>
                                <TextBlock Text='{Binding Path=[" + columnName + @"]}' Padding='0,0,10,0' />
                             </DataTemplate>");

            return BuildDataTemplate(builder.ToString());
        }

        private DataTemplate GetRightAlignColumnTemplate(string columnName)
        {
            var builder = new StringBuilder();
            builder.Append(@"<DataTemplate xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation'>
                                <TextBlock Text='{Binding Path=[" + columnName + @"]}' TextAlignment='Right' Padding='0,0,10,0' />
                             </DataTemplate>");

            return BuildDataTemplate(builder.ToString());
        }

        private DataTemplate GetShortDateColumnTemplate(string columnName)
        {
            var builder = new StringBuilder();
            builder.Append(@"<DataTemplate xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation' 
                                           xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml' 
                                           xmlns:cv='clr-namespace:Ecm.CustomAddin.Converter;assembly=Ecm.CustomAddin'>
                                <DataTemplate.Resources>
                                    <cv:FormatShortDateStringConverter x:Key='FormatShortDateStringConverter'/>
                                </DataTemplate.Resources>
                                <TextBlock Text='{Binding Path=[" + columnName + @"]}' Padding='0,0,10,0' />
                             </DataTemplate>");

            return BuildDataTemplate(builder.ToString());
        }

        private DataTemplate GetLongDateColumnTemplate(string columnName)
        {
            var builder = new StringBuilder();
            builder.Append(@"<DataTemplate xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation' 
                                           xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml' 
                                           xmlns:cv='clr-namespace:Ecm.CustomAddin.Converter;assembly=Ecm.CustomAddin'>
                                <DataTemplate.Resources>
                                    <cv:FormatLongDateStringConverter x:Key='FormatLongDateStringConverter'/>
                                </DataTemplate.Resources>
                                <TextBlock Text='{Binding Path=[" + columnName + @"]}' Padding='0,0,10,0' />
                             </DataTemplate>");

            return BuildDataTemplate(builder.ToString());
        }

        public DataTemplate GetBooleanColumnTemplate(string columnName)
        {
            var builder = new StringBuilder();
            builder.Append(@"<DataTemplate xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation' 
                                           xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml' 
                                           xmlns:cv='clr-namespace:Ecm.CustomAddin.Converter;assembly=Ecm.CustomAddin'>
                                <DataTemplate.Resources>
                                    <cv:YesNoConverter x:Key='YesNoConverter'/>
                                </DataTemplate.Resources>
                                <TextBlock Text='{Binding Path=[" + columnName + @"]}' Padding='0,0,10,0' />
                             </DataTemplate>");

            return BuildDataTemplate(builder.ToString());
        }

        private DataTemplate GetCheckBoxColumnTemplate(string columnName)
        {
            var builder = new StringBuilder();
            builder.Append(@"<DataTemplate xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation'>
                                <StackPanel>
                                    <CheckBox IsChecked='{Binding Path=[" + columnName + @"]}' VerticalAlignment='Center' HorizontalAlignment='Center' Margin='4,0,4,0'></CheckBox>
                                </StackPanel>
                             </DataTemplate>");

            return BuildDataTemplate(builder.ToString());
        }

        private DataTemplate BuildDataTemplate(string xamlString)
        {
            using (var sr = new StringReader(xamlString))
            {
                using (XmlReader xr = XmlReader.Create(sr))
                {
                    return System.Windows.Markup.XamlReader.Load(xr) as DataTemplate;
                }
            }
        }

        private void ListViewTargetUpdated(object sender, DataTransferEventArgs e)
        {
            try
            {
                // Get a reference to the ListView's GridView...
                var listview = sender as ListView;
                if (listview != null)
                {
                    var gridView = listview.View as GridView;
                    if (gridView != null)
                    {
                        UpdateColumnWidths(gridView);
                    }
                }
            }
            catch (Exception ex)
            {
                ProcessHelper.ProcessException(ex);
            }
        }

        private void ListViewEndSortingProcess(object sender, EventArgs e)
        {
            try
            {
                // Get a reference to the ListView's GridView...
                var listview = sender as SortableListView;
                if (listview != null)
                {
                    var gridView = listview.View as GridView;
                    if (gridView != null)
                    {
                        UpdateColumnWidths(gridView);
                    }
                }
            }
            catch (Exception ex)
            {
                ProcessHelper.ProcessException(ex);
            }
        }

        private void UpdateColumnWidths(GridView gridView)
        {
            // For each column...
            foreach (GridViewColumn column in gridView.Columns)
            {
                column.Width = 0;
                column.Width = double.NaN;
            }
        }

        private void ResultItemPreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                var item = (ListViewItem)sender;
                var listview = (SortableListView)ItemsControl.ItemsControlFromItemContainer(item);

                if (e.RightButton == MouseButtonState.Pressed)
                {
                    e.Handled = true;

                    // User right-clickes on unselected row
                    if (item.IsSelected == false)
                    {
                        // Clear current selected rows
                        ((SearchResultModel)listview.DataContext).IsSelected = false;

                        // Set the focus row to be selected
                        item.IsSelected = true;
                    }
                }
                else if (e.LeftButton == MouseButtonState.Pressed)
                {
                    // Business Rule: User press and keep SHIFT keydown, then click left button of mouse to select multiple items
                    if (Keyboard.Modifiers == ModifierKeys.Shift)
                    {
                        e.Handled = true;

                        // If there is no selected item, set selected status to the item
                        if (listview.SelectedIndex == -1)
                        {
                            item.IsSelected = true;
                        }
                        else
                        {
                            int currentIndex = listview.ItemContainerGenerator.IndexFromContainer(item);

                            // If the current selected item is the first item in the current selection of listview, deselect all
                            if (currentIndex == listview.SelectedIndex)
                            {
                                ((SearchResultModel)listview.DataContext).IsSelected = false;
                            }
                            else
                            {
                                // The first selected item is used for the pivot item
                                int startIndex = listview.SelectedIndex;
                                int endIndex = currentIndex;

                                ((SearchResultModel)listview.DataContext).IsSelected = false;

                                if (startIndex < endIndex)
                                {
                                    // Set selected status to all items in the range from the pivot item to the current item
                                    for (int index = startIndex; index <= endIndex; index++)
                                    {
                                        ((ListViewItem)listview.ItemContainerGenerator.ContainerFromIndex(index)).IsSelected = true;
                                    }
                                }
                                else
                                {
                                    // Set selected status to all items in the range from the pivot item to the current item
                                    for (int index = startIndex; index >= endIndex; index--)
                                    {
                                        ((ListViewItem)listview.ItemContainerGenerator.ContainerFromIndex(index)).IsSelected = true;
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        item.IsSelected = true;
                        var viewModel = DataContext as SearchViewModel;
                        viewModel.LoadPageData();
                    }
                }
            }
            catch (Exception ex)
            {
                ProcessHelper.ProcessException(ex);
            }
        }

        private void ExcelItemMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                //_viewModel.OpenDocument(((DataRowView)(((ContentControl)(e.Source)).Content)).Row);
                var item = sender as ListViewItem;
                // Set the focus row to be selected
                if (item != null)
                {
                    _viewModel.OpenDocumentCommand.Execute(item);
                }

                //e.Handled = true;
            }
            catch (Exception ex)
            {
                ProcessHelper.ProcessException(ex);
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


        private void ActionWorkerDoWork(object sender, DoWorkEventArgs e)
        {
            // Download binary and split image if any
            try
            {
                e.Result = _viewModel.GetSelectedDocuments();
            }
            catch (Exception ex)
            {
                e.Result = ex;
            }
        }

        private void LvlFieldEndSortingProcess(object sender, EventArgs e)
        {
            LvlFieldTargetUpdated(sender, null);
        }

        private void LvlFieldTargetUpdated(object sender, DataTransferEventArgs e)
        {
            UpdateGridView(sender as ListView);
        }

        private void UpdateGridView(ListView listView)
        {
            if (listView != null)
            {
                var gridView = listView.View as GridView;

                if (gridView == null || gridView.Columns.Count < 1)
                {
                    return;
                }

                foreach (GridViewColumn column in gridView.Columns)
                {
                    column.Width = 0;
                    column.Width = double.NaN;
                }
            }
        }

        private void btnRemoveCondition_Click(object sender, RoutedEventArgs e)
        {
            var searchExpressionModel = ((FrameworkElement)sender).DataContext as SearchExpressionViewModel;
            _viewModel.RemoveAdditionalField.Execute(searchExpressionModel);
        }

        private void CloseDialog()
        {
            this.Close();
        }
    }
}