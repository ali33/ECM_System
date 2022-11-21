using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using Ecm.Model;

namespace Ecm.Admin.TemplateSelector
{
    public class AddEditButtonTemplateSelector : DataTemplateSelector
    {
        public DataTemplate EditButton { get; set; }
        public DataTemplate AddButton { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item is FieldMetaDataModel)
            {
                FieldMetaDataModel field = item as FieldMetaDataModel;

                if (field.IsLookup)
                    return EditButton;
                else
                    return AddButton;
            }

            return AddButton;
        }
    }
}
