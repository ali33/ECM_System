using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;

namespace Ecm.Capture.View
{
    /// <summary>
    /// Interaction logic for SearchQueryName.xaml
    /// </summary>
    public partial class SearchQueryName : IDataErrorInfo
    {
        public static readonly DependencyProperty QueryNameProperty =
           DependencyProperty.Register("QueryName", typeof(string), typeof(SearchQueryName));

        public string QueryName
        {
            get { return GetValue(QueryNameProperty) as string; }
            set { SetValue(QueryNameProperty, value); }
        }

        public Form Dialog { get; set; }

        public Guid DocumentTypeId { get; set; }

        public Func<Guid, string, bool> QueryNameExisted;

        public RoutedCommand SaveCommand;
        public RoutedCommand CancelCommand;

        public SearchQueryName()
        {
            InitializeComponent();

            SaveCommand = new RoutedCommand("Save", typeof(SearchQueryName), new InputGestureCollection { new KeyGesture(Key.Enter) });
            var commandBinding = new CommandBinding(SaveCommand, Save, CanSave);
            CommandBindings.Add(commandBinding);

            CancelCommand = new RoutedCommand("Cancel", typeof (SearchQueryName), new InputGestureCollection {new KeyGesture(Key.Escape)});
            commandBinding = new CommandBinding(CancelCommand, Cancel);
            CommandBindings.Add(commandBinding);

            ButtonSave.Command = SaveCommand;
            ButtonCancel.Command = CancelCommand;
        }

        private void CanSave(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = (QueryName + string.Empty).Trim() != string.Empty && Error == string.Empty;
        }

        private void Save(object sender, ExecutedRoutedEventArgs e)
        {
            Dialog.DialogResult = DialogResult.OK;
            Dialog.Close();
        }

        private void Cancel(object sender, ExecutedRoutedEventArgs e)
        {
            Dialog.DialogResult = DialogResult.Cancel;
            Dialog.Close();
        }

        public string Error
        {
            get { return this["QueryName"]; }
        }

        public string this[string columnName]
        {
            get
            {
                if (columnName == "QueryName")
                {
                    if (QueryNameExisted != null && QueryNameExisted(DocumentTypeId, QueryName))
                    {
                        return "This query name is already existed.";
                    }
                }

                return string.Empty;
            }
        }
    }
}
