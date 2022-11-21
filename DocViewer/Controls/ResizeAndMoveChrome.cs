using System.Windows;
using System.Windows.Controls;

namespace Ecm.DocViewer.Controls
{
    public class ResizeAndMoveChrome : Control
    {
        static ResizeAndMoveChrome()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ResizeAndMoveChrome), new FrameworkPropertyMetadata(typeof(ResizeAndMoveChrome)));
        }
    }
}