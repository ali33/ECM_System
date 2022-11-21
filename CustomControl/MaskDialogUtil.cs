using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Ecm.CustomControl
{
    public class MaskDialogUtil
    {
        public static void ShowMaskLayout(UIElement element)
        {
            var page = FindVisualParent<Page>(element);
            if (page != null)
            {
                var topParent = page.Content as Grid;
                if (topParent != null)
                {
                    var maskLayout = new Grid {Background = Brushes.Gray, Opacity = 0.4};
                    topParent.Children.Add(maskLayout);
                }

                page.IsEnabled = false;
            }
        }

        public static void HideMaskLayout(UIElement element)
        {
            var page = FindVisualParent<Page>(element);
            if (page != null)
            {
                var topParent = page.Content as Grid;
                if (topParent != null)
                {
                    topParent.Children.RemoveAt(topParent.Children.Count - 1);
                }

                page.IsEnabled = true;
            }
        }

        private static T FindVisualParent<T>(UIElement element) where T : UIElement
        {
            var parent = element;
            while (parent != null)
            {
                var correctlyTyped = parent as T;
                if (correctlyTyped != null)
                {
                    return correctlyTyped;
                }

                parent = VisualTreeHelper.GetParent(parent) as UIElement;
            }

            return null;
        }
    }
}
