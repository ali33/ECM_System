using System;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using Ecm.CustomControl;
using Ecm.ContentViewer.Controls;
using Ecm.ContentViewer.Model;
using Ecm.ContentViewer.View;
using System.Collections.ObjectModel;

namespace Ecm.ContentViewer.Helper
{
    public class DrawingHelper
    {
        private const double _minDragDrawing = 2f;
        private readonly CanvasElement _canvas;
        private readonly Action _startSelectionAction;
        private bool _canDrawShape;
        private bool _enable;
        private Rectangle _fakeShape;
        private Point _startPoint;

        public DrawingHelper(CanvasElement canvas, Action startSelectionAction, MainViewer mainViewer)
        {
            _canvas = canvas;
            _startSelectionAction = startSelectionAction;
            MainViewer = mainViewer;
        }

        public MainViewer MainViewer { get; private set; }

        private ObservableCollection<BatchTypeModel> BatchTypes { get; set; }

        private string Username { get; set; }

        public void EnableDrawing()
        {
            if (!_enable)
            {
                _canvas.MouseEnter += new MouseEventHandler(MouseEnter);
                _canvas.MouseDown += new MouseButtonEventHandler(MouseDown);
                _canvas.MouseMove += new MouseEventHandler(MouseMove);
                _canvas.MouseUp += new MouseButtonEventHandler(MouseUp);
                _canvas.MouseLeave += new MouseEventHandler(MouseLeave);
                _enable = true;
            }
        }


        public void RemoveDrawing()
        {
            _canvas.MouseEnter -= MouseEnter;
            _canvas.MouseDown -= MouseDown;
            _canvas.MouseMove -= MouseMove;
            _canvas.MouseUp -= MouseUp;
            _canvas.MouseLeave -= MouseLeave;

            _canvas.DeselectAll();
            _enable = false;
            CleanDrawing();
            _canvas.Cursor = null;
        }

        private void AnnotationControlChanged(object sender, EventArgs e)
        {
            _canvas.OnContentChanged();
        }

        private void CleanDrawing()
        {
            _canvas.Children.Remove(_fakeShape);
            _fakeShape = null;
            _canDrawShape = false;
        }

        private void DrawAnnotationShapes(Point endMousePosition)
        {
            AnnotationTypeModel type = GetAnnotationType();
            double deltaX = Math.Abs(endMousePosition.X - _startPoint.X);
            double deltaY = Math.Abs(endMousePosition.Y - _startPoint.Y);
            var viewerContainer = UIHelper.FindVisualParent<MainViewer>(_canvas);

            var info = new AnnotationModel
            {
                Width = deltaX / _canvas.CurrentZoomRatio,
                Height = deltaY / _canvas.CurrentZoomRatio,
                Top = Math.Min(_startPoint.Y, endMousePosition.Y) / _canvas.CurrentZoomRatio,
                Left = Math.Min(_startPoint.X, endMousePosition.X) / _canvas.CurrentZoomRatio,
                Type = type
            };

            if (type == AnnotationTypeModel.Line)
            {
                if (_startPoint.X < endMousePosition.X && _startPoint.Y < endMousePosition.Y)
                {
                    info.LineStartAt = RectangleVertexModel.TopLeft;
                    info.LineEndAt = RectangleVertexModel.BottomRight;
                }
                else if (_startPoint.X > endMousePosition.X && _startPoint.Y > endMousePosition.Y)
                {
                    info.LineStartAt = RectangleVertexModel.BottomRight;
                    info.LineEndAt = RectangleVertexModel.TopLeft;
                }
                else if (_startPoint.X < endMousePosition.X && _startPoint.Y > endMousePosition.Y)
                {
                    info.LineStartAt = RectangleVertexModel.BottomLeft;
                    info.LineEndAt = RectangleVertexModel.TopRight;
                }
                else
                {
                    info.LineStartAt = RectangleVertexModel.TopRight;
                    info.LineEndAt = RectangleVertexModel.BottomLeft;
                }
            }
            else if (type == AnnotationTypeModel.OCRZone)
            {
                info.OCRTemplateZone = new OCRTemplateZoneModel { FieldMetaData = new FieldModel() };
                info.MetaFields = BatchTypes[0].DocTypes[0].Fields;
            }

            info.CreatedBy = Username;
            info.CreatedOn = DateTime.Now;
            info.ModifiedBy = Username;
            info.ModifiedOn = DateTime.Now;

            _canvas.PageInfo.Annotations.Add(info);
            var shape = new AnnotationControl(info, _canvas);
            shape.ContentChanged += AnnotationControlChanged;
            shape.IsNew = true;
            shape.IsSelected = true;
            var key = new ComponentResourceKey(typeof(CanvasElement), "AnnotationStyle");
            shape.Style = (Style)_canvas.TryFindResource(key);

            if (_startSelectionAction != null)
            {
                _startSelectionAction();
            }

            if (shape.AnnotationInfo.Type != AnnotationTypeModel.Text)
            {
                _canvas.OnContentChanged();
            }
        }

