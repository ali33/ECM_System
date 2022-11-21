using System;
using System.Collections.Generic;
using System.Linq;
using Ecm.Mvvm;
using Ecm.Model;
using System.Collections.ObjectModel;
using Ecm.Model.DataProvider;
using System.Windows.Input;
using System.ComponentModel;
using System.Resources;
using System.Reflection;

namespace Ecm.Admin.ViewModel
{
    public class PermissionViewModel : ComponentViewModel
    {
        private readonly PermissionProvider _permissionProvider = new PermissionProvider();

        private ObservableCollection<MenuModel> _permissionItems = new ObservableCollection<MenuModel>();
        private ObservableCollection<UserGroupModel> _userGroups = new ObservableCollection<UserGroupModel>();
        private ObservableCollection<DocumentTypeModel> _documentTypes = new ObservableCollection<DocumentTypeModel>();
        private bool _viewByDocumentType = true;
        private bool _viewByUserGroup;
        private bool _isPermissionModified;
        private DocumentTypePermissionModel _permission;
        private AnnotationPermissionModel _annotationPermission;
        private AuditPermissionModel _auditPermission;
        private bool _isSelectAllRedaction;
        private bool _isSelectAllHighlight;
        private bool _isSelectAllText;
        private bool _userGroupEnabled = true;
        private bool _documentTypeEnabled = true;

        private RelayCommand _permissionSelectedCommand;
        private RelayCommand _changeMenuDisplayCommand;
        private RelayCommand _savePermissionCommand;
        private RelayCommand _checkCommand;
        private RelayCommand _selectAllRedactionCommand;
        private RelayCommand _selectAllHighlightCommand;
        private RelayCommand _selectAllTextCommand;
        private readonly ResourceManager _resource = new ResourceManager("Ecm.Admin.Resources", Assembly.GetExecutingAssembly());
        private bool _enableEditPermission;

        public PermissionViewModel()
        {
            EnableEditPermission = false;
        }

        public sealed override void Initialize()
        {
            Permission = null;
            AnnotationPermission = null;
            EnableEditPermission = false;
            GenerateApplicationPermissionMenu();
        }

        public Action SaveCompleted { get; set; }

        public DocumentTypePermissionModel Permission
        {
            get { return _permission; }
            set
            {
                _permission = value;
                EditPanelVisibled = value != null;
                OnPropertyChanged("Permission");
            }
        }

        public AnnotationPermissionModel AnnotationPermission
        {
            get { return _annotationPermission; }
            set
            {
                _annotationPermission = value;
                EditPanelVisibled = value != null;
                OnPropertyChanged("AnnotationPermission");
            }
        }

