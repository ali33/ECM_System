using System;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Windows.Input;
using Ecm.CaptureAdmin.View;
using Ecm.CaptureDomain;
using Ecm.CaptureModel;
using Ecm.Mvvm;
using System.Collections.Generic;

namespace Ecm.CaptureAdmin.ViewModel
{
    public class DocFieldViewModel : BaseDependencyProperty
    {
        private RelayCommand _saveFieldCommand;
        private RelayCommand _cancelFieldCommand;
        private RelayCommand _configPicklistCommand;
        private RelayCommand _configColumnCommand;
        private RelayCommand _showValidationPatternCommand;
        private RelayCommand _showValidationScriptCommand;

        private FieldModel _field;
        private DialogBaseView _dialog;
        private readonly Action<FieldModel> _saveAction;
        private readonly DocTypeViewModel _docTypeViewModel;
        private bool _isRegularExpressionPopupOpen;
        private bool _isValidationPopupOpen;
        private List<TableColumnModel> _tableColumns;

        public DocFieldViewModel(Action<FieldModel> saveAction, DocTypeViewModel docTypeViewModel)
        {
            _saveAction = saveAction;
            _docTypeViewModel = docTypeViewModel;
            _tableColumns = new List<TableColumnModel>();
        }

        //Events
        public event CloseDialog CloseDialog;

        //Public properties
        public bool HasParent
        {
            get;
            set;
        }

        public bool CanNotEdit { get; set; }

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

        public ICommand SaveFieldCommand
        {
            get { return _saveFieldCommand ?? (_saveFieldCommand = new RelayCommand(p => SaveField(), p => CanSave())); }
        }

        public ICommand CancelSaveFieldCommand
        {
            get { return _cancelFieldCommand ?? (_cancelFieldCommand = new RelayCommand(p => Cancel())); }
        }

        public ICommand ConfigPicklistCommand
        {
            get
            {
                if (_configPicklistCommand == null)
                {
                    _configPicklistCommand = new RelayCommand(p => ConfigPicklist());
                }
                return _configPicklistCommand;
            }
        }

        public ICommand ConfigColumnCommand
        {
            get
            {
                if (_configColumnCommand == null)
                {
                    _configColumnCommand = new RelayCommand(p => ConfigColumn());
                }
                return _configColumnCommand;
            }
        }

        public ICommand ShowValidationPatternCommand
        {
            get
            {
                if (_showValidationPatternCommand == null)
                {
                    _showValidationPatternCommand = new RelayCommand(p => ShowValidationPatternPopup());
                }
                return _showValidationPatternCommand;
            }

        }

        public ICommand ShowValidationScriptCommand
        {
            get
            {
                if (_showValidationScriptCommand == null)
                {
                    _showValidationScriptCommand = new RelayCommand(p => ShowValidationScriptPopup());
                }
                return _showValidationScriptCommand;
            }
        }

        public bool IsValidationPopupOpen
        {
            get { return _isValidationPopupOpen; }
            set
            {
                _isValidationPopupOpen = value;
                OnPropertyChanged("IsValidationPopupOpen");
            }
        }

        public bool IsRegularExpressionPopupOpen
        {
            get { return _isRegularExpressionPopupOpen; }
            set
            {
                _isRegularExpressionPopupOpen = value;
                OnPropertyChanged("IsRegularExpressionPopupOpen");
            }
        }
        //Private methods

        private void ConfigPicklist()
        {
            PicklistViewModel viewModel = new PicklistViewModel(ConfigurePicklistCompleted) { IsEditMode = Field.Picklists != null };

            if (Field.Picklists == null)
            {
                Field.Picklists = new ObservableCollection<PicklistModel>();
            }

            viewModel.Picklists = Field.Picklists.Select(h => (PicklistModel)h.Clone()).ToList();
            PicklistView view = new PicklistView(viewModel);

            _dialog = new DialogBaseView(view)
                          {
                              Size = new Size(500, 300),
                              Text = new ResourceManager("Ecm.CaptureAdmin.PicklistView", Assembly.GetExecutingAssembly()).GetString("dgDialogTitle")
                          };
            _dialog.ShowDialog();
        }


        private void ConfigColumn()
        {
            TableConfigurationViewModel tableViewModel = new TableConfigurationViewModel(Field);
            TableConfigurationView tableView = new TableConfigurationView(tableViewModel);
            _tableColumns = Field.Children.ToList();
            tableViewModel.Load(_tableColumns);
            tableViewModel.CloseDialog = CloseTableViewDialog;

            _dialog = new DialogBaseView(tableView);
            _dialog.Size = new Size(700, 500);
            _dialog.Text = new ResourceManager("Ecm.CaptureAdmin.TableConfigurationView", Assembly.GetExecutingAssembly()).GetString("dgConfigDialogTitle");

            _dialog.ShowDialog();

            _tableColumns = tableViewModel.GetTableColumns();
            Field.DeletedChildrenIds = tableViewModel.GetRemoveColumns();
        }

        private void CloseTableViewDialog()
        {
            if (_dialog != null)
            {
                _dialog.Close();
            }
        }

        private void Cancel()
        {
            Close();
        }

        private bool CanSave()
        {
            bool isValid = _docTypeViewModel.DocType != null && Field != null && !string.IsNullOrEmpty(Field.Name) && !Field.HasErrorWithName && !Field.HasErrorWithDefaultValue;

            if (isValid)
            {
                if (Field.Id == Guid.Empty)
                {
                    isValid = !_docTypeViewModel.DocType.Fields.Any(f => !f.IsSelected && f.Name.Equals(Field.Name, StringComparison.CurrentCultureIgnoreCase));
                }
                else
                {
                    isValid = !_docTypeViewModel.DocType.Fields.Any(f => f.Name.Equals(Field.Name, StringComparison.CurrentCultureIgnoreCase) && f.Id != Field.Id);
                }
            }

            if (isValid && Field.DataType == FieldDataType.String && Field.MaxLength <= 0)
            {
                return false;
            }

            if (CanNotEdit)
            {
                return false;
            }

            if (Field.DataType == FieldDataType.Picklist)
            {
                if (Field.Picklists == null || Field.Picklists.Count == 0)
                {
                    return false;
                }
            }

            if (Field.DataType == FieldDataType.Table)
            {
                if (Field.Children == null || Field.Children.Count == 0)
                {
                    return false;
                }
            }

            return isValid;
        }

        private void SaveField()
        {
            if (_saveAction != null)
            {
                Field.Children = new ObservableCollection<TableColumnModel>();
                _tableColumns.ForEach(Field.Children.Add);
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

        private void ConfigurePicklistCompleted(PicklistViewModel viewModel)
        {
            if (viewModel != null)
            {
                //if (viewModel.IsEditMode)
                //{
                //    Field.Picklists = new ObservableCollection<PicklistModel>(viewModel.Picklists);
                //}
                //else
                //{
                //    foreach (var picklist in viewModel.Picklists)
                //    {
                //        Field.Picklists.Add(picklist);
                //    }
                //}

                Field.Picklists = new ObservableCollection<PicklistModel>(viewModel.Picklists);
            }

            if (_dialog != null)
            {
                _dialog.Close();
            }
        }


        private void ShowValidationPatternPopup()
        {
            if (IsRegularExpressionPopupOpen)
            {
                IsRegularExpressionPopupOpen = false;
            }
            else
            {
                IsRegularExpressionPopupOpen = true;
            }
        }

        private void ShowValidationScriptPopup()
        {
            if (IsValidationPopupOpen)
            {
                IsValidationPopupOpen = false;
            }
            else
            {
                IsValidationPopupOpen = true;
            }
        }

    }
}