        private AnnotationTypeModel GetAnnotationType()
        {
            return _canvas.EnableHighlight ? AnnotationTypeModel.Highlight :
                  (_canvas.EnableRedaction ? AnnotationTypeModel.Redaction :
                  (_canvas.EnableText ? AnnotationTypeModel.Text :
                  (_canvas.EnableLine ? AnnotationTypeModel.Line : AnnotationTypeModel.OCRZone)));
        }

        private void MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && e.ClickCount == 1 && !_canvas.IsNonImagePreview)
            {
                _canvas.DeselectAll();
                StartTracingDraw(e.GetPosition(_canvas));
                e.Handled = true;
            }
        }

        private void MouseEnter(object sender, MouseEventArgs e)
        {
            SetPlusCursor();
            e.Handled = true;
        }

        private void MouseLeave(object sender, MouseEventArgs e)
        {
            CleanDrawing();
            _canvas.Cursor = null;
        }

        private void MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && _fakeShape != null)
            {
                Point currentPosition = e.GetPosition(_canvas);
                double deltaX = Math.Abs(currentPosition.X - _startPoint.X);
                double deltaY = Math.Abs(currentPosition.Y - _startPoint.Y);

                if (Math.Min(deltaX, deltaY) >= _minDragDrawing) // Ensure user drag mouse for drawing. Default by minimum 2 points
                {
                    TracingDraw(e.GetPosition(_canvas));
                    e.Handled = true;
                }
            }
        }

        private void MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (_canDrawShape)
            {
                DrawAnnotationShapes(e.GetPosition(_canvas));
            }

            CleanDrawing();
        }

        private void SetPlusCursor()
        {
            _canvas.Cursor = new Cursor(Assembly.GetAssembly(typeof(CanvasElement)).GetManifestResourceStream("Ecm.ContentViewer.Resources.draw.cur"));
        }

        private void StartTracingDraw(Point startMousePosition)
        {
            _startPoint = startMousePosition;

            _fakeShape = new Rectangle();
            _canvas.Children.Add(_fakeShape);

            _fakeShape.StrokeDashArray = new DoubleCollection(new[] { 0.5, 1.0, 0.5 });
            _fakeShape.Stroke = Brushes.Gray;
            _fakeShape.Width = 0;
            _fakeShape.Height = 0;
            Canvas.SetLeft(_fakeShape, _startPoint.X);
            Canvas.SetTop(_fakeShape, _startPoint.Y);
        }

        private void TracingDraw(Point currentMousePosition)
        {
            double deltaX = currentMousePosition.X - _startPoint.X;
            double deltaY = currentMousePosition.Y - _startPoint.Y;

            _fakeShape.Width = Math.Abs(deltaX);
            _fakeShape.Height = Math.Abs(deltaY);

            if (deltaX < 0)
            {
                Canvas.SetLeft(_fakeShape, currentMousePosition.X);
            }

            if (deltaY < 0)
            {
                Canvas.SetTop(_fakeShape, currentMousePosition.Y);
            }

            _canDrawShape = true;
        }
    }
}