using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;
using Ecm.Mvvm;
using Ecm.Model;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Ecm.Model.DataProvider;
using System.Resources;
using System.Reflection;
using Ecm.Admin.View;
using System.IO;
using Ecm.Utility;
using System.Configuration;

namespace Ecm.Admin.ViewModel
{
    public class UserViewModel : ComponentViewModel
    {
        private UserModel _selectedUser;
        private UserModel _editUser;
        private ObservableCollection<UserGroupModel> _searchedUserGroups = new ObservableCollection<UserGroupModel>();
        private ObservableCollection<UserModel> _users = new ObservableCollection<UserModel>();
        private string _searchValue;
        private string _pictureButtonContext;
        private RelayCommand _addCommand;
        private RelayCommand _deleteCommand;
        private RelayCommand _saveCommand;
        private RelayCommand _cancelCommand;
        private RelayCommand _searchGroupCommand;
        private RelayCommand _selectUserGroupCommand;
        private RelayCommand _removeGroupCommand;
        private RelayCommand _cancelSelectionUuerGroupCommand;
        private RelayCommand _addUserGroupCommand;
        private RelayCommand _addPictureCommand;
        private RelayCommand _deletePictureCommand;
        private RelayCommand _sortCommand;
        private readonly UserProvider _userProvider = new UserProvider();
        private readonly UserGroupProvider _userGroupProvider = new UserGroupProvider();
        private DialogBaseView _dialog;
        private readonly ResourceManager _resource = new ResourceManager("Ecm.Admin.Resources", Assembly.GetExecutingAssembly());
        private string version = ConfigurationManager.AppSettings["Version"].ToString();
        private int _userCount;
        private bool _isACS;
        //Events
        //public event CloseDialog CloseDialog;

        public UserModel SelectedUser
        {
            get { return _selectedUser; }
            set
            {
                _selectedUser = value;

                OnPropertyChanged("SelectedUser");
            }
        }

        public UserModel EditUser
        {
            get { return _editUser; }
            set
            {
                _editUser = value;
                EditPanelVisibled = value != null;

                if (value != null)
                {
                    if (value.Picture != null && value.Picture.Length > 0)
                    {
                        PictureButtonContext = _resource.GetString("uiEditPictureContent");
                    }
                    else
                    {
                        PictureButtonContext = _resource.GetString("uiAddPictureContent");
                    }
                }

                OnPropertyChanged("EditUser");
            }
        }

        public string PictureButtonContext
        {
            get { return _pictureButtonContext; }
            set
            {
                _pictureButtonContext = value;
                OnPropertyChanged("PictureButtonContext");
            }
        }

        public string SearchValue
        {
            get { return _searchValue; }
            set
            {
                _searchValue = value;
                OnPropertyChanged("SearchValue");
            }
        }

        public ObservableCollection<UserGroupModel> SearchedUserGroups
        {
            get { return _searchedUserGroups; }
            set
            {
                _searchedUserGroups = value;
                OnPropertyChanged("SearchedUserGroups");
            }
        }

        public ObservableCollection<UserModel> Users
        {
            get { return _users; }
            set
            {
                _users = value;
                OnPropertyChanged("Users");
            }
        }

        public ObservableCollection<LanguageModel> Languages { get; set; }

        public bool IsACS
        {
            get { return _isACS; }
            set
            {
                _isACS = value;
                OnPropertyChanged("IsASC");
            }
        }

        public ICommand AddCommand
        {
            get { return _addCommand ?? (_addCommand = new RelayCommand(p => AddUser(), p=> CanAddUser())); }
        }

        private bool CanAddUser()
        {
            if (Utility.CryptographyHelper.DecryptUsingSymmetricAlgorithm(version, "D4A88355-7148-4FF2-A626-151A40F57330") == "demoversion")//kN+tuxaBIsS85NuA6yFXEQ==
            {
                return _userCount < 2;
            }
            else if (Utility.CryptographyHelper.DecryptUsingSymmetricAlgorithm(version, "D4A88355-7148-4FF2-A626-151A40F57330") == "fullversion")//02MNOS0VxUplqqL/JBj7Jw==
            {
                return true;
            }

            return false;
        }

        public ICommand DeleteCommand
        {
            get { return _deleteCommand ?? (_deleteCommand = new RelayCommand(p => DeleteUser(), p => CanDeleteUser())); }
        }

        public ICommand SaveCommand
        {
            get { return _saveCommand ?? (_saveCommand = new RelayCommand(p => SaveUser(), p => CanSaveUser())); }
        }

        public ICommand CancelCommand
        {
            get { return _cancelCommand ?? (_cancelCommand = new RelayCommand(p => Cancel())); }
        }

