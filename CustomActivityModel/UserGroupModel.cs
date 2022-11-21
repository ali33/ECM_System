using System;
using Ecm.Mvvm;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Resources;
using System.Reflection;

namespace Ecm.Workflow.Activities.CustomActivityModel
{
    [Serializable]
    public class UserGroupModel : BaseDependencyProperty
    {
        private Guid _id;
        private string _name;
        private bool _isSelected;
        private string _description;
        private ObservableCollection<UserModel> _users = new ObservableCollection<UserModel>();

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
    }
}
