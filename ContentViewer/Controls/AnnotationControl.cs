using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using Ecm.CustomControl;
using Ecm.ContentViewer.Model;
using Ecm.ContentViewer.Model;

namespace Ecm.ContentViewer.Controls
{
    public class AnnotationControl : ContentControl
    {
        #region Dependency properties

        public static readonly DependencyProperty CreatedByProperty = DependencyProperty.Register("CreatedBy", typeof(string), typeof(AnnotationControl));

        public static readonly DependencyProperty CreatedOnProperty = DependencyProperty.Register("CreatedOn", typeof(DateTime), typeof(AnnotationControl));

        public static readonly DependencyProperty EnableSelectionProperty = DependencyProperty.Register("EnableSelection", typeof(bool), typeof(AnnotationControl),
            new FrameworkPropertyMetadata(false, EnableSelectionChangedCallback));

        private static void EnableSelectionChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var annotationShape = d as AnnotationControl;
            if (annotationShape != null)
            {
                annotationShape.EnableSelectionMode((bool)e.NewValue);
            }
        }

        public static readonly DependencyProperty IsChangedProperty = DependencyProperty.Register("IsChanged", typeof(bool), typeof(AnnotationControl));

        public static readonly DependencyProperty IsSelectedProperty = DependencyProperty.Register("IsSelected", typeof(bool), typeof(AnnotationControl));

        public static readonly DependencyProperty LastModifiedByProperty = DependencyProperty.Register("LastModifiedBy", typeof(string), typeof(AnnotationControl));

        public static readonly DependencyProperty LastModifiedOnProperty = DependencyProperty.Register("LastModifiedOn", typeof(DateTime), typeof(AnnotationControl));

        public static readonly DependencyProperty ReadOnlyModeProperty = DependencyProperty.Register("ReadOnlyMode", typeof(bool), typeof(AnnotationControl), new FrameworkPropertyMetadata(true));

        public static readonly DependencyProperty SelectedLineColorProperty = DependencyProperty.Register("SelectedLineColor", typeof(string), typeof(AnnotationControl));

        public static readonly DependencyProperty SelectedLineStyleProperty = DependencyProperty.Register("SelectedLineStyle", typeof(LineStyleModel), typeof(AnnotationControl));

        public static readonly DependencyProperty SelectedLineWeightProperty = DependencyProperty.Register("SelectedLineWeight", typeof(int), typeof(AnnotationControl));

        public static readonly DependencyProperty ShowToolbarProperty = DependencyProperty.Register("ShowToolbar", typeof(bool), typeof(AnnotationControl));

        public static readonly DependencyProperty ShowOCRZoneToolbarProperty = DependencyProperty.Register("ShowOCRZoneToolbar", typeof(bool), typeof(AnnotationControl));

        public static readonly DependencyProperty AnnotationInfoProperty = DependencyProperty.Register("AnnotationInfo", typeof(AnnotationModel), typeof(AnnotationControl));

        #endregion

        #region Commands

        public static RoutedUICommand SelectLineColorCommand = new RoutedUICommand("SelectLineColorCommand", "SelectLineColorCommand", typeof(AnnotationControl));

        public static RoutedUICommand SelectLineStyleCommand = new RoutedUICommand("SelectLineStyleCommand", "SelectLineStyleCommand", typeof(AnnotationControl));

        public static RoutedUICommand SelectLineWeightCommand = new RoutedUICommand("SelectLineWeightCommand", "SelectLineWeightCommand", typeof(AnnotationControl));

        #endregion

        #region Public properties and methods

