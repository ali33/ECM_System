using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Windows.Input;
using Ecm.CaptureModel;
using Ecm.CaptureModel.DataProvider;
using Ecm.Mvvm;

namespace Ecm.CaptureAdmin.ViewModel
{
    public class PermissionViewModel : ComponentViewModel
    {
        private readonly ResourceManager _resource = new ResourceManager("Ecm.CaptureAdmin.Resources", Assembly.GetExecutingAssembly());
        private readonly PermissionProvider _permissionProvider = new PermissionProvider();
        private readonly DocTypeProvider _docTypeProvider = new DocTypeProvider();

        private ObservableCollection<MenuModel> _permissionItems = new ObservableCollection<MenuModel>();
        private ObservableCollection<UserGroupModel> _userGroups = new ObservableCollection<UserGroupModel>();
        private ObservableCollection<BatchTypeModel> _batchTypes = new ObservableCollection<BatchTypeModel>();
        private ObservableCollection<DocumentFieldPermissionViewModel> _documentFieldPermissionViewModels = new ObservableCollection<DocumentFieldPermissionViewModel>();

        private bool _viewByBatchType = true;
        private bool _viewByUserGroup;
        private bool _isPermissionModified;
        private BatchTypePermissionModel _permission;
        private bool _userGroupEnabled = true;
        private bool _batchTypeEnabled = true;

        private RelayCommand _permissionSelectedCommand;
        private RelayCommand _changeMenuDisplayCommand;
        private RelayCommand _savePermissionCommand;
        private RelayCommand _checkCommand;
        private bool _enableEditPermission;

        public PermissionViewModel()
        {
            EnableEditPermission = false;
        }

        public sealed override void Initialize()
        {
            Permission = null;
            EnableEditPermission = false;
            GenerateApplicationPermissionMenu();
        }

        public Action SaveCompleted { get; set; }

        public BatchTypePermissionModel Permission
        {
            get { return _permission; }
            set
            {
                _permission = value;
                EditPanelVisibled = value != null;
                OnPropertyChanged("Permission");
            }
        }

        public ObservableCollection<DocumentFieldPermissionViewModel> DocumentFieldPermissionViewModels
        {
            get { return _documentFieldPermissionViewModels; }
            set
            {
                _documentFieldPermissionViewModels = value;
                OnPropertyChanged("DocumentFieldPermissionViewModels");
            }
        }

        public ObservableCollection<MenuModel> PermissionItems
        {
            get { return _permissionItems; }
            set
            {
                _permissionItems = value;
                OnPropertyChanged("PermissionItems");
            }
        }

        public ObservableCollection<UserGroupModel> UserGroups
        {
            get { return _userGroups; }
            set
            {
                _userGroups = value;
                OnPropertyChanged("UserGroups");
            }
        }

        public ObservableCollection<BatchTypeModel> BatchTypes
        {
            get { return _batchTypes; }
            set
            {
                _batchTypes = value;
                OnPropertyChanged("BatchTypes");
            }
        }

        public bool ViewByBatchType
        {
            get { return _viewByBatchType; }
            set
            {
                _viewByBatchType = value;
                OnPropertyChanged("ViewByBatchType");

                if (value)
                {
                    ViewByUserGroup = false;
                    EnableEditPermission = false;
                }
            }
        }

        public bool ViewByUserGroup
        {
            get { return _viewByUserGroup; }
            set
            {
                _viewByUserGroup = value;
                OnPropertyChanged("ViewByUserGroup");

                if (value)
                {
                    ViewByBatchType = false;
                    EnableEditPermission = false;
                }
            }
        }

        public bool EnableEditPermission
        {
            get { return _enableEditPermission; }
            set
            {
                _enableEditPermission = value;
                OnPropertyChanged("EnableEditPermission");
            }
        }

        public bool BatchTypeEnabled
        {
            get { return _batchTypeEnabled; }
            set
            {
                _batchTypeEnabled = value;
                OnPropertyChanged("BatchTypeEnabled");
            }
        }

