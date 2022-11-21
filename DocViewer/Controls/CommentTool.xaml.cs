using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Navigation;

namespace Ecm.DocViewer.Controls
{
    public partial class CommentTool
    {
        public static readonly DependencyProperty AllowTextSelectionProperty =
                DependencyProperty.Register("AllowTextSelection", typeof(bool), typeof(CommentTool), new PropertyMetadata(false));

        public static readonly DependencyProperty EnableConvertUrlProperty =
                DependencyProperty.Register("EnableConvertUrl", typeof(bool), typeof(CommentTool), new PropertyMetadata(false));

        public static readonly DependencyProperty MaxTextLengthProperty =
                DependencyProperty.Register("MaxTextLength", typeof(int), typeof(CommentTool), new PropertyMetadata(100));

        public static readonly DependencyProperty ShowReadOnlyModeProperty =
                        DependencyProperty.Register("ShowReadOnlyMode", typeof(bool), typeof(CommentTool),
                            new FrameworkPropertyMetadata(false, ShowReadOnlyModeChangedCallback));

        private static void ShowReadOnlyModeChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var commentEditor = d as CommentTool;
            if (commentEditor != null)
            {
                commentEditor.ShowReadOnly((bool)e.NewValue);
            }
        }

        public static readonly DependencyProperty XamlContentProperty =
                        DependencyProperty.Register("XamlContent", typeof(string), typeof(CommentTool),
                            new FrameworkPropertyMetadata(string.Empty, XamlContentChangedCallback));

