using System;

using Ecm.Mvvm;

namespace Ecm.Model
{
    public class ActionLogModel : BaseDependencyProperty
    {
        private string _username;
        private DateTime _loggedDate;
        private string _actionName;
        private string _ipAddress;
        private string _message;
        private string _objectType;
        private Guid? _objectId;
        private bool _isSelected;
        private UserModel _user;

        public Guid Id { get; set; }

        public string Username 
        {
            get { return _username; }
            set
            {
                _username = value;
                OnPropertyChanged("Username");
            }
        }

        public DateTime LoggedDate
        {
            get { return _loggedDate; }
            set
            {
                _loggedDate = value;
                OnPropertyChanged("LoggedDate");
            }
        }

        public string ActionName
        {
            get { return _actionName; }
            set
            {
                _actionName = value;
                OnPropertyChanged("ActionName");
            }
        }

        public string IpAddress
        {
            get { return _ipAddress; }
            set
            {
                _ipAddress = value;
                OnPropertyChanged("IpAddress");
            }
        }

        public string Message
        {
            get { return _message; }
            set
            {
                _message = value;
                OnPropertyChanged("Message");
            }
        }

        public string ObjectType
        {
            get { return _objectType; }
            set
            {
                _objectType = value;
                OnPropertyChanged("ObjectType");
            }
        }

        public Guid? ObjectId
        {
            get { return _objectId; }
            set
            {
                _objectId = value;
                OnPropertyChanged("ObjectId");
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

        public UserModel User
        {
            get { return _user; }
            set
            {
                _user = value;
                OnPropertyChanged("User");
            }
        }

        public ActionNameModel ActionNameEnum { get; set; }
    }
}
