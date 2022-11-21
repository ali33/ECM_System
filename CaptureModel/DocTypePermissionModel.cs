using System;
using Ecm.Mvvm;
using System.Collections.ObjectModel;
using System.Collections.Generic;

namespace Ecm.CaptureModel
{
    [Serializable]
    public class DocTypePermissionModel : BaseDependencyProperty
    {
        private Guid _docTypeId;
        private Guid _userGroupId;
        private bool _canAccess;

        public DocTypePermissionModel()
        {
        }

        public Guid Id { get; set; }

        public Guid DocTypeId
        {
            get { return _docTypeId; }
            set
            {
                _docTypeId = value;
                OnPropertyChanged("DocTypeId");
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

        public bool CanAccess
        {
            get { return _canAccess; }
            set
            {
                _canAccess = value;
                OnPropertyChanged("CanAccess");
            }
        }


            
        public static DocTypePermissionModel GetAllowAll()
        {
            return new DocTypePermissionModel
                       {
                           CanAccess = true
                       };
        }
    }
}
