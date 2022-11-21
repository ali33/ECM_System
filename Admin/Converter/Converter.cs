using System;
using System.Windows.Data;
using System.Windows;
using System.Globalization;
using Ecm.Domain;
using System.Resources;
using System.Reflection;
using System.IO;
using System.Windows.Media.Imaging;
using Ecm.Model;
using System.Collections.Generic;

namespace Ecm.Admin.Converter
{
    public class HasLookupConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var field = (FieldMetaDataModel)value;

            if (field == null || field.DocTypeId == Guid.Empty)
            {
                return Visibility.Hidden;
            }

            if (field.DataType == FieldDataType.Boolean || field.DataType == FieldDataType.Date || field.DataType == FieldDataType.Folder || field.DataType == FieldDataType.Picklist || field.DataType == FieldDataType.Table)
            {
                return Visibility.Hidden;
            }

            return Visibility.Visible;
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

    public class BooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null)
            {
                return Boolean.Parse(value.ToString());
            }

            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Boolean.Parse(value.ToString());
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
            if (value + string.Empty == "In Between")
            {
                return true;
            }

            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    /// <summary>
    /// This class include method that support convert value to boolean. If value is equal to "=" then return true, otherwise return false
    /// </summary>
    public class EqualOperatorSelectedConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (value + string.Empty) == "=";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    /// <summary>
    /// This class include method that support convert value to boolean. If value is equal to "Not Equal" then return true, otherwise return false
    /// </summary>
    public class NotEqualOperatorSelectedConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (value + string.Empty) == "Not Equal";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    /// <summary>
    /// This class include method that support convert target type to decimal and to the contrary
    /// </summary>
    public class DecimalSelectedConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (value + "").Equals("Decimal", StringComparison.OrdinalIgnoreCase);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return "Decimal";
        }
    }

    /// <summary>
    /// This class include method that support convert target type to String and to the contrary
    /// </summary>
    public class StringSelectedConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (value + "").Equals("String", StringComparison.OrdinalIgnoreCase);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return "String";
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
            ResourceManager resource = new ResourceManager("Ecm.Admin.Resources", Assembly.GetExecutingAssembly());

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
            ResourceManager resource = new ResourceManager("Ecm.Admin.Resources", Assembly.GetExecutingAssembly());

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
            ResourceManager resource = new ResourceManager("Ecm.Admin.Resources", Assembly.GetExecutingAssembly());
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
            ResourceManager resource = new ResourceManager("Ecm.Admin.Resources", Assembly.GetExecutingAssembly());
            return retValue.ToString(resource.GetString("DateTimeFormat"));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    public class TableSelectedConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Visibility visibility = Visibility.Collapsed;
            if (value + "" == "Table")
            {
                visibility = Visibility.Visible;
            }

            return visibility;
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

    public class TableNotSelectedConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return !(value + "").Equals("Table", StringComparison.OrdinalIgnoreCase);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class PicklistSelectedConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Visibility visibility = Visibility.Hidden;
            if (value + "" == "Picklist")
            {
                visibility = Visibility.Visible;
            }

            return visibility;
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

    public class DateSelectedConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (value + "").Equals("Current Date", StringComparison.OrdinalIgnoreCase);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((bool)value == false) return "";
            else return "Current Date";
        }
    }

    public class BooleanSelectedConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value + "" == "True") return "True";
            else return "False";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value + "" == "True") return "True";
            else return "False";
        }
    }

    public class IndexDataTypeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            FieldDataType dataType = (FieldDataType)value;
            string indexDataType = string.Empty;

            switch (dataType)
            {
                case FieldDataType.Integer:
                    indexDataType = "Integer";
                    break;
                case FieldDataType.Decimal:
                    indexDataType = "Decimal";
                    break;
                case FieldDataType.String:
                    indexDataType = "String";
                    break;
                case FieldDataType.Date:
                    indexDataType = "Date";
                    break;
                case FieldDataType.Picklist:
                    indexDataType = "Picklist";
                    break;
                case FieldDataType.Table:
                    indexDataType = "Table";
                    break;
                case FieldDataType.Boolean:
                    indexDataType = "Boolean";
                    break;
            }

            return indexDataType;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

}