        public AnnotationControl(AnnotationModel annotationInfo, CanvasElement itemCanvas)
        {
            ItemCanvasContainer = itemCanvas;
            SizeChanged += AnnotationShapeSizeChanged;
            Loaded += AnnotationShapeLoaded;
            AnnotationInfo = annotationInfo;
            ItemCanvasContainer = itemCanvas;
            _currentZoomRatio = itemCanvas.CurrentZoomRatio;
            Width = AnnotationInfo.Width * itemCanvas.CurrentZoomRatio;
            Height = AnnotationInfo.Height * itemCanvas.CurrentZoomRatio;
            LastModifiedBy = AnnotationInfo.ModifiedBy;
            LastModifiedOn = AnnotationInfo.ModifiedOn.Value;
            CreatedBy = AnnotationInfo.CreatedBy;
            CreatedOn = AnnotationInfo.CreatedOn;

            CreateContainer(Width, Height);
            itemCanvas.Children.Add(this);
            Canvas.SetTop(this, AnnotationInfo.Top * itemCanvas.CurrentZoomRatio);
            Canvas.SetLeft(this, AnnotationInfo.Left * itemCanvas.CurrentZoomRatio);

            switch (AnnotationInfo.Type)
            {
                case AnnotationTypeModel.Redaction:
                    CreateBlackBox();
                    break;
                case AnnotationTypeModel.Highlight:
                    CreateHighlightBox();
                    break;
                case AnnotationTypeModel.Text:
                    CreateTextAnnotation(!string.IsNullOrEmpty(AnnotationInfo.Content));
                    ReadOnlyMode = false;
                    break;
                case AnnotationTypeModel.Line:
                    CreateLineAnnotation();
                    InitilizeRotateVertexDictionary();
                    InitializeLineCommands();
                    break;
                case AnnotationTypeModel.OCRZone:
                    CreateOCRZone();
                    break;
            }

            if (annotationInfo != null)
            {
                annotationInfo.PropertyChanged += AnnotationInfoPropertyChanged;
            }
        }

        public void RotateLeft()
        {
            AnnotationInfo.RotateAngle -= 90;
            Rotate();
            RotateLine(false);
        }

        public void RotateRight()
        {
            AnnotationInfo.RotateAngle += 90;
            Rotate();
            RotateLine(true);
        }

        public void Select()
        {
            if ((AnnotationInfo.Type == AnnotationTypeModel.Highlight && ItemCanvasContainer.Permission.CanDeleteHighlight) ||
                (AnnotationInfo.Type == AnnotationTypeModel.Redaction && ItemCanvasContainer.Permission.CanDeleteRedaction) ||
                (AnnotationInfo.Type == AnnotationTypeModel.Text && ItemCanvasContainer.Permission.CanDeleteText) ||
                (AnnotationInfo.Type == AnnotationTypeModel.Line && ItemCanvasContainer.Permission.CanSeeLine) ||
                IsNew || AnnotationInfo.Type == AnnotationTypeModel.OCRZone)
            {
                IsSelected = true;
            }
        }

        public void ShowTextBox()
        {
            Width = AnnotationContainer.Width;
            Height = AnnotationContainer.Height;
            CreateTextAnnotation(false);
            ReadOnlyMode = false;
            EnableSelectionMode(false);
        }

        public void UnSelect()
        {
            IsSelected = false;

            if (AnnotationInfo.Type == AnnotationTypeModel.Text)
            {
                var textAnnotation = AnnotationContainer.Children[0] as TextTool;
                if (textAnnotation != null)
                {
                    if (string.IsNullOrEmpty(textAnnotation.XamlContent))
                    {
                        ItemCanvasContainer.Children.Remove(this);
                    }
                    else
                    {
                        CreateTextAnnotation(true);
                    }

                    if (AnnotationInfo.Content + string.Empty != textAnnotation.XamlContent)
                    {
                        AnnotationInfo.Content = textAnnotation.XamlContent;
                        OnContentChanged();
                    }
                }
            }
        }

        public void Zoom(double ratio)
        {
            _isActualChanged = false;
            double currentZoomRatio = _currentZoomRatio;
            _currentZoomRatio = ratio;

            Width = (Width / currentZoomRatio) * ratio;
            Height = (Height / currentZoomRatio) * ratio;

            Canvas.SetTop(this, (Canvas.GetTop(this) / currentZoomRatio) * ratio);
            Canvas.SetLeft(this, (Canvas.GetLeft(this) / currentZoomRatio) * ratio);

            if (AnnotationInfo.Type == AnnotationTypeModel.Text)
            {
                var textAnnotation = AnnotationContainer.Children[0] as TextTool;
                if (textAnnotation != null)
                {
                    textAnnotation.ZoomRatio = ratio;
                }
            }
            else if (AnnotationInfo.Type == AnnotationTypeModel.Line)
            {
                UpdateLine();
            }

            _isActualChanged = true;
        }

        public static void ValidateOCRZone(CanvasElement container)
        {
            var annotationShapes = container.Children.OfType<AnnotationControl>().Where(p => p.AnnotationInfo.Type == AnnotationTypeModel.OCRZone);
            foreach (var annotationShape in annotationShapes)
            {
                if (annotationShape.AnnotationInfo.OCRTemplateZone.FieldMetaData.Id == Guid.Empty)
                {
                    continue;
                }

                var zoneRect = (Border)annotationShape.AnnotationContainer.Children[0];
                if (annotationShapes.Any(p => p != annotationShape &&
                                              p.AnnotationInfo.OCRTemplateZone.FieldMetaData.Id ==
                                              annotationShape.AnnotationInfo.OCRTemplateZone.FieldMetaData.Id))
                {
                    zoneRect.BorderBrush = OCRInValidColor;
                }
                else
                {
                    zoneRect.BorderBrush = OCRValidColor;
                }
            }
        }

