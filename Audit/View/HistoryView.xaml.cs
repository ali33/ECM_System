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
using Ecm.Audit.ViewModel;
using Ecm.Model;
using System.Windows.Controls.Primitives;
using Ecm.DocViewer.Converter;
using Ecm.CustomControl;
using System.Data;
using System.IO;
using System.Xml;
using System.ComponentModel;
using Ecm.Domain;
using Ecm.DocViewer.Controls;
using Ecm.DocViewer.Model;
using Ecm.Model.DataProvider;
using Ecm.DocViewer.Helper;
using Ecm.AppHelper;

namespace Ecm.Audit.View
{
    /// <summary>
    /// Interaction logic for HistoryView.xaml
    /// </summary>
    public partial class HistoryView : UserControl
    {
        private HistoryViewModel _viewModel;
        private ActionLogProvider _actionLogProvider = new ActionLogProvider();
        private const string _folderName = "Search";

        public HistoryView()
        {
            InitializeComponent();
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
                pnMainSearch.Height = 20;
            }
        }

        private void SearchViewLoaded(object sender, RoutedEventArgs e)
        {
            try
            {
                _viewModel = DataContext as HistoryViewModel;
                if (_viewModel != null)
                {
                    _viewModel.PropertyChanged += OnPropertyChanged;
                    if (_viewModel.SearchResults != null && _viewModel.SearchResults.Count > 0)
                    {
                        BuildResultView(_viewModel.SearchResults);
                    }
                }
            }
            catch(Exception ex)
            {
                ProcessHelper.ProcessException(ex);
            }
        }

        private void DocTypeMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if ((sender as ListBox).SelectedItem == null)
            {
                return;
            }

            try
            {
                _viewModel.RunAdvanceSearch();

                if (SearchPanel.Visibility == System.Windows.Visibility.Hidden)
                {
                    SearchPanel.Visibility = System.Windows.Visibility.Collapsed;
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
                if (_viewModel.SearchResults != null)
                {
                    BuildResultView(_viewModel.SearchResults);
                }
                else
                {
                    PanelResult.Children.Clear();
                    var emptyRow = new TextBlock
                    {
                        Text = "No document match your criteria.",
                        Style = TryFindResource("EmptyTextRow") as Style
                    };
                    PanelResult.Children.Add(emptyRow);
                }
            }
        }

        private void BuildResultView(IEnumerable<SearchResultModel> results)
        {
            PanelResult.Children.Clear();
            foreach (var result in results)
            {
                Grid gridHeader = BuildResultHeader(result);
                PanelResult.Children.Add(gridHeader);
                Grid gridData = BuildResultData(result);
                PanelResult.Children.Add(gridData);
                var toggleHeaderButton = gridHeader.Children[0] as ToggleButton;
                if (toggleHeaderButton != null)
                {
                    toggleHeaderButton.Tag = gridData;
                    toggleHeaderButton.Click += ToogleHeaderButtonClick;
                }
            }
        }

