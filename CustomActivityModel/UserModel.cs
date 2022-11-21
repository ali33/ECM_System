using System;
using System.Collections.ObjectModel;
using System.ComponentModel;

using Ecm.Mvvm;
using Ecm.Utility;
using System.Resources;
using System.Reflection;

namespace Ecm.Workflow.Activities.CustomActivityModel
{
    [Serializable]
    public class UserModel : BaseDependencyProperty
    {
        private Guid _id;
        private string _username;
        private string _password;
        private string _fullname;
        private string _emailAddress;
        private bool _isAdmin;
        private ObservableCollection<UserGroupModel> _userGroups = new ObservableCollection<UserGroupModel>();
        private bool _isSelected;
        private string _description;
        private long? _languageId;
        private byte[] _picture;

        public UserModel()
        {
            UserGroups = new ObservableCollection<UserGroupModel>();
        }

        public UserModel(Action<UserModel> action)
        {
            ErrorChecked = action;
        }

        public Action<UserModel> ErrorChecked { get; set; }

        public Guid Id
        {
            get { return _id; }
            set
            {
                _id = value;
                OnPropertyChanged("Id");
            }
        }

        public string Username
        {
            get { return _username; }
            set
            {
                _username = value;
                OnPropertyChanged("Username");
            }
        }

        public string Password
        {
            get { return _password; }
            set
            {
                _password = value;
                OnPropertyChanged("Password");
            }
        }
        public string Fullname
        {
            get { return _fullname; }
            set
            {
                _fullname = value;
                OnPropertyChanged("Fullname");
            }
        }

        public string EmailAddress
        {
            get { return _emailAddress; }
            set
            {
                _emailAddress = value;
                OnPropertyChanged("EmailAddress");
            }
        }

        public string Description
        {
            get { return _description; }
            set
            {
                _description = value;
                OnPropertyChanged("Description");
            }
        }

        public bool IsAdmin
        {
            get
            {
                return _isAdmin;
            }
            set
            {
                _isAdmin = value;
                OnPropertyChanged("IsAdmin");
            }
        }

        public ObservableCollection<UserGroupModel> UserGroups
        {
            get { return _userGroups; }
            set { _userGroups = value; }
        }

        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                _isSelected = value;
                OnPropertyChanged("IsSelected");
            }
        }

        public long? LanguageId
        {
            get { return _languageId; }
            set
            {
                _languageId = value;
                OnPropertyChanged("LanguageId");
            }
        }

        public byte[] Picture
        {
            get { return _picture; }
            set
            {
                _picture = value;
                OnPropertyChanged("Picture");
            }
        }
    }
}
