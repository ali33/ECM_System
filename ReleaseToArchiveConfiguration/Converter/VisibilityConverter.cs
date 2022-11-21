using Ecm.Domain;
using System;
using System.Linq;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using Ecm.Model;

namespace Ecm.Workflow.Activities.ReleaseToArchiveConfiguration.Converter
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

    public class TableSelectedConverter : IMultiValueConverter
    {
        //public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        //{
        //    Visibility visibility = Visibility.Hidden;
        //    object[] values = value as object[];

        //    if (values != null)
        //    {
        //        string fieldName = values[0].ToString();
        //        FieldDataType dataType = (values[1] as DocumentType).FieldMetaDatas.FirstOrDefault(p => p.Name == fieldName).DataTypeEnum;

        //        if (dataType == FieldDataType.Table)
        //        {
        //            visibility = Visibility.Visible;
        //        }
        //    }

        //    return visibility;
        //}

        //public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        //{
        //    if ((Visibility)value == Visibility.Visible)
        //    {
        //        return true;
        //    }

        //    return false;
        //}

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            Visibility visibility = Visibility.Collapsed;

            if (values != null && values[0] != null)
            {
                Guid fieldId = (Guid)values[0];
                FieldMetaDataModel field = (values[1] as DocumentTypeModel).Fields.FirstOrDefault(p => p.Id == fieldId);
                
                if(field==null)
                {
                    return Visibility.Collapsed;
                }

                FieldDataType dataType = field.DataType;

                if (dataType == FieldDataType.Table)
                {
                    visibility = Visibility.Visible;
                }
            }

            return visibility;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    public class CommandParameterConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            return values;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
