using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using Ecm.CaptureDomain;
using Ecm.Mvvm;
using Ecm.Workflow.Activities.CustomActivityModel;
using System;
using Ecm.Workflow.Activities.CustomActivityDomain;

namespace Ecm.Workflow.Activities.HumanStepPermissionDesigner.ViewModel
{
    public class HumanStepPermissionViewModel : ComponentViewModel
    {
        private UserGroupPermissionViewModel _selectedUserGroupPermissionViewModel;
        private ObservableCollection<AnnotationPermissionViewModel> _annotaionViewModel = new ObservableCollection<AnnotationPermissionViewModel>();
        private ObservableCollection<DocumentFieldPermissionViewModel> _fieldViewModel = new ObservableCollection<DocumentFieldPermissionViewModel>();
        private bool _isViewAvailable;
        private ObservableCollection<MenuItemModel> _userGroupMenuItems;
        private UserGroup _selectedUserGroup;
        private bool _isDataModified;

        public List<UserGroup> UserGroups { get; set; }

        public List<DocumentType> DocTypes { get; set; }

        public ActivityPermission Permission { get; set; }

        public UserGroupPermissionViewModel SelectedUserGroupPermissionViewModel
        {
            get { return _selectedUserGroupPermissionViewModel; }
            set
            {
                _selectedUserGroupPermissionViewModel = value;
                OnPropertyChanged("SelectedUserGroupPermissionViewModel");
            }
        }

        public ObservableCollection<MenuItemModel> UserGroupMenuItems
        {
            get { return _userGroupMenuItems; }
            set
            {
                _userGroupMenuItems = value;
                OnPropertyChanged("UserGroupMenuItems");
            }
        }

        public ObservableCollection<AnnotationPermissionViewModel> AnnotationPermissionViewModels
        {
            get { return _annotaionViewModel; }
            set
            {
                _annotaionViewModel = value;
                OnPropertyChanged("AnnotationPermissionViewModels");
            }
        }

        public ObservableCollection<DocumentFieldPermissionViewModel> DocumentFieldPermissionViewModels
        {
            get { return _fieldViewModel; }
            set
            {
                _fieldViewModel = value;
                OnPropertyChanged("DocumentFieldPermissionViewModels");
            }
        }

        public bool IsViewAvailable
        {
            get
            {
                return _isViewAvailable;
            }
            set
            {
                _isViewAvailable = value;
                OnPropertyChanged("IsViewAvailable");
            }
        }