        public event EventHandler ContentChanged;

        public AnnotationModel AnnotationInfo
        {
            get { return GetValue(AnnotationInfoProperty) as AnnotationModel; }
            set { SetValue(AnnotationInfoProperty, value); }
        }

        public string CreatedBy
        {
            get
            {
                return GetValue(CreatedByProperty) + string.Empty;
            }
            set
            {
                SetValue(CreatedByProperty, value);
            }
        }

        public DateTime CreatedOn
        {
            get
            {
                return (DateTime)GetValue(CreatedOnProperty);
            }
            set
            {
                SetValue(CreatedOnProperty, value);
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

        public bool IsNew { get; set; }

        public bool IsSelected
        {
            get
            {
                return (bool)GetValue(IsSelectedProperty);
            }
            set
            {
                var textAnnotation = AnnotationContainer.Children[0] as TextTool;
                if (textAnnotation != null && (!value || (AnnotationInfo.Type == AnnotationTypeModel.Text && !textAnnotation.ShowReadOnlyMode)))
                {
                    ReadOnlyMode = false;
                }
                else if (value)
                {
                    ReadOnlyMode = true;
                }

                if (value && AnnotationInfo.Type == AnnotationTypeModel.Line)
                {
                    ShowToolbar = true;
                }
                else if (value && AnnotationInfo.Type == AnnotationTypeModel.OCRZone)
                {
                    ShowOCRZoneToolbar = true;
                }
                else
                {
                    ShowToolbar = false;
                    ShowOCRZoneToolbar = false;
                }

                SetValue(IsSelectedProperty, value);
            }
        }

        public string LastModifiedBy
        {
            get
            {
                return GetValue(LastModifiedByProperty) + string.Empty;
            }
            set
            {
                SetValue(LastModifiedByProperty, value);
            }
        }

        public DateTime LastModifiedOn
        {
            get
            {
                return (DateTime)GetValue(LastModifiedOnProperty);
            }
            set
            {
                SetValue(LastModifiedOnProperty, value);
            }
        }

        public bool ReadOnlyMode
        {
            get
            {
                return (bool)GetValue(ReadOnlyModeProperty);
            }
            set
            {
                SetValue(ReadOnlyModeProperty, value);
            }
        }

        public double RotateAngle
        {
            get
            {
                return AnnotationInfo.RotateAngle;
            }
        }

        public string SelectedLineColor
        {
            get
            {
                return (string)GetValue(SelectedLineColorProperty);
            }
            set
            {
                SetValue(SelectedLineColorProperty, value);
            }
        }

        public LineStyleModel SelectedLineStyle
        {
            get
            {
                return (LineStyleModel)GetValue(SelectedLineStyleProperty);
            }
            set
            {
                SetValue(SelectedLineStyleProperty, value);
            }
        }

        public int SelectedLineWeight
        {
            get
            {
                return (int)GetValue(SelectedLineWeightProperty);
            }
            set
            {
                SetValue(SelectedLineWeightProperty, value);
            }
        }

        public bool ShowToolbar
        {
            get
            {
                return (bool)GetValue(ShowToolbarProperty);
            }
            set
            {
                if (value && AnnotationInfo.Type != AnnotationTypeModel.Line)
                {
                    value = false;
                }

                SetValue(ShowToolbarProperty, value);
            }
        }

        public bool ShowOCRZoneToolbar
        {
            get { return (bool)GetValue(ShowOCRZoneToolbarProperty); }
            set { SetValue(ShowOCRZoneToolbarProperty, value); }
        }

        public CanvasElement ItemCanvasContainer { get; private set; }

        public Canvas AnnotationContainer { get; private set; }

        public static Brush OCRFieldNotSetColor = Brushes.Brown;
        public static Brush OCRValidColor = Brushes.SpringGreen;
        public static Brush OCRInValidColor = Brushes.Red;

        #endregion

        #region Helper methods

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);

            if (e.Property.Name == "Top")
            {
                AnnotationInfo.Top = Canvas.GetTop(this) / _currentZoomRatio;
            }

            if (e.Property.Name == "Left")
            {
                AnnotationInfo.Left = Canvas.GetLeft(this) / _currentZoomRatio;
            }

            if (e.Property.Name == "Width")
            {
                AnnotationInfo.Width = (double)e.NewValue / _currentZoomRatio;
            }

            if (e.Property.Name == "Height")
            {
                AnnotationInfo.Height = (double)e.NewValue / _currentZoomRatio;
            }

            if (e.Property.Name == "Top" || e.Property.Name == "Left" || e.Property.Name == "Width"
                || e.Property.Name == "Height" || e.Property.Name == "SelectedLineStyle"
                || e.Property.Name == "SelectedLineWeight" || e.Property.Name == "SelectedLineColor")
            {
                if (_isActualChanged)
                {
                    OnContentChanged();
                }
            }
        }

