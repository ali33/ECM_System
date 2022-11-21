using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;

namespace Ecm.ContentViewer.Controls
{
    /// <summary>
    ///   Control allow user to edit and view text with multiple format (mini MS word)
    /// </summary>
    public partial class TextTool
    {
        public static readonly DependencyProperty AllowTextSelectionProperty = DependencyProperty.Register( "AllowTextSelection", typeof(bool), 
            typeof(TextTool), new PropertyMetadata(false));

        public static readonly DependencyProperty SelectedColorProperty = DependencyProperty.Register( "SelectedColor", typeof(SolidColorBrush),
            typeof(TextTool), new FrameworkPropertyMetadata(Brushes.Red, SelectedColorChangedCallback));

        public static readonly DependencyProperty ShowReadOnlyModeProperty = DependencyProperty.Register("ShowReadOnlyMode", typeof(bool),
                typeof(TextTool), new FrameworkPropertyMetadata(false, ShowReadOnlyModeChangedCallback));

        public static readonly DependencyProperty XamlContentProperty = DependencyProperty.Register("XamlContent", typeof(string),
            typeof(TextTool), new FrameworkPropertyMetadata(string.Empty, XamlContentChangedCallback));

        public static readonly DependencyProperty ZoomRatioProperty = DependencyProperty.Register("ZoomRatio", typeof(double), typeof(TextTool), new PropertyMetadata(1d));

        public TextTool()
        {
            InitializeComponent();
            Loaded += TextAnnotationLoaded;
            SizeChanged += TextAnnotationSizeChanged;
        }

        public bool AllowTextSelection
        {
            get
            {
                return (bool)GetValue(AllowTextSelectionProperty);
            }
            set
            {
                SetValue(AllowTextSelectionProperty, value);
            }
        }

        public double BackupZoomRatio
        {
            get
            {
                return _backupZoomRatio;
            }
            set
            {
                _backupZoomRatio = value;
            }
        }

        public List<string> FontSizes
        {
            get
            {
                var sizes = new List<string>
                                {
                                    "8",
                                    "9",
                                    "10",
                                    "11",
                                    "12",
                                    "14",
                                    "16",
                                    "18",
                                    "20",
                                    "22",
                                    "24",
                                    "26",
                                    "28",
                                    "36",
                                    "48",
                                    "72"
                                };
                return sizes;
            }
        }

        public double NewMinHeight
        {
            get
            {
                return myTextAnnotation.MinHeight;
            }
            set
            {
                myTextAnnotation.MinHeight = value;
            }
        }

        public SolidColorBrush SelectedColor
        {
            get
            {
                return GetValue(SelectedColorProperty) as SolidColorBrush;
            }
            set
            {
                SetValue(SelectedColorProperty, value);
            }
        }

        public bool ShowReadOnlyMode
        {
            get
            {
                return (bool)GetValue(ShowReadOnlyModeProperty);
            }
            set
            {
                SetValue(ShowReadOnlyModeProperty, value);
            }
        }

        public Brush ToolbarBackground
        {
            get
            {
                return toolBar.Background;
            }
            set
            {
                toolBar.Background = value;
            }
        }

        public string XamlContent
        {
            get
            {
                return (string)GetValue(XamlContentProperty);
            }
            set
            {
                SetValue(XamlContentProperty, value);
            }
        }

        public double ZoomRatio
        {
            get
            {
                return (double)GetValue(ZoomRatioProperty);
            }
            set
            {
                SetValue(ZoomRatioProperty, value);
            }
        }

        private static void SelectedColorChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var textAnnotation = d as TextTool;
            if (textAnnotation != null)
            {
                textAnnotation.ChangeColor(e.NewValue as SolidColorBrush);
            }
        }