        public ICommand SearchGroupCommand
        {
            get { return _searchGroupCommand ?? (_searchGroupCommand = new RelayCommand(p => SearchGroup())); }
        }

        public ICommand SelectGroupCommand
        {
            get { return _selectUserGroupCommand ?? (_selectUserGroupCommand = new RelayCommand(p => SelectGroup(), p => CanSelectGroup())); }
        }

        public ICommand CancelSelectGroupCommand
        {
            get { return _cancelSelectionUuerGroupCommand ?? (_cancelSelectionUuerGroupCommand = new RelayCommand(p => CancelSelection())); }
        }

        public ICommand RemoveGroupCommand
        {
            get { return _removeGroupCommand ?? (_removeGroupCommand = new RelayCommand(p => RemoveGroup(), p => CanRemoveGroup())); }
        }

        public ICommand AddUserGroupCommand
        {
            get
            {
                return _addUserGroupCommand ?? (_addUserGroupCommand = new RelayCommand(p => AddUserGroup(), p => CanAddUserGroup()));
            }
        }

        public ICommand AddPictureCommand
        {
            get { return _addPictureCommand ?? (_addPictureCommand = new RelayCommand(p => AddPicture())); }
        }

        public ICommand DeletePictureCommand
        {
            get { return _deletePictureCommand ?? (_deletePictureCommand = new RelayCommand(p => DeletePicture(), p=> CanDeletePicture())); }
        }

        public ICommand SortCommand
        {
            get { return _sortCommand ?? (_sortCommand = new RelayCommand(p => Sort(p))); }
        }

        public sealed override void Initialize()
        {
            UpdateUserCount();
            SelectedUser = null;
            LoadData();
        }

        public void CheckUserExist(UserModel user)
        {
            bool hasError = false;
            if (!string.IsNullOrWhiteSpace(user.Username))
            {
                hasError = Users.Any(p => p.Username.Equals(user.Username.Trim(), StringComparison.CurrentCultureIgnoreCase) && p.Id != user.Id);
            }

            user.HasError = hasError;
        }

        //Private methods
        private void Sort(object sortDirection)
        {
            bool direction = (bool)sortDirection;
            Users.Clear();

            var userList = _userProvider.GetUsers();
            if (direction)
            {
                var desc = from u in userList
                           orderby u.Username descending
                           select u;
                Users = new ObservableCollection<UserModel>(desc);
            }
            else
            {
                var asc = from u in userList
                          orderby u.Username ascending
                          select u;
                Users = new ObservableCollection<UserModel>(asc);
            }

        }

        private bool CanAddUserGroup()
        {
            return EditUser != null && !EditUser.IsAdmin;
        }

        private void AddUserGroup()
        {
            SearchedUserGroups = new ObservableCollection<UserGroupModel>();
            SearchValue = string.Empty;

            var dialogView = new SelectionUserGroupView(this);
            var resource = new ResourceManager("Ecm.Admin.SelectionUserGroupView", Assembly.GetExecutingAssembly());
            _dialog = new DialogBaseView(dialogView)
            {
                Text = resource.GetString("dgDialogTitle"),
                Size = new System.Drawing.Size(600, 330)
            };
            _dialog.ShowDialog();
        }

        private void CancelSelection()
        {
            SearchedUserGroups = new ObservableCollection<UserGroupModel>();

            if (_dialog != null)
            {
                _dialog.Close();
            }
        }

        private bool CanRemoveGroup()
        {
            return EditUser != null && EditUser.UserGroups != null && EditUser.UserGroups.Any(g => g.IsSelected);
        }

        private void RemoveGroup()
        {
            IList<UserGroupModel> userGroups = EditUser.UserGroups.ToList();
            foreach (UserGroupModel group in userGroups)
            {
                if (group.IsSelected)
                {
                    EditUser.UserGroups.Remove(group);
                }
            }
        }

        private bool CanSelectGroup()
        {
            return SearchedUserGroups.Any(g => g.IsSelected);
        }

        private void SelectGroup()
        {
            foreach (UserGroupModel group in SearchedUserGroups)
            {
                if (group.IsSelected)
                {
                    EditUser.UserGroups.Add(group);
                }
            }

            if (_dialog != null)
            {
                _dialog.Close();
            }
        }

        private void SearchGroup()
        {
            IsProcessing = true;

            var worker = new BackgroundWorker();
            worker.DoWork += DoSearch;
            worker.RunWorkerCompleted += DoSearchCompleted;
            worker.RunWorkerAsync();
        }

        private void DoSearchCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            IsProcessing = false;
        }

