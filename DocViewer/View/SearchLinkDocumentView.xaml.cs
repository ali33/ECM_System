using Ecm.CustomControl;
using Ecm.DocViewer.Converter;
using Ecm.DocViewer.ViewModel;
using Ecm.Domain;
using Ecm.Model;
using Ecm.Model.DataProvider;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml;

namespace Ecm.DocViewer
{
    /// <summary>
    /// Interaction logic for SearchLinkDocumentView.xaml
    /// </summary>
    public partial class SearchLinkDocumentView
    {
        private SearchProvider _searchProvider = new SearchProvider();
        private ObservableCollection<SearchResultModel> _searchResults = new ObservableCollection<SearchResultModel>();
        private DocumentTypeProvider _docTypeProvider = new DocumentTypeProvider();
        public DialogViewer Dialog { get; set; }

        //public RoutedCommand CancelLinkCommand;
        //public RoutedCommand AddLinkCommand;

        public RoutedCommand AddLinkCommand
        {
            get { return (RoutedCommand)GetValue(AddLinkCommandProperty); }
            set { SetValue(AddLinkCommandProperty, value); }
        }
        public RoutedCommand CancelLinkCommand
        {
            get { return (RoutedCommand)GetValue(CancelLinkCommandProperty); }
            set { SetValue(CancelLinkCommandProperty, value); }
        }

        public static readonly DependencyProperty CancelLinkCommandProperty =
            DependencyProperty.Register("CancelLinkCommand", typeof(RoutedCommand), typeof(SearchLinkDocumentView), new UIPropertyMetadata(null));

        public static readonly DependencyProperty AddLinkCommandProperty =
            DependencyProperty.Register("AddLinkCommand", typeof(RoutedCommand), typeof(SearchLinkDocumentView), new UIPropertyMetadata(null));

        public ViewerContainer ViewerContainer { get; set; }
        public static readonly DependencyProperty SearchResultsProperty =
           DependencyProperty.Register("SearchResults", typeof(ObservableCollection<SearchResultModel>), typeof(SearchLinkDocumentView),
               new FrameworkPropertyMetadata(new ObservableCollection<SearchResultModel>()));

        public static readonly DependencyProperty IsSearchEnabledProperty =
           DependencyProperty.Register("IsSearchEnabled", typeof(bool), typeof(SearchLinkDocumentView));

        public static readonly DependencyProperty DocTypesProperty =
           DependencyProperty.Register("DocTypes", typeof(ObservableCollection<DocumentTypeModel>), typeof(SearchLinkDocumentView));

        public static readonly DependencyProperty DocumentTypeProperty =
           DependencyProperty.Register("DocumentType", typeof(DocumentTypeModel), typeof(SearchLinkDocumentView));

        public static readonly DependencyProperty AvailableFieldsProperty =
           DependencyProperty.Register("AvailableFields", typeof(ObservableCollection<FieldMetaDataModel>), typeof(SearchLinkDocumentView));

        public static readonly DependencyProperty SearchQueryExpressionsProperty =
           DependencyProperty.Register("SearchQueryExpressions", typeof(ObservableCollection<SearchExpressionViewModel>), typeof(SearchLinkDocumentView));

        public static readonly DependencyProperty DocumentProperty =
           DependencyProperty.Register("Document", typeof(DocumentModel), typeof(SearchLinkDocumentView));

        public static readonly DependencyProperty SearchResultProperty =
            DependencyProperty.Register("SearchResult", typeof(SearchResultModel), typeof(SearchLinkDocumentView));

        public SearchResultModel SearchResult
        {
            get { return GetValue(SearchResultProperty) as SearchResultModel; }
            set
            {
                SetValue(SearchResultProperty, value);
            }
        }

        public ObservableCollection<SearchResultModel> SearchResults
        {
            get { return GetValue(SearchResultsProperty) as ObservableCollection<SearchResultModel>; }
            set { SetValue(SearchResultsProperty, value);
                if (value != null)
                {
                    value.CollectionChanged -= SearchResultsCollectionChanged;
                    value.CollectionChanged += SearchResultsCollectionChanged;
                }
            }
        }

