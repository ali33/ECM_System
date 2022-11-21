using Ecm.CustomControl;
using Ecm.CaptureDomain;
using Ecm.CaptureModel;
using Ecm.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using System.Globalization;
using System.ComponentModel;

namespace Ecm.CaptureViewer
{
    /// <summary>
    /// Interaction logic for TableColumnView.xaml
    /// </summary>
    public partial class TableColumnView : UserControl
    {
        private FieldValueModel _parentField;
        private const string COLUMN_ID = "ID_A5F03635-59C6-436A-893E-23EAD21EB3B6";
        public static readonly DependencyProperty TableDataSourceProperty = DependencyProperty.Register("TableDataSource", typeof(DataTable), typeof(TableColumnView));
        public static readonly DependencyProperty ParentFieldProperty = DependencyProperty.Register("ParentField", typeof(FieldValueModel), typeof(TableColumnView));
        public static readonly DependencyProperty ViewModeProperty = DependencyProperty.Register("ViewMode", typeof(bool), typeof(TableColumnView));
        private RelayCommand _addRowCommand;
        private RelayCommand _closeCommand;
        private RelayCommand _saveCommand;
        private RelayCommand _removeRowCommand;

        private List<Guid> _deleteValueGuids = new List<Guid>();
        private bool _isSaving = false;

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

        public bool ViewMode
        {
            get { return (bool)GetValue(ViewModeProperty); }
            set { SetValue(ViewModeProperty, value); }
        }

        public ICommand AddRowCommand
        {
            get
            {
                if (_addRowCommand == null)
                {
                    _addRowCommand = new RelayCommand(p => AddRow());
                }
                return _addRowCommand;
            }
        }

        public ICommand RemoveRowCommand
        {
            get
            {
                if (_removeRowCommand == null)
                {
                    _removeRowCommand = new RelayCommand(p => RemoveRow(p));
                }
                return _removeRowCommand;
            }
        }

        public ICommand CloseCommand
        {
            get
            {
                if (_closeCommand == null)
                {
                    _closeCommand = new RelayCommand(p => Close());
                }
                return _closeCommand;
            }
        }

        public ICommand SaveCommand
        {
            get
            {
                if (_saveCommand == null)
                {
                    _saveCommand = new RelayCommand(p => Save(), p => CanSave());
                }
                return _saveCommand;
            }
        }

        public TableColumnView(FieldValueModel parentField)
        {
            InitializeComponent();
            DataContext = _parentField = parentField;
            ViewMode = true;
        }

        public TableColumnView()
        {
            InitializeComponent();
            ViewMode = false;
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
            else
            {
                CreateBlankRow();
            }

            GenerateListView();
        }

        public DialogViewer Dialog { get; set; }

        private void GenerateListView()
        {
            CollectionViewSource viewSources = new CollectionViewSource();
            viewSources.Source = TableDataSource.DefaultView;
            viewSources.GroupDescriptions.Add(new PropertyGroupDescription("FakeGroup"));

            var listView = new ListView { SelectionMode = SelectionMode.Single };

            // Create a binding with auto resize column widths of listview
            var binding = new Binding() { NotifyOnTargetUpdated = true };
            binding.Source = viewSources;
            BindingOperations.SetBinding(listView, ItemsControl.ItemsSourceProperty, binding);
            Binding.AddTargetUpdatedHandler(listView, ListViewTargetUpdated);

            listView.BorderThickness = new Thickness(0);
            listView.HorizontalAlignment = HorizontalAlignment.Stretch;
            listView.Focusable = false;
            listView.SelectionMode = SelectionMode.Single;
            listView.AddHandler(MouseWheelEvent, new RoutedEventHandler(UnHandleMouseWheelEvent), true);
            listView.ItemContainerStyle = TryFindResource("TableColumnStyle") as Style;
            listView.GroupStyle.Add(new GroupStyle() { ContainerStyle = TryFindResource("TableColumnGroup") as Style });
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
                if (column.Width == 0)
                {
                    continue;
                }
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
            var removeColummn = new GridViewColumn();
            removeColummn.CellTemplate = TryFindResource("RemoveRowCellTemplate") as DataTemplate;
            view.Columns.Add(removeColummn);

            foreach (var field in _parentField.Field.Children)
            {
                var viewColumn = new GridViewColumn();
                if (field.IsRequired)
                {
                    viewColumn.HeaderTemplate = GetRequiredCellHeaderTemplate(field);
                }
                else
                {
                    viewColumn.Header = field.ColumnName;
                }

                PopulateColumnStyle(viewColumn, field);
                view.Columns.Add(viewColumn);

                if (_parentField.DocPermission!= null && !_parentField.DocPermission.CanSeeRestrictedField && field.IsRestricted)
                {
                    viewColumn.Width = 0;
                }
                else
                {
                    viewColumn.Width = double.NaN;
                }
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
            int rowCount = _parentField.TableValues.Select(p => p.RowNumber).Distinct().Count();

            for (int i = 0; i < rowCount; i++)
            {
                List<TableFieldValueModel> tableValues = new List<TableFieldValueModel>();
                tableValues = _parentField.TableValues.Where(p => p.RowNumber == i).ToList();
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

                    row[tableValue.Field.Name + COLUMN_ID] = tableValue.Id;
                }

                TableDataSource.Rows.Add(row);
            }
        }

