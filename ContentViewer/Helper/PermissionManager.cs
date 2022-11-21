using System.Linq;
using Ecm.ContentViewer.Model;
using Ecm.ContentViewer.Model;
using Ecm.ContentViewer.ViewModel;

namespace Ecm.ContentViewer.Helper
{
    public class PermissionManager
    {
        public PermissionManager(MainViewerViewModel mainViewModel)
        {
            MainViewModel = mainViewModel;
        }

        public MainViewerViewModel MainViewModel { get; private set; }

        public ContentViewerPermission GetContentViewerPermission(ContentItem item)
        {
            if (MainViewModel.ViewerMode == ViewerMode.OCRTemplate)
            {
                return new ContentViewerPermission {CanApplyOCRTemplate = true};
            }

            // TODO: Provide permission for WorkItem (workflow) mode
            if (MainViewModel.ViewerMode == ViewerMode.WorkItem || MainViewModel.ViewerMode == ViewerMode.Capture)
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

            // TODO: Provide permission for Capture mode
            return new ContentViewerPermission();
        }

        public bool CanCapture()
        {
            switch (MainViewModel.ViewerMode)
            {
                case ViewerMode.LightCapture:
                    return true;
                case ViewerMode.ContentModel:
                    return false;
                case ViewerMode.WorkItem:
                    return false;
                case ViewerMode.Capture:
                    if (MainViewModel.ThumbnailSelector.SelectedItems == null || MainViewModel.ThumbnailSelector.SelectedItems.Count == 0)
                    {
                        return false;
                    }
                    return MainViewModel.ThumbnailSelector.SelectedItems[0].BatchItem.BatchData.BatchType.BatchTypePermission.CanCapture;

            }

            // TODO: Provide permission for Capture mode
            return true;
        }

        public bool CanChangeDocumentType()
        {
            switch (MainViewModel.ViewerMode)
            {
                case DocViewerMode.LightCapture:
                    return true;
                case DocViewerMode.ContentModel:
                    return false;
                case DocViewerMode.WorkItem:
                    return false;
                case ViewerMode.Capture:
                    return true;
            }

            // TODO: Provide permission for Capture mode
            return true;
        }

        public bool CanChangeBatchType()
        {
            switch (MainViewModel.ViewerMode)
            {
                case ViewerMode.LightCapture:
                    return true;
                case ViewerMode.ContentModel:
                    return false;
                case ViewerMode.WorkItem:
                    return false;
                case ViewerMode.Capture:
                    return (MainViewModel.BatchTypes != null && MainViewModel.BatchTypes.Count > 1);
            }

            // TODO: Provide permission for Capture mode
            return true;
        }

        public bool CanCombineDocument()
        {
            switch (MainViewModel.ViewerMode)
            {
                case ViewerMode.LightCapture:
                case ViewerMode.ContentModel:
                    return false;
                case ViewerMode.WorkItem:
                    return false;
            }

            // TODO: Provide permission for Capture mode
            return true;
        }

        public bool CanCreateDocument()
        {
            switch (MainViewModel.ViewerMode)
            {
                case ViewerMode.LightCapture:
                    return true;
                case ViewerMode.ContentModel:
                    return false;
                case ViewerMode.WorkItem:
                    return false;
            }

            // TODO: Provide permission for Capture mode
            return true;
        }

        public bool CanDelete()
        {
            switch (MainViewModel.ViewerMode)
            {
                case ViewerMode.LightCapture:
                    return true;
                case ViewerMode.ContentModel:
                case ViewerMode.WorkItem:
                    if (MainViewModel.ThumbnailSelector.SelectedItems == null || MainViewModel.ThumbnailSelector.SelectedItems.Count == 0)
                    {
                        return false;
                    }
                    return MainViewModel.ThumbnailSelector.SelectedItems[0].BatchItem.BatchData.Permission.CanDelete;
            }

            // TODO: Provide permission for Capture mode
            return true;
        }

        public bool CanInsert()
        {
            switch (MainViewModel.ViewerMode)
            {
                case ViewerMode.LightCapture:
                    return true;
                case ViewerMode.ContentModel:
                case ViewerMode.WorkItem:
                    if (MainViewModel.ThumbnailSelector.SelectedItems == null || MainViewModel.ThumbnailSelector.SelectedItems.Count == 0)
                    {
                        return false;
                    }
                    return MainViewModel.ThumbnailSelector.SelectedItems[0].BatchItem.BatchData.Permission.CanInsertDocument;
            }

            // TODO: Provide permission for Capture mode
            return true;
        }

        public bool CanModifyIndex()
        {
            switch (MainViewModel.ViewerMode)
            {
                case ViewerMode.LightCapture:
                    return true;
                case ViewerMode.ContentModel:
                case ViewerMode.WorkItem:
                    if (MainViewModel.ThumbnailSelector.SelectedItems == null || MainViewModel.ThumbnailSelector.SelectedItems.Count == 0)
                    {
                        return false;
                    }
                    return MainViewModel.ThumbnailSelector.SelectedItems[0].BatchItem.BatchData.Permission.CanModifyIndexes;
            }

            // TODO: Provide permission for Capture mode
            return true;
        }

        public bool CanReject()
        {
            switch (MainViewModel.ViewerMode)
            {
                case ViewerMode.LightCapture:
                case ViewerMode.ContentModel:
                    return false;
                case ViewerMode.WorkItem:
                    if (MainViewModel.ThumbnailSelector.SelectedItems == null || MainViewModel.ThumbnailSelector.SelectedItems.Count == 0)
                    {
                        return false;
                    }

                    return MainViewModel.ThumbnailSelector.SelectedItems[0].BatchItem.BatchData.Permission.CanReject;
                case ViewerMode.Capture:
                    return false;

            }
            // TODO: Provide permission for Capture mode
            return true;
        }

