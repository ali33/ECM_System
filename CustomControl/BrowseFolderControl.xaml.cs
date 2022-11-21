using System.Windows;

namespace Ecm.CustomControl
{
    public partial class BrowseFolderControl
    {
        public static readonly DependencyProperty FolderProperty =
            DependencyProperty.Register("Folder", typeof(string), typeof(BrowseFolderControl));

        public BrowseFolderControl()
        {
            InitializeComponent();
        }

        public string Folder
        {
            get { return GetValue(FolderProperty) as string; }
            set { SetValue(FolderProperty, value); }
        }

        private void ButtonClick(object sender, RoutedEventArgs e)
        {
            var folderDialog = new System.Windows.Forms.FolderBrowserDialog();
            System.Windows.Forms.DialogResult result = folderDialog.ShowDialog();

            if (result == System.Windows.Forms.DialogResult.Yes || result == System.Windows.Forms.DialogResult.OK)
            {
                Folder = folderDialog.SelectedPath;
            }
        }
    }
}