        private void AdjustSize()
        {
            if (AnnotationInfo.Type == AnnotationTypeModel.Text)
            {
                var textAnnotation = AnnotationContainer.Children[0] as TextTool;
                if (textAnnotation != null)
                {
                    var rotateTransform = textAnnotation.LayoutTransform as RotateTransform;
                    if (rotateTransform != null && Math.Abs((rotateTransform.Angle / 90) % 2) == 1)
                    {
                        textAnnotation.Width = Height;
                        textAnnotation.Height = Width;
                    }
                }
            }
        }

        private void AnnotationShapeLoaded(object sender, RoutedEventArgs e)
        {
            if (AnnotationInfo.Type == AnnotationTypeModel.Line)
            {
                HideLineOptionPopup("PopupLineStyle", SelectedLineStyle.ToString());
                HideLineOptionPopup("PopupLineWeight", SelectedLineWeight.ToString());
                HideLineOptionPopup("PopupLineColor", SelectedLineColor);
            }
        }

        private void AnnotationShapeSizeChanged(object sender, SizeChangedEventArgs e)
        {
            AnnotationContainer.Width = e.NewSize.Width;
            AnnotationContainer.Height = e.NewSize.Height;
        }

        private void ContainerSizeChanged(object sender, SizeChangedEventArgs e)
        {
            var frameworkElement = AnnotationContainer.Children[0] as FrameworkElement;
            if (frameworkElement != null)
            {
                frameworkElement.Width = e.NewSize.Width;
                frameworkElement.Height = e.NewSize.Height;
            }

            AdjustSize();
            UpdateLine();
        }

        private void CreateBlackBox()
        {
            var rect = new Rectangle { Fill = Brushes.Black, Width = Width, Height = Height };
            AnnotationContainer.Children.Add(rect);
        }

        private void CreateContainer(double width, double height)
        {
            AnnotationContainer = new Canvas { Width = width, Height = height, Background = Brushes.Transparent };
            AnnotationContainer.SizeChanged += ContainerSizeChanged;
            Content = AnnotationContainer;
        }

        private void CreateHighlightBox()
        {
            var rect = new Rectangle { Fill = Brushes.Yellow, Opacity = 0.5, Width = Width, Height = Height };
            AnnotationContainer.Children.Add(rect);
        }

        private void CreateLineAnnotation()
        {
            if (AnnotationInfo.LineWeight == 0)
            {
                AnnotationInfo.LineWeight = 3;
            }

            Height = Math.Max(Height, AnnotationInfo.LineWeight);
            Width = Math.Max(Width, AnnotationInfo.LineWeight);

            var line = new ArrowWithLine();
            AnnotationContainer.Children.Add(line);

            UpdateLine();

            SelectedLineStyle = AnnotationInfo.LineStyle;
            SelectedLineWeight = AnnotationInfo.LineWeight;
            SelectedLineColor = AnnotationInfo.LineColor;
        }

        private void CreateTextAnnotation(bool readOnly)
        {
            TextTool textAnnotation = null;
            if (AnnotationContainer.Children.Count > 0)
            {
                textAnnotation = AnnotationContainer.Children[0] as TextTool;
            }

            if (textAnnotation == null)
            {
                textAnnotation = new TextTool();
                if (!string.IsNullOrEmpty(AnnotationInfo.Content))
                {
                    textAnnotation.XamlContent = AnnotationInfo.Content;
                }

                AnnotationContainer.Children.Add(textAnnotation);
            }
            else
            {
                textAnnotation.BackupZoomRatio = ItemCanvasContainer.CurrentZoomRatio;
            }

            Width = Math.Max(Width, textAnnotation.MinWidth);
            Height = Math.Max(Height, textAnnotation.MinHeight);
            MinWidth = textAnnotation.MinWidth;
            MinHeight = textAnnotation.MinHeight;
            textAnnotation.Width = Width;
            textAnnotation.Height = Height;
            textAnnotation.ShowReadOnlyMode = readOnly;

            if (readOnly)
            {
                Rotate();
            }
            else
            {
                textAnnotation.LayoutTransform = new RotateTransform(0); // Ensure editor always readable for user
            }

            AdjustSize();
        }

