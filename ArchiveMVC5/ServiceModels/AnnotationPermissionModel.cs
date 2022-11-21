
using System;

namespace ArchiveMVC5.Models
{
    public class AnnotationPermissionModel
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
            }
        }

        public Guid DocTypeId
        {
            get { return _docTypeId; }
            set
            {
                _docTypeId = value;
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

        public bool AllowedSeeText
        {
            get { return _allowedSeeText; }
            set
            {
                _allowedSeeText = value;
            }
        }

        public bool AllowedAddText
        {
            get { return _allowedAddText; }
            set
            {
                _allowedAddText = value;
            }
        }

        public bool AllowedDeleteText
        {
            get { return _allowedDeleteText; }
            set
            {
                _allowedDeleteText = value;
            }
        }

        public bool AllowedSeeHighlight
        {
            get { return _allowedSeeHighlight; }
            set
            {
                _allowedSeeHighlight = value;
            }
        }

        public bool AllowedAddHighlight
        {
            get { return _allowedAddHighlight; }
            set
            {
                _allowedAddHighlight = value;
            }
        }

        public bool AllowedDeleteHighlight
        {
            get { return _allowedDeleteHighlight; }
            set
            {
                _allowedDeleteHighlight = value;
            }
        }

        public bool AllowedHideRedaction
        {
            get { return _allowedHideRedaction; }
            set
            {
                _allowedHideRedaction = value;
            }
        }

        public bool AllowedAddRedaction
        {
            get { return _allowedAddRedaction; }
            set
            {
                _allowedAddRedaction = value;
            }
        }

        public bool AllowedDeleteRedaction
        {
            get { return _allowedDeleteRedaction; }
            set
            {
                _allowedDeleteRedaction = value;
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
