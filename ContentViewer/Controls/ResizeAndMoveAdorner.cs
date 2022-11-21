using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace Ecm.ContentViewer.Controls
{
    public class ResizeAndMoveAdorner : Adorner
    {
        public ResizeAndMoveAdorner(ContentControl designerItem)
            : base(designerItem)
        {
            SnapsToDevicePixels = true;
            _chrome = new ResizeAndMoveChrome { DataContext = designerItem };
            _visuals = new VisualCollection(this) { _chrome };
        }

        protected override int VisualChildrenCount
        {
            get
            {
                return _visuals.Count;
            }
        }

        protected override Size ArrangeOverride(Size arrangeBounds)
        {
            _chrome.Arrange(new Rect(arrangeBounds));
            return arrangeBounds;
        }

        protected override Visual GetVisualChild(int index)
        {
            return _visuals[index];
        }

        private readonly ResizeAndMoveChrome _chrome;

        private readonly VisualCollection _visuals;
    }
}