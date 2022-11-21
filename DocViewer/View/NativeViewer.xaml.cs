using System.Windows;

namespace Ecm.DocViewer
{
    public partial class NativeViewer
    {
        public static readonly DependencyProperty FilePathProperty =
           DependencyProperty.Register("FilePath", typeof(string), typeof(NativeViewer));

        public NativeViewer()
        {
            InitializeComponent();
        }

        public string FilePath
        {
            get { return GetValue(FilePathProperty) as string; }
            set { SetValue(FilePathProperty, value); }
        }
    }
}
