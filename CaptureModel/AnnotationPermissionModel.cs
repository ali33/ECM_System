using Ecm.Mvvm;
using System;

namespace Ecm.CaptureModel
{
    public class AnnotationPermissionModel : BaseDependencyProperty
    {
        private Guid _id;
        private Guid _docTypeId;
        private Guid _userGroupId;
        private bool _canSeeText;
        private bool _canAddText;
        private bool _canDeleteText;
        private bool _canSeeHighlight;
        private bool _canAddHighlight;
        private bool _canDeleteHighlight;
        private bool _canHideRedaction;
        private bool _canAddRedaction;
        private bool _canDeleteRedaction;


        public Guid Id
        {
            get { return _id; }
            set
            {
                _id = value;
                OnPropertyChanged("Id");
            }
        }

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

        public bool CanSeeText
        {
            get { return _canSeeText; }
            set
            {
                _canSeeText = value;
                OnPropertyChanged("CanSeeText");
            }
        }

        public bool CanAddText
        {
            get { return _canAddText; }
            set
            {
                _canAddText = value;
                OnPropertyChanged("CanAddText");
            }
        }

        public bool CanDeleteText
        {
            get { return _canDeleteText; }
            set
            {
                _canDeleteText = value;
                OnPropertyChanged("CanDeleteText");
            }
        }

        public bool CanSeeHighlight
        {
            get { return _canSeeHighlight; }
            set
            {
                _canSeeHighlight = value;
                OnPropertyChanged("CanSeeHighlight");
            }
        }

        public bool CanAddHighlight
        {
            get { return _canAddHighlight; }
            set
            {
                _canAddHighlight = value;
                OnPropertyChanged("CanAddHighlight");
            }
        }

        public bool CanDeleteHighlight
        {
            get { return _canDeleteHighlight; }
            set
            {
                _canDeleteHighlight = value;
                OnPropertyChanged("CanDeleteHighlight");
            }
        }

        public bool CanHideRedaction
        {
            get { return _canHideRedaction; }
            set
            {
                _canHideRedaction = value;
                OnPropertyChanged("CanHideRedaction");
            }
        }

        public bool CanAddRedaction
        {
            get { return _canAddRedaction; }
            set
            {
                _canAddRedaction = value;
                OnPropertyChanged("CanAddRedaction");
            }
        }

        public bool CanDeleteRedaction
        {
            get { return _canDeleteRedaction; }
            set
            {
                _canDeleteRedaction = value;
                OnPropertyChanged("CanDeleteRedaction");
            }
        }

        public static AnnotationPermissionModel GetAllowAll()
        {
            return new AnnotationPermissionModel
                       {
                           CanAddHighlight = true,
                           CanAddRedaction = true,
                           CanAddText = true,
                           CanDeleteHighlight = true,
                           CanDeleteRedaction = true,
                           CanDeleteText = true,
                           CanHideRedaction = true,
                           CanSeeHighlight = true,
                           CanSeeText = true
                       };
        }
    }
}
