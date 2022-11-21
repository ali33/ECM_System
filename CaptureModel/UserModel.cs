using System;
using System.Collections.ObjectModel;
using System.ComponentModel;

using Ecm.Mvvm;
using Ecm.Utility;
using System.Resources;
using System.Reflection;

namespace Ecm.CaptureModel
{
    [Serializable]
    public class UserModel : BaseDependencyProperty, IDataErrorInfo
    {
        private Guid _id;
        private string _username;
        private string _password;
        private string _fullname;
        private string _emailAddress;
        private bool _isAdmin;
        private UserTypeModel _type;
        private ObservableCollection<UserGroupModel> _userGroups = new ObservableCollection<UserGroupModel>();
        private bool _isSelected;
        private string _description;
        private Guid? _languageId;
        private LanguageModel _language;
        private byte[] _picture;
        private bool _applyForArchive;
        private ResourceManager _resource = new ResourceManager("Ecm.CaptureModel.Resources", Assembly.GetExecutingAssembly());

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

        public string EncryptedPassword { get; set; }

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

        public UserTypeModel Type
        {
            get { return _type; }
            set
            {
                _type = value;
                OnPropertyChanged("Type");
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

        public Guid? LanguageId
        {
            get { return _languageId; }
            set
            {
                _languageId = value;
                OnPropertyChanged("LanguageId");
            }
        }

        public LanguageModel Language
        {
            get { return _language; }
            set
            {
                _language = value;
                OnPropertyChanged("Language");
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

        public bool ApplyForArchive
        {
            get { return _applyForArchive; }
            set
            {
                _applyForArchive = value;
                OnPropertyChanged("ApplyForArchive");
            }
        }

        public bool HasError { get; set; }

        #region IDataErrorInfo Members

        public string Error
        {
            get { return null; }
        }

        public string this[string columnName]
        {
            get
            {
                string msg = string.Empty;
                if (columnName == "EmailAddress")
                {
                    if (EmailAddress == null)
                    {
                        //HasError &= false;
                        return null;
                    }

                    //HasError &= false;

                    if (!CommonValidator.IsEmail(EmailAddress))
                    {
                        //HasError &= true;
                        msg = _resource.GetString("uiEmailInvalid");
                    }
                }

                if (columnName == "Username")
                {
                    if (ErrorChecked != null)
                    {
                        ErrorChecked(this);
                    }

                    if (HasError)
                    {
                        msg = string.IsNullOrWhiteSpace(Username) ? _resource.GetString("uiUserNameEmpty") : _resource.GetString("uiUsernameExisted");
                    }
                }

                HasError |= !string.IsNullOrEmpty(msg);
                return msg;
            }
        }

        #endregion
    }
}
