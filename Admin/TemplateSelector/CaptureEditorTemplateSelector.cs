using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using Ecm.Admin.ViewModel;
using Ecm.Model;
using Ecm.Domain;

namespace Ecm.Admin.TemplateSelector
{
    public class CaptureEditorTemplateSelector : DataTemplateSelector
    {
        public DataTemplate StringEditorTemplate { get; set; }

        public DataTemplate DecimalEditorTemplate { get; set; }

        public DataTemplate IntegerEditorTemplate { get; set; }

        public DataTemplate DateTimeEditorTemplate { get; set; }

        public DataTemplate PickListEditorTemplate { get; set; }

        public DataTemplate LookupStringEditorTemplate { get; set; }

        public DataTemplate LookupDecimalEditorTemplate { get; set; }

        public DataTemplate LookupIntegerEditorTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item is FieldMetaDataModel)
            {
                FieldMetaDataModel field = item as FieldMetaDataModel;

                switch (field.DataType)
                {
                    case FieldDataType.Integer:
                        if (field.IsLookup)
                        {
                            return LookupIntegerEditorTemplate;
                        }
                        return IntegerEditorTemplate;
                    case FieldDataType.Decimal:
                        if (field.IsLookup)
                        {
                            return LookupDecimalEditorTemplate;
                        }
                        return DecimalEditorTemplate;
                    case FieldDataType.String:
                        if (field.IsLookup)
                        {
                            return LookupStringEditorTemplate;
                        }
                        return StringEditorTemplate;
                    case FieldDataType.Date:
                        return DateTimeEditorTemplate;
                    case FieldDataType.Picklist:
                        return PickListEditorTemplate;
                }

            }

            return null;
        }
    }
}
