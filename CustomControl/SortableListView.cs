using System;
using System.Linq;
using System.Windows.Controls;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using System.Reflection;
using System.Data;

namespace Ecm.CustomControl
{
    public class SortableListView : ListView
    {
        private ListSortDirection _lastDirection = ListSortDirection.Ascending;

        private SortableGridViewColumn _lastSortedOnColumn;
        
        public SortableListView()
        {
            AlternationCount = 2;
        }

        public event EventHandler EndSortingProcess;

        public string ColumnHeaderNotSortedTemplate
        {
            get
            {
                return (string)GetValue(ColumnHeaderNotSortedTemplateProperty);
            }
            set
            {
                SetValue(ColumnHeaderNotSortedTemplateProperty, value);
            }
        }

        public string ColumnHeaderSortedAscendingTemplate
        {
            get
            {
                return (string)GetValue(ColumnHeaderSortedAscendingTemplateProperty);
            }
            set
            {
                SetValue(ColumnHeaderSortedAscendingTemplateProperty, value);
            }
        }

        public string ColumnHeaderSortedDescendingTemplate
        {
            get
            {
                return (string)GetValue(ColumnHeaderSortedDescendingTemplateProperty);
            }
            set
            {
                SetValue(ColumnHeaderSortedDescendingTemplateProperty, value);
            }
        }

        public ListSortDirection LastDirection
        {
            get
            {
                return _lastDirection;
            }
        }

        public string SortPropertyName
        {
            get
            {
                return _lastSortedOnColumn == null ? string.Empty : _lastSortedOnColumn.SortPropertyName;
            }
        }

        public bool AutoGenerateColumns
        {
            get { return (bool)GetValue(AutoGenerateColumnsProperty); }
            set { SetValue(AutoGenerateColumnsProperty, value); }
        }

        // Call this method to refresh the listview if it is defined by desgining, not coding
        public void Sort()
        {
            var gridView = View as GridView;
            SortableGridViewColumn sortableGridViewColumn = null;

            if (gridView != null)
            {
                foreach (GridViewColumn gridViewColumn in gridView.Columns)
                {
                    var viewColumn = gridViewColumn as SortableGridViewColumn;

                    if (viewColumn != null)
                    {
                        if (viewColumn.IsDefaultSortColumn)
                        {
                            sortableGridViewColumn = viewColumn;
                        }
                        else if (!String.IsNullOrEmpty(viewColumn.SortPropertyName)
                                 && !String.IsNullOrEmpty(ColumnHeaderNotSortedTemplate))
                        {
                            viewColumn.HeaderTemplate = TryFindResource(ColumnHeaderNotSortedTemplate) as DataTemplate;
                        }
                    }
                }
            }

            if (sortableGridViewColumn != null)
            {
                Sort(sortableGridViewColumn.SortPropertyName, sortableGridViewColumn.Direction);

                if (sortableGridViewColumn.Direction == ListSortDirection.Ascending
                    && !String.IsNullOrEmpty(ColumnHeaderSortedAscendingTemplate))
                {
                    sortableGridViewColumn.HeaderTemplate =
                        TryFindResource(ColumnHeaderSortedAscendingTemplate) as DataTemplate;
                }
                else if (sortableGridViewColumn.Direction == ListSortDirection.Descending
                         && !String.IsNullOrEmpty(ColumnHeaderSortedDescendingTemplate))
                {
                    sortableGridViewColumn.HeaderTemplate =
                        TryFindResource(ColumnHeaderSortedDescendingTemplate) as DataTemplate;
                }

                _lastSortedOnColumn = sortableGridViewColumn;
            }
        }

