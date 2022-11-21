using System;
using Ecm.Mvvm;

namespace ArchiveMVC.Models
{
    [Serializable]
    public class DocumentTypePermissionModel
    {
        private Guid _id;
        private Guid _docTypeId;
        private Guid _userGroupId;
        private bool _allowedDeletePage;
        private bool _allowedAppendPage;
        private bool _allowedReplacePage;
        private bool _allowedSeeRetrictedField;
        private bool _allowedUpdateFieldValue;
        private bool _alowedPrintDocument;
        private bool _allowedEmailDocument;
        private bool _allowedRotatePage;
        private bool _allowedExportFieldValue;
        private bool _allowedDownloadOffline;
        private bool _allowedHideAllAnnotation;
        private bool _allowedCapture;
        private bool _allowedSearch;
        private bool _allowedChangeDocumentType;
        private bool _allowedReOrderPage;
        private bool _allowedSplitDocument;

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

        public bool AllowedDeletePage
        {
            get { return _allowedDeletePage; }
            set
            {
                _allowedDeletePage = value;
            }
        }

        public bool AllowedAppendPage
        {
            get { return _allowedAppendPage; }
            set
            {
                _allowedAppendPage = value;
            }
        }

        public bool AllowedReplacePage
        {
            get { return _allowedReplacePage; }
            set
            {
                _allowedReplacePage = value;
            }
        }

        public bool AllowedSeeRetrictedField
        {
            get { return _allowedSeeRetrictedField; }
            set
            {
                _allowedSeeRetrictedField = value;
            }
        }

        public bool AllowedUpdateFieldValue
        {
            get { return _allowedUpdateFieldValue; }
            set
            {
                _allowedUpdateFieldValue = value;
            }
        }

        public bool AlowedPrintDocument
        {
            get { return _alowedPrintDocument; }
            set
            {
                _alowedPrintDocument = value;
            }
        }

        public bool AllowedEmailDocument
        {
            get { return _allowedEmailDocument; }
            set
            {
                _allowedEmailDocument = value;
            }
        }

        public bool AllowedRotatePage
        {
            get { return _allowedRotatePage; }
            set
            {
                _allowedRotatePage = value;
            }
        }

        public bool AllowedExportFieldValue
        {
            get { return _allowedExportFieldValue; }
            set
            {
                _allowedExportFieldValue = value;
            }
        }

        public bool AllowedDownloadOffline
        {
            get { return _allowedDownloadOffline; }
            set
            {
                _allowedDownloadOffline = value;
            }
        }

        public bool AllowedHideAllAnnotation
        {
            get { return _allowedHideAllAnnotation; }
            set
            {
                _allowedHideAllAnnotation = value;
            }
        }

        public bool AllowedCapture
        {
            get { return _allowedCapture; }
            set
            {
                _allowedCapture = value;
            }
        }

        public bool AllowedSearch
        {
            get { return _allowedSearch; }
            set
            {
                _allowedSearch = value;
            }
        }

        public bool AllowedChangeDocumentType
        {
            get { return _allowedChangeDocumentType; }
            set
            {
                _allowedChangeDocumentType = value;
            }
        }

        public bool AllowedReOrderPage
        {
            get { return _allowedReOrderPage; }
            set
            {
                _allowedReOrderPage = value;
            }
        }

        public bool AllowedSplitDocument
        {
            get { return _allowedSplitDocument; }
            set
            {
                _allowedSplitDocument = value;
            }
        }

        public static DocumentTypePermissionModel GetAllowAll()
        {
            return new DocumentTypePermissionModel
                       {
                           AllowedAppendPage = true,
                           AllowedCapture = true,
                           AllowedChangeDocumentType = true,
                           AlowedPrintDocument = true,
                           AllowedUpdateFieldValue = true,
                           AllowedSplitDocument = true,
                           AllowedSeeRetrictedField = true,
                           AllowedSearch = true,
                           AllowedRotatePage = true,
                           AllowedReplacePage = true,
                           AllowedReOrderPage = true,
                           AllowedHideAllAnnotation = true,
                           AllowedExportFieldValue = true,
                           AllowedEmailDocument = true,
                           AllowedDownloadOffline = true,
                           AllowedDeletePage = true
                       };
        }
    }
}
