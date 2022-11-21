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
using Ecm.Model;
using Ecm.CustomAddin.ViewModel;
using Ecm.Domain;
using System.IO;
using System.Xml;
using System.Data;

namespace Ecm.CustomAddin.View
{
    /// <summary>
    /// Interaction logic for DashboardView.xaml
    /// </summary>
    public partial class DashboardView : UserControl
    {
        private DashboardViewModel _viewModel;
        private bool _lookupCompleted;

        public DashboardView(DocumentModel docModel, PageModel pageModel)
        {
            InitializeComponent();
            DataContext = _viewModel = new DashboardViewModel(docModel, pageModel);
        }

        private void AutoCompleteBoxLoaded(object sender, RoutedEventArgs e)
        {
            var at = sender as AutoCompleteBox;
            if (at != null)
            {
                var fieldValue = at.DataContext as FieldValueModel;
                if (TryFindResource("LookupDataTemplate" + at.Uid) != null)
                {
                    Resources.Remove("LookupDataTemplate" + at.Uid);
                }

                if (fieldValue != null)
                {
                    DataTemplate template = GetLookupDataTemplate(fieldValue.Field.Maps.Select(p => p.Name).ToList());
                    if (fieldValue.Field.LookupInfo != null && fieldValue.Field.LookupInfo.MinPrefixLength > 0)
                    {
                        at.MinimumPrefixLength = fieldValue.Field.LookupInfo.MinPrefixLength;
                    }

                    at.ItemTemplate = template;
                    at.ValueMemberPath = fieldValue.Field.Name;
                }
            }
        }

        private void AutoCompleteBoxTextChanged(object sender, RoutedEventArgs e)
        {
            var autoCompleteTextBox = sender as AutoCompleteBox;
            if (autoCompleteTextBox != null && !string.IsNullOrEmpty(autoCompleteTextBox.Text))
            {
                autoCompleteTextBox.Populating += AutoCompleteTextBoxPopulating;
            }
        }

        private void AutoCompleteTextBoxPopulating(object sender, PopulatingEventArgs e)
        {
            try
            {
                var at = sender as AutoCompleteBox;
                if (at != null)
                {
                    var fieldValue = at.DataContext as FieldValueModel;
                    if (!_lookupCompleted)
                    {
                        if (fieldValue != null)
                        {
                            fieldValue.LookupData = _viewModel.GetLookupData(fieldValue.Field, fieldValue.Value);
                            if (fieldValue.LookupData != null)
                            {
                                fieldValue.LookupData.TableName = fieldValue.Field.Name;
                                at.ItemsSource = fieldValue.LookupData.DefaultView;
                                at.ItemFilter += Filter;
                                at.PopulateComplete();
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

        private void AutoCompleteBoxSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems != null && e.AddedItems.Count > 0)
            {
                var at = sender as AutoCompleteBox;
                if (at != null && at.IsDropDownOpen)
                {
                    LookupFinished(at);
                }
            }

            e.Handled = true;
        }

        private void AutoCompleteBoxPreviewKeyDown(object sender, KeyEventArgs e)
        {
            var at = sender as AutoCompleteBox;
            if (e.Key == Key.Tab && at != null)
            {
                var index = at.DataContext as FieldValueModel;
                if (index != null && at.IsDropDownOpen)
                {
                    LookupFinished(at);
                }
            }
            else if (e.Key == Key.Enter && at != null)
            {
                at.IsDropDownOpen = false;
                e.Handled = true;
            }
        }

        private DataTemplate GetLookupDataTemplate(IEnumerable<string> lookupMaps)
        {
            var stb = new StringBuilder();
            var textBockCollection = new StringBuilder();
            int i = 1;
            const string textValueTemplate = @"<Border BorderThickness='0' BorderBrush='#b9d3e3' Height='26'>
                                                   <TextBlock HorizontalAlignment='Right' Foreground='Black' Background='Transparent' TextAlignment='<<align>>'
                                                              Text='{Binding <<value>>}' Margin='2,1,2,0' Width='100' Height='22'/>
                                               </Border>";
            foreach (string map in lookupMaps)
            {
                FieldDataType? type = GetColumnType(map);
                if (type != null)
                {
                    string align = "Right";
                    string converter = string.Empty;

                    switch (type.Value)
                    {
                        case FieldDataType.Boolean:
                            align = "Left";
                            converter = ", Converter={StaticResource YesNoConverter}";
                            break;
                        case FieldDataType.Integer:
                        case FieldDataType.Decimal:
                            align = "Right";
                            break;
                        case FieldDataType.Date:
                            converter = ", Converter={StaticResource ShortDateTimeConverter}";
                            align = "Left";
                            break;
                        case FieldDataType.String:
                        case FieldDataType.Picklist:
                            align = "Left";
                            break;
                    }

                    //Always align left hand side for first column to make consistently with search UI.
                    if (i == 1)
                    {
                        align = "Left";
                    }

                    string textValue = textValueTemplate.Replace("<<align>>", align);
                    textValue = textValue.Replace("<<value>>", map + converter);
                    textBockCollection.Append(textValue);

                    i++;
                }
            }

            stb.Append(@"<DataTemplate xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation'
                                       xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'
                                       xmlns:cv='clr-namespace:Ecm.CustomAddin.Converter;assembly=Ecm.CustomAddin'>
                            <DataTemplate.Resources>
                                <cv:ShortDateTimeConverter x:Key='ShortDateTimeConverter' />
                                <cv:YesNoConverter x:Key='YesNoConverter' />
                            </DataTemplate.Resources>
                            <StackPanel HorizontalAlignment='Left' Orientation='Horizontal' Width='Auto'>
                                <<value>>
                            </StackPanel>
                        </DataTemplate>");

            string template = stb.ToString().Replace("<<value>>", textBockCollection.ToString());
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

        private FieldDataType? GetColumnType(string name)
        {
            if (_viewModel.FieldValues.Any(p => p.Field.Name == name))
            {
                return _viewModel.FieldValues.First(p => p.Field.Name == name).Field.DataType;
            }

            return null;
        }

        public bool Filter(string search, object item)
        {
            search = (search + string.Empty).ToLower();
            var row = item as DataRowView;
            if (row != null)
            {
                string indexName = row.DataView.Table.TableName;
                int position = row.DataView.Table.Columns.IndexOf(indexName);
                if (position >= 0)
                {
                    if (_viewModel.FieldValues.Any(p => p.Field.Name == indexName))
                    {
                        return (row[position] != null && row[position].ToString().ToLower().Contains(search));
                    }
                }
            }

            return false;
        }

        private void LookupFinished(AutoCompleteBox at)
        {
            var lookupItem = at.SelectedItem as DataRowView;
            if (lookupItem != null)
            {
                _lookupCompleted = true;
                foreach (FieldValueModel index in _viewModel.FieldValues)
                {
                    int position = lookupItem.DataView.Table.Columns.IndexOf(index.Field.Name);
                    if (position >= 0)
                    {
                        index.Value = lookupItem[position].ToString();
                    }
                }

                _lookupCompleted = false;
            }
        }

        private static void SetFocus(UIElement control)
        {
            if (control is AutoCompleteBox && (control as AutoCompleteBox).TextBox != null)
            {
                (control as AutoCompleteBox).TextBox.Focus();
            }
            else if (control is DatePicker)
            {
                (control as DatePicker).MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
            }
            else
            {
                control.Focus();
            }
        }
    }
}