        private void CreateBlankRow()
        {
            DataRow row = TableDataSource.NewRow();

            foreach (DataColumn column in TableDataSource.Columns)
            {


                //System.Type type = column.DataType;
                //row[column] = DBNull.Value;



                //switch (Type.GetTypeCode(type))
                //{
                //    case TypeCode.String:
                //        row[column] = string.Empty;
                //        break;
                //    case TypeCode.Decimal:
                //    case TypeCode.Int32:
                //        row[column] = string.Empty;
                //        break;
                //    case TypeCode.DateTime:
                //        row[column] = DateTime.Today.ToString("d", CultureInfo.InvariantCulture);
                //        break;
                //}
            }

            TableDataSource.Rows.Add(row);
        }

        private void BuildDataSource()
        {
            TableDataSource = new DataTable(_parentField.Field.Name);

            foreach (var field in _parentField.Field.Children)
            {
                TableDataSource.Columns.Add(field.ColumnName + COLUMN_ID, typeof(string));
                TableDataSource.Columns.Add(GetColumn(field));
            }
        }

        private void AddRow()
        {
            CreateBlankRow();
        }

        private void RemoveRow(object commandArg)
        {
            DataRowView rowView = (commandArg as Button).DataContext as DataRowView;
            DataRow row = rowView.Row;

            foreach (DataColumn column in rowView.DataView.Table.Columns)
            {
                if (column.ColumnName.Contains(COLUMN_ID))
                {
                    var id = row[column.ColumnName] == DBNull.Value ? Guid.Empty : Guid.Parse(row[column.ColumnName].ToString());
                    if (id != Guid.Empty)
                    {
                        _deleteValueGuids.Add(id);
                    }
                }
            }

            TableDataSource.Rows.Remove(row);
        }

        private void Close()
        {
            Dialog.Close();
        }

        private void Save()
        {
            if (_isSaving)
            {
                return;
            }
            _isSaving = true;

            RemoveBlankRows();

            int rowIndex = 0;
            foreach (DataRow row in TableDataSource.Rows)
            {
                foreach (DataColumn column in TableDataSource.Columns)
                {
                    if (column.ColumnName.Contains(COLUMN_ID))
                    {
                        continue;
                    }

                    //Guid guid = row[column.ColumnName + COLUMN_ID] != null ? ((Guid)row[column.ColumnName + COLUMN_ID]) : Guid.Empty;
                    var fieldValue = new TableFieldValueModel();
                    fieldValue.Field = _parentField.Field.Children.SingleOrDefault(p => p.ColumnName == column.ColumnName).Field;
                    fieldValue.RowNumber = rowIndex;
                    if (row[column] != DBNull.Value)
                    {
                        if (column.DataType == typeof(DateTime))
                        {
                            fieldValue.Value = ((DateTime)row[column]).ToString("yyyy-MM-dd");
                        }
                        else
                        {
                            fieldValue.Value = row[column].ToString();
                        }
                    }

                    fieldValue.Id = row[column.ColumnName + COLUMN_ID] == DBNull.Value ? Guid.Empty : Guid.Parse(row[column.ColumnName + COLUMN_ID].ToString());

                    if (fieldValue.Id == Guid.Empty)
                    {
                        fieldValue.Id = Guid.NewGuid();
                        fieldValue.IsNew = true;
                        _parentField.TableValues.Add(fieldValue);
                    }
                    else
                    {
                        TableFieldValueModel editFieldValue = _parentField.TableValues.SingleOrDefault(p => p.Id == fieldValue.Id);
                        editFieldValue.Field = fieldValue.Field;
                        editFieldValue.RowNumber = fieldValue.RowNumber;
                        editFieldValue.Value = fieldValue.Value;
                    }
                }

                rowIndex++;
            }

            _parentField.TableValues = new ObservableCollection<TableFieldValueModel>(_parentField.TableValues.Where(h => !_deleteValueGuids.Contains(h.Id)));

            Dialog.Close();
            _isSaving = false;
        }