        protected override void OnInitialized(EventArgs e)
        {
            //add the event handler to the GridViewColumnHeader. This strongly ties this ListView to a GridView.
            AddHandler(ButtonBase.ClickEvent, new RoutedEventHandler(GridViewColumnHeaderClickedHandler));

            var gridView = View as GridView;
            if (gridView != null)
            {
                // Check whether there is any column in grid view have sorting
                bool hasSort =
                    gridView.Columns.Any(
                        p =>
                        p is SortableGridViewColumn
                        && !string.IsNullOrEmpty((p as SortableGridViewColumn).SortPropertyName));

                if (!hasSort)
                {
                    base.OnInitialized(e);
                    return;
                }

                // determine which column is marked as IsDefaultSortColumn. Stops on the first column marked this way.
                SortableGridViewColumn sortableGridViewColumn = null;

                foreach (GridViewColumn gridViewColumn in gridView.Columns)
                {
                    var viewColumn = gridViewColumn as SortableGridViewColumn;

                    if (viewColumn != null)
                    {
                        if (viewColumn.IsDefaultSortColumn)
                        {
                            sortableGridViewColumn = viewColumn;
                        }
                        else if (!String.IsNullOrEmpty(viewColumn.SortPropertyName)
                                 && !String.IsNullOrEmpty(ColumnHeaderNotSortedTemplate))
                        {
                            viewColumn.HeaderTemplate =
                                TryFindResource(ColumnHeaderNotSortedTemplate) as DataTemplate;
                        }
                    }
                }

                // if the default sort column is defined, sort the data and then update the templates as necessary.
                if (sortableGridViewColumn != null)
                {
                    _lastSortedOnColumn = sortableGridViewColumn;
                    Sort(sortableGridViewColumn.SortPropertyName, sortableGridViewColumn.Direction);

                    if (sortableGridViewColumn.Direction == ListSortDirection.Ascending
                        && !String.IsNullOrEmpty(ColumnHeaderSortedAscendingTemplate))
                    {
                        sortableGridViewColumn.HeaderTemplate =
                            TryFindResource(ColumnHeaderSortedAscendingTemplate) as DataTemplate;
                    }
                    else if (sortableGridViewColumn.Direction == ListSortDirection.Descending
                             && !String.IsNullOrEmpty(ColumnHeaderSortedDescendingTemplate))
                    {
                        sortableGridViewColumn.HeaderTemplate =
                            TryFindResource(ColumnHeaderSortedDescendingTemplate) as DataTemplate;
                    }

                    if (EndSortingProcess != null)
                    {
                        EndSortingProcess(this, EventArgs.Empty);
                    }
                }
            }

            base.OnInitialized(e);
        }

        protected override void OnItemsSourceChanged(System.Collections.IEnumerable oldValue, System.Collections.IEnumerable newValue)
        {
            base.OnItemsSourceChanged(oldValue, newValue);

            if (AutoGenerateColumns)
            {
                GridView gridView = new GridView();

                Type sourceItemType = ItemsSource.AsQueryable().ElementType;
                PropertyInfo[] properties = sourceItemType.GetProperties();

                foreach (var prop in properties)
                {
                    GridViewColumn column = new GridViewColumn();

                    if (prop.PropertyType == typeof(BitmapImage))
                    {
                        DataTemplate template = new DataTemplate();

                        FrameworkElementFactory childFactory = new FrameworkElementFactory(typeof(System.Windows.Controls.Image));

                        childFactory.SetBinding(System.Windows.Controls.Image.SourceProperty, new Binding(prop.Name));
                        childFactory.SetValue(WidthProperty, 16.0);
                        template.VisualTree = childFactory;

                        column.CellTemplate = template;
                    }
                    else if (prop.PropertyType == typeof(DataRow))
                    {
                        column.Header = (prop.GetCustomAttributes(typeof(DataColumn), false).First() as DescriptionAttribute).Description;
                        column.DisplayMemberBinding = new Binding(prop.Name);
                        column.Width = double.NaN;
                    }
                    else
                    {
                        column.Header = (prop.GetCustomAttributes(typeof(DescriptionAttribute), false).First() as DescriptionAttribute).Description;
                        column.DisplayMemberBinding = new Binding(prop.Name);
                        column.Width = double.NaN;
                    }

                    gridView.Columns.Add(column);
                }

                View = gridView;
            }
        }