        public AuditPermissionModel AuditPermission
        {
            get { return _auditPermission; }
            set
            {
                _auditPermission = value;
                OnPropertyChanged("AuditPermission");
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

        public ObservableCollection<DocumentTypeModel> DocumentTypes
        {
            get { return _documentTypes; }
            set
            {
                _documentTypes = value;
                OnPropertyChanged("DocumentTypes");
            }
        }

        public bool ViewByDocumentType
        {
            get { return _viewByDocumentType; }
            set
            {
                _viewByDocumentType = value;
                OnPropertyChanged("ViewByDocumentType");

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
                    ViewByDocumentType = false;
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

        public bool IsSelectAllRedaction
        {
            get { return _isSelectAllRedaction; }
            set
            {
                _isSelectAllRedaction = value;
                OnPropertyChanged("IsSelectAllRedaction");
               
            }
        }

        public bool IsSelectAllText
        {
            get { return _isSelectAllText; }
            set
            {
                _isSelectAllText = value;
                OnPropertyChanged("IsSelectAllText");
            }
        }

        public bool IsSelectAllHighlight
        {
            get { return _isSelectAllHighlight; }
            set
            {
                _isSelectAllHighlight = value;
                OnPropertyChanged("IsSelectAllHighlight");
            }
        }

        public bool DocumentTypeEnabled
        {
            get { return _documentTypeEnabled; }
            set
            {
                _documentTypeEnabled = value;
                OnPropertyChanged("DocumentTypeEnabled");
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

        public ICommand SelectAllHighlightCommand
        {
            get
            {
                return _selectAllHighlightCommand ?? (_selectAllHighlightCommand = new RelayCommand(p => SelectAllHighlight()));
            }
        }

        public ICommand SelectAllRedactionCommand
        {
            get
            {
                return _selectAllRedactionCommand ?? (_selectAllRedactionCommand = new RelayCommand(p => SelectAllRedaction()));
            }
        }

        public ICommand SelectAllTextCommand
        {
            get
            {
                return _selectAllTextCommand ?? (_selectAllTextCommand = new RelayCommand(p => SelectAllText()));
            }
        }

        private void SelectAllText()
        {
            AnnotationPermission.AllowedAddText = AnnotationPermission.AllowedDeleteText = AnnotationPermission.AllowedSeeText = IsSelectAllText;
            _isPermissionModified = true;
        }

        private void SelectAllRedaction()
        {
            AnnotationPermission.AllowedAddRedaction = AnnotationPermission.AllowedDeleteRedaction = AnnotationPermission.AllowedHideRedaction = IsSelectAllRedaction;
            _isPermissionModified = true;
        }

        private void SelectAllHighlight()
        {
            AnnotationPermission.AllowedAddHighlight = AnnotationPermission.AllowedDeleteHighlight = AnnotationPermission.AllowedSeeHighlight = IsSelectAllHighlight;
            _isPermissionModified = true;
        }
        
        //Private methods
        
        private void CheckPermission()
        {
            _isPermissionModified = true;
            CheckSelectAll();
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
                _permissionProvider.SavePermission(Permission, AnnotationPermission, AuditPermission);
                _isPermissionModified = false;
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
                UserGroupModel userGroup = null;
                DocumentTypeModel docType = null;

                if (SelectedMenu.IsDocType && ViewByUserGroup)
                {
                    userGroup = UserGroups.FirstOrDefault(g => g.Id == SelectedMenu.ParentId);
                    docType = DocumentTypes.FirstOrDefault(r => r.Id == SelectedMenu.Guid);
                }
                else if (ViewByDocumentType && SelectedMenu.IsUserGroup)
                {
                    docType = DocumentTypes.FirstOrDefault(r => r.Id == SelectedMenu.ParentId);
                    userGroup = UserGroups.FirstOrDefault(g => g.Id == SelectedMenu.Guid);
                }

                AuditPermission = _permissionProvider.GetAuditPermission(userGroup, docType);
                Permission = _permissionProvider.GetPermission(userGroup, docType);
                AnnotationPermission = _permissionProvider.GetAnnotationPermission(userGroup, docType);

                if (AuditPermission == null)
                {
                    AuditPermission = new AuditPermissionModel
                    {
                        UserGroupId = userGroup.Id,
                        DocTypeId = docType.Id
                    };
                }

                if (Permission == null)
                {
                    Permission = new DocumentTypePermissionModel
                    {
                        UserGroupId = userGroup.Id,
                        DocTypeId = docType.Id
                    };

                }

                if (AnnotationPermission == null)
                {
                    AnnotationPermission = new AnnotationPermissionModel
                    {
                        UserGroupId = userGroup.Id,
                        DocTypeId = docType.Id
                    };

                }
            
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
            var docTypeByUserGroup = new Dictionary<UserGroupModel, ObservableCollection<DocumentTypeModel>>();
            var userGroupsByDocType = new Dictionary<DocumentTypeModel, ObservableCollection<UserGroupModel>>();

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
                _documentTypes = _permissionProvider.GetDocTypes();
                _userGroups = _permissionProvider.GetUserGroups();

                if (_userGroups.Count == 0)
                {
                    UserGroupEnabled = false;
                }
                else
                {
                    UserGroupEnabled = true;
                }

                if (_documentTypes.Count == 0)
                {
                    DocumentTypeEnabled = false;
                }
                else
                {
                    DocumentTypeEnabled = true;
                }

                // Get the related repositories by each user group
                foreach (UserGroupModel userGroup in _userGroups)
                {
                    if (_documentTypes.Count > 0)
                    {
                        docTypeByUserGroup.Add(userGroup, _documentTypes);
                    }
                }

                if (ViewByDocumentType)
                {
                    // Determine the related user groups by each repository
                    foreach (DocumentTypeModel docType in _documentTypes)
                    {
                        var userGroups = new ObservableCollection<UserGroupModel>();

                        foreach (UserGroupModel userGroup in _userGroups)
                        {
                            if (docTypeByUserGroup.Keys.Contains(userGroup))
                            {
                                if (
                                    docTypeByUserGroup[userGroup].Any(
                                        p => p.Id == docType.Id && p.Name == docType.Name))
                                {
                                    userGroups.Add(userGroup);
                                }
                            }
                        }

                        if (userGroups.Count > 0)
                        {
                            userGroupsByDocType.Add(docType, userGroups);
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
                    var node = new MenuModel { Guid = userGroup.Id, MenuText = userGroup.Name, IsUserGroup = true };

                    PermissionItems.Add(node);

                    // Only add the related repositories to user group to display
                    if (docTypeByUserGroup.Keys.Contains(userGroup))
                    {
                        ObservableCollection<DocumentTypeModel> docType = docTypeByUserGroup[userGroup];

                        node.MenuItems = new ObservableCollection<MenuModel>(
                            (from p in docType
                             select new MenuModel {Guid = p.Id, ParentId = node.Guid, MenuText = p.Name, IsDocType = true}).ToList());
                    }
                }
            }
            else
            {
                foreach (DocumentTypeModel docType in _documentTypes)
                {
                    var node = new MenuModel { Guid = docType.Id, MenuText = docType.Name, IsDocType = true };

                    PermissionItems.Add(node);

                    if (userGroupsByDocType.Keys.Contains(docType))
                    {
                        ObservableCollection<UserGroupModel> userGroups = userGroupsByDocType[docType];

                        node.MenuItems = new ObservableCollection<MenuModel>(
                            (from p in userGroups
                             select new MenuModel
                                        {
                                            Guid = p.Id,
                                            ParentId = node.Guid,
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
                DocumentTypeModel docType = null;

                SelectedMenu = selectedMenu;
                ConfirmBeforeSwitchView();

                if (SelectedMenu.IsDocType && ViewByUserGroup)
                {
                    userGroup = UserGroups.FirstOrDefault(g => g.Id == SelectedMenu.ParentId);
                    docType = DocumentTypes.FirstOrDefault(r => r.Id == SelectedMenu.Guid);
                }
                else if (ViewByDocumentType && SelectedMenu.IsUserGroup)
                {
                    docType = DocumentTypes.FirstOrDefault(r => r.Id == SelectedMenu.ParentId);
                    userGroup = UserGroups.FirstOrDefault(g => g.Id == SelectedMenu.Guid);
                }

                EnableEditPermission = false;

                if (userGroup != null && docType != null)
                {
                    AuditPermission = _permissionProvider.GetAuditPermission(userGroup, docType);
                    Permission = _permissionProvider.GetPermission(userGroup, docType);
                    AnnotationPermission = _permissionProvider.GetAnnotationPermission(userGroup, docType);

                    if (AuditPermission == null)
                    {
                        AuditPermission = new AuditPermissionModel { DocTypeId = docType.Id, UserGroupId = userGroup.Id };
                    }
                    if (Permission == null)
                    {
                        Permission = new DocumentTypePermissionModel { DocTypeId = docType.Id, UserGroupId = userGroup.Id };
                    }
                    if (AnnotationPermission == null)
                    {
                        AnnotationPermission = new AnnotationPermissionModel { DocTypeId = docType.Id, UserGroupId = userGroup.Id };
                    }

                    CheckSelectAll();

                    EnableEditPermission = true;
                }
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
                        _permissionProvider.SavePermission(Permission, AnnotationPermission, AuditPermission);
                    }
                    catch (Exception ex)
                    {
                        ProcessHelper.ProcessException(ex);
                    }
                }
            }
            _isPermissionModified = false;
        }

        private void CheckSelectAll()
        {
            if (AnnotationPermission != null)
            {
                IsSelectAllHighlight = AnnotationPermission.AllowedAddHighlight && AnnotationPermission.AllowedDeleteHighlight && AnnotationPermission.AllowedSeeHighlight;
                IsSelectAllRedaction = AnnotationPermission.AllowedAddRedaction && AnnotationPermission.AllowedDeleteRedaction && AnnotationPermission.AllowedHideRedaction;
                IsSelectAllText = AnnotationPermission.AllowedAddText && AnnotationPermission.AllowedDeleteText && AnnotationPermission.AllowedSeeText;
            }
        }
    }
}
