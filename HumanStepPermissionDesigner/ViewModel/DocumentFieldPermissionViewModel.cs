using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ecm.Mvvm;
using System.Collections.ObjectModel;
using Ecm.CaptureDomain;

namespace Ecm.Workflow.Activities.HumanStepPermissionDesigner.ViewModel
{
    public class DocumentFieldPermissionViewModel : ComponentViewModel
    {
        private ObservableCollection<FieldPermissionViewModel> _fieldViewModel = new ObservableCollection<FieldPermissionViewModel>();
        private DocumentType _documentType;
        private DocumentPermission _permission;
        private bool _isDataModified;

        public DocumentFieldPermissionViewModel(DocumentType docType, DocumentPermission permission)
        {
            _documentType = docType;
            _permission = permission;

            LoadFieldPermission();
        }

        public DocumentType DocumentType
        {
            get { return _documentType; }
            set
            {
                _documentType = value;
                OnPropertyChanged("DocumentType");
            }
        }

        public bool IsDataModified
        {
            get { return _isDataModified; }
            set
            {
                _isDataModified = value;
                OnPropertyChanged("IsDataModified");
            }
        }

        public bool CanSeeRestrictedField
        {
            get { return _permission.CanSeeRestrictedField; }
            set
            {
                _permission.CanSeeRestrictedField = value;
                OnPropertyChanged("CanSeeRestrictedField");
            }
        }

        public ObservableCollection<FieldPermissionViewModel> FieldPermissions
        {
            get { return _fieldViewModel; }
            set
            {
                _fieldViewModel = value;
                OnPropertyChanged("FieldPermissions");
            }
        }

        private void LoadFieldPermission()
        {
            if (DocumentType == null)
            {
                return;
            }

            if (_permission != null && _permission.FieldPermissions != null && _permission.FieldPermissions.Count > 0)
            {
                if (DocumentType.Fields != null)
                {
                    foreach (var field in DocumentType.Fields.Where(h => !h.IsSystemField))
                    {
                        var fieldPermission = _permission.FieldPermissions.FirstOrDefault(p => p.FieldId == field.Id);

                        if (fieldPermission == null)
                        {
                            fieldPermission = new DocumentFieldPermission()
                            {
                                DocTypeId = DocumentType.Id,
                                FieldId = field.Id,
                                UserGroupId = _permission.UserGroupId
                            };

                            FieldPermissionViewModel viewModel = new FieldPermissionViewModel(fieldPermission);
                            viewModel.FieldName = field.Name;
                            viewModel.FieldId = field.Id;
                            viewModel.PropertyChanged += viewModel_PropertyChanged;
                            _permission.FieldPermissions.Add(fieldPermission);
                            FieldPermissions.Add(viewModel);
                        }
                        else
                        {
                            FieldPermissionViewModel viewModel = new FieldPermissionViewModel(fieldPermission);
                            viewModel.FieldName = field != null ? field.Name : string.Empty;
                            viewModel.FieldId = field.Id;
                            viewModel.DocTypeId = DocumentType.Id;
                            viewModel.UserGroupId = _permission.UserGroupId;
                            viewModel.PropertyChanged += viewModel_PropertyChanged;
                            FieldPermissions.Add(viewModel);
                        }
                    }
                }

                //foreach (DocumentFieldPermission fieldPermission in _permission.FieldPermissions)
                //{
                //    var field = DocumentType.Fields.FirstOrDefault(p => p.Id == fieldPermission.FieldId);
                //    if (field == null)
                //    {
                //        continue;
                //    }

                //    FieldPermissionViewModel viewModel = new FieldPermissionViewModel(fieldPermission);
                //    viewModel.FieldName = field != null ? field.Name : string.Empty;
                //    viewModel.FieldId = field.Id;
                //    viewModel.DocTypeId = DocumentType.Id;
                //    viewModel.UserGroupId = _permission.UserGroupId;
                //    viewModel.PropertyChanged += viewModel_PropertyChanged;
                //    FieldPermissions.Add(viewModel);
                //}
            }
            else
            {
                foreach (DocumentFieldMetaData field in DocumentType.Fields.Where(p => !p.IsSystemField))
                {
                    DocumentFieldPermission fieldPermission = new DocumentFieldPermission()
                    {
                        DocTypeId = DocumentType.Id,
                        FieldId = field.Id,
                        UserGroupId = _permission.UserGroupId
                    };

                    FieldPermissionViewModel viewModel = new FieldPermissionViewModel(fieldPermission);
                    viewModel.FieldName = field.Name;
                    viewModel.FieldId = field.Id;
                    viewModel.PropertyChanged += viewModel_PropertyChanged;
                    _permission.FieldPermissions.Add(fieldPermission);
                    FieldPermissions.Add(viewModel);
                }
            }
        }

        void viewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            IsDataModified = true;
        }
    }
}
