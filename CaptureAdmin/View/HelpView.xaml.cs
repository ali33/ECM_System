using System;
using System.Reflection;
namespace Ecm.CaptureAdmin.View
{
    /// <summary>
    /// Interaction logic for HelpView.xaml
    /// </summary>
    public partial class HelpView
    {
        public HelpView()
        {
            InitializeComponent();
            webBrowser.Source = new Uri("file:///" + System.IO.Path.Combine(System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Help.htm"));
        }
    }
}
