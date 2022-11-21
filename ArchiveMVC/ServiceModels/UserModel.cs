using System;
using Ecm.Mvvm;

using System.ComponentModel;
using Ecm.Utility;
using System.Collections.Generic;

namespace ArchiveMVC.Models
{
    [Serializable]
    public class UserModel
    {
        private Guid _id;
        private string _username;
        private string _password;
        private string _fullname;
        private string _emailAddress;
        private bool _isAdmin;
        private UserTypeModel _type;
        private List<UserGroupModel> _userGroups = new List<UserGroupModel>();
        private bool _isSelected;
        private string _description;
        private Guid? _languageId;
        private LanguageModel _language;
        private byte[] _picture;

        public UserModel()
        {
            UserGroups = new List<UserGroupModel>();
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
            }
        }

        public string Username
        {
            get { return _username; }
            set
            {
                _username = value;
            }
        }

        public string Password
        {
            get { return _password; }
            set
            {
                _password = value;
            }
        }
        public string Fullname
        {
            get { return _fullname; }
            set
            {
                _fullname = value;
            }
        }

        public string EmailAddress
        {
            get { return _emailAddress; }
            set
            {
                _emailAddress = value;
            }
        }

        public string Description
        {
            get { return _description; }
            set
            {
                _description = value;
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
            }
        }

        public UserTypeModel Type
        {
            get { return _type; }
            set
            {
                _type = value;
            }
        }

        public List<UserGroupModel> UserGroups
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
            }
        }

        public Guid? LanguageId
        {
            get { return _languageId; }
            set
            {
                _languageId = value;
            }
        }

        public LanguageModel Language
        {
            get { return _language; }
            set
            {
                _language = value;
            }
        }

        public byte[] Picture
        {
            get { return _picture; }
            set
            {
                _picture = value;
            }
        }

        public bool HasError { get; set; }

    }
}
