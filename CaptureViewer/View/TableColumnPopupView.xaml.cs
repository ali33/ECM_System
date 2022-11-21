using Ecm.CaptureDomain;
using Ecm.CaptureModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
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
using System.Xml;
using Ecm.CustomControl;

namespace Ecm.CaptureViewer
{
    /// <summary>
    /// Interaction logic for TableColumnPopupView.xaml
    /// </summary>
    public partial class TableColumnPopupView : UserControl
    {
        private FieldValueModel _parentField;
        private const string COLUMN_ID = "ID_A5F03635-59C6-436A-893E-23EAD21EB3B6";
        public static readonly DependencyProperty TableDataSourceProperty = DependencyProperty.Register("TableDataSource", typeof(DataTable), typeof(TableColumnPopupView));
        public static readonly DependencyProperty ParentFieldProperty = DependencyProperty.Register("ParentField", typeof(FieldValueModel), typeof(TableColumnPopupView));
        public static readonly DependencyProperty ViewModeProperty = DependencyProperty.Register("ViewMode", typeof(bool), typeof(TableColumnPopupView));

        public TableColumnPopupView()
        {
            try
            {
                InitializeComponent();
            }
            catch (Exception ex)
            {
            }
        }

        public DataTable TableDataSource
        {
            get { return GetValue(TableDataSourceProperty) as DataTable; }
            set { SetValue(TableDataSourceProperty, value); }
        }

        public FieldValueModel ParentField
        {
            get { return GetValue(ParentFieldProperty) as FieldValueModel; }
            set { SetValue(ParentFieldProperty, value); }
        }

        void TableColumnView_Loaded(object sender, RoutedEventArgs e)
        {
            if (_parentField == null)
            {
                ParentField = _parentField = DataContext as FieldValueModel;
            }

            BuildDataSource();

            if (_parentField.TableValues.Count > 0)
            {
                LoadData();
            }

            GenerateListView();
        }

        private void GenerateListView()
        {
            var listView = new ListView { SelectionMode = SelectionMode.Single };
            new ListViewLayoutManager(listView);
            // Create a binding with auto resize column widths of listview
            var binding = new Binding() { NotifyOnTargetUpdated = true };
            binding.Source = TableDataSource.DefaultView;
            BindingOperations.SetBinding(listView, ItemsControl.ItemsSourceProperty, binding);
            //Binding.AddTargetUpdatedHandler(listView, ListViewTargetUpdated);

            listView.BorderThickness = new Thickness(0);
            listView.HorizontalAlignment = HorizontalAlignment.Stretch;
            listView.Focusable = false;
            listView.SelectionMode = SelectionMode.Single;
            //listView.AddHandler(MouseWheelEvent, new RoutedEventHandler(UnHandleMouseWheelEvent), true);
            listView.ItemContainerStyle = TryFindResource("TableColumnStyle") as Style;
            listView.View = GenerateGridView();
            pnTableField.Children.Add(listView);
        }

        private void ListViewTargetUpdated(object sender, DataTransferEventArgs e)
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

        private void UpdateColumnWidths(GridView gridView)
        {
            // For each column...
            foreach (GridViewColumn column in gridView.Columns)
            {
                column.Width = 0;
                column.Width = double.NaN;
            }
        }

        private void UnHandleMouseWheelEvent(object sender, RoutedEventArgs e)
        {
            e.Handled = false;
        }

        private GridView GenerateGridView()
        {
            var view = new GridView();
            BuildColumns(view);
            return view;
        }

        private void BuildColumns(GridView view)
        {
            for (int i = 0; i < _parentField.Field.Children.Count; i++)
            {
                var field = _parentField.Field.Children[i];

                if (_parentField.DocPermission!=null && !_parentField.DocPermission.CanSeeRestrictedField && field.IsRestricted)
                {
                    continue;
                }

                var viewColumn = new GridViewColumn
                {
                    Header = field.ColumnName,
                    DisplayMemberBinding = new Binding(field.ColumnName)
                };

                if (i == _parentField.Field.Children.Count - 1)
                {
                    RangeColumn.ApplyWidth(viewColumn, 100, double.NaN, 200, true);
                }

                //PopulateColumnStyle(viewColumn, field);
                view.Columns.Add(viewColumn);
            }

        }

        private void PopulateColumnStyle(GridViewColumn viewColumn, TableColumnModel column)
        {
            switch (column.DataType)
            {
                case FieldDataType.String:
                    viewColumn.CellTemplate = GetStringCellTemplate(column);
                    break;
                case FieldDataType.Date:
                    viewColumn.CellTemplate = GetDateCellTemplate(column);
                    break;
                case FieldDataType.Decimal:
                    viewColumn.CellTemplate = GetDecimalCellTemplate(column);
                    break;
                case FieldDataType.Integer:
                    viewColumn.CellTemplate = GetIntegerCellTemplate(column);
                    break;
            }

            viewColumn.DisplayMemberBinding = null;
        }

