using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Windows.Input;
using Ecm.CaptureAdmin.View;
using Ecm.CaptureDomain;
using Ecm.CaptureModel;
using Ecm.Mvvm;
using System.ComponentModel;

namespace Ecm.CaptureAdmin.ViewModel
{
    public class DocTypeViewModel : BaseDependencyProperty
    {
        public DocTypeViewModel(Action<DocTypeModel> saveAction, BatchTypeViewModel batchTypeViewModel)
        {
            _saveAction = saveAction;
            _batchTypeViewModel = batchTypeViewModel;

            _errorMessage.Add("uiInvalidIntegerValue", _resouceManager.GetString("uiInvalidIntegerValue"));
            _errorMessage.Add("uiInvalidDecimalValue", _resouceManager.GetString("uiInvalidDecimalValue"));
            _errorMessage.Add("uiFieldNameExisted", _resouceManager.GetString("uiFieldNameExisted"));
        }

        public event CloseDialog CloseDialog;

        public DocFieldViewModel DocFieldViewModel { get; set; }

        public DocTypeModel DocType
        {
            get { return _docType; }
            set
            {
                _docType = value;
                OnPropertyChanged("DocType");
                value.PropertyChanged += value_PropertyChanged;
                if (_docType != null)
                {
                    DocFields.Clear();

                    foreach (var field in _docType.Fields.Where(p => p.ParentFieldId == null))
                    {
                        DocFields.Add(field);
                    }
                }
            }
        }

        void value_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            //IsEditMode = true;
            _isDataModified = true;
        }

        public string IconFilePath
        {
            get { return _iconFilePath; }
            set
            {
                _iconFilePath = value;
                OnPropertyChanged("IconFilePath");
            }
        }

