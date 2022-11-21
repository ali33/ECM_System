using System;
using System.Collections.Generic;

namespace CaptureMVC.Models
{
    [Serializable]
    public class DocumentTypePermissionModel
    {
        // Properties
        public Guid Id { get; set; }

        public Guid DocTypeId { get; set; }

        public Guid UserGroupId { get; set; }

        public bool CanDeletePage { get; set; }

        public bool CanAppendPage { get; set; }

        public bool CanReplacePage { get; set; }

        public bool CanSeeRetrictedField { get; set; }

        public bool CanUpdateFieldValue { get; set; }

        public bool CanPrintDocument { get; set; }

        public bool CanEmailDocument { get; set; }

        public bool CanRotatePage { get; set; }

        public bool CanExportFieldValue { get; set; }

        public bool CanDownloadOffline { get; set; }

        public bool CanHideAllAnnotation { get; set; }

        public bool CanCapture { get; set; }

        public bool CanSearch { get; set; }

        public bool CanChangeDocumentType { get; set; }

        public bool CanReOrderPage { get; set; }

        public bool CanSplitDocument { get; set; }

        // Methods
        public static DocumentTypePermissionModel GetAll()
        {
            return new DocumentTypePermissionModel
            {
                CanAppendPage = true,
                CanCapture = true,
                CanChangeDocumentType = true,
                CanPrintDocument = true,
                CanUpdateFieldValue = true,
                CanSplitDocument = true,
                CanSeeRetrictedField = true,
                CanSearch = true,
                CanRotatePage = true,
                CanReplacePage = true,
                CanReOrderPage = true,
                CanHideAllAnnotation = true,
                CanExportFieldValue = true,
                CanEmailDocument = true,
                CanDownloadOffline = true,
                CanDeletePage = true
            };
        }
    }
}