        private Grid BuildResultHeader(SearchResultModel result)
        {
            var grid = new Grid { Style = TryFindResource("ResultHeader") as Style };
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto), MinWidth = 250 });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            var toggleButton = new ToggleButton { Style = TryFindResource("ResultHeaderToogle") as Style };
            var header = new TextBlock
                             {
                                 DataContext = result,
                                 Style = TryFindResource("ResultHeaderText") as Style
                             };
            var binding = new Binding("HeaderText") { NotifyOnTargetUpdated = true };
            BindingOperations.SetBinding(header, TextBlock.TextProperty, binding);
            var showMoreButton = new Button
                                     {
                                         DataContext = result,
                                         Content = "Show more results",
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

        private Grid BuildResultData(SearchResultModel result)
        {
            var grid = new Grid();
            if (result.DataResult.Rows.Count == 0)
            {
                grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) });
                grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) });
                var emptyRow = new TextBlock
                                   {
                                       Text = "No document match your criteria.",
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
            listView.SelectionMode = SelectionMode.Single;
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
                    column.ColumnName == Common.COLUMN_BINARY_TYPE ||
                    column.ColumnName == Common.COLUMN_SELECTED || column.ColumnName == Common.COLUMN_CHECKED ||
                    column.ColumnName == Common.COLUMN_DOCUMENT_VERSION_ID)
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
            if (column.ColumnName == Common.COLUMN_SELECTED || column.ColumnName == Common.COLUMN_CHECKED)
            {
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
                        searchResult.LoadMoreResult();
                    }
                }
            }
            catch(Exception ex)
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
            builder.Append(@"<DataTemplate xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation' xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'>
                                <TextBlock Text='{Binding Path=[" + columnName + @"],Converter={StaticResource FormatShortDateStringConverter}}' Padding='0,0,10,0' />
                             </DataTemplate>");

            return BuildDataTemplate(builder.ToString());
        }

        private DataTemplate GetLongDateColumnTemplate(string columnName)
        {
            var builder = new StringBuilder();
            builder.Append(@"<DataTemplate xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation' xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'>
                                <TextBlock Text='{Binding Path=[" + columnName + @"],Converter={StaticResource FormatLongDateStringConverter}}' Padding='0,0,10,0' />
                             </DataTemplate>");

            return BuildDataTemplate(builder.ToString());
        }

        public DataTemplate GetBooleanColumnTemplate(string columnName)
        {
            var builder = new StringBuilder();
            builder.Append(@"<DataTemplate xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation' xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'>
                                <TextBlock Text='{Binding Path=[" + columnName + @"], Converter={StaticResource YesNoConverter}}' Padding='0,0,10,0' />
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

        private void ResultItemMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                _viewModel.OpenDocument(((DataRowView)(((ContentControl)(e.Source)).Content)).Row);
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
                var item = sender as ListViewItem;
                var listview = ItemsControl.ItemsControlFromItemContainer(item) as SortableListView;

                if (e.RightButton == MouseButtonState.Pressed)
                {
                    e.Handled = true;

                    // User right-clickes on unselected row
                    if (item.IsSelected == false)
                    {
                        // Clear current selected rows
                        (listview.DataContext as SearchResultModel).IsSelected = false;

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
                                (listview.DataContext as SearchResultModel).IsSelected = false;
                            }
                            else
                            {
                                // The first selected item is used for the pivot item
                                int startIndex = listview.SelectedIndex;
                                int endIndex = currentIndex;

                                (listview.DataContext as SearchResultModel).IsSelected = false;

                                if (startIndex < endIndex)
                                {
                                    // Set selected status to all items in the range from the pivot item to the current item
                                    for (int index = startIndex; index <= endIndex; index++)
                                    {
                                        (listview.ItemContainerGenerator.ContainerFromIndex(index) as ListViewItem).IsSelected = true;
                                    }
                                }
                                else
                                {
                                    // Set selected status to all items in the range from the pivot item to the current item
                                    for (int index = startIndex; index >= endIndex; index--)
                                    {
                                        (listview.ItemContainerGenerator.ContainerFromIndex(index) as ListViewItem).IsSelected = true;
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

        private void PrintClick(object sender, RoutedEventArgs e)
        {
            try
            {
                _viewModel.IsProcessing = true;

                var printWorker = new BackgroundWorker();
                printWorker.DoWork += ActionWorkerDoWork;
                printWorker.RunWorkerCompleted += PrintWorkerRunWorkerCompleted;
                printWorker.RunWorkerAsync(true);

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

        private void PrintWorkerRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Result is Exception)
            {
                _viewModel.IsProcessing = false;
                ProcessHelper.ProcessException(e.Result as Exception);
            }
            else
            {
                try
                {
                    if (e.Result != null)
                    {
                        var documents = (List<DocumentModel>)e.Result;
                        var items = new List<CanvasElement>();

                        foreach (var document in documents)
                        {
                            var permission = new ContentViewerPermission
                            {
                                CanHideAnnotation = document.DocumentType.AnnotationPermission.AllowedHideRedaction,
                                CanSeeHighlight = document.DocumentType.AnnotationPermission.AllowedSeeHighlight,
                                CanSeeText = document.DocumentType.AnnotationPermission.AllowedSeeText
                            };

                            items.AddRange(document.Pages.Select(page => new CanvasElement(page.FileBinaries, page, permission)));

                            _actionLogProvider.AddLog("Print document", ActionName.Print, ObjectType.Document, document.Id);
                        }

                        var printHelper = new PrintHelper("CloudECM", new WorkingFolder(_folderName)) { HandleException = ProcessHelper.ProcessException };
                        printHelper.Print(items);
                    }
                }
                catch (Exception ex)
                {
                    ProcessHelper.ProcessException(ex);
                }
                finally
                {
                    _viewModel.IsProcessing = false;
                }
            }
        }


        private void Mail_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _viewModel.IsProcessing = true;

                var emailWorker = new BackgroundWorker();
                emailWorker.DoWork += ActionWorkerDoWork;
                emailWorker.RunWorkerCompleted += EmailWorkerRunWorkerCompleted;
                emailWorker.RunWorkerAsync(true);
            }
            catch (Exception ex)
            {
                ProcessHelper.ProcessException(ex);
            }
        }

        private void EmailWorkerRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Result is Exception)
            {
                _viewModel.IsProcessing = false;
                ProcessHelper.ProcessException(e.Result as Exception);
            }
            else
            {
                try
                {
                    if (e.Result != null)
                    {
                        var emailHelper = new SendMailHelper(new WorkingFolder(_folderName)) { HandleException = ProcessHelper.ProcessException };
                        var documents = (List<DocumentModel>)e.Result;

                        foreach (var document in documents)
                        {
                            string fileName = Guid.NewGuid().ToString();

                            if (document.BinaryType == FileTypeModel.Image)
                            {
                                var permission = new ContentViewerPermission
                                {
                                    CanHideAnnotation = document.DocumentType.AnnotationPermission.AllowedHideRedaction,
                                    CanSeeHighlight = document.DocumentType.AnnotationPermission.AllowedSeeHighlight,
                                    CanSeeText = document.DocumentType.AnnotationPermission.AllowedSeeText
                                };

                                var items = new List<CanvasElement>();
                                items.AddRange(document.Pages.Select(page => new CanvasElement(page.FileBinaries, page, permission)));

                                emailHelper.AddAttachment(items, fileName);
                            }
                            else
                            {
                                string extension = document.Pages[0].FileExtension.StartsWith(".") ? document.Pages[0].FileExtension : "." + document.Pages[0].FileExtension;
                                emailHelper.AddAttachment(document.Pages[0].FileBinaries, fileName, extension);
                            }

                            _actionLogProvider.AddLog("Email document", ActionName.SendEmail, ObjectType.Document, document.Id);
                        }

                        emailHelper.SendMail();
                    }
                }
                catch (Exception ex)
                {
                    ProcessHelper.ProcessException(ex);
                }
                finally
                {
                    _viewModel.IsProcessing = false;
                }
            }
        }

    }
}
