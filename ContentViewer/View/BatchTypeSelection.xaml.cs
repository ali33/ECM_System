using Ecm.ContentViewer.Model;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;

namespace Ecm.ContentViewer.View
{
    /// <summary>
    /// Interaction logic for BatchTypeSelection.xaml
    /// </summary>
    public partial class BatchTypeSelection
    {
        public static readonly DependencyProperty BatchTypesProperty = 
            DependencyProperty.Register("BatchTypes", typeof(ObservableCollection<BatchTypeModel>), typeof(BatchTypeSelection));

        public static readonly DependencyProperty SelectedBatchTypeProperty = 
            DependencyProperty.Register("SelectedBatchType", typeof(BatchTypeModel), typeof(BatchTypeSelection));

        public static readonly DependencyProperty IsAutoCreateBatchProperty = 
            DependencyProperty.Register("IsAutoCreateBatch", typeof(bool), typeof(BatchTypeSelection));

        public static readonly DependencyProperty IsReopenDialogProperty =
            DependencyProperty.Register("IsReopenDialog", typeof(bool), typeof(BatchTypeSelection));

        public ObservableCollection<BatchTypeModel> BatchTypes
        {
            get { return GetValue(BatchTypesProperty) as ObservableCollection<BatchTypeModel>; }
            set
            {
                ObservableCollection<BatchTypeModel> batchTypes = value;
                if (batchTypes != null)
                {
                    batchTypes = new ObservableCollection<BatchTypeModel>(batchTypes.OrderBy(p => p.Name));
                }

                SetValue(BatchTypesProperty, batchTypes);
            }
        }

        public DialogViewer Dialog { get; set; }

        public BatchTypeModel SelectedBatchType
        {
            get { return GetValue(SelectedBatchTypeProperty) as BatchTypeModel; }
            set
            {
                BatchTypeModel selectedbatchType = value;
                SetValue(SelectedBatchTypeProperty, selectedbatchType);
            }
        }

        public bool IsReopenDialog
        {
            get { return (bool)GetValue(IsReopenDialogProperty); }
            set { SetValue(IsReopenDialogProperty, value); }
        }

        public bool IsAutoCreateBatch
        {
            get { return (bool)GetValue(IsAutoCreateBatchProperty); }
            set { SetValue(IsAutoCreateBatchProperty, value); }
        }

        public RoutedCommand OkCommand;

        public RoutedCommand CancelCommand;

        public BatchTypeSelection()
        {
            InitializeComponent();
            OkCommand = new RoutedCommand("OKCommand", typeof(BatchTypeSelection), new InputGestureCollection { new KeyGesture(Key.Enter) });
            CommandBindings.Add(new CommandBinding(OkCommand, Ok, CanOk));

            CancelCommand = new RoutedCommand("CancelCommand", typeof(BatchTypeSelection), new InputGestureCollection { new KeyGesture(Key.Escape) });
            CommandBindings.Add(new CommandBinding(CancelCommand, Cancel));

            btnCancel.Command = CancelCommand;
            btnOK.Command = OkCommand;

            Loaded += new RoutedEventHandler(BatchTypeSelection_Loaded);
        }

        void BatchTypeSelection_Loaded(object sender, RoutedEventArgs e)
        {
            lbxBatchType.SelectedIndex = 0;
        }

        private void CanOk(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = SelectedBatchType != null;
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

        private void ItemDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Dialog.DialogResult = DialogResult.OK;
            Dialog.Close();
        }

    }
}
