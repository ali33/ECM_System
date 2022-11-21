using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Threading;

namespace Ecm.Mvvm
{
    /// <summary>
    ///   The purpose of this class is implement binding function and focusing function
    /// </summary>
    public class FocusBinding : BindingDecoratorBase
    {
        public override object ProvideValue(IServiceProvider provider)
        {
            DependencyObject elem;
            DependencyProperty prop;
            if (base.TryGetTargetItems(provider, out elem, out prop))
            {
                FocusController.SetFocusableProperty(elem, prop);
            }

            return base.ProvideValue(provider);
        }
    }

    /// <summary>
    ///   Implemented by a ViewModel that needs to control where input focus is in a View.
    /// </summary>
    public interface IFocusMover
    {
        /// <summary>
        ///   Raised when the input focus should move to a control whose 'active' dependency property is bound to the specified property.
        /// </summary>
        event EventHandler<MoveFocusEventArgs> MoveFocus;
    }

    /// <summary>
    ///   A base class for custom markup extension which provides properties that can be found on regular markup extension.
    /// </summary>
    [MarkupExtensionReturnType(typeof(object))]
    public abstract class BindingDecoratorBase : MarkupExtension
    {
        /// <summary>
        ///   The decorated binding class.
        /// </summary>
        [Browsable(false)]
        public Binding Binding
        {
            get
            {
                return _binding;
            }
            set
            {
                _binding = value;
            }
        }

        [DefaultValue(BindingMode.Default)]
        public BindingMode Mode
        {
            get
            {
                return _binding.Mode;
            }
            set
            {
                _binding.Mode = value;
            }
        }

        [DefaultValue(null)]
        public PropertyPath Path
        {
            get
            {
                return _binding.Path;
            }
            set
            {
                _binding.Path = value;
            }
        }

        [DefaultValue(UpdateSourceTrigger.Default)]
        public UpdateSourceTrigger UpdateSourceTrigger
        {
            get
            {
                return _binding.UpdateSourceTrigger;
            }
            set
            {
                _binding.UpdateSourceTrigger = value;
            }
        }

        [DefaultValue(false)]
        public bool ValidatesOnDataErrors
        {
            get
            {
                return _binding.ValidatesOnDataErrors;
            }
            set
            {
                _binding.ValidatesOnDataErrors = value;
            }
        }

        public override object ProvideValue(IServiceProvider provider)
        {
            // Create a binding and associate it with the target
            return _binding.ProvideValue(provider);
        }

        protected virtual bool TryGetTargetItems(IServiceProvider provider, out DependencyObject target, out DependencyProperty dp)
        {
            target = null;
            dp = null;
            if (provider == null)
            {
                return false;
            }

            // Create a binding and assign it to the target
            var service = (IProvideValueTarget)provider.GetService(typeof(IProvideValueTarget));
            if (service == null)
            {
                return false;
            }

            // We need dependency objects / properties
            target = service.TargetObject as DependencyObject;
            dp = service.TargetProperty as DependencyProperty;
            return target != null && dp != null;
        }

        /// <summary>
        ///   The decorated binding class.
        /// </summary>
        private Binding _binding = new Binding();
    }

    internal static class FocusController
    {
        internal static DependencyProperty GetFocusableProperty(DependencyObject obj)
        {
            return (DependencyProperty)obj.GetValue(FocusablePropertyProperty);
        }

        internal static void SetFocusableProperty(DependencyObject obj, DependencyProperty value)
        {
            obj.SetValue(FocusablePropertyProperty, value);
        }

        private static void CreateHandler(DependencyObject element, DependencyProperty property)
        {
            var focusMover = element.GetValue(FrameworkElement.DataContextProperty) as IFocusMover;
            if (focusMover == null)
            {
                var handler = element.GetValue(MoveFocusSinkProperty) as MoveFocusSink;
                if (handler != null)
                {
                    handler.ReleaseReferences();
                    element.ClearValue(MoveFocusSinkProperty);
                }
            }
            else
            {
                var handler = new MoveFocusSink(element as UIElement, property);
                focusMover.MoveFocus += handler.HandleMoveFocus;
                element.SetValue(MoveFocusSinkProperty, handler);
            }
        }

        private static void OnFocusablePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var element = sender as FrameworkElement;
            if (element == null)
            {
                return;
            }

            var property = e.NewValue as DependencyProperty;
            if (property == null)
            {
                return;
            }

            element.DataContextChanged += delegate { CreateHandler(element, property); };

            if (element.DataContext != null)
            {
                CreateHandler(element, property);
            }
        }

        private class MoveFocusSink
        {
            public MoveFocusSink(UIElement element, DependencyProperty property)
            {
                _element = element;
                _property = property;
            }

            internal void HandleMoveFocus(object sender, MoveFocusEventArgs e)
            {
                if (_element == null || _property == null)
                {
                    return;
                }

                Binding binding = BindingOperations.GetBinding(_element, _property);
                if (binding == null)
                {
                    return;
                }

                if (e.FocusedProperty != binding.Path.Path)
                {
                    return;
                }

                // Delay the call to allow the current batch
                // of processing to finish before we shift focus.
                _element.Dispatcher.BeginInvoke((Action)(() => _element.Focus()), DispatcherPriority.Background);
            }

            internal void ReleaseReferences()
            {
                _element = null;
                _property = null;
            }

            private UIElement _element;

            private DependencyProperty _property;
        }

        internal static readonly DependencyProperty FocusablePropertyProperty =
            DependencyProperty.RegisterAttached(
                "FocusableProperty",
                typeof(DependencyProperty),
                typeof(FocusController),
                new UIPropertyMetadata(null, OnFocusablePropertyChanged));

        private static readonly DependencyProperty MoveFocusSinkProperty =
            DependencyProperty.RegisterAttached(
                "MoveFocusSink", typeof(MoveFocusSink), typeof(FocusController), new UIPropertyMetadata(null));
    }

    public class MoveFocusEventArgs : EventArgs
    {
        public MoveFocusEventArgs(string focusedProperty)
        {
            FocusedProperty = focusedProperty;
        }

        public string FocusedProperty { get; private set; }
    }
}