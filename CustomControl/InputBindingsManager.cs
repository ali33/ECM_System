using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

namespace Ecm.CustomControl
{
    public class InputBindingsManager
    {
        public static DependencyProperty GetUpdatePropertySourceWhenEnterPressed(DependencyObject dp)
        {
            return (DependencyProperty)dp.GetValue(UpdatePropertySourceWhenEnterPressedProperty);
        }

        public static void SetUpdatePropertySourceWhenEnterPressed(DependencyObject dp, DependencyProperty value)
        {
            dp.SetValue(UpdatePropertySourceWhenEnterPressedProperty, value);
        }

        private static void DoUpdateSource(object source)
        {
            DependencyProperty property = GetUpdatePropertySourceWhenEnterPressed(source as DependencyObject);

            if (property != null)
            {
                UIElement elt = source as UIElement;

                if (elt != null)
                {
                    BindingExpression binding = BindingOperations.GetBindingExpression(elt, property);

                    if (binding != null)
                    {
                        binding.UpdateSource();
                    }
                }
            }
        }

        private static void HandlePreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                DoUpdateSource(e.Source);
            }
        }

        private static void OnUpdatePropertySourceWhenEnterPressedPropertyChanged(
            DependencyObject dp, DependencyPropertyChangedEventArgs e)
        {
            UIElement element = dp as UIElement;

            if (element != null)
            {
                if (e.OldValue != null)
                {
                    element.PreviewKeyDown -= HandlePreviewKeyDown;
                }

                if (e.NewValue != null)
                {
                    element.PreviewKeyDown += HandlePreviewKeyDown;
                }
            }
        }

        public static readonly DependencyProperty UpdatePropertySourceWhenEnterPressedProperty =
            DependencyProperty.RegisterAttached(
                "UpdatePropertySourceWhenEnterPressed",
                typeof(DependencyProperty),
                typeof(InputBindingsManager),
                new PropertyMetadata(null, OnUpdatePropertySourceWhenEnterPressedPropertyChanged));
    }
}