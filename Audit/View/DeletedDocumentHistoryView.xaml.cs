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
using Ecm.CustomControl;
using System.Data;
using Ecm.Domain;
using Ecm.Model;
using System.ComponentModel;
using Ecm.DocViewer.Converter;
using System.Windows.Controls.Primitives;
using System.IO;
using System.Xml;
using Ecm.AppHelper;
using Ecm.DocViewer.Helper;
using Ecm.DocViewer.Model;
using Ecm.DocViewer.Controls;
using Ecm.Model.DataProvider;

namespace Ecm.Audit.View
{
    /// <summary>
    /// Interaction logic for DeletedDocumentHistoryView.xaml
    /// </summary>
    public partial class DeletedDocumentHistoryView : UserControl
    {
        private DeletedDocumentHistoryViewModel _viewModel;
        private ActionLogProvider _actionLogProvider = new ActionLogProvider();
        private const string _folderName = "Search";

        public DeletedDocumentHistoryView()
        {
            InitializeComponent();
            Loaded += SearchViewLoaded;
        }
        private void SearchViewLoaded(object sender, RoutedEventArgs e)
        {
            try
            {
                _viewModel = DataContext as DeletedDocumentHistoryViewModel;
                if (_viewModel != null)
                {
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
                lblEmptyRow.Visibility = Visibility.Collapsed;
                lvDeletedDocument.Visibility = Visibility.Visible;
                if (_viewModel.SearchResults != null)
                {
                    if (_viewModel.SearchResults[0] == null)
                    {
                        lblEmptyRow.Visibility = Visibility.Visible;
                        lvDeletedDocument.Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        BuildResultView(_viewModel.SearchResults);
                    }
                }
                else
                {
                    var emptyRow = new TextBlock
                    {
                        Text = "No document match your criteria.",
                        Style = TryFindResource("EmptyTextRow") as Style
                    };
                }
            }
        }

        private void BuildResultView(IEnumerable<SearchResultModel> results)
        {
            foreach (var result in results)
            {
                if (result == null)
                {
                    return;
                }

                Grid gridHeader = BuildResultHeader(result);
                BuildResultData(result);
                var toggleHeaderButton = gridHeader.Children[0] as ToggleButton;
                if (toggleHeaderButton != null)
                {
                    toggleHeaderButton.Tag = deletedGridData;
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
            binding = new Binding("HasMoreResult") { NotifyOnTargetUpdated = true, Converter = new BoolVisibilityConverter() };
            grid.Children.Add(toggleButton);
            grid.Children.Add(header);
            Grid.SetColumn(header, 1);
            return grid;
        }

        private void BuildResultData(SearchResultModel result)
        {
            lvDeletedDocument.DataContext = result;
            // Create a binding with auto resize column widths of listview
            Binding.AddTargetUpdatedHandler(lvDeletedDocument, ListViewTargetUpdated);

            lvDeletedDocument.View = GenerateGridView(result);
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
        
        private void BuildColumns(SearchResultModel searchResult, GridView view)
        {
            DataTable table = searchResult.DataResult;
            foreach (DataColumn column in table.Columns)
            {
                if (column.ColumnName == Common.COLUMN_DOCUMENT || column.ColumnName == Common.COLUMN_CHECKED ||
                    column.ColumnName == Common.COLUMN_DOCUMENT_ID ||
                    column.ColumnName == Common.COLUMN_DOCUMENT_TYPE_ID ||
                    column.ColumnName == Common.COLUMN_BINARY_TYPE ||
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

        private void PopulateColumnStyle(SearchResultModel searchResult, SortableGridViewColumn viewColumn, DataColumn column)
        {
            if (column.ColumnName == Common.COLUMN_SELECTED || column.ColumnName == Common.COLUMN_CHECKED)
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

        private void lbxDeletedDocType_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if ((sender as ListBox).SelectedItem == null)
            {
                return;
            }

            try
            {
                _viewModel.SearchDocForDeletedDocType();
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
                    //HeaderSeparator.Visibility = Visibility.Collapsed;
                }
                else
                {
                    pnDocTypes.Visibility = Visibility.Visible;
                    pnLeftHeader.Visibility = Visibility.Visible;
                    pnLeft.Width = pnLeft.MaxWidth;
                    //HeaderSeparator.Visibility = Visibility.Visible;
                }
            }
            catch (Exception ex)
            {
                ProcessHelper.ProcessException(ex);
            }
        }
        private void DeletedResultItemMouseDoubleClick(object sender, MouseButtonEventArgs e)
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

        private void DeletedResultItemPreviewMouseDown(object sender, MouseButtonEventArgs e)
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

        private void lbxExistingDocType_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if ((sender as ListBox).SelectedItem == null)
            {
                return;
            }

            try
            {
                _viewModel.SearchDeletedDocumentFromExistingDocType();
            }
            catch (Exception ex)
            {
                ProcessHelper.ProcessException(ex);
            }
        }

        private void lbxDeletedDocType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            lbxExistingDocType.SelectedIndex = -1;
        }

        private void lbxExistingDocType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            lbxDeletedDocType.SelectedIndex = -1;
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
