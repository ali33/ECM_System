using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Ecm.ImportData
{
    /// <summary>
    /// Interaction logic for ShowLog.xaml
    /// </summary>
    public partial class ShowLog : Window
    {
        public ShowLog()
        {
            InitializeComponent();

            String appStartPath = System.IO.Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);
            DirectoryInfo dir = new DirectoryInfo(System.IO.Path.Combine(appStartPath, "Log"));
            cmbFileName.ItemsSource = dir.GetFiles();
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (((ComboBox)sender).SelectedItem != null)
            {
                FileInfo file = (FileInfo)((ComboBox)sender).SelectedItem;
                rtfValue.AppendText(File.ReadAllText(file.FullName));
            }

        }
    }
}
