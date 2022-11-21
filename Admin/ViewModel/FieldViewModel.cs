using System;
using System.Drawing;
using Ecm.Mvvm;
using Ecm.Model;
using System.Windows.Input;
using Ecm.Admin.View;
using System.Collections.ObjectModel;
using Ecm.Domain;
using System.Linq;
using System.Resources;
using System.Reflection;
using System.Collections.Generic;

namespace Ecm.Admin.ViewModel
{
    public class FieldViewModel : BaseDependencyProperty
    {
        private RelayCommand _saveFieldCommand;
        private RelayCommand _cancelFieldCommand;
        private RelayCommand _configPicklistCommand;
        private RelayCommand _configRowCommand;
        
        private FieldMetaDataModel _field;
        private DialogBaseView _dialog;
        //private List<TableColumnModel> _tableColumns;

        public FieldViewModel(Action<FieldMetaDataModel> field, DocumentTypeViewModel documentTypeViewModel)
        {
            DataAction = field;
            DocumentTypeViewModel = documentTypeViewModel;
            //_tableColumns = new List<TableColumnModel>();
        }

        //Events
        public event CloseDialog CloseDialog;

        private Action<FieldMetaDataModel> DataAction { get; set; }

        //Public properties
        public DocumentTypeViewModel DocumentTypeViewModel { get; set; }

        public bool HasParent
        {
            get;
            set;
        }

        public FieldMetaDataModel Field
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

        public ICommand ConfigRowCommand
        {
            get
            {
                if (_configRowCommand == null)
                {
                    _configRowCommand = new RelayCommand(p => ConfigRow());
                }

                return _configRowCommand;
            }
        }

        //Private methods

        private bool CanEditRow()
        {
            return Field.Children.ToList() != null && Field.Children.ToList().Count > 0;
        }

        private void ConfigRow()
        {
            TableConfigurationViewModel tableViewModel = new TableConfigurationViewModel(Field);
            TableConfigurationView tableView = new TableConfigurationView(tableViewModel);
            //_tableColumns = Field.Children.ToList();
            tableViewModel.Load(Field.Children.ToList());
            tableViewModel.CloseDialog = CloseTableViewDialog;

            _dialog = new DialogBaseView(tableView);
            _dialog.Size = new Size(700, 500);
            _dialog.Text = new ResourceManager("Ecm.Admin.TableConfigurationView", Assembly.GetExecutingAssembly()).GetString("dgConfigDialogTitle");

            _dialog.ShowDialog();
        }

        private void CloseTableViewDialog(bool isSave, TableConfigurationViewModel tableViewModel)
        {
            if (_dialog != null)
            {
                if (isSave)
                {
                    Field.Children = new ObservableCollection<TableColumnModel>(tableViewModel.GetTableColumns());
                    Field.DeletedChildrenIds = tableViewModel.GetRemoveColumns();
                }

                _dialog.Close();
            }
        }

        private void ConfigPicklist()
        {
            PicklistViewModel viewModel = new PicklistViewModel(ConfigurePicklistCompleted) { IsEditMode = Field.Picklists != null };

            if (Field.Picklists == null)
            {
                Field.Picklists = new ObservableCollection<PicklistModel>();
            }

            viewModel.Picklists = Field.Picklists.ToList();
            PicklistView view = new PicklistView(viewModel);

            _dialog = new DialogBaseView(view)
                          {
                              Size = new Size(500, 300),
                              Text = new ResourceManager("Ecm.Admin.PicklistView", Assembly.GetExecutingAssembly()).GetString("dgDialogTitle")
                          };
            _dialog.ShowDialog();
        }

        private void Cancel()
        {
            if (CloseDialog != null)
            {
                CloseDialog();
            }
        }

        private bool CanSave()
        {
            bool isValid = Field != null && !string.IsNullOrEmpty(Field.Name) && !string.IsNullOrWhiteSpace(Field.Name) && !Field.HasErrorWithName && !Field.HasErrorWithDefaultValue;

            if (isValid)
            {
                if (Field.Id == Guid.Empty)
                {
                    isValid = !DocumentTypeViewModel.Fields.Any(f => !f.IsSelected && f.Name.Equals(Field.Name, StringComparison.CurrentCultureIgnoreCase));
                }
                else
                {
                    isValid = !DocumentTypeViewModel.Fields.Any(f => f.Name.Equals(Field.Name, StringComparison.CurrentCultureIgnoreCase) && f.Id != Field.Id);
                }
            }

            if (isValid && Field.DataType == FieldDataType.String && Field.MaxLength <= 0)
            {
                isValid = false;
            }
            else if (isValid && Field.DataType == FieldDataType.Picklist && (Field.Picklists == null || Field.Picklists.Count <= 0))
            {
                isValid = false;
            }

            return isValid;
        }

        private void SaveField()
        {
            //Field.Children = new ObservableCollection<TableColumnModel>();
           // _tableColumns.ForEach(Field.Children.Add);
            DataAction(Field);
        }

        private void ConfigurePicklistCompleted(PicklistViewModel viewModel)
        {
            if (viewModel != null)
            {
                if (viewModel.IsEditMode)
                {
                    Field.Picklists = new ObservableCollection<PicklistModel>(viewModel.Picklists);
                }
                else
                {
                    foreach (var picklist in viewModel.Picklists)
                    {
                        Field.Picklists.Add(picklist);
                    }
                }
            }

            if (_dialog != null)
            {
                _dialog.Close();
            }
        }
    }
}
