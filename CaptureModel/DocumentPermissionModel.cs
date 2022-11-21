using System;
using Ecm.Mvvm;
using System.Collections.ObjectModel;
using System.Collections.Generic;

namespace Ecm.CaptureModel
{
    [Serializable]
    public class DocumentPermissionModel : BaseDependencyProperty
    {
        private Guid _docTypeId;
        private Guid _userGroupId;
        private bool _canSeeRestrictedField;

        public DocumentPermissionModel()
        {
            FieldPermissions = new List<DocumentFieldPermissionModel>();
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

        public bool CanSeeRestrictedField
        {
            get { return _canSeeRestrictedField; }
            set
            {
                _canSeeRestrictedField = value;
                OnPropertyChanged("CanSeeRestrictedField");
            }
        }

        public List<DocumentFieldPermissionModel> FieldPermissions { get; set; }



        public static DocumentPermissionModel GetAllowAll()
        {
            return new DocumentPermissionModel
                       {
                           CanSeeRestrictedField = true
                       };
        }
    }
}
