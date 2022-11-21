using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using Ecm.Model;
using Ecm.Domain;
using System.Windows.Controls;

namespace Ecm.CustomAddin.TemplateSelector
{
    public class FieldEditorTemplateSelector : DataTemplateSelector
    {
        public DataTemplate BooleanEditorTemplate { get; set; }

        public DataTemplate DateTimeEditorTemplate { get; set; }

        public DataTemplate DecimalEditorTemplate { get; set; }

        public DataTemplate FolderEditorTemplate { get; set; }

        public DataTemplate IntegerEditorTemplate { get; set; }

        public DataTemplate LookupDecimalEditorTemplate { get; set; }

        public DataTemplate LookupIntegerEditorTemplate { get; set; }

        public DataTemplate LookupStringEditorTemplate { get; set; }

        public DataTemplate PickListEditorTemplate { get; set; }

        public DataTemplate StringEditorTemplate { get; set; }

        public DataTemplate TableEditorTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            var field = item as FieldValueModel;
            if (field != null)
            {
                switch (field.Field.DataType)
                {
                    case FieldDataType.Integer:
                        if (field.Field.IsLookup)
                        {
                            return LookupIntegerEditorTemplate;
                        }
                        return IntegerEditorTemplate;
                    case FieldDataType.Decimal:
                        if (field.Field.IsLookup)
                        {
                            return LookupDecimalEditorTemplate;
                        }
                        return DecimalEditorTemplate;
                    case FieldDataType.String:
                        if (field.Field.IsLookup)
                        {
                            return LookupStringEditorTemplate;
                        }
                        return StringEditorTemplate;
                    case FieldDataType.Date:
                        return DateTimeEditorTemplate;
                    case FieldDataType.Picklist:
                        return PickListEditorTemplate;
                    case FieldDataType.Boolean:
                        return BooleanEditorTemplate;
                    case FieldDataType.Folder:
                        return FolderEditorTemplate;
                    case FieldDataType.Table:
                        return TableEditorTemplate;
                }
            }

            return null;
        }
    }
}
