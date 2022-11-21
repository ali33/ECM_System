using System;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Xml;
using Ecm.Capture.ViewModel;
using Ecm.CustomControl;
using Ecm.CaptureViewer.Converter;
using Ecm.CaptureModel;
using System.Configuration;
using System.Resources;
using System.Reflection;
using Ecm.Capture.Converter;
using System.Diagnostics;

namespace Ecm.Capture.View
{
    public partial class WorkItemListView
    {
        #region Private members
        private readonly ResourceManager _resource = new ResourceManager("Ecm.Capture.Resources", Assembly.GetExecutingAssembly());

        private WorkItemListViewModel _viewModel;

        #endregion

        public WorkItemListView()
        {
            InitializeComponent();
            Loaded += WorkItemListViewLoaded;
        }

        #region Private methods

        private void WorkItemListViewLoaded(object sender, RoutedEventArgs e)
        {
            try
            {
                _viewModel = DataContext as WorkItemListViewModel;
                if (_viewModel != null)
                {
                    _viewModel.PropertyChanged += ViewModelPropertyChanged;
                }
            }
            catch (Exception ex)
            {
                ProcessHelper.ProcessException(ex);
            }
        }

        private void ViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            try
            {
                if (e.PropertyName == "SearchResult")
                {
                    if (App.Current != null && App.Current.Dispatcher != null)
                    {
                        App.Current.Dispatcher.Invoke(new Action(delegate
                        {
                            if (_viewModel.SearchResult != null)
                            {
                                BuildResultView(_viewModel.SearchResult);
                            }
                            else
                            {
                                pnResult.Children.Clear();

                                var emptyRow = new TextBlock
                                {
                                    Text = _resource.GetString("uiNoWorkitemFound"),
                                    Style = TryFindResource("EmptyTextRow") as Style
                                };
                                pnResult.Children.Add(emptyRow);
                            }
                        }));
                    }
                }
            }
            catch (Exception ex)
            {
                ProcessHelper.ProcessException(ex);
            }
        }

        private void BuildResultView(WorkItemSearchResultModel result)
        {
            pnResult.Children.Clear();

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

        private Grid BuildResultHeader(WorkItemSearchResultModel result)
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
            var binding = new Binding("BatchTypeName") { NotifyOnTargetUpdated = true };
            BindingOperations.SetBinding(docTypeName, TextBlock.TextProperty, binding);
            var spaceLine = new TextBlock { Text = " - " };
            var resultCount = new TextBlock
            {
                DataContext = result
            };
            binding = new Binding("ResultCount") { NotifyOnTargetUpdated = true };
            BindingOperations.SetBinding(resultCount, TextBlock.TextProperty, binding);
            header.Children.Add(docTypeName);
            header.Children.Add(spaceLine);
            header.Children.Add(resultCount);

            var showMoreButton = new Button
            {
                DataContext = result,
                Content = _resource.GetString("uiShowMoreResults"),
                Style = TryFindResource("ResultHeaderButton") as Style,
                Tag = result
            };
            binding = new Binding("HasMoreResult") { NotifyOnTargetUpdated = true, Converter = new BoolVisibilityConverter() };
            BindingOperations.SetBinding(showMoreButton, VisibilityProperty, binding);
            showMoreButton.Click += ShowMoreButtonClick;
            grid.Children.Add(toggleButton);
            grid.Children.Add(header);
            grid.Children.Add(showMoreButton);
            Grid.SetColumn(header, 1);
            Grid.SetColumn(showMoreButton, 2);

            return grid;
        }

