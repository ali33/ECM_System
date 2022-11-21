using System;

using Ecm.Mvvm;

namespace ArchiveMVC.Models
{
    public class ActionLogModel
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
            }
        }

        public DateTime LoggedDate
        {
            get { return _loggedDate; }
            set
            {
                _loggedDate = value;
            }
        }

        public string ActionName
        {
            get { return _actionName; }
            set
            {
                _actionName = value;
            }
        }

        public string IpAddress
        {
            get { return _ipAddress; }
            set
            {
                _ipAddress = value;
            }
        }

        public string Message
        {
            get { return _message; }
            set
            {
                _message = value;
            }
        }

        public string ObjectType
        {
            get { return _objectType; }
            set
            {
                _objectType = value;
            }
        }

        public Guid? ObjectId
        {
            get { return _objectId; }
            set
            {
                _objectId = value;
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

        public UserModel User
        {
            get { return _user; }
            set
            {
                _user = value;
            }
        }
    }
}