        public ObservableCollection<SearchExpressionViewModel> SearchQueryExpressions
        {
            get { return GetValue(SearchQueryExpressionsProperty) as ObservableCollection<SearchExpressionViewModel>; }
            set { SetValue(SearchQueryExpressionsProperty, value);}
        }

        public bool IsSearchEnabled
        {
            get { return (bool)GetValue(IsSearchEnabledProperty); }
            set { SetValue(IsSearchEnabledProperty, value); }
        }

        public ObservableCollection<DocumentTypeModel> DocTypes
        {
            get { return GetValue(DocTypesProperty) as ObservableCollection<DocumentTypeModel>; }
            set { SetValue(DocTypesProperty, value); }
        }

        public ObservableCollection<FieldMetaDataModel> AvailableFields
        {
            get { return GetValue(AvailableFieldsProperty) as ObservableCollection<FieldMetaDataModel>; }
            set { SetValue(AvailableFieldsProperty, value); }
        }

        public DocumentTypeModel DocumentType
        {
            get { return (DocumentTypeModel)GetValue(DocumentTypeProperty); }
            set { SetValue(DocumentTypeProperty, value);
                if(value != null)
                {
                    LoadDefaultSearchExpression();
                    LoadAvailableFields();
                    RunSearch();
                }
            }
        }

        public DocumentModel Document
        {
            get { return (DocumentModel)GetValue(DocumentProperty); }
            set { SetValue(DocumentProperty, value); }
        }

        public SearchLinkDocumentView()
        {
            AddLinkCommand = new RoutedCommand("AddLinkCommand", typeof(SearchLinkDocumentView)); //new RoutedCommand("",);
            var commandBinding = new CommandBinding(AddLinkCommand, AddLink, CanAddLink);
            this.CommandBindings.Add(commandBinding);

            CancelLinkCommand = new RoutedCommand("CancelLinkCommand", typeof(SearchLinkDocumentView)); //new RoutedCommand("",);
            commandBinding = new CommandBinding(CancelLinkCommand, CancelLink);
            this.CommandBindings.Add(commandBinding);

            InitializeComponent();
            Loaded += SearchViewLoaded;
        }

        private void CancelLink(object sender, ExecutedRoutedEventArgs e)
        {
            Dialog.Close();
        }