        public bool CanReleaseWithLoosePage()
        {
            switch (MainViewModel.ViewerMode)
            {
                case ViewerMode.LightCapture:
                case ViewerMode.ContentModel:
                case ViewerMode.WorkItem:
                case ViewerMode.Capture:
                    if (MainViewModel.ThumbnailSelector.SelectedItems == null || MainViewModel.ThumbnailSelector.SelectedItems.Count == 0)
                    {
                        return false;
                    }
                    return MainViewModel.ThumbnailSelector.SelectedItems[0].BatchItem.BatchData.Permission.CanReleaseLoosePage;
            }
            // TODO: Provide permission for Capture mode
            return true;
        }

        public bool CanReOrderPage()
        {
            switch (MainViewModel.ViewerMode)
            {
                case ViewerMode.LightCapture:
                    return true;
                case ViewerMode.ContentModel:
                case ViewerMode.WorkItem:
                    if (MainViewModel.ThumbnailSelector.SelectedItems == null || MainViewModel.ThumbnailSelector.SelectedItems.Count == 0)
                    {
                        return false;
                    }

                    return MainViewModel.ThumbnailSelector.SelectedItems[0].BatchItem.BatchData.Permission.CanModifyDocument;
            }

            // TODO: Provide permission for Capture mode
            return true;
        }

        public bool CanReplace()
        {
            switch (MainViewModel.ViewerMode)
            {
                case ViewerMode.LightCapture:
                    return true;
                case ViewerMode.ContentModel:
                case ViewerMode.WorkItem:
                    if (MainViewModel.ThumbnailSelector.SelectedItems == null || MainViewModel.ThumbnailSelector.SelectedItems.Count == 0)
                    {
                        return false;
                    }

                    return MainViewModel.ThumbnailSelector.SelectedItems[0].BatchItem.BatchData.Permission.CanModifyDocument;
            }

            // TODO: Provide permission for Capture mode
            return true;
        }

        public bool CanRotate()
        {
            switch (MainViewModel.ViewerMode)
            {
                case ViewerMode.LightCapture:
                    return true;
                case ViewerMode.ContentModel:
                case ViewerMode.WorkItem:
                    if (MainViewModel.ThumbnailSelector.SelectedItems == null || ViewerContainer.ThumbnailSelector.SelectedItems.Count == 0)
                    {
                        return false;
                    }

                    return MainViewModel.ThumbnailSelector.SelectedItems[0].BatchItem.BatchData.Permission.CanModifyDocument;
                case ViewerMode.OCRTemplate:
                    return false;
            }

            // TODO: Provide permission for Capture mode
            return true;
        }

        public bool CanSendLink()
        {
            switch (MainViewModel.ViewerMode)
            {
                case ViewerMode.LightCapture:
                case ViewerMode.ContentModel:
                    return false;
                case ViewerMode.WorkItem:
                    if (MainViewModel.ThumbnailSelector.SelectedItems == null || MainViewModel.ThumbnailSelector.SelectedItems.Count == 0)
                    {
                        return false;
                    }

                    return MainViewModel.ThumbnailSelector.SelectedItems[0].BatchItem.BatchData.Permission.CanSendLink;
                case ViewerMode.Capture:
                    return false;
            }
            // TODO: Provide permission for Capture mode
            return true;
        }

        public bool CanSplitDocument()
        {
            switch (MainViewModel.ViewerMode)
            {
                case ViewerMode.LightCapture:
                    return true;
                case ViewerMode.ContentModel:
                    return false;
                case ViewerMode.WorkItem:
                    if (MainViewModel.ThumbnailSelector.SelectedItems == null || MainViewModel.ThumbnailSelector.SelectedItems.Count == 0)
                    {
                        return false;
                    }

                    return MainViewModel.ThumbnailSelector.SelectedItems[0].BatchItem.BatchData.Permission.CanModifyDocument;
                case ViewerMode.Capture:
                    return true;
                case ViewerMode.OCRTemplate:
                    return false;
            }

            // TODO: Provide permission for Capture mode
            return true;
        }

        public bool CanUnReject()
        {
            switch (MainViewModel.ViewerMode)
            {
                case ViewerMode.LightCapture:
                case ViewerMode.ContentModel:
                    return false;
                case ViewerMode.WorkItem:
                    return false;
            }
            // TODO: Provide permission for Capture mode
            return true;
        }

        public bool CanDelegateItems()
        {
            switch (MainViewModel.ViewerMode)
            {
                case ViewerMode.Capture:
                case ViewerMode.ContentModel:
                case ViewerMode.LightCapture:
                case ViewerMode.OCRTemplate:
                    return false;
                case ViewerMode.WorkItem:
                    return false;
            }

            return false;
        }

        private ContentModel GetDocument(ContentItem item)
        {
            var tempItem = item ?? MainViewModel.ThumbnailSelector.Cursor;

            if (tempItem != null)
            {
                if (tempItem.ItemType == ContentItemType.Page &&
                    tempItem.Parent.ItemType == ContentItemType.ContentModel)
                {
                    return tempItem.Parent.DocumentData;
                }

                if (tempItem.ItemType == ContentItemType.ContentModel)
                {
                    return tempItem.DocumentData;
                }

            }

            return null;
        }

        private BatchModel GetBatch(ContentItem item)
        {
            var tempItem = item ?? MainViewModel.ThumbnailSelector.Cursor;

            if (tempItem != null)
            {
                if (tempItem.ItemType == ContentItemType.Batch)
                {
                    return tempItem.BatchData;
                }
                else if (tempItem.ItemType == ContentItemType.ContentModel)
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