        public UserGroup SelectedUserGroup
        {
            get { return _selectedUserGroup; }
            set
            {
                _selectedUserGroup = value;
                OnPropertyChanged("SelectedUserGroup");
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

        public new void Initialize()
        {
            // Initialize user group menu
            var currentMenuItems = new ObservableCollection<MenuItemModel>();

            var root = new MenuItemModel
            {
                DisplayText = "User groups",
                Id = Guid.Empty,
                Type = MenuItemType.Root,
                MenuItems = new ObservableCollection<MenuItemModel>(),
                IsExpand = true
            };
            currentMenuItems.Add(root);

            if (Permission != null && Permission.UserGroupPermissions != null)
            {
                //foreach (HumanStepUserGroupPermission userGroupPermission in Permission.UserGroupPermissions)
                for (int i = 0; i < Permission.UserGroupPermissions.Count; i++)
                {
                    UserGroupPermission userGroupPermission = Permission.UserGroupPermissions[i];
                    UserGroup group = UserGroups.FirstOrDefault(p => p.Id == userGroupPermission.UserGroupId);

                    if (group != null)
                    {
                        MenuItemModel menuItem = new MenuItemModel
                                               {
                                                   DisplayText = group.Name,
                                                   Id = group.Id,
                                                   Type = MenuItemType.UserGroup,
                                                   Parent = root
                                               };

                        root.MenuItems.Add(menuItem);
                    }
                }

                root.MenuItems = GetMenuItems(root.MenuItems.OrderBy(p => p.DisplayText).ToList());
            }

            UserGroupMenuItems = currentMenuItems;
            InitializeFakeUserGroupPermission();

            SetSelection(UserGroupMenuItems[0].MenuItems.FirstOrDefault());
        }

        public void InitializeUserGroupPermission()
        {
            if (SelectedUserGroup != null)
            {
                InitializeUserGroupPermission(SelectedUserGroup.Id);
            }
        }

        public void InitializeAnnotationPermission()
        {
            if (SelectedUserGroup != null)
            {
                InitializeAnnotationPermission(SelectedUserGroup.Id);
            }
        }

        public void InitializeDocumentTypePermission()
        {
            if (SelectedUserGroup != null)
            {
                InitializeDocumentFieldPermission(SelectedUserGroup.Id);
            }
        }

        public void SetSelection(MenuItemModel menuItem)
        {
            if (menuItem != null)
            {
                ClearSelection(UserGroupMenuItems);
                menuItem.IsSelected = true;
            }
        }

        public void RemoveUserGroup(MenuItemModel menuItem)
        {
            if (menuItem != null)
            {
                var removedItem = UserGroupMenuItems[0].MenuItems.FirstOrDefault(p => p.Id == menuItem.Id);
                if (removedItem != null)
                {
                    UserGroupMenuItems[0].MenuItems.Remove(removedItem);
                    //HumanStepUserGroupPermission userGroupPermission = Permission.UserGroupPermissions.FirstOrDefault(p => p.UserGroupId == menuItem.Id);
                    UserGroupPermission userGroupPermission = Permission.UserGroupPermissions.FirstOrDefault(p => p.UserGroupId == menuItem.Id);
                    List<AnnotationPermission> annotationPermissions = Permission.AnnotationPermissions.Where(p => p.UserGroupId == menuItem.Id).ToList();
                    List<DocumentPermission> documentPermissions = Permission.DocumentPermissions.Where(p => p.UserGroupId == menuItem.Id).ToList();

                    if (userGroupPermission != null)
                    {
                        Permission.UserGroupPermissions.Remove(userGroupPermission);
                    }

                    foreach (AnnotationPermission annotationPermission in annotationPermissions)
                    {
                        Permission.AnnotationPermissions.Remove(annotationPermission);
                    }

                    foreach (DocumentPermission documentPermission in documentPermissions)
                    {
                        Permission.DocumentPermissions.Remove(documentPermission);
                    }

                    IsDataModified = true;
                    InitializeFakeUserGroupPermission();
                    SetSelection(UserGroupMenuItems[0].MenuItems.FirstOrDefault());
                }
            }
        }

        public List<UserGroup> GetUnassignedUserGroups()
        {
            return UserGroups.Where(g => !UserGroupMenuItems[0].MenuItems.Any(p => p.Id == g.Id)).OrderBy(p => p.Name).ToList();
        }

        public void AddUserGroups(List<UserGroup> userGroups)
        {
            MenuItemModel menuItemModel = null;
            foreach (UserGroup userGroup in userGroups)
            {
                menuItemModel = new MenuItemModel()
                {
                    Id = userGroup.Id,
                    Type = MenuItemType.UserGroup,
                    Parent = UserGroupMenuItems[0],
                    DisplayText = userGroup.Name,
                };

                UserGroupMenuItems[0].MenuItems.Add(menuItemModel);
                InitializeUserGroupPermission(userGroup.Id);
                InitializeAnnotationPermission(userGroup.Id);
                InitializeDocumentFieldPermission(userGroup.Id);
            }

            UserGroupMenuItems[0].MenuItems = GetMenuItems(UserGroupMenuItems[0].MenuItems.OrderBy(p => p.DisplayText).ToList());
            SetSelection(menuItemModel);

            IsDataModified = true;
        }

        private void ClearSelection(IEnumerable<MenuItemModel> menuItems)
        {
            if (menuItems != null)
            {
                foreach (MenuItemModel menuItem in menuItems)
                {
                    menuItem.IsSelected = false;
                    if (menuItem.MenuItems != null && menuItem.MenuItems.Count > 0)
                    {
                        ClearSelection(menuItem.MenuItems);
                    }
                }
            }
        }

        private void InitializeUserGroupPermission(Guid groupId)
        {
            if (groupId != Guid.Empty)
            {
                //HumanStepUserGroupPermission userGroupPermission = Permission.UserGroupPermissions.FirstOrDefault(p => p.UserGroupId == groupId);
                UserGroupPermission userGroupPermission = Permission.UserGroupPermissions.FirstOrDefault(p => p.UserGroupId == groupId);
                if (userGroupPermission == null)
                {
                    userGroupPermission = new UserGroupPermission//new HumanStepUserGroupPermission
                    {
                        UserGroupId = groupId,
                        CanViewOtherItems = true
                    };

                    Permission.UserGroupPermissions.Add(userGroupPermission);
                }

                SelectedUserGroupPermissionViewModel = new UserGroupPermissionViewModel(userGroupPermission, DocTypes);
                SelectedUserGroupPermissionViewModel.PropertyChanged += SelectedUserGroupPermissionViewModelPropertyChanged;
            }
            else
            {
                InitializeFakeUserGroupPermission();
            }
        }

        private void InitializeAnnotationPermission(Guid groupId)
        {
            if (groupId != Guid.Empty)
            {
                //List<HumanStepAnnotationPermission> annotationPermissions = Permission.AnnotationPermissions.Where(p => p.UserGroupId == groupId).ToList();
                List<AnnotationPermission> annotationPermissions = Permission.AnnotationPermissions.Where(p => p.UserGroupId == groupId).ToList();
                if (annotationPermissions != null && annotationPermissions.Count > 0)
                {
                    AnnotationPermissionViewModels.Clear();

                    foreach (var docType in DocTypes)
                    {
                       

                        var anno = annotationPermissions.FirstOrDefault(h=>h.DocTypeId == docType.Id);
                        if (anno == null)
                        {
                            anno = new AnnotationPermission() { UserGroupId = groupId, DocTypeId = docType.Id };
                            Permission.AnnotationPermissions.Add(anno);
                            AnnotationPermissionViewModels.Add(new AnnotationPermissionViewModel(anno, docType));
                        }
                        else
                        {
                            AnnotationPermissionViewModel annotationViewModel = new AnnotationPermissionViewModel(anno, docType);
                            annotationViewModel.PropertyChanged += annotationViewModel_PropertyChanged;
                            AnnotationPermissionViewModels.Add(annotationViewModel);
                        }
                    }
                }
                else
                {
                    InitializeDefaultAnnotationPermission(groupId);
                }
            }
            else
            {
                InitializeFakeAnnotationPermission();
            }
        }

        private void InitializeDocumentFieldPermission(Guid groupId)
        {
            if (groupId != Guid.Empty)
            {
                List<DocumentPermission> documentTypePermissions = Permission.DocumentPermissions.Where(p => p.UserGroupId == groupId).ToList();
                if (documentTypePermissions != null && documentTypePermissions.Count > 0)
                {
                    DocumentFieldPermissionViewModels.Clear();

                    foreach (var docType in DocTypes)
                    {
                        var fieldPermission = documentTypePermissions.FirstOrDefault(h => h.DocTypeId == docType.Id);
                        if (fieldPermission == null)
                        {
                            DocumentPermission docTypePermission = new DocumentPermission() { UserGroupId = groupId, DocTypeId = docType.Id };
                            Permission.DocumentPermissions.Add(docTypePermission);
                            DocumentFieldPermissionViewModels.Add(new DocumentFieldPermissionViewModel(docType, docTypePermission));
                        }
                        else
                        {
                            DocumentFieldPermissionViewModel fieldViewModel = new DocumentFieldPermissionViewModel(docType, fieldPermission);
                            fieldViewModel.PropertyChanged += fieldViewModel_PropertyChanged;
                            DocumentFieldPermissionViewModels.Add(fieldViewModel);
                        }
                    }
                }
                else
                {
                    InitializeDefaultDocumentFieldPermission(groupId);
                }
            }
            else
            {
                InitializeFakeDocumentFieldPermission();
            }
        }

        void fieldViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IsDataModified")
            {
                IsDataModified = true;
            }
        }

        void annotationViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IsDataModified")
            {
                IsDataModified = true;
            }
        }

