using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Ecm.ContentViewer.Controls
{
    public class SeparatedBorder : Border
    {
        protected override Geometry GetLayoutClip(Size layoutSlotSize)
        {
            return null;
        }
    }
}