using System.Reflection;
using System.Windows;
using System.Windows.Input;
using Ecm.DocViewer.Controls;

namespace Ecm.DocViewer.Helper
{
    public class PanToolHelper
    {
        private readonly DrawCanvas _canvas;
        private Point _dragScrollStartAt;
        private bool _enabled;

        public PanToolHelper(DrawCanvas canvas)
        {
            _canvas = canvas;
        }

        public void Enable()
        {
            if (!_enabled)
            {
                _canvas.MouseDown += MouseDown;
                _canvas.MouseMove += MouseMove;
                _canvas.MouseUp += MouseUp;

                _enabled = true;
            }
        }

        public void Remove()
        {
            _canvas.MouseDown -= MouseDown;
            _canvas.MouseMove -= MouseMove;
            _canvas.MouseUp -= MouseUp;

            _enabled = false;
            Mouse.Capture(null);
            _canvas.Cursor = null;
        }

        private void MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                Mouse.Capture(_canvas);
                SetGrabCursor();
            }
        }

        private void MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && Mouse.Captured == _canvas)
            {
                Point pos = Mouse.GetPosition(_canvas.ParentScrollViewer);

                if (_dragScrollStartAt != new Point(0, 0) && pos != _dragScrollStartAt)
                {
                    if (_canvas.ParentScrollViewer.ExtentHeight > _canvas.ParentScrollViewer.ActualHeight)
                    {
                        _canvas.ParentScrollViewer.ScrollToVerticalOffset(
                            _canvas.ParentScrollViewer.VerticalOffset + _dragScrollStartAt.Y - pos.Y);
                    }

                    if (_canvas.ParentScrollViewer.ExtentWidth > _canvas.ParentScrollViewer.ActualWidth)
                    {
                        _canvas.ParentScrollViewer.ScrollToHorizontalOffset(
                            _canvas.ParentScrollViewer.HorizontalOffset + _dragScrollStartAt.X - pos.X);
                    }
                }

                _dragScrollStartAt = pos;
            }
        }

        private void MouseUp(object sender, MouseButtonEventArgs e)
        {
            Mouse.Capture(null);
            _dragScrollStartAt = new Point(0, 0);
        }

        private void SetGrabCursor()
        {
            _canvas.Cursor = new Cursor(Assembly.GetAssembly(typeof(PanToolHelper)).GetManifestResourceStream("Ecm.DocViewer.Resources.grab.cur"));
        }
    }
}