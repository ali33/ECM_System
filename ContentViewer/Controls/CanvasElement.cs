using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;

using Ecm.ContentViewer.Helper;
using Ecm.ContentViewer.Model;
using Ecm.Model;
using Ecm.ContentViewer.Model;
using Ecm.ContentViewer.View;

namespace Ecm.ContentViewer.Controls
{
    /// <summary>
    ///   Present for the container of an image
    /// </summary>
    public class CanvasElement : Canvas
    {
        public static readonly DependencyProperty EnableHideAnnotationProperty = DependencyProperty.Register("EnableHideAnnotation", typeof(bool), typeof(CanvasElement),
            new FrameworkPropertyMetadata(false, EnableHideAnnotationChangedCallback));

        public static readonly DependencyProperty EnableHighlightProperty = DependencyProperty.Register("EnableHighlight", typeof(bool), typeof(CanvasElement));

        public static readonly DependencyProperty EnableLineProperty = DependencyProperty.Register("EnableLine", typeof(bool), typeof(CanvasElement));

        public static readonly DependencyProperty EnableRedactionProperty = DependencyProperty.Register("EnableRedaction", typeof(bool), typeof(CanvasElement));

        public static readonly DependencyProperty EnableOCRZoneProperty = DependencyProperty.Register("EnableOCRZone", typeof(bool), typeof(CanvasElement));

        public static readonly DependencyProperty EnableSelectionProperty = DependencyProperty.Register("EnableSelection", typeof(bool), typeof(CanvasElement),
            new FrameworkPropertyMetadata(false, EnableSelectionChangedCallback));

        public static readonly DependencyProperty EnableTextProperty = DependencyProperty.Register("EnableText", typeof(bool), typeof(CanvasElement));

        public static readonly DependencyProperty IsChangedProperty = DependencyProperty.Register("IsChanged", typeof(bool), typeof(CanvasElement));

        /// <summary>
        /// Use to create image when capturing which have enough 
        /// </summary>
        public CanvasElement(string imageFile, PageModel pageInfo, ContentViewerPermission permission, MainViewer mainViewer)
        {
            var image = new BitmapImage();
            image.BeginInit();
            image.CacheOption = BitmapCacheOption.OnLoad;
            image.CreateOptions = BitmapCreateOptions.IgnoreColorProfile;
            image.UriSource = new Uri("file:///" + imageFile, UriKind.RelativeOrAbsolute);
            image.EndInit();
            image.Freeze();
            PageInfo = pageInfo;
            Permission = permission;

            DpiX = image.DpiX;
            DpiY = image.DpiY;
            ImageFile = imageFile;
            Background = new ImageBrush(image) { RelativeTransform = new RotateTransform(0, 0.5, 0.5) };
            Width = pageInfo.Width = image.Width;
            Height = pageInfo.Height = image.Height;
            var key = new ComponentResourceKey(typeof(CanvasElement), "ItemCanvasStyle");
            Style = (Style)TryFindResource(key);
            MainViewer = mainViewer;

            _rotationTool = new RotationToolHelper(this);
            _drawingTool = new DrawingHelper(this, OnStartSelection, mainViewer);
            _selectionTool = new SelectionToolHelper(this, OnStartSelection);
            RenderOptions.SetBitmapScalingMode(this, BitmapScalingMode.HighQuality);
            CreatePageNumber();
            PopulateAnnotations();
            ShowPageNumber();
        }

        public CanvasElement(byte[] binary, PageModel pageInfo, ContentViewerPermission permission, MainViewer mainViewer)
        {
            var image = new BitmapImage();
            using (var stream = new MemoryStream(binary))
            {
                image.BeginInit();
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.CreateOptions = BitmapCreateOptions.IgnoreColorProfile;
                image.StreamSource = stream;
                image.EndInit();
            }

            PageInfo = pageInfo;
            if (pageInfo.Width == 0)
            {
                pageInfo.Width = image.Width;
            }

            if (pageInfo.Height == 0)
            {
                pageInfo.Height = image.Height;
            }

            Permission = permission;

            DpiX = image.DpiX;
            DpiY = image.DpiY;
            ImageBinary = binary;
            Background = new ImageBrush(image) { RelativeTransform = new RotateTransform(pageInfo.RotateAngle, 0.5, 0.5) };
            Width = pageInfo.Width;
            Height = pageInfo.Height;
            var key = new ComponentResourceKey(typeof(CanvasElement), "ItemCanvasStyle");
            Style = (Style)TryFindResource(key);
            MainViewer = mainViewer;

            _rotationTool = new RotationToolHelper(this);
            _drawingTool = new DrawingHelper(this, OnStartSelection, MainViewer);
            _selectionTool = new SelectionToolHelper(this, OnStartSelection);
            RenderOptions.SetBitmapScalingMode(this, BitmapScalingMode.HighQuality);
            CreatePageNumber();
            PopulateAnnotations();
            ShowPageNumber();
        }