        private void CreateOCRZone()
        {
            var rect = new Border
                           {
                               Background = Brushes.Transparent,
                               Width = Width,
                               Height = Height,
                               BorderBrush = OCRValidColor,
                               BorderThickness = new Thickness(2)
                           };
            AnnotationContainer.Children.Add(rect);
            if (AnnotationInfo.OCRTemplateZone.FieldMetaData.Id == Guid.Empty)
            {
                rect.BorderBrush = OCRFieldNotSetColor;
            }
        }

        private void EnableSelectionMode(bool enable)
        {
            AnnotationContainer.IsHitTestVisible = !enable;
            AnnotationContainer.Children[0].IsHitTestVisible = !enable;
        }

        private ArrowEnds GetArrowEnds()
        {
            switch (AnnotationInfo.LineStyle)
            {
                case LineStyleModel.ArrowAtStart:
                    return ArrowEnds.Start;
                case LineStyleModel.ArrowAtEnd:
                    return ArrowEnds.End;
                case LineStyleModel.ArrowAtBoth:
                    return ArrowEnds.Both;
                default:
                    return ArrowEnds.None;
            }
        }

        private RectangleVertexModel GetNextVertex(RectangleVertexModel item, List<RectangleVertexModel> vertexes)
        {
            if (vertexes.IndexOf(item) == vertexes.Count - 1)
            {
                return vertexes[0];
            }

            return vertexes[vertexes.IndexOf(item) + 1];
        }

        private Point GetPointAtVertex(RectangleVertexModel vertex)
        {
            int offset = AnnotationInfo.LineWeight;

            switch (vertex)
            {
                case RectangleVertexModel.TopRight:
                    return new Point(Width == offset ? offset : Width - offset, offset);
                case RectangleVertexModel.BottomLeft:
                    return new Point(offset, Height == offset ? offset : Height - offset);
                case RectangleVertexModel.BottomRight:
                    return new Point(
                        Width == offset ? offset : Width - offset, Height == offset ? offset : Height - offset);
                default:
                    return new Point(offset, offset);
            }
        }

        private void HideLineOptionPopup(string popupName, string selectedValue)
        {
            var popup = Template.FindName(popupName, this) as Popup;
            if (popup != null)
            {
                popup.IsOpen = false;
                IEnumerable<ToggleButton> items = UIHelper.FindVisualChildren<ToggleButton>(popup);
                foreach (ToggleButton item in items)
                {
                    item.IsChecked = item.Tag.ToString().ToUpper() == selectedValue.ToUpper();
                }
            }
        }

        private void InitializeLineCommands()
        {
            CommandBindings.Add(new CommandBinding(SelectLineStyleCommand, SelectLineStyleCommandExecute));
            CommandBindings.Add(new CommandBinding(SelectLineWeightCommand, SelectLineWeightCommandExecute));
            CommandBindings.Add(new CommandBinding(SelectLineColorCommand, SelectLineColorCommandExecute));
        }

        private void InitilizeRotateVertexDictionary()
        {
            _rotateRightVertexesSequences =
                new List<RectangleVertexModel>(
                    new[]
                        {
                            RectangleVertexModel.TopLeft, RectangleVertexModel.TopRight, RectangleVertexModel.BottomRight,
                            RectangleVertexModel.BottomLeft
                        });

            _rotateLeftVertexesSequences =
                new List<RectangleVertexModel>(
                    new[]
                        {
                            RectangleVertexModel.TopLeft, RectangleVertexModel.BottomLeft, RectangleVertexModel.BottomRight,
                            RectangleVertexModel.TopRight
                        });
        }