        private void DoSearch(object sender, DoWorkEventArgs e)
        {
            try
            {
                SearchedUserGroups = new ObservableCollection<UserGroupModel>();
                IList<UserGroupModel> userGroups = _userGroupProvider.GetUserGroups();

                IEnumerable<UserGroupModel> query = from p in userGroups
                                                    where !EditUser.UserGroups.Any(q => q.Id == p.Id) &&
                                                          p.Name.ToLower().Contains(SearchValue.ToLower())
                                                    select p;
                SearchedUserGroups = new ObservableCollection<UserGroupModel>(query);
            }
            catch (Exception ex)
            {
                ProcessHelper.ProcessException(ex);
            }
        }

        private void Cancel()
        {
            EditPanelVisibled = false;
            EditUser = null;
            SelectedUser = null;

            if (ResetListView != null)
            {
                ResetListView();
            }

            LoadData();
        }

        private void AddUser()
        {
            IsEditMode = true;

            if (ResetListView != null)
            {
                ResetListView();
            }

            EditUser = new UserModel(CheckUserExist);
            if (Languages != null && Languages.Count > 0)
            {
                EditUser.Language = Languages[0];
            }
        }

        private void DeleteUser()
        {
            var resource = new ResourceManager("Ecm.Admin.Resources", Assembly.GetExecutingAssembly());

            if (DialogService.ShowTwoStateConfirmDialog(resource.GetString("uiConfirmDelete")) == DialogServiceResult.Yes)
            {
                IsProcessing = true;

                var worker = new BackgroundWorker();
                worker.DoWork += DoDeleteUser;
                worker.RunWorkerCompleted += DoDeleteCompleted;
                worker.RunWorkerAsync();
            }
        }

        private void DoDeleteCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            IsProcessing = false;
            UpdateUserCount();
        }

        private void DoDeleteUser(object sender, DoWorkEventArgs e)
        {
            try
            {
                _userProvider.DeleteUser(SelectedUser);
                LoadData();
            }
            catch (Exception ex)
            {
                ProcessHelper.ProcessException(ex);
            }
        }

        private bool CanDeleteUser()
        {
            return SelectedUser != null;
        }

        private bool CanSaveUser()
        {
            bool isValid = false;

            if (EditUser != null && !string.IsNullOrWhiteSpace(EditUser.Username))
            {
                isValid = !Users.Any(p => p.Username.Trim().Equals(EditUser.Username.Trim(), StringComparison.CurrentCultureIgnoreCase) && p.Id != EditUser.Id) && 
                    CommonValidator.IsEmail(EditUser.EmailAddress);
            }

            return EditUser != null && isValid && !string.IsNullOrWhiteSpace(EditUser.Username) &&
                !string.IsNullOrWhiteSpace(EditUser.Password) && !string.IsNullOrWhiteSpace(EditUser.EmailAddress) && EditUser.Language != null;
        }

        private void SaveUser()
        {
            IsProcessing = true;
            EditUser.LanguageId = EditUser.Language.Id;
            if (EditUser.IsAdmin)
            {
                EditUser.UserGroups.Clear();
            }

            var worker = new BackgroundWorker();
            worker.DoWork += DoSave;
            worker.RunWorkerCompleted += DoSaveCompleted;
            worker.RunWorkerAsync();
        }

        private void DoSaveCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            IsProcessing = false;
            UpdateUserCount();
        }

        private void DoSave(object sender, DoWorkEventArgs e)
        {
            try
            {
                _userProvider.SaveUser(EditUser);
                EditUser = null;
                LoadData();
            }
            catch (Exception ex)
            {
                ProcessHelper.ProcessException(ex);
            }

        }

        private void LoadData()
        {
            try
            {
                Languages = new ObservableCollection<LanguageModel>(new LanguageProvider().GetLanguages());
                Users = _userProvider.GetUsers();
                EditUser = null;
            }
            catch (Exception ex)
            {
                ProcessHelper.ProcessException(ex);
            }
        }

        private void AddPicture()
        {
            string filePath = DialogService.ShowFileBrowseDialog(string.Empty);
            if (!string.IsNullOrEmpty(filePath))
            {
                string extension = Path.GetExtension(filePath);
                var allowedExtensions = new[] { ".png", ".jpg" };
                if (allowedExtensions.Contains(extension.ToLower()))
                {
                    EditUser.Picture = Utility.UtilsStream.ReadAllBytes(new FileStream(filePath, FileMode.Open, FileAccess.Read));
                }
                else
                {
                    DialogService.ShowErrorDialog("Only image is supported. (PNG) and (JPEG)");
                }
            }
        }

        private bool CanDeletePicture()
        {
            return EditUser != null && EditUser.Picture != null && EditUser.Picture.Length > 0;
        }

        private void DeletePicture()
        {
            EditUser.Picture = null;
        }

        private void UpdateUserCount()
        {
            _userCount = new UserProvider().GetUsers().Count;
        }

    }
}
