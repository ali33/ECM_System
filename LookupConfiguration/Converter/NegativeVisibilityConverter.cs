using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Resources;
using System.Reflection;

namespace Ecm.Workflow.Activities.LookupConfiguration.Converter
{
    public class NegativeVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Visibility visible = Visibility.Visible;

            if ((bool)value)
            {
                visible = Visibility.Collapsed;
            }

            return visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
    public class YesNoConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (Boolean.Parse(value.ToString()))
            {
                return "Yes";
            }
            return "No";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class LongDateTimeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (string.IsNullOrEmpty(string.Empty + value))
            {
                return null;
            }
            DateTime retValue = System.Convert.ToDateTime(value);
            return retValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return string.Empty;
            }

            DateTime dtConvertedValue = DateTime.MinValue;
            DateTime.TryParse(string.Empty + value, out dtConvertedValue);
            if (dtConvertedValue == DateTime.MinValue)
            {
                return string.Empty;
            }
            ResourceManager resource = new ResourceManager("Ecm.Workflow.Activities.LookupConfiguration.Resource", Assembly.GetExecutingAssembly());

            return dtConvertedValue.ToString(resource.GetString("DateTimeFormat"));
        }
    }

    public class ShortDateTimeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (string.IsNullOrEmpty(string.Empty + value))
            {
                return null;
            }
            DateTime retValue = System.Convert.ToDateTime(value);
            return retValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return string.Empty;
            }

            DateTime dtConvertedValue = DateTime.MinValue;
            DateTime.TryParse(string.Empty + value, out dtConvertedValue);
            if (dtConvertedValue == DateTime.MinValue)
            {
                return string.Empty;
            }
            ResourceManager resource = new ResourceManager("Ecm.Workflow.Activities.LookupConfiguration.Resource", Assembly.GetExecutingAssembly());

            return dtConvertedValue.ToString(resource.GetString("ShortDateTimeFormat"));
        }
    }

    public class FormatShortDateStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (string.IsNullOrEmpty(string.Empty + value))
            {
                return null;
            }

            DateTime retValue = System.Convert.ToDateTime(value);
            ResourceManager resource = new ResourceManager("Ecm.Workflow.Activities.LookupConfiguration.Resource", Assembly.GetExecutingAssembly());
            return retValue.ToString(resource.GetString("ShortDateTimeFormat"));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    public class FormatLongDateStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (string.IsNullOrEmpty(string.Empty + value))
            {
                return null;
            }

            DateTime retValue = System.Convert.ToDateTime(value);
            ResourceManager resource = new ResourceManager("Ecm.Workflow.Activities.LookupConfiguration.Resource", Assembly.GetExecutingAssembly());
            return retValue.ToString(resource.GetString("DateTimeFormat"));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

}
