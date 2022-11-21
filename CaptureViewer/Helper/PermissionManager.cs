using System.Linq;
using Ecm.CaptureViewer.Model;
using Ecm.CaptureModel;
using System;

namespace Ecm.CaptureViewer.Helper
{
    public class PermissionManager
    {
        public PermissionManager(ViewerContainer viewerContainer)
        {
            ViewerContainer = viewerContainer;
        }

        public ViewerContainer ViewerContainer { get; private set; }

        public ContentViewerPermission GetContentViewerPermission(ContentItem item)
        {
            // TODO: Provide permission for WorkItem (workflow) mode
            try
            {
                if (ViewerContainer.DocViewerMode == DocViewerMode.Capture)
                {
                    BatchModel batch = GetBatch(item);
                    if (batch != null)
                    {
                        return new ContentViewerPermission
                        {
                            CanAddHighlight = batch.Permission.CanAnnotate,
                            //CanAddLine = false,
                            CanAddRedaction = batch.Permission.CanAnnotate,
                            CanAddText = batch.Permission.CanAnnotate,
                            CanDeleteHighlight = batch.Permission.CanAnnotate,
                            //CanDeleteLine = false,
                            CanDeleteRedaction = batch.Permission.CanAnnotate,
                            CanDeleteText = batch.Permission.CanAnnotate,
                            CanEmail = batch.Permission.CanEmail,
                            CanHideAnnotation = batch.Permission.CanAnnotate,
                            CanPrint = batch.Permission.CanPrint,
                            CanSeeHighlight = batch.Permission.CanAnnotate,
                            //CanSeeLine = false,
                            CanSeeText = batch.Permission.CanAnnotate
                        };
                    }

                    return new ContentViewerPermission();
                }
                else if (ViewerContainer.DocViewerMode == DocViewerMode.WorkItem)
                {
                    DocumentModel document = GetDocument(item);

                    if (document != null)
                    {
                        var batch = GetBatch(item);

                        return new ContentViewerPermission
                        {
                            CanAddHighlight = batch.Permission.CanAnnotate & document.AnnotationPermission.CanAddHighlight,
                            //CanAddLine = false,
                            CanAddRedaction = batch.Permission.CanAnnotate & document.AnnotationPermission.CanAddRedaction,
                            CanAddText = batch.Permission.CanAnnotate & document.AnnotationPermission.CanAddText,
                            CanDeleteHighlight = batch.Permission.CanAnnotate & document.AnnotationPermission.CanDeleteHighlight,
                            //CanDeleteLine = false,
                            CanDeleteRedaction = batch.Permission.CanAnnotate & document.AnnotationPermission.CanDeleteRedaction,
                            CanDeleteText = batch.Permission.CanAnnotate & document.AnnotationPermission.CanDeleteText,
                            CanEmail = batch.Permission.CanEmail,
                            CanHideAnnotation = batch.Permission.CanAnnotate & document.AnnotationPermission.CanHideRedaction,
                            CanPrint = batch.Permission.CanPrint,
                            CanSeeHighlight = batch.Permission.CanAnnotate & document.AnnotationPermission.CanSeeHighlight,
                            //CanSeeLine = false,
                            CanSeeText = batch.Permission.CanAnnotate & document.AnnotationPermission.CanSeeText
                        };
                    }
                }
                else if (ViewerContainer.DocViewerMode == DocViewerMode.OCRTemplate)
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

                // TODO: Provide permission for Capture mode
            }
            catch (Exception ex)
            {
                ViewerContainer.HandleException(ex);
            }
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
                case DocViewerMode.Capture:
                    if (ViewerContainer.ThumbnailSelector.SelectedItems == null || ViewerContainer.ThumbnailSelector.SelectedItems.Count == 0)
                    {
                        return false;
                    }
                    return ViewerContainer.ThumbnailSelector.SelectedItems[0].BatchItem.BatchData.BatchType.BatchTypePermission.CanCapture;

            }

