using Ecm.ContentViewer.Model;
using Ecm.ContentViewer.Extension;
using Ecm.ContentViewer.Model;
using Ecm.ContentViewer.ViewModel;
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

namespace Ecm.ContentViewer.View
{
    /// <summary>
    /// Interaction logic for IndexViewer.xaml
    /// </summary>
    public partial class IndexViewer : UserControl
    {
        private Point _origContentMouseDownPoint;
        private bool _lookupCompleted;
        private IndexViewerViewModel _viewModel;

        public IndexViewer()
        {
            InitializeComponent();
            Loaded += IndexViewerLoaded;
            cltIndexView.ItemContainerGenerator.StatusChanged += ItemContainerGeneratorStatusChanged;
        }

        private void IndexViewerLoaded(object sender, RoutedEventArgs e)
        {
            _viewModel = DataContext as IndexViewerViewModel;
        }

        private void ItemContainerGeneratorStatusChanged(object sender, EventArgs e)
        {
            if (cltIndexView.ItemContainerGenerator.Status == System.Windows.Controls.Primitives.GeneratorStatus.ContainersGenerated)
            {
                var container = cltIndexView.ItemContainerGenerator.ContainerFromIndex(0) as ContentPresenter;
                if (container != null)
                {
                    container.ApplyTemplate();
                    var indexEditorTemplateSelector = container.ContentTemplateSelector as FieldEditorTemplateSelector;
                    if (indexEditorTemplateSelector != null)
                    {
                        var selectTemplate = indexEditorTemplateSelector.SelectTemplate(_viewModel.FieldValues[0], container);
                        if (selectTemplate != null)
                        {
                            var control = selectTemplate.FindName("editorControl", container) as FrameworkElement;
                            if (control != null)
                            {
                                control.Loaded += FirstControlLoaded;
                                control.IsVisibleChanged += FirstControlIsVisibleChanged;
                                control.KeyDown += IndexEditorKeyDown;
                            }
                        }
                    }
                }

                if (_viewModel.FieldValues != null && _viewModel.FieldValues.Count > 1)
                {
                    var lastContainer = cltIndexView.ItemContainerGenerator.ContainerFromIndex(_viewModel.FieldValues.Count - 1) as ContentPresenter;
                    if (lastContainer != null)
                    {
                        lastContainer.ApplyTemplate();
                        if (container != null)
                        {
                            var indexEditorTemplateSelector = container.ContentTemplateSelector as FieldEditorTemplateSelector;
                            if (indexEditorTemplateSelector != null)
                            {
                                var selectTemplate = indexEditorTemplateSelector.SelectTemplate(_viewModel.FieldValues[_viewModel.FieldValues.Count - 1], lastContainer);
                                if (selectTemplate != null)
                                {
                                    var control = selectTemplate.FindName("editorControl", lastContainer) as FrameworkElement;
                                    if (control != null)
                                    {
                                        control.KeyDown += IndexEditorKeyDown;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private void IndexEditorKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Tab)
            {
                var control = sender as FrameworkElement;
                if (control != null)
                {
                    var index = control.DataContext as FieldValueModel;
                    int position = _viewModel.FieldValues.IndexOf(index);

                    if (e.KeyboardDevice.Modifiers == ModifierKeys.Shift && position == 0)
                    {
                        _viewModel.PreviousDocumentCommand.Execute(null);
                    }

                    if (e.KeyboardDevice.Modifiers == ModifierKeys.None && position == _viewModel.FieldValues.Count - 1)
                    {
                        _viewModel.NextDocumentCommand.Execute(null);
                    }
                }
            }
        }

        private void FirstControlLoaded(object sender, RoutedEventArgs e)
        {
            var control = sender as UIElement;
            SetFocus(control);
        }

        private void FirstControlIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var control = sender as UIElement;
            SetFocus(control);
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

        private void DatePickerLoaded(object sender, RoutedEventArgs e)
        {
            // The 2 events below can't register from xaml because they're Handled = true internal. The way below help them is triggered
            var datePicker = sender as DatePicker;
            if (datePicker != null)
            {
                datePicker.AddHandler(GotFocusEvent, new RoutedEventHandler(EditorControlGotFocus), true);
                datePicker.AddHandler(LostFocusEvent, new RoutedEventHandler(EditorControlLostFocus), true);
            }
        }

        private void EditorControlGotFocus(object sender, RoutedEventArgs e)
        {
            var editorControl = sender as Control;
            pnlIndexSnippetContainer.Visibility = Visibility.Collapsed;
            if (editorControl != null && editorControl.Tag != null)
            {
                var fieldValue = editorControl.Tag as FieldValueModel;
                if (fieldValue != null && fieldValue.SnippetImage != null)
                {
                    pnlIndexSnippetContainer.Visibility = Visibility.Visible;
                    SnippetImage.Source = fieldValue.SnippetImage;
                    CenterImage();
                    if (_viewModel.MainViewModel.OpeningContainerItem != null &&
                        _viewModel.MainViewModel.OpeningContainerItem.ItemType == ContentItemType.ContentModel &&
                        _viewModel.IndexedItems.Any(p => p == _viewModel.MainViewModel.OpeningContainerItem) && fieldValue.Field.OCRTemplateZone != null)
                    {
                        var ocrTemplatePage = _viewModel.MainViewModel.OpeningContainerItem.DocumentData.DocumentType.OCRTemplate.OCRTemplatePages.FirstOrDefault(p => p.Id == fieldValue.Field.OCRTemplateZone.OCRTemplatePageId);
                        if (ocrTemplatePage != null)
                        {
                            (_viewModel.MainViewModel.ContentViewModel as ImageViewerViewModel).UnPromptOcr(ocrTemplatePage.PageIndex + 1, fieldValue.Field.OCRTemplateZone, ocrTemplatePage);
                        }
                    }
                }
            }
        }

        private void EditorControlLostFocus(object sender, RoutedEventArgs e)
        {
            pnlIndexSnippetContainer.Visibility = Visibility.Collapsed;
            (_viewModel.MainViewModel.ContentViewModel as ImageViewerViewModel).UnPromptOcr();
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
                    if (fieldValue.Field.LookupInfo.MinPrefixLength > 0)
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
                            fieldValue.LookupData = _viewModel.MainViewModel.GetLookupData(fieldValue.Field, fieldValue.Value);
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
                _viewModel.MainViewModel.HandleException(ex);
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
                                       xmlns:cv='clr-namespace:Ecm.CaptureViewer.Converter;assembly=CaptureViewer'>
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

        private void btnShowDetail_Click(object sender, RoutedEventArgs e)
        {
            FieldValueModel fieldValueModel = (sender as Button).DataContext as FieldValueModel;
            FieldValueModel editFieldValue = _viewModel.FieldValues.SingleOrDefault(p => p.Field.Id == fieldValueModel.Field.Id);

            var dialogViewer = new DialogViewer { Width = 600, Height = 500, Text = "Table columns" };
            var columnView = new TableColumnView(fieldValueModel)
            {
                Dialog = dialogViewer
            };
            dialogViewer.WpfContent = columnView;
            dialogViewer.ShowDialog();

            editFieldValue.TableValues = fieldValueModel.TableValues;
            _viewModel.MainViewModel.IsChanged = true;
            _viewModel.MainViewModel.Items[0].IsChanged = true;
            editFieldValue.Value = DateTime.Now.ToString();
        }

        private void ZoomAndPanControlMouseDown(object sender, MouseButtonEventArgs e)
        {
            SnippetImage.Focus();
            Keyboard.Focus(SnippetImage);
            _origContentMouseDownPoint = e.GetPosition(SnippetImage);

            if (e.ChangedButton == MouseButton.Left)
            {
                // Just a plain old left-down initiates panning mode.
                ZoomAndPanControl.CaptureMouse();
            }
        }

        private void ZoomAndPanControlMouseUp(object sender, MouseButtonEventArgs e)
        {
            ZoomAndPanControl.ReleaseMouseCapture();
            e.Handled = true;
        }

        private void ZoomAndPanControlMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                Point curContentMousePoint = e.GetPosition(SnippetImage);
                Vector dragOffset = curContentMousePoint - _origContentMouseDownPoint;
                double top = Canvas.GetTop(SnippetImage);
                double left = Canvas.GetLeft(SnippetImage);
                top = (double.IsNaN(top) ? 0 : top);
                left = (double.IsNaN(left) ? 0 : left);
                Canvas.SetLeft(SnippetImage, left + dragOffset.X);
                Canvas.SetTop(SnippetImage, top + dragOffset.Y);
                e.Handled = true;
            }
        }

        private void ZoomAndPanControlMouseWheel(object sender, MouseWheelEventArgs e)
        {
            e.Handled = true;

            if (e.Delta > 0)
            {
                ZoomIn();
            }
            else if (e.Delta < 0)
            {
                ZoomOut();
            }

            CenterImage();
        }

        private void ZoomOut()
        {
            var scaleTransform = (ScaleTransform)SnippetImage.RenderTransform;
            if (scaleTransform.ScaleX > 0.3)
            {
                scaleTransform.ScaleX -= 0.1;
                scaleTransform.ScaleY -= 0.1;
            }
        }

        private void ZoomIn()
        {
            var scaleTransform = (ScaleTransform)SnippetImage.RenderTransform;
            scaleTransform.ScaleX += 0.1;
            scaleTransform.ScaleY += 0.1;
        }

        private void CenterImage()
        {
            SnippetImage.UpdateLayout();

            var scaleTransform = (ScaleTransform)SnippetImage.RenderTransform;
            var centerX = (ZoomAndPanControl.ActualWidth - SnippetImage.ActualWidth * scaleTransform.ScaleX) / 2;
            var centerY = (ZoomAndPanControl.ActualHeight - SnippetImage.ActualHeight * scaleTransform.ScaleY) / 2;
            if (centerX < 0)
            {
                Canvas.SetLeft(SnippetImage, 0);
            }
            else
            {
                Canvas.SetLeft(SnippetImage, centerX);
            }

            if (centerY < 0)
            {
                Canvas.SetTop(SnippetImage, 0);
            }
            else
            {
                Canvas.SetTop(SnippetImage, centerY);
            }
        }

        private void StackPanel_MouseEnter(object sender, MouseEventArgs e)
        {
            _viewModel.IsShowTableDetail = true;
        }

        private void StackPanel_MouseLeave(object sender, MouseEventArgs e)
        {
            _viewModel.IsShowTableDetail = false;
        }

    }
}
