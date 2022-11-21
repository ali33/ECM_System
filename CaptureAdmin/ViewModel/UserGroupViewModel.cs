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
    public class UserGroupViewModel : ComponentViewModel
    {
        private RelayCommand _addGroupCommand;
        private RelayCommand _deleteGroupCommand;
        private RelayCommand _saveCommand;
        private RelayCommand _cancelCommand;
        private RelayCommand _removeUserCommand;
        private RelayCommand _selectUserCommand;
        private RelayCommand _cancelSelectionUserCommand;
        private RelayCommand _searchUserCommand;

        private UserGroupModel _selectedUserGroup;
        private UserGroupModel _editUserGroup;

        private ObservableCollection<UserGroupModel> _userGroups = new ObservableCollection<UserGroupModel>();
        private ObservableCollection<UserModel> _searchedUsers = new ObservableCollection<UserModel>();
        private string _searchValue;

        private readonly UserGroupProvider _userGroupProvider = new UserGroupProvider();
        private readonly UserProvider _userProvider = new UserProvider();
        private readonly ResourceManager _resource = new ResourceManager("Ecm.CaptureAdmin.Resources", Assembly.GetExecutingAssembly());
        //Events
        public event CloseDialog CloseDialog;

        public override sealed void Initialize()
        {
            try
            {
                SelectedUserGroup = null;
                LoadData();
            }
            catch (Exception ex)
            {
                ProcessHelper.ProcessException(ex);
            }
        }

        public void CheckUserGroup(UserGroupModel userGroup)
        {
            bool hasError = false;

            if (!string.IsNullOrWhiteSpace(userGroup.Name))
            {
                hasError = UserGroups.Any(p => p.Name.Equals(userGroup.Name, StringComparison.CurrentCultureIgnoreCase) && p.Id != userGroup.Id);
            }

            userGroup.HasError = hasError;
        }

        //Public properties
        public UserGroupModel SelectedUserGroup
        {
            get { return _selectedUserGroup; }
            set
            {
                _selectedUserGroup = value;
                OnPropertyChanged("SelectedUserGroup");
            }
        }

        public UserGroupModel EditUserGroup
        {
            get { return _editUserGroup; }
            set
            {
                _editUserGroup = value;
                EditPanelVisibled = value != null;
                OnPropertyChanged("EditUserGroup");
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

        public ObservableCollection<UserGroupModel> UserGroups
        {
            get { return _userGroups; }
            set
            {
                _userGroups = value;
                OnPropertyChanged("UserGroups");
            }
        }

        public ObservableCollection<UserModel> SearchedUsers
        {
            get { return _searchedUsers; }
            set
            {
                _searchedUsers = value;
                OnPropertyChanged("SearchedUsers");
            }
        }

        public ICommand AddGroupCommand
        {
            get { return _addGroupCommand ?? (_addGroupCommand = new RelayCommand(p => AddGroup())); }
        }

        public ICommand DeleteGroupCommand
        {
            get { return _deleteGroupCommand ?? (_deleteGroupCommand = new RelayCommand(p => DeleteGroup(), p => CanDeleteGroup())); }
        }

        public ICommand SelectUserCommand
        {
            get { return _selectUserCommand ?? (_selectUserCommand = new RelayCommand(p => SelectUser(),p => CanSelectUser())); }
        }

        public ICommand RemoveUserCommand
        {
            get { return _removeUserCommand ?? (_removeUserCommand = new RelayCommand(p => RemoveUser(), p => CanRemoveUser())); }
        }

        public ICommand SearchUserCommand
        {
            get { return _searchUserCommand ?? (_searchUserCommand = new RelayCommand(p => SearchUser(), p => CanSearch())); }
        }

        public ICommand SaveCommand
        {
            get { return _saveCommand ?? (_saveCommand = new RelayCommand(p => Save(), p => CanSave())); }
        }

        public ICommand CancelCommand
        {
            get { return _cancelCommand ?? (_cancelCommand = new RelayCommand(p => Cancel())); }
        }

        public ICommand CancelSelectUserCommand
        {
            get { return _cancelSelectionUserCommand ?? (_cancelSelectionUserCommand = new RelayCommand(p => CancelSelectUser())); }
        }

        //Private methods

        private bool CanSelectUser()
        {
            if (SearchedUsers != null)
            {
                return SearchedUsers.Any(user => user.IsSelected);
            }

            return false;
        }

        private void CancelSelectUser()
        {
            SearchedUsers = new ObservableCollection<UserModel>();
            if (CloseDialog != null)
            {
                CloseDialog();
            }
        }

        private void Cancel()
        {
            EditUserGroup = null;
            EditPanelVisibled = false;
            if (ResetListView != null)
            {
                ResetListView();
            }

            LoadData();
        }

        private bool CanRemoveUser()
        {
            return EditUserGroup != null && EditUserGroup.Users != null && EditUserGroup.Users.Any(u => u.IsSelected);
        }

        private bool CanSearch()
        {
            return true;
        }

        private bool CanSave()
        {
            return EditUserGroup != null && !EditUserGroup.HasError && !string.IsNullOrEmpty(EditUserGroup.Name);
        }

        private void Save()
        {
            IsProcessing = true;

            var worked = new BackgroundWorker();
            worked.DoWork += DoSave;
            worked.RunWorkerCompleted += DoSaveCompleted;
            worked.RunWorkerAsync();
        }

        private void DoSaveCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            IsProcessing = false;
        }

        private void DoSave(object sender, DoWorkEventArgs e)
        {
            try
            {
                _userGroupProvider.Save(EditUserGroup);
                EditUserGroup = null;
                LoadData();
            }
            catch (Exception ex)
            {
                ProcessHelper.ProcessException(ex);
            }
        }

        private void SearchUser()
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
                SearchedUsers = new ObservableCollection<UserModel>();
                // Only assign normal user to user group, ignore admin user
                IList<UserModel> users = _userProvider.GetUsers().Where(p => !p.IsAdmin).ToList();

                IEnumerable<UserModel> query = from p in users
                                               where !EditUserGroup.Users.Any(q => q.Id == p.Id) &&
                                                     p.Username.ToLower().Contains(SearchValue.ToLower())
                                               select p;
                SearchedUsers = new ObservableCollection<UserModel>(query);
            }
            catch (Exception ex)
            {
                ProcessHelper.ProcessException(ex);
            }
        }

        private void RemoveUser()
        {
            IList<UserModel> users = EditUserGroup.Users.ToList();
            foreach (UserModel user in users)
            {
                if (user.IsSelected)
                {
                    EditUserGroup.Users.Remove(user);
                }
            }
        }

        private void SelectUser()
        {
            foreach (UserModel user in SearchedUsers)
            {
                if (user.IsSelected)
                {
                    EditUserGroup.Users.Add(user);
                }
            }

            if (CloseDialog != null)
            {
                CloseDialog();
            }
        }

        private void DeleteGroup()
        {
            if (DialogService.ShowTwoStateConfirmDialog(_resource.GetString("uiConfirmDelete")) == DialogServiceResult.Yes)
            {
                IsProcessing = true;

                var worker = new BackgroundWorker();
                worker.DoWork += DoDelete;
                worker.RunWorkerCompleted += DoDeleteCompleted;
                worker.RunWorkerAsync();
            }
        }

        private void DoDeleteCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            IsProcessing = false;
        }

        private void DoDelete(object sender, DoWorkEventArgs e)
        {
            try
            {
                _userGroupProvider.DeleteUserGroup(SelectedUserGroup);
                SelectedUserGroup = null;
                EditUserGroup = null;
                LoadData();
            }
            catch (Exception ex)
            {
                ProcessHelper.ProcessException(ex);
            }
        }

        private bool CanDeleteGroup()
        {
            return SelectedUserGroup != null;
        }

        private void AddGroup()
        {
            EditUserGroup = new UserGroupModel(CheckUserGroup);
        }

        private void LoadData()
        {
            UserGroups = _userGroupProvider.GetUserGroups();
        }
    }
}