        public ObservableCollection<FieldModel> DocFields
        {
            get { return _docFields; }
            set
            {
                _docFields = value;
                OnPropertyChanged("DocFields");
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

        public ICommand BrowseCommand
        {
            get { return _browseCommand ?? (_browseCommand = new RelayCommand(p => Browse())); }
        }

        public ICommand AddFieldCommand
        {
            get { return _addFieldCommand ?? (_addFieldCommand = new RelayCommand(p => AddField(), p => CanAddField())); }
        }

        public ICommand EditFieldCommand
        {
            get { return _editFieldCommand ?? (_editFieldCommand = new RelayCommand(p => EditField(), p => CanEditField())); }
        }

        public ICommand ViewFieldCommand
        {
            get { return _viewFieldCommand ?? (_viewFieldCommand = new RelayCommand(p => EditField())); }
        }

        public ICommand DeleteFieldCommand
        {
            get { return _deleteFieldCommand ?? (_deleteFieldCommand = new RelayCommand(p => DeleteField(), p => CanEditField())); }
        }

        #region Private methods

        private void Browse()
        {
            string filePath = DialogService.ShowFileBrowseDialog("Image files (*.png, *.jpg)|*.png;*.jpg");
            if (!string.IsNullOrEmpty(filePath))
            {
                IconFilePath = filePath;
                string extension = Path.GetExtension(filePath) + string.Empty;
                var allowedExtensions = new[] { ".png", ".jpg" };
                if (allowedExtensions.Contains(extension.ToLower()))
                {
                    DocType.Icon = Utility.UtilsStream.ReadAllBytes(new FileStream(filePath, FileMode.Open, FileAccess.Read));
                }
                else
                {
                    DialogService.ShowErrorDialog("Only image is supported. (PNG | JPG)");
                }
            }
        }

        private void Cancel()
        {
            Close();
        }

        private bool CanSave()
        {
            bool isValid = _batchTypeViewModel.EditBatchType != null && DocType != null && !string.IsNullOrEmpty(DocType.Name) && !DocType.HasError && DocType.Fields != null && DocType.Fields.Count > 0;

            if (isValid)
            {
                if (DocType.Id == Guid.Empty)
                {
                    isValid = !_batchTypeViewModel.EditBatchType.DocTypes.Any(f => !f.IsSelected && f.Name.Equals(DocType.Name, StringComparison.CurrentCultureIgnoreCase));
                }
                else
                {
                    isValid = !_batchTypeViewModel.EditBatchType.DocTypes.Any(f => f.Name.Equals(DocType.Name, StringComparison.CurrentCultureIgnoreCase) && f.Id != DocType.Id);
                }
            }

            return isValid && _isDataModified;
        }

        private void Save()
        {
            if (_saveAction != null)
            {
                _saveAction(DocType);
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

        private bool CanAddField()
        {
            return DocType != null && !string.IsNullOrWhiteSpace(DocType.Name) && !DocType.HaveDoc;
        }

        private void AddField()
        {
            DocFieldViewModel = new DocFieldViewModel(OnSaveFieldComplete, this)
            {
                IsEditMode = false,
                Field = new FieldModel
                {
                    DocTypeId = DocType.Id,
                    Picklists = new ObservableCollection<PicklistModel>(),
                    ErrorMessages = _errorMessage,
                    ErrorChecked = CheckAddDocField
                }
            };

            LoadDocFieldView(DocFieldViewModel, false);
        }

        private bool CanEditField()
        {
            return CanAddField() && DocType.Fields != null && DocType.Fields.Any(f => f.IsSelected);
        }

        private void EditField()
        {
            FieldModel selectField = DocType.Fields.FirstOrDefault(f => f.IsSelected);
            if (selectField != null)
            {
                DocFieldViewModel = new DocFieldViewModel(OnSaveFieldComplete, this)
                {
                    IsEditMode = true,
                    //Field = new FieldModel
                    //{
                    //    DataType = selectField.DataType,
                    //    DisplayOrder = selectField.DisplayOrder,
                    //    Id = selectField.Id,
                    //    DocTypeId = selectField.DocTypeId,
                    //    IsSelected = selectField.IsSelected,
                    //    IsSystemField = selectField.IsSystemField,
                    //    IsRequired = selectField.IsRequired,
                    //    IsRestricted = selectField.IsRestricted,
                    //    Name = selectField.Name,
                    //    MaxLength = selectField.MaxLength,
                    //    DefaultValue = selectField.DefaultValue,
                    //    UseCurrentDate = selectField.UseCurrentDate,
                    //    ErrorMessages = _errorMessage,
                    //    ErrorChecked = CheckEditDocField,
                    //    ValidationPattern = selectField.ValidationPattern,
                    //    ValidationScript = selectField.ValidationScript,
                    //    Picklists = selectField.Picklists,
                    //    Children = selectField.Children
                    //}
                    Field = (FieldModel)selectField.Clone()
                };

                //foreach (var selected in selectField.Children)
                //{
                //    DocFieldViewModel.Field.Children.Add(selected);
                //}

                LoadDocFieldView(DocFieldViewModel, true);
            }
        }

        private void DeleteField()
        {
            FieldModel deleteField = DocType.Fields.FirstOrDefault(f => f.IsSelected);

            if (deleteField != null)
            {
                DocType.Fields.Remove(deleteField);

                if (DocType.Id != Guid.Empty && deleteField.Id != Guid.Empty)
                {
                    DocType.DeletedFields.Add(deleteField.Id);
                }

                _isDataModified = true;
            }
        }

        private void OnSaveFieldComplete(FieldModel field)
        {
            if (field.DataType != FieldDataType.Date)
            {
                field.UseCurrentDate = false;
            }

            field.Name = string.Format("{0}", field.Name).Trim();

            //ObservableCollection<FieldModel> docFields = DocType.Fields;
            if (field.Id == Guid.Empty)
            {
                if (DocFieldViewModel.IsEditMode)
                {
                    for (int i = 0; i < DocType.Fields.Count; i++)
                    {
                        if (DocType.Fields[i].WorkingId == field.WorkingId)
                        {
                            DocType.Fields[i] = field;
                            break;
                        }
                    }
                }
                else
                {
                    DocType.Fields.Add(field);
                }


              
                //if (DocFieldViewModel.IsEditMode)
                //{
                //    //FieldModel deleteField = DocFields.FirstOrDefault(f => f.IsSelected);
                //    FieldModel deleteField = DocType.Fields.FirstOrDefault(f => f.IsSelected);
                //    int index = -1;
                //    if (deleteField != null)
                //    {
                //        index = DocFields.IndexOf(deleteField);
                //        DocType.Fields.Remove(deleteField);
                //        //DocFields.Remove(deleteField);
                //    }

                //    if (index == -1)
                //    {
                //        DocType.Fields.Add(field);
                //    }
                //    else
                //    {
                //        DocType.Fields.Insert(index, field);
                //    }
                //}
                //else
                //{
                //    DocType.Fields.Add(field);
                //}
            }
            else
            {
                //FieldModel editField = DocType.Fields.FirstOrDefault(f => f.Id == field.Id);

                for (int i = 0; i < DocType.Fields.Count; i++)
                {
                    if (DocType.Fields[i].Id == field.Id)
                    {
                        DocType.Fields[i] = field;
                        break;
                    }
                }

                //if (editField != null)
                //{
                //    editField.Children = field.Children;
                //    editField.DataType = field.DataType;
                //    editField.MaxLength = field.MaxLength;
                //    editField.DefaultValue = field.DefaultValue;
                //    editField.DeletedMaps = field.DeletedMaps;
                //    editField.DisplayOrder = field.DisplayOrder;
                //    editField.DocTypeId = field.DocTypeId;
                //    editField.Id = field.Id;
                //    editField.IsLookup = field.IsLookup;
                //    editField.IsRequired = field.IsRequired;
                //    editField.IsRestricted = field.IsRestricted;
                //    editField.IsSelected = field.IsSelected;
                //    editField.IsSystemField = field.IsSystemField;
                //    editField.LookupInfo = field.LookupInfo;
                //    editField.Maps = field.Maps;
                //    editField.Name = field.Name = string.Format("{0}", field.Name).Trim();
                //    editField.Picklists = field.Picklists;
                //    editField.UseCurrentDate = field.UseCurrentDate;
                //    editField.DeletedChildrenIds = field.DeletedChildrenIds;

                //    if (field.Children != null)
                //    {
                //        editField.Children.Clear();

                //        foreach (var editChildren in field.Children)
                //        {
                //            editField.Children.Add(editChildren);
                //        }
                //    }
                //}
            }

            _isDataModified = true;
            //DocFields = docFields;
        }

        private void CheckEditDocField(FieldModel field)
        {
            var fieldName = string.Format("{0}", field.Name).Trim();
            field.HasErrorWithName =
                string.IsNullOrEmpty(fieldName)
                || DocType.Fields.Any(f => !f.IsSelected
                                            && f.Name.Equals(fieldName, StringComparison.CurrentCultureIgnoreCase));
        }

        private void CheckAddDocField(FieldModel field)
        {
            var fieldName = string.Format("{0}", field.Name).Trim();
            field.HasErrorWithName =
                string.IsNullOrEmpty(fieldName)
                || DocType.Fields.Any(f => f.Name.Equals(fieldName, StringComparison.CurrentCultureIgnoreCase) && f.WorkingId != field.WorkingId);
        }

        private void LoadDocFieldView(DocFieldViewModel viewModel, bool isEdit)
        {
            ResourceManager localResource = new ResourceManager("Ecm.CaptureAdmin.CreateDocFieldView", Assembly.GetExecutingAssembly());
            string editTitle = localResource.GetString("dgEditDialogTitle.Text");
            string addTitle = localResource.GetString("dgAddDialogTitle.Text");
            viewModel.CloseDialog += ViewModelCloseDialog;
            viewModel.CanNotEdit = DocType.HaveDoc;

            var view = new CreateDocFieldView(viewModel);
            _dialog = new DialogBaseView(view) { Size = new Size(600, 550), Text = isEdit ? editTitle : addTitle };
            _dialog.ShowDialog();
        }

        private void ViewModelCloseDialog()
        {
            if (_dialog != null)
            {
                _dialog.Close();
            }
        }

        #endregion

        #region Private members

        private RelayCommand _browseCommand;
        private RelayCommand _saveCommand;
        private RelayCommand _cancelCommand;
        private RelayCommand _addFieldCommand;
        private RelayCommand _editFieldCommand;
        private RelayCommand _viewFieldCommand;
        private RelayCommand _deleteFieldCommand;
        private DocTypeModel _docType;
        private string _iconFilePath;
        private bool _isDataModified;
        private DialogBaseView _dialog;
        private ObservableCollection<FieldModel> _docFields = new ObservableCollection<FieldModel>();
        private readonly BatchTypeViewModel _batchTypeViewModel;
        private readonly Action<DocTypeModel> _saveAction;
        private readonly Dictionary<string, string> _errorMessage = new Dictionary<string, string>();
        private readonly ResourceManager _resouceManager = new ResourceManager("Ecm.CaptureAdmin.Resources", Assembly.GetExecutingAssembly());

        #endregion
    }
}
