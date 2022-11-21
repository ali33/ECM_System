using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace Ecm.CustomControl
{
    public partial class ColorPicker
    {
        public static DependencyProperty CurrentColorProperty =
            DependencyProperty.Register("CurrentColor", typeof(SolidColorBrush), typeof(ColorPicker), new PropertyMetadata(Brushes.Black));

        public static RoutedUICommand SelectColorCommand = new RoutedUICommand("SelectColorCommand", "SelectColorCommand", typeof(ColorPicker));

        public ColorPicker()
        {
            DataContext = this;
            InitializeComponent();
            CommandBindings.Add(new CommandBinding(SelectColorCommand, SelectColorCommandExecute));
        }

        public SolidColorBrush CurrentColor
        {
            get { return (SolidColorBrush)GetValue(CurrentColorProperty); }
            set { SetValue(CurrentColorProperty, value); }
        }

        private void SelectColorCommandExecute(object sender, ExecutedRoutedEventArgs e)
        {
            var convertFromString = ColorConverter.ConvertFromString(e.Parameter.ToString());
            if (convertFromString != null)
            {
                CurrentColor = new SolidColorBrush((Color) convertFromString);
            }
        }

        private void ButtonClick(object sender, RoutedEventArgs e)
        {
            popup.IsOpen = false;
            e.Handled = false;
        }
    }
}