        private Grid BuildResultData(WorkItemSearchResultModel result)
        {
            var grid = new Grid();
            if (result.DataResult.Rows.Count == 0)
            {
                grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) });
                grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) });
                var emptyRow = new TextBlock
                {
                    Text = _resource.GetString("uiNoWorkitemFound"),
                    Style = TryFindResource("EmptyTextRow") as Style
                };
                grid.Children.Add(emptyRow);
                Grid.SetRow(emptyRow, 1);
            }

            var listView = new SortableListView { SelectionMode = SelectionMode.Multiple, DataContext = result };

            // Create a binding with auto resize column widths of listview
            var binding = new Binding("DataResult") { NotifyOnTargetUpdated = true };
            BindingOperations.SetBinding(listView, ItemsControl.ItemsSourceProperty, binding);
            Binding.AddTargetUpdatedHandler(listView, ListViewTargetUpdated);

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

        private GridView GenerateGridView(WorkItemSearchResultModel searchResult)
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

        private void BuildColumns(WorkItemSearchResultModel searchResult, GridView view)
        {
            ResourceManager resource = new ResourceManager("Ecm.Capture.WorkItemListView", Assembly.GetExecutingAssembly());

            DataTable table = searchResult.DataResult;
            foreach (DataColumn column in table.Columns)
            {
                if (column.ColumnName == Common.COLUMN_BATCH ||
                    column.ColumnName == Common.COLUMN_BATCH_ID ||
                    column.ColumnName == Common.COLUMN_BATCH_TYPE_ID ||
                    column.ColumnName == Common.COLUMN_BINARY_TYPE ||
                    column.ColumnName == Common.COLUMN_BLOCKING_ACTIVITY_DESCRIPTION ||
                    column.ColumnName == Common.COLUMN_BLOCKING_BOOKMARK ||
                    column.ColumnName == Common.COLUMN_WORKFLOW_DEFINITION_ID ||
                    column.ColumnName == Common.COLUMN_WORKFLOW_INSTANCE_ID ||
                    column.ColumnName == Common.COLUMN_VERSION ||
                    column.ColumnName == Common.COLUMN_MODIFIED_BY ||
                    column.ColumnName == Common.COLUMN_MODIFIED_ON ||
                    column.ColumnName == Common.COLUMN_PERMISSION)
                {
                    continue;
                }

                var displayColumnName = column.ColumnName.StartsWith("gvh") ? resource.GetString(column.ColumnName) : column.ColumnName;

                var viewColumn = new SortableGridViewColumn
                {
                    Header = displayColumnName,
                    DisplayMemberBinding = new Binding(column.ColumnName),
                    SortPropertyName = column.ColumnName
                };

                PopulateColumnStyle(searchResult, viewColumn, column);
                view.Columns.Add(viewColumn);
            }
        }

        private void PopulateColumnStyle(WorkItemSearchResultModel searchResult, SortableGridViewColumn viewColumn, DataColumn column)
        {
            if (column.ColumnName == Common.COLUMN_SELECTED)
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
                    if (column.ColumnName == Common.COLUMN_MODIFIED_ON
                        || column.ColumnName == Common.COLUMN_CREATED_ON
                        || column.ColumnName == Common.COLUMN_BLOCKING_DATE
                        || column.ColumnName == Common.COLUMN_LAST_ACCESSED_DATE)
                    {
                        viewColumn.CellTemplate = GetLongDateColumnTemplate(column.ColumnName);
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
                    var searchResult = showMoreButton.Tag as WorkItemSearchResultModel;
                    if (searchResult != null)
                    {
                        //searchResult.LoadMoreResult(LoginViewModel.LoginUser);
                    }
                }
            }
            catch (Exception ex)
            {
                ProcessHelper.ProcessException(ex);
            }
        }

        private void ResultItemMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                _viewModel.OpenWorkItem(((DataRowView)(((ContentControl)(e.Source)).Content)).Row);
                var item = sender as ListViewItem;

                // Set the focus row to be selected
                if (item != null)
                {
                    item.IsSelected = true;
                }

                e.Handled = true;
            }
            catch (Exception ex)
            {
                ProcessHelper.ProcessException(ex);
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
                        ((WorkItemSearchResultModel)listview.DataContext).IsSelected = false;

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
                                ((WorkItemSearchResultModel)listview.DataContext).IsSelected = false;
                            }
                            else
                            {
                                // The first selected item is used for the pivot item
                                int startIndex = listview.SelectedIndex;
                                int endIndex = currentIndex;

                                ((WorkItemSearchResultModel)listview.DataContext).IsSelected = false;

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
                }
            }
            catch (Exception ex)
            {
                ProcessHelper.ProcessException(ex);
            }
        }

        private void ListViewEndSortingProcess(object sender, EventArgs e)
        {
            // Update listview column widths to make sure that direction arrow will be visible
            ListViewTargetUpdated(sender, null);
        }

        private void ListViewTargetUpdated(object sender, DataTransferEventArgs e)
        {
            // Get a reference to the ListView's GridView...
            ListView listview = sender as ListView;

            if (listview != null)
            {
                GridView gridView = listview.View as GridView;

                if (gridView != null)
                {
                    // For each column...
                    foreach (GridViewColumn column in gridView.Columns)
                    {
                        if (((SortableGridViewColumn)column).SortPropertyName != null)
                        {
                            column.Width = 0;
                            column.Width = double.NaN;
                        }
                    }
                }
            }
        }

        private DataTemplate GetStringColumnTemplate(string columnName)
        {
            var textBlockFactory = new FrameworkElementFactory(typeof(TextBlock));
            textBlockFactory.SetValue(TextBlock.PaddingProperty, new Thickness(0, 0, 10, 0));

            var binding = new Binding(columnName);
            textBlockFactory.SetValue(TextBlock.TextProperty, binding);

            if ("gvhStatus".Equals(columnName, StringComparison.OrdinalIgnoreCase))
            {
                var bindingToolTip = new Binding(columnName);
                textBlockFactory.SetValue(TextBlock.ToolTipProperty, bindingToolTip);
            }

            var dataTemplate = new DataTemplate();
            dataTemplate.VisualTree = textBlockFactory;

            return dataTemplate;
        }

        private DataTemplate GetRightAlignColumnTemplate(string columnName)
        {
            var textBlockFactory = new FrameworkElementFactory(typeof(TextBlock));
            textBlockFactory.SetValue(TextBlock.PaddingProperty, new Thickness(0, 0, 10, 0));
            textBlockFactory.SetValue(TextBlock.TextAlignmentProperty, TextAlignment.Right);

            var binding = new Binding(columnName);
            textBlockFactory.SetValue(TextBlock.TextProperty, binding);

            var dataTemplate = new DataTemplate();
            dataTemplate.VisualTree = textBlockFactory;

            return dataTemplate;
        }

        private DataTemplate GetShortDateColumnTemplate(string columnName)
        {
            var textBlockFactory = new FrameworkElementFactory(typeof(TextBlock));
            textBlockFactory.SetValue(TextBlock.PaddingProperty, new Thickness(0, 0, 10, 0));

            var binding = new Binding(columnName);
            binding.Converter = new FormatShortDateStringConverter();
            textBlockFactory.SetValue(TextBlock.TextProperty, binding);

            var dataTemplate = new DataTemplate();
            dataTemplate.VisualTree = textBlockFactory;

            return dataTemplate;
        }

        private DataTemplate GetLongDateColumnTemplate(string columnName)
        {
            var textBlockFactory = new FrameworkElementFactory(typeof(TextBlock));
            textBlockFactory.SetValue(TextBlock.PaddingProperty, new Thickness(0, 0, 10, 0));

            var binding = new Binding(columnName);
            binding.Converter = new FormatLongDateStringConverter();
            textBlockFactory.SetValue(TextBlock.TextProperty, binding);

            var dataTemplate = new DataTemplate();
            dataTemplate.VisualTree = textBlockFactory;

            return dataTemplate;
        }

        public DataTemplate GetBooleanColumnTemplate(string columnName)
        {
            var textBlockFactory = new FrameworkElementFactory(typeof(TextBlock));
            textBlockFactory.SetValue(TextBlock.PaddingProperty, new Thickness(0, 0, 10, 0));

            var binding = new Binding(columnName);
            binding.Converter = new Ecm.CaptureViewer.Converter.YesNoConverter();
            textBlockFactory.SetValue(TextBlock.TextProperty, binding);

            var dataTemplate = new DataTemplate();
            dataTemplate.VisualTree = textBlockFactory;

            return dataTemplate;
        }

        private DataTemplate GetCheckBoxColumnTemplate(string columnName)
        {
            var checkBoxFactory = new FrameworkElementFactory(typeof(CheckBox));
            checkBoxFactory.SetValue(CheckBox.PaddingProperty, new Thickness(4, 0, 4, 0));
            checkBoxFactory.SetValue(CheckBox.VerticalAlignmentProperty, VerticalAlignment.Center);
            checkBoxFactory.SetValue(CheckBox.HorizontalAlignmentProperty, HorizontalAlignment.Center);

            var binding = new Binding(columnName);
            checkBoxFactory.SetValue(CheckBox.IsCheckedProperty, binding);

            var dataTemplate = new DataTemplate();
            dataTemplate.VisualTree = checkBoxFactory;

            return dataTemplate;
        }

        #endregion
    }
}
