using System.Linq;
using Ecm.DocViewer.Model;
using Ecm.Model;

namespace Ecm.DocViewer.Helper
{
    public class PermissionManager
    {
        public PermissionManager(ViewerContainer viewerContainer)
        {
            ViewerContainer = viewerContainer;
        }

        public ViewerContainer ViewerContainer { get; private set; }

        public ContentViewerPermission GetViewerPermission(ContentItem item)
        {
            if (ViewerContainer.DocViewerMode == DocViewerMode.LightCapture || 
                ViewerContainer.DocViewerMode == DocViewerMode.Document)
            {
                DocumentModel document = GetDocument(item);
                if (document != null)
                {
                    return new ContentViewerPermission
                    {
                        CanAddHighlight = document.DocumentType.AnnotationPermission.AllowedAddHighlight,
                        CanAddLine = false,
                        CanAddRedaction = document.DocumentType.AnnotationPermission.AllowedAddRedaction,
                        CanAddText = document.DocumentType.AnnotationPermission.AllowedAddText,
                        CanDeleteHighlight = document.DocumentType.AnnotationPermission.AllowedDeleteHighlight,
                        CanDeleteLine = false,
                        CanDeleteRedaction = document.DocumentType.AnnotationPermission.AllowedDeleteRedaction,
                        CanDeleteText = document.DocumentType.AnnotationPermission.AllowedDeleteText,
                        CanEmail = document.DocumentType.DocumentTypePermission.AllowedEmailDocument,
                        CanHideAnnotation = document.DocumentType.DocumentTypePermission.AllowedHideAllAnnotation,
                        CanPrint = document.DocumentType.DocumentTypePermission.AlowedPrintDocument,
                        CanSeeHighlight = document.DocumentType.AnnotationPermission.AllowedSeeHighlight,
                        CanSeeLine = false,
                        CanSeeText = document.DocumentType.AnnotationPermission.AllowedSeeText
                    };
                }

                return new ContentViewerPermission();
            }
            
            if (ViewerContainer.DocViewerMode == DocViewerMode.OCRTemplate)
            {
                return new ContentViewerPermission
                {
                    CanApplyOCRTemplate = true,
                    CanAddHighlight = false,
                    //CanAddLine = false,
                    CanAddRedaction = false,
                    CanAddText = false,
                    CanDeleteHighlight = false,
                    //CanDeleteLine = false,
                    CanDeleteRedaction = false,
                    CanDeleteText = false,
                    CanEmail = false,
                    CanHideAnnotation = false,
                    CanPrint = false,
                    CanSeeHighlight = false,
                    //CanSeeLine = false,
                    CanSeeText = false
                };
            }

            // TODO: Provide permission for WorkItem (workflow) mode
            if (ViewerContainer.DocViewerMode == DocViewerMode.WorkItem)
            {
                DocumentModel document = GetDocument(item);
                if (document != null)
                {
                    return new ContentViewerPermission
                    {
                        CanAddHighlight = document.DocumentType.AnnotationPermission.AllowedAddHighlight,
                        CanAddLine = false,
                        CanAddRedaction = document.DocumentType.AnnotationPermission.AllowedAddRedaction,
                        CanAddText = document.DocumentType.AnnotationPermission.AllowedAddText,
                        CanDeleteHighlight = document.DocumentType.AnnotationPermission.AllowedDeleteHighlight,
                        CanDeleteLine = false,
                        CanDeleteRedaction = document.DocumentType.AnnotationPermission.AllowedDeleteRedaction,
                        CanDeleteText = document.DocumentType.AnnotationPermission.AllowedDeleteText,
                        CanEmail = document.DocumentType.DocumentTypePermission.AllowedEmailDocument,
                        CanHideAnnotation = document.DocumentType.DocumentTypePermission.AllowedHideAllAnnotation,
                        CanPrint = document.DocumentType.DocumentTypePermission.AlowedPrintDocument,
                        CanSeeHighlight = document.DocumentType.AnnotationPermission.AllowedSeeHighlight,
                        CanSeeLine = false,
                        CanSeeText = document.DocumentType.AnnotationPermission.AllowedSeeText
                    };
                }

                return new ContentViewerPermission();
            }

            // TODO: Provide permission for Capture mode
            return new ContentViewerPermission();
        }

