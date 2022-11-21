using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using Ecm.Workflow.Activities.CustomActivityModel;

namespace Ecm.Workflow.Activities.LookupConfiguration.TemplateSelector
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
                    case "integer":
                    case "tinyint":
                    case "smallint":
                    case "bigint":
                    case "long":
                    case "real":
                    case "double":
                    case "double precision":
                        return IntegerEditorTemplate;
                    case "number":
                    case "decimal":
                    case "float":
                    case "numberic":
                        return DecimalEditorTemplate;
                    case "varchar":
                    case "text":
                    case "nvarchar":
                    case "ntext":
                    case "longtest":
                    case "tinytext":
                    case "mediumtext":
                    case "varchar2":
                    case "nvarchar2":
                    case "char":
                    case "character varying":
                        return StringEditorTemplate;
                    case "date":
                    case "datetime":
                        return DateTimeEditorTemplate;
                    case "boolean":
                    case "bool":
                    case "bit":
                        return BooleanEditorTemplate;
                }
            }

            return null;
        }
    }
}
