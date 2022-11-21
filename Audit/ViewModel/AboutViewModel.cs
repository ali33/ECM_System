using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ecm.Mvvm;
using System.Reflection;

namespace Ecm.Audit.ViewModel
{
    public class AboutViewModel : ComponentViewModel
    {
        public string AboutText
        {
            get
            {
                return Resources.uiAboutText;
            }
        }

        public string CopyRightsDetail
        {
            get
            {
                string copyRightsDetail = string.Format(Resources.uiCopyrightWithYear, DateTime.Now.Year);
                StringBuilder sb = new StringBuilder();

                sb.AppendLine(copyRightsDetail);
                sb.AppendLine(Resources.uiCopyright);

                return sb.ToString();
            }
        }

        public string ProductName
        {
            get
            {
                Assembly assembly = Assembly.GetEntryAssembly();
                string productName = string.Empty;

                if (assembly != null)
                {
                    object[] customAttributes = assembly.GetCustomAttributes(typeof(AssemblyProductAttribute), false);
                    if ((customAttributes != null) && (customAttributes.Length > 0))
                    {
                        productName = ((AssemblyProductAttribute)customAttributes[0]).Product;
                    }

                    if (string.IsNullOrEmpty(productName))
                    {
                        productName = string.Empty;
                    }
                }

                return productName;
            }
        }

        public string ProductVersion
        {
            get
            {
                Assembly assembly = Assembly.GetEntryAssembly();
                Version version = assembly.GetName().Version;
                return version.ToString();
            }
        }

    }
}
