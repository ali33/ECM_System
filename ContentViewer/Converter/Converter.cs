using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows;
using System.Windows.Controls;
using System.Linq;

using Ecm.ContentViewer.Extension;
using Ecm.ContentViewer.Model;
using Ecm.ContentViewer.Properties;

namespace Ecm.ContentViewer.Converter
{
    public class BoolVisibilityConverter : IValueConverter
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

    public class NegativeVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var visible = (Visibility) value;
            if (visible == Visibility.Visible)
            {
                return Visibility.Collapsed;
            }

            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    public class StringBoolVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Visibility visible = Visibility.Visible;

            if (value + string.Empty == string.Empty)
            {
                visible = Visibility.Collapsed;
            }

            return visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    public class ThumbnailMarginMultiplierConverter : IValueConverter
    {
        public double Length { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var item = value as TreeViewItem;
            if (item == null)
            {
                return new Thickness(0);
            }

            return new Thickness(Length * item.GetDepth(), 0, 0, 0);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return 0;
        }
    }

    public class NegativeBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return !(bool)value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    public class FileTypeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var fileFormat = (FileFormatModel)new FileFormatConverter().Convert(value, null, null, null);
            if (fileFormat == FileFormatModel.Tif || 
                fileFormat == FileFormatModel.Png || 
                fileFormat == FileFormatModel.Gif || 
                fileFormat == FileFormatModel.Bmp || 
                fileFormat == FileFormatModel.Jpg)
            {
                return FileTypeModel.Image;
            }

            if (fileFormat == FileFormatModel.Media)
            {
                return FileTypeModel.Media;
            }

            if (fileFormat == FileFormatModel.Rtf || fileFormat == FileFormatModel.Txt || fileFormat == FileFormatModel.Log)
            {
                return FileTypeModel.Text;
            }

            return FileTypeModel.Native;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    public class FileFormatConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var fileType = value as string;
            if (fileType != null)
            {
                fileType = fileType.Replace(".", string.Empty).ToLower().Trim();
            }

            var mediaExtensions = new[] { "aiff", "asf", "au", "avi", "dvr-ms", "m1v", "mid", 
                                          "midi", "mp3", "mp4", "mpe", "mpeg", 
                                          "mpg", "rmi", "vob", "wav", "wm", "wma", 
                                          "wmv", "dat", "flv",
                                          "m4v", "mov", "3gp", "3g2", "m2v"};
            if (mediaExtensions.Contains(fileType))
            {
                return FileFormatModel.Media;
            }

            switch (fileType)
            {
                case "tif":
                case "tiff":
                    return FileFormatModel.Tif;
                case "png":
                    return FileFormatModel.Png;
                case "gif":
                    return FileFormatModel.Gif;
                case "bmp":
                    return FileFormatModel.Bmp;
                case "jpg":
                case "jpeg":
                    return FileFormatModel.Jpg;
                case "doc":
                case "docx":
                    return FileFormatModel.Doc;
                case "pdf":
                    return FileFormatModel.Pdf;
                case "xls":
                case "xlsx":
                    return FileFormatModel.Xls;
                case "xml":
                    return FileFormatModel.Xml;
                case "txt":
                    return FileFormatModel.Txt;
                case "ppt":
                case "pptx":
                    return FileFormatModel.Ppt;
                case "zip":
                case "rar":
                case "tar":
                case "7z":
                case "cab":
                case "gzip":
                    return FileFormatModel.Zip;
                case "html":
                case "htm":
                    return FileFormatModel.Html;
                case "xps":
                    return FileFormatModel.Xps;
                case "log":
                    return FileFormatModel.Log;
                case "rtf":
                    return FileFormatModel.Rtf;
                default:
                    return FileFormatModel.Unknown;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
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

            string[] stringFormats = new[] { "d/M/yyyy", "dd/MM/yyyy", "M/d/yyyy", "MM/dd/yyyy", "yyyyMMdd", "yyyyddMM" };

            string date = value.ToString();
            int index = date.IndexOf(" ");

            if (index != -1)
            {
                date = date.Substring(0, index);
            }

            DateTime retValue = DateTime.ParseExact(date, stringFormats, culture, DateTimeStyles.None);
            return retValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            DateTime dtConvertedValue;
            if (value == null)
            {
                return string.Empty;
            }

            DateTime.TryParse(string.Empty + value, out dtConvertedValue);
            if (dtConvertedValue == DateTime.MinValue)
            {
                return string.Empty;
            }

            return dtConvertedValue.ToString(Resources.ShortDateTimeFormat);
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
            DateTime dtConvertedValue;
            if (value == null)
            {
                return string.Empty;
            }

            DateTime.TryParse(string.Empty + value, out dtConvertedValue);
            if (dtConvertedValue == DateTime.MinValue)
            {
                return string.Empty;
            }
            return dtConvertedValue.ToString(Resources.LongDateTimeFormat);
        }
    }

    public class YesNoConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((value + string.Empty).Trim() != string.Empty)
            {
                if (Boolean.Parse(value.ToString()))
                {
                    return "Yes";
                }
            }

            return "No";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class NullPathConverter : IValueConverter
    {
        public object Convert(object value, Type targerType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return new Uri("pack://application:,,,/ContentViewer;component/Resources/Images/mute.gif");
            }

            return new Uri(value.ToString());
        }

        public object ConvertBack(object value, Type targerType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

}
