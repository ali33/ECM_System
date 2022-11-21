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
using Ecm.Workflow.Activities.LookupConfiguration.ViewModel;
using Ecm.Localization;
using System.Collections.ObjectModel;
using Ecm.Workflow.Activities.CustomActivityModel;
using Ecm.CustomControl;
using System.Data;
using System.IO;
using System.Xml;

namespace Ecm.Workflow.Activities.LookupConfiguration.View
{
    /// <summary>
    /// Interaction logic for LookupView.xaml
    /// </summary>
    public partial class LookupConfigurationView : UserControl
    {
        private LookupConfigurationViewModel _viewModel;

        public LookupConfigurationView(LookupConfigurationViewModel viewModel)
        {
            InitializeComponent();
            DataContext = _viewModel = viewModel;
            viewModel.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(viewModel_PropertyChanged);
        }

        void viewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "TestData")
            {
                BuildTestData(_viewModel.TestData);
            }
        }

        public DialogViewer Dialog { get; set; }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            if (Dialog != null)
            {
                Dialog.Close();
            }
        }

        private void cboLookupSource_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LookupConfigurationViewModel viewModel = DataContext as LookupConfigurationViewModel;

            if (viewModel != null && viewModel.LookupInfo != null)
            {
                if (viewModel.LookupInfo.SourceName != null)
                {
                    viewModel.LoadLookupColumn();
                    viewModel.LoadIndexMapping();
                    

                    if (viewModel.IsStored)
                    {
                        viewModel.LoadOperatorSourceForStored();
                        viewModel.LoadParameters(viewModel.LookupInfo.SourceName);
                    }
                    else
                    {
                        viewModel.LoadOperatorSource();
                    }
                }
                else
                {
                    viewModel.LookupInfo.FieldMappings = new ObservableCollection<LookupMappingModel>();
                    lvMapping.ItemsSource = viewModel.LookupInfo.FieldMappings;
                }
            }
        }

        private void cboDatabase_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LookupConfigurationViewModel viewModel = DataContext as LookupConfigurationViewModel;

            if (viewModel != null && viewModel.LookupInfo != null)
            {
                viewModel.LoadDataSource(viewModel.LookupSource);
            }
        }

        private void cboSchema_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LookupConfigurationViewModel viewModel = DataContext as LookupConfigurationViewModel;

            if (viewModel != null && viewModel.LookupInfo != null)
            {
                viewModel.LoadDataSource(viewModel.LookupSource);
            }
        }

        private void BuildTestData(DataTable result)
        {
            var listView = new SortableListView { SelectionMode = SelectionMode.Multiple };

            // Create a binding with auto resize column widths of listview
            var binding = new Binding("TestData") { NotifyOnTargetUpdated = true };
            BindingOperations.SetBinding(listView, ItemsControl.ItemsSourceProperty, binding);

            listView.BorderThickness = new Thickness(0);
            listView.HorizontalAlignment = HorizontalAlignment.Stretch;
            listView.Focusable = false;
            listView.ItemContainerStyle = TryFindResource("TestDataStyle") as Style;
            listView.View = GenerateGridView(result);
            pnResult.Children.Clear();
            pnResult.Children.Add(listView);
        }

        private GridView GenerateGridView(DataTable searchResult)
        {
            if (searchResult == null)
            {
                return null;
            }

            var view = new GridView();
            BuildColumns(searchResult, view);
            return view;
        }

        private void BuildColumns(DataTable searchResult, GridView view)
        {
            foreach (DataColumn column in searchResult.Columns)
            {
                var viewColumn = new SortableGridViewColumn
                {
                    Header = column.ColumnName,
                    DisplayMemberBinding = new Binding(column.ColumnName),
                    SortPropertyName = column.ColumnName
                };

                //PopulateColumnStyle(searchResult, viewColumn, column);
                view.Columns.Add(viewColumn);
            }
        }

        private void PopulateColumnStyle(DataTable searchResult, SortableGridViewColumn viewColumn, DataColumn column)
        {
            if (column.DataType != typeof(string))
            {
                viewColumn.CellTemplate = GetRightAlignColumnTemplate(column.ColumnName);
                viewColumn.DisplayMemberBinding = null;

                if (column.DataType == typeof(DateTime))
                {
                    viewColumn.CellTemplate = GetShortDateColumnTemplate(column.ColumnName);
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
    }
}
