using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Media;

namespace Ecm.DocViewer.Controls
{
    public class ResizeThumb : Thumb
    {
        public ResizeThumb()
        {
            DragStarted += ResizeThumbDragStarted;
            DragDelta += ResizeThumbDragDelta;
            DragCompleted += ResizeThumbDragCompleted;
        }

        private void ResizeThumbDragCompleted(object sender, DragCompletedEventArgs e)
        {
            if (_adorner != null)
            {
                AdornerLayer adornerLayer = AdornerLayer.GetAdornerLayer(_canvas);
                if (adornerLayer != null)
                {
                    adornerLayer.Remove(_adorner);
                }

                _adorner = null;
            }
        }

        private void ResizeThumbDragDelta(object sender, DragDeltaEventArgs e)
        {
            if (_designerItem != null && _canvas != null && _designerItem.IsSelected)
            {
                double minLeft = double.MaxValue;
                double minTop = double.MaxValue;
                double maxRight = 0;
                double maxBottom = 0;
                double minDeltaHorizontal = double.MaxValue;
                double minDeltaVertical = double.MaxValue;
                double maxDeltaVertical = double.MaxValue;
                double maxDeltaHorizontal = double.MaxValue;

                foreach (AnnotationControl item in _canvas.SelectedItems)
                {
                    minLeft = Math.Min(Canvas.GetLeft(item), minLeft);
                    minTop = Math.Min(Canvas.GetTop(item), minTop);
                    maxRight = Math.Max(Canvas.GetLeft(item) + item.Width, maxRight);
                    maxBottom = Math.Max(Canvas.GetTop(item) + item.Height, maxBottom);

                    minDeltaVertical = Math.Min(minDeltaVertical, item.ActualHeight - item.MinHeight);
                    minDeltaHorizontal = Math.Min(minDeltaHorizontal, item.ActualWidth - item.MinWidth);
                    maxDeltaVertical = Math.Min(_canvas.Height - maxBottom, maxDeltaVertical);
                    maxDeltaHorizontal = Math.Min(_canvas.Width - maxRight, maxDeltaHorizontal);
                }

                foreach (AnnotationControl item in _canvas.SelectedItems)
                {
                    double dragDeltaVertical;
                    switch (VerticalAlignment)
                    {
                        case VerticalAlignment.Bottom:
                            if (e.VerticalChange < 0)
                            {
                                dragDeltaVertical = Math.Min(-e.VerticalChange, minDeltaVertical);
                                item.Height = item.ActualHeight - dragDeltaVertical;
                            }
                            else
                            {
                                dragDeltaVertical = Math.Min(e.VerticalChange, maxDeltaVertical);
                                item.Height = item.ActualHeight + dragDeltaVertical;
                            }
                            break;
                        case VerticalAlignment.Top:
                            dragDeltaVertical = Math.Min(Math.Max(-minTop, e.VerticalChange), minDeltaVertical);
                            Canvas.SetTop(item, Canvas.GetTop(item) + dragDeltaVertical);
                            item.Height = item.ActualHeight - dragDeltaVertical;
                            break;
                    }

                    double dragDeltaHorizontal;
                    switch (HorizontalAlignment)
                    {
                        case HorizontalAlignment.Left:
                            dragDeltaHorizontal = Math.Min(Math.Max(-minLeft, e.HorizontalChange), minDeltaHorizontal);
                            Canvas.SetLeft(item, Canvas.GetLeft(item) + dragDeltaHorizontal);
                            item.Width = item.ActualWidth - dragDeltaHorizontal;
                            break;
                        case HorizontalAlignment.Right:
                            if (e.HorizontalChange < 0)
                            {
                                dragDeltaHorizontal = Math.Min(-e.HorizontalChange, minDeltaHorizontal);
                                item.Width = item.ActualWidth - dragDeltaHorizontal;
                            }
                            else
                            {
                                dragDeltaHorizontal = Math.Min(e.HorizontalChange, maxDeltaHorizontal);
                                item.Width = item.ActualWidth + dragDeltaHorizontal;
                            }
                            break;
                    }
                }

                e.Handled = true;
            }
        }

        private void ResizeThumbDragStarted(object sender, DragStartedEventArgs e)
        {
            _designerItem = DataContext as AnnotationControl;

            if (_designerItem != null)
            {
                _canvas = VisualTreeHelper.GetParent(_designerItem) as CanvasElement;
            }
        }

        private Adorner _adorner;

        private CanvasElement _canvas;

        private AnnotationControl _designerItem;
    }
}