using Ecm.Model;
using Ecm.Workflow.Activities.CustomActivityModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Data;

namespace Ecm.Workflow.Activities.ReleaseToArchiveConfiguration.Converter
{
    public class MappingColumnTypeConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var mappingModel = (values[2] as ListViewItem).Content as MappingFieldModel;
            if (mappingModel == null)
            {
                return null;
            }

            var archiveFields = values[0] as ObservableCollection<FieldMetaDataModel>;
            var captureFields = (values[1] as CaptureModel.DocTypeModel).Fields;

            var captureField = captureFields.FirstOrDefault(h => h.Id == mappingModel.CaptureFieldId);
            if (captureField == null)
            {
                return null;
            }

            switch (captureField.DataType)
            {
                case Ecm.CaptureDomain.FieldDataType.String:
                    return new ObservableCollection<FieldMetaDataModel>(archiveFields.Where(h => 
                        h.Id == Guid.Empty
                        || h.DataType == Domain.FieldDataType.String));
                case Ecm.CaptureDomain.FieldDataType.Integer:
                    return new ObservableCollection<FieldMetaDataModel>(archiveFields.Where(h =>
                        h.Id == Guid.Empty
                        || h.DataType == Domain.FieldDataType.String
                        || h.DataType == Domain.FieldDataType.Integer
                        || h.DataType == Domain.FieldDataType.Decimal));
                case Ecm.CaptureDomain.FieldDataType.Decimal:
                    return new ObservableCollection<FieldMetaDataModel>(archiveFields.Where(h =>
                        h.Id == Guid.Empty
                        || h.DataType == Domain.FieldDataType.String
                        || h.DataType == Domain.FieldDataType.Decimal));
                case Ecm.CaptureDomain.FieldDataType.Picklist:
                    return new ObservableCollection<FieldMetaDataModel>(archiveFields.Where(h =>
                        h.Id == Guid.Empty
                        || h.DataType == Domain.FieldDataType.String
                        || h.DataType == Domain.FieldDataType.Picklist));
                case Ecm.CaptureDomain.FieldDataType.Boolean:
                    return new ObservableCollection<FieldMetaDataModel>(archiveFields.Where(h =>
                        h.Id == Guid.Empty
                        || h.DataType == Domain.FieldDataType.String
                        || h.DataType == Domain.FieldDataType.Boolean));
                case Ecm.CaptureDomain.FieldDataType.Date:
                    return new ObservableCollection<FieldMetaDataModel>(archiveFields.Where(h =>
                        h.Id == Guid.Empty
                        || h.DataType == Domain.FieldDataType.String
                        || h.DataType == Domain.FieldDataType.Date));
                case Ecm.CaptureDomain.FieldDataType.Table:
                    return new ObservableCollection<FieldMetaDataModel>(archiveFields.Where(h =>
                        h.Id == Guid.Empty
                        || h.DataType == Domain.FieldDataType.Table));
                default:
                    return new ObservableCollection<FieldMetaDataModel>(archiveFields.Where(h =>
                        h.Id == Guid.Empty));
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class MappingTableTypeConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var mappingModel = (values[2] as ListViewItem).Content as MappingFieldModel;
            if (mappingModel == null)
            {
                return null;
            }

            var archiveFields = values[0] as ObservableCollection<Ecm.Model.TableColumnModel>;
            var captureFields = values[1] as ObservableCollection<CaptureModel.TableColumnModel>;

            var captureField = captureFields.FirstOrDefault(h => h.FieldId == mappingModel.CaptureFieldId);
            if (captureField == null)
            {
                return null;
            }

            switch (captureField.DataType)
            {
                case Ecm.CaptureDomain.FieldDataType.String:
                    return new ObservableCollection<Ecm.Model.TableColumnModel>(archiveFields.Where(h =>
                        h.FieldId == Guid.Empty
                        || h.DataType == Domain.FieldDataType.String));
                case Ecm.CaptureDomain.FieldDataType.Integer:
                    return new ObservableCollection<Ecm.Model.TableColumnModel>(archiveFields.Where(h =>
                        h.FieldId == Guid.Empty
                        || h.DataType == Domain.FieldDataType.String
                        || h.DataType == Domain.FieldDataType.Integer
                        || h.DataType == Domain.FieldDataType.Decimal));
                case Ecm.CaptureDomain.FieldDataType.Decimal:
                    return new ObservableCollection<Ecm.Model.TableColumnModel>(archiveFields.Where(h =>
                        h.FieldId == Guid.Empty
                        || h.DataType == Domain.FieldDataType.String
                        || h.DataType == Domain.FieldDataType.Decimal));
                case Ecm.CaptureDomain.FieldDataType.Date:
                    return new ObservableCollection<Ecm.Model.TableColumnModel>(archiveFields.Where(h =>
                        h.FieldId == Guid.Empty
                        || h.DataType == Domain.FieldDataType.String
                        || h.DataType == Domain.FieldDataType.Date));
                default:
                    return new ObservableCollection<Ecm.Model.TableColumnModel>(archiveFields.Where(h =>
                        h.FieldId == Guid.Empty));
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
