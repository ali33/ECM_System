using Ecm.Mvvm;
using System;

namespace Ecm.Model
{
    public class MemberShipModel : BaseDependencyProperty
    {
        private Guid _id;
        private Guid _userGroupId;
        private Guid _userId;

        public Guid Id
        {
            get { return _id; }
            set
            {
                _id = value;
                OnPropertyChanged("Id");
            }
        }

        public Guid UserGroupId
        {
            get { return _userGroupId; }
            set
            {
                _userGroupId = value;
                OnPropertyChanged("UserGroupId");
            }
        }

        public Guid UserId
        {
            get { return _userId; }
            set
            {
                _userId = value;
                OnPropertyChanged("UserId");
            }
        }

    }
}
