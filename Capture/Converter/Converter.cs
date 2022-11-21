using System;
using System.Windows.Data;
using System.Windows;
using System.Globalization;
using System.Windows.Media.Imaging;
using System.IO;

using Ecm.Capture.Properties;
using System.Resources;
using System.Reflection;
using Ecm.CaptureModel;

namespace Ecm.Capture.Converter
{
    public class VisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Visibility visible = Visibility.Collapsed;
            if (parameter as string == "hidden")
            {
                visible = Visibility.Hidden;
            }

            if ((bool)value)
            {
                visible = Visibility.Visible;
            }

            return visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((Visibility)value == Visibility.Visible)
            {
                return true;
            }
            return false;
        }
    }

    public class YesNoConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (string.IsNullOrWhiteSpace(value + string.Empty))
            {
                return string.Empty;
            }

            if (Boolean.Parse(value.ToString()))
            {
                return "Yes";
            }

            return "No";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((value + string.Empty).Trim() != string.Empty)
            {
                if (value + string.Empty == "Yes")
                {
                    return "True";
                }
            }
            else
            {
                return string.Empty;
            }

            return "False";
        }
    }

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

    public class NegativeBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (targetType != typeof(bool))
            {
                throw new InvalidOperationException("The target must be a boolean");
            }

            return !(bool)value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    public class LongDateTimeConverter : IValueConverter
    {
        private readonly ResourceManager _resource = new ResourceManager("Ecm.Capture.Resources", Assembly.GetExecutingAssembly());
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (string.IsNullOrEmpty(string.Empty + value))
            {
                return null;
            }
            string[] stringFormats = new[] { "d/M/yyyy", "dd/MM/yyyy", "M/d/yyyy", "MM/dd/yyyy", "yyyyMMdd", "yyyyddMM" };

            DateTime retValue = DateTime.ParseExact(value.ToString(), stringFormats, culture, DateTimeStyles.None);
            return retValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return string.Empty;
            }

            DateTime dtConvertedValue;
            DateTime.TryParse(string.Empty + value, out dtConvertedValue);
            if (dtConvertedValue == DateTime.MinValue)
            {
                return string.Empty;
            }
            return dtConvertedValue.ToString(_resource.GetString("DateTimeFormat"));
        }
    }

    public class ShortDateTimeConverter : IValueConverter
    {
        private readonly ResourceManager _resource = new ResourceManager("Ecm.Capture.Resources", Assembly.GetExecutingAssembly());

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (string.IsNullOrEmpty(string.Empty + value))
            {
                return null;
            }
            string[] stringFormats = new[] { "d/M/yyyy", "dd/MM/yyyy", "M/d/yyyy", "MM/dd/yyyy", "yyyyMMdd", "yyyyddMM" };

            DateTime retValue = DateTime.ParseExact(value.ToString(), stringFormats, culture, DateTimeStyles.None);
            return retValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return string.Empty;
            }

            DateTime dtConvertedValue;
            DateTime.TryParse(string.Empty + value, out dtConvertedValue);
            if (dtConvertedValue == DateTime.MinValue)
            {
                return string.Empty;
            }
            return dtConvertedValue.ToString(_resource.GetString("ShortDateTimeFormat"));
        }
    }

    public class FormatShortDateStringConverter : IValueConverter
    {
        private readonly ResourceManager _resource = new ResourceManager("Ecm.Capture.Resources", Assembly.GetExecutingAssembly());

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (string.IsNullOrEmpty(string.Empty + value))
            {
                return null;
            }
            DateTime retValue = System.Convert.ToDateTime(value);
            //return retValue.ToString(_resource.GetString("ShortDateTimeFormat"));
            return retValue.ToShortDateString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    public class FormatLongDateStringConverter : IValueConverter
    {
        private readonly ResourceManager _resource = new ResourceManager("Ecm.Capture.Resources", Assembly.GetExecutingAssembly());

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (string.IsNullOrEmpty(string.Empty + value))
            {
                return null;
            }
            DateTime retValue = System.Convert.ToDateTime(value);
            //return retValue.ToString(_resource.GetString("DateTimeFormat"));
            return retValue.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    public class ImageSourceBinaryConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || ((byte[])value).Length == 0)
            {
                return null;
            }

            using (var me = new MemoryStream((byte[])value))
            {
                var image = new BitmapImage();
                image.BeginInit();
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.CreateOptions = BitmapCreateOptions.IgnoreColorProfile;
                image.StreamSource = me;
                image.EndInit();
                return image;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    public class EqualOperatorSelectedConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (value + string.Empty) == Common.EQUAL;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    public class NotEqualOperatorSelectedConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (value + string.Empty) == Common.NOT_EQUAL;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    public class BetweenOperatorSelectedConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (value + string.Empty) == Common.IN_BETWEEN;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }


}