        private void CanAddLink(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void AddLink(object sender, ExecutedRoutedEventArgs e)
        {
            foreach(DataRow row in SearchResult.DataResult.Rows)
            {
                if (Convert.ToBoolean(row[Common.COLUMN_CHECKED].ToString()))
                {
                    Guid linkDocId = new Guid(row[Common.COLUMN_DOCUMENT_ID].ToString());
                    DocumentModel document = ViewerContainer.Items[0].BatchItem.Children.SingleOrDefault(p => p.DocumentData.Id == ViewerContainer.SelectedDocument.Id).DocumentData;
                    if (document != null)
                    {
                        LinkDocumentModel linkDoc = new LinkDocumentModel
                        {
                            DocumentId = ViewerContainer.SelectedDocument.Id,
                            LinkedDocumentId = linkDocId, LinkedDocument = new DocumentProvider().GetDocument(linkDocId)
                        };

                        document.LinkDocuments.Add(linkDoc);
                        ViewerContainer.LinkDocViewer.LinkDocuments.Add(linkDoc);
                    }
                }
            }

            Dialog.Close();
            //ViewerContainer.SelectedDocument.LinkDocuments.Add(new LinkDocumentModel());
        }

        private const string _folderName = "Search";
        private readonly ActionLogProvider _actionLogProvider = new ActionLogProvider();
        private readonly ResourceManager _resource = new ResourceManager("Ecm.DocViewer.ViewerContainer", Assembly.GetExecutingAssembly());

        public void ToggleSearchConditionArea(bool show)
        {
            if (show)
            {
                SearchPanel.Visibility = Visibility.Visible;
                SearchButtonPanel.Visibility = Visibility.Visible;
                AdvanceSearchTitle.Visibility = Visibility.Collapsed;
            }
            else
            {
                SearchPanel.Visibility = Visibility.Collapsed;
                SearchButtonPanel.Visibility = Visibility.Collapsed;
                AdvanceSearchTitle.Visibility = Visibility.Visible;
            }
        }

        private void SearchViewLoaded(object sender, RoutedEventArgs e)
        {
            SearchQueryExpressions = new ObservableCollection<SearchExpressionViewModel>();
            AvailableFields = new ObservableCollection<FieldMetaDataModel>();

            ToggleSearchConditionArea(false);
            LoadDocumentTypes();
            DocumentType = ViewerContainer.SelectedDocumentType;
        }

        private void LoadDocumentTypes()
        {
            try
            {
                DocTypes = _docTypeProvider.GetDocumentTypes();
            }
            catch
            {
                throw;
            }
        }

        private void RunSearch()
        {
            ViewerContainer.IsProcessing = true;
            //BackgroundWorker loadData = new BackgroundWorker();

            //loadData.DoWork += LoadData_DoWork;
            //loadData.RunWorkerCompleted += LoadData_RunWorkerCompleted;
            //loadData.RunWorkerAsync(this);
            try
            {
                SearchResult = _searchProvider.RunAdvanceSearch(0, DocumentType.Id, new SearchQueryModel { SearchQueryExpressions = GetSearchExpressions() });

                if (ViewerContainer.SelectedDocument != null && ViewerContainer.SelectedDocument.Id != Guid.Empty)
                {
                    DataTable newDataTable = SearchResult.DataResult.AsEnumerable()
                            .Where(r => ViewerContainer.SelectedDocument.Id != (r.Field<Guid>(Common.COLUMN_DOCUMENT_ID)) && !ViewerContainer.SelectedDocument.LinkDocuments.Select(p=>p.LinkedDocumentId).Contains(r.Field<Guid>(Common.COLUMN_DOCUMENT_ID)))
                            .CopyToDataTable();
                    SearchResult.DataResult.Clear();
                    SearchResult.DataResult = newDataTable;
                }

                if (SearchResult != null)
                {
                    BuildResultView(new ObservableCollection<SearchResultModel> { SearchResult });
                    //e.Result = SearchResults;

                }
                else
                {
                    var emptyRow = new TextBlock
                    {
                        Text = _resource.GetString("uiNoDocumentMatch"),
                        Style = TryFindResource("EmptyTextRow") as Style
                    };

                    pnResult.Children.Add(emptyRow);
                    //e.Result = null;
                }
            }
            catch (Exception ex)
            {
                //e.Result = ex;
            }

            ViewerContainer.IsProcessing = false;
        }

        private void LoadData_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            ViewerContainer.IsProcessing = false;
            IsSearchEnabled = true;

            if (!(e.Result is Exception))
            {
                try
                {
                    SearchResults = e.Result as ObservableCollection<SearchResultModel>;
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }

        private void LoadData_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                SearchResultModel searchResult = _searchProvider.RunAdvanceSearch(0, DocumentType.Id, new SearchQueryModel { SearchQueryExpressions = GetSearchExpressions() });

                if (SearchResults != null && SearchResults.Count > 0)
                {
                    BuildResultView(SearchResults);
                    e.Result = SearchResults;

                }
                else
                {
                    var emptyRow = new TextBlock
                    {
                        Text = _resource.GetString("uiNoDocumentMatch"),
                        Style = TryFindResource("EmptyTextRow") as Style
                    };

                    pnResult.Children.Add(emptyRow);
                    e.Result = null;
                }
            }
            catch (Exception ex)
            {
                e.Result = ex;
            }

        }

        private void LoadDefaultSearchExpression()
        {
            SearchQueryExpressions.Clear();

            int count = DocumentType.Fields.Count;
            for (int i = 0; i < count; i++)
            {
                if (!DocumentType.Fields[i].IsSystemField && DocumentType.Fields[i].DataType != FieldDataType.Table)
                {
                    var searchExpressionViewModel = new SearchExpressionViewModel();
                    var expression = new SearchQueryExpressionModel
                    {
                        Condition = i == 0 ? SearchConjunction.None : SearchConjunction.And,
                        Field = DocumentType.Fields[i]
                    };

                    searchExpressionViewModel.SearchQueryExpression = expression;
                    SearchQueryExpressions.Add(searchExpressionViewModel);
                }
            }
        }