        private void OnContentChanged()
        {
            if (ContentChanged != null)
            {
                IsChanged = true;

                ViewerContainer viewer = GetViewerContainer();
                if (IsNew)
                {
                    if (viewer != null)
                    {
                        AnnotationInfo.CreatedBy = CreatedBy = viewer.UserName;
                    }
                    else
                    {
                        AnnotationInfo.CreatedBy = CreatedBy;
                    }

                    AnnotationInfo.CreatedOn = CreatedOn = DateTime.Now;
                }
                else
                {
                    if (viewer != null)
                    {
                        AnnotationInfo.ModifiedBy = LastModifiedBy = viewer.UserName;
                    }
                    else
                    {
                        AnnotationInfo.ModifiedBy = LastModifiedBy;
                    }
                    AnnotationInfo.ModifiedOn = LastModifiedOn = DateTime.Now;
                }

                ContentChanged(this, EventArgs.Empty);
            }
        }

        private ViewerContainer GetViewerContainer()
        {
            return UIHelper.FindVisualParent<ViewerContainer>(this);
        }

        private void Rotate()
        {
            AnnotationInfo.RotateAngle = AnnotationInfo.RotateAngle % 360;

            if (AnnotationInfo.Type == AnnotationTypeModel.Text)
            {
                var frameworkElement = AnnotationContainer.Children[0] as FrameworkElement;
                if (frameworkElement != null)
                {
                    frameworkElement.LayoutTransform = new RotateTransform(AnnotationInfo.RotateAngle);
                }
            }
        }

        private void RotateLine(bool right)
        {
            if (AnnotationInfo.Type == AnnotationTypeModel.Line)
            {
                AnnotationInfo.RotateAngle = AnnotationInfo.RotateAngle % 360;
                List<RectangleVertexModel> vertexes = right ? _rotateRightVertexesSequences : _rotateLeftVertexesSequences;

                AnnotationInfo.LineStartAt = GetNextVertex(AnnotationInfo.LineStartAt, vertexes);
                AnnotationInfo.LineEndAt = GetNextVertex(AnnotationInfo.LineEndAt, vertexes);
            }
        }

        private void SelectLineColorCommandExecute(object sender, ExecutedRoutedEventArgs e)
        {
            SelectedLineColor = e.Parameter.ToString();
            AnnotationInfo.LineColor = SelectedLineColor;
            HideLineOptionPopup("PopupLineColor", SelectedLineColor);
            UpdateLine();
        }

        private void SelectLineStyleCommandExecute(object sender, ExecutedRoutedEventArgs e)
        {
            SelectedLineStyle = (LineStyleModel)Enum.Parse(typeof(LineStyleModel), e.Parameter + string.Empty);
            AnnotationInfo.LineStyle = SelectedLineStyle;
            HideLineOptionPopup("PopupLineStyle", SelectedLineStyle.ToString());
            UpdateLine();
        }

        private void SelectLineWeightCommandExecute(object sender, ExecutedRoutedEventArgs e)
        {
            SelectedLineWeight = Convert.ToInt32(e.Parameter);
            AnnotationInfo.LineWeight = SelectedLineWeight;
            HideLineOptionPopup("PopupLineWeight", SelectedLineWeight.ToString());
            UpdateLine();
        }

        private void UpdateLine()
        {
            if (AnnotationInfo.Type == AnnotationTypeModel.Line)
            {
                Height = Math.Max(Height, AnnotationInfo.LineWeight);
                Width = Math.Max(Width, AnnotationInfo.LineWeight);

                var line = AnnotationContainer.Children[0] as ArrowWithLine;
                Point startPoint = GetPointAtVertex(AnnotationInfo.LineStartAt);
                Point endPoint = GetPointAtVertex(AnnotationInfo.LineEndAt);
                if (line != null)
                {
                    line.X1 = startPoint.X;
                    line.Y1 = startPoint.Y;
                    line.X2 = endPoint.X;
                    line.Y2 = endPoint.Y;

                    line.Stroke = new BrushConverter().ConvertFromString(AnnotationInfo.LineColor) as Brush;
                    line.StrokeThickness = AnnotationInfo.LineWeight * _currentZoomRatio;
                    line.ArrowEnds = GetArrowEnds();
                    line.IsArrowClosed = true;
                    line.Fill = line.Stroke;
                }
            }
        }

        private void AnnotationInfoPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (AnnotationInfo.Type == AnnotationTypeModel.OCRZone && e.PropertyName == "OCRTemplateZone")
            {
                ValidateOCRZone(ItemCanvasContainer);
            }
        }

        #endregion

        #region Private members

        private double _currentZoomRatio = 1.0f;
        private bool _isActualChanged = true;
        private List<RectangleVertexModel> _rotateLeftVertexesSequences = new List<RectangleVertexModel>();
        private List<RectangleVertexModel> _rotateRightVertexesSequences = new List<RectangleVertexModel>();

        #endregion
    }
}