        private void LoadData()
        {
            int rowCount = _parentField.TableValues.Max(p => p.RowNumber);

            for (int i = 0; i <= rowCount; i++)
            {
                List<TableFieldValueModel> tableValues = new List<TableFieldValueModel>();
                tableValues = _parentField.TableValues.Where(p => p.RowNumber == i).ToList();

                if (tableValues.Count == 0)
                {
                    continue;
                }

                DataRow row = TableDataSource.NewRow();

                foreach (TableFieldValueModel tableValue in tableValues)
                {
                    if (string.IsNullOrWhiteSpace(tableValue.Value))
                    {
                        row[tableValue.Field.Name] = DBNull.Value;
                    }
                    else
                    {
                        row[tableValue.Field.Name] = tableValue.Value;
                    }
                }

                TableDataSource.Rows.Add(row);
            }
        }

        private void BuildDataSource()
        {
            TableDataSource = new DataTable(_parentField.Field.Name);

            foreach (var field in _parentField.Field.Children)
            {
                TableDataSource.Columns.Add(GetColumn(field.ColumnName, field.DataType));
            }
        }

        private DataColumn GetColumn(string name, FieldDataType dataType)
        {
            Type type = typeof(string);
            switch (dataType)
            {
                case FieldDataType.String:
                    type = typeof(string);
                    break;
                case FieldDataType.Date:
                    type = typeof(DateTime);
                    break;
                case FieldDataType.Decimal:
                    type = typeof(decimal);
                    break;
                case FieldDataType.Integer:
                    type = typeof(int);
                    break;
            }

            return new DataColumn(name, type);
        }

        private DataTemplate GetStringCellTemplate(TableColumnModel column)
        {
            string template = @"<DataTemplate xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation' 
                                              xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'
                                              x:Key='StringValueTemplate'>
                                        <TextBlock Text='{Binding Path=<<value>>, Mode=TwoWay}' Height='25' 
                                                        HorizontalAlignment='Left' MinWidth='100'
                                                        x:Name='editorControl' Tag='{Binding Path=.}' 
                                                        />
                                </DataTemplate>";

            template = template.Replace("<<value>>", column.ColumnName);

            return BuildDataTemplate(template);
        }

        private DataTemplate GetDecimalCellTemplate(TableColumnModel column)
        {
            string template = @"<DataTemplate xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation' 
                                              xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'
                                              x:Key='DecimalValueTemplate'>
                                            <TextBlock Height='25' MinWidth='50'
                                                     Text='{Binding Path=<<value>>, Mode=TwoWay}' 
                                                     HorizontalAlignment='Right'
                                                     x:Name='editorControl'
                                                     Tag='{Binding Path=.}' 
                                                     />
                                </DataTemplate>";

            template = template.Replace("<<value>>", column.ColumnName);

            return BuildDataTemplate(template);
        }

        private DataTemplate GetIntegerCellTemplate(TableColumnModel column)
        {
            string template = @"<DataTemplate xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation' 
                                              xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'
                                              x:Key='IntegerValueTemplate'>
                                            <TextBlock Height='25' MinWidth='50'
                                                        Text='{Binding Path=<<value>>, Mode=TwoWay}' 
                                                        HorizontalAlignment='Right'
                                                        x:Name='editorControl'
                                                        Tag='{Binding Path=.}' 
                                                        />
                                </DataTemplate>";

            template = template.Replace("<<value>>", column.ColumnName);

            return BuildDataTemplate(template);
        }

        private DataTemplate GetDateCellTemplate(TableColumnModel column)
        {
            string template = @"<DataTemplate xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation' 
                                              xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml' xmlns:cv='clr-namespace:Ecm.CaptureViewer.Converter;assembly=CaptureViewer'
                                              x:Key='DateValueTemplate'>
                                            <DataTemplate.Resources>
                                                <cv:ShortDateTimeConverter x:Key='ShortDateTimeConverter' />
                                            </DataTemplate.Resources>
                                            <TextBlock Height='25' MinWidth='100'
                                                        HorizontalAlignment='Right' 
                                                        Padding='2'  
                                                        VerticalAlignment='Center' 
                                                        Text='{Binding Path=<<value>>, Converter={StaticResource ShortDateTimeConverter}, Mode=TwoWay}' 
                                                        x:Name='editorControl' Tag='{Binding Path=.}'/>
                                </DataTemplate>";

            template = template.Replace("<<value>>", column.ColumnName);

            return BuildDataTemplate(template);
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
    }
}