            // TODO: Provide permission for Capture mode
            return true;
        }

        public bool CanIndex()
        {
            switch (ViewerContainer.DocViewerMode)
            {
                case DocViewerMode.OCRTemplate:
                    return false;
                case DocViewerMode.WorkItem:
                    if (ViewerContainer.ThumbnailSelector.SelectedItems == null || ViewerContainer.ThumbnailSelector.SelectedItems.Count == 0)
                    {
                        return false;
                    }
                    return ViewerContainer.ThumbnailSelector.SelectedItems[0].BatchItem.BatchData.Permission.CanModifyIndexes;
                default:
                    if (ViewerContainer.ThumbnailSelector.SelectedItems == null || ViewerContainer.ThumbnailSelector.SelectedItems.Count == 0)
                    {
                        return false;
                    }
                    return ViewerContainer.ThumbnailSelector.SelectedItems[0].BatchItem.BatchData.BatchType.BatchTypePermission.CanIndex;
            }
        }

        public bool CanClassify()
        {
            switch (ViewerContainer.DocViewerMode)
            {
                case DocViewerMode.OCRTemplate:
                    return false;
                default:
                    if (ViewerContainer.ThumbnailSelector.SelectedItems == null || ViewerContainer.ThumbnailSelector.SelectedItems.Count == 0)
                    {
                        return false;
                    }
                    return ViewerContainer.ThumbnailSelector.SelectedItems[0].BatchItem.BatchData.BatchType.BatchTypePermission.CanClassify;
            }
        }

        public bool CanChangeDocumentType()
        {
            switch (ViewerContainer.DocViewerMode)
            {
                case DocViewerMode.LightCapture:
                    return true;
                case DocViewerMode.Document:
                    return false;
                case DocViewerMode.WorkItem:
                    return false;
                case DocViewerMode.Capture:
                    return true;
            }

            // TODO: Provide permission for Capture mode
            return true;
        }