        private void LoadAvailableFields()
        {
            AvailableFields.Clear();
            foreach (var field in DocumentType.Fields.Where(p => !p.IsSystemField && p.DataType != FieldDataType.Table))
            {
                AvailableFields.Add(field);
            }

            FieldMetaDataModel fieldModel = new FieldMetaDataModel
            {
                Id = Guid.Empty,
                Name = "Created by",
                DataType = FieldDataType.String,
                FieldUniqueId = Common.DOCUMENT_CREATED_BY
            };

            AvailableFields.Add(fieldModel);

            fieldModel = new FieldMetaDataModel
            {
                Id = Guid.Empty,
                Name = "Created date",
                DataType = FieldDataType.Date,
                FieldUniqueId = Common.DOCUMENT_CREATED_DATE
            };

            AvailableFields.Add(fieldModel);

            fieldModel = new FieldMetaDataModel
            {
                Id = Guid.Empty,
                Name = "Document Id",
                DataType = FieldDataType.String,
                FieldUniqueId = Common.DOCUMENT_ID
            };

            AvailableFields.Add(fieldModel);

            fieldModel = new FieldMetaDataModel
            {
                Id = Guid.Empty,
                Name = "Modified by",
                DataType = FieldDataType.String,
                FieldUniqueId = Common.DOCUMENT_MODIFIED_BY
            };

            AvailableFields.Add(fieldModel);

            fieldModel = new FieldMetaDataModel
            {
                Id = Guid.Empty,
                Name = "Modified date",
                DataType = FieldDataType.Date,
                FieldUniqueId = Common.DOCUMENT_MODIFIED_DATE
            };

            AvailableFields.Add(fieldModel);

            fieldModel = new FieldMetaDataModel
            {
                Id = Guid.Empty,
                Name = "Page count",
                DataType = FieldDataType.Integer,
                FieldUniqueId = Common.DOCUMENT_PAGE_COUNT
            };

            AvailableFields.Add(fieldModel);

            fieldModel = new FieldMetaDataModel
            {
                Id = Guid.Empty,
                Name = "Binary type",
                DataType = FieldDataType.String,
                FieldUniqueId = Common.DOCUMENT_FILE_BINARY
            };

            AvailableFields.Add(fieldModel);

            fieldModel = new FieldMetaDataModel
            {
                Id = Guid.Empty,
                Name = "Version",
                DataType = FieldDataType.Integer,
                FieldUniqueId = Common.DOCUMENT_VERSION
            };

            AvailableFields.Add(fieldModel);

        }

        private ObservableCollection<SearchQueryExpressionModel> GetSearchExpressions()
        {
            var expressions = new ObservableCollection<SearchQueryExpressionModel>();
            var valuedExpressions = SearchQueryExpressions.Where(p => !string.IsNullOrEmpty(p.SearchQueryExpression.Value1) &&
                                                                       (p.SearchQueryExpression.Operator != Domain.SearchOperator.InBetween ||
                                                                        !string.IsNullOrEmpty(p.SearchQueryExpression.Value2)));
            foreach (var item in valuedExpressions)
            {
                var searchQueryExpression = new SearchQueryExpressionModel
                {
                    Condition = item.SearchQueryExpression.Condition,
                    Field = item.SearchQueryExpression.Field,
                    Id = item.SearchQueryExpression.Id,
                    Operator = item.SearchQueryExpression.Operator,
                    OperatorText = item.SearchQueryExpression.OperatorText,
                    SearchQueryId = item.SearchQueryExpression.SearchQueryId,
                    Value1 = item.SearchQueryExpression.Value1,
                    Value2 = item.SearchQueryExpression.Value2,
                    FieldUniqueId = item.SearchQueryExpression.Field.Id != Guid.Empty ?
                                            item.SearchQueryExpression.Field.Id.ToString() :
                                            item.SearchQueryExpression.Field.FieldUniqueId

                };

                expressions.Add(searchQueryExpression);
            }

            if (expressions.Count > 0)
            {
                expressions[0].Condition = Domain.SearchConjunction.None;
            }

            return expressions;
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

            foreach (var result in results)
            {
                BuildViewForEachResult(result);
            }
        }