        /// <summary>
        /// This constructor used to create the CanvasElement on memory which is used for print/download/email without showing on UI
        /// </summary>
        public CanvasElement(byte[] binary, PageModel pageInfo, ContentViewerPermission permission)
        {
            var image = new BitmapImage();
            using (var stream = new MemoryStream(binary))
            {
                image.BeginInit();
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.CreateOptions = BitmapCreateOptions.IgnoreColorProfile;
                image.StreamSource = stream;
                image.EndInit();
            }

            PageInfo = pageInfo;
            if (pageInfo.Width == 0)
            {
                pageInfo.Width = image.Width;
            }

            if (pageInfo.Height == 0)
            {
                pageInfo.Height = image.Height;
            }

            Permission = permission;

            DpiX = image.DpiX;
            DpiY = image.DpiY;
            ImageBinary = binary;
            Background = new ImageBrush(image) { RelativeTransform = new RotateTransform(pageInfo.RotateAngle, 0.5, 0.5) };
            Width = pageInfo.Width;
            Height = pageInfo.Height;
            RenderOptions.SetBitmapScalingMode(this, BitmapScalingMode.HighQuality);
            CreatePageNumber();
            PopulateAnnotations();
        }

        public CanvasElement(Uri imageUri, MainViewer mainViewer)
        {
            var image = new BitmapImage();
            image.BeginInit();
            image.CacheOption = BitmapCacheOption.OnLoad;
            image.CreateOptions = BitmapCreateOptions.IgnoreColorProfile;
            image.UriSource = imageUri;
            image.EndInit();
            image.Freeze();

            DpiX = image.DpiX;
            DpiY = image.DpiY;
            Background = new ImageBrush(image) { RelativeTransform = new RotateTransform(0, 0.5, 0.5) };
            Width = image.Width;
            Height = image.Height;
            var key = new ComponentResourceKey(typeof(CanvasElement), "ItemCanvasStyle");
            Style = (Style)TryFindResource(key);
            MainViewer = mainViewer;

            _rotationTool = new RotationToolHelper(this);
            _drawingTool = new DrawingHelper(this, OnStartSelection, mainViewer);
            _selectionTool = new SelectionToolHelper(this, OnStartSelection);
            RenderOptions.SetBitmapScalingMode(this, BitmapScalingMode.HighQuality);
            CreatePageNumber();
            PopulateAnnotations();
            ShowPageNumber();
        }

        private CanvasElement()
        {
            CreatePageNumber();
            HidePageNumber();
        }

        public event EventHandler ContentChanged;

        public event EventHandler MouseDoubleClick;

        public event EventHandler StartSelection;

        public Size ActualItemSize
        {
            get
            {
                return new Size(
                    Width + _itemContainer.BorderThickness.Left + _itemContainer.BorderThickness.Right,
                    Height + _itemContainer.BorderThickness.Top + _itemContainer.BorderThickness.Bottom);
            }
        }

        public MainViewer MainViewer { get; private set; }

        public double CurrentZoomRatio
        {
            get
            {
                return _currentZoomRatio;
            }
        }

        public double DpiX { get; private set; }

        public double DpiY { get; private set; }

        public bool EnableHideAnnotation
        {
            get
            {
                return (bool)GetValue(EnableHideAnnotationProperty);
            }
            set
            {
                SetValue(EnableHideAnnotationProperty, value);
            }
        }

        public bool EnableHighlight
        {
            get
            {
                return (bool)GetValue(EnableHighlightProperty);
            }
            set
            {
                SetValue(EnableHighlightProperty, value);
            }
        }

        public bool EnableLine
        {
            get
            {
                return (bool)GetValue(EnableLineProperty);
            }
            set
            {
                SetValue(EnableLineProperty, value);
            }
        }

        public bool EnableRedaction
        {
            get
            {
                return (bool)GetValue(EnableRedactionProperty);
            }
            set
            {
                SetValue(EnableRedactionProperty, value);
            }
        }

        public bool EnableSelection
        {
            get
            {
                return (bool)GetValue(EnableSelectionProperty);
            }
            set
            {
                SetValue(EnableSelectionProperty, value);
            }
        }

