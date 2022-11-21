using System.Windows.Controls;
using System.Windows.Input;
using System.Windows;

namespace Ecm.CustomControl
{
    public class KeyLessScrollViewer : ScrollViewer
    {
        public event ScrollBarVisibilityChanged VerticalScrollBarVisibilityChanged;

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.KeyboardDevice.Modifiers == ModifierKeys.Control)
            {
                if (e.Key == Key.Left || e.Key == Key.Right || e.Key == Key.Up || e.Key == Key.Down ||
                    e.Key == Key.PageDown || e.Key == Key.PageUp)
                {
                    return;
                }
            }

            base.OnKeyDown(e);
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            if (e.Property.Name == "ComputedVerticalScrollBarVisibility")
            {
                var visibility = (Visibility)e.NewValue;
                if (VerticalScrollBarVisibilityChanged != null)
                {
                    VerticalScrollBarVisibilityChanged(this, visibility);
                }
            }

            base.OnPropertyChanged(e);
        }
    }

    public delegate void ScrollBarVisibilityChanged(object sender, Visibility visibility);
}
