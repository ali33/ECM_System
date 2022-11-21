using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Ecm.CaptureViewer.Controls
{
    public class SeparateBorder : Border
    {
        protected override Geometry GetLayoutClip(Size layoutSlotSize)
        {
            return null;
        }
    }
}