        public bool EnableText
        {
            get
            {
                return (bool)GetValue(EnableTextProperty);
            }
            set
            {
                SetValue(EnableTextProperty, value);
            }
        }

        public bool EnableOCRZone
        {
            get { return (bool) GetValue(EnableOCRZoneProperty); }
            set { SetValue(EnableOCRZoneProperty, value); }
        }

        public byte[] ImageBinary { get; set; }

        public string ImageFile { get; set; }

        public bool IsChanged
        {
            get
            {
                return (bool)GetValue(IsChangedProperty);
            }
            set
            {
                SetValue(IsChangedProperty, value);
            }
        }

        public Border ItemContainer
        {
            get
            {
                if (_itemContainer == null)
                {
                    var color = ColorConverter.ConvertFromString("#a5acb5");
                    _itemContainer = new Border
                    {
                        BorderBrush = new SolidColorBrush((Color)color),
                        BorderThickness = new Thickness(1),
                        Child = this,
                        Effect = new DropShadowEffect { BlurRadius = 0, Color = Colors.Gray, ShadowDepth = 3, Direction = -45 }
                    };
                }

                return _itemContainer;
            }
        }

        public Size OriginalItemSize
        {
            get
            {
                return new Size(PageInfo.Width + _itemContainer.BorderThickness.Left + _itemContainer.BorderThickness.Right,
                                PageInfo.Height + _itemContainer.BorderThickness.Top + _itemContainer.BorderThickness.Bottom);
            }
        }

        public IEnumerable<AnnotationControl> SelectedItems
        {
            get
            {
                IEnumerable<AnnotationControl> selectedItems = from item in Children.OfType<AnnotationControl>()
                                                             where item.IsSelected
                                                             select item;

                return selectedItems;
            }
        }

        public bool IsNonImagePreview { get; set; }

        public PageModel PageInfo { get; set; }

        public ContentViewerPermission Permission { get; private set; }

        public void Clean()
        {
            if (Background != null)
            {
                var imageBrush = Background as ImageBrush;
                if (imageBrush != null)
                {
                    imageBrush.ImageSource = null;
                }

                Background = null;
            }

            Children.Clear();
        }

        public CanvasElement Clone()
        {
            var clonedItem = new CanvasElement
                                 {
                                     DpiX = DpiX,
                                     DpiY = DpiY,
                                     ImageFile = ImageFile,
                                     Background = Background,
                                     Width = PageInfo.Width,
                                     Height = PageInfo.Height,
                                     Permission = Permission,
                                     PageInfo = PageInfo
                                 };

            clonedItem.PopulateAnnotations();
            var key = new ComponentResourceKey(typeof(CanvasElement), "ItemCanvasStyle");
            clonedItem.Style = (Style)TryFindResource(key);
            clonedItem._rotationTool = new RotationToolHelper(clonedItem);
            clonedItem._drawingTool = new DrawingHelper(clonedItem, OnStartSelection, MainViewer);
            clonedItem._selectionTool = new SelectionToolHelper(clonedItem, OnStartSelection);
            RenderOptions.SetBitmapScalingMode(clonedItem, BitmapScalingMode.HighQuality);

            return clonedItem;
        }

        public void DeselectAll()
        {
            List<AnnotationControl> children = Children.OfType<AnnotationControl>().ToList();
            foreach (AnnotationControl item in children)
            {
                item.UnSelect();
            }
        }

        public void EnableDrawingMode(bool enable)
        {
            if (enable)
            {
                _drawingTool.EnableDrawing();
            }
            else
            {
                _drawingTool.RemoveDrawing();
            }
        }

        public AnnotationControl GetAnnotationShape(Point mousePosition)
        {
            for (int i = Children.Count - 1; i >= 0; i--)
            {
                if (Children[i] is AnnotationControl)
                {
                    var item = Children[i] as AnnotationControl;
                    if (item != null)
                    {
                        var itemRect = VisualTreeHelper.GetDescendantBounds(item);
                        var itemBounds = item.TransformToAncestor(this).TransformBounds(itemRect);

                        if (itemBounds.Contains(mousePosition))
                        {
                            return item;
                        }
                    }
                }
            }

            return null;
        }

        public void HidePageNumber()
        {
            if (_pageTextContainer != null)
            {
                _pageTextContainer.Visibility = Visibility.Collapsed;
            }
        }

        public void ShowPageNumber()
        {
            if (_pageTextContainer != null)
            {
                _pageTextContainer.Visibility = Visibility.Visible;
            }
        }

