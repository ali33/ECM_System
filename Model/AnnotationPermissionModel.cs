using Ecm.Mvvm;
using System;

namespace Ecm.Model
{
    public class AnnotationPermissionModel : BaseDependencyProperty
    {
        private Guid _id;
        private Guid _docTypeId;
        private Guid _userGroupId;
        private bool _allowedSeeText;
        private bool _allowedAddText;
        private bool _allowedDeleteText;
        private bool _allowedSeeHighlight;
        private bool _allowedAddHighlight;
        private bool _allowedDeleteHighlight;
        private bool _allowedHideRedaction;
        private bool _allowedAddRedaction;
        private bool _allowedDeleteRedaction;


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

        public bool AllowedSeeText
        {
            get { return _allowedSeeText; }
            set
            {
                _allowedSeeText = value;
                OnPropertyChanged("AllowedSeeText");
            }
        }

        public bool AllowedAddText
        {
            get { return _allowedAddText; }
            set
            {
                _allowedAddText = value;
                OnPropertyChanged("AllowedAddText");
            }
        }

        public bool AllowedDeleteText
        {
            get { return _allowedDeleteText; }
            set
            {
                _allowedDeleteText = value;
                OnPropertyChanged("AllowedDeleteText");
            }
        }

        public bool AllowedSeeHighlight
        {
            get { return _allowedSeeHighlight; }
            set
            {
                _allowedSeeHighlight = value;
                OnPropertyChanged("AllowedSeeHighlight");
            }
        }

        public bool AllowedAddHighlight
        {
            get { return _allowedAddHighlight; }
            set
            {
                _allowedAddHighlight = value;
                OnPropertyChanged("AllowedAddHighlight");
            }
        }

        public bool AllowedDeleteHighlight
        {
            get { return _allowedDeleteHighlight; }
            set
            {
                _allowedDeleteHighlight = value;
                OnPropertyChanged("AllowedDeleteHighlight");
            }
        }

        public bool AllowedHideRedaction
        {
            get { return _allowedHideRedaction; }
            set
            {
                _allowedHideRedaction = value;
                OnPropertyChanged("AllowedHideRedaction");
            }
        }

        public bool AllowedAddRedaction
        {
            get { return _allowedAddRedaction; }
            set
            {
                _allowedAddRedaction = value;
                OnPropertyChanged("AllowedAddRedaction");
            }
        }

        public bool AllowedDeleteRedaction
        {
            get { return _allowedDeleteRedaction; }
            set
            {
                _allowedDeleteRedaction = value;
                OnPropertyChanged("AllowedDeleteRedaction");
            }
        }

        public static AnnotationPermissionModel GetAllowAll()
        {
            return new AnnotationPermissionModel
                       {
                           AllowedAddHighlight = true,
                           AllowedAddRedaction = true,
                           AllowedAddText = true,
                           AllowedDeleteHighlight = true,
                           AllowedDeleteRedaction = true,
                           AllowedDeleteText = true,
                           AllowedHideRedaction = true,
                           AllowedSeeHighlight = true,
                           AllowedSeeText = true
                       };
        }
    }
}
