using Ecm.Domain;
using System;
using System.Linq;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using Ecm.Model;

namespace Ecm.Workflow.Activities.ReleaseToArchiveConfiguration.Converter
{
    public class HaveMappingToFontWeightConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (bool)value ? FontWeights.Bold : FontWeights.Normal;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
