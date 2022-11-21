using System;
using System.Reflection;
using System.Resources;
using System.Text;
using Ecm.Mvvm;

namespace Ecm.CaptureAdmin.ViewModel
{
    public class AboutViewModel : ComponentViewModel
    {
        private readonly ResourceManager _resource;

        public AboutViewModel()
        {
            _resource = new ResourceManager("Ecm.CaptureAdmin.Resources", Assembly.GetExecutingAssembly());
        }

        public string AboutText
        {
            get
            {
                return _resource.GetString("uiAboutText");
            }
        }

        public string CopyRightsDetail
        {
            get
            {
                string copyRightsDetail = string.Format(_resource.GetString("uiCopyrightWithYear"), DateTime.Now.Year);
                StringBuilder sb = new StringBuilder();

                sb.AppendLine(copyRightsDetail);
                sb.AppendLine(_resource.GetString("uiCopyright"));

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
                    if (customAttributes.Length > 0)
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