        private bool CanSave()
        {
            if (_isSaving)
            {
                return false;
            }

            return ValidateData();
        }


        private DataColumn GetColumn(TableColumnModel model)
        {
            Type type = typeof(string);
            DataColumn column;
            switch (model.DataType)
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

            column = new DataColumn(model.ColumnName, type);

            switch (model.DataType)
            {
                case FieldDataType.String:
                    if (!string.IsNullOrWhiteSpace(model.DefaultValue))
                    {
                        column.DefaultValue = model.DefaultValue.Trim();
                    }
                    break;

                case FieldDataType.Date:
                    if (model.UseCurrentDate)
                    {
                        column.DefaultValue = DateTime.Now.Date;
                    }
                    break;
                case FieldDataType.Decimal:
                    decimal tempDec;
                    if (decimal.TryParse(string.Format("{0}", model.DefaultValue).Trim(), out tempDec))
                    {
                        column.DefaultValue = tempDec;
                    }
                    break;

                case FieldDataType.Integer:
                    int tempInt;
                    if (int.TryParse(string.Format("{0}", model.DefaultValue).Trim(), out tempInt))
                    {
                        column.DefaultValue = tempInt;
                    }

                    break;
            }

            return column;
        }

        private DataTemplate GetStringCellTemplate(TableColumnModel column)
        {
            string template = @"<DataTemplate xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation' 
                                              xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml' xmlns:cc='clr-namespace:Ecm.CustomControl;assembly=CustomControl'
                                              x:Key='StringValueTemplate'>
                                        <TextBox Text='{Binding Path=<<value>>, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged, ValidatesOnDataErrors=True}' Height='25' 
                                                        IsTabStop='True' HorizontalAlignment='Stretch' MinWidth='100'
                                                        MaxLength='{Binding Path=Field.MaxLength}'
                                                        cc:SelectTextOnFocus.Active='True' x:Name='editorControl'
                                                        Style='{DynamicResource TextboxFocusIndexField}' Tag='{Binding Path=.}' 
                                                        />
                                </DataTemplate>";

            template = template.Replace("<<value>>", column.ColumnName);

            return BuildDataTemplate(template);
        }

        private DataTemplate GetDecimalCellTemplate(TableColumnModel column)
        {
            string template = @"<DataTemplate xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation' 
                                              xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'  xmlns:cc='clr-namespace:Ecm.CustomControl;assembly=CustomControl'
                                               xmlns:cv='clr-namespace:Ecm.CaptureViewer.Converter;assembly=CaptureViewer'
                                              x:Key='DecimalValueTemplate'>
                                            <DataTemplate.Resources>
                                                <cv:IndexFieldDecimalConverter x:Key='IndexFieldDecimalConverter' />
                                            </DataTemplate.Resources>
                                            <TextBox cc:NumericTextBoxBehavior.Mask='Decimal' 
                                                    MaxLength='15' Height='25' MinWidth='50'
                                                    Text='{Binding Path=<<value>>, Mode=TwoWay, Converter={StaticResource IndexFieldDecimalConverter}, UpdateSourceTrigger=PropertyChanged, ValidatesOnDataErrors=True}' 
                                                    IsTabStop='True' HorizontalAlignment='Stretch'
                                                    cc:SelectTextOnFocus.Active='True' x:Name='editorControl'
                                                    Style='{DynamicResource TextboxFocusIndexField}' Tag='{Binding Path=.}'/>
                                </DataTemplate>";

            template = template.Replace("<<value>>", column.ColumnName);

            return BuildDataTemplate(template);
        }

