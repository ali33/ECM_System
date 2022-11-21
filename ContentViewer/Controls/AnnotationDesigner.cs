using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace Ecm.ContentViewer.Controls
{
    public class AnnotationDesigner : Control
    {
        #region Dependency properties

        public static readonly DependencyProperty ShowDecoratorProperty = DependencyProperty.Register("ShowDecorator", typeof(bool), typeof(AnnotationDesigner),
            new FrameworkPropertyMetadata(false, ShowDecoratorPropertyChanged));

        private static void ShowDecoratorPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var decorator = d as AnnotationDesigner;
            var showDecorator = (bool)e.NewValue;

            if (showDecorator)
            {
                if (decorator != null)
                {
                    decorator.ShowAdorner();
                }
            }
            else
            {
                if (decorator != null)
                {
                    decorator.HideAdorner();
                }
            }
        }
        
        #endregion

        #region Public methods

        public AnnotationDesigner()
        {
            Unloaded += AnnotationItemDecoratorUnloaded;
        }

        public bool ShowDecorator
        {
            get
            {
                return (bool)GetValue(ShowDecoratorProperty);
            }
            set
            {
                SetValue(ShowDecoratorProperty, value);
            }
        }

        #endregion

        #region Private methods

        private void AnnotationItemDecoratorUnloaded(object sender, RoutedEventArgs e)
        {
            if (_adorner != null)
            {
                var adornerLayer = AdornerLayer.GetAdornerLayer(this);
                if (adornerLayer != null)
                {
                    adornerLayer.Remove(_adorner);
                }

                _adorner = null;
            }
        }

        private void HideAdorner()
        {
            if (_adorner != null)
            {
                _adorner.Visibility = Visibility.Hidden;
            }
        }

        private void ShowAdorner()
        {
            if (_adorner == null)
            {
                AdornerLayer adornerLayer = AdornerLayer.GetAdornerLayer(this);

                if (adornerLayer != null)
                {
                    var designerItem = DataContext as ContentControl;
                    _adorner = new ResizeAndMoveAdorner(designerItem);
                    adornerLayer.Add(_adorner);
                    _adorner.Visibility = ShowDecorator ? Visibility.Visible : Visibility.Hidden;
                }
            }
            else
            {
                _adorner.Visibility = Visibility.Visible;
            }
        }

        #endregion

        #region Private members

        private Adorner _adorner;

        #endregion
    }
}