        private void InitializeFakeUserGroupPermission()
        {
            //SelectedUserGroupPermissionViewModel = new UserGroupPermissionViewModel(new HumanStepUserGroupPermission
            SelectedUserGroupPermissionViewModel = new UserGroupPermissionViewModel(new UserGroupPermission
                                                                                               {
                                                                                                   UserGroupId = Guid.Empty,
                                                                                                   CanViewOtherItems = true
                                                                                               }, DocTypes);
            IsViewAvailable = false;
        }

        private void InitializeDefaultAnnotationPermission(Guid groupId)
        {
            AnnotationPermissionViewModels.Clear();
            foreach (DocumentType docType in DocTypes)
            {
                //HumanStepAnnotationPermission anno = new HumanStepAnnotationPermission() { UserGroupId = groupId, DocTypeId = docType.Id };
                AnnotationPermission anno = new AnnotationPermission() { UserGroupId = groupId, DocTypeId = docType.Id };
                Permission.AnnotationPermissions.Add(anno);
                AnnotationPermissionViewModels.Add(new AnnotationPermissionViewModel(anno, docType));
            }

            //IsViewAvailable = false;
        }

        private void InitializeDefaultDocumentFieldPermission(Guid groupId)
        {
            DocumentFieldPermissionViewModels.Clear();
            foreach (DocumentType docType in DocTypes)
            {
                //HumanStepAnnotationPermission anno = new HumanStepAnnotationPermission() { UserGroupId = groupId, DocTypeId = docType.Id };
                DocumentPermission docTypePermission = new DocumentPermission() { UserGroupId = groupId, DocTypeId = docType.Id };
                Permission.DocumentPermissions.Add(docTypePermission);
                DocumentFieldPermissionViewModels.Add(new DocumentFieldPermissionViewModel(docType, docTypePermission));
            }

            //IsViewAvailable = false;
        }