        private void GridViewColumnHeaderClickedHandler(object sender, RoutedEventArgs e)
        {
            var headerClicked = e.OriginalSource as GridViewColumnHeader;

            // ensure that we clicked on the column header and not the padding that's added to fill the space.
            if (headerClicked != null && headerClicked.Role != GridViewColumnHeaderRole.Padding)
            {
                var sortableGridViewColumn = (headerClicked.Column) as SortableGridViewColumn;

                // ensure that the column header is the correct type and a sort property has been set.
                if (sortableGridViewColumn != null && !String.IsNullOrEmpty(sortableGridViewColumn.SortPropertyName))
                {
                    ListSortDirection direction;
                    bool newSortColumn = false;

                    // determine if this is a new sort, or a switch in sort direction.
                    if (_lastSortedOnColumn == null)
                    {
                        newSortColumn = true;
                        direction = ListSortDirection.Ascending;
                    }
                    else if (
                        !String.Equals(
                            sortableGridViewColumn.SortPropertyName,
                            _lastSortedOnColumn.SortPropertyName,
                            StringComparison.InvariantCultureIgnoreCase))
                    {
                        newSortColumn = true;
                        direction = _lastDirection;
                    }
                    else
                    {
                        direction = _lastDirection == ListSortDirection.Ascending ? ListSortDirection.Descending : ListSortDirection.Ascending;
                    }

                    // get the sort property name from the column's information.
                    string sortPropertyName = sortableGridViewColumn.SortPropertyName;

                    // Sort the data.
                    Sort(sortPropertyName, direction);

                    if (direction == ListSortDirection.Ascending)
                    {
                        if (!String.IsNullOrEmpty(ColumnHeaderSortedAscendingTemplate))
                        {
                            sortableGridViewColumn.HeaderTemplate =
                                TryFindResource(ColumnHeaderSortedAscendingTemplate) as DataTemplate;
                        }
                        else
                        {
                            sortableGridViewColumn.HeaderTemplate = null;
                        }
                    }
                    else
                    {
                        if (!String.IsNullOrEmpty(ColumnHeaderSortedDescendingTemplate))
                        {
                            sortableGridViewColumn.HeaderTemplate =
                                TryFindResource(ColumnHeaderSortedDescendingTemplate) as DataTemplate;
                        }
                        else
                        {
                            sortableGridViewColumn.HeaderTemplate = null;
                        }
                    }

                    // Remove arrow from previously sorted header
                    if (newSortColumn && _lastSortedOnColumn != null)
                    {
                        if (!String.IsNullOrEmpty(ColumnHeaderNotSortedTemplate))
                        {
                            _lastSortedOnColumn.HeaderTemplate =
                                TryFindResource(ColumnHeaderNotSortedTemplate) as DataTemplate;
                        }
                        else
                        {
                            _lastSortedOnColumn.HeaderTemplate = null;
                        }
                    }
                    _lastSortedOnColumn = sortableGridViewColumn;

                    if (EndSortingProcess != null)
                    {
                        EndSortingProcess(this, EventArgs.Empty);
                    }
                }
            }
        }

        private void Sort(string sortBy, ListSortDirection direction)
        {
            _lastDirection = direction;
            ICollectionView dataView = CollectionViewSource.GetDefaultView(ItemsSource);
            dataView.SortDescriptions.Clear();
            var sd = new SortDescription(sortBy, direction);
            dataView.SortDescriptions.Add(sd);
            dataView.Refresh();
        }

        public static readonly DependencyProperty ColumnHeaderNotSortedTemplateProperty =
            DependencyProperty.Register(
                "ColumnHeaderNotSortedTemplate", typeof(string), typeof(SortableListView), new UIPropertyMetadata(""));

        public static readonly DependencyProperty AutoGenerateColumnsProperty =
            DependencyProperty.Register("AutoGenerateColumns", typeof(bool), typeof(SortableListView), new PropertyMetadata(false));

        public static readonly DependencyProperty ColumnHeaderSortedAscendingTemplateProperty =
            DependencyProperty.Register(
                "ColumnHeaderSortedAscendingTemplate",
                typeof(string),
                typeof(SortableListView),
                new UIPropertyMetadata(""));

        public static readonly DependencyProperty ColumnHeaderSortedDescendingTemplateProperty =
            DependencyProperty.Register(
                "ColumnHeaderSortedDescendingTemplate",
                typeof(string),
                typeof(SortableListView),
                new UIPropertyMetadata(""));
    }
}