        public bool CanCapture()
        {
            switch (ViewerContainer.DocViewerMode)
            {
                case DocViewerMode.LightCapture:
                    return true;
                case DocViewerMode.Document:
                    return false;
                case DocViewerMode.WorkItem:
                    return false;
            }

            // TODO: Provide permission for Capture mode
            return true;
        }

        public bool CanChangeDocumentType()
        {
            switch (ViewerContainer.DocViewerMode)
            {
                case DocViewerMode.LightCapture:
                    return true;
                case DocViewerMode.Document:
                    DocumentModel document = GetDocument(null);
                    if (document != null)
                    {
                        return document.DocumentType.DocumentTypePermission.AllowedChangeDocumentType;
                    }

                    return false;
                case DocViewerMode.WorkItem:
                    return false;
            }

            // TODO: Provide permission for Capture mode
            return true;
        }

        public bool CanCombineDocument()
        {
            switch (ViewerContainer.DocViewerMode)
            {
                case DocViewerMode.LightCapture:
                case DocViewerMode.Document:
                    return false;
                case DocViewerMode.WorkItem:
                    return false;
            }

            // TODO: Provide permission for Capture mode
            return true;
        }

        public bool CanCreateDocument()
        {
            switch (ViewerContainer.DocViewerMode)
            {
                case DocViewerMode.LightCapture:
                    return true;
                case DocViewerMode.Document:
                    return false;
                case DocViewerMode.WorkItem:
                    return false;
            }

            // TODO: Provide permission for Capture mode
            return true;
        }

        public bool CanDelete()
        {
            switch (ViewerContainer.DocViewerMode)
            {
                case DocViewerMode.LightCapture:
                    return true;
                case DocViewerMode.Document:
                case DocViewerMode.WorkItem:
                    DocumentModel document = GetDocument(null);
                    if (document != null)
                    {
                        return document.DocumentType.DocumentTypePermission.AllowedDeletePage;
                    }

                    return false;
            }

            // TODO: Provide permission for Capture mode
            return true;
        }

        public bool CanInsert()
        {
            switch (ViewerContainer.DocViewerMode)
            {
                case DocViewerMode.LightCapture:
                    return true;
                case DocViewerMode.Document:
                case DocViewerMode.WorkItem:
                    DocumentModel document = GetDocument(null);
                    if (document != null)
                    {
                        return document.DocumentType.DocumentTypePermission.AllowedAppendPage;
                    }

                    return false;
            }

            // TODO: Provide permission for Capture mode
            return true;
        }

        public bool CanModifyIndex()
        {
            switch (ViewerContainer.DocViewerMode)
            {
                case DocViewerMode.LightCapture:
                    return true;
                case DocViewerMode.Document:
                case DocViewerMode.WorkItem:
                    DocumentModel document = GetDocument(null);
                    if (document == null && ViewerContainer.ThumbnailSelector.Cursor != null &&
                        ViewerContainer.ThumbnailSelector.Cursor.ItemType == ContentItemType.Batch &&
                        ViewerContainer.ThumbnailSelector.Cursor.Children.Count(p => p.ItemType == ContentItemType.Document) > 0)
                    {
                        document = ViewerContainer.ThumbnailSelector.Cursor.Children.First(p => p.ItemType == ContentItemType.Document).DocumentData;
                    }

                    if (document != null)
                    {
                        return document.DocumentType.DocumentTypePermission.AllowedUpdateFieldValue;
                    }

                    return false;
            }

            // TODO: Provide permission for Capture mode
            return true;
        }

        public bool CanSeeRetrictedField()
        {
            switch (ViewerContainer.DocViewerMode)
            {
                case DocViewerMode.LightCapture:
                    return true;
                case DocViewerMode.Document:
                case DocViewerMode.WorkItem:
                    DocumentModel document = GetDocument(null);
                    if (document == null && ViewerContainer.ThumbnailSelector.Cursor != null &&
                        ViewerContainer.ThumbnailSelector.Cursor.ItemType == ContentItemType.Batch &&
                        ViewerContainer.ThumbnailSelector.Cursor.Children.Count(p => p.ItemType == ContentItemType.Document) > 0)
                    {
                        document = ViewerContainer.ThumbnailSelector.Cursor.Children.First(p => p.ItemType == ContentItemType.Document).DocumentData;
                    }

                    if (document != null)
                    {
                        return document.DocumentType.DocumentTypePermission.AllowedSeeRetrictedField;
                    }

                    return false;
            }

            // TODO: Provide permission for Capture mode
            return true;
        }

