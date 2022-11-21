using System;
using System.Linq;
using System.Windows.Input;
using Ecm.CaptureDomain;
using Ecm.CaptureModel;
using Ecm.Mvvm;

namespace Ecm.CaptureAdmin.ViewModel
{
    public class BatchFieldViewModel : BaseDependencyProperty
    {
        private RelayCommand _saveCommand;
        private RelayCommand _cancelCommand;
        private FieldModel _field;
        private readonly Action<FieldModel> _saveAction;
        private readonly BatchTypeViewModel _batchTypeViewModel;

        public BatchFieldViewModel(Action<FieldModel> saveAction, BatchTypeViewModel batchTypeViewModel)
        {
            _saveAction = saveAction;
            _batchTypeViewModel = batchTypeViewModel;
        }

        //Events
        public event CloseDialog CloseDialog;

        //Public properties

        public FieldModel Field
        {
            get { return _field; }
            set
            {
                _field = value;
                OnPropertyChanged("Field");
            }
        }

        public bool IsEditMode { get; set; }

        public ICommand SaveCommand
        {
            get { return _saveCommand ?? (_saveCommand = new RelayCommand(p => Save(), p => CanSave())); }
        }

        public ICommand CancelCommand
        {
            get { return _cancelCommand ?? (_cancelCommand = new RelayCommand(p => Cancel())); }
        }

        //Private methods

        private void Cancel()
        {
            Close();
        }

        private bool CanSave()
        {
            bool isValid = _batchTypeViewModel.EditBatchType != null && Field != null && !string.IsNullOrEmpty(Field.Name) && !Field.HasErrorWithName && !Field.HasErrorWithDefaultValue;

            if (isValid)
            {
                if (Field.Id == Guid.Empty)
                {
                    isValid = !_batchTypeViewModel.EditBatchType.Fields.Any(f => !f.IsSelected && f.Name.Equals(Field.Name, StringComparison.CurrentCultureIgnoreCase));
                }
                else
                {
                    isValid = !_batchTypeViewModel.EditBatchType.Fields.Any(f => f.Name.Equals(Field.Name, StringComparison.CurrentCultureIgnoreCase) && f.Id != Field.Id);
                }
            }

            if (isValid && Field.DataType == FieldDataType.String && Field.MaxLength <= 0)
            {
                isValid = false;
            }

            return isValid;
        }

        private void Save()
        {
            if (_saveAction != null)
            {
                _saveAction(Field);
            }

            Close();
        }

        private void Close()
        {
            if (CloseDialog != null)
            {
                CloseDialog();
            }
        }
    }
}
