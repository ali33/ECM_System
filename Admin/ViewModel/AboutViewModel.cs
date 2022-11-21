using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ecm.Mvvm;
using System.Reflection;
using System.Resources;
using System.Windows.Input;

namespace Ecm.Admin.ViewModel
{
    public class AboutViewModel : ComponentViewModel
    {
        private ResourceManager _resource;
        private MainViewModel _mainViewModel;
        private RelayCommand _showSupportCommand;

        public AboutViewModel(MainViewModel mainViewModel)
        {
            _resource = new ResourceManager("Ecm.Admin.Resources", Assembly.GetExecutingAssembly());
            _mainViewModel = mainViewModel;
        }

        public ICommand ShowSupportCommand
        {
            get
            {
                if (_showSupportCommand == null)
                {
                    _showSupportCommand = new RelayCommand(p => DisplaySupportView());
                }
                return _showSupportCommand;
            }
        }

        private void DisplaySupportView()
        {
            _mainViewModel.ViewModel = new SupportViewModel();
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
                ResourceManager resource = new ResourceManager("Ecm.Admin.Resources", Assembly.GetExecutingAssembly());

                string copyRightsDetail = string.Format(resource.GetString("uiCopyrightWithYear"), DateTime.Now.Year);
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