        public bool CanReject()
        {
            switch (ViewerContainer.DocViewerMode)
            {
                case DocViewerMode.LightCapture:
                case DocViewerMode.Document:
                    return false;
            }
            // TODO: Provide permission for Capture mode
            return true;
        }

        public bool CanReleaseWithLoosePage()
        {
            switch (ViewerContainer.DocViewerMode)
            {
                case DocViewerMode.LightCapture:
                case DocViewerMode.Document:
                    return false;
                case DocViewerMode.WorkItem:
                    return false;
            }
            // TODO: Provide permission for Capture mode
            return true;
        }

        public bool CanReOrderPage()
        {
            switch (ViewerContainer.DocViewerMode)
            {
                case DocViewerMode.WorkItem:
                    DocumentModel document = GetDocument(null);
                    if (document != null)
                    {
                        return document.DocumentType.DocumentTypePermission.AllowedReOrderPage;
                    }

                    return false;
            }

            // TODO: Provide permission for Capture mode
            return true;
        }

        public bool CanReplace()
        {
            switch (ViewerContainer.DocViewerMode)
            {
                case DocViewerMode.LightCapture:
                    return true;
                case DocViewerMode.Document:
                case DocViewerMode.WorkItem:
                    DocumentModel document = GetDocument(null);
                    if (document != null)
                    {
                        return document.DocumentType.DocumentTypePermission.AllowedReplacePage;
                    }

                    return false;
            }

            // TODO: Provide permission for Capture mode
            return true;
        }

        public bool CanRotate()
        {
            switch (ViewerContainer.DocViewerMode)
            {
                case DocViewerMode.LightCapture:
                    return true;
                case DocViewerMode.Document:
                case DocViewerMode.WorkItem:
                    DocumentModel document = GetDocument(null);
                    if (document != null)
                    {
                        return document.DocumentType.DocumentTypePermission.AllowedRotatePage;
                    }

                    return false;
                case DocViewerMode.OCRTemplate:
                    return false;
            }

            // TODO: Provide permission for Capture mode
            return true;
        }

        public bool CanSendLink()
        {
            switch (ViewerContainer.DocViewerMode)
            {
                case DocViewerMode.LightCapture:
                case DocViewerMode.Document:
                    return false;
                case DocViewerMode.WorkItem:
                    DocumentModel document = GetDocument(null);
                    if (document != null)
                    {
                        return document.DocumentType.DocumentTypePermission.AllowedEmailDocument;
                    }
                    return false;
            }
            // TODO: Provide permission for Capture mode
            return true;
        }

        public bool CanSplitDocument()
        {
            switch (ViewerContainer.DocViewerMode)
            {
                case DocViewerMode.LightCapture:
                    return true;
                case DocViewerMode.Document:
                    DocumentModel document = GetDocument(null);
                    if (document != null)
                    {
                        return document.DocumentType.DocumentTypePermission.AllowedSplitDocument;
                    }

                    return false;
                case DocViewerMode.WorkItem:
                    return false;
            }

            // TODO: Provide permission for Capture mode
            return true;
        }

        public bool CanUnReject()
        {
            switch (ViewerContainer.DocViewerMode)
            {
                case DocViewerMode.LightCapture:
                case DocViewerMode.Document:
                    return false;
                case DocViewerMode.WorkItem:
                    return false;
            }
            // TODO: Provide permission for Capture mode
            return true;
        }

        private DocumentModel GetDocument(ContentItem item)
        {
            var tempItem = item ?? ViewerContainer.ThumbnailSelector.Cursor;

            if (tempItem != null)
            {
                if (tempItem.ItemType == ContentItemType.Page &&
                    tempItem.Parent.ItemType == ContentItemType.Document)
                {
                    return tempItem.Parent.DocumentData;
                }

                if (tempItem.ItemType == ContentItemType.Document)
                {
                    return tempItem.DocumentData;
                }

            }

            return null;
        }
    }
}