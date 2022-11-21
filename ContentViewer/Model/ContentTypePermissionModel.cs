using System;
using Ecm.Mvvm;
using System.Collections.ObjectModel;
using System.Collections.Generic;

namespace Ecm.ContentViewer.Model
{
    [Serializable]
    public class ContentTypePermissionModel : BaseDependencyProperty
    {
        private Guid _docTypeId;
        private Guid _userGroupId;
        private bool _canAccess;
        private bool _canSeeRestrictedField;

        public ContentTypePermissionModel()
        {
            FieldPermissions = new List<ContentFieldPermissionModel>();
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

        public bool CanSeeRestrictedField
        {
            get { return _canSeeRestrictedField; }
            set
            {
                _canSeeRestrictedField = value;
                OnPropertyChanged("CanSeeRestrictedField");
            }
        }

        public List<ContentFieldPermissionModel> FieldPermissions { get; set; }
            
        public static ContentTypePermissionModel GetAllowAll()
        {
            return new ContentTypePermissionModel
                       {
                           CanAccess = true,
                           CanSeeRestrictedField = true
                       };
        }
    }
}
