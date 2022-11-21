using System;
using System.Activities.Presentation;
using System.Activities.Presentation.Toolbox;
using System.ComponentModel;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace Ecm.WorkflowDesigner.Converter
{
    public class ToolboxItemWrapperIconConverter : IValueConverter
    {
        public string DefaultResource { get; set; }

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            Drawing retVal = null;

            ToolboxItemWrapper wrapper = value as ToolboxItemWrapper;

            if (wrapper != null)
            {
                // Generate the resource name
                string resourceName = GenerateResourceName(wrapper.Type);

                // And lookup in app resources
                DrawingBrush icon = Application.Current.Resources[resourceName] as DrawingBrush;

                // No icon found, now try the designer - this is really the last resort
                if (icon == null)
                {
                    // Get the [Designer] for the passed activity
                    DesignerAttribute designer = Attribute.GetCustomAttribute(wrapper.Type, typeof (DesignerAttribute)) as DesignerAttribute;

                    if (designer != null)
                    {
                        ActivityDesigner ad = Activator.CreateInstance(Type.GetType(designer.DesignerTypeName)) as ActivityDesigner;
                        if (ad != null)
                        {
                            icon = ad.Icon;
                        }
                    }
                }

                // If not found, provide a fallback
                if (icon == null)
                {
                    icon = Application.Current.Resources[DefaultResource] as DrawingBrush;
                }

                if (icon != null)
                {
                    retVal = icon.Drawing;
                }
            }

            return retVal;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        private string GenerateResourceName(Type type)
        {
            string resource = DefaultResource;

            if (type.IsGenericType)
            {
                resource = string.Concat(type.Name.Substring(0, type.Name.IndexOf('`')), "Icon");
            }
            else
            {
                switch (type.Name)
                {
                    case "Flowchart":
                        resource = "FlowChartIcon";
                        break;
                    case "TransactedReceiveScope":
                        resource = "TransactionReceiveScopeIcon";
                        break;
                    default:
                        resource = string.Concat(type.Name, "Icon");
                        break;
                }
            }

            return resource;
        }
    }

}
