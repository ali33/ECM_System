using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using Ecm.Capture.View;

namespace Ecm.Capture
{
    public partial class RejectNotesView
    {
        public static readonly DependencyProperty NotesProperty =
            DependencyProperty.Register("Notes", typeof(string), typeof(RejectNotesView));

        public DialogBaseView Dialog { get; set; }

        public string Notes
        {
            get { return GetValue(NotesProperty) as string; }
            set { SetValue(NotesProperty, value); }
        }

        public RoutedCommand OkCommand;

        public RoutedCommand CancelCommand;

        public RejectNotesView()
        {
            InitializeComponent();

            OkCommand = new RoutedCommand("OKCommand", typeof(RejectNotesView), new InputGestureCollection { new KeyGesture(Key.Enter) });
            CommandBindings.Add(new CommandBinding(OkCommand, Ok, CanOk));

            var keys = new KeyGesture(Key.Escape);
            CancelCommand = new RoutedCommand("CancelCommand", typeof(RejectNotesView),
                                              new InputGestureCollection { keys });
            CommandBindings.Add(new CommandBinding(CancelCommand, Cancel));

            ButtonOK.Command = OkCommand;
            ButtonCancel.Command = CancelCommand;
        }

        private void CanOk(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = !string.IsNullOrEmpty((Notes + string.Empty).Trim());
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
