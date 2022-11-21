using Ecm.Mvvm;
using System;

namespace ArchiveMVC.Models
{
    public class MemberShipModel
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
            }
        }

        public Guid UserGroupId
        {
            get { return _userGroupId; }
            set
            {
                _userGroupId = value;
            }
        }

        public Guid UserId
        {
            get { return _userId; }
            set
            {
                _userId = value;
            }
        }

    }
}