        private void BuildViewForEachResult(SearchResultModel result)
        {
            //Grid gridHeader = BuildResultHeader(result);
            //pnResult.Children.Add(gridHeader);
            Grid gridData = BuildResultData(result);
            pnResult.Children.Add(gridData);
            pnResult.Children.Add(new Grid { Height = 20 });

            //var toggleHeaderButton = gridHeader.Children[0] as ToggleButton;
            //if (toggleHeaderButton != null)
            //{
            //    toggleHeaderButton.Tag = gridData;
            //    toggleHeaderButton.Click += ToogleHeaderButtonClick;
            //}
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
                    column.ColumnName == Common.COLUMN_SELECTED ||
                    column.ColumnName == Common.COLUMN_DOCUMENT_VERSION_ID// ||column.ColumnName == Common.COLUMN_CHECKED
                    )
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
            if (column.ColumnName == Common.COLUMN_SELECTED)
            {
                return;
            }

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
                throw ex;
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
            catch (Exception ex)
            {
                throw ex; 
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
                throw ex;
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
                throw ex;
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
                throw ex;
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
                }
            }
            catch (Exception ex)
            {
                throw ex;
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
                throw ex;
            }
        }

        //private void BtnExpandLeftClick(object sender, RoutedEventArgs e)
        //{
        //    try
        //    {
        //        if (btnExpandLeft.IsChecked.Value)
        //        {
        //            pnDocTypes.Visibility = Visibility.Collapsed;
        //            pnLeftHeader.Visibility = Visibility.Collapsed;
        //            pnLeft.Width = 22;
        //            //HeaderSeparator.Visibility = Visibility.Collapsed;
        //        }
        //        else
        //        {
        //            pnDocTypes.Visibility = Visibility.Visible;
        //            pnLeftHeader.Visibility = Visibility.Visible;
        //            pnLeft.Width = pnLeft.MaxWidth;
        //            //HeaderSeparator.Visibility = Visibility.Visible;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        ProcessHelper.ProcessException(ex);
        //    }
        //}

        //private void PnlMainSearchSizeChanged(object sender, SizeChangedEventArgs e)
        //{
        //    try
        //    {
        //        if (pnMainSearch.ActualHeight <= 35)
        //        {
        //            SearchPanel.Visibility = Visibility.Collapsed;
        //            SearchButtonPanel.Visibility = Visibility.Collapsed;
        //            btnExpandSearch.IsChecked = true;
        //        }
        //        else
        //        {
        //            SearchPanel.Visibility = Visibility.Visible;
        //            SearchButtonPanel.Visibility = Visibility.Visible;
        //            btnExpandSearch.IsChecked = false;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        ProcessHelper.ProcessException(ex);
        //    }
        //}

        //private void PnlLeftSizeChanged(object sender, SizeChangedEventArgs e)
        //{
        //    try
        //    {
        //        if (pnLeft.ActualWidth <= 22)
        //        {
        //            pnLeftHeader.Visibility = Visibility.Collapsed;
        //            pnDocTypes.Visibility = Visibility.Collapsed;
        //            pnLeft.Width = 22;
        //            btnExpandLeft.IsChecked = true;
        //            //HeaderSeparator.Visibility = Visibility.Collapsed;
        //        }
        //        else
        //        {
        //            pnLeftHeader.Visibility = Visibility.Visible;
        //            pnDocTypes.Visibility = Visibility.Visible;
        //            btnExpandLeft.IsChecked = false;
        //            //HeaderSeparator.Visibility = Visibility.Visible;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        ProcessHelper.ProcessException(ex);
        //    }
        //}
    }
}
