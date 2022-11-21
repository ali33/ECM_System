using System;
using Ecm.Mvvm;

using System.ComponentModel;
using System.Collections.Generic;

namespace ArchiveMVC.Models
{
    [Serializable]
    public class UserGroupModel
    {
        private Guid _id;
        private string _name;
        private UserGroupTypeModel _type;
        private bool _isSelected;
        private string _description;
        private List<UserModel> _users = new List<UserModel>();

        public UserGroupModel()
        {
            Users = new List<UserModel>();
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
           }
        }

        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
            }
        }

        public UserGroupTypeModel Type
        {
            get { return _type; }
            set
            {
                _type = value;
            }
        }

        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                _isSelected = value;
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

        public List<UserModel> Users
        {
            get { return _users; }
            set
            {
                _users = value;
            }
        }

        public bool HasError { get; set; }


    }
}