        public void SetPermission(ContentViewerPermission permission)
        {
            Permission = permission;
            PopulateAnnotations();
        }

        public void OnContentChanged()
        {
            IsChanged = true;
            if (ContentChanged != null)
            {
                ContentChanged(this, EventArgs.Empty);
            }
        }

        public void ShowPageText(int pageIndex, int totalPage)
        {
            _pageText.Text = (pageIndex + 1).ToString();
            if (totalPage > 0)
            {
                _pageText.Text = string.Format("{0} of {1}", pageIndex + 1, totalPage);
            }

            PositionPageText();
        }

        public void RemoveToolBarForShapes()
        {
            List<AnnotationControl> shapes = Children.OfType<AnnotationControl>().Where(p => p.IsSelected).ToList();
            if (shapes.Count > 0)
            {
                foreach (AnnotationControl shape in shapes)
                {
                    shape.ShowToolbar = false;
                }
            }
        }

        public void RotateLeft()
        {
            _rotationTool.RotateLeft();
            PositionPageText();
        }

        public void RotateRight()
        {
            _rotationTool.RotateRight();
            PositionPageText();
        }

        public void Zoom(double ratio)
        {
            Width = (Width / _currentZoomRatio) * ratio;
            Height = (Height / _currentZoomRatio) * ratio;
            List<AnnotationControl> items = Children.OfType<AnnotationControl>().ToList();
            foreach (AnnotationControl item in items)
            {
                item.Zoom(ratio);
            }

            // Change font-size for page text
            _pageText.FontSize = (_pageText.FontSize / _currentZoomRatio) * ratio;
            PositionPageText();

            _currentZoomRatio = ratio;
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            Loaded += ItemCanvasLoaded;
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left && e.ClickCount == 2)
            {
                if (MouseDoubleClick != null)
                {
                    MouseDoubleClick(this, e);
                }
            }

            base.OnMouseDown(e);
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
            if (e.Property.Name == "Width" || e.Property.Name == "Height")
            {
                if (_itemContainer != null)
                {
                    _itemContainer.Width = Width + _itemContainer.BorderThickness.Left + _itemContainer.BorderThickness.Right;
                    _itemContainer.Height = Height + _itemContainer.BorderThickness.Top + _itemContainer.BorderThickness.Bottom;
                }
            }
        }

