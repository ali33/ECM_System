using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Ecm.CaptureViewer.Controls
{
    public class DragAdorner : Adorner
    {
        public double _scale = 1.0f;
        protected double _xCenter;
        protected double _yCenter;
        protected VisualBrush _brush;
        protected UIElement _child;
        protected UIElement _owner;
        private double _leftOffset;
        private double _topOffset;

        public DragAdorner(UIElement owner)
            : base(owner)
        {
        }

        public DragAdorner(UIElement owner, UIElement adornElement, bool useVisualBrush, double opacity, Brush borderBrush, Thickness borderThickness)
            : base(owner)
        {
            _owner = owner;
            if (useVisualBrush)
            {
                var brush = new VisualBrush(adornElement) {Opacity = opacity};
                var border = new Border {BorderBrush = borderBrush, BorderThickness = borderThickness};
                var content = new Rectangle
                                  {
                                      Width = 100d,
                                      Height = (100d/adornElement.DesiredSize.Width)*adornElement.DesiredSize.Height,
                                      Fill = brush
                                  };

                border.Child = content;
                _xCenter = (content.Width + borderThickness.Left + borderThickness.Right) / 2;
                _yCenter = (content.Height + borderThickness.Top + borderThickness.Bottom) / 2;
                _child = border;
            }
            else
            {
                _child = adornElement;
            }
        }

        public double LeftOffset
        {
            get
            {
                return _leftOffset;
            }
            set
            {
                _leftOffset = value - _xCenter;
                UpdatePosition();
            }
        }

        public double TopOffset
        {
            get
            {
                return _topOffset;
            }
            set
            {
                _topOffset = value - _yCenter;
                UpdatePosition();
            }
        }

        public override GeneralTransform GetDesiredTransform(GeneralTransform transform)
        {
            var result = new GeneralTransformGroup();

            result.Children.Add(base.GetDesiredTransform(transform));
            result.Children.Add(new TranslateTransform(_leftOffset, _topOffset));
            return result;
        }

        protected override int VisualChildrenCount
        {
            get
            {
                return 1;
            }
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            _child.Arrange(new Rect(_child.DesiredSize));
            return finalSize;
        }

        protected override Visual GetVisualChild(int index)
        {
            return _child;
        }

        protected override Size MeasureOverride(Size finalSize)
        {
            _child.Measure(finalSize);
            return _child.DesiredSize;
        }

        private void UpdatePosition()
        {
            var adorner = (AdornerLayer)Parent;
            if (adorner != null)
            {
                adorner.Update(AdornedElement);
            }
        }
    }
}