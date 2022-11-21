using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Ecm.CustomControl
{
    public class UIHelper
    {
        public static DependencyObject FindChild(DependencyObject o, Type childType)
        {
            DependencyObject foundChild = null;
            if (o != null)
            {
                int childrenCount = VisualTreeHelper.GetChildrenCount(o);
                for (int i = 0; i < childrenCount; i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(o, i);
                    if (child.GetType() != childType)
                    {
                        foundChild = FindChild(child, childType);
                    }
                    else
                    {
                        foundChild = child;
                        break;
                    }
                }
            }

            return foundChild;
        }

        public static IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj != null)
            {
                IEnumerable children = LogicalTreeHelper.GetChildren(depObj);
                foreach (DependencyObject child in children)
                {
                    if (child != null && child is T)
                    {
                        yield return (T)child;
                    }

                    foreach (T childOfChild in FindVisualChildren<T>(child))
                    {
                        yield return childOfChild;
                    }
                }
            }
        }

        public static T FindVisualParent<T>(UIElement element) where T : UIElement
        {
            UIElement parent = element;
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

        public static ImageSource GetImageFromControl(FrameworkElement visual, double dpiX, double dpiY, double width, double height)
        {
            // get size of control
            var sizeOfControl = new Size(visual.ActualWidth, visual.ActualHeight);
            // measure and arrange the control
            visual.Measure(sizeOfControl);
            // arrange the surface
            visual.Arrange(new Rect(sizeOfControl));

            // craete and render surface and push bitmap to it
            var renderBitmap = new RenderTargetBitmap(
                (int)(width * dpiX / 96), (int)(height * dpiY / 96), dpiX, dpiY, PixelFormats.Pbgra32);
            // now render surface to bitmap
            renderBitmap.Render(visual);

            // encode png data
            var pngEncoder = new PngBitmapEncoder();
            // puch rendered bitmap into it
            pngEncoder.Frames.Add(BitmapFrame.Create(renderBitmap));

            // return encoder
            return pngEncoder.Frames[0];
        }
    }
}