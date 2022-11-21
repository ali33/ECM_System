using System;
using Ecm.Mvvm;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Resources;
using System.Reflection;

namespace Ecm.CaptureModel
{
    [Serializable]
    public class UserGroupModel : BaseDependencyProperty, IDataErrorInfo
    {
        private Guid _id;
        private string _name;
        private UserGroupTypeModel _type;
        private bool _isSelected;
        private string _description;
        private ObservableCollection<UserModel> _users = new ObservableCollection<UserModel>();
        private ResourceManager _resource = new ResourceManager("Ecm.CaptureModel.Resources", Assembly.GetExecutingAssembly());

        public UserGroupModel()
        {
            Users = new ObservableCollection<UserModel>();
        }

        public UserGroupModel(Action<UserGroupModel> action)
        {
            ErrorChecked = action;
        }

        public Action<UserGroupModel> ErrorChecked { get; set; }

        public Guid Id
        {
            get { return _id; }
            set
            {
                _id = value;
                OnPropertyChanged("Id");
            }
        }

        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
                OnPropertyChanged("Name");
            }
        }

        public UserGroupTypeModel Type
        {
            get { return _type; }
            set
            {
                _type = value;
                OnPropertyChanged("Type");
            }
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

        public string Description
        {
            get { return _description; }
            set
            {
                _description = value;
                OnPropertyChanged("Description");
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

        public bool HasError { get; set; }

        public string Error
        {
            get { return null; }
        }

        public string this[string columnName]
        {
            get
            {
                string errorMess = string.Empty;

                if (columnName == "Name")
                {
                    if (ErrorChecked != null)
                    {
                        ErrorChecked(this);
                    }

                    if (HasError)
                    {
                        errorMess = string.IsNullOrWhiteSpace(Name) ? _resource.GetString("uiUserGroupNameEmpty") : _resource.GetString("uiUserGroupExisted");
                    }
                }

                return errorMess;
            }
        }
    }
}