        public bool UserGroupEnabled
        {
            get { return _userGroupEnabled; }
            set
            {
                _userGroupEnabled = value;
                OnPropertyChanged("UserGroupEnabled");
            }
        }

        public ICommand PermissionSelectedCommand
        {
            get
            {
                return _permissionSelectedCommand ?? (_permissionSelectedCommand = new RelayCommand(p => SelectPermission(p as MenuModel)));
            }
        }

        public ICommand ChangeMenuDisplayCommand
        {
            get
            {
                return _changeMenuDisplayCommand ?? (_changeMenuDisplayCommand = new RelayCommand(p => ChangeMenuDisplay(), p => CanChangeMenuDisplay()));
            }
        }

        public ICommand SavePermissionCommand
        {
            get
            {
                return _savePermissionCommand ?? (_savePermissionCommand = new RelayCommand(p => Save(), p => CanSave()));
            }
        }

        public ICommand CheckCommand
        {
            get
            {
                return _checkCommand ?? (_checkCommand = new RelayCommand(p => CheckPermission()));
            }
        }
        
        //Private methods
        
        private void CheckPermission()
        {
            _isPermissionModified = true;
        }

        private bool CanSave()
        {
            return _isPermissionModified;
        }

        private void Save()
        {
            IsProcessing = true;

            var worked = new BackgroundWorker();
            worked.DoWork += DoSave;
            worked.RunWorkerCompleted += DoSaveComleted;
            worked.RunWorkerAsync();
        }

        private void DoSave(object sender, DoWorkEventArgs e)
        {
            try
            {

                InternalSave();
            }
            catch (Exception ex)
            {
                e.Result = ex;
            }

        }

        private void DoSaveComleted(object sender, RunWorkerCompletedEventArgs e)
        {
            IsProcessing = false;

            if (e.Result is Exception)
            {
                ProcessHelper.ProcessException(e.Result as Exception);
            }
            else
            {
                _isPermissionModified = false;
            }
        }

        private void InternalSave()
        {
            List<DocTypePermissionModel> docTypePermissions = new List<DocTypePermissionModel>();

            foreach (DocumentFieldPermissionViewModel fieldViewModel in DocumentFieldPermissionViewModels)
            {
                //foreach (FieldPermissionViewModel fieldPermission in fieldViewModel.FieldPermissions)
                //{
                //    DocumentFieldPermissionModel fieldModel = fieldPermission.FieldPermission;
                //    fieldViewModel.DocumentTypePermission.FieldPermissions.Add(fieldModel);
                //}

                docTypePermissions.Add(fieldViewModel.DocumentTypePermission);
            }

            _permissionProvider.SaveBatchTypePermission(Permission, docTypePermissions);

            UserGroupModel userGroup = null;
            BatchTypeModel batchType = null;

            if (SelectedMenu.IsDocType && ViewByUserGroup)
            {
                userGroup = UserGroups.FirstOrDefault(g => g.Id == SelectedMenu.ParentId);
                batchType = BatchTypes.FirstOrDefault(r => r.Id == SelectedMenu.Id);
            }
            else if (ViewByBatchType && SelectedMenu.IsUserGroup)
            {
                batchType = BatchTypes.FirstOrDefault(r => r.Id == SelectedMenu.ParentId);
                userGroup = UserGroups.FirstOrDefault(g => g.Id == SelectedMenu.Id);
            }

            if (userGroup != null && batchType != null && Permission.Id == Guid.Empty)
            {
                Permission = _permissionProvider.GetBatchTypePermission(userGroup.Id, batchType.Id);
            }
        }
        public MenuModel SelectedMenu { get; set; }

        private bool CanChangeMenuDisplay()
        {
            return PermissionItems != null && PermissionItems.Count > 0;
        }

        private void ChangeMenuDisplay()
        {
            ConfirmBeforeSwitchView();
            GenerateApplicationPermissionMenu();
        }

