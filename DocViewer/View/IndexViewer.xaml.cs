using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Xml;
using Ecm.DocViewer.Extension;
using Ecm.DocViewer.Model;
using Ecm.Domain;
using Ecm.Model;
using System.Windows.Input;

namespace Ecm.DocViewer
{
    public partial class IndexViewer
    {
        private bool _lookupCompleted;
        private Point _origContentMouseDownPoint;

        #region Dependency properties

        public static readonly DependencyProperty FieldValuesProperty =
            DependencyProperty.Register("FieldValues", typeof(ObservableCollection<FieldValueModel>), typeof(IndexViewer));

        public static readonly DependencyProperty CanUpdateIndexValueProperty =
            DependencyProperty.Register("CanUpdateIndexValue", typeof(bool), typeof(IndexViewer));

        #endregion

        public IndexViewer()
        {
            InitializeComponent();
            Loaded += IndexViewerLoaded;
            cltIndexView.ItemContainerGenerator.StatusChanged += ItemContainerGeneratorStatusChanged;
        }

        public void UpdateTitle()
        {
            if (FieldValues.Count > 0)
            {
                if (!ViewerContainer.ThumbnailCommandManager.IndexedItems.Any(p => p.ItemType == ContentItemType.Batch))
                {
                    var docs = ViewerContainer.ThumbnailCommandManager.IndexedItems[0].BatchItem.Children.Where(p => p.ItemType == ContentItemType.Document).ToList();
                    lblTitle1.Text = string.Empty;
                    foreach (var doc in docs)
                    {
                        if (ViewerContainer.ThumbnailCommandManager.IndexedItems.Contains(doc))
                        {
                            lblTitle1.Text += "," + (docs.IndexOf(doc) + 1);
                        }
                    }

                    lblTitle1.Text = lblTitle1.Text.Substring(1) + ". " + ViewerContainer.ThumbnailCommandManager.IndexedItems[0].DocumentData.DocumentType.Name;
                    lblTitle2.Text = "(" + docs.Count + " doc(s))";
                }
            }
            else
            {
                lblTitle1.Text = Properties.Resources.NoIndexFound;
                lblTitle2.Text = string.Empty;
            }
        }

        public ViewerContainer ViewerContainer { get; set; }

        public ObservableCollection<FieldValueModel> FieldValues
        {
            get { return GetValue(FieldValuesProperty) as ObservableCollection<FieldValueModel>; }
            set { SetValue(FieldValuesProperty, value); }
        }

        public bool CanUpdateIndexValue
        {
            get { return (bool)GetValue(CanUpdateIndexValueProperty); }
            set { SetValue(CanUpdateIndexValueProperty, value); }
        }

        #region Private methods

        private void IndexViewerLoaded(object sender, RoutedEventArgs e)
        {
            btnNextDoc.Command = ViewerContainer.ThumbnailCommandManager.IndexNextDocCommand;
            btnPrevDoc.Command = ViewerContainer.ThumbnailCommandManager.IndexPreviousDocCommand;
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
                        var selectTemplate = indexEditorTemplateSelector.SelectTemplate(FieldValues[0], container);
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

                if (FieldValues != null && FieldValues.Count > 1)
                {
                    var lastContainer = cltIndexView.ItemContainerGenerator.ContainerFromIndex(FieldValues.Count - 1) as ContentPresenter;
                    if (lastContainer != null)
                    {
                        lastContainer.ApplyTemplate();
                        if (container != null)
                        {
                            var indexEditorTemplateSelector = container.ContentTemplateSelector as FieldEditorTemplateSelector;
                            if (indexEditorTemplateSelector != null)
                            {
                                var selectTemplate = indexEditorTemplateSelector.SelectTemplate(FieldValues[FieldValues.Count - 1], lastContainer);
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
                    int position = FieldValues.IndexOf(index);

                    if (e.KeyboardDevice.Modifiers == ModifierKeys.Shift && position == 0)
                    {
                        ViewerContainer.ThumbnailCommandManager.IndexPreviousDocCommand.Execute(null, btnPrevDoc);
                    }

                    if (e.KeyboardDevice.Modifiers == ModifierKeys.None && position == FieldValues.Count - 1)
                    {
                        ViewerContainer.ThumbnailCommandManager.IndexNextDocCommand.Execute(null, btnNextDoc);
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
                    if (ViewerContainer.OpeningContainerItem != null && 
                        ViewerContainer.OpeningContainerItem.ItemType == ContentItemType.Document &&
                        ViewerContainer.ThumbnailCommandManager.IndexedItems.Any(p => p == ViewerContainer.OpeningContainerItem))
                    {
                        var ocrTemplatePage = ViewerContainer.OpeningContainerItem.DocumentData.DocumentType.OCRTemplate.OCRTemplatePages.FirstOrDefault(p => p.Id == fieldValue.Field.OCRTemplateZone.OCRTemplatePageId);
                        if (ocrTemplatePage != null)
                        {
                            ViewerContainer.ImageViewer.PromptOCRZone(ocrTemplatePage.PageIndex + 1, fieldValue.Field.OCRTemplateZone, ocrTemplatePage);
                        }
                    }
                }
            }
        }

        private void EditorControlLostFocus(object sender, RoutedEventArgs e)
        {
            pnlIndexSnippetContainer.Visibility = Visibility.Collapsed;
            ViewerContainer.ImageViewer.UnPromptOCRZone();
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
                    DataTemplate template = GetLookupDataTemplate(fieldValue.Field.Maps.Where(p=> !string.IsNullOrEmpty(p.DataColumn)).Select(p => p.Name).ToList());
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
                            fieldValue.LookupData = ViewerContainer.GetLookupData(fieldValue.Field, fieldValue.Value);
                            if (fieldValue.LookupData != null)
                            {
                                fieldValue.LookupData.TableName = fieldValue.Field.Name;
                                at.ItemsSource = fieldValue.LookupData.DefaultView;
                                at.ItemFilter = Filter;
                                at.PopulateComplete();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ViewerContainer.HandleException(ex);
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
                                       xmlns:cv='clr-namespace:Ecm.DocViewer.Converter;assembly=DocViewer'>
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
            if (FieldValues.Any(p => p.Field.Name == name))
            {
                return FieldValues.First(p => p.Field.Name == name).Field.DataType;
            }

            return null;
        }

        public bool Filter(string search, object item)
        {
            return true;

            search = (search + string.Empty).ToLower();
            var row = item as DataRowView;
            if (row != null)
            {
                string indexName = row.DataView.Table.TableName;
                int position = row.DataView.Table.Columns.IndexOf(indexName);
                if (position >= 0)
                {
                    if (FieldValues.Any(p => p.Field.Name == indexName))
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
                foreach (FieldValueModel index in FieldValues)
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
            FieldValueModel editFieldValue = FieldValues.SingleOrDefault(p=>p.Field.Id == fieldValueModel.Field.Id);

            var dialogViewer = new DialogViewer { Width = 600, Height = 500, Text = fieldValueModel.Field.Name };
            var columnView = new TableColumnView(fieldValueModel)
            {
                Dialog = dialogViewer
            };
            dialogViewer.WpfContent = columnView;
            dialogViewer.ShowDialog();


            editFieldValue.TableValues = fieldValueModel.TableValues;
            ViewerContainer.IsChanged = true;
            ViewerContainer.Items[0].IsChanged = true;
            editFieldValue.Value = DateTime.Now.ToString();
        }

        #endregion

        #region Snippet utilities

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

        #endregion
    }
}