        private static void XamlContentChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var commentEditor = d as CommentTool;
            if (commentEditor != null)
            {
                commentEditor.LoadContent(e.NewValue + string.Empty);
            }
        }

        public static readonly DependencyProperty ZoomRatioProperty =
                DependencyProperty.Register("ZoomRatio", typeof(double), typeof(CommentTool), new PropertyMetadata(1d));

        public static readonly DependencyProperty SelectedColorProperty =
                DependencyProperty.Register("SelectedColor", typeof(SolidColorBrush), typeof(CommentTool),
                            new FrameworkPropertyMetadata(Brushes.Red, SelectedColorChangedCallback));

        private static void SelectedColorChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var commentEditor = d as CommentTool;
            if (commentEditor != null)
            {
                commentEditor.ChangeColor(e.NewValue as SolidColorBrush);
            }
        }

        public static readonly DependencyProperty HideToolbarModeProperty =
            DependencyProperty.Register("HideToolbarMode", typeof(bool), typeof(CommentTool),
                                        new FrameworkPropertyMetadata(false, HideToolbarModeChangedCallback));

        private static void HideToolbarModeChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var commentEditor = d as CommentTool;
            if (commentEditor != null)
            {
                commentEditor.HideToolbar((bool)e.NewValue);
            }
        }

        public CommentTool()
        {
            InitializeComponent();
            Loaded += TextAnnotationLoaded;
            SizeChanged += TextAnnotationSizeChanged;
        }

        public bool AllowTextSelection
        {
            get { return (bool)GetValue(AllowTextSelectionProperty); }
            set { SetValue(AllowTextSelectionProperty, value); }
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

        public bool ShowReadOnlyMode
        {
            get { return (bool)GetValue(ShowReadOnlyModeProperty); }
            set { SetValue(ShowReadOnlyModeProperty, value); }
        }

        public string XamlContent
        {
            get { return (string)GetValue(XamlContentProperty); }
            set { SetValue(XamlContentProperty, value); }
        }

        public double ZoomRatio
        {
            get { return (double)GetValue(ZoomRatioProperty); }
            set { SetValue(ZoomRatioProperty, value); }
        }

        public double BackupZoomRatio
        {
            get { return _backupZoomRatio; }
            set { _backupZoomRatio = value; }
        }

        public SolidColorBrush SelectedColor
        {
            get { return GetValue(SelectedColorProperty) as SolidColorBrush; }
            set { SetValue(SelectedColorProperty, value); }
        }

        public double NewMinHeight
        {
            get { return myTextAnnotation.MinHeight; }
            set { myTextAnnotation.MinHeight = value; }
        }

        public Brush ToolbarBackground
        {
            get { return toolBar.Background; }
            set { toolBar.Background = value; }
        }

        public bool HideToolbarMode
        {
            get { return (bool)GetValue(HideToolbarModeProperty); }
            set { SetValue(HideToolbarModeProperty, value); }
        }

        public int MaxTextLength
        {
            get { return (int)GetValue(MaxTextLengthProperty); }
            set { SetValue(MaxTextLengthProperty, value); }
        }

        public bool EnableConvertUrl
        {
            get { return (bool)GetValue(EnableConvertUrlProperty); }
            set { SetValue(EnableConvertUrlProperty, value); }
        }

        public bool EnableDropShadow
        {
            set
            {
                if (value)
                {
                    var shadow = new DropShadowEffect {Color = Colors.Gray, BlurRadius = 10, ShadowDepth = 0};
                    RichTextControl.Effect = shadow;
                }
            }
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

        private void LoadContent(string content)
        {
            if (!_internalChanges)
            {
                var range = new TextRange(RichTextControl.Document.ContentStart, RichTextControl.Document.ContentEnd);
                if (string.IsNullOrEmpty(content))
                {
                    range.Text = string.Empty;
                }
                else if (content.StartsWith("<Section xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\" xml:space=\"preserve\""))
                {
                    var tmpRtb = new RichTextBox();
                    var tmpRange = new TextRange(tmpRtb.Document.ContentStart, tmpRtb.Document.ContentEnd);
                    var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));
                    tmpRange.Load(stream, DataFormats.Xaml);

                    BlockCollection blocks = tmpRtb.Document.Blocks;
                    var paras = new Block[blocks.Count];
                    blocks.CopyTo(paras, 0);

                    paras = RemoveLastEmptyBlocks(paras);

                    var resultDocument = new FlowDocument();
                    RichTextControl.Document = resultDocument;
                    var outerTextRange = new TextRange(resultDocument.ContentStart, resultDocument.ContentEnd);
                    LocalValueEnumerator globalProperties = tmpRtb.Document.GetLocalValueEnumerator();
                    ApplyProperties(globalProperties, outerTextRange);
                    GenerateParagraphs(paras, resultDocument);
                }
                else
                {
                    if (EnableConvertUrl)
                    {
                        string[] lines = content.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
                        var newLines = new List<string>();

                        // Remove last empty lines
                        int i = lines.Length - 1;
                        while (i >= 0)
                        {
                            string line = lines[i];
                            if (!string.IsNullOrEmpty(line) && line.Trim() != string.Empty)
                            {
                                break;
                            }

                            i--;
                        }
                        newLines.AddRange(lines.Take(i + 1));

                        var flowDocument = new FlowDocument();
                        foreach (string line in newLines)
                        {
                            var graph = new Paragraph();

                            string[] words = line.Split(new[] { ' ' }, StringSplitOptions.None);

                            // Seperate words correctly, adjecent spaces is grouped into one word
                            IEnumerable<string> segments = GenerateSegments(words);

                            // Generate UI element for flow document
                            foreach (string word in segments)
                            {
                                Inline inline;
                                if (IsUrlMatched(word))
                                {
                                    inline = new Hyperlink(new Run(word));
                                    (inline as Hyperlink).NavigateUri = new Uri(word, UriKind.RelativeOrAbsolute);
                                    (inline as Hyperlink).Foreground = Brushes.Blue;
                                }
                                else
                                {
                                    inline = new Run(word);
                                }

                                graph.Inlines.Add(inline);
                            }

                            flowDocument.Blocks.Add(graph);
                        }

                        RichTextControl.Document = flowDocument;
                    }
                    else
                    {
                        range.Text = content;
                    }
                }
            }
            else if (content == string.Empty)
            {
                new TextRange(RichTextControl.Document.ContentStart, RichTextControl.Document.ContentEnd) { Text = string.Empty };
            }
        }

        private IEnumerable<string> GenerateSegments(IEnumerable<string> words)
        {
            var segments = new List<string>();
            bool needToMakeNewSegment = true;
            foreach (string word in words)
            {
                if (IsUrlMatched(word))
                {
                    segments.Add(word);
                    needToMakeNewSegment = true;
                }
                else
                {
                    if (needToMakeNewSegment)
                    {
                        if (segments.Count > 0)
                        {
                            string segment = segments[segments.Count - 1];
                            if (segment.Contains(' '))
                            {
                                segment = segment.Substring(0, segment.Length - 1);
                                segments[segments.Count - 1] = segment;
                            }
                        }

                        segments.Add(segments.Count > 0 ? string.Format(" {0} ", word) : string.Format("{0} ", word));
                        needToMakeNewSegment = false;
                    }
                    else
                    {
                        segments[segments.Count - 1] += word + " ";
                    }
                }
            }

            if (segments.Count > 0)
            {
                string segment = segments[segments.Count - 1];
                if (segment.Contains(' '))
                {
                    segment = segment.Substring(0, segment.Length - 1);
                    segments[segments.Count - 1] = segment;
                }
            }

            return segments;
        }

        private bool IsParagraphEmpty(Paragraph paragraph)
        {
            var range = new TextRange(paragraph.ContentStart, paragraph.ContentEnd);
            return range.IsEmpty || string.IsNullOrEmpty(range.Text.Trim());
        }

        private Block[] RemoveLastEmptyBlocks(Block[] blocks)
        {
            // Count last empty lines
            IEnumerable<Block> reversedBlocks = blocks.Reverse();
            int i = reversedBlocks.Cast<Paragraph>().TakeWhile(IsParagraphEmpty).Count();

            // Remove last empty lines
            if (i > 0)
            {
                blocks = blocks.Take(blocks.Length - i).ToArray();
            }

            return blocks;
        }

        private void ApplyProperties(LocalValueEnumerator properties, TextRange textRange)
        {
            while (properties.MoveNext())
            {
                LocalValueEntry property = properties.Current;
                DependencyProperty dp = property.Property;
                object value = property.Value;

                if (dp != null && !dp.ReadOnly &&
                    (dp == TextElement.BackgroundProperty ||
                     dp == TextElement.FontFamilyProperty ||
                     dp == TextElement.FontSizeProperty ||
                     dp == TextElement.FontStretchProperty ||
                     dp == TextElement.FontWeightProperty ||
                     dp == TextElement.ForegroundProperty ||
                     dp == TextElement.FontStyleProperty ||
                     dp == Inline.TextDecorationsProperty))
                {
                    textRange.ApplyPropertyValue(dp, value);
                }
            }
        }

        private bool IsUrlMatched(string word)
        {
            MatchCollection matches = _urlRegex.Matches(word);
            bool isMatched = (matches.Count == 1 && matches[0].Value.Equals(word, StringComparison.OrdinalIgnoreCase));
            if (!isMatched)
            {
                matches = _absoluteRegex.Matches(word);
                isMatched = (matches.Count == 1 && matches[0].Value.Equals(word, StringComparison.OrdinalIgnoreCase));
            }

            return isMatched;
        }

        private void GenerateParagraphs(IEnumerable<Block> paras, FlowDocument resultDocument)
        {
            foreach (Paragraph para in paras)
            {
                var resultParagraph = new Paragraph();

                InlineCollection paraChildren = para.Inlines;
                var inlines = new Inline[paraChildren.Count];
                paraChildren.CopyTo(inlines, 0);

                foreach (Inline inline in inlines)
                {
                    if (inline is Run)
                    {
                        var run = inline as Run;
                        LocalValueEnumerator localProperties = run.GetLocalValueEnumerator();
                        string runText = run.Text;
                        string[] words = runText.Split(new[] { ' ' }, StringSplitOptions.None);

                        // Seperate words correctly, adjecent spaces is grouped into one word
                        IEnumerable<string> segments = GenerateSegments(words);

                        // Generate UI element for flow document
                        foreach (string word in segments)
                        {
                            var innerRun = new Run(word);
                            var tmpRtb = new RichTextBox { Document = new FlowDocument() };
                            tmpRtb.Document.Blocks.Add(new Paragraph(innerRun));

                            var innerTextRange = new TextRange(innerRun.ContentStart, innerRun.ContentEnd);
                            ApplyProperties(localProperties, innerTextRange);

                            Inline newInline;
                            if (IsUrlMatched(word))
                            {
                                newInline = new Hyperlink(innerRun);
                                (newInline as Hyperlink).NavigateUri = new Uri(word, UriKind.RelativeOrAbsolute);
                                (newInline as Hyperlink).Foreground = Brushes.Blue;
                            }
                            else
                            {
                                newInline = innerRun;
                            }

                            resultParagraph.Inlines.Add(newInline);
                        }
                    }
                    else if (inline is Hyperlink)
                    {
                        var range = new TextRange(inline.ContentStart, inline.ContentEnd);
                        (inline as Hyperlink).NavigateUri = new Uri(range.Text, UriKind.RelativeOrAbsolute);
                        (inline as Hyperlink).Foreground = Brushes.Blue;
                        resultParagraph.Inlines.Add(inline);
                    }
                    else
                    {
                        resultParagraph.Inlines.Add(inline);
                    }
                }

                resultDocument.Blocks.Add(resultParagraph);
            }
        }

        private void GetContent()
        {
            if (!ShowReadOnlyMode)
            {
                _internalChanges = true;
                var range = new TextRange(RichTextControl.Document.ContentStart, RichTextControl.Document.ContentEnd);
                if (string.IsNullOrEmpty(range.Text.Replace(Environment.NewLine, string.Empty).Trim()))
                {
                    XamlContent = string.Empty;
                    return;
                }

                if (EnableConvertUrl)
                {
                    var stream1 = new MemoryStream();
                    range.Save(stream1, DataFormats.Xaml);
                    var reader1 = new StreamReader(stream1);
                    stream1.Position = 0;
                    reader1.ReadToEnd();

                    BlockCollection blocks = RichTextControl.Document.Blocks;
                    var paras = new Block[blocks.Count];
                    blocks.CopyTo(paras, 0);

                    // Count last empty lines
                    IEnumerable<Block> reversedParas = paras.Reverse();
                    int i = reversedParas.Cast<Paragraph>().TakeWhile(IsParagraphEmpty).Count();

                    // Remove last empty lines
                    if (i > 0)
                    {
                        paras = paras.Take(paras.Length - i).ToArray();
                    }

                    var resultDocument = new FlowDocument();
                    var outerTextRange = new TextRange(resultDocument.ContentStart, resultDocument.ContentEnd);
                    LocalValueEnumerator globalProperties = RichTextControl.Document.GetLocalValueEnumerator();
                    ApplyProperties(globalProperties, outerTextRange);
                    GenerateParagraphs(paras, resultDocument);

                    range = new TextRange(resultDocument.ContentStart, resultDocument.ContentEnd);
                    var stream = new MemoryStream();
                    range.Save(stream, DataFormats.Xaml);
                    var reader = new StreamReader(stream);
                    stream.Position = 0;
                    XamlContent = reader.ReadToEnd();
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
        }

        private void TextAnnotationSizeChanged(object sender, SizeChangedEventArgs e)
        {
            RichTextControl.Width = e.NewSize.Width;
            RichTextControl.Height = e.NewSize.Height - toolBar.ActualHeight;
            RichTextControl.Document.PageWidth = e.NewSize.Width;
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

        private void RichTextControlSelectionChanged(object sender, RoutedEventArgs e)
        {
            // Only apply properties if we show toolbar
            if (!HideToolbarMode)
            {
                var selectionRange = new TextRange(RichTextControl.Selection.Start, RichTextControl.Selection.End);
                ToolStripButtonBold.IsChecked = selectionRange.GetPropertyValue(FontWeightProperty).ToString() == "Bold";
                ToolStripButtonItalic.IsChecked = selectionRange.GetPropertyValue(FontStyleProperty).ToString() == "Italic";
                ToolStripButtonUnderline.IsChecked = selectionRange.GetPropertyValue(Inline.TextDecorationsProperty) == TextDecorations.Underline;
                cboFontSize.SelectedValue = selectionRange.GetPropertyValue(FlowDocument.FontSizeProperty).ToString();
            }
        }

        private void RichTextControlKeyDown(object sender, KeyEventArgs e)
        {
            var range = new TextRange(RichTextControl.Document.ContentStart, RichTextControl.Document.ContentEnd);
            string text = range.Text;
            int endNewLineIndex = text.LastIndexOf(Environment.NewLine);
            if (endNewLineIndex != -1)
            {
                text = text.Replace(Environment.NewLine, string.Empty);
            }

            if (text.Length >= MaxTextLength && (RichTextControl.Selection.IsEmpty || AreNewLinesOnly(RichTextControl.Selection.Text)))
            {
                e.Handled = true;
            }
            else
            {
                string fontSize = cboFontSize.SelectedValue + string.Empty;
                if (!string.IsNullOrEmpty(fontSize))
                {
                    RichTextControl.Selection.ApplyPropertyValue(TextElement.FontSizeProperty, fontSize);
                }

                RichTextControl.Selection.ApplyPropertyValue(TextElement.ForegroundProperty, SelectedColor);
            }
        }

        private void RichTextControlKeyUp(object sender, KeyEventArgs e)
        {
            // Disable formatting data (such as Bold, Italic, Underline) if we hide toolbar
            if (!HideToolbarMode)
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
                    ToolStripButtonUnderline.IsChecked = !ToolStripButtonUnderline.IsChecked;
                    e.Handled = true;
                }
            }
        }

        private void RichTextControlTextChanged(object sender, TextChangedEventArgs e)
        {
            GetContent();
        }

        private void RemoveToolbarOverflow()
        {
            var overflowGrid = toolBar.Template.FindName("OverflowGrid", toolBar) as FrameworkElement;
            if (overflowGrid != null)
            {
                overflowGrid.Visibility = Visibility.Collapsed;
            }
        }

        private void ToolBarSizeChanged(object sender, SizeChangedEventArgs e)
        {
            RemoveToolbarOverflow();
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

        private void ChangeColor(SolidColorBrush brush)
        {
            RichTextControl.Selection.ApplyPropertyValue(ForegroundProperty, brush);
            RichTextControl.Focus();

            GetContent();
        }

        private void HideToolbar(bool hideToolbar)
        {
            if (hideToolbar)
            {
                toolBar.Visibility = Visibility.Collapsed;

                // Disable making the selected text to be bold
                var command = new CommandBinding();
                command.CanExecute += BlockTheCommand;
                command.Command = EditingCommands.ToggleBold;
                RichTextControl.CommandBindings.Add(command);

                // Disable making the selected text to be italic
                command = new CommandBinding();
                command.CanExecute += BlockTheCommand;
                command.Command = EditingCommands.ToggleItalic;
                RichTextControl.CommandBindings.Add(command);

                // Disable making the selected text to be underline
                command = new CommandBinding();
                command.CanExecute += BlockTheCommand;
                command.Command = EditingCommands.ToggleUnderline;
                RichTextControl.CommandBindings.Add(command);
            }
        }

        private void BlockTheCommand(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = false;
            e.Handled = true;
        }

        private void TextAnnotationRequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            string url = e.Uri.IsAbsoluteUri ? e.Uri.ToString() : "http://" + e.Uri;
            if (e.Uri.IsAbsoluteUri && e.Uri.Scheme.ToLower() == "file")
            {
                System.Diagnostics.Process.Start("explorer.exe", url);
            }
            else
            {
                System.Diagnostics.Process.Start(url);
            }

            e.Handled = true;
        }

        private void RichTextControlPasting(object sender, DataObjectPastingEventArgs e)
        {
            if (e.SourceDataObject.GetDataPresent(DataFormats.Text))
            {
                // Populate the remaining characters that we can add
                int remainingLength = GetRemainingLength();

                string text = (string)e.SourceDataObject.GetData(DataFormats.Text) ?? string.Empty;
                string[] lines = text.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
                string[] reversedLines = lines.Reverse().ToArray();
                int emptyLinesCount = reversedLines.TakeWhile(line => line.Trim() == string.Empty).Count();

                if (emptyLinesCount > 0)
                {
                    lines = lines.Take(lines.Length - emptyLinesCount).ToArray();
                }

                string trimedText = string.Empty;
                string tempText = string.Empty;

                foreach (string line in lines)
                {
                    int tmpLength = remainingLength - tempText.Length;
                    if (tmpLength <= 0)
                    {
                        break;
                    }

                    string token = line.Substring(0, line.Length <= tmpLength ? line.Length : tmpLength);
                    tempText += token;
                    trimedText += token + Environment.NewLine;
                }

                if (trimedText.Length > 0)
                {
                    trimedText = trimedText.Substring(0, trimedText.LastIndexOf(Environment.NewLine));
                }

                if (trimedText.Length == 0)
                {
                    e.CancelCommand();
                }
                else
                {
                    e.DataObject = new DataObject(DataFormats.Text, trimedText);
                }
            }
        }

        private int GetRemainingLength()
        {
            var globalRange = new TextRange(RichTextControl.Document.ContentStart, RichTextControl.Document.ContentEnd);
            int globalLength = globalRange.IsEmpty ? 0 : globalRange.Text.Replace(Environment.NewLine, string.Empty).Length;
            int selectionLength = RichTextControl.Selection.IsEmpty ? 0 : RichTextControl.Selection.Text.Replace(Environment.NewLine, string.Empty).Length;
            return MaxTextLength - globalLength + selectionLength;
        }

        private bool AreNewLinesOnly(string text)
        {
            string[] tokens = text.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            return tokens.Count() == 0;
        }

        private bool _internalChanges;
        private double _backupZoomRatio = 1;

        // There are many possible forms of URL such as file
        // URL is often separated into two types of Absolute and Relative
        // The general syntax of absolute URLs is the following:
        //          scheme://host:port/path/filename
        // Relative URL often misses the scheme part:
        //          www.yahoo.com
        //          //localhost/share
        private readonly Regex _urlRegex = new Regex(@"(?#Protocol)(?:(?:ht|f)tp(?:s?)\:\/\/|~/|/)?(?#Username:Password)(?:\w+:\w+@)?(?#Subdomains)(?:(?:[-\w]+\.)+(?#TopLevel Domains)(?:com|org|net|gov|mil|biz|info|mobi|name|aero|jobs|museum|travel|[a-z]{2,3}))(?#Port)(?::[\d]{1,5})?(?#Directories)(?:(?:(?:/(?:[-\w~!$+|.,=]|%[a-f\d]{2})+)+|/)+|\?|#)?(?#Query)(?:(?:\?(?:[-\w~!$+|.,*:]|%[a-f\d{2}])+=(?:[-\w~!$+|.,*:=]|%[a-f\d]{2})*)(?:&(?:[-\w~!$+|.,*:]|%[a-f\d{2}])+=(?:[-\w~!$+|.,*:=]|%[a-f\d]{2})*)*)*(?#Anchor)(?:#(?:[-\w~!$+|.,*:=]|%[a-f\d]{2})*)?");
        private readonly Regex _absoluteRegex = new Regex(@"^(http|https|ftp|file)\://([a-zA-Z0-9\.\-]+(\:[a-zA-Z0-9\.&amp;%\$\-]+)*@)*((25[0-5]|2[0-4][0-9]|[0-1]{1}[0-9]{2}|[1-9]{1}[0-9]{1}|[1-9])\.(25[0-5]|2[0-4][0-9]|[0-1]{1}[0-9]{2}|[1-9]{1}[0-9]{1}|[1-9]|0)\.(25[0-5]|2[0-4][0-9]|[0-1]{1}[0-9]{2}|[1-9]{1}[0-9]{1}|[1-9]|0)\.(25[0-5]|2[0-4][0-9]|[0-1]{1}[0-9]{2}|[1-9]{1}[0-9]{1}|[0-9])|localhost|([a-zA-Z]+[a-zA-Z0-9\-]*)|([a-zA-Z0-9\-]+\.)*[a-zA-Z0-9\-]+\.(com|edu|gov|int|mil|net|org|biz|arpa|info|name|pro|aero|coop|museum|[a-zA-Z]{2,3}))(\:[0-9]+)*(/($|[a-zA-Z0-9\.\,\?\'\\\+&amp;%\$#\=~_\-]+))*$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    }
}