        private static void ShowReadOnlyModeChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var textAnnotation = d as TextTool;
            if (textAnnotation != null)
            {
                textAnnotation.ShowReadOnly((bool)e.NewValue);
            }
        }

        private static void XamlContentChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var textAnnotation = d as TextTool;
            if (textAnnotation != null)
            {
                textAnnotation.LoadContent(e.NewValue + string.Empty);
            }
        }

        private void ChangeColor(SolidColorBrush brush)
        {
            RichTextControl.Selection.ApplyPropertyValue(ForegroundProperty, brush);
            RichTextControl.Focus();
            GetContent();
        }

        private void GetContent()
        {
            _internalChanges = true;
            var range = new TextRange(RichTextControl.ContentModel.ContentStart, RichTextControl.ContentModel.ContentEnd);
            if (string.IsNullOrEmpty(range.Text.Replace(Environment.NewLine, string.Empty).Trim()))
            {
                XamlContent = string.Empty;
            }
            else
            {
                var stream = new MemoryStream();
                range.Save(stream, DataFormats.Xaml);
                var reader = new StreamReader(stream);
                stream.Position = 0;
                XamlContent = reader.ReadToEnd();
            }
        }

        private void LoadContent(string content)
        {
            if (!_internalChanges)
            {
                var range = new TextRange(RichTextControl.ContentModel.ContentStart, RichTextControl.ContentModel.ContentEnd);
                try
                {
                    // Remove empty lines
                    // Each empty line is formatted as "<Paragraph><Run></Run></Paragraph>" token, however if user choice another color, for example,
                    // this token is not suitable for parsing, so I use token "</Run></Paragraph>" to parse.
                    var newContent = new StringBuilder();
                    const string regular = "</Run></Paragraph>";
                    string[] tokens = content.Split(new[] { regular }, StringSplitOptions.RemoveEmptyEntries);
                    for (int i = 0; i < tokens.Length; i++)
                    {
                        string token = tokens[i];
                        string data = token.Substring(token.LastIndexOf('>') + 1);
                        if (i == 0)
                        {
                            if (!string.IsNullOrEmpty(data) && data.Trim() != string.Empty)
                            {
                                newContent.Append(token + regular);
                            }
                            else
                            {
                                newContent.Append(token.Substring(0, token.LastIndexOf("<Paragraph>")));
                            }
                        }
                        else if (i == tokens.Length - 1)
                        {
                            newContent.Append(token);
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(data) && data.Trim() != string.Empty)
                            {
                                newContent.Append(token + regular);
                            }
                        }
                    }
                    // End of removing empty lines

                    byte[] byteArray = Encoding.UTF8.GetBytes(newContent.ToString());
                    var stream = new MemoryStream(byteArray);
                    range.Load(stream, content.StartsWith(
                        "<Section xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\" xml:space=\"preserve\"")
                                           ? DataFormats.Xaml
                                           : DataFormats.Text);
                }
                catch (XamlParseException) // The text in previous version didn't have Xaml format
                {
                    range.Text = content;
                }
            }
            else if (content == string.Empty)
            {
                new TextRange(RichTextControl.ContentModel.ContentStart, RichTextControl.ContentModel.ContentEnd) { Text = string.Empty };
            }
        }

        private void RemoveToolbarOverflow()
        {
            var overflowGrid = toolBar.Template.FindName("OverflowGrid", toolBar) as FrameworkElement;
            if (overflowGrid != null)
            {
                overflowGrid.Visibility = Visibility.Collapsed;
            }
        }

        private void RichTextControlKeyDown(object sender, KeyEventArgs e)
        {
            string fontSize = cboFontSize.SelectedValue + string.Empty;
            if (!string.IsNullOrEmpty(fontSize))
            {
                RichTextControl.Selection.ApplyPropertyValue(TextElement.FontSizeProperty, fontSize);
            }

            RichTextControl.Selection.ApplyPropertyValue(TextElement.ForegroundProperty, SelectedColor);
        }

        private void RichTextControlKeyUp(object sender, KeyEventArgs e)
        {
            // Ctrl + B
            if ((Keyboard.Modifiers == ModifierKeys.Control) && (e.Key == Key.B))
            {
                ToolStripButtonBold.IsChecked = !ToolStripButtonBold.IsChecked;
                e.Handled = true;
            }

            // Ctrl + I
            if ((Keyboard.Modifiers == ModifierKeys.Control) && (e.Key == Key.I))
            {
                ToolStripButtonItalic.IsChecked = !ToolStripButtonItalic.IsChecked;
                e.Handled = true;
            }

            // Ctrl + U
            if ((Keyboard.Modifiers == ModifierKeys.Control) && (e.Key == Key.U))
            {
                ToolStripButtonUnderline.IsChecked = ToolStripButtonUnderline.IsChecked;
                e.Handled = true;
            }
        }

        private void RichTextControlSelectionChanged(object sender, RoutedEventArgs e)
        {
            var selectionRange = new TextRange(RichTextControl.Selection.Start, RichTextControl.Selection.End);
            ToolStripButtonBold.IsChecked = selectionRange.GetPropertyValue(FontWeightProperty).ToString() == "Bold";
            ToolStripButtonItalic.IsChecked = selectionRange.GetPropertyValue(FontStyleProperty).ToString() == "Italic";
            ToolStripButtonUnderline.IsChecked = selectionRange.GetPropertyValue(Inline.TextDecorationsProperty) == TextDecorations.Underline;
            cboFontSize.SelectedValue = selectionRange.GetPropertyValue(FlowDocument.FontSizeProperty).ToString();
        }

        private void RichTextControlTextChanged(object sender, TextChangedEventArgs e)
        {
            GetContent();
        }

        private void ShowReadOnly(bool readOnlyMode)
        {
            RichTextControl.IsReadOnly = readOnlyMode;
            RichTextControl.IsHitTestVisible = !readOnlyMode || AllowTextSelection;
            RichTextControl.Focusable = !readOnlyMode || AllowTextSelection;

            if (readOnlyMode)
            {
                ZoomRatio = BackupZoomRatio;
                toolBar.Visibility = Visibility.Collapsed;
                RichTextControl.BorderThickness = new Thickness(0);
                RichTextControl.Background = Brushes.Transparent;
                Background = Brushes.Transparent;
            }
            else
            {
                BackupZoomRatio = ZoomRatio;
                ZoomRatio = 1;

                toolBar.Visibility = Visibility.Visible;
                RichTextControl.BorderThickness = new Thickness(1);
                RichTextControl.Background = Brushes.White;
                RichTextControl.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
                Background = Brushes.White;
                RichTextControl.Focus();
            }

            RemoveToolbarOverflow();

            if (IsLoaded)
            {
                GetContent();
            }
        }

        private void TextAnnotationLoaded(object sender, RoutedEventArgs e)
        {
            cboFontSize.SelectedValue = "12";
            MouseEnter += TextAnnotationMouseEnter;
            RichTextControl.Focus();
        }

        private void TextAnnotationMouseEnter(object sender, MouseEventArgs e)
        {
            if (!ShowReadOnlyMode)
            {
                Cursor = Cursors.Arrow;
                e.Handled = true;
            }
            else
            {
                Cursor = null;
            }
        }

        private void TextAnnotationSizeChanged(object sender, SizeChangedEventArgs e)
        {
            RichTextControl.Width = e.NewSize.Width;
            RichTextControl.Height = e.NewSize.Height - toolBar.ActualHeight;
            RichTextControl.ContentModel.PageWidth = e.NewSize.Width;
        }

        private void CboFontSizeDropDownClosed(object sender, EventArgs e)
        {
            string fontSize = cboFontSize.SelectedValue + string.Empty;

            if (!string.IsNullOrEmpty(fontSize))
            {
                RichTextControl.Selection.ApplyPropertyValue(FontSizeProperty, fontSize);
                RichTextControl.Focus();
            }

            GetContent();
        }

        private void ToolBarSizeChanged(object sender, SizeChangedEventArgs e)
        {
            RemoveToolbarOverflow();
        }

        private double _backupZoomRatio = 1;

        private bool _internalChanges;
    }
}