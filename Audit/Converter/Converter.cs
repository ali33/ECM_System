using System;
using System.Windows.Data;
using System.Windows;
using System.Globalization;
using Ecm.Model;
using System.Resources;
using System.Reflection;
using System.IO;
using System.Windows.Media.Imaging;

namespace Ecm.Audit.Converter
{
    public class VisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Visibility visible = Visibility.Collapsed;

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
            var result = false;
            if (Boolean.TryParse(value.ToString(), out result))
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

    public class LongDateTimeConverter : IValueConverter
    {
        private ResourceManager resource = new ResourceManager("Ecm.Audit.Resources",Assembly.GetExecutingAssembly());
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

            DateTime dtConvertedValue;
            DateTime.TryParse(string.Empty + value, out dtConvertedValue);
            if (dtConvertedValue == DateTime.MinValue)
            {
                return string.Empty;
            }
            return dtConvertedValue.ToString(resource.GetString("DateTimeFormat"));
        }
    }

    public class ShortDateTimeConverter : IValueConverter
    {
        private ResourceManager resource = new ResourceManager("Ecm.Audit.Resources",Assembly.GetExecutingAssembly());

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (string.IsNullOrEmpty(string.Empty + value))
            {
                return null;
            }
            string[] stringFormats = new[] { "d/M/yyyy", "dd/MM/yyyy", "M/d/yyyy", "MM/dd/yyyy", "yyyyMMdd", "yyyyddMM",
                                            "d/M/yyyy hh:mm:ss tt", "d/M/yyyy hh:mm:ss","d/M/yyyy h:mm:ss tt", "d/M/yyyy h:mm:ss", "d/M/yyyy HH:mm:ss", "d/M/yyyy H:mm:ss",
                                            "dd/MM/yyyy hh:mm:ss tt", "dd/MM/yyyy hh:mm:ss","dd/MM/yyyy h:mm:ss tt", "dd/MM/yyyy h:mm:ss", "dd/MM/yyyy HH:mm:ss", "dd/MM/yyyy H:mm:ss",
                                            "M/d/yyyy hh:mm:ss tt", "M/d/yyyy hh:mm:ss","M/d/yyyy h:mm:ss tt", "M/d/yyyy h:mm:ss", "M/d/yyyy HH:mm:ss", "M/d/yyyy H:mm:ss",
                                            "MM/dd/yyyy hh:mm:ss tt", "MM/dd/yyyy hh:mm:ss","MM/dd/yyyy h:mm:ss tt", "MM/dd/yyyy h:mm:ss", "MM/dd/yyyy HH:mm:ss", "MM/dd/yyyy H:mm:ss",
                                            "yyyyMMdd hh:mm:ss tt", "yyyyMMdd hh:mm:ss","yyyyMMdd h:mm:ss tt", "yyyyMMdd h:mm:ss", "yyyyMMdd HH:mm:ss", "yyyyMMdd H:mm:ss",
                                            "yyyyddMM hh:mm:ss tt", "yyyyddMM hh:mm:ss","yyyyddMM h:mm:ss tt", "yyyyddMM h:mm:ss", "yyyyddMM HH:mm:ss", "yyyyddMM H:mm:ss"};

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
            return dtConvertedValue.ToString(resource.GetString("ShortDateTimeFormat"));
        }
    }

    public class FormatShortDateStringConverter : IValueConverter
    {
        private ResourceManager resource = new ResourceManager("Ecm.Audit.Resources",Assembly.GetExecutingAssembly());
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (string.IsNullOrEmpty(string.Empty + value))
            {
                return null;
            }
            DateTime retValue = System.Convert.ToDateTime(value);
            return retValue.ToString(resource.GetString("ShortDateTimeFormat"));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    public class FormatLongDateStringConverter : IValueConverter
    {
        private ResourceManager resource = new ResourceManager("Ecm.Audit.Resources",Assembly.GetExecutingAssembly());
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (string.IsNullOrEmpty(string.Empty + value))
            {
                return null;
            }
            DateTime retValue = System.Convert.ToDateTime(value);
            return retValue.ToString(resource.GetString("DateTimeFormat"));
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

}