        private DataTemplate GetIntegerCellTemplate(TableColumnModel column)
        {
            string template = @"<DataTemplate xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation' 
                                              xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'  xmlns:cc='clr-namespace:Ecm.CustomControl;assembly=CustomControl'
                                              x:Key='IntegerValueTemplate'>
                                            <TextBox cc:NumericTextBoxBehavior.Mask='Integer' Height='25' MinWidth='50'
                                                             cc:NumericTextBoxBehavior.MinimumValue='-2147483648'
                                                             cc:NumericTextBoxBehavior.MaximumValue='2147483647'
                                                             Text='{Binding Path=<<value>>, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged, ValidatesOnDataErrors=True}' 
                                                             IsTabStop='True' HorizontalAlignment='Stretch'
                                                             cc:SelectTextOnFocus.Active='True' x:Name='editorControl'
                                                             Style='{DynamicResource TextboxFocusIndexField}' Tag='{Binding Path=.}' 
                                                             />
                                </DataTemplate>";

            template = template.Replace("<<value>>", column.ColumnName);

            return BuildDataTemplate(template);
        }

        private DataTemplate GetDateCellTemplate(TableColumnModel column)
        {
            string template = @"<DataTemplate xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation' 
                                              xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'  xmlns:cc='clr-namespace:Ecm.CustomControl;assembly=CustomControl' xmlns:cv='clr-namespace:Ecm.CaptureViewer.Converter;assembly=CaptureViewer'
                                              x:Key='DateValueTemplate'>
                                            <DataTemplate.Resources>
                                                <cv:ShortDateTimeConverter x:Key='ShortDateTimeConverter' />
                                            </DataTemplate.Resources>
                                            <DatePicker Height='25' MinWidth='100'
                                                        HorizontalAlignment='Stretch' 
                                                        Padding='2'  
                                                        VerticalAlignment='Center' 
                                                        SelectedDate='{Binding Path=<<value>>, Converter={StaticResource ShortDateTimeConverter}, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged, ValidatesOnDataErrors=True}' 
                                                        IsTabStop='True' 
                                                        IsEnabled='{Binding Path=IsWrite}'
                                                        SelectedDateFormat='Short'
                                                        cc:SelectTextOnFocus.Active='True' x:Name='editorControl' Tag='{Binding Path=.}'/>
                                </DataTemplate>";

            template = template.Replace("<<value>>", column.ColumnName);

            return BuildDataTemplate(template);
        }

        private DataTemplate GetRequiredCellHeaderTemplate(TableColumnModel column)
        {
            string template = @"<DataTemplate xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation' 
                                              xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'
                                              x:Key='HeaderTemplate'>
                                            <Grid HorizontalAlignment='Stretch' MinWidth='60'
                                                  VerticalAlignment='Stretch'>
                                                <Grid.ColumnDefinitions>
                                                    <ColumnDefinition Width='0.8*'/>
                                                    <ColumnDefinition Width='0.2*'/>
                                                </Grid.ColumnDefinitions>
                                                <TextBlock Height='25' Margin='10,2,10,2' Text='<<value>>'/>
                                                <TextBlock Text='*' Foreground='Red' FontSize='12' FontWeight='Bold' Grid.Column='1' Height='25' Margin='2' HorizontalAlignment='Right'/>
                                            </Grid>
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

        private void RemoveBlankRows()
        {
            List<DataRow> blankRows = new List<DataRow>();

            foreach (DataRow row in TableDataSource.Rows)
            {
                if (row.ItemArray.Any(p => p != null && p.ToString() != string.Empty))
                {
                    continue;
                }
                else
                {
                    blankRows.Add(row);
                }
            }

            foreach (DataRow removeRow in blankRows)
            {
                TableDataSource.Rows.Remove(removeRow);
            }
        }

        private bool ValidateData()
        {
            if (TableDataSource == null || TableDataSource.Rows.Count == 0)
            {
                return false;
            }

            foreach (DataRow row in TableDataSource.Rows)
            {
                foreach (DataColumn column in TableDataSource.Columns)
                {
                    if (column.ColumnName.Contains(COLUMN_ID))
                    {
                        continue;
                    }

                    var field = _parentField.Field.Children.SingleOrDefault(p => p.ColumnName == column.ColumnName).Field;

                    if (field.IsRequired && row[column].ToString() == string.Empty)
                    {
                        return false;
                    }
                }
            }

            return true;
        }
    }
}
