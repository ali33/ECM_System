using System;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using Ecm.ContentViewer.Controls;
using Ecm.ContentViewer.Model;

namespace Ecm.ContentViewer.Helper
{
    public class SelectionToolHelper
    {
        #region Private members

        private const double _minDragDrawing = 2f;
        private readonly CanvasElement _canvas;
        private readonly Action _startSelectionAction;
        private bool _enabled;
        private Point _endPoint;
        private bool _hasSelection;
        private Rectangle _rubberband;
        private Point _startPoint;

        #endregion

        #region Public methods

        public SelectionToolHelper(CanvasElement canvas, Action startSelectionAction)
        {
            _canvas = canvas;
            _startSelectionAction = startSelectionAction;
        }

        public void EnableSelection()
        {
            if (!_enabled)
            {
                _canvas.MouseEnter += MouseEnter;
                _canvas.PreviewMouseDown += PreviewMouseDown;
                _canvas.MouseDown += MouseDown;
                _canvas.MouseMove += MouseMove;
                _canvas.MouseUp += MouseUp;
                _canvas.MouseLeave += MouseLeave;

                _enabled = true;
            }
        }

        public void RemoveSelection()
        {
            _canvas.MouseEnter -= MouseEnter;
            _canvas.PreviewMouseDown -= PreviewMouseDown;
            _canvas.MouseDown -= MouseDown;
            _canvas.MouseMove -= MouseMove;
            _canvas.MouseUp -= MouseUp;
            _canvas.MouseLeave -= MouseLeave;

            _canvas.Cursor = null;
            _enabled = false;
        }

        #endregion

        #region Private methods

        private void MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.Source == _canvas && e.LeftButton == MouseButtonState.Pressed && e.ClickCount == 1)
            {
                _startPoint = e.GetPosition(_canvas);
                _rubberband = new Rectangle();
                _canvas.Children.Add(_rubberband);
                _canvas.DeselectAll();

                _rubberband.StrokeDashArray = new DoubleCollection(new[] { 0.5, 1.0, 0.5 });
                _rubberband.Stroke = Brushes.Gray;
                _rubberband.Width = 0;
                _rubberband.Height = 0;
                Canvas.SetLeft(_rubberband, _startPoint.X);
                Canvas.SetTop(_rubberband, _startPoint.Y);
                e.Handled = true;
            }
        }

        private void MouseEnter(object sender, MouseEventArgs e)
        {
            SetSelectionCursor();
            e.Handled = true;
        }

        private void MouseLeave(object sender, MouseEventArgs e)
        {
            CleanRubberband();
        }

        private void MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && _rubberband != null)
            {
                _endPoint = e.GetPosition(_canvas);
                double deltaX = Math.Abs(_endPoint.X - _startPoint.X);
                double deltaY = Math.Abs(_endPoint.Y - _startPoint.Y);

                if (Math.Min(deltaX, deltaY) >= _minDragDrawing) // Ensure user drag mouse for selection. Default by minimum 2 points
                {
                    UpdateRubberband();
                    UpdateSelection();

                    _hasSelection = true;
                    e.Handled = true;
                }
            }
        }

        private void MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (!_hasSelection)
            {
                SelectCurrentAnnotation(e.GetPosition(_canvas));
            }

            CleanRubberband();
        }

        private void PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if (e.ClickCount == 2)
                {
                    AnnotationControl textShape = SelectCurrentAnnotation(e.GetPosition(_canvas));
                    if (textShape != null && textShape.AnnotationInfo.Type == AnnotationTypeModel.Text)
                    {
                        textShape.ShowTextBox();
                        e.Handled = true;
                    }
                }
                else if (e.ClickCount == 1 && (e.OriginalSource is Rectangle || e.OriginalSource is CanvasElement)) // for non-text annotation which has bug when select item on combobox in OCR template mode
                {
                    if (_startSelectionAction != null)
                    {
                        _startSelectionAction();
                    }

                    if ((Keyboard.Modifiers & (ModifierKeys.Shift | ModifierKeys.Control)) != ModifierKeys.None)
                    {
                        SelectCurrentAnnotation(e.GetPosition(_canvas));
                    }
                    else
                    {
                        AnnotationControl annotation = _canvas.GetAnnotationShape(e.GetPosition(_canvas));
                        if (annotation != null)
                        {
                            if (!annotation.IsSelected)
                            {
                                _canvas.DeselectAll();
                                //annotation.IsSelected = true;
                                annotation.Select();
                            }
                        }
                    }
                }
            }
        }

        private void CleanRubberband()
        {
            _canvas.Children.Remove(_rubberband);
            _rubberband = null;
            _hasSelection = false;
        }

        private AnnotationControl SelectCurrentAnnotation(Point mousePosition)
        {
            AnnotationControl annotation = _canvas.GetAnnotationShape(mousePosition);
            if (annotation != null)
            {
                annotation.Select();
                return annotation;
            }

            _canvas.RemoveToolBarForShapes();
            return null;
        }

        private void SetSelectionCursor()
        {
            _canvas.Cursor = new Cursor(Assembly.GetAssembly(typeof(CanvasElement)).GetManifestResourceStream("Ecm.ContentViewer.Resources.selection.cur"));
        }

        private void UpdateRubberband()
        {
            double left = Math.Min(_startPoint.X, _endPoint.X);
            double top = Math.Min(_startPoint.Y, _endPoint.Y);

            double width = Math.Abs(_startPoint.X - _endPoint.X);
            double height = Math.Abs(_startPoint.Y - _endPoint.Y);

            _rubberband.Width = width;
            _rubberband.Height = height;
            Canvas.SetLeft(_rubberband, left);
            Canvas.SetTop(_rubberband, top);
        }

        private void UpdateSelection()
        {
            var rubberBand = new Rect(_startPoint, _endPoint);
            for (int i = 0; i < _canvas.Children.Count; i++)
            {
                if (_canvas.Children[i] is AnnotationControl)
                {
                    var item = _canvas.Children[i] as AnnotationControl;
                    Rect itemRect = VisualTreeHelper.GetDescendantBounds(item);
                    if (item != null)
                    {
                        Rect itemBounds = item.TransformToAncestor(_canvas).TransformBounds(itemRect);
                        if (rubberBand.Contains(itemBounds))
                        {
                            item.Select();
                        }
                    }
                }
            }

            _canvas.RemoveToolBarForShapes();
        }

        #endregion
    }
}