using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;

namespace Ecm.CaptureViewer
{
    /// <summary>
    /// Interaction logic for TextViewer.xaml
    /// </summary>
    public partial class TextViewer
    {
        public static readonly DependencyProperty FilePathProperty =
        DependencyProperty.Register("FilePath", typeof(string), typeof(TextViewer));
        private string _format = DataFormats.Text;

        public TextViewer()
        {
            InitializeComponent();
        }

        public string FilePath
        {
            get { return GetValue(FilePathProperty) as string; }
            set { 
                SetValue(FilePathProperty, value);
                LoadTextDocument(value);
            }
        }

        public void LoadTextDocument(string fileName)
        {
            string text = string.Empty;
            if (System.IO.File.Exists(fileName))
            {
                FileInfo info = new FileInfo(fileName);

                using (MemoryStream memoryStream = new MemoryStream(File.ReadAllBytes(fileName)))
                {
                    TextRange range = new TextRange(txtContent.Document.ContentStart, txtContent.Document.ContentEnd);
                    range.Load(memoryStream, DataFormats.Rtf);
                }
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            LoadTextDocument(FilePath);
        }
    }
}
