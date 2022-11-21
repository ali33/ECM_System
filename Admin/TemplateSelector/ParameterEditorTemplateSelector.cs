using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using Ecm.Model;

namespace Ecm.Admin.TemplateSelector
{
    public class ParameterEditorTemplateSelector : DataTemplateSelector
    {
        public DataTemplate StringEditorTemplate { get; set; }

        public DataTemplate DecimalEditorTemplate { get; set; }

        public DataTemplate IntegerEditorTemplate { get; set; }

        public DataTemplate DateTimeEditorTemplate { get; set; }

        public DataTemplate BooleanEditorTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item is ParameterModel)
            {
                ParameterModel para = item as ParameterModel;
                switch (para.ParameterType)
                {
                    case "int":
                        return IntegerEditorTemplate;
                    case "decimal":
                        return DecimalEditorTemplate;
                    case "varchar":
                    case "text":
                    case "nvarchar":
                    case "ntext":
                        return StringEditorTemplate;
                    case "date":
                    case "datetime":
                        return DateTimeEditorTemplate;
                    case "bit":
                        return BooleanEditorTemplate;
                }
            }

            return null;
        }
    }
}