        private static void EnableHideAnnotationChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var itemCanvas = d as CanvasElement;
            if (itemCanvas != null)
            {
                itemCanvas.HideAllAnnotations((bool)e.NewValue);
            }
        }

        private static void EnableSelectionChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var itemCanvas = d as CanvasElement;
            if (itemCanvas != null)
            {
                itemCanvas.EnableSelectionMode((bool)e.NewValue);
            }
        }

        private void CreatePageNumber()
        {
            _pageTextContainer = new Border
                                     {
                                         Background = new BrushConverter().ConvertFrom("#cdcdcd") as Brush, 
                                         Opacity = 0.5f
                                     };
            Children.Add(_pageTextContainer);

            // Initialize page number
            _pageText = new TextBlock
            {
                FontFamily = new FontFamily("Arial"),
                FontWeight = FontWeight.FromOpenTypeWeight(400),
                FontSize = 30,
                Foreground = new BrushConverter().ConvertFrom("#000000") as Brush,
                Opacity = 0.8f, 
                Margin = new Thickness(4,2,4,2)
            };

            _pageTextContainer.Child = _pageText;
        }

        private void AnnotationShapeChanged(object sender, EventArgs e)
        {
            OnContentChanged();
        }

        private void EnableSelectionMode(bool enable)
        {
            List<AnnotationControl> shapes = Children.OfType<AnnotationControl>().ToList();
            foreach (AnnotationControl item in shapes)
            {
                item.EnableSelection = enable;
            }

            if (enable)
            {
                _selectionTool.EnableSelection();
            }
            else
            {
                _selectionTool.RemoveSelection();
            }
        }

        private void HideAllAnnotations(bool hide)
        {
            List<AnnotationControl> shapes = Children.OfType<AnnotationControl>().ToList();
            foreach (AnnotationControl shape in shapes)
            {
                if (Permission.CanHideAnnotation || MainViewer.DocViewerMode == DocViewerMode.Capture || MainViewer.DocViewerMode == DocViewerMode.LightCapture)
                {
                    if (hide)
                    {
                        shape.Visibility = Visibility.Hidden;
                        shape.IsSelected = false;
                    }
                    else
                    {
                        shape.Visibility = Visibility.Visible;
                    }
                }
            }
        }

        private void ItemCanvasKeyDown(object sender, KeyEventArgs e)
        {
            List<AnnotationControl> shapes = Children.OfType<AnnotationControl>().Where(p => p.IsSelected).ToList();
            if (e.Key == Key.F2)
            {
                e.Handled = true;
                foreach (var shape in shapes)
                {
                    if (shape.AnnotationInfo.Type == AnnotationTypeModel.Text)
                    {
                        shape.ShowTextBox();
                    }
                }
            }
            else if (e.Key == Key.Delete)
            {
                e.Handled = true;
                foreach (AnnotationControl shape in shapes)
                {
                    if ((shape.AnnotationInfo.Type == AnnotationTypeModel.Highlight && Permission.CanDeleteHighlight) ||
                        (shape.AnnotationInfo.Type == AnnotationTypeModel.Redaction && Permission.CanDeleteRedaction) ||
                        (shape.AnnotationInfo.Type == AnnotationTypeModel.Text && Permission.CanDeleteText) ||
                        (shape.AnnotationInfo.Type == AnnotationTypeModel.Line && Permission.CanDeleteLine) ||
                        shape.IsNew || shape.AnnotationInfo.Type == AnnotationTypeModel.OCRZone)
                    {
                        PageInfo.Annotations.Remove(shape.AnnotationInfo);
                        Children.Remove(shape);
                        OnContentChanged();
                        AnnotationControl.ValidateOCRZone(this);
                    }
                }
            }
            else if (e.Key == Key.A && Keyboard.Modifiers == ModifierKeys.Control)
            {
                e.Handled = true;
                foreach (AnnotationControl shape in Children.OfType<AnnotationControl>())
                {
                    shape.Select();
                }

                RemoveToolBarForShapes();
            }
            else if (e.Key == Key.Escape)
            {
                e.Handled = true;
                foreach (AnnotationControl shape in shapes)
                {
                    shape.UnSelect();
                }
            }
        }

        private void ItemCanvasLoaded(object sender, RoutedEventArgs e)
        {
            if (!_isContentLoaded)
            {
                _isContentLoaded = true;
                Focusable = true;
                KeyDown += ItemCanvasKeyDown;
                ApplyAnnotationStyle();
            }
        }

        private void OnStartSelection()
        {
            Focus();
            if (StartSelection != null)
            {
                StartSelection(this, EventArgs.Empty);
            }
        }

        private void PositionPageText()
        {
            //if (_pageTextContainer != null)
            //{
            //    _pageTextContainer.UpdateLayout();
            //    SetLeft(_pageTextContainer, Width - _pageTextContainer.ActualWidth);
            //    SetTop(_pageTextContainer, Height - _pageTextContainer.ActualHeight);
            //    _pageText.Height = Double.NaN;
            //}
        }

        private void PopulateAnnotations()
        {
            // Clear all existing annotation
            var annotations = Children.OfType<AnnotationControl>().ToList();
            foreach (var annotation in annotations)
            {
                Children.Remove(annotation);
            }

            if (PageInfo != null)
            {
                var count = PageInfo.Annotations != null ? PageInfo.Annotations.Count : 0;
                for (var i = 0; i < count; i++)
                {
                    if (PageInfo.Annotations != null)
                    {
                        AnnotationModel info = PageInfo.Annotations[i];
                        if ((info.Type == AnnotationTypeModel.Highlight && Permission.CanSeeHighlight) ||
                            (info.Type == AnnotationTypeModel.Text && Permission.CanSeeText) ||
                            info.Type == AnnotationTypeModel.Redaction ||
                            (info.Type == AnnotationTypeModel.Line && Permission.CanSeeLine) ||
                            (info.Type == AnnotationTypeModel.OCRZone && Permission.CanApplyOCRTemplate))
                        {
                            new AnnotationControl(info, this);
                        }
                    }
                }
            }
        }

        private void ApplyAnnotationStyle()
        {
            var annotations = Children.OfType<AnnotationControl>();
            foreach (var annotation in annotations)
            {
                var key = new ComponentResourceKey(typeof(CanvasElement), "AnnotationStyle");
                annotation.Style = (Style)TryFindResource(key);
                annotation.UnSelect();
                annotation.ContentChanged += AnnotationShapeChanged;
            }
        }

        private double _currentZoomRatio = 1.0f;
        private bool _isContentLoaded;
        private Border _itemContainer;
        private TextBlock _pageText;
        private Border _pageTextContainer;
        private DrawingHelper _drawingTool;
        private RotationToolHelper _rotationTool;
        private SelectionToolHelper _selectionTool;
    }
}