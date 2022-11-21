using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using Ecm.DocViewer.Model;

namespace Ecm.DocViewer.Controls
{
    public abstract class ArrowBase : Shape
    {
        public static readonly DependencyProperty ArrowAngleProperty = DependencyProperty.Register("ArrowAngle", typeof(double),
            typeof(ArrowBase), new FrameworkPropertyMetadata(45.0, FrameworkPropertyMetadataOptions.AffectsMeasure));

        public static readonly DependencyProperty ArrowEndsProperty = DependencyProperty.Register("ArrowEnds", typeof(ArrowEnds),
            typeof(ArrowBase), new FrameworkPropertyMetadata(ArrowEnds.End, FrameworkPropertyMetadataOptions.AffectsMeasure));

        public static readonly DependencyProperty ArrowLengthProperty = DependencyProperty.Register("ArrowLength", typeof(double),
            typeof(ArrowBase), new FrameworkPropertyMetadata(12.0, FrameworkPropertyMetadataOptions.AffectsMeasure));

        public static readonly DependencyProperty IsArrowClosedProperty = DependencyProperty.Register("IsArrowClosed", typeof(bool),
            typeof(ArrowBase), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsMeasure));

        public double ArrowAngle
        {
            set
            {
                SetValue(ArrowAngleProperty, value);
            }
            get
            {
                return (double)GetValue(ArrowAngleProperty);
            }
        }

        public ArrowEnds ArrowEnds
        {
            set
            {
                SetValue(ArrowEndsProperty, value);
            }
            get
            {
                return (ArrowEnds)GetValue(ArrowEndsProperty);
            }
        }

        public double ArrowLength
        {
            set
            {
                SetValue(ArrowLengthProperty, value);
            }
            get
            {
                return (double)GetValue(ArrowLengthProperty);
            }
        }

        public bool IsArrowClosed
        {
            set
            {
                SetValue(IsArrowClosedProperty, value);
            }
            get
            {
                return (bool)GetValue(IsArrowClosedProperty);
            }
        }

        protected ArrowBase()
        {
            _pathgeo = new PathGeometry();

            _pathfigLine = new PathFigure();
            _polysegLine = new PolyLineSegment();
            _pathfigLine.Segments.Add(_polysegLine);

            _pathfigHead1 = new PathFigure();
            _polysegHead1 = new PolyLineSegment();
            _pathfigHead1.Segments.Add(_polysegHead1);

            _pathfigHead2 = new PathFigure();
            _polysegHead2 = new PolyLineSegment();
            _pathfigHead2.Segments.Add(_polysegHead2);
        }

        protected override Geometry DefiningGeometry
        {
            get
            {
                int count = _polysegLine.Points.Count;

                if (count > 0)
                {
                    // Draw the arrow at the start of the line.
                    if ((ArrowEnds & ArrowEnds.Start) == ArrowEnds.Start)
                    {
                        Point pt1 = _pathfigLine.StartPoint;
                        Point pt2 = _polysegLine.Points[0];
                        _pathgeo.Figures.Add(CalculateArrow(_pathfigHead1, pt2, pt1));
                    }

                    // Draw the arrow at the end of the line.
                    if ((ArrowEnds & ArrowEnds.End) == ArrowEnds.End)
                    {
                        Point pt1 = count == 1 ? _pathfigLine.StartPoint : _polysegLine.Points[count - 2];
                        Point pt2 = _polysegLine.Points[count - 1];
                        _pathgeo.Figures.Add(CalculateArrow(_pathfigHead2, pt1, pt2));
                    }
                }

                return _pathgeo;
            }
        }

        protected override Geometry GetLayoutClip(Size layoutSlotSize)
        {
            return null;
        }

        private PathFigure CalculateArrow(PathFigure pathfig, Point pt1, Point pt2)
        {
            var matx = new Matrix();
            Vector vect = pt1 - pt2;
            vect.Normalize();
            vect *= ArrowLength;

            var polyseg = pathfig.Segments[0] as PolyLineSegment;
            if (polyseg != null)
            {
                polyseg.Points.Clear();
            }

            matx.Rotate(ArrowAngle / 2);
            pathfig.StartPoint = pt2 + vect * matx;
            if (polyseg != null)
            {
                polyseg.Points.Add(pt2);
            }

            matx.Rotate(-ArrowAngle);
            if (polyseg != null)
            {
                polyseg.Points.Add(pt2 + vect * matx);
            }

            pathfig.IsClosed = IsArrowClosed;
            return pathfig;
        }

        protected PathFigure _pathfigLine;

        protected PathGeometry _pathgeo;

        protected PolyLineSegment _polysegLine;

        private readonly PathFigure _pathfigHead1;

        private readonly PathFigure _pathfigHead2;

        private readonly PolyLineSegment _polysegHead1;

        private readonly PolyLineSegment _polysegHead2;
    }
}