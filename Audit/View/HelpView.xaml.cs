using System;
using System.Windows.Controls;
using System.Reflection;

namespace Ecm.Audit.View
{
    /// <summary>
    /// Interaction logic for HelpView.xaml
    /// </summary>
    public partial class HelpView : UserControl
    {
        public HelpView()
        {
            InitializeComponent();
            webBrowser.Source = new Uri("file:///" + System.IO.Path.Combine(System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Help.htm"));
        }
    }
}