        private void InitializeFakeAnnotationPermission()
        {
            AnnotationPermissionViewModels.Clear();
            foreach (DocumentType docType in DocTypes)
            {
                //AnnotationPermissionViewModels.Add(new AnnotationPermissionViewModel(new HumanStepAnnotationPermission(), docType));
                AnnotationPermissionViewModels.Add(new AnnotationPermissionViewModel(new AnnotationPermission(), docType));
            }

            IsViewAvailable = false;
        }

        private void InitializeFakeDocumentFieldPermission()
        {
            DocumentFieldPermissionViewModels.Clear();
            foreach (DocumentType docType in DocTypes)
            {
                //AnnotationPermissionViewModels.Add(new AnnotationPermissionViewModel(new HumanStepAnnotationPermission(), docType));
                DocumentFieldPermissionViewModels.Add(new DocumentFieldPermissionViewModel(docType, new DocumentPermission()));
            }

            IsViewAvailable = false;
        }

        private void SelectedUserGroupPermissionViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IsDataModified")
            {
                IsDataModified = true;
            }
        }

        private ObservableCollection<MenuItemModel> GetMenuItems(List<MenuItemModel> menuItems)
        {
            var resultMenuItems = new ObservableCollection<MenuItemModel>();

            menuItems.ForEach(resultMenuItems.Add);

            return resultMenuItems;
        }
    }
}