        private void GenerateApplicationPermissionMenu()
        {
            var batchTypeByUserGroup = new Dictionary<UserGroupModel, ObservableCollection<BatchTypeModel>>();
            var userGroupsByBatchType = new Dictionary<BatchTypeModel, ObservableCollection<UserGroupModel>>();

            if (PermissionItems == null)
            {
                PermissionItems = new ObservableCollection<MenuModel>();
            }
            else
            {
                PermissionItems.Clear();
            }

            try
            {
                _batchTypes = _permissionProvider.GetBatchTypes();
                _userGroups = _permissionProvider.GetUserGroups();

                UserGroupEnabled = _userGroups.Count > 0;
                BatchTypeEnabled = _batchTypes.Count > 0;

                // Get the related repositories by each user group
                foreach (UserGroupModel userGroup in _userGroups)
                {
                    if (_batchTypes.Count > 0)
                    {
                        batchTypeByUserGroup.Add(userGroup, _batchTypes);
                    }
                }

                if (ViewByBatchType)
                {
                    // Determine the related user groups by each repository
                    foreach (BatchTypeModel batchType in _batchTypes)
                    {
                        var userGroups = new ObservableCollection<UserGroupModel>();

                        foreach (UserGroupModel userGroup in _userGroups)
                        {
                            if (batchTypeByUserGroup.Keys.Contains(userGroup))
                            {
                                if (batchTypeByUserGroup[userGroup].Any(p => p.Id == batchType.Id && p.Name == batchType.Name))
                                {
                                    userGroups.Add(userGroup);
                                }
                            }
                        }

                        if (userGroups.Count > 0)
                        {
                            userGroupsByBatchType.Add(batchType, userGroups);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ProcessHelper.ProcessException(ex);
            }

            if (ViewByUserGroup)
            {
                foreach (UserGroupModel userGroup in _userGroups)
                {
                    var node = new MenuModel { Id = userGroup.Id, MenuText = userGroup.Name, IsUserGroup = true };

                    PermissionItems.Add(node);

                    // Only add the related repositories to user group to display
                    if (batchTypeByUserGroup.Keys.Contains(userGroup))
                    {
                        ObservableCollection<BatchTypeModel> docType = batchTypeByUserGroup[userGroup];

                        node.MenuItems = new ObservableCollection<MenuModel>(
                            (from p in docType
                             select new MenuModel {Id =  p.Id, ParentId = node.Id, MenuText = p.Name, IsDocType = true}).ToList());
                    }
                }
            }
            else
            {
                foreach (BatchTypeModel batchType in _batchTypes)
                {
                    var node = new MenuModel { Id = batchType.Id, MenuText = batchType.Name, IsDocType = true };

                    PermissionItems.Add(node);

                    if (userGroupsByBatchType.Keys.Contains(batchType))
                    {
                        ObservableCollection<UserGroupModel> userGroups = userGroupsByBatchType[batchType];

                        node.MenuItems = new ObservableCollection<MenuModel>(
                            (from p in userGroups
                             select new MenuModel
                                        {
                                            Id = p.Id,
                                            ParentId = node.Id,
                                            MenuText = p.Name,
                                            IsUserGroup = true
                                        }).ToList());
                    }
                }
            }
        }

        private void SelectPermission(MenuModel selectedMenu)
        {
            if (selectedMenu != null)
            {
                UserGroupModel userGroup = null;
                BatchTypeModel batchType = null;

                SelectedMenu = selectedMenu;
                ConfirmBeforeSwitchView();

                if (SelectedMenu.IsDocType && ViewByUserGroup)
                {
                    userGroup = UserGroups.FirstOrDefault(g => g.Id == SelectedMenu.ParentId);
                    batchType = BatchTypes.FirstOrDefault(r => r.Id == SelectedMenu.Id);
                }
                else if (ViewByBatchType && SelectedMenu.IsUserGroup)
                {
                    batchType = BatchTypes.FirstOrDefault(r => r.Id == SelectedMenu.ParentId);
                    userGroup = UserGroups.FirstOrDefault(g => g.Id == SelectedMenu.Id);
                }

                EnableEditPermission = false;

                if (userGroup != null && batchType != null)
                {
                    Permission = _permissionProvider.GetBatchTypePermission(userGroup.Id, batchType.Id);
                    
                    if (Permission == null)
                    {
                        Permission = new BatchTypePermissionModel { BatchTypeId = batchType.Id, UserGroupId = userGroup.Id };
                    }

                    DocumentFieldPermissionViewModels = new ObservableCollection<DocumentFieldPermissionViewModel>();

                    List<DocTypeModel> docTypes = new List<DocTypeModel>();
                    docTypes = _docTypeProvider.GetDocTypes(batchType.Id).ToList();

                    foreach (DocTypeModel documentType in docTypes)
                    {
                        DocTypePermissionModel docTypePermission = _permissionProvider.GetDocTypePermission(userGroup.Id, documentType.Id);

                        DocumentFieldPermissionViewModel documentFieldPermissionViewModel = new DocumentFieldPermissionViewModel
                        {
                            DocumentType = documentType,
                            DocumentTypePermission = docTypePermission == null ?
                                                new DocTypePermissionModel { DocTypeId = documentType.Id, UserGroupId = userGroup.Id } :
                                                docTypePermission
                        };

                        //List<DocumentFieldPermissionModel> fieldPermissionModels = _permissionProvider.GetFieldPermission(userGroup.Id, documentType.Id).ToList();

                        //foreach (FieldModel fieldModel in documentType.Fields.Where(p => !p.IsSystemField))
                        //{
                        //    FieldPermissionViewModel fieldViewModel = new FieldPermissionViewModel
                        //    {
                        //        FieldPermission = (fieldPermissionModels == null || fieldPermissionModels.Count == 0) ?
                        //                        new DocumentFieldPermissionModel
                        //                        {
                        //                            Field = fieldModel,
                        //                            FieldId = fieldModel.Id,
                        //                            DocTypeId = documentType.Id,
                        //                            UserGroupId = userGroup.Id
                        //                        } :
                        //                        fieldPermissionModels.FirstOrDefault(p => p.FieldId == fieldModel.Id) == null ? new DocumentFieldPermissionModel() : fieldPermissionModels.FirstOrDefault(p => p.FieldId == fieldModel.Id)
                        //    };
                            
                        //    fieldViewModel.AllPermission = fieldViewModel.FieldPermission.CanRead && fieldViewModel.FieldPermission.CanWrite && fieldViewModel.FieldPermission.Hidden;
                        //    fieldViewModel.FieldName = fieldModel.Name;
                        //    fieldViewModel.PropertyChanged += new PropertyChangedEventHandler(fieldViewModel_PropertyChanged);
                        //    documentFieldPermissionViewModel.FieldPermissions.Add(fieldViewModel);
                        //}
                        documentFieldPermissionViewModel.PropertyChanged += new PropertyChangedEventHandler(documentFieldPermissionViewModel_PropertyChanged);
                        DocumentFieldPermissionViewModels.Add(documentFieldPermissionViewModel);
                    }

                    EnableEditPermission = true;
                }
            }
        }

        void documentFieldPermissionViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "PermissionChanged")
            {
                _isPermissionModified = true;
            }
        }

        void fieldViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "AllPermission")
            {
                _isPermissionModified = true;
            }
        }

        private void ConfirmBeforeSwitchView()
        {
            if (_isPermissionModified)
            {
                if (DialogService.ShowTwoStateConfirmDialog(string.Format(_resource.GetString("uiSaveConfirmation"), _resource.GetString("Permission"))) != DialogServiceResult.No)
                {
                    try
                    {
                        InternalSave();
                    }
                    catch (Exception ex)
                    {
                        ProcessHelper.ProcessException(ex);
                    }
                }
            }
            _isPermissionModified = false;
        }
    }
}
