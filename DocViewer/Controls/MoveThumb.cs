using System;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace Ecm.DocViewer.Controls
{
    public class MoveThumb : Thumb
    {
        public MoveThumb()
        {
            DragStarted += MoveThumbDragStarted;
            DragDelta += MoveThumbDragDelta;
        }

        private void MoveThumbDragDelta(object sender, DragDeltaEventArgs e)
        {
            if (_designerItem != null && _designerCanvas != null && _designerItem.IsSelected)
            {
                double minLeft = double.MaxValue;
                double minTop = double.MaxValue;
                double maxRight = 0;
                double maxBottom = 0;

                foreach (AnnotationControl item in _designerCanvas.SelectedItems)
                {
                    minLeft = Math.Min(Canvas.GetLeft(item), minLeft);
                    minTop = Math.Min(Canvas.GetTop(item), minTop);
                    maxRight = Math.Max(Canvas.GetLeft(item) + item.Width, maxRight);
                    maxBottom = Math.Max(Canvas.GetTop(item) + item.Height, maxBottom);
                }

                double deltaHorizontal = e.HorizontalChange < 0 ? Math.Max(-minLeft, e.HorizontalChange) : Math.Min(_designerCanvas.Width - maxRight, e.HorizontalChange);
                double deltaVertical = e.VerticalChange < 0 ? Math.Max(-minTop, e.VerticalChange) : Math.Min(_designerCanvas.Height - maxBottom, e.VerticalChange);

                foreach (AnnotationControl item in _designerCanvas.SelectedItems)
                {
                    Canvas.SetLeft(item, Canvas.GetLeft(item) + deltaHorizontal);
                    Canvas.SetTop(item, Canvas.GetTop(item) + deltaVertical);
                }

                e.Handled = true;
            }
        }

        private void MoveThumbDragStarted(object sender, DragStartedEventArgs e)
        {
            _designerItem = DataContext as AnnotationControl;

            if (_designerItem != null)
            {
                _designerCanvas = VisualTreeHelper.GetParent(_designerItem) as CanvasElement;
            }
        }

        private CanvasElement _designerCanvas;

        private AnnotationControl _designerItem;
    }
}