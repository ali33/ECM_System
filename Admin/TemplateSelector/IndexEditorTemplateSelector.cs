using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Data;
using System.Globalization;
using Ecm.Model;
using Ecm.Domain;

namespace Ecm.Admin.TemplateSelector
{
    /// <summary>
    /// This class define the template for the editor of indexes in document tab
    /// </summary>
    public class IndexEditorTemplateSelector : DataTemplateSelector
    {
        public DataTemplate StringEditorTemplate { get; set; }

        public DataTemplate DecimalEditorTemplate { get; set; }

        public DataTemplate IntegerEditorTemplate { get; set; }

        public DataTemplate DateTimeEditorTemplate { get; set; }

        public DataTemplate PickListEditorTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item is FieldMetaDataModel)
            {
                FieldMetaDataModel field = item as FieldMetaDataModel;

                switch (field.DataType)
                {
                    case FieldDataType.Integer:
                        return IntegerEditorTemplate;
                    case FieldDataType.Decimal:
                        return DecimalEditorTemplate;
                    case FieldDataType.String:
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