        public bool CanChangeBatchType()
        {
            switch (ViewerContainer.DocViewerMode)
            {
                case DocViewerMode.LightCapture:
                    return true;
                case DocViewerMode.Document:
                    return false;
                case DocViewerMode.WorkItem:
                    return false;
                case DocViewerMode.Capture:
                    return (ViewerContainer.BatchTypes != null && ViewerContainer.BatchTypes.Count > 1);
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
                case DocViewerMode.Capture:
                    if (ViewerContainer.ThumbnailSelector.SelectedItems == null || ViewerContainer.ThumbnailSelector.SelectedItems.Count == 0)
                    {
                        return false;
                    }
                    return ViewerContainer.ThumbnailSelector.SelectedItems[0].BatchItem.BatchData.BatchType.BatchTypePermission.CanClassify;
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
            if (ViewerContainer.ThumbnailSelector.SelectedItems == null || ViewerContainer.ThumbnailSelector.SelectedItems.Count == 0)
            {
                return false;
            }

            ContentItem selectedItem = ViewerContainer.ThumbnailSelector.SelectedItems[0];
            
            switch (ViewerContainer.DocViewerMode)
            {
                case DocViewerMode.LightCapture:
                    return true;
                case DocViewerMode.Document:
                case DocViewerMode.WorkItem:
                    return selectedItem.ItemType == ContentItemType.Batch ? selectedItem.BatchItem.BatchData.Permission.CanDelete : selectedItem.BatchItem.BatchData.Permission.CanModifyDocument;
                case DocViewerMode.Capture:
                    return selectedItem.ItemType == ContentItemType.Batch ? true : selectedItem.BatchItem.BatchData.BatchType.BatchTypePermission.CanClassify;
            }

            return true;
        }

        public bool CanInsert()
        {
            if (ViewerContainer.ThumbnailSelector.SelectedItems == null || ViewerContainer.ThumbnailSelector.SelectedItems.Count == 0)
            {
                return false;
            }

            ContentItem selectedItem = ViewerContainer.ThumbnailSelector.SelectedItems[0];
            
            switch (ViewerContainer.DocViewerMode)
            {
                case DocViewerMode.LightCapture:
                    return true;
                case DocViewerMode.Document:
                case DocViewerMode.WorkItem:
                    return ViewerContainer.ThumbnailSelector.SelectedItems[0].BatchItem.BatchData.Permission.CanModifyDocument;
                case DocViewerMode.Capture:
                    return ViewerContainer.ThumbnailSelector.SelectedItems[0].BatchItem.BatchData.BatchType.BatchTypePermission.CanClassify;
            }

            // TODO: Provide permission for Capture mode
            return true;
        }

        public bool CanModifiedDocument()
        {
            switch (ViewerContainer.DocViewerMode)
            {
                case DocViewerMode.LightCapture:
                    return true;
                case DocViewerMode.Document:
                case DocViewerMode.WorkItem:
                    if (ViewerContainer.ThumbnailSelector.SelectedItems == null || ViewerContainer.ThumbnailSelector.SelectedItems.Count == 0)
                    {
                        return false;
                    }
                    return ViewerContainer.ThumbnailSelector.SelectedItems[0].BatchItem.BatchData.Permission.CanModifyDocument;
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
                    if (ViewerContainer.ThumbnailSelector.SelectedItems == null || ViewerContainer.ThumbnailSelector.SelectedItems.Count == 0)
                    {
                        return false;
                    }
                    return ViewerContainer.ThumbnailSelector.SelectedItems[0].BatchItem.BatchData.Permission.CanModifyIndexes;
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
                case DocViewerMode.WorkItem:
                    if (ViewerContainer.ThumbnailSelector.SelectedItems == null || ViewerContainer.ThumbnailSelector.SelectedItems.Count == 0)
                    {
                        return false;
                    }

                    return ViewerContainer.ThumbnailSelector.SelectedItems[0].BatchItem.BatchData.Permission.CanReject;
                case DocViewerMode.Capture:
                    return false;

            }
            // TODO: Provide permission for Capture mode
            return true;
        }

        public bool CanReleaseWithLoosePage()
        {
            if (ViewerContainer.ThumbnailSelector.SelectedItems == null || ViewerContainer.ThumbnailSelector.SelectedItems.Count == 0)
            {
                return false;
            }
            switch (ViewerContainer.DocViewerMode)
            {
                case DocViewerMode.LightCapture:
                case DocViewerMode.Document:
                case DocViewerMode.WorkItem:
                case DocViewerMode.Capture:
                    return ViewerContainer.ThumbnailSelector.SelectedItems[0].BatchItem.BatchData.Permission.CanReleaseLoosePage;
            }
            // TODO: Provide permission for Capture mode
            return true;
        }

        public bool CanReOrderPage()
        {
            if (ViewerContainer.ThumbnailSelector.SelectedItems == null || ViewerContainer.ThumbnailSelector.SelectedItems.Count == 0)
            {
                return false;
            }

            ContentItem selectedItem = ViewerContainer.ThumbnailSelector.SelectedItems[0];
            switch (ViewerContainer.DocViewerMode)
            {
                case DocViewerMode.WorkItem:
                    return selectedItem.BatchItem.BatchData.Permission.CanModifyDocument;
                case DocViewerMode.Capture:
                    return selectedItem.BatchItem.BatchData.BatchType.BatchTypePermission.CanClassify;
            }

            // TODO: Provide permission for Capture mode
            return true;
        }

        public bool CanReplace()
        {
            if (ViewerContainer.ThumbnailSelector.SelectedItems == null || ViewerContainer.ThumbnailSelector.SelectedItems.Count == 0)
            {
                return false;
            }
            ContentItem selectedItem = ViewerContainer.ThumbnailSelector.SelectedItems[0];
            switch (ViewerContainer.DocViewerMode)
            {
                case DocViewerMode.LightCapture:
                    return true;
                case DocViewerMode.Document:
                case DocViewerMode.WorkItem:
                    return selectedItem.BatchItem.BatchData.Permission.CanModifyDocument;
                case DocViewerMode.Capture:
                    return selectedItem.BatchItem.BatchData.BatchType.BatchTypePermission.CanClassify;
            }

            // TODO: Provide permission for Capture mode
            return true;
        }

        public bool CanRotate()
        {
            if (ViewerContainer.ThumbnailSelector.SelectedItems == null || ViewerContainer.ThumbnailSelector.SelectedItems.Count == 0)
            {
                return false;
            }
            ContentItem selectedItem = ViewerContainer.ThumbnailSelector.SelectedItems[0];

            switch (ViewerContainer.DocViewerMode)
            {
                case DocViewerMode.LightCapture:
                    return true;
                case DocViewerMode.Document:
                case DocViewerMode.WorkItem:
                    return selectedItem.BatchItem.BatchData.Permission.CanModifyDocument;
                case DocViewerMode.OCRTemplate:
                    return true;
                case DocViewerMode.Capture:
                    return selectedItem.BatchItem.BatchData.BatchType.BatchTypePermission.CanClassify;
            }

            // TODO: Provide permission for Capture mode
            return true;
        }

        public bool CanSendLink()
        {
            if (ViewerContainer.ThumbnailSelector.SelectedItems == null || ViewerContainer.ThumbnailSelector.SelectedItems.Count == 0)
            {
                return false;
            }

            switch (ViewerContainer.DocViewerMode)
            {
                case DocViewerMode.LightCapture:
                case DocViewerMode.Document:
                    return false;
                case DocViewerMode.WorkItem:
                    return ViewerContainer.ThumbnailSelector.SelectedItems[0].BatchItem.BatchData.Permission.CanSendLink;
                case DocViewerMode.Capture:
                    return false;
            }
            // TODO: Provide permission for Capture mode
            return true;
        }

        public bool CanSplitDocument()
        {
            if (ViewerContainer.ThumbnailSelector.SelectedItems == null || ViewerContainer.ThumbnailSelector.SelectedItems.Count == 0)
            {
                return false;
            }

            switch (ViewerContainer.DocViewerMode)
            {
                case DocViewerMode.LightCapture:
                    return true;
                case DocViewerMode.Document:
                    return false;
                case DocViewerMode.WorkItem:
                    return ViewerContainer.ThumbnailSelector.SelectedItems[0].BatchItem.BatchData.Permission.CanModifyDocument;
                case DocViewerMode.Capture:
                    return ViewerContainer.ThumbnailSelector.SelectedItems[0].BatchItem.BatchData.BatchType.BatchTypePermission.CanClassify;
                case DocViewerMode.OCRTemplate:
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

        public bool CanDelegateItems()
        {
            switch (ViewerContainer.DocViewerMode)
            {
                case DocViewerMode.Capture:
                case DocViewerMode.Document:
                case DocViewerMode.LightCapture:
                case DocViewerMode.OCRTemplate:
                    return false;
                case DocViewerMode.WorkItem:
                    return false;
            }

            return false;
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

        private BatchModel GetBatch(ContentItem item)
        {
            var tempItem = item ?? ViewerContainer.ThumbnailSelector.Cursor;

            if (tempItem != null)
            {
                if (tempItem.ItemType == ContentItemType.Batch)
                {
                    return tempItem.BatchData;
                }
                else if (tempItem.ItemType == ContentItemType.Document)
                {
                    return tempItem.Parent.BatchData;
                }
                else
                {
                    if (tempItem.Parent.Parent != null)
                    {
                        return tempItem.Parent.Parent.BatchData;
                    }
                    else
                    {
                        //loose page
                        return tempItem.Parent.BatchData;
                    }
                }
            }


            return null;
        }
    }
}