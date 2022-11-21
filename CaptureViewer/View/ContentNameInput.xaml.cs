using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using Ecm.CaptureDomain;
using Ecm.CaptureModel;

namespace Ecm.CaptureViewer
{
    /// <summary>
    /// Interaction logic for BatchTypeSelection.xaml
    /// </summary>
    public partial class ContentNameInput
    {
        public static readonly DependencyProperty ContentNameProperty =
            DependencyProperty.Register("ContentName", typeof(string), typeof(ContentNameInput));

        public RoutedCommand OkCommand;
        public RoutedCommand CancelCommand;

        public DialogViewer Dialog { get; set; }

        public string ContentName
        {
            get { return GetValue(ContentNameProperty) as string; }
            set { SetValue(ContentNameProperty, value); }
        }

        public ContentNameInput()
        {
            InitializeComponent();
            OkCommand = new RoutedCommand("OKCommand", typeof(BatchTypeSelection), new InputGestureCollection { new KeyGesture(Key.Enter) });
            CommandBindings.Add(new CommandBinding(OkCommand, Ok, CanOk));

            CancelCommand = new RoutedCommand("CancelCommand", typeof(BatchTypeSelection), new InputGestureCollection { new KeyGesture(Key.Escape) });
            CommandBindings.Add(new CommandBinding(CancelCommand, Cancel));

            ButtonOK.Command = OkCommand;
            ButtonCancel.Command = CancelCommand;
        }

        private void CanOk(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = string.Format("{0}", this.ContentName).Trim() != string.Empty;
        }

        private void Ok(object sender, ExecutedRoutedEventArgs e)
        {
            Dialog.DialogResult = DialogResult.OK;
            Dialog.Close();
        }

        private void Cancel(object sender, ExecutedRoutedEventArgs e)
        {
            Dialog.DialogResult = DialogResult.Cancel;
            Dialog.Close();
        }
    }
}
